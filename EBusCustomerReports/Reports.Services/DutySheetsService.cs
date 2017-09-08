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
    public class DutySheetsService : BaseServices
    {
        public DataSet GetDutySheetDetails(string connKey, string companyName, bool ShowAllOpr, string selectedDate, string duties)
        {
            var result = new DataSet();
            Int32 SelectedDayPos = 0;
            string SelectedDayChar = string.Empty;

            var keyVal = new Dictionary<int, string>();
            keyVal.Add(1, "S");
            keyVal.Add(2, "M");
            keyVal.Add(3, "T");
            keyVal.Add(4, "W");
            keyVal.Add(5, "T");
            keyVal.Add(6, "F");
            keyVal.Add(7, "S");

            if (ShowAllOpr == false && !string.IsNullOrEmpty(selectedDate))
            {
                SelectedDayPos = (int)CustomDateTime.ConvertStringToDateSaFormat(selectedDate).DayOfWeek + 1;
                if (keyVal.ContainsKey(SelectedDayPos))
                {
                    SelectedDayChar = keyVal[SelectedDayPos].ToString();
                }
            }

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));
            try
            {
                myConnection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = myConnection;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "EbusGetDutySheetsDetails";

                    cmd.Parameters.AddWithValue("@SelectedDayChar", SelectedDayChar);
                    cmd.Parameters.AddWithValue("@SelectedDayPos", SelectedDayPos);
                    cmd.Parameters.AddWithValue("@DutyId", duties);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(result);
                    }
                }

                var newColumn = new DataColumn("CompanyName", typeof(string));
                newColumn.DefaultValue = companyName;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("DutySelected", typeof(string));
                newColumn.DefaultValue = !string.IsNullOrEmpty(duties) ? "Duties : " + duties.Replace(",", ", ") : "Duties : All Duties";
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("DateSelected", typeof(string));
                newColumn.DefaultValue = !string.IsNullOrEmpty(selectedDate) ? "Date : " + selectedDate : "All Operating Days";
                result.Tables[0].Columns.Add(newColumn);

                //newColumn = new DataColumn("ContractSelected", typeof(string));
                //newColumn.DefaultValue = !string.IsNullOrEmpty(contractSelected) ? "Contracts : " + contractSelected : "All Contracts";
                //result.Tables[0].Columns.Add(newColumn);

                if (result.Tables[0].Rows.Count == 0)
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

        public DataSet GetTimeTableDetails(string connKey, string companyName, bool ShowAllOpr, string selectedDate, string duties, string contractSelected)
        {
            var result = new DataSet();
            Int32 SelectedDayPos = 0;
            string SelectedDayChar = string.Empty;

            var keyVal = new Dictionary<int, string>();
            keyVal.Add(1, "S");
            keyVal.Add(2, "M");
            keyVal.Add(3, "T");
            keyVal.Add(4, "W");
            keyVal.Add(5, "T");
            keyVal.Add(6, "F");
            keyVal.Add(7, "S");

            if (ShowAllOpr == false && !string.IsNullOrEmpty(selectedDate))
            {
                SelectedDayPos = (int)CustomDateTime.ConvertStringToDateSaFormat(selectedDate).DayOfWeek + 1;
                if (keyVal.ContainsKey(SelectedDayPos))
                {
                    SelectedDayChar = keyVal[SelectedDayPos].ToString();
                }
            }

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));
            try
            {
                myConnection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = myConnection;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "EbusGetTimetableDetails";

                    cmd.Parameters.AddWithValue("@SelectedDayChar", SelectedDayChar);
                    cmd.Parameters.AddWithValue("@SelectedDayPos", SelectedDayPos);
                    cmd.Parameters.AddWithValue("@DutyId", duties);
                    cmd.Parameters.AddWithValue("@Contracts", contractSelected);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(result);
                    }
                }

                var newColumn = new DataColumn("CompanyName", typeof(string));
                newColumn.DefaultValue = companyName;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("DutySelected", typeof(string));
                newColumn.DefaultValue = !string.IsNullOrEmpty(duties) ? "Duties : " + duties : "Duties : All Duties";
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("DateSelected", typeof(string));
                newColumn.DefaultValue = !string.IsNullOrEmpty(selectedDate) ? "Date : " + selectedDate : "Date : All Operating Days";
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("ContractSelected", typeof(string));
                newColumn.DefaultValue = !string.IsNullOrEmpty(contractSelected) ? "Contracts : " + contractSelected : "Contracts : All Contracts";
                result.Tables[0].Columns.Add(newColumn);


                if (result.Tables[0] != null && result.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < result.Tables[0].Rows.Count; i++)
                    {
                        var fromstage = result.Tables[0].Rows[i]["int4_StartStage"].ToString();
                        var fromStageId = result.Tables[0].Rows[i]["StartStage"].ToString();
                        var toStage = result.Tables[0].Rows[i]["int4_EndStage"].ToString();
                        var toStageID = result.Tables[0].Rows[i]["EndStage"].ToString();


                        if (!string.IsNullOrEmpty(fromstage.ToString()))
                        {
                            result.Tables[0].Rows[i]["StartStage"] = string.Format("{1}({0}) - {3}({2})", fromstage, fromStageId, toStage, toStageID);
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
