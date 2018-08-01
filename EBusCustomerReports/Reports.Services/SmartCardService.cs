using Reports.Services.Helpers;
using Reports.Services.Models;
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
    public class SmartCardService : BaseServices
    {
        public DataSet GetSmartCardTransDataSet(string connKey, SmartCardTransFilter filter, string companyName)
        {
            var ds = new DataSet();
            var table1 = SmartCardDataset();
            var filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var amountRechargedClass = "741,742,743,744,745,746";
            var tripsRechargedClass = "711,712,713,715,717,721,722";
            var result = GetSmartCardTransData(connKey, filter);

            if (result.Any())
            {
                foreach (var item in result)
                {
                    item.CardIdFilter = "Smart Card : " + filter.SmartCardNumber + " (" + item.SerialNumber + ")";
                    item.DateRangeFilter = filterDateRange;

                    table1.Rows.Add(
                                item.ClassID,
                                item.ClassName,
                                item.NonRevenue,
                                item.Revenue,
                                item.TransDate,
                                item.SerialNumber,
                                item.SerialNumberHex,
                                item.RouteID,
                                item.JourneyID,
                                item.OperatorID + "-" + item.OperatorName,
                                item.ETMID,
                                item.BusID,
                                item.DutyID,
                                item.ModuleID,
                                item.RevenueBalance,
                                item.TripBalance,
                                item.TransDay,
                                item.TDate,
                                item.TTime,
                                item.DateRangeFilter,
                                item.CardIdFilter,
                                companyName,
                                (item.DutyID == "8000" ? (amountRechargedClass.Contains(item.ClassID.ToString().Trim()) ? "R " + (Convert.ToDouble(item.AmountRecharged) / 100).ToString() : tripsRechargedClass.Contains(item.ClassID.ToString().Trim()) ? item.TripsRecharged : "0") : "0"),
                                string.IsNullOrEmpty(item.SmartCardExipry) ? "NILL" : item.SmartCardExipry
                        );
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["DateRangeFilter"] = filterDateRange;
                dr["CardIdFilter"] = "Smart Card : " + filter.SmartCardNumber;
                table1.Rows.Add(dr);
            }

            ds.Tables.Add(table1);
            return ds;
        }

        private List<SmartCardTransData> GetSmartCardTransData(string connKey, SmartCardTransFilter filter)
        {
            var result = new List<SmartCardTransData>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            Int64 val = 0;

            Int64.TryParse(filter.SmartCardNumber, out val);


            if (val == 0)
            {
                return new List<SmartCardTransData>();
            }

            try
            {
                var cmd = new SqlCommand("EbusSmartCardTrans", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@smartCardNumBi", Convert.ToInt64(filter.SmartCardNumber).ToString("X"));
                cmd.Parameters.AddWithValue("@smartCardNumLi", ToLittleHex(Convert.ToInt64(filter.SmartCardNumber).ToString("X")));
                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new SmartCardTransData();

                    if (dr["ClassID"] != null && dr["ClassID"].ToString() != string.Empty)
                    {
                        sch.ClassID = Convert.ToInt32(dr["ClassID"].ToString());
                    }

                    if (dr["ClassName"] != null && dr["ClassName"].ToString() != string.Empty)
                    {
                        sch.ClassName = dr["ClassName"].ToString();
                    }

                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = dr["NonRevenue"].ToString();
                    }
                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = dr["Revenue"].ToString();
                    }
                    if (dr["TransTime"] != null && dr["TransTime"].ToString() != string.Empty)
                    {
                        sch.TransDate = DateTime.Parse(dr["TransTime"].ToString());
                        sch.TDate = sch.TransDate.ToString("dd/MM/yyyy");
                        sch.TTime = sch.TransDate.ToString("hh:mm tt");
                    }
                    if (dr["SerialNumber"] != null && dr["SerialNumber"].ToString() != string.Empty)
                    {
                        sch.SerialNumber = dr["SerialNumber"].ToString();
                    }
                    if (dr["SerialNumberHex"] != null && dr["SerialNumberHex"].ToString() != string.Empty)
                    {
                        sch.SerialNumberHex = dr["SerialNumberHex"].ToString();
                    }

                    if (dr["RouteID"] != null && dr["RouteID"].ToString() != string.Empty)
                    {
                        sch.RouteID = dr["RouteID"].ToString();
                    }
                    if (dr["JourneyID"] != null && dr["JourneyID"].ToString() != string.Empty)
                    {
                        sch.JourneyID = dr["JourneyID"].ToString();
                    }
                    if (dr["OperatorID"] != null && dr["OperatorID"].ToString() != string.Empty)
                    {
                        sch.OperatorID = dr["OperatorID"].ToString();
                    }
                    if (dr["OperatorName"] != null && dr["OperatorName"].ToString() != string.Empty)
                    {
                        sch.OperatorName = dr["OperatorName"].ToString();
                    }
                    if (dr["ETMID"] != null && dr["ETMID"].ToString() != string.Empty)
                    {
                        sch.ETMID = dr["ETMID"].ToString();
                    }
                    if (dr["BusID"] != null && dr["BusID"].ToString() != string.Empty)
                    {
                        sch.BusID = dr["BusID"].ToString();
                    }
                    if (dr["DutyID"] != null && dr["DutyID"].ToString() != string.Empty)
                    {
                        sch.DutyID = dr["DutyID"].ToString();
                    }

                    if (dr["ModuleID"] != null && dr["ModuleID"].ToString() != string.Empty)
                    {
                        sch.ModuleID = dr["ModuleID"].ToString();
                    }
                    if (dr["RevenueBalance"] != null && dr["RevenueBalance"].ToString() != string.Empty)
                    {
                        sch.RevenueBalance = dr["RevenueBalance"].ToString();
                    }
                    if (dr["TripBalance"] != null && dr["TripBalance"].ToString() != string.Empty)
                    {
                        sch.TripBalance = dr["TripBalance"].ToString();
                    }
                    if (dr["TransDay"] != null && dr["TransDay"].ToString() != string.Empty)
                    {
                        sch.TransDay = dr["TransDay"].ToString();
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
            var len = userInput.Length;
            if (len < 8) { return userInput; }
            return userInput.Substring(len - 2, 2) + userInput.Substring(len - 4, 2) + userInput.Substring(len - 6, 2) + userInput.Substring(len - 8, 2);
        }

        public SmartCardHotList SaveSmartCardHolistingRequest(string connKey, SmartCardHotList model)
        {
            var result = IsDuplicateRequest(model.SmartCardNubmer.Trim(), connKey);
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            if (result.IsDuplicate == true)
            {
                return result;
            }

            try
            {
                var cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                if (string.IsNullOrEmpty(model.Comments))
                {
                    model.Comments = "";
                }
                cmd.CommandText = string.Format("insert into  SmartCardHotlisting (SmartCardID,Reason,Comments,CreatedBy,CreatedDateTime) values ('{0}','{1}','{2}','{3}','{4}') "
                      , model.SmartCardNubmer, model.ReasonSelected, model.Comments, model.CreatedBy, model.CreatedDate);

                cmd.CommandTimeout = 500000;

                myConnection.Open();

                cmd.ExecuteNonQuery();
            }

            finally
            {
                myConnection.Close();
            }

            return result;
        }

        public SmartCardHotList IsDuplicateRequest(string cardNum, string connKey)
        {
            var myConnection = new SqlConnection(GetConnectionString(connKey));
            var result = new SmartCardHotList();
            result.IsDuplicate = false;

            try
            {
                var cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "Select top 1 * from SmartCardHotlisting where SmartCardID = '" + cardNum + "'";

                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {

                    result.IsDuplicate = true;

                    if (dr["SmartCardID"] != null && dr["SmartCardID"].ToString() != string.Empty)
                    {
                        result.SmartCardNubmer = dr["SmartCardID"].ToString();
                    }

                    if (dr["CreatedBy"] != null && dr["CreatedBy"].ToString() != string.Empty)
                    {
                        result.CreatedBy = dr["CreatedBy"].ToString();
                    }

                    if (dr["CreatedDateTime"] != null && dr["CreatedDateTime"].ToString() != string.Empty)
                    {
                        result.CreatedDate = (DateTime)dr["CreatedDateTime"];
                    }
                }
            }

            finally
            {
                myConnection.Close();
            }

            return result;
        }

        public DataSet GetSmartCardUsageData(string connKey, string companyName, int usedCount, string startDate, string endDate)
        {
            var result = new DataSet();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));
            try
            {
                myConnection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = myConnection;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "EbusSmartCardUsageData";

                    cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(startDate));
                    cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(endDate));
                    cmd.Parameters.AddWithValue("@UsedCount", usedCount);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(result);
                    }
                }

                var newColumn = new DataColumn("CompanyName", typeof(string));
                newColumn.DefaultValue = companyName;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("DateSelected", typeof(string));
                newColumn.DefaultValue = "Date Range : " + startDate + " to " + endDate;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("UsedCount", typeof(string));
                newColumn.DefaultValue = "No Of Time Used : " + usedCount;
                result.Tables[0].Columns.Add(newColumn);

                if (result.Tables[0] != null && result.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < result.Tables[0].Rows.Count; i++)
                    {
                        var hexVal = result.Tables[0].Rows[i]["str_SerialNumber"].ToString();

                        if (!string.IsNullOrEmpty(hexVal))
                        {
                            if (hexVal.Length < 8)
                            {
                                hexVal = hexVal.PadLeft(8, '0');
                            }

                            var littleIndian = LittleEndian(hexVal);
                            var decValue = Convert.ToInt64(littleIndian, 16);
                            //result.Tables[0].Rows[i]["str_SerialNumber"] = Math.Abs(int.Parse(hexVal.ToString(), System.Globalization.NumberStyles.HexNumber));
                            //result.Tables[0].Rows[i]["str_SerialNumber"] = Convert.ToInt32(hexVal, 16);
                            result.Tables[0].Rows[i]["str_SerialNumber"] = decValue;
                        }
                    }
                }
                else
                {
                    result = MasterHelper.FillDefaultValuesForEmptyDataSet(result);
                }
            }
            finally
            {
                myConnection.Close();
            }
            return result;
        }

        private string LittleEndian(string num)
        {
            var chars = num.Reverse().ToList();
            var littleIndian = "";
            for (int i = 0; i < 8; i += 2)
            {
                littleIndian = littleIndian + chars[i + 1].ToString() + chars[i].ToString();
            }
            return littleIndian;
        }

        public List<SelectListItem> GetAllHotlistReasons(string connKey)
        {
            var res = new List<SelectListItem>();
            res.Add(new SelectListItem { Text = "Select", Selected = true, Value = "" });

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "select * from EbusCardHoltlistReasons";
                cmd.CommandTimeout = 500000;
                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();

                    if (dr["ID"] != null && dr["ID"].ToString() != string.Empty)
                    {
                        obj.Value = dr["ID"].ToString().Trim();
                    }

                    if (dr["ReasonName"] != null && dr["ReasonName"].ToString() != string.Empty)
                    {
                        obj.Text = dr["ReasonName"].ToString().Trim();
                    }
                    res.Add(obj);
                }
            }

            finally
            {
                myConnection.Close();
            }

            return res;
        }

        public List<SelectListItem> GetAllDriverRoutes(string connKey)
        {
            var res = new List<SelectListItem>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "select distinct str4_RouteNumber,str50_RouteName from qdMainRoutes";
                cmd.CommandTimeout = 500000;
                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();

                    if (dr["str4_RouteNumber"] != null && dr["str4_RouteNumber"].ToString() != string.Empty)
                    {
                        obj.Value = dr["str4_RouteNumber"].ToString().Trim();
                    }

                    if (dr["str50_RouteName"] != null && dr["str50_RouteName"].ToString() != string.Empty)
                    {
                        obj.Text = dr["str4_RouteNumber"].ToString().Trim() + " - " + dr["str50_RouteName"].ToString().Trim();
                    }
                    res.Add(obj);
                }
            }

            finally
            {
                myConnection.Close();
            }

            return res.OrderBy(s => s.Value).ToList();
        }

        public DataSet GetCashVsSmartCardUsageByRouteDataSet(string connKey, string RoutesSelected, string StartDate, string EndDate, string companyName)
        {
            var ds = new DataSet();
            var table1 = GetCashVsSmartCardUsageByRouteDataTable();
            var filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", StartDate, EndDate);
            var filterRoutes = string.Format("{0} :  {1} ", "Routes", RoutesSelected);

            var data = GetCashVsSmartCardUsageByRouteData(connKey, RoutesSelected, StartDate, EndDate);

            foreach (var item in data)
            {

                table1.Rows.Add(
                            item.str_RouteID,
                            item.str50_RouteName,
                            item.ColorCodeTickets,
                            item.ColorCodePasses,
                            item.int4_JourneyTickets,
                            item.int4_JourneyTicketsPercent,
                            item.int4_JourneyPasses,
                            item.int4_JourneyPassesPercent,
                            item.int4_JourneyTransfer,
                            item.int4_JourneyTransferPercent,
                            item.TotalPassengers,
                            filterDateRange,
                            companyName,
                            filterRoutes
                    );
            }
            ds.Tables.Add(table1);
            return ds;
        }

        public List<CashVsSmartCardUsageByRouteData> GetCashVsSmartCardUsageByRouteData(string connKey, string RoutesSelected, string StartDate, string EndDate)
        {
            var result = new List<CashVsSmartCardUsageByRouteData>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("GetCashVsSmartCardUsageByRouteData", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Routes", RoutesSelected == null ? "" : RoutesSelected);
                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new CashVsSmartCardUsageByRouteData();

                    if (dr["int4_JourneyPasses"] != null && dr["int4_JourneyPasses"].ToString() != string.Empty)
                    {
                        sch.int4_JourneyPasses = Convert.ToInt32(dr["int4_JourneyPasses"].ToString());
                    }
                    if (dr["int4_JourneyTickets"] != null && dr["int4_JourneyTickets"].ToString() != string.Empty)
                    {
                        sch.int4_JourneyTickets = Convert.ToInt32(dr["int4_JourneyTickets"].ToString());
                    }
                    if (dr["int4_JourneyTransfer"] != null && dr["int4_JourneyTransfer"].ToString() != string.Empty)
                    {
                        sch.int4_JourneyTransfer = Convert.ToInt32(dr["int4_JourneyTransfer"].ToString());
                    }


                    sch.TotalPassengers = sch.int4_JourneyPasses + sch.int4_JourneyTickets + sch.int4_JourneyTransfer;

                    if (sch.TotalPassengers > 0)
                    {
                        sch.int4_JourneyPassesPercent = Math.Round((sch.int4_JourneyPasses / sch.TotalPassengers) * 100);
                        sch.int4_JourneyTicketsPercent = Math.Round((sch.int4_JourneyTickets / sch.TotalPassengers) * 100);
                        sch.int4_JourneyTransferPercent = Math.Round((sch.int4_JourneyTransfer / sch.TotalPassengers) * 100);
                    }

                    if (sch.int4_JourneyPassesPercent > 50)
                    {
                        sch.ColorCodePasses = "green";
                    }

                    if (sch.int4_JourneyTicketsPercent > 50)
                    {
                        sch.ColorCodeTickets = "red";
                    }

                    if (dr["str_RouteID"] != null && dr["str_RouteID"].ToString() != string.Empty)
                    {
                        sch.str_RouteID = dr["str_RouteID"].ToString();
                    }
                    if (dr["str50_RouteName"] != null && dr["str50_RouteName"].ToString() != string.Empty)
                    {
                        sch.str50_RouteName = dr["str50_RouteName"].ToString();
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

        public DataSet GetCashierSummaryReportDataset(string connKey, string companyName, CashierReportSummaryFilter filter)
        {
            var result = new DataSet();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));
            try
            {
                myConnection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = myConnection;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "EbusCashierSummaryReport";

                    cmd.Parameters.AddWithValue("@StartDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate));
                    cmd.Parameters.AddWithValue("@EndDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate));
                    cmd.Parameters.AddWithValue("@StaffNumber", filter.CashiersSelected != null ? string.Join(",", filter.CashiersSelected) : "");

                    cmd.Parameters.AddWithValue("@LocationIDs", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "");
                    cmd.Parameters.AddWithValue("@Terminals", filter.TerminalsSelected != null ? string.Join(",", filter.TerminalsSelected) : "");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(result);
                    }
                }

                var newColumn = new DataColumn("CompanyName", typeof(string));
                newColumn.DefaultValue = companyName;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("DateSelected", typeof(string));
                newColumn.DefaultValue = "Date Range : " + filter.StartDate + " to " + filter.EndDate;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("Cashiers", typeof(string));
                newColumn.DefaultValue = string.Format("Cashiers : {0} ", filter.CashiersSelected != null ? string.Join(",", filter.CashiersSelected) : "All");
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("Locations", typeof(string));
                newColumn.DefaultValue = string.Format("Locations : {0} ", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "All");
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("Terminals", typeof(string));
                newColumn.DefaultValue = string.Format("Terminals : {0} ", filter.TerminalsSelected != null ? string.Join(",", filter.TerminalsSelected) : "All");
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("Difference", typeof(decimal));
                result.Tables[0].Columns.Add(newColumn);

                result = MasterHelper.FillDefaultValuesForEmptyDataSet(result);

                if (result.Tables[0] != null && result.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < result.Tables[0].Rows.Count; i++)
                    {
                        decimal vall = 0;
                        var ress = result.Tables[0].Rows[i]["Overs"];

                        if (!string.IsNullOrEmpty(ress.ToString()) && Convert.ToDecimal(ress) != 0)
                        {
                            vall = Convert.ToDecimal(ress);
                        }

                        ress = result.Tables[0].Rows[i]["Shorts"];

                        if (!string.IsNullOrEmpty(ress.ToString()) && Convert.ToDecimal(ress) != 0)
                        {
                            vall = Convert.ToDecimal(ress);
                        }

                        result.Tables[0].Rows[i]["Difference"] = vall;
                    }
                }
                else
                {
                    result = MasterHelper.FillDefaultValuesForEmptyDataSet(result);
                }
            }
            finally
            {
                myConnection.Close();
            }
            return result;
        }

        public DataSet GetCashierReconciliationReportDataset(string connKey, string company, CashierReportSummaryFilter filter)
        {
            var result = new List<CashierReconciliationData>();
            var ds = new DataSet();
            var filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var staffs = string.Format("Staff Selected : {0} ", filter.StaffSelected != null ? string.Join(",", filter.StaffSelected) : "All");
            var locationsSelected = string.Format("Location Selected : {0} ", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "All");
            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = myConnection;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "EbusCashierReconcillationSummaryReport";

                    cmd.Parameters.AddWithValue("@StartDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate));
                    cmd.Parameters.AddWithValue("@EndDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate));
                    cmd.Parameters.AddWithValue("@StaffNumber", filter.StaffSelected != null ? string.Join(",", filter.StaffSelected) : "");
                    cmd.Parameters.AddWithValue("@LocationIDs", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "");

                    myConnection.Open();
                    SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    while (dr.Read())
                    {
                        var sch = new CashierReconciliationData();
                        //StaffNumber	Date	Time	Revenue	str50_StaffName dateFilter
                        if (dr["StaffNumber"] != null && dr["StaffNumber"].ToString() != string.Empty)
                        {
                            sch.StaffNumber = (dr["StaffNumber"].ToString());
                        }

                        if (dr["StaffName"] != null && dr["StaffName"].ToString() != string.Empty)
                        {
                            sch.StaffName = (dr["StaffName"].ToString());
                        }

                        if (dr["Cashier"] != null && dr["Cashier"].ToString() != string.Empty)
                        {
                            sch.Cashier = (dr["Cashier"].ToString());
                        }

                        if (dr["DutyRevenue"] != null && dr["DutyRevenue"].ToString() != string.Empty)
                        {
                            sch.DutyRevenue = (dr["DutyRevenue"].ToString());
                        }
                        if (dr["Difference"] != null && dr["Difference"].ToString() != string.Empty)
                        {
                            sch.Difference = (dr["Difference"].ToString());
                        }
                        if (dr["TransactionDatetime"] != null && dr["TransactionDatetime"].ToString() != string.Empty)
                        {
                            sch.TransactionDatetime = (Convert.ToDateTime(dr["TransactionDatetime"]).ToString("dd/MM/yyyy"));
                        }
                        if (dr["Location"] != null && dr["Location"].ToString() != string.Empty)
                        {
                            sch.Location = (dr["Location"].ToString());
                        }
                        if (dr["CashierReason"] != null && dr["CashierReason"].ToString() != string.Empty)
                        {
                            sch.CashierReason = (dr["CashierReason"].ToString());
                        }
                        result.Add(sch);
                    }
                }

                var table1 = CashierReconciliationSummay();
                if (result.Any())
                {
                    foreach (var item in result)
                    {
                        table1.Rows.Add(
                            item.StaffNumber,
                            item.StaffName,
                            item.Cashier,
                            item.DutyRevenue,
                            item.Difference,
                            item.TransactionDatetime,
                            staffs,
                            filterDateRange,
                            locationsSelected,
                            item.Location,
                            item.CashierReason == "0" ? "" : item.CashierReason,
                            company
                       );
                    }
                }
                else
                {
                    DataRow dr = table1.NewRow();
                    dr["StaffSelected"] = staffs;
                    dr["DateFilterSelected"] = filterDateRange;
                    dr["LocationSelected"] = locationsSelected;

                    table1.Rows.Add(dr);
                }


                ds.Tables.Add(table1);
            }
            finally
            {
                myConnection.Close();
            }
            return ds;
        }

        public DataSet GetDailyAuditByCashierTerminalDataset(string connKey, CashierReportSummaryFilter filter, string companyName)
        {
            var result = GetDailyAuditByCashierTerminalData(connKey, filter);

            //filter details
            var filterDateRange = string.Format("{0}: {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var casherSelected = string.Format("Cashiers : {0} ", filter.CashiersSelected != null ? string.Join(",", filter.CashiersSelected) : "All");
            var staffSelected = string.Format("Staffs : {0} ", filter.StaffSelected != null ? string.Join(",", filter.StaffSelected) : "All");
            var locationSelected = string.Format("Locations : {0} ", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "All");
            var terminalsSelected = string.Format("Terminals : {0} ", filter.TerminalsSelected != null ? string.Join(",", filter.TerminalsSelected) : "All");

            var ds = new DataSet();
            var table1 = DailyAuditByCashierTerminalDataset();

            var filteredResult = new List<DailyAuditData>();

            result.ForEach(s =>
            {

                var multiplePairExistRes = result.Where(r => r.FirstJourney == s.FirstJourney
                    && r.EmployeeNo == s.EmployeeNo
                    && r.DutyDate == s.DutyDate).Count();

                if (multiplePairExistRes > 1)
                {
                    var multiplePairExistFil = filteredResult.Where(r => r.FirstJourney == s.FirstJourney
                    && r.EmployeeNo == s.EmployeeNo
                    && r.DutyDate == s.DutyDate).Count();

                    if (multiplePairExistRes > 1)
                    {
                        filteredResult.RemoveAll(r => r.FirstJourney == s.FirstJourney
                                        && r.EmployeeNo == s.EmployeeNo
                                        && r.DutyDate == s.DutyDate
                                        && r.TotalPs == 0); //remove all zero

                        var multiplePairExistFilNonZeroPsg = filteredResult.Where(r => r.FirstJourney == s.FirstJourney
                                        && r.EmployeeNo == s.EmployeeNo
                                        && r.DutyDate == s.DutyDate).Count();
                        if (multiplePairExistFilNonZeroPsg > 0 && s.TotalPs <= 0)
                        {
                            //there is already non zero object so ignore current zero object
                        }
                        else
                        {
                            filteredResult.Add(s);
                        }
                    }
                    else
                    {
                        filteredResult.Add(s);
                    }
                }
                else
                {
                    filteredResult.Add(s);
                }
            });

            if (filteredResult.Any())
            {
                foreach (var res in filteredResult)
                {
                    table1.Rows.Add(
                        res.EmployeeNo,
                        res.EmployeeName,
                        res.Module,
                        res.Duty,
                        res.DutyDate,
                        res.DutySignOn,
                        res.DutySignOff,
                        res.BusNumber,
                        res.EquipmentNumber,
                        res.FirstRoute,
                        res.FirstJourney,
                        res.Revenue,
                        res.Tickets,
                        res.Passes,
                        res.Transfers,
                        res.modulesignoff,
                        res.modulesignon,
                        companyName,
                        filterDateRange,
                        staffSelected,
                        casherSelected,
                        res.TotalPs,
                        locationSelected,
                        terminalsSelected,
                        res.CashierName, res.str4_LocationCode, res.Terminal, res.Cashsignon, res.Cashsignoff, res.CashierNum, res.NonRevenue, res.TransactionDatetime);
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["companyName"] = companyName;
                dr["DateRangeFilter"] = filterDateRange;
                dr["StaffsSelected"] = staffSelected;
                dr["casherSelected"] = casherSelected;
                dr["LocationSelected"] = locationSelected;
                dr["TermnalSelected"] = terminalsSelected;
                table1.Rows.Add(dr);
            }


            ds.Tables.Add(table1);
            return ds;
        }

        public DataSet GetDailyAuditByCashierTerminalDatasetSummary(string connKey, CashierReportSummaryFilter filter, string companyName)
        {
            var result = GetDailyAuditByCashierTerminalDataSummary(connKey, filter);

            //filter details
            var filterDateRange = string.Format("{0}: {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var casherSelected = string.Format("Cashiers : {0} ", filter.CashiersSelected != null ? string.Join(",", filter.CashiersSelected) : "All");
            var staffSelected = string.Format("Staffs : {0} ", filter.StaffSelected != null ? string.Join(",", filter.StaffSelected) : "All");
            var locationSelected = string.Format("Locations : {0} ", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "All");
            var terminalsSelected = string.Format("Terminals : {0} ", filter.TerminalsSelected != null ? string.Join(",", filter.TerminalsSelected) : "All");

            var ds = new DataSet();
            var table1 = DailyAuditByCashierTerminalDataset();

            var filteredResult = new List<DailyAuditData>();

            result.ForEach(s =>
            {

                var multiplePairExistRes = result.Where(r => r.FirstJourney == s.FirstJourney
                    && r.EmployeeNo == s.EmployeeNo
                    && r.DutyDate == s.DutyDate).Count();

                if (multiplePairExistRes > 1)
                {
                    var multiplePairExistFil = filteredResult.Where(r => r.FirstJourney == s.FirstJourney
                    && r.EmployeeNo == s.EmployeeNo
                    && r.DutyDate == s.DutyDate).Count();

                    if (multiplePairExistRes > 1)
                    {
                        filteredResult.RemoveAll(r => r.FirstJourney == s.FirstJourney
                                        && r.EmployeeNo == s.EmployeeNo
                                        && r.DutyDate == s.DutyDate
                                        && r.TotalPs == 0); //remove all zero

                        var multiplePairExistFilNonZeroPsg = filteredResult.Where(r => r.FirstJourney == s.FirstJourney
                                        && r.EmployeeNo == s.EmployeeNo
                                        && r.DutyDate == s.DutyDate).Count();
                        if (multiplePairExistFilNonZeroPsg > 0 && s.TotalPs <= 0)
                        {
                            //there is already non zero object so ignore current zero object
                        }
                        else
                        {
                            filteredResult.Add(s);
                        }
                    }
                    else
                    {
                        filteredResult.Add(s);
                    }
                }
                else
                {
                    filteredResult.Add(s);
                }
            });

            var eachEmployee = filteredResult.Select(x => x.EmployeeName).Distinct().ToList();
            var bindlist = new List<DailyAuditData>();
            foreach (var emp in eachEmployee)
            {
                var item = new DailyAuditData();
                foreach (var res in filteredResult.OrderBy(x => x.DutyDate))
                {
                    if (res.EmployeeName == emp)
                    {
                        item.EmployeeNo = res.EmployeeNo;
                        item.EmployeeName = res.EmployeeName;
                        item.Module = res.Module;
                        item.Duty = res.Duty;
                        item.DutyDate = res.DutyDate;
                        if (string.IsNullOrEmpty(item.DutySignOn))
                        {
                            item.DutySignOn = res.modulesignon;
                        }
                        item.DutySignOff = res.modulesignoff;
                        item.BusNumber = res.BusNumber;
                        item.EquipmentNumber = res.EquipmentNumber;
                        item.FirstRoute = res.FirstRoute;
                        item.FirstJourney = res.FirstJourney;
                        if (string.IsNullOrEmpty(item.Revenue))
                        {
                            item.Revenue = res.Revenue;
                        }
                        else
                        {
                            item.Revenue = (Convert.ToDouble(item.Revenue) + Convert.ToDouble(res.Revenue)).ToString();
                        }
                        if (string.IsNullOrEmpty(item.NonRevenue))
                        {
                            item.NonRevenue = res.NonRevenue;
                        }
                        else
                        {
                            item.NonRevenue = (Convert.ToDouble(item.NonRevenue) + Convert.ToDouble(res.NonRevenue)).ToString();
                        }

                        if (string.IsNullOrEmpty(item.Tickets))
                        {
                            item.Tickets = res.Tickets;
                        }
                        else
                        {
                            item.Tickets = (Convert.ToInt32(item.Tickets) + Convert.ToInt32(res.Tickets)).ToString();
                        }

                        if (string.IsNullOrEmpty(item.Passes))
                        {
                            item.Passes = res.Passes;
                        }
                        else
                        {
                            item.Passes = (Convert.ToInt32(item.Passes) + Convert.ToInt32(res.Passes)).ToString();
                        }
                        if (string.IsNullOrEmpty(item.modulesignon))
                        {
                            item.modulesignon = res.modulesignon;
                        }
                        item.modulesignoff = res.modulesignoff;
                        item.TotalPs = item.TotalPs + res.TotalPs;
                        item.CashierName = res.CashierName;
                        item.str4_LocationCode = res.str4_LocationCode;
                        item.Terminal = res.Terminal;
                        item.Cashsignon = res.Cashsignon;
                        item.Cashsignoff = res.Cashsignoff;
                        item.CashierNum = res.CashierNum;
                        item.TransactionDatetime = res.TransactionDatetime;
                    }
                }
                bindlist.Add(item);
            }

            if (bindlist.Any())
            {
                foreach (var res in bindlist)
                {
                    table1.Rows.Add(
                        res.EmployeeNo,
                        res.EmployeeName,
                        res.Module,
                        res.Duty,
                        res.DutyDate,
                        res.DutySignOn,
                        res.DutySignOff,
                        res.BusNumber,
                        res.EquipmentNumber,
                        res.FirstRoute,
                        res.FirstJourney,
                        res.Revenue,
                        res.Tickets,
                        res.Passes,
                        res.Transfers,
                        res.modulesignoff,
                        res.modulesignon,
                        companyName,
                        filterDateRange,
                        staffSelected,
                        casherSelected,
                        res.TotalPs,
                        locationSelected,
                        terminalsSelected,
                        res.CashierName.Trim(), res.str4_LocationCode.Trim(), res.Terminal.Trim(), res.Cashsignon, res.Cashsignoff, res.CashierNum.Trim(), res.NonRevenue, res.TransactionDatetime.Trim());
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["companyName"] = companyName;
                dr["DateRangeFilter"] = filterDateRange;
                dr["StaffsSelected"] = staffSelected;
                dr["casherSelected"] = casherSelected;
                dr["LocationSelected"] = locationSelected;
                dr["TermnalSelected"] = terminalsSelected;
                table1.Rows.Add(dr);
            }


            ds.Tables.Add(table1);
            return ds;
        }

        public DataSet GetDailyAuditAtamelangReportDataset(string connKey, CashierReportSummaryFilter filter, string companyName)
        {
            var result = GetDailyAuditAtamelangReportDataset(connKey, filter);

            //filter details
            var filterDateRange = string.Format("{0}: {1}", "Date ", filter.StartDate);
            var staffSelected = string.Format("Staffs : {0} ", filter.StaffSelected != null ? string.Join(",", filter.StaffSelected) : "All");
            var locationSelected = string.Format("Locations : {0} ", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "All");

            var ds = new DataSet();
            var table1 = DailyAuditByCashierTerminalDataset();

            var filteredResult = new List<DailyAuditData>();

            result.ForEach(s =>
            {

                var multiplePairExistRes = result.Where(r => r.FirstJourney == s.FirstJourney
                    && r.EmployeeNo == s.EmployeeNo
                    && r.DutyDate == s.DutyDate).Count();

                if (multiplePairExistRes > 1)
                {
                    var multiplePairExistFil = filteredResult.Where(r => r.FirstJourney == s.FirstJourney
                    && r.EmployeeNo == s.EmployeeNo
                    && r.DutyDate == s.DutyDate).Count();

                    if (multiplePairExistRes > 1)
                    {
                        filteredResult.RemoveAll(r => r.FirstJourney == s.FirstJourney
                                        && r.EmployeeNo == s.EmployeeNo
                                        && r.DutyDate == s.DutyDate
                                        && r.TotalPs == 0); //remove all zero

                        var multiplePairExistFilNonZeroPsg = filteredResult.Where(r => r.FirstJourney == s.FirstJourney
                                        && r.EmployeeNo == s.EmployeeNo
                                        && r.DutyDate == s.DutyDate).Count();
                        if (multiplePairExistFilNonZeroPsg > 0 && s.TotalPs <= 0)
                        {
                            //there is already non zero object so ignore current zero object
                        }
                        else
                        {
                            filteredResult.Add(s);
                        }
                    }
                    else
                    {
                        filteredResult.Add(s);
                    }
                }
                else
                {
                    filteredResult.Add(s);
                }
            });

            if (filteredResult.Any())
            {
                foreach (var res in filteredResult)
                {
                    table1.Rows.Add(
                        res.EmployeeNo,
                        res.EmployeeName.Trim() + " (" + res.EmployeeNo + ")",
                        res.Module,
                        res.Duty,
                        res.DutyDate,
                        res.DutySignOn,
                        res.DutySignOff,
                        res.BusNumber,
                        res.EquipmentNumber.PadLeft(6, '0'),
                        res.FirstRoute,
                        res.FirstJourney,
                        res.Revenue,
                        res.Tickets,
                        res.Passes,
                        res.Transfers,
                        res.modulesignoff,
                        res.modulesignon,
                        companyName,
                        filterDateRange,
                        staffSelected,
                        "",
                        res.TotalPs,
                        locationSelected,
                        "",
                        res.CashierName.Trim(), res.str4_LocationCode.Trim() + " (" + res.LocationID + ")", res.Terminal.Trim(), res.Cashsignon, res.Cashsignoff, res.CashierNum.Trim(), res.NonRevenue, res.TransactionDatetime.Trim());
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["companyName"] = companyName;
                dr["DateRangeFilter"] = filterDateRange;
                dr["StaffsSelected"] = staffSelected;
                dr["casherSelected"] = "";
                dr["LocationSelected"] = locationSelected;
                dr["TermnalSelected"] = "";
                table1.Rows.Add(dr);
            }


            ds.Tables.Add(table1);
            return ds;
        }

        public DataSet GetDailyAuditMatatieleReportDataset(string connKey, CashierReportSummaryFilter filter, string companyName)
        {
            var result = GetDailyAuditMatatieleReportDataset(connKey, filter);

            //filter details
            var filterDateRange = string.Format("{0}: {1}", "Date ", filter.StartDate);
            var staffSelected = string.Format("Staffs : {0} ", filter.StaffSelected != null ? string.Join(",", filter.StaffSelected) : "All");
            var locationSelected = string.Format("Locations : {0} ", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "All");
            var classesSelected = string.Format("Classes : {0} ", filter.ClassesSelected != null ? string.Join(",", filter.ClassesSelected) : "All");
            var classTypesSelected = string.Format("ClassTypes : {0} ", filter.ClassTypesSelected != null ? string.Join(",", filter.ClassTypesSelected) : "All");

            var ds = new DataSet();
            var table1 = DailyAuditByCashierTerminalDataset();

            var filteredResult = new List<DailyAuditData>();

            result.ForEach(s =>
            {

                var multiplePairExistRes = result.Where(r => r.FirstJourney == s.FirstJourney
                    && r.EmployeeNo == s.EmployeeNo
                    && r.DutyDate == s.DutyDate).Count();

                if (multiplePairExistRes > 1)
                {
                    var multiplePairExistFil = filteredResult.Where(r => r.FirstJourney == s.FirstJourney
                    && r.EmployeeNo == s.EmployeeNo
                    && r.DutyDate == s.DutyDate).Count();

                    if (multiplePairExistRes > 1)
                    {
                        filteredResult.RemoveAll(r => r.FirstJourney == s.FirstJourney
                                        && r.EmployeeNo == s.EmployeeNo
                                        && r.DutyDate == s.DutyDate
                                        && r.TotalPs == 0); //remove all zero

                        var multiplePairExistFilNonZeroPsg = filteredResult.Where(r => r.FirstJourney == s.FirstJourney
                                        && r.EmployeeNo == s.EmployeeNo
                                        && r.DutyDate == s.DutyDate).Count();
                        if (multiplePairExistFilNonZeroPsg > 0 && s.TotalPs <= 0)
                        {
                            //there is already non zero object so ignore current zero object
                        }
                        else
                        {
                            filteredResult.Add(s);
                        }
                    }
                    else
                    {
                        filteredResult.Add(s);
                    }
                }
                else
                {
                    filteredResult.Add(s);
                }
            });

            if (filteredResult.Any())
            {
                foreach (var res in filteredResult)
                {
                    table1.Rows.Add(
                        res.EmployeeNo,
                        res.EmployeeName.Trim() + " (" + res.EmployeeNo + ")",
                        res.Module,
                        res.Duty,
                        res.DutyDate,
                        res.DutySignOn,
                        res.DutySignOff,
                        res.BusNumber,
                        res.EquipmentNumber.PadLeft(6, '0'),
                        res.FirstRoute,
                        res.FirstJourney,
                        res.Revenue,
                        res.Tickets,
                        res.Passes,
                        res.Transfers,
                        res.modulesignoff,
                        res.modulesignon,
                        companyName,
                        filterDateRange,
                        staffSelected,
                        "",
                        res.TotalPs,
                        locationSelected,
                        "",
                        res.CashierName.Trim(), res.str4_LocationCode.Trim() + " (" + res.LocationID + ")", res.Terminal.Trim(), res.Cashsignon, res.Cashsignoff, res.CashierNum.Trim(), res.NonRevenue, "",
                        classesSelected,
                        classTypesSelected);
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["companyName"] = companyName;
                dr["DateRangeFilter"] = filterDateRange;
                dr["StaffsSelected"] = staffSelected;
                dr["casherSelected"] = "";
                dr["LocationSelected"] = locationSelected;
                dr["TermnalSelected"] = "";
                dr["ClassesSelected"] = locationSelected;
                dr["ClassTypesSelected"] = locationSelected;
                table1.Rows.Add(dr);
            }


            ds.Tables.Add(table1);
            return ds;
        }


        public List<DailyAuditData> GetDailyAuditByCashierTerminalData(string connKey, CashierReportSummaryFilter filter)
        {
            var schs = new List<DailyAuditData>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("DailyAuditByCashierTerminal", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@StafIds", filter.StaffSelected != null ? string.Join(",", filter.StaffSelected) : "");
                cmd.Parameters.AddWithValue("@CasherIds", filter.CashiersSelected != null ? string.Join(",", filter.CashiersSelected) : "");
                cmd.Parameters.AddWithValue("@LocationIDs", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "");
                cmd.Parameters.AddWithValue("@Terminals", filter.TerminalsSelected != null ? string.Join(",", filter.TerminalsSelected) : "");
                cmd.Parameters.AddWithValue("@FromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@ToDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new DailyAuditData();

                    if (dr["EmployeeNo"] != null && dr["EmployeeNo"].ToString() != string.Empty)
                    {
                        sch.EmployeeNo = Convert.ToInt32(dr["EmployeeNo"].ToString());
                    }

                    if (dr["DutyDate"] != null && dr["DutyDate"].ToString() != string.Empty)
                    {
                        var datePart = dr["DutyDate"].ToString().Split(' ')[0];
                        var date = datePart.Split('/');
                        var mont = (date[0].Length == 1 ? "0" + date[0] : date[0]).Trim();

                        sch.DutyDate = (date[1].Length == 1 ? "0" + date[1] : date[1]).Trim() + "/" + mont + "/" + date[2].Trim();
                    }

                    if (dr["DutySignOn"] != null && dr["DutySignOn"].ToString() != string.Empty)
                    {
                        sch.DutySignOn = dr["DutySignOn"].ToString();
                    }

                    if (dr["DutySignOff"] != null && dr["DutySignOff"].ToString() != string.Empty)
                    {
                        sch.DutySignOff = dr["DutySignOff"].ToString();
                    }

                    if (dr["Module"] != null && dr["Module"].ToString() != string.Empty)
                    {
                        sch.Module = Convert.ToInt32(dr["Module"].ToString());
                    }

                    if (dr["TotalPs"] != null && dr["TotalPs"].ToString() != string.Empty)
                    {
                        sch.TotalPs = Convert.ToInt32(dr["TotalPs"].ToString());
                    }

                    if (dr["Duty"] != null && dr["Duty"].ToString() != string.Empty)
                    {
                        sch.Duty = Convert.ToInt32(dr["Duty"].ToString());
                    }

                    if (dr["EmployeeName"] != null && dr["EmployeeName"].ToString() != string.Empty)
                    {
                        sch.EmployeeName = dr["EmployeeName"].ToString().Trim();
                    }

                    if (dr["BusNumber"] != null && dr["BusNumber"].ToString() != string.Empty)
                    {
                        sch.BusNumber = dr["BusNumber"].ToString().Trim();
                    }

                    if (dr["EquipmentNumber"] != null && dr["EquipmentNumber"].ToString() != string.Empty)
                    {
                        sch.EquipmentNumber = dr["EquipmentNumber"].ToString().Trim();
                    }

                    if (dr["FirstRoute"] != null && dr["FirstRoute"].ToString() != string.Empty)
                    {
                        sch.FirstRoute = dr["FirstRoute"].ToString().Trim();
                    }

                    if (dr["FirstJourney"] != null && dr["FirstJourney"].ToString() != string.Empty)
                    {
                        sch.FirstJourney = dr["FirstJourney"].ToString().Trim();
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = dr["Revenue"].ToString().Trim();
                    }

                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = dr["NonRevenue"].ToString().Trim();
                    }

                    if (dr["CashierNum"] != null && dr["CashierNum"].ToString() != string.Empty)
                    {
                        sch.CashierNum = dr["CashierNum"].ToString().Trim();
                    }

                    if (dr["Tickets"] != null && dr["Tickets"].ToString() != string.Empty)
                    {
                        sch.Tickets = dr["Tickets"].ToString().Trim();
                    }

                    if (dr["Passes"] != null && dr["Passes"].ToString() != string.Empty)
                    {
                        sch.Passes = dr["Passes"].ToString().Trim();
                    }

                    if (dr["Transfers"] != null && dr["Transfers"].ToString() != string.Empty)
                    {
                        sch.Transfers = dr["Transfers"].ToString().Trim();
                    }

                    if (dr["modulesignoff"] != null && dr["modulesignoff"].ToString() != string.Empty)
                    {
                        sch.modulesignoff = Convert.ToDateTime(dr["modulesignoff"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }

                    if (dr["modulesignon"] != null && dr["modulesignon"].ToString() != string.Empty)
                    {
                        sch.modulesignon = Convert.ToDateTime(dr["modulesignon"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }

                    ////
                    if (dr["CashierName"] != null && dr["CashierName"].ToString() != string.Empty)
                    {
                        sch.CashierName = dr["CashierName"].ToString().Trim();
                    }
                    if (dr["str4_LocationCode"] != null && dr["str4_LocationCode"].ToString() != string.Empty)
                    {
                        sch.str4_LocationCode = dr["str4_LocationCode"].ToString().Trim();
                    }
                    if (dr["Terminal"] != null && dr["Terminal"].ToString() != string.Empty)
                    {
                        sch.Terminal = dr["Terminal"].ToString().Trim();
                    }
                    /////
                    if (dr["Cashsignoff"] != null && dr["Cashsignoff"].ToString() != string.Empty)
                    {
                        sch.Cashsignoff = Convert.ToDateTime(dr["Cashsignoff"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }
                    if (dr["Cashsignon"] != null && dr["Cashsignon"].ToString() != string.Empty)
                    {
                        sch.Cashsignon = Convert.ToDateTime(dr["Cashsignon"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }
                    schs.Add(sch);
                }
            }
            finally
            {
                myConnection.Close();
            }
            return schs;
        }

        public List<DailyAuditData> GetDailyAuditByCashierTerminalDataSummary(string connKey, CashierReportSummaryFilter filter)
        {
            var schs = new List<DailyAuditData>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("DailyAuditByCashierTerminalSummary", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@StafIds", filter.StaffSelected != null ? string.Join(",", filter.StaffSelected) : "");
                cmd.Parameters.AddWithValue("@CasherIds", filter.CashiersSelected != null ? string.Join(",", filter.CashiersSelected) : "");
                cmd.Parameters.AddWithValue("@LocationIDs", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "");
                cmd.Parameters.AddWithValue("@Terminals", filter.TerminalsSelected != null ? string.Join(",", filter.TerminalsSelected) : "");
                cmd.Parameters.AddWithValue("@FromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@ToDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new DailyAuditData();

                    if (dr["EmployeeNo"] != null && dr["EmployeeNo"].ToString() != string.Empty)
                    {
                        sch.EmployeeNo = Convert.ToInt32(dr["EmployeeNo"].ToString());
                    }

                    if (dr["DutyDate"] != null && dr["DutyDate"].ToString() != string.Empty)
                    {
                        var datePart = dr["DutyDate"].ToString().Split(' ')[0];
                        var date = datePart.Split('/');
                        var mont = (date[0].Length == 1 ? "0" + date[0] : date[0]).Trim();

                        sch.DutyDate = (date[1].Length == 1 ? "0" + date[1] : date[1]).Trim() + "/" + mont + "/" + date[2].Trim();
                    }

                    if (dr["DutySignOn"] != null && dr["DutySignOn"].ToString() != string.Empty)
                    {
                        sch.DutySignOn = dr["DutySignOn"].ToString();
                    }

                    if (dr["DutySignOff"] != null && dr["DutySignOff"].ToString() != string.Empty)
                    {
                        sch.DutySignOff = dr["DutySignOff"].ToString();
                    }

                    if (dr["Module"] != null && dr["Module"].ToString() != string.Empty)
                    {
                        sch.Module = Convert.ToInt32(dr["Module"].ToString());
                    }

                    if (dr["TotalPs"] != null && dr["TotalPs"].ToString() != string.Empty)
                    {
                        sch.TotalPs = Convert.ToInt32(dr["TotalPs"].ToString());
                    }

                    if (dr["Duty"] != null && dr["Duty"].ToString() != string.Empty)
                    {
                        sch.Duty = Convert.ToInt32(dr["Duty"].ToString());
                    }

                    if (dr["EmployeeName"] != null && dr["EmployeeName"].ToString() != string.Empty)
                    {
                        sch.EmployeeName = dr["EmployeeName"].ToString().Trim();
                    }

                    if (dr["BusNumber"] != null && dr["BusNumber"].ToString() != string.Empty)
                    {
                        sch.BusNumber = dr["BusNumber"].ToString().Trim();
                    }

                    if (dr["EquipmentNumber"] != null && dr["EquipmentNumber"].ToString() != string.Empty)
                    {
                        sch.EquipmentNumber = dr["EquipmentNumber"].ToString().Trim();
                    }

                    if (dr["FirstRoute"] != null && dr["FirstRoute"].ToString() != string.Empty)
                    {
                        sch.FirstRoute = dr["FirstRoute"].ToString().Trim();
                    }

                    if (dr["FirstJourney"] != null && dr["FirstJourney"].ToString() != string.Empty)
                    {
                        sch.FirstJourney = dr["FirstJourney"].ToString().Trim();
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = dr["Revenue"].ToString().Trim();
                    }

                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = dr["NonRevenue"].ToString().Trim();
                    }

                    if (dr["CashierNum"] != null && dr["CashierNum"].ToString() != string.Empty)
                    {
                        sch.CashierNum = dr["CashierNum"].ToString().Trim();
                    }

                    if (dr["Tickets"] != null && dr["Tickets"].ToString() != string.Empty)
                    {
                        sch.Tickets = dr["Tickets"].ToString().Trim();
                    }

                    if (dr["Passes"] != null && dr["Passes"].ToString() != string.Empty)
                    {
                        sch.Passes = dr["Passes"].ToString().Trim();
                    }

                    if (dr["Transfers"] != null && dr["Transfers"].ToString() != string.Empty)
                    {
                        sch.Transfers = dr["Transfers"].ToString().Trim();
                    }

                    if (dr["modulesignoff"] != null && dr["modulesignoff"].ToString() != string.Empty)
                    {
                        sch.modulesignoff = Convert.ToDateTime(dr["modulesignoff"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }

                    if (dr["modulesignon"] != null && dr["modulesignon"].ToString() != string.Empty)
                    {
                        sch.modulesignon = Convert.ToDateTime(dr["modulesignon"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }

                    ////
                    if (dr["CashierName"] != null && dr["CashierName"].ToString() != string.Empty)
                    {
                        sch.CashierName = dr["CashierName"].ToString().Trim();
                    }
                    if (dr["str4_LocationCode"] != null && dr["str4_LocationCode"].ToString() != string.Empty)
                    {
                        sch.str4_LocationCode = dr["str4_LocationCode"].ToString().Trim();
                    }
                    if (dr["LocationID"] != null && dr["LocationID"].ToString() != string.Empty)
                    {
                        sch.LocationID = dr["LocationID"].ToString().Trim();
                    }
                    if (dr["Terminal"] != null && dr["Terminal"].ToString() != string.Empty)
                    {
                        sch.Terminal = dr["Terminal"].ToString().Trim();
                    }
                    /////
                    if (dr["Cashsignoff"] != null && dr["Cashsignoff"].ToString() != string.Empty)
                    {
                        sch.Cashsignoff = Convert.ToDateTime(dr["Cashsignoff"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }
                    if (dr["Cashsignon"] != null && dr["Cashsignon"].ToString() != string.Empty)
                    {
                        sch.Cashsignon = Convert.ToDateTime(dr["Cashsignon"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }
                    if (dr["TransactionDatetime"] != null && dr["TransactionDatetime"].ToString() != string.Empty)
                    {
                        sch.TransactionDatetime = Convert.ToDateTime(dr["TransactionDatetime"].ToString().Trim()).ToString("dd/MM/yyyy");
                    }
                    schs.Add(sch);
                }
            }
            finally
            {
                myConnection.Close();
            }
            return schs;
        }

        public List<DailyAuditData> GetDailyAuditAtamelangReportDataset(string connKey, CashierReportSummaryFilter filter)
        {
            var schs = new List<DailyAuditData>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("DailyAuditByCashierTerminalSummary", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@StafIds", filter.StaffSelected != null ? string.Join(",", filter.StaffSelected) : "");
                cmd.Parameters.AddWithValue("@CasherIds", filter.CashiersSelected != null ? string.Join(",", filter.CashiersSelected) : "");
                cmd.Parameters.AddWithValue("@LocationIDs", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "");
                cmd.Parameters.AddWithValue("@Terminals", filter.TerminalsSelected != null ? string.Join(",", filter.TerminalsSelected) : "");
                cmd.Parameters.AddWithValue("@FromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@ToDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new DailyAuditData();

                    if (dr["EmployeeNo"] != null && dr["EmployeeNo"].ToString() != string.Empty)
                    {
                        sch.EmployeeNo = Convert.ToInt32(dr["EmployeeNo"].ToString());
                    }

                    if (dr["DutyDate"] != null && dr["DutyDate"].ToString() != string.Empty)
                    {
                        var datePart = dr["DutyDate"].ToString().Split(' ')[0];
                        var date = datePart.Split('/');
                        var mont = (date[0].Length == 1 ? "0" + date[0] : date[0]).Trim();

                        sch.DutyDate = (date[1].Length == 1 ? "0" + date[1] : date[1]).Trim() + "/" + mont + "/" + date[2].Trim();
                    }

                    if (dr["DutySignOn"] != null && dr["DutySignOn"].ToString() != string.Empty)
                    {
                        sch.DutySignOn = dr["DutySignOn"].ToString();
                    }

                    if (dr["DutySignOff"] != null && dr["DutySignOff"].ToString() != string.Empty)
                    {
                        sch.DutySignOff = dr["DutySignOff"].ToString();
                    }

                    if (dr["Module"] != null && dr["Module"].ToString() != string.Empty)
                    {
                        sch.Module = Convert.ToInt32(dr["Module"].ToString());
                    }

                    if (dr["TotalPs"] != null && dr["TotalPs"].ToString() != string.Empty)
                    {
                        sch.TotalPs = Convert.ToInt32(dr["TotalPs"].ToString());
                    }

                    if (dr["Duty"] != null && dr["Duty"].ToString() != string.Empty)
                    {
                        sch.Duty = Convert.ToInt32(dr["Duty"].ToString());
                    }

                    if (dr["EmployeeName"] != null && dr["EmployeeName"].ToString() != string.Empty)
                    {
                        sch.EmployeeName = dr["EmployeeName"].ToString().Trim();
                    }

                    if (dr["BusNumber"] != null && dr["BusNumber"].ToString() != string.Empty)
                    {
                        sch.BusNumber = dr["BusNumber"].ToString().Trim();
                    }

                    if (dr["EquipmentNumber"] != null && dr["EquipmentNumber"].ToString() != string.Empty)
                    {
                        sch.EquipmentNumber = dr["EquipmentNumber"].ToString().Trim();
                    }

                    if (dr["FirstRoute"] != null && dr["FirstRoute"].ToString() != string.Empty)
                    {
                        sch.FirstRoute = dr["FirstRoute"].ToString().Trim();
                    }

                    if (dr["FirstJourney"] != null && dr["FirstJourney"].ToString() != string.Empty)
                    {
                        sch.FirstJourney = dr["FirstJourney"].ToString().Trim();
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = dr["Revenue"].ToString().Trim();
                    }

                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = dr["NonRevenue"].ToString().Trim();
                    }

                    if (dr["CashierNum"] != null && dr["CashierNum"].ToString() != string.Empty)
                    {
                        sch.CashierNum = dr["CashierNum"].ToString().Trim();
                    }

                    if (dr["Tickets"] != null && dr["Tickets"].ToString() != string.Empty)
                    {
                        sch.Tickets = dr["Tickets"].ToString().Trim();
                    }

                    if (dr["Passes"] != null && dr["Passes"].ToString() != string.Empty)
                    {
                        sch.Passes = dr["Passes"].ToString().Trim();
                    }

                    if (dr["Transfers"] != null && dr["Transfers"].ToString() != string.Empty)
                    {
                        sch.Transfers = dr["Transfers"].ToString().Trim();
                    }

                    if (dr["modulesignoff"] != null && dr["modulesignoff"].ToString() != string.Empty)
                    {
                        sch.modulesignoff = Convert.ToDateTime(dr["modulesignoff"].ToString().Trim()).ToString("dd/MM/yyyy");
                    }

                    if (dr["modulesignon"] != null && dr["modulesignon"].ToString() != string.Empty)
                    {
                        sch.modulesignon = Convert.ToDateTime(dr["modulesignon"].ToString().Trim()).ToString("dd/MM/yyyy");
                    }

                    ////
                    if (dr["CashierName"] != null && dr["CashierName"].ToString() != string.Empty)
                    {
                        sch.CashierName = dr["CashierName"].ToString().Trim();
                    }
                    if (dr["str4_LocationCode"] != null && dr["str4_LocationCode"].ToString() != string.Empty)
                    {
                        sch.str4_LocationCode = dr["str4_LocationCode"].ToString().Trim();
                    }
                    if (dr["LocationID"] != null && dr["LocationID"].ToString() != string.Empty)
                    {
                        sch.LocationID = dr["LocationID"].ToString().Trim();
                    }
                    if (dr["Terminal"] != null && dr["Terminal"].ToString() != string.Empty)
                    {
                        sch.Terminal = dr["Terminal"].ToString().Trim();
                    }
                    /////
                    if (dr["Cashsignoff"] != null && dr["Cashsignoff"].ToString() != string.Empty)
                    {
                        sch.Cashsignoff = Convert.ToDateTime(dr["Cashsignoff"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }
                    if (dr["Cashsignon"] != null && dr["Cashsignon"].ToString() != string.Empty)
                    {
                        sch.Cashsignon = Convert.ToDateTime(dr["Cashsignon"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }
                    if (dr["TransactionDatetime"] != null && dr["TransactionDatetime"].ToString() != string.Empty)
                    {
                        sch.TransactionDatetime = Convert.ToDateTime(dr["TransactionDatetime"].ToString().Trim()).ToString("dd/MM/yyyy");
                    }
                    schs.Add(sch);
                }
            }
            finally
            {
                myConnection.Close();
            }
            return schs;
        }

        public List<DailyAuditData> GetDailyAuditMatatieleReportDataset(string connKey, CashierReportSummaryFilter filter)
        {
            var schs = new List<DailyAuditData>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("DailyAuditForMatatiele", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@StafIds", filter.StaffSelected != null ? string.Join(",", filter.StaffSelected) : "");
                cmd.Parameters.AddWithValue("@Classes", filter.ClassesSelected != null ? string.Join(",", filter.ClassesSelected) : "");
                cmd.Parameters.AddWithValue("@LocationIDs", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "");
                cmd.Parameters.AddWithValue("@ClassTypes", filter.ClassTypesSelected != null ? string.Join(",", filter.ClassTypesSelected) : "");
                cmd.Parameters.AddWithValue("@FromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@ToDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new DailyAuditData();

                    if (dr["EmployeeNo"] != null && dr["EmployeeNo"].ToString() != string.Empty)
                    {
                        sch.EmployeeNo = Convert.ToInt32(dr["EmployeeNo"].ToString());
                    }

                    if (dr["DutyDate"] != null && dr["DutyDate"].ToString() != string.Empty)
                    {
                        var datePart = dr["DutyDate"].ToString().Split(' ')[0];
                        var date = datePart.Split('/');
                        var mont = (date[0].Length == 1 ? "0" + date[0] : date[0]).Trim();

                        sch.DutyDate = (date[1].Length == 1 ? "0" + date[1] : date[1]).Trim() + "/" + mont + "/" + date[2].Trim();
                    }

                    if (dr["DutySignOn"] != null && dr["DutySignOn"].ToString() != string.Empty)
                    {
                        sch.DutySignOn = dr["DutySignOn"].ToString();
                    }

                    if (dr["DutySignOff"] != null && dr["DutySignOff"].ToString() != string.Empty)
                    {
                        sch.DutySignOff = dr["DutySignOff"].ToString();
                    }

                    if (dr["Module"] != null && dr["Module"].ToString() != string.Empty)
                    {
                        sch.Module = Convert.ToInt32(dr["Module"].ToString());
                    }

                    if (dr["TotalPs"] != null && dr["TotalPs"].ToString() != string.Empty)
                    {
                        sch.TotalPs = Convert.ToInt32(dr["TotalPs"].ToString());
                    }

                    if (dr["Duty"] != null && dr["Duty"].ToString() != string.Empty)
                    {
                        sch.Duty = Convert.ToInt32(dr["Duty"].ToString());
                    }

                    if (dr["EmployeeName"] != null && dr["EmployeeName"].ToString() != string.Empty)
                    {
                        sch.EmployeeName = dr["EmployeeName"].ToString().Trim();
                    }

                    if (dr["BusNumber"] != null && dr["BusNumber"].ToString() != string.Empty)
                    {
                        sch.BusNumber = dr["BusNumber"].ToString().Trim();
                    }

                    if (dr["EquipmentNumber"] != null && dr["EquipmentNumber"].ToString() != string.Empty)
                    {
                        sch.EquipmentNumber = dr["EquipmentNumber"].ToString().Trim();
                    }

                    if (dr["FirstRoute"] != null && dr["FirstRoute"].ToString() != string.Empty)
                    {
                        sch.FirstRoute = dr["FirstRoute"].ToString().Trim();
                    }

                    if (dr["FirstJourney"] != null && dr["FirstJourney"].ToString() != string.Empty)
                    {
                        sch.FirstJourney = dr["FirstJourney"].ToString().Trim();
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = dr["Revenue"].ToString().Trim();
                    }

                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = dr["NonRevenue"].ToString().Trim();
                    }

                    if (dr["CashierNum"] != null && dr["CashierNum"].ToString() != string.Empty)
                    {
                        sch.CashierNum = dr["CashierNum"].ToString().Trim();
                    }

                    if (dr["Tickets"] != null && dr["Tickets"].ToString() != string.Empty)
                    {
                        sch.Tickets = dr["Tickets"].ToString().Trim();
                    }

                    if (dr["Passes"] != null && dr["Passes"].ToString() != string.Empty)
                    {
                        sch.Passes = dr["Passes"].ToString().Trim();
                    }

                    if (dr["Transfers"] != null && dr["Transfers"].ToString() != string.Empty)
                    {
                        sch.Transfers = dr["Transfers"].ToString().Trim();
                    }

                    if (dr["modulesignoff"] != null && dr["modulesignoff"].ToString() != string.Empty)
                    {
                        sch.modulesignoff = Convert.ToDateTime(dr["modulesignoff"].ToString().Trim()).ToString("dd/MM/yyyy");
                    }

                    if (dr["modulesignon"] != null && dr["modulesignon"].ToString() != string.Empty)
                    {
                        sch.modulesignon = Convert.ToDateTime(dr["modulesignon"].ToString().Trim()).ToString("dd/MM/yyyy");
                    }

                    ////
                    if (dr["CashierName"] != null && dr["CashierName"].ToString() != string.Empty)
                    {
                        sch.CashierName = dr["CashierName"].ToString().Trim();
                    }
                    if (dr["str4_LocationCode"] != null && dr["str4_LocationCode"].ToString() != string.Empty)
                    {
                        sch.str4_LocationCode = dr["str4_LocationCode"].ToString().Trim();
                    }
                    if (dr["LocationID"] != null && dr["LocationID"].ToString() != string.Empty)
                    {
                        sch.LocationID = dr["LocationID"].ToString().Trim();
                    }
                    if (dr["Terminal"] != null && dr["Terminal"].ToString() != string.Empty)
                    {
                        sch.Terminal = dr["Terminal"].ToString().Trim();
                    }
                    /////
                    if (dr["Cashsignoff"] != null && dr["Cashsignoff"].ToString() != string.Empty)
                    {
                        sch.Cashsignoff = Convert.ToDateTime(dr["Cashsignoff"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }
                    if (dr["Cashsignon"] != null && dr["Cashsignon"].ToString() != string.Empty)
                    {
                        sch.Cashsignon = Convert.ToDateTime(dr["Cashsignon"].ToString().Trim()).ToString("dd/MM/yyyy HH:mm");
                    }
                    if (dr["TransactionDatetime"] != null && dr["TransactionDatetime"].ToString() != string.Empty)
                    {
                        sch.TransactionDatetime = Convert.ToDateTime(dr["TransactionDatetime"].ToString().Trim()).ToString("dd/MM/yyyy");
                    }
                    schs.Add(sch);
                }
            }
            finally
            {
                myConnection.Close();
            }
            return schs;
        }
    }
}