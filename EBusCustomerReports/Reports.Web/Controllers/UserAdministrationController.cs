using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Reports.Web.Controllers
{
    [Authorize]
    public class UserAdministrationController : Controller
    {
        //
        // GET: /UserAdministration/

        public ActionResult ViewExistingUsers()
        {
            return View();
        }

    }
}
