namespace Reports.Services.Models
{
    public class SellerSummaryData
    {
        public string TransDate { get; set; }
        public string StartTime { get; set; }
        public string StopTime { get; set; }
        public string EtmID { get; set; }
        public string Staff { get; set; }
        public string ClassTypeName { get; set; }
        public string Class { get; set; }
        public double Revenue { get; set; }
        public int TicketCount { get; set; }
        public int TripCount { get; set; }
        public string DateRange { get; set; }
        public string ClassFilter { get; set; }
        public string ClassTypeFilter { get; set; }
        public string StaffFilter { get; set; }
        public string CompanyName { get; set; }
        public string MonthName { get; set; }
        public int AverageTicketValue { get; set; }
    }
}
