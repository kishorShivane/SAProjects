using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models
{
   public  class ClassSummaryData
    {
        public string ClassTypeName { get; set; }
        public string Class { get; set; }
        public string ClassGroup { get; set; }

        public double Revenue { get; set; }
        public double NonRevenue { get; set; }

        public Int32 TicketCount { get; set; }
        public Int32 TripCount { get; set; }

        public string DateRange { get; set; }
        public string ClassFilter { get; set; }
        public string ClassGroupFilter { get; set; }
        public string RouteFilter { get; set; }
        public string CompanyName { get; set; }

        public string MonthName { get; set; }
        public int AverageTicketValue { get; set; }
    }
}
