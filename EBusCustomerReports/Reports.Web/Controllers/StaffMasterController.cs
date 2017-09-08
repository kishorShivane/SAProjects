using Helpers.Security;
using Reports.Services;
using Reports.Services.Models;
using Reports.Services.Models.StaffMaster;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Reports.Web.Controllers
{
    [Authorize]
    public class StaffMasterController : Controller
    {
        StaffMasterService StaffMasterService = new StaffMasterService();

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
            var obj = new StaffMaster();
            obj.StaffTypes = StaffMasterService.GetStaffType(ConnectionKey).ToList();
            obj.Locations = StaffMasterService.GetLocations(ConnectionKey).ToList();
            obj.PINSeed = ConfigurationManager.AppSettings["DefaultPINSeed"].ToString();
            return View(obj);
        }

        [HttpPost]
        public JsonResult SearchStaff(string staffName, string staffNumber, string status, string pinSeed,string staffType, string location)
        {
            try
            {
                var response = StaffMasterService.GetStaffs(ConnectionKey, staffName, staffNumber, status, pinSeed, staffType,location);
                Session["SearchedStaff"] = response.OrderBy(x=>x.StaffName).ToList();
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult InsertOrUpdateStaff(StaffMaster StaffMaster, bool isAdd)
        {
            try
            {
                var response = StaffMasterService.InsertOrUpdateStaff(StaffMaster, ConnectionKey, isAdd);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetStaffType()
        {
            try
            {
                var response = StaffMasterService.GetStaffType(ConnectionKey);
                return Json(response.OrderBy(x=>x.Value));
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public JsonResult DeleteStaffType(string StaffTypeID)
        {
            var result = "";
            try
            {
                var response = StaffMasterService.DeleteStaffType(StaffTypeID, ConnectionKey);
                if (response) { result = "Deleted Successfully !!"; } else { result = "Delete failed, Staff Type trying to delete has dependency!!"; }
                return Json(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult InsertOrUpdateStaffType(int staffTypeID, string staffType,string isAdd)
        {
            try
            {
                var response = StaffMasterService.InsertOrUpdateStaffType(staffType, staffTypeID,isAdd, ConnectionKey);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetLocations()
        {
            try
            {
                var response = StaffMasterService.GetLocations(ConnectionKey);
                return Json(response.OrderBy(x => x.Value));
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult DeleteLocation(string LocationCode)
        {
            var result = "";
            try
            {
                var response = StaffMasterService.DeleteLocation(LocationCode, ConnectionKey);
                if (response) { result = "Deleted Successfully !!"; } else { result = "Delete failed, Location trying to delete has dependency!!"; }
                return Json(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult InsertOrUpdateLocation(string locationCode, string locationName,string isAdd)
        {
            try
            {
                var response = StaffMasterService.InsertOrUpdateLocation(locationCode, locationName, isAdd,ConnectionKey);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public ActionResult PrintStaffs()
        {
            var objCollection = (List<StaffMaster>)Session["SearchedStaff"];
            if (objCollection != null && objCollection.Any()) {
                objCollection.Where(x => x.Status).OrderBy(x=>x.StaffNumber).GroupBy(x => x.LocationCode);
            }
            return View(objCollection);
        }
    }
}