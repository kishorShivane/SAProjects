using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class DutySheetsViewModel
    {
        public DutySheetsViewModel()
        {
            Duties = new List<SelectListItem>();
            Contracts = new List<SelectListItem>();
            ExcelOrPDF = true;
            ShowAllOperatingDays = false;
        }

        public bool ShowAllOperatingDays { get; set; }

        public string[] DutiesSelected { get; set; }
        public List<SelectListItem> Duties { get; set; }

        public string[] ContractsSelected { get; set; }
        public List<SelectListItem> Contracts { get; set; }

        [Required]
        public string DutyDate { get; set; }
              
        public bool ExcelOrPDF { get; set; } 
    }
}
