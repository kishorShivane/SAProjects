using Reports.Services.Helpers;
using Reports.Services.Models;
using Reports.Services.Models.Passenger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Reports.Services
{
    public class PassengerService : BaseServices
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
                cmd.Parameters.Add(new SqlParameter("@SmartCardSerialNumber", string.IsNullOrEmpty(passengerData.SmartCardNumber) ? "" : ToLittleHex(Convert.ToInt64(passengerData.SmartCardNumber).ToString("X"))));
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

        public DataSet GetPassengerTransDataSet(string connKey, PassengerTransFilter filter, string companyName)
        {
            DataSet ds = new DataSet();
            DataTable table1 = PassengerTransDataset();
            //
            string filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            string amountRechargedClass = "741,742,743,744,745,746";
            string tripsRechargedClass = "711,712,713,715,717,721,722";
            List<PassengerTransData> result = GetPassengerTransData(connKey, filter);

            if (result.Any())
            {

                foreach (PassengerTransData item in result)
                {
                    string serialNumber = GetSerialNumberForSmartCard(item.SerialNumber);

                    item.CardIdFilter = "Smart Card : " + (string.IsNullOrEmpty(filter.SmartCardNumber)? "": filter.SmartCardNumber + " (" + item.SerialNumber + ")");
                    item.DateRangeFilter = filterDateRange;
                    item.FirstNameFilter = "Firstname : " + filter.FirstName;
                    item.SurnameFilter = "Surname : " + filter.SurName;
                    item.IDNumberFilter = "ID Number : " + filter.IDNumber;
                    item.CellNumberFilter = "CellPhoneNumber : " + filter.CellPhoneNumber;
                    item.DutyFilter = "Duty : " + filter.DutyNumber;
                    item.BusFilter = "Bus : " + filter.BusNumber;
                    table1.Rows.Add(
                                serialNumber,
                                item.SerialNumberHex,
                                item.FirstName,
                                item.Surname,
                                item.IDNumber,
                                item.CellPhoneNumber,
                                item.ClassName,
                                item.Duty,
                                item.Route,
                                item.DriverID + "-" + item.DriverName,
                                item.Bus,
                                item.TDate,
                                item.TTime,
                                item.Revenue,
                                item.NonRevenue,
                                (item.Duty == "8000" ? (amountRechargedClass.Contains(item.ClassID.ToString().Trim()) ? "R " + (Convert.ToDouble(item.AmountRecharged) / 100).ToString() : tripsRechargedClass.Contains(item.ClassID.ToString().Trim()) ? item.TripsRecharged : "0") : "0"),
                                item.RevenueBalance,
                                item.TripBalance,
                                string.IsNullOrEmpty(item.SmartCardExipry) ? "NILL" : item.SmartCardExipry,
                                item.DateRangeFilter,
                                item.CardIdFilter,
                                item.FirstNameFilter,
                                item.SurnameFilter,
                                item.IDNumberFilter,
                                item.CellNumberFilter,
                                item.DutyFilter,
                                item.BusFilter,
                                companyName
                                );
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["CardIdFilter"] = "Smart Card : " + (string.IsNullOrEmpty(filter.SmartCardNumber) ? "" : filter.SmartCardNumber + " (" + ToLittleHex(Convert.ToInt64(filter.SmartCardNumber).ToString("X")) + ")");
                dr["DateRangeFilter"] = filterDateRange;
                dr["FirstNameFilter"] = "Firstname : " + filter.FirstName;
                dr["SurnameFilter"] = "Surname : " + filter.SurName;
                dr["IDNumberFilter"] = "ID Number : " + filter.IDNumber;
                dr["CellNumberFilter"] = "CellPhoneNumber : " + filter.CellPhoneNumber;
                dr["DutyFilter"] = "Duty : " + filter.DutyNumber;
                dr["BusFilter"] = "Bus : " + filter.BusNumber;
                dr["CompanyName"] = companyName;
                table1.Rows.Add(dr);
            }

            ds.Tables.Add(table1);
            return ds;
        }

        private string GetSerialNumberForSmartCard(string smartCardNumber)
        {
            string serialNumber = "";
            if (!string.IsNullOrEmpty(smartCardNumber))
            {
                string littleIndian = LittleEndian(smartCardNumber);
                serialNumber = Convert.ToInt64(littleIndian, 16).ToString();
            }
            return serialNumber;
        }

        public static string ToLittleHex(string userInput)
        {
            if (userInput.Length == 6)
            {
                userInput = "00" + userInput;
            }
            if (userInput.Length == 7)
            {
                userInput = "0" + userInput;
            }
            if (userInput.Length == 5)
            {
                userInput = "000" + userInput;
            }
            if (userInput.Length == 4)
            {
                userInput = "0000" + userInput;
            }
            if (userInput.Length == 2)
            {
                userInput = "0000000" + userInput;
            }
            int len = userInput.Length;
            if (len < 8) { return userInput; }
            return userInput.Substring(len - 2, 2) + userInput.Substring(len - 4, 2) + userInput.Substring(len - 6, 2) + userInput.Substring(len - 8, 2);
        }

        private string LittleEndian(string num)
        {
            List<char> chars = num.Reverse().ToList();
            string littleIndian = "";
            for (int i = 0; i < 8; i += 2)
            {
                littleIndian = littleIndian + chars[i + 1].ToString() + chars[i].ToString();
            }
            return littleIndian;
        }

        private List<PassengerTransData> GetPassengerTransData(string connKey, PassengerTransFilter filter)
        {
            List<PassengerTransData> result = new List<PassengerTransData>();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand("EbusPassengerTrans", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@smartCardNumBi", string.IsNullOrEmpty(filter.SmartCardNumber)? "": Convert.ToInt64(filter.SmartCardNumber).ToString("X"));
                cmd.Parameters.AddWithValue("@smartCardNumLi", string.IsNullOrEmpty(filter.SmartCardNumber) ? "" : ToLittleHex(Convert.ToInt64(filter.SmartCardNumber).ToString("X")));
                cmd.Parameters.AddWithValue("@fromDate", string.IsNullOrEmpty(filter.StartDate) ? "" : CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", string.IsNullOrEmpty(filter.EndDate) ? "" : CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@firstName", string.IsNullOrEmpty(filter.FirstName) ? "" : filter.FirstName);
                cmd.Parameters.AddWithValue("@surname", string.IsNullOrEmpty(filter.SurName) ? "" : filter.SurName);
                cmd.Parameters.AddWithValue("@idNumber", string.IsNullOrEmpty(filter.IDNumber) ? "" : filter.IDNumber);
                cmd.Parameters.AddWithValue("@cellNumber", string.IsNullOrEmpty(filter.CellPhoneNumber) ? "" : filter.CellPhoneNumber);
                cmd.Parameters.AddWithValue("@dutyID", string.IsNullOrEmpty(filter.DutyNumber) ? "" : filter.DutyNumber);
                cmd.Parameters.AddWithValue("@busID", string.IsNullOrEmpty(filter.BusNumber) ? "" : filter.BusNumber);
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    PassengerTransData sch = new PassengerTransData();

                    if (dr["SerialNumber"] != null && dr["SerialNumber"].ToString() != string.Empty)
                    {
                        sch.SerialNumber = dr["SerialNumber"].ToString();
                    }

                    if (dr["SerialNumberHex"] != null && dr["SerialNumberHex"].ToString() != string.Empty)
                    {
                        sch.SerialNumberHex = dr["SerialNumberHex"].ToString();
                    }

                    if (dr["FirstName"] != null && dr["FirstName"].ToString() != string.Empty)
                    {
                        sch.FirstName = dr["FirstName"].ToString();
                    }

                    if (dr["Surname"] != null && dr["Surname"].ToString() != string.Empty)
                    {
                        sch.Surname = dr["Surname"].ToString();
                    }

                    if (dr["IDNumber"] != null && dr["IDNumber"].ToString() != string.Empty)
                    {
                        sch.IDNumber = dr["IDNumber"].ToString();
                    }

                    if (dr["CellPhoneNumber"] != null && dr["CellPhoneNumber"].ToString() != string.Empty)
                    {
                        sch.CellPhoneNumber = dr["CellPhoneNumber"].ToString();
                    }

                    if (dr["ClassID"] != null && dr["ClassID"].ToString() != string.Empty)
                    {
                        sch.ClassID = Convert.ToInt32(dr["ClassID"].ToString());
                    }

                    if (dr["ClassName"] != null && dr["ClassName"].ToString() != string.Empty)
                    {
                        sch.ClassName = dr["ClassName"].ToString();
                    }

                    if (dr["Duty"] != null && dr["Duty"].ToString() != string.Empty)
                    {
                        sch.Duty = dr["Duty"].ToString();
                    }

                    if (dr["Route"] != null && dr["Route"].ToString() != string.Empty)
                    {
                        sch.Route = dr["Route"].ToString();
                    }

                    if (dr["DriverID"] != null && dr["DriverID"].ToString() != string.Empty)
                    {
                        sch.DriverID = dr["DriverID"].ToString();
                    }

                    if (dr["DriverName"] != null && dr["DriverName"].ToString() != string.Empty)
                    {
                        sch.DriverName = dr["DriverName"].ToString();
                    }

                    if (dr["Bus"] != null && dr["Bus"].ToString() != string.Empty)
                    {
                        sch.Bus = dr["Bus"].ToString();
                    }

                    if (dr["TransTime"] != null && dr["TransTime"].ToString() != string.Empty)
                    {
                        sch.TransDate = DateTime.Parse(dr["TransTime"].ToString());
                        sch.TDate = sch.TransDate.ToString("dd/MM/yyyy");
                        sch.TTime = sch.TransDate.ToString("hh:mm tt");
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = dr["Revenue"].ToString();
                    }

                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = dr["NonRevenue"].ToString();
                    }

                    if (dr["RevenueBalance"] != null && dr["RevenueBalance"].ToString() != string.Empty)
                    {
                        sch.RevenueBalance = dr["RevenueBalance"].ToString();
                    }

                    if (dr["TripBalance"] != null && dr["TripBalance"].ToString() != string.Empty)
                    {
                        sch.TripBalance = dr["TripBalance"].ToString();
                    }

                    if (dr["AmountRecharged"] != null && dr["AmountRecharged"].ToString() != string.Empty)
                    {
                        sch.AmountRecharged = dr["AmountRecharged"].ToString();
                    }

                    if (dr["TripsRecharged"] != null && dr["TripsRecharged"].ToString() != string.Empty)
                    {
                        sch.TripsRecharged = dr["TripsRecharged"].ToString();
                    }

                    if (dr["SmartCardExipry"] != null && dr["SmartCardExipry"].ToString() != string.Empty)
                    {
                        sch.SmartCardExipry = DateTime.Parse(dr["SmartCardExipry"].ToString()).ToString("dd/MM/yyyy");
                    }
                    result.Add(sch);
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