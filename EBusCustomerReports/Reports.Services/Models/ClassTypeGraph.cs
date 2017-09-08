using System;

namespace Reports.Services.Models
{
    public class ClassTypeGraph
    {
        public string MonthName { get; set; }
        public string DatteSel { get; set; }
        public string ClassTypeName { get; set; }
        public double Revenue { get; set; }
        public double NonRevenue { get; set; }

        public string Last2MonthName { get; set; }
        public Int32 Last2Revenue { get; set; }

        public string LastMonthName { get; set; }
        public Int32 LastRevenue { get; set; }

        public string CurrentMonthName { get; set; }
        public Int32 CurrentRevenue { get; set; }
    }
}
