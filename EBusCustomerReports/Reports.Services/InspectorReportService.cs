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
    public class InspectorReportService : BaseServices
    {
        public DataSet GetInspectorTransDataSet(string connKey, InspectorReportFilter filter, string companyName)
        {
            var ds = new DataSet();
            var table1 = InspectorReportDataset();
            var filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);

            var filterDriversSelected = "Drivers: Filter Not Selected";

            if (filter.DriversSelected != null && filter.DriversSelected.Length > 0)
            {
                filterDriversSelected = "Drivers: " + string.Join(", ", filter.DriversSelected);
            }

            var result = GetInspectorData(connKey, filter);

            if (result.Any())
            {
                foreach (var item in result)
                {
                    table1.Rows.Add(
                        item.stagetime,
                        item.journeyid,
                        item.routeId,
                        item.drivername + "-" + item.driverno,
                        item.busid,
                        item.inspectorno,
                        item.int4_dutyid,
                        item.int2_stageid,
                        item.StageName,
                        item.InsDate,
                        item.InsTime,
                        filterDateRange,
                        companyName,
                        filterDriversSelected);
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["DateRangeFilter"] = filterDateRange;
                dr["CompanyName"] = companyName;
                dr["DriversFilter"] = filterDriversSelected;

                table1.Rows.Add(dr);
            }
            

            ds.Tables.Add(table1);
            return ds;
        }

        public List<InspectorReportData> GetInspectorData(string connKey, InspectorReportFilter filter)
        {
            var result = new List<InspectorReportData>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("EbusInspectorReport", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@DriverIds", filter.DriversSelected == null ? "" : string.Join(",", filter.DriversSelected));
                cmd.Parameters.AddWithValue("@InspectorIds", filter.InspectorsSelected == null ? "" : string.Join(",", filter.InspectorsSelected));
                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new InspectorReportData();

                    if (dr["journeyid"] != null && dr["journeyid"].ToString() != string.Empty)
                    {
                        sch.journeyid = (dr["journeyid"].ToString());
                    }

                    if (dr["routeId"] != null && dr["routeId"].ToString() != string.Empty)
                    {
                        sch.routeId = (dr["routeId"].ToString());
                    }

                    if (dr["driverno"] != null && dr["driverno"].ToString() != string.Empty)
                    {
                        sch.driverno = (dr["driverno"].ToString());
                    }
                    if (dr["drivername"] != null && dr["drivername"].ToString() != string.Empty)
                    {
                        sch.drivername = (dr["drivername"].ToString());
                    }
                    if (dr["busid"] != null && dr["busid"].ToString() != string.Empty)
                    {
                        sch.busid = (dr["busid"].ToString());
                    }
                    if (dr["inspectorno"] != null && dr["inspectorno"].ToString() != string.Empty)
                    {
                        sch.inspectorno = Convert.ToInt32(dr["inspectorno"].ToString());
                    }

                    if (dr["int4_dutyid"] != null && dr["int4_dutyid"].ToString() != string.Empty)
                    {
                        sch.int4_dutyid = (dr["int4_dutyid"].ToString());
                    }
                    if (dr["int2_stageid"] != null && dr["int2_stageid"].ToString() != string.Empty)
                    {
                        sch.int2_stageid = (dr["int2_stageid"].ToString());
                    }
                    if (dr["StageName"] != null && dr["StageName"].ToString() != string.Empty)
                    {
                        sch.StageName = (dr["StageName"].ToString());
                    }


                    if (dr["stagetime"] != null && dr["stagetime"].ToString() != string.Empty)
                    {
                        sch.stagetime = DateTime.Parse(dr["stagetime"].ToString());
                        sch.InsDate = sch.stagetime.ToString("dd/MM/yyyy");
                        sch.InsTime = sch.stagetime.ToString("hh:mm tt");
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

        public InspectorReportFilter GetInspectorReportFilter(string connKey)
        {
            var model = new InspectorReportFilter();

            var staff = GetAllSatffDetails(connKey);

            model.Drivers = staff.Where(s => s.OperatorType.ToLower() == "Driver".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Inspectors = staff.Where(s => s.OperatorType.ToLower() == "Inspector".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();

            return model;
        }

        public DriverDetailsFilter GetDriverDetailsReportFilter(string connKey)
        {
            var model = new DriverDetailsFilter();

            var staff = GetAllSatffDetails(connKey);

            model.Drivers = staff.Select(s => new SelectListItem { Text = string.Format("{0} - {1} ", s.OperatorName, s.OperatorID), Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            return model;
        }

        //Driver, Inspector
        public  List<OperatorDetails> GetAllSatffDetails(string connKey)
        {
            var res = new List<OperatorDetails>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "select int4_StaffID, str50_StaffName, str25_Description from staff S " +
                                    "inner join StaffType ST on S.int4_StaffTypeID = ST.int4_StaffTypeID;";

                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new OperatorDetails();

                    if (dr["str50_StaffName"] != null && dr["str50_StaffName"].ToString() != string.Empty)
                    {
                        obj.OperatorName = dr["str50_StaffName"].ToString().Trim();
                    }

                    if (dr["int4_StaffID"] != null && dr["int4_StaffID"].ToString() != string.Empty)
                    {
                        obj.OperatorID = dr["int4_StaffID"].ToString().Trim();
                    }
                    if (dr["str25_Description"] != null && dr["str25_Description"].ToString() != string.Empty)
                    {
                        obj.OperatorType = dr["str25_Description"].ToString().Trim();
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

        public List<SelectListItem> GetAllLocations(string connKey)
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

                cmd.CommandText = "select str4_LocationCode,str50_LocationGroupName from locationgroup";
                cmd.CommandTimeout = 500000;
                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();

                    if (dr["str4_LocationCode"] != null && dr["str4_LocationCode"].ToString() != string.Empty)
                    {
                        obj.Value = dr["str4_LocationCode"].ToString().Trim();
                    }

                    if (dr["str50_LocationGroupName"] != null && dr["str50_LocationGroupName"].ToString() != string.Empty)
                    {
                        obj.Text = dr["str4_LocationCode"].ToString().Trim() + " - " + dr["str50_LocationGroupName"].ToString().Trim();
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

        public List<SelectListItem> GetAllClasses(string connKey)
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

                cmd.CommandText = "select int2_Class,str50_LongName from Class";
                cmd.CommandTimeout = 500000;
                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();

                    if (dr["int2_Class"] != null && dr["int2_Class"].ToString() != string.Empty)
                    {
                        obj.Value = dr["int2_Class"].ToString().Trim();
                    }

                    if (dr["str50_LongName"] != null && dr["str50_LongName"].ToString() != string.Empty)
                    {
                        obj.Text = dr["int2_Class"].ToString().Trim() + " - " + dr["str50_LongName"].ToString().Trim();
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

        public List<SelectListItem> GetAllClassTypes(string connKey)
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

                cmd.CommandText = "select int_TypeID,str30_Description from ClassType";
                cmd.CommandTimeout = 500000;
                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();

                    if (dr["int_TypeID"] != null && dr["int_TypeID"].ToString() != string.Empty)
                    {
                        obj.Value = dr["int_TypeID"].ToString().Trim();
                    }

                    if (dr["str30_Description"] != null && dr["str30_Description"].ToString() != string.Empty)
                    {
                        obj.Text = dr["int_TypeID"].ToString().Trim() + " - " + dr["str30_Description"].ToString().Trim();
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

        public List<SelectListItem> GetAllTerminals(string connKey)
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

                cmd.CommandText = "select distinct Terminal from CashierDetail where Terminal is not null";
                cmd.CommandTimeout = 500000;
                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();

                    if (dr["Terminal"] != null && dr["Terminal"].ToString() != string.Empty)
                    {
                        obj.Value = dr["Terminal"].ToString().Trim();
                    }

                    if (dr["Terminal"] != null && dr["Terminal"].ToString() != string.Empty)
                    {
                        obj.Text = dr["Terminal"].ToString().Trim();
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

        public DataSet GetDriverTransactionDetails(string connKey, DateTime selectedDate, Int32 staffId, string companyName)
        {
            var result = new DataSet();
            var operatorobj = GetAllSatffDetails(connKey).Where(s => s.OperatorID.Equals(staffId.ToString())).FirstOrDefault();
            var operatorName = string.Empty;

            if (operatorobj != null)
            {
                operatorName = operatorobj.OperatorName;
            }

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));
            try
            {
                myConnection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = myConnection;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "EbusGetDriverTransactionDetails";

                    cmd.Parameters.AddWithValue("@Selecteddate", selectedDate);
                    cmd.Parameters.AddWithValue("@StaffID", staffId);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(result);
                    }
                }
                //var dt = result.Tables[0];

                //if (dt.Rows.Count > 0)
                //{
                //    for (int i = dt.Rows.Count - 1; i >= 0; i--)
                //    {
                //        if (i == 0)
                //        {
                //            break;
                //        }
                //        for (int j = i - 1; j >= 0; j--)
                //        {
                //            if ( dt.Rows[i]["id_Trans"] == DBNull.Value && dt.Rows[j]["id_Trans"] != DBNull.Value && Convert.ToInt32(dt.Rows[i]["id_Trans"]) == Convert.ToInt32(dt.Rows[j]["id_Trans"])  )
                //            {
                //                dt.Rows[i].Delete();
                //                break;
                //            }
                //        }
                //    }
                //    dt.AcceptChanges();
                //}

                var newColumn = new DataColumn("CompanyName", typeof(string));
                newColumn.DefaultValue = companyName;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("DriverName", typeof(string));
                newColumn.DefaultValue = operatorName;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("DateSelected", typeof(string));
                newColumn.DefaultValue = selectedDate.ToString("dd-MM-yyyy"); ;
                result.Tables[0].Columns.Add(newColumn);

                if (result.Tables[0] != null && result.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < result.Tables[0].Rows.Count; i++)
                    {
                        var hexString = result.Tables[0].Rows[i]["TransType"].ToString().Split('#');

                        var hexVal = "";
                        if (hexString.Length > 1)
                        {
                            hexVal = hexString[1].Trim();
                        }
                        if (!string.IsNullOrEmpty(hexVal.ToString()))
                        {
                            hexVal = SmartCardService.ToLittleHex(hexVal);
                            result.Tables[0].Rows[i]["TransType"] = string.Format("Smart Card # {0}", Math.Abs(int.Parse(hexVal.ToString(), System.Globalization.NumberStyles.HexNumber)));
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
    }
}
