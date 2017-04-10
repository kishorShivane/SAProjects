using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbusFileImporter.DataProvider.Models
{
    public partial class Duty
    {
        public int id_Duty { get; set; }
        public int id_Module { get; set; }
        public int int4_DutyID { get; set; }
        public Nullable<int> int4_OperatorID { get; set; }
        public string str_ETMID { get; set; }
        public Nullable<int> int4_GTValue { get; set; }
        public Nullable<int> int4_NextTicketNumber { get; set; }
        public Nullable<int> int4_DutySeqNum { get; set; }
        public Nullable<System.DateTime> dat_DutyStartDate { get; set; }
        public Nullable<System.DateTime> dat_DutyStartTime { get; set; }
        public Nullable<System.DateTime> dat_DutyStopDate { get; set; }
        public Nullable<System.DateTime> dat_DutyStopTime { get; set; }
        public Nullable<System.DateTime> dat_TrafficDate { get; set; }
        public string str_BusID { get; set; }
        public Nullable<int> int4_DutyRevenue { get; set; }
        public Nullable<int> int4_DutyTickets { get; set; }
        public Nullable<int> int4_DutyPasses { get; set; }
        public Nullable<int> int4_DutyNonRevenue { get; set; }
        public Nullable<int> int4_DutyTransfer { get; set; }
        public string str_FirstRouteID { get; set; }
        public Nullable<short> int2_FirstJourneyID { get; set; }
        public Nullable<System.DateTime> dat_RecordMod { get; set; }
        public Nullable<int> id_BatchNo { get; set; }
        public Nullable<byte> byt_IeType { get; set; }
        public string str_EpromVersion { get; set; }
        public string str_OperatorVersion { get; set; }
        public string str_SpecialVersion { get; set; }
        public Nullable<int> int4_DutyAnnulCash { get; set; }
        public Nullable<int> int4_DutyAnnulCount { get; set; }
    }
}
