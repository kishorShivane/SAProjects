using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models
{
    public class SalesAnalysisByRoute
    {
        public string RouteID { get; set; }
        public string RouteName { get; set; }

        public Int32 ClassID { get; set; }

        public string ClassName { get; set; }
        public string ClassType { get; set; }

        public double Revenue { get; set; }
        public double NonRevenue { get; set; }
        public double AnnulCash { get; set; }
        public Int32 TxCount { get; set; }
        public Int32 TicketCount { get; set; }

        public string dateRange { get; set; }
        public string RoutesFilters { get; set; }
        public string ClassIdFilters { get; set; }
        public string CompanyName { get; set; }
        public string ClassGroupFilter { get; set; }
    }
}
