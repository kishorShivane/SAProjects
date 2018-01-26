using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services
{
    public class JourneyAnalysisSummaryBySubRoute
    {
        public string DutyID { get; set; }
        public string str4_JourneyNo { get; set; }
        public string RouteNumber { get; set; }
        public string SubRouteNumber { get; set; }
        public string Contract { get; set; }
        public string int4_JourneyTickets { get; set; }
        public string int4_JourneyPasses { get; set; }
        public string TripStatus { get; set; }
        public Int64 Int_TotalPassengers { get; set; }
        public string routeName { get; set; }
        public string ScheduledTrips { get; set; }
        public string OperatedTrips { get; set; }
        public string NotOperatedTrips { get; set; }
        public string Tickets { get; set; }
        public string Passes { get; set; }
        public string TotalPassengers { get; set; }
        public string JourneyRevenue { get; set; }
        public string TotalRevenue { get; set; }

    }
}
