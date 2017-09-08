using Helpers.Security;
using Reports.Services;
using Reports.Services.Models;
using Reports.Services.Models.ServiceRequest;
using Reports.Services.Models.StaffMaster;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System;
using Reports.Web.Helpers;

namespace Reports.Web.Controllers
{
    [Authorize]
    public class ServiceRequestController : Controller
    {
        ServiceRequestService ServiceRequestService = new ServiceRequestService();

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
            var userid = GetUserSettings().Username;
            var obj = new ServiceRequest();
            obj.RequestStatuss = ServiceRequestService.GetRequestStatus(ConnectionKey).ToList();
            obj.RequestTypes = ServiceRequestService.GetRequestType(ConnectionKey).ToList();
            obj.IsSpecialUser = IsSpecialUser(userid);
            return View(obj);
        }

        public bool IsSpecialUser(string userid)
        {
            return string.IsNullOrEmpty(ConfigurationManager.AppSettings["SpecialUsers"]) ? false : ConfigurationManager.AppSettings["SpecialUsers"].Contains(userid);
        }

        [HttpPost]
        public JsonResult SearchServiceRequest(string status)
        {
            try
            {
                var userid = GetUserSettings().Username;

                var response = ServiceRequestService.GetServiceRequests(ConnectionKey, status, IsSpecialUser(userid) ? "" : userid);
                Session["SearchedServiceRequest"] = response.OrderBy(x => x.ServiceRequestID).ToList();
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetServiceRequestAudit(string serviceRequestID)
        {
            try
            {
                var response = ServiceRequestService.GetServiceRequestAudit(ConnectionKey, serviceRequestID);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult InsertOrUpdateServiceRequest(ServiceRequest ServiceRequest)
        {
            try
            {
                ServiceRequest.ServiceRequestID = GetServiceRequestIDFormat();
                var userid = GetUserSettings().Username;
                var response = ServiceRequestService.InsertOrUpdateServiceRequest(ServiceRequest, ConnectionKey, userid);
                if (ServiceRequest.RequestStatusID == 1 || ServiceRequest.RequestStatusID == 3)
                {
                    var filePath = HttpContext.Server.MapPath("../EmailTemplete/ServiceRequestMailTemplate.html");
                    MailHelper.SendServiceRequestMail(filePath, ServiceRequest.Email, ServiceRequest.RequestType,
                        ServiceRequest.ServiceRequestID + response, userid, ServiceRequest.Comments, ServiceRequest.Priority, ServiceRequest.RequestStatus);
                }
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public ActionResult PrintStaffs()
        {
            var objCollection = (List<ServiceRequest>)Session["SearchedServiceRequest"];
            if (objCollection != null && objCollection.Any())
            {
                objCollection.OrderBy(x => x.ID);
            }
            return View(objCollection);
        }

        public string GetServiceRequestIDFormat()
        {
            var res = "";
            switch (ConnectionKey.Trim())
            {
                case "tabanana30":
                    res = "Ebus-NTB-";
                    break;
                case "mokopane10":
                    res = "Ebus-MP-";
                    break;
                case "marblehall20":
                    res = "Ebus-MH-";
                    break;
                case "gautengcoaches40":
                    res = "Ebus-GC-";
                    break;
                case "ugu50":
                    res = "Ebus-UGU-";
                    break;
                case "ikhwezi60":
                    res = "Ebus-IKH-";
                    break;
                case "atamelang70":
                    res = "Ebus-ATG-";
                    break;
                case "sihlangene80":
                    res = "Ebus-SBC-";
                    break;
                case "ezakheni90":
                    res = "Ebus-EZT-";
                    break;
                case "mthonjaneni95":
                    res = "Ebus-MTS-";
                    break;
                default:
                    res = "Ebus-Def-";
                    break;
            }
            return res;
        }
    }
}