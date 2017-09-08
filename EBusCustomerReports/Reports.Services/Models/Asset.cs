using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reports.Services.Models
{
    public class Asset
    {        
        public int ID { get; set; }
        public string ETMSerialNumber { get; set; }
        public string ETMType { get; set; }
        public DateTime LastCommunicatedDate { get; set; }
    }
}
