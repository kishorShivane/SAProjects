using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class SmartCardTransFilter
    {
        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "SmartCardNumber must be 10 digit numeric")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "SmartCardNumber must be numeric")]
        public string SmartCardNumber { get; set; }

        public bool ExcelOrPDF { get; set; }
    }
}
