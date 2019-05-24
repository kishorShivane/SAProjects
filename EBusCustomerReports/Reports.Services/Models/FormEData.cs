using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models
{
    public class FormEData
    {
        public string Contract { get; set; }
        public string DOTRoute { get; set; }
        public string WayfarerRoute { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string ScheduledTrips { get; set; }
        public string OperatedTrips { get; set; }
        public string NotOperatedTrips { get; set; }
        public string Schedulekilometres { get; set; }
        public string OperatedKilometres { get; set; }
        public string Tickets { get; set; }
        public string Passes { get; set; }
        public string Transfers { get; set; }
        public string TotalPassengers { get; set; }
        public string Revenue { get; set; }
        public string NonRevenue { get; set; }
        public string TotalRevenue { get; set; }
        public string AvgPassengerPerTrip { get; set; }
        public string AvgRevenuePerTrip { get; set; }

        public string DateRangeFilter { get; set; }
        public string ContractsFilter { get; set; }
        public string companyName { get; set; }

        public string DutyId { get; set; }
        public int IntDutyId { get; set; }
        public string JourneyId { get; set; }
        public string DutiesFilter { get; set; }

        public string DateSelected { get; set; }
    }
}