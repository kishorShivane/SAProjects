using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reports.Services.Models.UserAdministration
{
    public class UserAdministration
    {
        public List<SelectListItem> Companies { get; set; }
        public List<SelectListItem> ApplicationRoles { get; set; }
        public List<AdministrationDB.ApplicationMenu> ApplicationMenus { get; set; }
    }

    public class Company
    {
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public string ConnectionKey { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public bool Status { get; set; }
    }
}
