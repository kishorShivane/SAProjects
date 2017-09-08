using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;
using Helpers;
using Reports.Services;
using Helpers.Security;
using System.Threading;
using Reports.Services.Models;

namespace Reports.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        public ActionResult StaffEditor()
        {
            var service = new UserService();
            var model = service.GetAllStaffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View(model);
        }

        public ActionResult UserAccess()
        {
            return View();
        }

        public ActionResult Users()
        {
            return View();
        }
    }
}
