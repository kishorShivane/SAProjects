using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models.UserAdministration
{
    public class ApplicationFunctionality
    {
        public int ID { get; set; }
        public int ScreenID { get; set; }
        public string FunctionalityName { get; set; }
        public string FunctionalityCode { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public bool Status { get; set; }

    }
}
