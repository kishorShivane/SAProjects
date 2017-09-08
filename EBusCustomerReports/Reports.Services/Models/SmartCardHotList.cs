using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class SmartCardHotList
    {
        public SmartCardHotList()
        {
            Reasons = new List<SelectListItem>();           
        }

        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "The field SmartCardNubmer must be a string with a length of 10 characters")]
        public string SmartCardNubmer { get; set; }
        public string Comments { get; set; }

        [Required]
        [Display(Name = "Reason")]
        public string ReasonSelected { get; set; }
        public List<SelectListItem> Reasons { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public bool IsDuplicate { get; set; }
    }
}
