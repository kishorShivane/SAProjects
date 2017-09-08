using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class DriverDetailsFilter
    {
        public DriverDetailsFilter()
        {
            Drivers = new List<SelectListItem>();
            ExcelOrPDF = true; //PDF=true, EXCEL=false
        }

        public string DriversSelected { get; set; }
        public List<SelectListItem> Drivers { get; set; }

        [Required]
        public string StartDate { get; set; }

        public bool ExcelOrPDF { get; set; } 
    }
}
