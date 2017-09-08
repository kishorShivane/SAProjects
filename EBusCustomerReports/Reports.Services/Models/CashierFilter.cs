using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class CashierFilter
    {
        public CashierFilter()
        {
            Locations = new List<SelectListItem>();
        }

        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        public string[] LocationsSelected { get; set; }
        public List<SelectListItem> Locations { get; set; }

        public bool ExcelOrPDF { get; set; } 
    }
}
