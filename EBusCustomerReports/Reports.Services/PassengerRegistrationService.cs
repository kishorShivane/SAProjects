using Reports.Services.Models.Passenger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Reports.Services
{
    public class PassengerRegistrationService : BaseServices
    {
        public List<PassengerData> GetPassenger(string connectionKey, string smartCardNumber, string firstName, string status, string idNumber, string cellPhone, string passengerType)
        {
            List<PassengerData> result = new List<PassengerData>();
            SqlConnection myConnection = new SqlConnection(GetConnectionString(connectionKey));

            try
            {
                SqlCommand cmd = new SqlCommand("eBusPassengerMaster_GetPassengerDetails", myConnection)
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
                    PassengerData item = new PassengerData
                    {
                        ID = dr["ID"].ToString(),
                        PassengerType = dr["PassengerType"].ToString(),
                        SmartCardNumber = dr["SmartCardNumber"].ToString(),
                        SmartCardTypeID = Convert.ToInt32(dr["SmartCardTypeID"]),
                        Title = dr["Title"].ToString(),
                        Initials = dr["Initials"].ToString(),
                        FirstName = dr["FirstName"].ToString().ToUpper(),
                        Surname = dr["Surname"].ToString().ToUpper(),
                        IDNumber = dr["IDNumber"].ToString(),
                        DateOfBirth = (dr["DateOfBirth"] == DBNull.Value && dr["DateOfBirth"].ToString() == "") ? "" : Convert.ToDateTime(dr["DateOfBirth"]).ToString("dd-MM-yyyy"),
                        Email = dr["Email"].ToString(),
                        CellPhoneNumber = dr["CellPhoneNumber"].ToString(),
                        AlternativePhoneNumber = dr["AlternativePhoneNumber"].ToString(),
                        Address = dr["Address"].ToString(),
                        Status = Convert.ToBoolean(dr["Status"])
                    };
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
            List<SelectListItem> result = new List<SelectListItem>();
            SqlConnection myConnection = new SqlConnection(GetConnectionString(connectionKey));

            try
            {
                SqlCommand cmd = new SqlCommand("eBusPassengerMaster_GetSmartCardTypes", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    SelectListItem item = new SelectListItem
                    {
                        Value = dr["ID"].ToString(),
                        Text = dr["Description"].ToString()
                    };
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
            int Status = 1;
            SqlConnection myConnection = new SqlConnection(GetConnectionString(conKey));
            string dob = !string.IsNullOrEmpty(passengerData.DateOfBirth) ? passengerData.DateOfBirth.Split('-')[1] + "/" + passengerData.DateOfBirth.Split('-')[0] + '/' + passengerData.DateOfBirth.Split('-')[2] : "";
            try
            {
                SqlCommand cmd = new SqlCommand("eBusPassengerMaster_InsertOrUpdatePassenger", myConnection)
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
                cmd.Parameters.Add(new SqlParameter("@DateOfBirth", dob));
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
            catch (Exception)
            {
                throw;
            }
            finally
            {
                myConnection.Close();
            }

            return Status;
        }
        public bool SetPassengerStatus(bool status, string passengerID, string conKey)
        {
            bool result = false;
            SqlConnection myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                SqlCommand cmd = new SqlCommand("eBusPassengerMaster_SetPassengerStatus", myConnection)
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