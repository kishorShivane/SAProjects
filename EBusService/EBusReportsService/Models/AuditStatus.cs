using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBusReportsService.Models
{
    public class AuditStatus
    {
        public string str_BusId { get; set; }
        public string LastestDate { get; set; }
        public string Str_ETMID { get; set; }
        public string ETMType { get; set; }
        public string int4_OperatorID { get; set; }
        public string Color { get; set; }
        public string ColorName { get; set; }

        public Int32 SortingStatus { get; set; }
    }
}
