using Helpers.Security;
using Reports.Services;
using Reports.Services.Models;
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
    public class SchedulingSystemController : Controller
    {

        DutyWizardService DutyWizardService = new DutyWizardService();

        public bool EnableMailTracking
        {
            get { if (ConfigurationManager.AppSettings["SMTP"] == "true") { return true; } else return false; }
        }

        public string ConnectionKey
        {
            get { if (((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey != null) { return ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey; } else return ""; }
        }

        public string CompanyName
        {
            get { if (((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName != null) { return ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName; } else return ""; }
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
            var obj = new DutyWizard();
            var hours = MasterHelper.GetHours();
            var minutes = MasterHelper.GetMinutes();
            var locations = DutyWizardService.GetLocations(ConnectionKey).ToList();
            obj.DutyTrip.Hours = hours.ToList();
            obj.DutyTrip.Minutes = minutes.ToList();
            obj.DutyBooking.Hours = hours.ToList();
            obj.DutyBooking.Minutes = minutes.ToList();
            obj.DutyEvent.Hours = hours.ToList();
            obj.DutyEvent.Minutes = minutes.ToList();
            obj.Duty.Locations = locations.Where(x => x.Value != "0").ToList();
            obj.Locations = locations;

            obj.DutyBooking.Stages = DutyWizardService.GetStages(ConnectionKey).ToList();
            obj.DutyTrip.Routes = DutyWizardService.GetRoutes(ConnectionKey, true).ToList();
            obj.DutyTrip.Contract = DutyWizardService.GetContracts(ConnectionKey).ToList();
            obj.WorkSpaces = DutyWizardService.GetWorkSpaces(ConnectionKey).ToList();
            obj.DutyEvent.Events = DutyWizardService.GetEvents(ConnectionKey).ToList();
            return View(obj);

            return View();
        }

        [HttpPost]
        public JsonResult SearchDuties(int WorkSpaceID, string DutyID, string locationID)
        {
            try
            {
                var userset = GetUserSettings();
                var conKey = userset.ConnectionKey;
                var comp = userset.CompanyName;
                var response = DutyWizardService.GetDutiesForWorkspace(conKey, WorkSpaceID, DutyID, locationID);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        #region Get Data
        [HttpPost]
        public JsonResult GetOperatedDaysForDuty(int DutyID)
        {
            try
            {
                var response = DutyWizardService.GetOperatedDaysForDuty(ConnectionKey, DutyID);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetBookingsForOperatedDay(int OperatedDayID)
        {
            try
            {
                var response = DutyWizardService.GetBookingsForOperatedDay(ConnectionKey, OperatedDayID);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetTripsForBooking(int BookingID)
        {
            try
            {
                var response = DutyWizardService.GetTripsForBooking(ConnectionKey, BookingID);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetEventsForBooking(int BookingID)
        {
            try
            {
                var response = DutyWizardService.GetEventsForBooking(ConnectionKey, BookingID);
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetRoutesForPosition(bool isPosition)
        {
            try
            {
                var response = DutyWizardService.GetRoutes(ConnectionKey, isPosition).ToList();
                return Json(response);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GetEvents()
        {
            try
            {
                var response = DutyWizardService.GetEvents(ConnectionKey).ToList();
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        #endregion

        #region Insert or Update
        [HttpPost]
        public JsonResult InsertOrUpdateDuty(Duty Duty)
        {
            try
            {
                var subject = "";
                var body = "";
                if (Duty.ID > 0 && EnableMailTracking)
                {
                    subject = "Updated Duty";
                    body = GetMailContentDuty(Duty.ID, "Before");
                }
                var status = DutyWizardService.InsertOrUpdateDuty(Duty, ConnectionKey);
                if (status != -1 && EnableMailTracking)
                {
                    if (Duty.ID == 0)
                    {
                        subject = "Created New Duty";
                        body = GetMailContentDuty(0, "Inserted Duty");
                    }
                    else
                    {
                        body = body + GetMailContentDuty(Duty.ID, "After");
                    }
                    new Task(() =>
                    {
                        DutyWizardService.SendEmail(subject, body);
                    }).Start();
                }
                return Json(status);
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
            return Json("Failed");
        }

        [HttpPost]
        public JsonResult InsertOrUpdateDutyOperatedDays(DutyOperatedDay DutyOperatedDay)
        {
            try
            {
                var subject = "";
                var body = "";
                if (DutyOperatedDay.ID > 0 && EnableMailTracking)
                {
                    subject = "Updated Operating Days";
                    body = GetMailContentOperatingDay(DutyOperatedDay.ID, "Before");
                }
                DutyWizardService.InsertOrUpdateDutyOperatedDays(DutyOperatedDay, ConnectionKey);

                if (EnableMailTracking)
                {
                    if (DutyOperatedDay.ID == 0)
                    {
                        subject = "Created New Operating Days";
                        body = GetMailContentOperatingDay(0, "Inserted Operating Days");
                    }
                    else
                    {
                        body = body + GetMailContentOperatingDay(DutyOperatedDay.ID, "After");
                    }
                    new Task(() =>
                    {
                        DutyWizardService.SendEmail(subject, body);
                    }).Start();
                }
                return Json("Operating Days saved successfully!!");
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
            return Json("Failed");
        }

        [HttpPost]
        public JsonResult InsertOrUpdateDutyBooking(DutyBooking DutyBooking, string isSlot)
        {
            try
            {
                var subject = "";
                var body = "";
                if (DutyBooking.ID > 0 && EnableMailTracking)
                {
                    subject = "Updated Booking";
                    body = GetMailContentBooking(DutyBooking.ID, "Before");
                }
                var status = DutyWizardService.InsertOrUpdateDutyBooking(DutyBooking, ConnectionKey);
                if (status != -1 && EnableMailTracking)
                {
                    if (DutyBooking.ID == 0)
                    {
                        subject = "Created New Booking";
                        body = GetMailContentBooking(0, "Inserted Booking");
                    }
                    else
                    {
                        body = body + GetMailContentBooking(DutyBooking.ID, "After");
                    }
                    new Task(() =>
                    {
                        DutyWizardService.SendEmail(subject, body);
                    }).Start();
                }
                return Json(status);
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
            return Json(-1);
        }

        [HttpPost]
        public JsonResult InsertOrUpdateDutyTrip(DutyTrip DutyTrip, string isSlot)
        {
            try
            {
                var subject = "";
                var body = "";
                if (DutyTrip.ID > 0 && EnableMailTracking)
                {
                    subject = "Updated Trip";
                    body = GetMailContentTrip(DutyTrip.ID, "Before");
                }

                var status = DutyWizardService.InsertOrUpdateDutyTrip(DutyTrip, ConnectionKey, (isSlot == "1") ? true : false);
                if (status != -1 && EnableMailTracking)
                {
                    if (DutyTrip.ID == 0)
                    {
                        subject = "Created New Trip";
                        body = GetMailContentTrip(0, "Inserted Trip");
                    }
                    else
                    {
                        body = body + GetMailContentTrip(DutyTrip.ID, "After");
                    }
                    new Task(() =>
                    {
                        DutyWizardService.SendEmail(subject, body);
                    }).Start();
                }
                return Json(status);
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
            return Json(-1);
        }

        [HttpPost]
        public JsonResult InsertOrUpdateDutyEvent(DutyEvent DutyEvent, string isSlot)
        {
            try
            {
                var subject = "";
                var body = "";
                if (DutyEvent.DutyEventID > 0 && EnableMailTracking)
                {
                    subject = "Updated Event";
                    body = GetMailContentEvent(DutyEvent.DutyEventID, "Before");
                }


                var status = DutyWizardService.InsertOrUpdateDutyEvent(DutyEvent, ConnectionKey, (isSlot == "1") ? true : false);
                if (status != -1 && EnableMailTracking)
                {
                    if (DutyEvent.DutyEventID == 0)
                    {
                        subject = "Created New Event";
                        body = GetMailContentEvent(0, "Inserted Event");
                    }
                    else
                    {
                        body = body + GetMailContentEvent(DutyEvent.DutyEventID, "After");
                    }
                    new Task(() =>
                    {
                        DutyWizardService.SendEmail(subject, body);
                    }).Start();
                }
                return Json(status);
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
            return Json(-1);
        }
        #endregion

        #region Mail Helper
        public string GetMailContentDuty(int dutyID, string headerContent)
        {
            var body = new StringBuilder();
            var duty = DutyWizardService.GetDuty(ConnectionKey, dutyID);
            body.Append("</br></br></br><h4>" + headerContent + "</h4></br><table border='5'><tr><th>DutyID</th><th>Description</th><th>Location</th><th>WorkSpaceID</th></tr><tbody>");
            body.Append("<tr>");
            body.Append("<td>");
            body.Append(duty.ReferenceDutyID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(duty.Description);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(duty.Location);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(duty.WorkSpace);
            body.Append("</td>");
            body.Append("</tr>");
            body.Append("</tbody></table>");
            return body.ToString();
        }
        public string GetMailContentBooking(int bookingID, string headerContent)
        {
            var body = new StringBuilder();
            var booking = DutyWizardService.GetBooking(ConnectionKey, bookingID);
            body.Append("</br></br></br><h4>" + headerContent + "</h4></br><table border='5'><tr><th>DutyID</th><th>OperatingDayID</th><th>BookingID</th><th>BookOnTime</th><th>BookOffTime</th><th>PlaceOn</th><th>PlaceOff</th></tr><tbody>");
            body.Append("<tr>");
            body.Append("<td>");
            body.Append(booking.DutyID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(booking.DutyOperatedDayID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(booking.DutyBookingID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(booking.BookOnTime.Hour.ToString() + ":" + booking.BookOnTime.Minute.ToString());
            body.Append("</td>");
            body.Append("<td>");
            body.Append(booking.BookOffTime.Hour.ToString() + ":" + booking.BookOffTime.Minute.ToString());
            body.Append("</td>");
            body.Append("<td>");
            body.Append(booking.PlaceOn);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(booking.PlaceOff);
            body.Append("</td>");
            body.Append("</tr>");
            body.Append("</tbody></table>");
            return body.ToString();
        }
        public string GetMailContentOperatingDay(int operatingDayID, string headerContent)
        {
            var body = new StringBuilder();
            var operatingDay = DutyWizardService.GetOperatingDays(ConnectionKey, operatingDayID);
            body.Append("</br></br></br><h4>" + headerContent + "</h4></br><table border='5'><tr><th>DutyID</th><th>OperatingDayID</th><th>OperatingDayBitMask</th><th>OperatingDayString</th></tr><tbody>");
            body.Append("<tr>");
            body.Append("<td>");
            body.Append(operatingDay.DutyID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(operatingDay.DutyOperatedDayID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(operatingDay.OperatedDayBitmask);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(operatingDay.OperatedDayString);
            body.Append("</td>");
            body.Append("</tr>");
            body.Append("</tbody></table>");
            return body.ToString();
        }
        public string GetMailContentTrip(int tripID, string headerContent)
        {
            var body = new StringBuilder();
            var trip = DutyWizardService.GetBookingTrip(ConnectionKey, tripID);
            body.Append("</br></br></br><h4>" + headerContent + "</h4></br><table border='5'><tr><th>DutyID</th><th>OperatingDayID</th><th>BookingID</th><th>TripID</th><th>MainRouteNumber</th><th>SubRouteNumber</th><th>Contract</th><th>StartTime</th><th>EndTime</th><th>Position</th></tr><tbody>");
            body.Append("<tr>");
            body.Append("<td>");
            body.Append(trip.DutyID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(trip.DutyOperatedDayID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(trip.DutyBookingID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(trip.ID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(trip.MainRouteNumber);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(trip.SubRouteNumber);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(trip.Contract);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(trip.StartTime.Hour.ToString() + ":" + trip.StartTime.Minute.ToString());
            body.Append("</td>");
            body.Append("<td>");
            body.Append(trip.EndTime.Hour.ToString() + ":" + trip.EndTime.Minute.ToString());
            body.Append("</td>");
            body.Append("<td>");
            body.Append(trip.Position);
            body.Append("</td>");
            body.Append("</tr>");
            body.Append("</tbody></table>");
            return body.ToString();
        }
        public string GetMailContentEvent(int eventID, string headerContent)
        {
            var body = new StringBuilder();
            var item = DutyWizardService.GetBookingEvent(ConnectionKey, eventID);
            body.Append("</br></br></br><h4>" + headerContent + "</h4></br><table border='5'><tr><th>DutyID</th><th>OperatingDayID</th><th>BookingID</th><th>EventID</th><th>StartTime</th><th>EndTime</th><th>Description</th></tr><tbody>");
            body.Append("<tr>");
            body.Append("<td>");
            body.Append(item.DutyID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(item.DutyOperatedDayID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(item.DutyBookingID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(item.DutyEventID);
            body.Append("</td>");
            body.Append("<td>");
            body.Append(item.StartTime.Hour.ToString() + ":" + item.StartTime.Minute.ToString());
            body.Append("</td>");
            body.Append("<td>");
            body.Append(item.EndTime.Hour.ToString() + ":" + item.EndTime.Minute.ToString());
            body.Append("</td>");
            body.Append("<td>");
            body.Append(item.Description);
            body.Append("</td>");
            body.Append("</tr>");
            body.Append("</tbody></table>");
            return body.ToString();
        }

        #endregion

        #region Delete
        [HttpPost]
        public JsonResult DeleteDuty(int DutyID)
        {
            try
            {
                var subject = "";
                var body = "";
                if (DutyID > 0 && EnableMailTracking)
                {
                    subject = "Deleted Duty";
                    body = GetMailContentDuty(DutyID, "Deleted Duty");
                }
                DutyWizardService.DeleteDuty(DutyID, ConnectionKey);
                if (EnableMailTracking)
                {
                    new Task(() =>
                    {
                        DutyWizardService.SendEmail(subject, body);
                    }).Start();
                }

                return Json("Duty deleted successfully!!");
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
            return Json("Failed");
        }

        [HttpPost]
        public JsonResult DeleteDutyOperatedDay(int DutyOperatedDayID)
        {
            try
            {
                var subject = "";
                var body = "";
                if (DutyOperatedDayID > 0 && EnableMailTracking)
                {
                    subject = "Deleted Operating Day";
                    body = GetMailContentDuty(DutyOperatedDayID, "Deleted Operating Day");
                }
                DutyWizardService.DeleteDutyOperatedDay(DutyOperatedDayID, ConnectionKey);
                if (EnableMailTracking)
                {
                    new Task(() =>
                    {
                        DutyWizardService.SendEmail(subject, body);
                    }).Start();
                }
                return Json("Operating Days deleted successfully!!");
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
            return Json("Failed");
        }

        [HttpPost]
        public JsonResult DeleteDutyBooking(int DutyBookingID)
        {
            try
            {
                var subject = "";
                var body = "";
                if (DutyBookingID > 0 && EnableMailTracking)
                {
                    subject = "Deleted Booking";
                    body = GetMailContentDuty(DutyBookingID, "Deleted Booking");
                }
                DutyWizardService.DeleteDutyBooking(DutyBookingID, ConnectionKey);
                if (EnableMailTracking)
                {
                    new Task(() =>
                    {
                        DutyWizardService.SendEmail(subject, body);
                    }).Start();
                }
                return Json("Booking deleted successfully!!");
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
            return Json("Failed");
        }

        [HttpPost]
        public JsonResult DeleteDutyTrip(int DutyTripID)
        {
            try
            {
                var subject = "";
                var body = "";
                if (DutyTripID > 0 && EnableMailTracking)
                {
                    subject = "Deleted Trip";
                    body = GetMailContentDuty(DutyTripID, "Deleted Trip");
                }
                DutyWizardService.DeleteDutyTrip(DutyTripID, ConnectionKey);
                if (EnableMailTracking)
                {
                    new Task(() =>
                    {
                        DutyWizardService.SendEmail(subject, body);
                    }).Start();
                }
                return Json("Trip deleted successfully!!");
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
            return Json("Failed");
        }

        [HttpPost]
        public JsonResult DeleteDutyEvent(int DutyEventID)
        {
            try
            {
                var subject = "";
                var body = "";
                if (DutyEventID > 0 && EnableMailTracking)
                {
                    subject = "Deleted Event";
                    body = GetMailContentDuty(DutyEventID, "Deleted Event");
                }
                DutyWizardService.DeleteDutyEvent(DutyEventID, ConnectionKey);
                if (EnableMailTracking)
                {
                    new Task(() =>
                    {
                        DutyWizardService.SendEmail(subject, body);
                    }).Start();
                }
                return Json("Event deleted successfully!!");
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
            return Json("Failed");
        }
        #endregion

        #region Dependency Check
        [HttpPost]
        public JsonResult CheckDutyDependency(int DutyID)
        {
            try
            {
                return Json(DutyWizardService.CheckDutyDependency(DutyID, ConnectionKey));
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
        }

        [HttpPost]
        public JsonResult CheckOperatedDayDependency(int DutyOperatedDayID)
        {
            try
            {
                return Json(DutyWizardService.CheckOperatedDayDependency(DutyOperatedDayID, ConnectionKey));
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
        }

        [HttpPost]
        public JsonResult CheckBookingDependency(int DutyBookingID)
        {
            try
            {
                return Json(DutyWizardService.CheckBookingDependency(DutyBookingID, ConnectionKey));
            }
            catch (System.Exception ex)
            {
                return Json(ex);
            }
        }
        #endregion

        [HttpPost]
        public JsonResult UpdateDutyScheduleInfo()
        {
            try
            {
                new Task(() =>
                {
                    var response = DutyWizardService.UpdateDutyScheduleInfo(ConnectionKey);
                    if (response != null)
                    {
                        CreateDBFFile(response);
                        DutyWizardService.SendEmail("DBF File Created Successfully", "DBF File Created Successfully" + DateTime.Now.ToShortDateString());
                    }
                }).Start();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                DutyWizardService.SendEmail("Exception", ex.InnerException.ToString());
                throw;
            }
        }

        private void CreateDBFFile(List<DutyDBF> dbfTableContent)
        {
            try
            {
                var folderName = CompanyName + DateTime.Now.ToString("ddMMyyyyHHmmss");
                string filepath = Server.MapPath("~/DBF/" + folderName);
                var provider = ConfigurationManager.AppSettings["Provider"];
                provider = provider.Replace("##filepath##", filepath);
                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                OleDbConnection dBaseConnection = null;

                string TableName = "DUTY";

                using (dBaseConnection = new OleDbConnection(provider))
                {
                    dBaseConnection.Open();

                    OleDbCommand olecommand = dBaseConnection.CreateCommand();

                    if ((System.IO.File.Exists(filepath + "/" + TableName + ".dbf")))
                    {
                        System.IO.File.Delete(filepath + "/" + TableName + ".dbf");
                        olecommand.CommandText = "CREATE TABLE [" + TableName + "] ([BOARD] varchar(4), [DUTY] varchar(5), [ROUTE] varchar(6), [JOURNEY] varchar(4), [DIR] varchar(1), [START] varchar(4), [DAYS] NUMERIC)";
                        olecommand.ExecuteNonQuery();
                    }
                    else
                    {
                        olecommand.CommandText = "CREATE TABLE [" + TableName + "] ([BOARD] varchar(4), [DUTY] varchar(5), [ROUTE] varchar(6), [JOURNEY] varchar(4), [DIR] varchar(1), [START] varchar(4), [DAYS] NUMERIC)";
                        olecommand.ExecuteNonQuery();
                    }

                    OleDbDataAdapter oleadapter = new OleDbDataAdapter(olecommand);
                    OleDbCommand oleinsertCommand = dBaseConnection.CreateCommand();

                    foreach (var item in dbfTableContent)
                    {
                        oleinsertCommand.CommandText = "INSERT INTO [" + TableName + "] ([BOARD], [DUTY],[ROUTE],[JOURNEY],[DIR],[START],[DAYS]) VALUES ('0002','2001','B004','0502','1','0502',15)";
                        //oleinsertCommand.CommandText = "INSERT INTO [" + TableName + "] ([BOARD], [DUTY],[ROUTE],[JOURNEY],[DIR],[START],[DAYS]) VALUES ('" + item.BOARD + "','" + item.DUTY + "','" + item.ROUTE + "','" + item.JOURNEY + "','" + item.DIR + "','" + item.START + "'," + item.DAYS + ")";
                        oleinsertCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                DutyWizardService.SendEmail("Exception", ex.InnerException.ToString());
                throw;
            }

        }
    }
}
