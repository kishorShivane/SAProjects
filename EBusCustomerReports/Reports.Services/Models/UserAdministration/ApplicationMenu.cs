using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models.UserAdministration
{
    public class ApplicationMenu
    {
        public int ID { get; set; }
        public string ApplicationMenuName { get; set; }
        public string ApplicationMenuCode { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public bool Status { get; set; }
        public List<ApplicationScreen> ApplicationScreens { get; set; } = new List<ApplicationScreen>();

    }
}
