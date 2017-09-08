using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Reports.Services.Models.StaffMaster
{
    public class StaffMaster
    {
        public StaffMaster()
        {
            StaffTypes = new List<SelectListItem>();
            Locations = new List<SelectListItem>();
        }
        public string StaffNumber { get; set; }
        public string StaffName { get; set; }
        public bool InUse { get; set; }
        public string LocationCode { get; set; }
        public string StaffTypeID { get; set; }
        public string StaffType { get; set; }
        public string StaffSubTypeID { get; set; }
        public List<SelectListItem> StaffTypes { get; set; }
        public List<SelectListItem> Locations { get; set; }
        public string PIN { get; set; }
        public string PINSeed { get; set; }
        public bool Status { get; set; }
        public string SerialNumber { get; set; }
        public bool IsSpecialStaff { get; set; }
    }
}