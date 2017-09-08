using System;


namespace Reports.Services.Models
{
    class DayRevenue
    {
        public string dateSelected { get; set; }
        public string dayOfWeek { get; set; }
        public decimal revenueFromdrivers { get; set; }
        public decimal revenueFromSellers { get; set; }
    }
}
