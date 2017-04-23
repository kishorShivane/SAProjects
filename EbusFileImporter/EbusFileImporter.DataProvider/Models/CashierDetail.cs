﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbusFileImporter.DataProvider.Models
{
    public partial class CashierDetail
    {
        public int ID { get; set; }
        public string StaffNumber { get; set; }
        public Nullable<System.DateTime> SignOnDatTime { get; set; }
        public Nullable<System.DateTime> SignOffDateTime { get; set; }
        public string Activity { get; set; }
        public string CashInType { get; set; }
        public Nullable<System.DateTime> TransactionDateTime { get; set; }
        public string DriverTotal { get; set; }
        public string CashPaidIn { get; set; }
        public string Shorts { get; set; }
        public Nullable<int> NetTickets { get; set; }
        public Nullable<int> NetPasses { get; set; }
        public Nullable<int> CashInReceiptNo { get; set; }
        public string CashierID { get; set; }
        public Nullable<System.DateTime> ImportDateTime { get; set; }
        public string Reason { get; set; }
        public string Overs { get; set; }
        public string Terminal { get; set; }
        public string UID { get; set; }
        public string ESN { get; set; }
        public long PSN { get; set; }
    }
}
