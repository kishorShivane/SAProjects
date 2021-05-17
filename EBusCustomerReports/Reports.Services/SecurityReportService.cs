using Reports.Services.Helpers;
using Reports.Services.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;


namespace Reports.Services
{
    public class SecurityReportService : BaseServices
    {
        public DataSet GetSecurityReportDataSet(string connKey, SecurityReportFilter filter, string companyName)
        {
            DataSet ds = new DataSet();
            DataTable table1 = InspectorReportDataset();
            string filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);

            string filterDriversSelected = "Security: Filter Not Selected";

            if (filter.SecuritySelected != null && filter.SecuritySelected.Length > 0)
            {
                filterDriversSelected = "Security: " + string.Join(", ", filter.SecuritySelected);
            }

            List<SecurityReportData> result = GetSecurityData(connKey, filter);

            if (result.Any())
            {
                foreach (SecurityReportData item in result)
                {
                    table1.Rows.Add(
                        item.stagetime,
                        item.journeyid,
                        item.routeId,
                        item.drivername + "-" + item.driverno,
                        item.busid,
                        item.SecurityName + "-" + item.SecurityNumber,
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

        public List<SecurityReportData> GetSecurityData(string connKey, SecurityReportFilter filter)
        {
            List<SecurityReportData> result = new List<SecurityReportData>();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand("EbusSecurityReport", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@SecurityIds", filter.SecuritySelected == null ? "" : string.Join(",", filter.SecuritySelected));
                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    SecurityReportData sch = new SecurityReportData();

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
                    if (dr["SecurityNumber"] != null && dr["SecurityNumber"].ToString() != string.Empty)
                    {
                        sch.SecurityNumber = Convert.ToInt32(dr["SecurityNumber"].ToString());
                    }
                    if (dr["SecurityName"] != null && dr["SecurityName"].ToString() != string.Empty)
                    {
                        sch.SecurityName = (dr["SecurityName"].ToString());
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

        public SecurityReportFilter GetSecurityReportFilter(string connKey)
        {
            SecurityReportFilter model = new SecurityReportFilter();

            List<OperatorDetails> staff = GetAllSatffDetails(connKey);

            model.Securities = staff.Where(s => s.OperatorType.ToLower() == "security".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();

            return model;
        }

        //Driver, Inspector
        public List<OperatorDetails> GetAllSatffDetails(string connKey)
        {
            List<OperatorDetails> res = new List<OperatorDetails>();
            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand()
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
                    OperatorDetails obj = new OperatorDetails();

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

    }
}
