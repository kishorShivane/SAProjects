using System;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using System.Xml.Linq;
using LoginHelper;
using ViewModels;
using Reports.Services;

namespace ProductTracking.Web.Controllers
{
    public class LoginController : Controller
    {

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            var viewModel = new LoginViewModel();
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
                new AuthenticateUser().SetAuthenticationCookie(viewModel);
                return RedirectToAction("Index", "Report");
            }
            return RedirectToAction("Login", "Login");
        }


        private bool ValidateUserCredentials(LoginViewModel viewModel)
        {
            var isValidUser = false;
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
            var valid = false;

            string usersXmlPath = Server.MapPath("~/XML_Files/ApplicationsUsersList.xml");

            var doc = XDocument.Load(usersXmlPath);
            var userNames = (from c in doc.Root.Descendants("user")
                            where c.Attribute("name").Value.ToLower() == viewModel.UserName.ToLower()
                            select c).FirstOrDefault();

            if (userNames != null && userNames.Attribute("password").Value.ToString().ToLower() == viewModel.Password.ToLower())
            {
                viewModel.CompanyName = userNames.Attribute("companyname").Value.ToString();
                viewModel.ConnKey = userNames.Attribute("connectionkey").Value.ToString().ToLower();
                valid = true;
            }

            return valid;
        }
    }
}
