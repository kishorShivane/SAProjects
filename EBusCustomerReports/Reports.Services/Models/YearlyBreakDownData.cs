using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models
{
    public class YearlyBreakDownData
    {
        //Year	Passengers	Revenue	NonRevenue			
        public Int32 Year { get; set; }
        public Int32 Passengers { get; set; }

        public double Revenue { get; set; }
        public double NonRevenue { get; set; }

        public Int32 ClassID { get; set; }
        public string ClassName { get; set; }
        public string ClassNameFull { get; set; }
        public string str_RouteID { get; set; }
        public string RouteName { get; set; }
    }
}