using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class SchVsOprViewModel
    {
        public SchVsOprViewModel()
        {
            Duties = new List<SelectListItem>();
            Contracts = new List<SelectListItem>();
            ExcelOrPDF = true; //PDF=true, EXCEL=false
        }

        public string[] DutiesSelected { get; set; }
        public List<SelectListItem> Duties { get; set; }

        public string[] ContractsSelected { get; set; }
        public List<SelectListItem> Contracts { get; set; }

        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        public bool ExcelOrPDF { get; set; }
    }

    public class EarlyLateRunningModel : SchVsOprViewModel
    {
        public EarlyLateRunningModel()
        {
            TimeList = new List<SelectListItem>();
            TimeList.Add(new SelectListItem { Selected = true, Text = "Bit Early: +5 min", Value = "5" });
            TimeList.Add(new SelectListItem { Selected = false, Text = "Early: +10 min", Value = "10" });
            TimeList.Add(new SelectListItem { Selected = false, Text = "Very Early: +15 Min", Value = "15" });
            TimeList.Add(new SelectListItem { Selected = false, Text = "Very Late: -15 min; ", Value = "-15" });
            TimeList.Add(new SelectListItem { Selected = false, Text = "Late: -10 min", Value = "-10" });
            TimeList.Add(new SelectListItem { Selected = false, Text = "Bit Late - 5 min", Value = "-5" });
        }

        public string TimeSelected { get; set; }
        public List<SelectListItem> TimeList { get; set; }
    }
}