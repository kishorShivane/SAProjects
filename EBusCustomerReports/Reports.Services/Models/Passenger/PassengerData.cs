using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reports.Services.Models.Passenger
{
    public class PassengerData
    {
        public PassengerData()
        { SmartCardTypes = new List<SelectListItem>(); PassengerTypes = new List<SelectListItem>(); }

        public string ID { get; set; }
        public string PassengerType { get; set; }
        public string SmartCardNumber { get; set; }
        public int SmartCardTypeID { get; set; }
        public string SmartCardType { get; set; }
        public bool Status { get; set; }
        public string Title { get; set; }
        public string Initials { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string IDNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public string CellPhoneNumber { get; set; }
        public string AlternativePhoneNumber { get; set; }
        public string Address { get; set; }

        public List<SelectListItem> SmartCardTypes{ get; set; }
        public List<SelectListItem> PassengerTypes { get; set; }

    }
}
