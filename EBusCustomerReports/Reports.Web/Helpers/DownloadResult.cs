using System;
using System.Web;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;

namespace Helpers
{
    public class DownloadPdfResult : ActionResult
    {
        public DownloadPdfResult(ReportClass rptH,string fileName)
        {
            this.FileName = fileName;
            this.RptH = rptH;
        }

        public string FileName { get; set; }
        public ReportClass RptH { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            //we have to send file as attachment to download. use httpResponse in mvc
            HttpContext.Current.Response.Clear();
            try
            {
                RptH.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat,
                                          HttpContext.Current.Response, true, FileName);

            }
            catch (Exception)
            {
                
            }
            finally
            {
                HttpContext.Current.Response.End();
            }
        }
    }
}