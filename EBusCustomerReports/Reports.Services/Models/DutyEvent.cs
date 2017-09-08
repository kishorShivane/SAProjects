using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reports.Services.Models
{
    public class DutyEvent
    {
        public DutyEvent()
        {
            Hours = new List<SelectListItem>();
            Minutes = new List<SelectListItem>();
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string StartTimeHour { get; set; }
        public string StartTimeMinute { get; set; }
        public string EndTimeHour { get; set; }
        public string EndTimeMinute { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int DutyBookingID { get; set; }
        public int DutyEventID { get; set; }
        public int DutyOperatedDayID { get; set; }
        public int DutyID { get; set; }
        public List<SelectListItem> Hours { get; set; }
        public List<SelectListItem> Minutes { get; set; }
        public List<SelectListItem> Events { get; set; }
    }
}
