namespace Reports.Services.Models
{
    public class DailyAuditData
    {
        public int EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public int Module { get; set; }
        public int Duty { get; set; }
        public string DutyDate { get; set; }
        public string DutySignOn { get; set; }
        public string DutySignOff { get; set; }
        public string BusNumber { get; set; }
        public string EquipmentNumber { get; set; }
        public string FirstRoute { get; set; }
        public string FirstJourney { get; set; }
        public string Revenue { get; set; }
        public string NonRevenue { get; set; }
        public string Tickets { get; set; }
        public string Passes { get; set; }
        public string Transfers { get; set; }
        public string modulesignoff { get; set; }
        public string modulesignon { get; set; }
        public int TotalPs { get; set; }

        public string CashierName { get; set; }
        public string CashierNum { get; set; }
        public string str4_LocationCode { get; set; }
        public string LocationID { get; set; }
        public string Terminal { get; set; }
        public string DateRangeFilter { get; set; }

        public string Cashsignon { get; set; }
        public string Cashsignoff { get; set; }
        public string TransactionDatetime { get; set; }
        public int Class { get; set; }
        public string MJNonRevenue { get; set; }
        public string MJPasses { get; set; }
        public string SVNonRevenue { get; set; }
        public string SVPasses { get; set; }
    }
}
