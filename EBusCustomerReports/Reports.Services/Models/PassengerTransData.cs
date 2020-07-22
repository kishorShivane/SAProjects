using System;
using System.Collections.Generic;

namespace Reports.Services.Models
{
    public class PassengerTransData
    {
        public string SerialNumber { get; set; }
        public string SerialNumberHex { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string IDNumber { get; set; }
        public string CellPhoneNumber { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public string Duty { get; set; }
        public string Route { get; set; }
        public string DriverID { get; set; }
        public string DriverName { get; set; }
        public string Bus { get; set; }
        public DateTime TransDate { get; set; }
        public string TDate { get; set; }
        public string TTime { get; set; }
        public string Revenue { get; set; }
        public string NonRevenue { get; set; }
        public string RevenueBalance { get; set; }
        public string TripBalance { get; set; }
        public string AmountRecharged { get; set; }
        public string TripsRecharged { get; set; }
        public string SmartCardExipry { get; set; }
        public string CardIdFilter { get; set; }
        public string DateRangeFilter { get; set; }
        public string FirstNameFilter { get; set; }
        public string SurnameFilter { get; set; }
        public string IDNumberFilter { get; set; }
        public string CellNumberFilter { get; set; }
        public string DutyFilter { get; set; }
        public string BusFilter { get; set; }


    }
}
