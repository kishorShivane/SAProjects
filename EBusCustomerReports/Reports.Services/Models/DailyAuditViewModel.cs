using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class DailyAuditViewModel
    {
        public DailyAuditViewModel()
        {
            AllStaffs = new List<SelectListItem>();
            StaffTypes = new List<SelectListItem>();
            Locations = new List<SelectListItem>();
            ExcelOrPDF = true; //PDF=true, EXCEL=false
        }

        public string[] StaffsSelected { get; set; }
        public List<SelectListItem> AllStaffs { get; set; }

        public string[] StaffTypesSelected { get; set; }
        public List<SelectListItem> StaffTypes { get; set; }

        public string[] LocationsSelected { get; set; }
        public List<SelectListItem> Locations { get; set; }

        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        public bool ExcelOrPDF { get; set; } 
    }
}
