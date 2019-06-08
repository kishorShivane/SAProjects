using Reports.Services.Helpers;
using Reports.Services.Models;
using Reports.Services.Models.SmartCard;
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
    public class SmartCardMasterService : BaseServices
    {
        public List<SmartCardData> GetSmartCard(string connectionKey, string smartCardNumber, string firstName, string status, string idNumber, string cellPhone)
        {
            var result = new List<SmartCardData>();
            var myConnection = new SqlConnection(GetConnectionString(connectionKey));

            try
            {
                var cmd = new SqlCommand("eBusSmartCardMaster_GetSmartCardDetails", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
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
                    var item = new SmartCardData();
                    item.ID = dr["ID"].ToString();
                    item.SmartCardNumber = dr["SmartCardNumber"].ToString();
                    item.SmartCardTypeID = Convert.ToInt32(dr["SmartCardTypeID"]);
                    item.Title = dr["Title"].ToString();
                    item.Initials = dr["Initials"].ToString();
                    item.FirstName = dr["FirstName"].ToString();
                    item.Surname = dr["Surname"].ToString();
                    item.IDNumber = dr["IDNumber"].ToString();
                    item.DateOfBirth = Convert.ToDateTime(dr["DateOfBirth"]).ToShortDateString();
                    item.Email = dr["Email"].ToString();
                    item.CellPhoneNumber = dr["CellPhoneNumber"].ToString();
                    item.Address = dr["Address"].ToString();
                    item.SmartCardStatus = Convert.ToBoolean(dr["Status"]);
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.ToList();
        }

        public List<SelectListItem> GetSmartCardType(string connectionKey)
        {
            var result = new List<SelectListItem>();
            var myConnection = new SqlConnection(GetConnectionString(connectionKey));

            try
            {
                var cmd = new SqlCommand("eBusSmartCardMaster_GetSmartCardTypes", myConnection)
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

        public int InsertOrUpdateSmartCard(SmartCardData smartCardData, string conKey)
        {
            var Status = 1;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusSmartCardMaster_InsertOrUpdateSmartCard", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@ID", smartCardData.ID));
                cmd.Parameters.Add(new SqlParameter("@SmartCardNumber", smartCardData.SmartCardNumber));
                cmd.Parameters.Add(new SqlParameter("@SmartCardTypeID", smartCardData.SmartCardTypeID));
                cmd.Parameters.Add(new SqlParameter("@Status", smartCardData.SmartCardStatus));
                cmd.Parameters.Add(new SqlParameter("@Title", smartCardData.Title));
                cmd.Parameters.Add(new SqlParameter("@Initials", smartCardData.Initials));
                cmd.Parameters.Add(new SqlParameter("@FirstName", smartCardData.FirstName));
                cmd.Parameters.Add(new SqlParameter("@Surname", smartCardData.Surname));
                cmd.Parameters.Add(new SqlParameter("@IDNumber", smartCardData.IDNumber));
                cmd.Parameters.Add(new SqlParameter("@DateOfBirth", smartCardData.DateOfBirth));
                cmd.Parameters.Add(new SqlParameter("@Email", smartCardData.Email));
                cmd.Parameters.Add(new SqlParameter("@CellPhoneNumber", smartCardData.CellPhoneNumber));
                cmd.Parameters.Add(new SqlParameter("@Address", smartCardData.Address));
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
        public bool SetSmartCardStatus(bool status,string smartCardID, string conKey)
        {
            var result = false;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusSmartCardMaster_SetSmartCardStatus", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@Status", status));
                cmd.Parameters.Add(new SqlParameter("@SmartCardID", smartCardID));

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