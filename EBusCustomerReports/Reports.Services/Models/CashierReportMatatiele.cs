using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models
{
    public class CashierReportMatatiele
    {
        public string StaffNumber { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int DriverTotal { get; set; }
        public int CashPaidIn { get; set; }
        public string Shorts { get; set; }
        public int NetTickets { get; set; }
        public int NetPasses { get; set; }
        public int CashinReceiptNo { get; set; }
        public string CashierID { get; set; }
        public string ImportDateTime { get; set; }
        public string StaffName { get; set; }
        public string CashierName { get; set; }
        public string LocationDesc { get; set; }
        public string Reason { get; set; }
        public string Overs { get; set; }
        public string Terminal { get; set; }
        public string CashInType { get; set; }
        public string CompanyName { get; set; }
        public string DateSelected { get; set; }
        public string Cashiers { get; set; }
        public string Locations { get; set; }
        public string Terminals { get; set; }
        public int Difference { get; set; }
        
    }
}
