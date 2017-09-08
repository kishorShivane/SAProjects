using Reports.Services.Helpers;
using Reports.Services.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services
{
    public class CashierServices : BaseServices
    {
        public DataSet GetCashierDataSet(string connKey, CashierFilter filter, string companyName)
        {
            var ds = new DataSet();
            var table1 = CashierDataSet();
            var filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var locations = string.Format("Locations : {0} ", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "All");

            var result = GetCashierData(connKey, filter);

            if (result.Any())
            {
                foreach (var item in result)
                {
                    table1.Rows.Add(
                        item.StaffNumber,
                        item.Date,
                        item.Revenue,
                        item.StaffName,
                        item.CashierID,
                        item.Location,
                        item.CashierName,
                        filterDateRange,
                        locations,
                        companyName,
                        item.Date + " " + item.Time
                   );
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["dateFilter"] = filterDateRange;
                dr["Locations"] = locations;
                dr["CompanyName"] = companyName;

                table1.Rows.Add(dr);
            }

            ds.Tables.Add(table1);
            return ds;
        }

        public List<CashierData> GetCashierData(string connKey, CashierFilter filter)
        {
            var result = new List<CashierData>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("EbusCashierReport", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@LocationIDs", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "");
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new CashierData();
                    //StaffNumber	Date	Time	Revenue	str50_StaffName dateFilter
                    if (dr["StaffNumber"] != null && dr["StaffNumber"].ToString() != string.Empty)
                    {
                        sch.StaffNumber = (dr["StaffNumber"].ToString());
                    }

                    if (dr["Date"] != null && dr["Date"].ToString() != string.Empty)
                    {
                        sch.Date = (dr["Date"].ToString());
                    }

                    if (dr["Time"] != null && dr["Time"].ToString() != string.Empty)
                    {
                        sch.Time = (dr["Time"].ToString());
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = (dr["Revenue"].ToString());
                    }

                    if (dr["StaffName"] != null && dr["StaffName"].ToString() != string.Empty)
                    {
                        sch.StaffName = (dr["StaffName"].ToString());
                    }
                    if (dr["Location"] != null && dr["Location"].ToString() != string.Empty)
                    {
                        sch.Location = (dr["Location"].ToString());
                    }
                    if (dr["CashierName"] != null && dr["CashierName"].ToString() != string.Empty)
                    {
                        sch.CashierName = (dr["CashierName"].ToString());
                    }

                    if (dr["CashierID"] != null && dr["CashierID"].ToString() != string.Empty)
                    {
                        sch.CashierID = (dr["CashierID"].ToString());
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
