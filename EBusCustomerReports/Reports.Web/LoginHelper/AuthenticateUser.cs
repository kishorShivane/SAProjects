using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Helpers.Security;
using ViewModels;

namespace LoginHelper
{
    public class AuthenticateUser : Controller
    {
        public void SetAuthenticationCookie(LoginViewModel viewModel)
        {
            var tkt = new FormsAuthenticationTicket(
                       1, viewModel.UserName, DateTime.Now, DateTime.Now.AddDays(1), true, string.Empty);
            var encryptedTkt = FormsAuthentication.Encrypt(tkt);

            var formsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTkt);
            System.Web.HttpContext.Current.Response.Cookies.Add(formsCookie);
            SetUserInformation(viewModel.CompanyName, viewModel.ConnKey,viewModel.AccessCodes,viewModel.RoleID);
        }

        public void SetUserInformation(string companyName, string ConnKey, List<string> accessCodes, int roleID)
        {
            var authCookie = System.Web.HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName];
            var ticket = FormsAuthentication.Decrypt(authCookie.Value);

            var properties = new PrincipleProperties(ticket.UserData)
            {
                CompanyName = companyName,
                ConnKey = ConnKey,
                AccessCodes = accessCodes,
                RoleID = roleID
            };

            var newTicket = SetPrinciple(ticket, properties);
            properties = new PrincipleProperties(newTicket.UserData);

            SetPrinciple(ticket, properties);
        }

        public FormsAuthenticationTicket SetPrinciple(FormsAuthenticationTicket ticket, PrincipleProperties properties)
        {
            var newticket = new FormsAuthenticationTicket(
                ticket.Version,
                ticket.Name,
                ticket.IssueDate,
                ticket.Expiration,
                ticket.IsPersistent,
                properties.Serialize(),
                ticket.CookiePath);

            var encryptedTkt = FormsAuthentication.Encrypt(newticket);

            var formsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTkt);
            System.Web.HttpContext.Current.Response.Cookies.Add(formsCookie);

            var ebusIdentity = new EBusIdentity(ticket);
            var principle = new EBusPrinciple(ebusIdentity, properties);
            Thread.CurrentPrincipal = principle;
            return newticket;
        }

    }
}
