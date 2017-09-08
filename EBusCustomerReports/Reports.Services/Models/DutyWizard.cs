using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Reports.Services.Models
{
    public class DutyWizard
    {
        public DutyWizard()
        {
            Duty = new Duty();
            DutyBooking = new DutyBooking();
            DutyEvent = new DutyEvent();
            DutyOperatedDay = new DutyOperatedDay();
            DutyTrip = new DutyTrip();
        }
        public Duty Duty { get; set; }
        public DutyBooking DutyBooking { get; set; }
        public DutyEvent DutyEvent { get; set; }
        public DutyOperatedDay DutyOperatedDay { get; set; }
        public DutyTrip DutyTrip { get; set; }
        public List<SelectListItem> WorkSpaces { get; set; }
        public List<Duty> Duties { get; set; }
        public List<SelectListItem> Locations { get; set; } 
    }
}