using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class SalesAnalysisFilter
    {
        public SalesAnalysisFilter()
        {
            Classes = new List<SelectListItem>();
            Routes = new List<SelectListItem>();
            ClassGroups = new List<SelectListItem>();
            ExcelOrPDF = true; //PDF=true, EXCEL=false
            RouteTypes = new List<SelectListItem>();
            ClassesTypes = new List<SelectListItem>();
            RouteTypes.Add(new SelectListItem { Text = "Driver", Selected = false, Value = "Driver" });
            RouteTypes.Add(new SelectListItem { Text = "Seller", Selected = false, Value = "Seller" });
        }


        public string[] RouteTypeSelected { get; set; }
        public List<SelectListItem> RouteTypes { get; set; }

        public string[] ClassesSelected { get; set; }
        public List<SelectListItem> Classes { get; set; }

        public string[] ClassesTypesSelected { get; set; }
        public List<SelectListItem> ClassesTypes { get; set; }

        public string[] RoutesSelected { get; set; }
        public List<SelectListItem> Routes { get; set; }

        public string[] ClassGroupsSelected { get; set; }
        public List<SelectListItem> ClassGroups { get; set; }

        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        public bool ExcelOrPDF { get; set; }
    }
}
