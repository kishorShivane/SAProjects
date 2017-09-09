using EbusDataProvider;
using Newtonsoft.Json;
using SpecialHire.Models;
using SpecialHire.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SpecialHire.Controllers
{
    public class BookingQuoteController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.UserName = commonHelper.GetLoggedInUserName();
            BookingQuoteInfoModal bookingQuoteInfo = new BookingQuoteInfoModal();
            try
            {
                bookingQuoteInfo.Titles = commonHelper.GetTitles();
                bookingQuoteInfo.Time = commonHelper.GetTime();
                bookingQuoteInfo.Events = DBHelper.GetEvents();
                bookingQuoteInfo.PaymentTerms = DBHelper.GetPaymentTerms();
                bookingQuoteInfo.PaymentModes = DBHelper.GetPaymentModes();
                bookingQuoteInfo.BusTypes = DBHelper.GetBusTypes();
                bookingQuoteInfo.TrailerTypes = DBHelper.GetTrailerTypes();
            }
            catch (System.Exception)
            {

                throw;
            }

            return View(bookingQuoteInfo);
        }

        [HttpPost]
        public JsonResult SearchBookingQuotes(string QuotationID, string PhoneNumber, string Name)
        {
            try
            {
                var response = DBHelper.SearchBookingQuotes(QuotationID, PhoneNumber, Name);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetQuotationByID(int QuotationID)
        {
            try
            {
                //var bookingQuote = new BookingQuoteInfo();
                var response = DBHelper.GetBookingQuoteByID(QuotationID);

                return response == null ? Json("NoRecordsFound") : Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetBookingByQuotationID(int QuotationID)
        {
            try
            {
                //var bookingQuote = new BookingQuoteInfo();
                var response = DBHelper.GetBookingByQuotationID(QuotationID);

                return response == null ? Json("NoRecordsFound") : Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetVehicleDetailsByQuotationID(int QuotationID)
        {
            try
            {
                //var bookingQuote = new BookingQuoteInfo();
                var response = DBHelper.GetVehicleDetailsByQuotationID(QuotationID);

                return response == null ? Json("NoRecordsFound") : Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetTrailerDetailsByQuotationID(int QuotationID)
        {
            try
            {
                //var bookingQuote = new BookingQuoteInfo();
                var response = DBHelper.GetTrailerDetailsByQuotationID(QuotationID);

                return response == null ? Json("NoRecordsFound") : Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GenerateBookingQuote(BookingQuoteInfoModal bookingQuoteInfo)
        {
            try
            {
                DBHelper.GenerateBookingQuote(ref bookingQuoteInfo);
                GenerateQuotationToPDF(bookingQuoteInfo);
                return Json("Record saved successfully!!");
            }
            catch (System.Exception)
            {
                throw;
            }
            return Json("Failed");
        }

        [HttpPost]
        public JsonResult GetBusTypeDetails(int BusTypeID)
        {
            try
            {
                var response = DBHelper.GetBusTypeDetails(BusTypeID);

                return response == null ? Json("NoRecordsFound") : Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public JsonResult GetTrailerTypeDetails(int TrailerTypeID)
        {
            try
            {
                var response = DBHelper.GetTrailerTypeDetails(TrailerTypeID);

                return response == null ? Json("NoRecordsFound") : Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public ActionResult ExportQuotationToPDF()
        {
            return View();
        }

        public void GenerateQuotationToPDF(BookingQuoteInfoModal bookingQuoteInfo)
        {
            var count = 0;
            var temp = "";
            var vehicles = bookingQuoteInfo.BookingVehicleInfo;
            bookingQuoteInfo.BookingVehicleInfo = new List<BookingVehicleInfoModal>();
            vehicles.Sort();
            for (var i = 0; i < vehicles.Count; i++)
            {
                if (i != 0)
                {
                    if (temp == vehicles[i].BusType)
                    { count = count + 1; }
                    else
                    {
                        bookingQuoteInfo.BookingVehicleInfo.Add(new BookingVehicleInfoModal() { BusType = vehicles[i].BusType, Quantitiy = count });
                        temp = vehicles[i].BusType;
                        count = 1;
                    }
                }
                else {
                    temp = vehicles[i].BusType;
                    count = count + 1;
                }
            }
            bookingQuoteInfo.BookingVehicleInfo.Add(new BookingVehicleInfoModal() { BusType = temp, Quantitiy = count });

            if (bookingQuoteInfo.IsTrailerRequired)
            {
                count = 0;
                temp = "";
                var trailers = bookingQuoteInfo.BookingTrailerInfo;
                bookingQuoteInfo.BookingTrailerInfo = new List<BookingTrailerInfoModal>();
                trailers.Sort();
                for (var i = 0; i < trailers.Count; i++)
                {
                    if (i != 0)
                    {
                        if (temp == trailers[i].TrailerType)
                        { count = count + 1; }
                        else
                        {
                            bookingQuoteInfo.BookingTrailerInfo.Add(new BookingTrailerInfoModal() { TrailerType = trailers[i].TrailerType, Quantitiy = count });
                            temp = trailers[i].TrailerType;
                            count = 1;
                        }
                    }
                    else {
                        temp = trailers[i].TrailerType;
                        count = count + 1;
                    }
                }
                bookingQuoteInfo.BookingTrailerInfo.Add(new BookingTrailerInfoModal() { TrailerType = temp, Quantitiy = count });
            }

            if (bookingQuoteInfo.CompTelephoneExtension != String.Empty)
            {
                bookingQuoteInfo.CompTelephoneNumber = string.Empty;
                bookingQuoteInfo.CompTelephoneNumber = bookingQuoteInfo.CompTelephoneNumber + bookingQuoteInfo.CompTelephoneExtension;
            }
            var extension = ".pdf";
            var fileName = "SpecialHire_Quotation_" + bookingQuoteInfo.ID.ToString() + "_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + extension;

            var actionResult = new Rotativa.ViewAsPdf("ExportQuotationToPDF",bookingQuoteInfo);
            var byteArray = actionResult.BuildPdf(ControllerContext);
            var fileStream = new FileStream(Server.MapPath("~/PDF/"+ fileName), FileMode.Create, FileAccess.Write);
            fileStream.Write(byteArray, 0, byteArray.Length);
            fileStream.Close();

            bookingQuoteInfo.QuotationFileName = fileName;
            DBHelper.UpdateQuotationFileName(fileName,bookingQuoteInfo.ID);
            EmailHelper.SendQuotationConfirmation(bookingQuoteInfo);
        }

        public ActionResult UrlAsPDF()
        {
            return new Rotativa.UrlAsPdf("http://www.Google.com") { FileName = "UrlTest.pdf" };
        }
        public ActionResult Table()
        {
            return View();
        }

    }
}