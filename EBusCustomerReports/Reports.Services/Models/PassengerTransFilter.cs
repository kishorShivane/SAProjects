using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models
{
    public class PassengerTransFilter
    {
        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessage = "SmartCardNumber must be 10 digit numeric")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "SmartCardNumber must be numeric")]
        public string SmartCardNumber { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string SurName { get; set; } = "";
        public string IDNumber { get; set; } = "";
        [StringLength(10, MinimumLength = 10, ErrorMessage = "CellPhoneNumber must be 10 digit numeric")]
        public string CellPhoneNumber { get; set; }
        public string DutyNumber { get; set; } = "";
        public string BusNumber { get; set; } = "";


        public bool ExcelOrPDF { get; set; }
    }
}
