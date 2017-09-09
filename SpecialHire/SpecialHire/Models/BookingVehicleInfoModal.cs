using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpecialHire.Models
{
    public class BookingVehicleInfoModal
    {
        public int ID { get; set; }
        public string BusType { get; set; }
        public int BusTypeID { get; set; }
        public int Capacity { get; set; }
        public int Standing { get; set; }
        public int Sitting { get; set; }
        public string Description { get; set; }
        public int Quantitiy { get; set; }
    }
}