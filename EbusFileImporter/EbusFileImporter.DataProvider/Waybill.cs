//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EbusFileImporter.DataProvider
{
    using System;
    using System.Collections.Generic;
    
    public partial class Waybill
    {
        public int ModuleID { get; set; }
        public Nullable<System.DateTime> dat_Start { get; set; }
        public Nullable<System.DateTime> dat_End { get; set; }
        public Nullable<int> int4_Operator { get; set; }
        public string str8_BusID { get; set; }
        public string str6_EtmID { get; set; }
        public Nullable<int> int4_EtmGrandTotal { get; set; }
        public Nullable<int> int4_Revenue { get; set; }
        public Nullable<System.DateTime> dat_Match { get; set; }
        public Nullable<System.DateTime> dat_Actual { get; set; }
        public Nullable<int> Imported_Operator { get; set; }
        public string Imported_BusID { get; set; }
        public string Imported_ETMID { get; set; }
        public Nullable<int> Imported_GT { get; set; }
        public Nullable<int> Imported_Revenue { get; set; }
    }
}
