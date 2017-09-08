using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reports.Services.Models
{
    public class Duty
    {
        
        public int ID { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string LocationID { get; set; }
        public int Period { get; set; }
        public bool IsDeleted { get; set; }
        public int ReferenceDutyID { get; set; }
        public int WorkSpaceID { get; set; }
        public string WorkSpace { get; set; }
        public List<SelectListItem> Locations { get; set; }
    }
}
