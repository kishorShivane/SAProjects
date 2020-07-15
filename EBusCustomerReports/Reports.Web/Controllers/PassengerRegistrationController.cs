using Helpers.Security;
using Reports.Services;
using Reports.Services.Models;
using Reports.Services.Models.Passenger;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Reports.Web.Controllers
{
    [Authorize]
    public class PassengerRegistrationController : Controller
    {
        private PassengerRegistrationService passengerService = new PassengerRegistrationService();

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
            PassengerData model = new PassengerData
            {
                SmartCardTypes = passengerService.GetSmartCardType(ConnectionKey),
                PassengerTypes = passengerService.GetPassengerTypes()
            };
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchPassenger(string smartCardNumber, string firstName, string status, string idNumber, string cellPhone, string passengerType)
        {
            try
            {
                List<PassengerData> response = passengerService.GetPassenger(ConnectionKey, smartCardNumber, firstName, status, idNumber, cellPhone, passengerType);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult InsertOrUpdatePassenger(PassengerData smartCardData)
        {
            try
            {
                int response = passengerService.InsertOrUpdatePassenger(smartCardData, ConnectionKey);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }


        [HttpPost]
        public JsonResult SetPassengerStatus(bool status, string passengerID)
        {
            string result = "";
            try
            {
                bool response = passengerService.SetPassengerStatus(status, passengerID, ConnectionKey);
                if (response)
                {
                    if (status)
                    {
                        result = "Passenger registered successfully !!";
                    }
                    else
                    {
                        result = "Passenger removed successfully !!";
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