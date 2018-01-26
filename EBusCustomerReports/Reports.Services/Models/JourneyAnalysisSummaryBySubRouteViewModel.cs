using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reports.Services.Models
{
    public class JourneyAnalysisSummaryBySubRouteViewModel
    {
        public JourneyAnalysisSummaryBySubRouteViewModel()
        {
            Classes = new List<SelectListItem>();
            Contracts = new List<SelectListItem>();
            ClassesTypes = new List<SelectListItem>();
            Routes = new List<SelectListItem>();
            ExcelOrPDF = true; //PDF=true, EXCEL=false
        }

        public string[] ClassesSelected { get; set; }
        public List<SelectListItem> Classes { get; set; }

        public string[] ContractsSelected { get; set; }
        public List<SelectListItem> Contracts { get; set; }

        public string[] ClassesTypeSelected { get; set; }
        public List<SelectListItem> ClassesTypes { get; set; }

        public string[] RoutesSelected { get; set; }
        public List<SelectListItem> Routes { get; set; }

        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        public bool ExcelOrPDF { get; set; }
    }
}
