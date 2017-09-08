using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class SmartCardUsageFilter
    {
        public SmartCardUsageFilter()
        {
            ExcelOrPDF = true; //PDF=true, EXCEL=false
        }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer Number")]
        [Display(Name = "Number Of TimesUsed")]
        public Int32? NumberOfTimesUsed{ get; set; }

        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        public bool ExcelOrPDF { get; set; } 
    }
}
