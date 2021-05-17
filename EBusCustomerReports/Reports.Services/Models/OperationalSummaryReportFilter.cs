using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class OperationalSummaryReportFilter
    {
        public OperationalSummaryReportFilter()
        {
            Reports = new List<SelectListItem>();
            ExcelOrPDF = true; //PDF=true, EXCEL=false
        }     

        public string[] ReportSelected { get; set; }
        public List<SelectListItem> Reports { get; set; }

        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        public bool ExcelOrPDF { get; set; }
    }
}
