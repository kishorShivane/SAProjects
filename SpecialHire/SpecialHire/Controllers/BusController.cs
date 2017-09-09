using SpecialHire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpecialHire.Controllers
{
    public class BusController : BaseController
    {
        // GET: Bus
        public ActionResult Index()
        {
            ViewBag.UserName = commonHelper.GetLoggedInUserName();
            BusModal busModal = new BusModal();
            try
            {
                busModal.BusTypes = DBHelper.GetBusTypes();
            }
            catch (System.Exception)
            {
                throw;
            }

            return View(busModal);
        }


        [HttpPost]
        public JsonResult SearchBusses(string BusNumber, string NumberPlate, int BusTypeID)
        {
            try
            {
                var response = DBHelper.SearchBusses(BusNumber, NumberPlate, BusTypeID);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}