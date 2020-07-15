using Reports.Services.Helpers;
using Reports.Services.Models;
using Reports.Services.Models.Passenger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reports.Services
{
    public class PassengerRegistrationService : BaseServices
    {
        public List<PassengerData> GetPassenger(string connectionKey, string smartCardNumber, string firstName, string status, string idNumber, string cellPhone, string passengerType)
        {
            var result = new List<PassengerData>();
            var myConnection = new SqlConnection(GetConnectionString(connectionKey));

            try
            {
                var cmd = new SqlCommand("eBusPassengerMaster_GetPassengerDetails", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@PassengerType", passengerType == "" ? "" : passengerType));
                cmd.Parameters.Add(new SqlParameter("@SmartCardNumber", smartCardNumber == "" ? "" : smartCardNumber));
                cmd.Parameters.Add(new SqlParameter("@FirstName", firstName == "" ? "" : firstName));
                cmd.Parameters.Add(new SqlParameter("@IDNumber", idNumber == "" ? "" : idNumber));
                cmd.Parameters.Add(new SqlParameter("@CellPhone", cellPhone == "" ? "" : cellPhone));
                cmd.Parameters.Add(new SqlParameter("@Status", status == "" ? "Active" : status));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new PassengerData();
                    item.ID = dr["ID"].ToString();
                    item.PassengerType = dr["PassengerType"].ToString();
                    item.SmartCardNumber = dr["SmartCardNumber"].ToString();
                    item.SmartCardTypeID = Convert.ToInt32(dr["SmartCardTypeID"]);
                    item.Title = dr["Title"].ToString();
                    item.Initials = dr["Initials"].ToString();
                    item.FirstName = dr["FirstName"].ToString();
                    item.Surname = dr["Surname"].ToString();
                    item.IDNumber = dr["IDNumber"].ToString();
                    item.DateOfBirth = dr["DateOfBirth"] == DBNull.Value ? "" : Convert.ToDateTime(dr["DateOfBirth"]).ToShortDateString();
                    item.Email = dr["Email"].ToString();
                    item.CellPhoneNumber = dr["CellPhoneNumber"].ToString();
                    item.AlternativePhoneNumber = dr["AlternativePhoneNumber"].ToString();
                    item.Address = dr["Address"].ToString();
                    item.Status = Convert.ToBoolean(dr["Status"]);
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.ToList();
        }

        public List<SelectListItem> GetPassengerTypes()
        {
            return new List<SelectListItem>() { new SelectListItem() { Text = "--Select--", Value = "0" }, new SelectListItem() { Text = "Cash", Value = "Cash" }, new SelectListItem() { Text = "Smartcard", Value = "Smartcard" } };
        }

        public List<SelectListItem> GetSmartCardType(string connectionKey)
        {
            var result = new List<SelectListItem>();
            var myConnection = new SqlConnection(GetConnectionString(connectionKey));

            try
            {
                var cmd = new SqlCommand("eBusPassengerMaster_GetSmartCardTypes", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Value = dr["ID"].ToString();
                    item.Text = dr["Description"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.ToList();
        }

        public int InsertOrUpdatePassenger(PassengerData passengerData, string conKey)
        {
            var Status = 1;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusPassengerMaster_InsertOrUpdatePassenger", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@ID", passengerData.ID));
                cmd.Parameters.Add(new SqlParameter("@PassengerType", passengerData.PassengerType));
                cmd.Parameters.Add(new SqlParameter("@SmartCardNumber", passengerData.SmartCardNumber));
                cmd.Parameters.Add(new SqlParameter("@SmartCardTypeID", passengerData.SmartCardTypeID));
                cmd.Parameters.Add(new SqlParameter("@Status", passengerData.Status));
                cmd.Parameters.Add(new SqlParameter("@Title", passengerData.Title));
                cmd.Parameters.Add(new SqlParameter("@Initials", passengerData.Initials));
                cmd.Parameters.Add(new SqlParameter("@FirstName", passengerData.FirstName));
                cmd.Parameters.Add(new SqlParameter("@Surname", passengerData.Surname));
                cmd.Parameters.Add(new SqlParameter("@IDNumber", passengerData.IDNumber));
                cmd.Parameters.Add(new SqlParameter("@DateOfBirth", passengerData.DateOfBirth));
                cmd.Parameters.Add(new SqlParameter("@Email", passengerData.Email));
                cmd.Parameters.Add(new SqlParameter("@CellPhoneNumber", passengerData.CellPhoneNumber));
                cmd.Parameters.Add(new SqlParameter("@AlternativePhoneNumber", passengerData.AlternativePhoneNumber));
                cmd.Parameters.Add(new SqlParameter("@Address", passengerData.Address));
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
        public bool SetPassengerStatus(bool status,string passengerID, string conKey)
        {
            var result = false;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusPassengerMaster_SetPassengerStatus", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@Status", status));
                cmd.Parameters.Add(new SqlParameter("@PassengerID", passengerID));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                if (cmd.ExecuteNonQuery().ToString() == "1")
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
    }
}