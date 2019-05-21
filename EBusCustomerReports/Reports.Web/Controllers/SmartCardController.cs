using Helpers.Security;
using Reports.Services;
using Reports.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Reports.Web.Controllers
{
    [Authorize]
    public class SmartCardController : Controller
    {
        SmartCardService SmartCardService = new SmartCardService();

        public string ConnectionKey
        {
            get { if (((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey != null) { return ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey; } else return ""; }
        }

        private UserSettings GetUserSettings()
        {
            var res = new UserSettings();
            res.ConnectionKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            res.CompanyName = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;
            res.Username = HttpContext.User.Identity.Name;
            return res;
        }

        // GET: SmartCard
        public ActionResult Index()
        {
            return View();
        }
    }
}