using Helpers.Security;
using Reports.Services;
using Reports.Services.Models;
using Reports.Services.Models.SmartCard;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Reports.Web.Controllers
{
    [Authorize]
    public class SmartCardController : Controller
    {
        private SmartCardMasterService SmartCardService = new SmartCardMasterService();

        public string ConnectionKey
        {
            get
            {
                if (((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey != null) { return ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey; }
                else
                {
                    return "";
                }
            }
        }

        private UserSettings GetUserSettings()
        {
            UserSettings res = new UserSettings
            {
                ConnectionKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey,
                CompanyName = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName,
                Username = HttpContext.User.Identity.Name
            };
            return res;
        }

        // GET: SmartCard
        public ActionResult Index()
        {
            SmartCardData model = new SmartCardData
            {
                SmartCardTypes = SmartCardService.GetSmartCardType(ConnectionKey)
            };
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchSmartCard(string smartCardNumber, string firstName, string status, string idNumber, string cellPhone)
        {
            try
            {
                List<SmartCardData> response = SmartCardService.GetSmartCard(ConnectionKey, smartCardNumber, firstName, status, idNumber, cellPhone);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult InsertOrUpdateSmartCard(SmartCardData smartCardData)
        {
            try
            {
                int response = SmartCardService.InsertOrUpdateSmartCard(smartCardData, ConnectionKey);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }


        [HttpPost]
        public JsonResult SetSmartCardStatus(bool status, string StaffTypeID)
        {
            string result = "";
            try
            {
                bool response = SmartCardService.SetSmartCardStatus(status, StaffTypeID, ConnectionKey);
                if (response)
                {
                    if (status)
                    {
                        result = "SmartCard activated successfully !!";
                    }
                    else
                    {
                        result = "SmartCard de-activated successfully !!";
                    }
                }
                else { result = "Operation Failed!!"; }
                return Json(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}