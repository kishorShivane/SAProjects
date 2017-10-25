using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services.Models
{
   public  class UserSettings
    {
        public string ConnectionKey { get; set; }
        public string CompanyName { get; set; }
        public string Username { get; set; }
        public List<string> AccessCodes { get; set; }
        public int RoleID { get; set; }

    }
}
