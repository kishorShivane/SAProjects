using System;

namespace EbusFileImporter.DataProvider
{
    public class Status
    {
        public string ETMNum { get; set; }
        public string ModuleNum { get; set; }
        public string IPAddress { get; set; }
        public Nullable<int> EquipmentTy { get; set; }
        public string BusNum { get; set; }
        public Nullable<int> DistrictId { get; set; }
        public Nullable<int> GarageId { get; set; }
        public Nullable<int> CustomerCode { get; set; }
        public Nullable<int> SubCustomer { get; set; }
        public string VCFVersion { get; set; }
        public string CodeVersion { get; set; }
        public string FLUVersion { get; set; }
        public string FileMan { get; set; }
        public string OptionsVer { get; set; }
        public string LastGoodCalDate { get; set; }
        public string LastGoodCalTime { get; set; }
        public string ETMDate { get; set; }
        public string ETMTime { get; set; }
        public string LastAuditSeqNum { get; set; }
        public string SIMID { get; set; }

    }
}
