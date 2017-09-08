using Helpers.Security;
using Reports.Services;
using Reports.Services.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Reports.Web.Controllers
{
    public class AssetController : Controller
    {
        AssetMasterService MachineMasterService = new AssetMasterService();

        private UserSettings GetUserSettings()
        {
            var res = new UserSettings();
            res.ConnectionKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            res.CompanyName = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;
            res.Username = HttpContext.User.Identity.Name;
            return res;
        }

        //
        // GET: /Machine/

        public ActionResult Index()
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var model = MachineMasterService.GetAssetDetails(conKey, "", "");
            Session[conKey] = model;
            return View(model);
        }

        public ActionResult ExportAssetToExcel()
        {
            var assets = new List<Asset>();
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            if (Session[conKey] == null)
            {
                assets = MachineMasterService.GetAssetDetails(conKey, "", "");
            }
            else
            {
                assets = (List<Asset>)Session[conKey];
            }

            //Create a dummy GridView
            GridView GridView1 = new GridView();
            GridView1.AllowPaging = false;
            GridView1.DataSource = ToDataTable(assets);
            GridView1.DataBind();

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition",
             "attachment;filename=AssetInformation.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);

            for (int i = 0; i < GridView1.Rows.Count; i++)
            {
                //Apply text style to each Row
                GridView1.Rows[i].Attributes.Add("class", "textmode");
            }
            GridView1.RenderControl(hw);

            //style to format numbers to string
            string style = @"<style> .textmode { mso-number-format:\@; } </style>";
            Response.Write(style);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();

            return View(assets);
        }

        public DataTable ToDataTable(IList<Asset> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(Asset));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (Asset item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
}
