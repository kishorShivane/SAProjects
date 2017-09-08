using Reports.Services.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace Reports.Services
{
    public class DutyWizardService:BaseServices
    {
        //public IEnumerable<WorkSpace> GetWorkSpaces(string connKey)
        //{
        //    var result = new List<WorkSpace>();
        //    var myConnection = new SqlConnection(GetConnectionString(connKey));

        //    try
        //    {
        //        var cmd = new SqlCommand("eBusDuty_GetWorkSpaces", myConnection)
        //        {
        //            CommandType = CommandType.StoredProcedure
        //        };

        //        cmd.CommandTimeout = 500000;
        //        myConnection.Open();
        //        SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

        //        while (dr.Read())
        //        {
        //            var item = new WorkSpace();
        //            item.ID = Convert.ToInt32(dr["ID"]);
        //            item.WorkSpaceName = dr["WorkspaceName"].ToString();
        //            result.Add(item);
        //        }
        //    }
        //    finally
        //    {
        //        myConnection.Close();
        //    }

        //    return result.OrderBy(s => s.WorkSpaceName).ToList();
        //}

        #region Get Data

        public IEnumerable<Duty> GetDutiesForWorkspace(string conKey, int workSpaceID, string dutyID = "", string locationID = "")
        {
            var result = new List<Duty>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetDutiesForWorkspace", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@WorkspaceID", workSpaceID));
                cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));
                cmd.Parameters.Add(new SqlParameter("@DutyID", dutyID == "" ? -1 : Convert.ToInt32(dutyID)));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new Duty();
                    item.ID = Convert.ToInt32(dr["ID"]);
                    item.ReferenceDutyID = Convert.ToInt32(dr["ReferenceDutyID"]);
                    item.Description = dr["Description"].ToString();
                    item.Location = dr["Location"].ToString();
                    item.LocationID = dr["LocationID"].ToString();
                    item.WorkSpaceID = Convert.ToInt32(dr["WorkSpaceID"]);
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.OrderBy(s => s.ReferenceDutyID).ToList();
        }

        public IEnumerable<DutyOperatedDay> GetOperatedDaysForDuty(string conKey, int dutyID)
        {
            var result = new List<DutyOperatedDay>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetOperatedDayForDuty", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@DutyID", dutyID));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new DutyOperatedDay();
                    item.ID = Convert.ToInt32(dr["ID"]);
                    item.DutyID = Convert.ToInt32(dr["DutyID"]);
                    item.OperatedDayBitmask = Convert.ToInt32(dr["OperatedDayBitmask"]);
                    item.OperatedDayString = dr["OperatedDayString"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.OrderBy(s => s.OperatedDayString).ToList();
        }

        public IEnumerable<DutyBooking> GetBookingsForOperatedDay(string conKey, int operatedDayID)
        {
            var result = new List<DutyBooking>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetBookingsForOperatedDay", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@OperatedDayID", operatedDayID));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new DutyBooking();
                    item.ID = Convert.ToInt32(dr["ID"]);
                    item.DutyOperatedDayID = Convert.ToInt32(dr["DutyOperatedDayID"]);
                    item.PlaceOffID = Convert.ToInt32(dr["PlaceOffID"]);
                    item.PlaceOnID = Convert.ToInt32(dr["PlaceOnID"]);
                    item.PlaceOff = dr["PlaceOff"].ToString();
                    item.PlaceOn = dr["PlaceOn"].ToString();
                    item.BookOffTimeHour = Convert.ToDateTime(dr["BookOffTime"]).Hour.ToString().PadLeft(2, '0');
                    item.BookOffTimeMinute = Convert.ToDateTime(dr["BookOffTime"]).Minute.ToString().PadLeft(2, '0');
                    item.BookOnTimeHour = Convert.ToDateTime(dr["BookOnTime"]).Hour.ToString().PadLeft(2, '0');
                    item.BookOnTimeMinute = Convert.ToDateTime(dr["BookOnTime"]).Minute.ToString().PadLeft(2, '0');
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.OrderBy(s => s.BookOffTimeHour).ToList();
        }

        public IEnumerable<DutyTrip> GetTripsForBooking(string conKey, int bookingID)
        {
            var result = new List<DutyTrip>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetTripsForBooking", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@BookingID", bookingID));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new DutyTrip();
                    item.ID = Convert.ToInt32(dr["ID"]);
                    item.DutyOperatedDayID = Convert.ToInt32(dr["DutyOperatedDayID"]);
                    item.DutyID = Convert.ToInt32(dr["DutyID"]);
                    item.DutyBookingID = Convert.ToInt32(dr["DutyBookingID"]);
                    item.Description = dr["Description"].ToString();
                    item.JourneyNo = dr["JourneyNo"].ToString();
                    item.MainRouteNumber = dr["MainRouteNumber"].ToString();
                    item.SubRouteNumber = dr["SubRouteNumber"].ToString();
                    item.RouteName = dr["RouteName"].ToString();
                    item.JourneyNo = dr["JourneyNo"].ToString();
                    item.Distance = Convert.ToDouble(dr["Distance"]);
                    item.ContractID = dr["Contract"].ToString();
                    item.DefaultContract = dr["DefaultContract"].ToString();
                    item.Direction = Convert.ToBoolean(dr["Direction"]);
                    item.StartTimeHour = Convert.ToDateTime(dr["StartTime"]).Hour.ToString().PadLeft(2, '0');
                    item.StartTimeMinute = Convert.ToDateTime(dr["StartTime"]).Minute.ToString().PadLeft(2, '0');
                    item.EndTimeHour = Convert.ToDateTime(dr["EndTime"]).Hour.ToString().PadLeft(2, '0');
                    item.EndTimeMinute = Convert.ToDateTime(dr["EndTime"]).Minute.ToString().PadLeft(2, '0');
                    item.IsDeleted = Convert.ToBoolean(dr["IsDeleted"]);
                    item.Cost = Convert.ToInt32(dr["Cost"]);
                    item.Team = Convert.ToInt32(dr["Team"]);
                    item.DestinationBlind = dr["DestinationBlind"].ToString();
                    item.Position = Convert.ToBoolean(dr["Position"]);
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.OrderBy(s => s.JourneyNo).ToList();
        }

        public IEnumerable<DutyEvent> GetEventsForBooking(string conKey, int bookingID)
        {
            var result = new List<DutyEvent>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetEventsForBooking", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@BookingID", bookingID));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new DutyEvent();
                    item.DutyEventID = Convert.ToInt32(dr["DutyEventID"]);
                    item.DutyOperatedDayID = Convert.ToInt32(dr["DutyOperatedDayID"]);
                    item.DutyBookingID = Convert.ToInt32(dr["DutyBookingID"]);
                    item.Description = dr["Description"].ToString();
                    item.StartTimeHour = Convert.ToDateTime(dr["StartTime"]).Hour.ToString().PadLeft(2, '0');
                    item.StartTimeMinute = Convert.ToDateTime(dr["StartTime"]).Minute.ToString().PadLeft(2, '0');
                    item.EndTimeHour = Convert.ToDateTime(dr["EndTime"]).Hour.ToString().PadLeft(2, '0');
                    item.EndTimeMinute = Convert.ToDateTime(dr["EndTime"]).Minute.ToString().PadLeft(2, '0');
                    item.IsDeleted = Convert.ToBoolean(dr["IsDeleted"]);
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.OrderBy(s => s.StartTimeHour).ToList();
        }

        #endregion

        #region Inserts and Updates


        public int InsertOrUpdateDuty(Duty duty, string conKey)
        {
            var Status = 1;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_InsertOrUpdateDuties", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@UIDutyID", duty.ReferenceDutyID));
                cmd.Parameters.Add(new SqlParameter("@Description", duty.Description));
                cmd.Parameters.Add(new SqlParameter("@Location", duty.Location));
                cmd.Parameters.Add(new SqlParameter("@Period", duty.Period));
                cmd.Parameters.Add(new SqlParameter("@DutyID", duty.ID));
                cmd.Parameters.Add(new SqlParameter("@DutyWorkspaceID", duty.WorkSpaceID));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    Status = Convert.ToInt32(dr["Status"]);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                myConnection.Close();
            }
            return Status;
        }

        public bool InsertOrUpdateDutyOperatedDays(DutyOperatedDay dutyOperatedDay, string conKey)
        {
            var result = false;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_InsertOrUpdateOperatedDays", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@DutyOperatedDayID", dutyOperatedDay.ID));
                cmd.Parameters.Add(new SqlParameter("@DutyID", dutyOperatedDay.DutyID));
                cmd.Parameters.Add(new SqlParameter("@OperatedDayBitmask", dutyOperatedDay.OperatedDayBitmask));
                cmd.Parameters.Add(new SqlParameter("@OperatedDayString", dutyOperatedDay.OperatedDayString));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {
                    result = true;
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }

        public int InsertOrUpdateDutyBooking(DutyBooking dutyBooking, string conKey, bool isSlot = false)
        {
            var Status = 1;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var bookOnTime = new DateTime(1900, 1, 1, Convert.ToInt32(dutyBooking.BookOnTimeHour), Convert.ToInt32(dutyBooking.BookOnTimeMinute), 0);
                var bookOffTime = new DateTime(1900, 1, 1, Convert.ToInt32(dutyBooking.BookOffTimeHour), Convert.ToInt32(dutyBooking.BookOffTimeMinute), 0);
                var cmd = new SqlCommand("eBusDuty_InsertOrUpdateDutyBooking", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.Add(new SqlParameter("@BookOnTime", bookOnTime));
                cmd.Parameters.Add(new SqlParameter("@BookOffTime", bookOffTime));
                cmd.Parameters.Add(new SqlParameter("@PlaceOn", dutyBooking.PlaceOn));
                cmd.Parameters.Add(new SqlParameter("@PlaceOff", dutyBooking.PlaceOff));
                cmd.Parameters.Add(new SqlParameter("@PlaceOnID", dutyBooking.PlaceOnID));
                cmd.Parameters.Add(new SqlParameter("@PlaceoffID", dutyBooking.PlaceOffID));
                cmd.Parameters.Add(new SqlParameter("@DutyOperatedDayID", dutyBooking.DutyOperatedDayID));
                cmd.Parameters.Add(new SqlParameter("@DutyBookingID", dutyBooking.ID));
                cmd.Parameters.Add(new SqlParameter("@IsSlot", isSlot));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    Status = Convert.ToInt32(dr["Status"]);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                myConnection.Close();
            }

            return Status;
        }

        public int InsertOrUpdateDutyTrip(DutyTrip dutyTrip, string conKey, bool isSlot = false)
        {
            var Status = 1;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var startTime = new DateTime(1900, 1, 1, Convert.ToInt32(dutyTrip.StartTimeHour), Convert.ToInt32(dutyTrip.StartTimeMinute), 0);
                var endTime = new DateTime(1900, 1, 1, Convert.ToInt32(dutyTrip.EndTimeHour), Convert.ToInt32(dutyTrip.EndTimeMinute), 0);

                var cmd = new SqlCommand("eBusDuty_InsertOrUpdateDutyTrip", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@DutyTripID", dutyTrip.ID));
                cmd.Parameters.Add(new SqlParameter("@DutyBookingsID", dutyTrip.DutyBookingID));
                cmd.Parameters.Add(new SqlParameter("@MainRouteNumber", dutyTrip.MainRouteNumber));
                cmd.Parameters.Add(new SqlParameter("@SubRouteNumber", dutyTrip.SubRouteNumber));
                cmd.Parameters.Add(new SqlParameter("@Contract", dutyTrip.ContractID));
                cmd.Parameters.Add(new SqlParameter("@StartTime", startTime));
                cmd.Parameters.Add(new SqlParameter("@EndTime", endTime));
                cmd.Parameters.Add(new SqlParameter("@JourneyNo", dutyTrip.StartTimeHour + dutyTrip.StartTimeMinute));
                cmd.Parameters.Add(new SqlParameter("@IsSlot", isSlot));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    Status = Convert.ToInt32(dr["Status"]);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                myConnection.Close();
            }

            return Status;
        }

        public int InsertOrUpdateDutyEvent(DutyEvent dutyEvent, string conKey,bool isSlot = false)
        {
            var Status = 1;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var startTime = new DateTime(1900, 1, 1, Convert.ToInt32(dutyEvent.StartTimeHour), Convert.ToInt32(dutyEvent.StartTimeMinute), 0);
                var endTime = new DateTime(1900, 1, 1, Convert.ToInt32(dutyEvent.EndTimeHour), Convert.ToInt32(dutyEvent.EndTimeMinute), 0);

                var cmd = new SqlCommand("eBusDuty_InsertOrUpdateDutyEvent", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@StartTime", startTime));
                cmd.Parameters.Add(new SqlParameter("@EndTime", endTime));
                cmd.Parameters.Add(new SqlParameter("@EventDescription", dutyEvent.Description));
                cmd.Parameters.Add(new SqlParameter("@DutyBookingsID", dutyEvent.DutyBookingID));
                cmd.Parameters.Add(new SqlParameter("@DutyEventsID", dutyEvent.DutyEventID));
                cmd.Parameters.Add(new SqlParameter("@IsSlot", isSlot));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    Status = Convert.ToInt32(dr["Status"]);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                myConnection.Close();
            }

            return Status;
        }
        #endregion

        #region Delete

        public bool DeleteDuty(int dutyID, string conKey)
        {
            var result = false;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_DeleteDuty", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@DutyID", dutyID));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {
                    result = true;
                }
            }
            finally
            {
                myConnection.Close();
            }
            return result;
        }

        public bool DeleteDutyOperatedDay(int dutyOperatedDayID, string conKey)
        {
            var result = false;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_DeleteOperatedDay", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@DutyOperatedDayID", dutyOperatedDayID));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {
                    result = true;
                }
            }
            finally
            {
                myConnection.Close();
            }
            return result;
        }

        public bool DeleteDutyBooking(int dutyBookingID, string conKey)
        {
            var result = false;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_DeleteBooking", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@DutyBookingID", dutyBookingID));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {
                    result = true;
                }
            }
            finally
            {
                myConnection.Close();
            }
            return result;
        }

        public bool DeleteDutyTrip(int dutyTripID, string conKey)
        {
            var result = false;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_DeleteTrip", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@DutyTripID", dutyTripID));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {
                    result = true;
                }
            }
            finally
            {
                myConnection.Close();
            }
            return result;
        }

        public bool DeleteDutyEvent(int dutyEventID, string conKey)
        {
            var result = false;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_DeleteEvent", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@DutyEventID", dutyEventID));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {
                    result = true;
                }
            }
            finally
            {
                myConnection.Close();
            }
            return result;
        }

        #endregion

        #region Validation

        public bool CheckDutyDependency(int dutyID, string conKey)
        {
            var result = false;
            if (GetOperatedDaysForDuty("", dutyID).ToList().Any())
            {
                result = true;
            }
            return result;
        }

        public bool CheckOperatedDayDependency(int dutyOperatedDayID, string conKey)
        {
            var result = false;
            if (GetBookingsForOperatedDay("", dutyOperatedDayID).ToList().Any())
            {
                result = true;
            }
            return result;
        }

        public List<DutyDBF> UpdateDutyScheduleInfo(string conKey)
        {
            var collection = new List<DutyDBF>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_LoadDutyScheduledInformation", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new DutyDBF();

                    item.BOARD = dr["BOARD"].ToString();
                    item.DAYS = CalculateOperatingDayString(dr["DAYS"].ToString());
                    item.DIR = dr["DIR"].ToString() == "true"?"1":"0";
                    item.DUTY = dr["DUTY"].ToString();
                    item.JOURNEY = dr["JOURNEY"].ToString();
                    item.ROUTE = dr["ROUTE"].ToString();
                    item.START = dr["START"].ToString();
                    collection.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return collection;
        }

        public string CalculateOperatingDayString(string OperatingDayBitMap)
        {
            var result = 0;
            var charArray = OperatingDayBitMap.ToCharArray();
            //_MTWTF__
            if (charArray[0] != '_')
            { result = result + 64; }
            if (charArray[1] != '_')
            { result = result + 1; }
            if (charArray[2] != '_')
            { result = result + 2; }
            if (charArray[3] != '_')
            { result = result + 4; }
            if (charArray[4] != '_')
            { result = result + 8; }
            if (charArray[5] != '_')
            { result = result + 16; }
            if (charArray[6] != '_')
            { result = result + 32; }
            if (charArray[7] != '_')
            { result = 127; }

            return result.ToString();
        }

        public bool CheckBookingDependency(int dutyBookingID, string conKey)
        {
            var result = false;
            if (GetTripsForBooking("", dutyBookingID).ToList().Any() || GetEventsForBooking("", dutyBookingID).ToList().Any())
            {
                result = true;
            }
            return result;
        }

        public bool SendEmail(string mailSubject, string mailBody)
        {
            var status = false;
            try
            {

                MailAddress mailfrom = null;
                SmtpClient smtp = null;

                smtp = new SmtpClient(ConfigurationManager.AppSettings["SMTP"], Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]));
                smtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["UserName"], ConfigurationManager.AppSettings["Password"]);
                smtp.EnableSsl = true;
                mailfrom = new MailAddress(ConfigurationManager.AppSettings["FromEmail"]);


                MailAddress mailto = new MailAddress(ConfigurationManager.AppSettings["ToEmail"]);
                MailMessage newmsg = new MailMessage(mailfrom, mailto);

                newmsg.Subject = mailSubject + " " + DateTime.Now.ToShortDateString();
                newmsg.IsBodyHtml = true;
                newmsg.Body = mailBody;

                smtp.Send(newmsg);
                status = false;
            }
            catch (Exception ex)
            {
                throw;
            }
            return status;
        }

        #endregion

        #region Master
        public IEnumerable<SelectListItem> GetRoutes(string conKey, bool isPosition)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "--", Value = "0" });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetRoutes", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@IsPosition", isPosition));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Text = dr["Route"].ToString();
                    item.Value = dr["RouteID"].ToString().Replace(" ", "");
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }
        public IEnumerable<SelectListItem> GetStages(string conKey)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "--", Value = "0" });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetStages", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Text = dr["StageName"].ToString();
                    item.Value = dr["StageNumber"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }
        public IEnumerable<SelectListItem> GetContracts(string conKey)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "--", Value = "0" });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetContracts", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Text = dr["Description"].ToString();
                    item.Value = dr["Contract"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }
        public IEnumerable<SelectListItem> GetWorkSpaces(string conKey)
        {
            var result = new List<SelectListItem>();
            //result.Add(new SelectListItem() { Text = "--", Value = "0" });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetWorkSpaces", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Text = dr["WorkspaceName"].ToString();
                    item.Value = dr["ID"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }
        public IEnumerable<SelectListItem> GetEvents(string conKey)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "--", Value = "0" });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetEvents", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Text = dr["Description"].ToString();
                    item.Value = dr["Description"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }
        public IEnumerable<SelectListItem> GetLocations(string conKey)
        {
            var result = new List<SelectListItem>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));
            result.Add(new SelectListItem() { Text = "All", Value = "0" });
            try
            {
                var cmd = new SqlCommand("eBusDuty_GetLocations", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Text = dr["Name"].ToString();
                    item.Value = dr["ID"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }

        public Duty GetDuty(string conKey, int ID)
        {
            var result = new Duty();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetBookingDuty", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    result.ReferenceDutyID = Convert.ToInt32(dr["DutyID"]);
                    result.Description = dr["Description"].ToString();
                    result.Location = dr["Location"].ToString();
                    result.WorkSpace = Convert.ToString(dr["WorkSpace"]);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }

        public DutyBooking GetBooking(string conKey, int ID)
        {
            var result = new DutyBooking();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetBooking", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    result.DutyID = Convert.ToInt32(dr["DutyID"]);
                    result.DutyOperatedDayID = Convert.ToInt32(dr["OperatingDayID"]);
                    result.DutyBookingID = Convert.ToInt32(dr["BookingID"]);
                    result.BookOnTime = Convert.ToDateTime(dr["BookOnTime"]);
                    result.BookOffTime = Convert.ToDateTime(dr["BookOffTime"]);
                    result.PlaceOff = dr["PlaceOff"].ToString();
                    result.PlaceOn = dr["PlaceOn"].ToString();
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }

        public DutyOperatedDay GetOperatingDays(string conKey, int ID)
        {
            var result = new DutyOperatedDay();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetBookingOperatingDays", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    result.DutyID = Convert.ToInt32(dr["DutyID"]);
                    result.DutyOperatedDayID = Convert.ToInt32(dr["OperatingDayID"]);
                    result.OperatedDayBitmask = Convert.ToInt32(dr["OperatingDayBitMask"]);
                    result.OperatedDayString = dr["OperatingDayString"].ToString();
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }

        public DutyTrip GetBookingTrip(string conKey, int ID)
        {
            var result = new DutyTrip();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetBookingTrip", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    result.DutyID = Convert.ToInt32(dr["DutyID"]);
                    result.DutyOperatedDayID = Convert.ToInt32(dr["OperatingDayID"]);
                    result.DutyBookingID = Convert.ToInt32(dr["BookingID"]);
                    result.ID = Convert.ToInt32(dr["TripID"]);
                    result.MainRouteNumber = dr["MainRouteNumber"].ToString();
                    result.SubRouteNumber = dr["SubRouteNumber"].ToString();
                    result.ContractValue = dr["Contract"].ToString();
                    result.StartTime = Convert.ToDateTime(dr["StartTime"]);
                    result.EndTime = Convert.ToDateTime(dr["EndTime"]);
                    result.Position = Convert.ToBoolean(dr["Position"]);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }

        public DutyEvent GetBookingEvent(string conKey, int ID)
        {
            var result = new DutyEvent();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusDuty_GetBookingEvent", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    result.DutyID = Convert.ToInt32(dr["DutyID"]);
                    result.DutyOperatedDayID = Convert.ToInt32(dr["OperatingDayID"]);
                    result.DutyBookingID = Convert.ToInt32(dr["BookingID"]);
                    result.DutyEventID = Convert.ToInt32(dr["EventID"]);
                    result.StartTime = Convert.ToDateTime(dr["StartTime"]);
                    result.EndTime = Convert.ToDateTime(dr["EndTime"]);
                    result.Description = Convert.ToString(dr["Description"]);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result;
        }
        #endregion

    }

}
