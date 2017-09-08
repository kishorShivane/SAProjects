using Helpers.Security;
using Reports.Services;
using Reports.Services.Models;
using Reports.Services.Models.RouteMaster;
using Reports.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Reports.Web.Controllers
{
    [Authorize]
    public class RouteMasterController : Controller
    {
        RouteMasterService RouteMasterService = new RouteMasterService();

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

        public ActionResult Index()
        {
            var obj = new RouteMaster();
            obj.SubRouteMaster.Contracts = RouteMasterService.GetContracts(ConnectionKey).ToList();
            obj.Routes = RouteMasterService.GetRoutes(ConnectionKey).ToList();
            return View(obj);
        }

        [HttpPost]
        public JsonResult SearchMainRoutes(string routeNumber)
        {
            try
            {
                var response = RouteMasterService.GetMainRoutes(ConnectionKey, routeNumber);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetSubRoutesForRoutes(string routeNumber)
        {
            try
            {
                var response = RouteMasterService.GetSubRoutesForRoutes(ConnectionKey, routeNumber);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }


        [HttpPost]
        public JsonResult GetStages(string routeNumber)
        {
            try
            {
                var response = RouteMasterService.GetStages(ConnectionKey, routeNumber);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult InsertOrUpdateSubRoute(SubRouteMaster SubRouteMaster)
        {
            try
            {
                var response = RouteMasterService.InsertOrUpdateSubRoute(SubRouteMaster, ConnectionKey);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult DeleteSubRoute(string subRouteNumber,string routeNumber)
        {
            try
            {
                var response = RouteMasterService.DeleteSubRoute(subRouteNumber, routeNumber, ConnectionKey);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetLastSubRouteNumber(string routeNumber)
        {
            try
            {
                var response = RouteMasterService.GetLastSubRouteNumber(routeNumber, ConnectionKey);
                response = response + 1;

                return Json(response.ToString().PadLeft(4, '0'));
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
