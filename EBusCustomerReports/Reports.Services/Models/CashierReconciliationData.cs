using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models
{
    public class CashierReconciliationData
    {
        public string StaffNumber { get; set; }
        public string StaffName { get; set; }
        public string Cashier { get; set; }
        public string CashierReason { get; set; }
        public string DutyRevenue { get; set; }
        public string Difference { get; set; }
        public string TransactionDatetime { get; set; }
        public string Location { get; set; }
    }
}
