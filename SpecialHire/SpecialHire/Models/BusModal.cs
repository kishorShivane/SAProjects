using EbusDataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpecialHire.Models
{
    public class BusModal
    {
        public int ID { get; set; }
        public string BusName { get; set; }
        public int BusTypeID { get; set; }
        public string BusType { get; set; }
        public string BusNumber { get; set; }
        public string NumberPlate { get; set; }
        public bool Status { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public List<SelectListItem> BusTypes { get; set; }
    }
}