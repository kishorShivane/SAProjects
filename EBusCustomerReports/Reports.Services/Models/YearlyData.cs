using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models
{
    public class YearlyData
    {
        public string MonthsSelected { get; set; }
        public string CompanySelected { get; set; }
        public string ClassFilterSelected { get; set; }
        public Int32 Year2Selected { get; set; }
        public Int32 Year1Selected { get; set; }

        public string Class { get; set; }
        public double Year2Revenue { get; set; }
        public double Year1Revenue { get; set; }
        public double RevenueDiff { get; set; }
        public double RevenueDiffPer { get; set; }

        public double Year2NonRevenue { get; set; }
        public double Year1NonRevenue { get; set; }
        public double NonRevenueDiff { get; set; }
        public double NonRevenueDiffPer { get; set; }

        public Int32 Year2Passenger { get; set; }
        public Int32 Year1Passenger { get; set; }
        public double PassengerDiff { get; set; }
        public double PassengerDiffPer { get; set; }

        public string RouteFilter { get; set; }
        public string RouteId { get; set; }
        public string RouteName { get; set; }
    }
}