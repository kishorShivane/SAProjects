using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models
{
    public class RevenueByDuty
    {
        public string DutyID { get; set; }
        public string JourneyID { get; set; }
        public int Class { get; set; }
        public string companyName { get; set; }
        public double Revenue { get; set; }
        public double NonRevenue { get; set; }

        public double AdultRevenue { get; set; }
        public double ChildRevenue { get; set; }
        public double AdultNonRevenue { get; set; }
        public double SchlorNonRevenue { get; set; }
        public double Cash { get; set; }
        public double Value { get; set; }
        public double Total { get; set; }
        public string DateRangeFilter { get; set; }
        public string DutyFilter { get; set; }
    }
}
