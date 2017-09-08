using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reports.Services.Models
{
    public class DutyBooking
    {
        public DutyBooking()
        {
            Stages = new List<SelectListItem>();
            Hours = new List<SelectListItem>();
            Minutes = new List<SelectListItem>();
        }
        public DateTime BookOnTime { get; set; }
        public DateTime BookOffTime { get; set; }
        public string BookOnTimeHour { get; set; }
        public string BookOnTimeMinute { get; set; }
        public string BookOffTimeHour { get; set; }
        public string BookOffTimeMinute { get; set; }
        public string PlaceOn { get; set; }
        public string PlaceOff { get; set; }
        public int PlaceOnID { get; set; }
        public int PlaceOffID { get; set; }
        public bool IsDeleted { get; set; }
        public int DutyOperatedDayID { get; set; }
        public int ID { get; set; }
        public int DutyID { get; set; }
        public int DutyBookingID { get; set; }
        public List<SelectListItem> Hours { get; set; }
        public List<SelectListItem> Minutes { get; set; }
        public List<SelectListItem> Stages { get; set; }
    }
}
