using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbusFileImporter.DataProvider.Models
{
    public partial class Module
    {
        public int id_Module { get; set; }
        public string str_LocationCode { get; set; }
        public Nullable<int> int4_ModuleID { get; set; }
        public Nullable<int> int4_SignOnID { get; set; }
        public Nullable<int> int4_OnReaderID { get; set; }
        public Nullable<System.DateTime> dat_SignOnDate { get; set; }
        public Nullable<System.DateTime> dat_SignOnTime { get; set; }
        public Nullable<int> int4_OffReaderID { get; set; }
        public Nullable<System.DateTime> dat_SignOffDate { get; set; }
        public Nullable<System.DateTime> dat_SignOffTime { get; set; }
        public Nullable<System.DateTime> dat_TrafficDate { get; set; }
        public Nullable<System.DateTime> dat_ModuleOutDate { get; set; }
        public Nullable<System.DateTime> dat_ModuleOutTime { get; set; }
        public Nullable<int> int4_HdrModuleRevenue { get; set; }
        public Nullable<int> int4_HdrModuleTickets { get; set; }
        public Nullable<int> int4_HdrModulePasses { get; set; }
        public Nullable<int> int4_ModuleRevenue { get; set; }
        public Nullable<int> int4_ModuleTickets { get; set; }
        public Nullable<int> int4_ModulePasses { get; set; }
        public Nullable<int> int4_ModuleNonRevenue { get; set; }
        public Nullable<int> int4_ModuleTransfer { get; set; }
        public Nullable<System.DateTime> dat_ImportStamp { get; set; }
        public Nullable<System.DateTime> dat_RecordMod { get; set; }
        public Nullable<int> int4_ImportModuleKey { get; set; }
        public Nullable<int> id_BatchNo { get; set; }
        public Nullable<byte> byt_IeType { get; set; }
        public Nullable<int> byt_ModuleType { get; set; }
    }
}
