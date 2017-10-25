using Helpers.Security;
using Reports.Services;
using Reports.Services.Models;
using Reports.Services.Models.UserAdministration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Reports.Web.Controllers
{
    [Authorize]
    public class UserAdministrationController : Controller
    {

        UserAdministrationService UserAdministrationService = new UserAdministrationService();

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
            res.RoleID = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.RoleID;
            return res;
        }
        //
        // GET: /UserAdministration/

        public ActionResult ViewUsers()
        {
            var userAdministration = new UserAdministration();
            var roleID = GetUserSettings().RoleID;
            var company = GetUserSettings().CompanyName;
            var companies = UserAdministrationService.GetCompanies().ToList();
            var applicationRoles = UserAdministrationService.GetApplicationRoles(roleID).ToList();

            if (roleID != 1)
            {
                companies = companies.Where(x => x.Value.Equals("0") || x.Text.Equals(company)).ToList();
            }
            userAdministration.Companies = companies;
            userAdministration.ApplicationRoles = applicationRoles;
            userAdministration.ApplicationMenus = UserAdministrationService.GetApplicationMenu().ToList();
            return View(userAdministration);
        }

        [HttpPost]
        public JsonResult SearchUsers(string CompanyID, string User, string ApplicationRoleID)
        {
            var roleID = GetUserSettings().RoleID;
            var company = GetUserSettings().CompanyName;
            try
            {
                var response = UserAdministrationService.GetUsers(CompanyID, ApplicationRoleID, User);
                if (roleID != 1)
                {
                    response = response.Where(x => x.RoleID >= roleID && x.Company.Equals(company)).ToList();
                }
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetUserInformation(string recordID)
        {
            try
            {
                var response = UserAdministrationService.GetUserInformation(recordID);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public JsonResult InsertOrUpdateUserInfo(UserInformation userInformation)
        {
            try
            {
                var response = UserAdministrationService.InsertOrUpdateUserInfo(userInformation);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public JsonResult DeleteUser(string userID)
        {
            try
            {
                var response = UserAdministrationService.DeleteUser(userID);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

    }
}
