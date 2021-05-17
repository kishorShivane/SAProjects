using Reports.Services.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;


namespace Reports.Services
{
    public class OperationalSummaryService : BaseServices
    {
        public OperationalSummaryReportFilter GetOperationalSummaryReportFilter(string connKey)
        {
            OperationalSummaryReportFilter model = new OperationalSummaryReportFilter
            {
                Reports = GetValidReportTypes()
            };
            return model;
        }

        private List<SelectListItem> GetValidReportTypes()
        {
            return new List<SelectListItem> { new SelectListItem() { Text = "Revenue By Duty", Value = "RevenueByDuty" } ,
            new SelectListItem() { Text = "Revenue By Driver", Value = "RevenueByDriver" },
            new SelectListItem() { Text = "Revenue By Route", Value = "RevenueByRoute" },
            new SelectListItem() { Text = "Inspections By Inspector", Value = "InspectionsByInspector" },
            new SelectListItem() { Text = "Busses Not Inspected", Value = "BussesNotInspected" } };
        }

        public DataSet GetOperationalSummaryDataSet(string conKey, OperationalSummaryReportFilter filters, string companyName)
        {
            var result = new DataSet();
            SqlConnection myConnection = new SqlConnection(GetConnectionString(conKey));
            try
            {
                myConnection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = myConnection;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "EbusOperationalSummaryReport";

                    cmd.Parameters.AddWithValue("@fromDate", filters.StartDate);
                    cmd.Parameters.AddWithValue("@toDate", filters.EndDate);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(result);
                    }
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
