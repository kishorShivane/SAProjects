using Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Reports.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        public override void Init()
        {
            PostAuthenticateRequest += Application_PostAuthenticateRequest;
            base.Init();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie == null)
            {
                return;
            }

            if (authCookie.Value == string.Empty)
            {
                return;
            }

            var ticket = FormsAuthentication.Decrypt(authCookie.Value);

            var properties = new PrincipleProperties(ticket.UserData);

            var windigoIdentity = new EBusIdentity(ticket);
            var principle = new EBusPrinciple(windigoIdentity, properties);

            HttpContext.Current.User = principle;
            Thread.CurrentPrincipal = principle;
        }
    }
}