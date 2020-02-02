using LoginHelper;
using Reports.Services;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using ViewModels;

namespace ProductTracking.Web.Controllers
{
    public class LoginController : Controller
    {
        private UserAdministrationService userAdministrationService = new UserAdministrationService();

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (TempData["UserExpired"] == null)
            {
                TempData["UserExpired"] = false;
            }

            LoginViewModel viewModel = new LoginViewModel();
            return View();
        }

        public RedirectToRouteResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        public RedirectToRouteResult UserLoginAndRedirect(LoginViewModel viewModel)
        {
            if (ValidateUserCredentials(viewModel))
            {
                if (viewModel.LastDate == null || DateTime.Compare(viewModel.LastDate.Value, DateTime.Now) >= 0)
                {
                    new AuthenticateUser().SetAuthenticationCookie(viewModel);
                    return RedirectToAction("Index", "Report");
                }
                else
                {
                    TempData["UserExpired"] = true;
                    TempData["LastDate"] = viewModel.LastDate;
                    TempData["UserName"] = viewModel.UserName;
                    return RedirectToAction("Login", "Login");
                }
            }
            return RedirectToAction("Login", "Login");
        }


        private bool ValidateUserCredentials(LoginViewModel viewModel)
        {
            bool isValidUser = false;
            if (viewModel.UserName != null && viewModel.Password != null && viewModel.UserName.Length > 0 && viewModel.Password.Length > 0)
            {
                isValidUser = IsUserValid(viewModel);
            }
            if (!isValidUser)
            {
                TempData["error"] = "Invalid Username and Password !";
            }
            else
            {
                TempData["error"] = string.Empty;
            }
            return isValidUser;
        }

        private bool IsUserValid(LoginViewModel viewModel)
        {
            bool valid = false;

            Reports.Services.Models.UserAdministration.UserInformation userDetails = userAdministrationService.ValidateUser(viewModel.UserName, viewModel.Password);

            //string usersXmlPath = Server.MapPath("~/XML_Files/ApplicationsUsersList.xml");

            //var doc = XDocument.Load(usersXmlPath);
            //var userNames = (from c in doc.Root.Descendants("user")
            //                where c.Attribute("name").Value.ToLower() == viewModel.UserName.ToLower()
            //                select c).FirstOrDefault();

            if (userDetails != null)
            {
                viewModel.CompanyName = userDetails.Company;
                viewModel.ConnKey = userDetails.ConnectionKey;
                viewModel.RoleID = userDetails.RoleID;
                viewModel.AccessCodes = userDetails.AccessCodes.Split(',').ToList();
                viewModel.WarningDate = Convert.ToDateTime(userDetails.WarningDate);
                viewModel.LastDate = Convert.ToDateTime(userDetails.LastDate);
                valid = true;
            }

            return valid;
        }
    }
}
