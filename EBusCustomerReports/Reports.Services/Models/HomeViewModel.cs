using System;

namespace Reports.Services.Models
{
    public class HomeViewModel
    {
        public int DriversCount { get; set; }
        public string RevenueFromDrivers { get; set; }

        public decimal RevenueSum { get; set; }

        public int SellersCount { get; set; }
        public string RevenueFromSellers { get; set; }

        public int ScheduledTripsCount { get; set; }
        public int OperatedTripsCount { get; set; }

        public float ScheduledDistance { get; set; }
        public float OperatedDistance { get; set; }

        public int TotalPasses { get; set; }
        public int TotalCashPassengers { get; set; }
        public int TotalTransfers { get; set; }

        public string DaysString { get; set; }
        public string DaysRevenueString { get; set; }
        public string DaysSellersRevenueString { get; set; }
        public string DaysPassengersCountString { get; set; }
    }
}