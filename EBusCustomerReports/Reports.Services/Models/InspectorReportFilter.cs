using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class InspectorReportFilter
    {
        public InspectorReportFilter()
        {
            Drivers = new List<SelectListItem>();
            Inspectors = new List<SelectListItem>();
            ExcelOrPDF = true; //PDF=true, EXCEL=false
        }

        public string[] DriversSelected { get; set; }
        public List<SelectListItem> Drivers { get; set; }

        public string[] InspectorsSelected { get; set; }
        public List<SelectListItem> Inspectors { get; set; }

        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        public bool ExcelOrPDF { get; set; }
    }

    public class CashierReportSummaryFilter
    {
        public CashierReportSummaryFilter()
        {
            Cashiers = new List<SelectListItem>();
            StaffList = new List<SelectListItem>();
            Terminals = new List<SelectListItem>();
            Locations = new List<SelectListItem>();
            ExcelOrPDF = true; //PDF=true, EXCEL=false
        }

        public string[] CashiersSelected { get; set; }
        public List<SelectListItem> Cashiers { get; set; }

        public string[] StaffSelected { get; set; }
        public List<SelectListItem> StaffList { get; set; }

        public string[] LocationsSelected { get; set; }
        public List<SelectListItem> Locations { get; set; }

        public string[] TerminalsSelected { get; set; }
        public List<SelectListItem> Terminals { get; set; }

        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        public bool ExcelOrPDF { get; set; }
    }
}
