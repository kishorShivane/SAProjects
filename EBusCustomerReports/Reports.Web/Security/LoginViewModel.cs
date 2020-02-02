using System.Collections.Generic;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System;

namespace ViewModels
{
    public class LoginViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string CompanyName { get; set; }
        public string ConnKey { get; set; }
        public List<string> AccessCodes { get; set; }
        public int RoleID { get; set; }
        public Nullable<DateTime> WarningDate{ get; set; }
        public Nullable<DateTime> LastDate { get; set; }
    }
}