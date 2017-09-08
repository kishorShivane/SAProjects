using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reports.Services.Models
{
    public class DutyTrip
    {
        public DutyTrip()
        {
            Hours = new List<SelectListItem>();
            Minutes = new List<SelectListItem>();
            Routes = new List<SelectListItem>();
            Contract = new List<SelectListItem>();
        }
        public int ID { get; set; }
        public int DutyID { get; set; }
        public int DutyBookingID { get; set; }
        public string Description { get; set; }
        public string JourneyNo { get; set; }
        public string MainRouteNumber { get; set; }
        public string SubRouteNumber { get; set; }
        public double Distance { get; set; }
        public string ContractID { get; set; }
        public string ContractValue { get; set; }
        public string DefaultContract { get; set; }
        public bool Direction { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string StartTimeHour { get; set; }
        public string StartTimeMinute { get; set; }
        public string EndTimeHour { get; set; }
        public string EndTimeMinute { get; set; }
        public bool IsDeleted { get; set; }
        public int Cost { get; set; }
        public int Team { get; set; }
        public string DestinationBlind { get; set; }
        public bool Position { get; set; }
        public int DutyOperatedDayID { get; set; }
        public string RouteName { get; set; }
        public List<SelectListItem> Hours { get; set; }
        public List<SelectListItem> Minutes { get; set; }
        public List<SelectListItem> Routes { get; set; }
        public List<SelectListItem> Contract { get; set; }
    }
}
