using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpecialHire.Models
{
    public class BookingTrailerInfoModal
    {
        public int ID { get; set; }
        public string TrailerType { get; set; }
        public int TrailerTypeID { get; set; }
        public int KG { get; set; }
        public string Description { get; set; }
        public int Quantitiy { get; set; }
    }
}