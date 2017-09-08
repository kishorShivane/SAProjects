using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models
{
    public class DutyOperatedDay
    {
        public int ID { get; set; }
        public int DutyID { get; set; }
        public int DutyOperatedDayID { get; set; }
        public int OperatedDayBitmask { get; set; }
        public string OperatedDayString { get; set; }
    }
}
