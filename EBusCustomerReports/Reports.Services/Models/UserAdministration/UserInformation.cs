using System;

namespace Reports.Services.Models.UserAdministration
{
    public class UserInformation
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int RoleID { get; set; }
        public string Role { get; set; }
        public int CompanyID { get; set; }
        public string Company { get; set; }
        public string ConnectionKey { get; set; }
        public string AccessCodes { get; set; }
        public string WarningDate { get; set; }
        public string LastDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public bool Status { get; set; }
    }
}
