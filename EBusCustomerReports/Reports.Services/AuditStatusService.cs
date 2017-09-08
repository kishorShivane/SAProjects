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
    public class AuditStatusService : BaseServices
    {
        public List<AuditStatus> GetAuditStatusForGraph(string connKey)
        {
            var result = new List<AuditStatus>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("EbusGetAuditStatusForGraph", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new AuditStatus();
                    if (dr["str_BusId"] != null && dr["str_BusId"].ToString() != string.Empty)
                    {
                        sch.str_BusId = (dr["str_BusId"].ToString());
                    }

                    if (dr["LastestDate"] != null && dr["LastestDate"].ToString() != string.Empty)
                    {
                        var date = dr["LastestDate"].ToString().Split(' ')[0].Split('/');

                        var mont = (date[0].Length == 1 ? "0" + date[0] : date[0]).Trim();
                        var d = (date[1].Length == 1 ? "0" + date[1] : date[1]).Trim();

                        sch.LastestDate = d + "/" + mont + "/" + date[2].Trim();

                        var date1 = CustomDateTime.ConvertStringToDateSaFormat(sch.LastestDate);
                        var date2 = DateTime.Now.Date;//curent date

                        var diff = (int)(date2 - date1).TotalDays;
                        var currentDayNum = (int)DateTime.Now.DayOfWeek; //sunday=0, mon=1, tue=2, wed=3, thr=4, fri=5, sat =6

                        if (currentDayNum == 1)//exclude sat sun chk f
                        {
                            diff = diff - 2;
                        }
                        else if (currentDayNum == 2)
                        {
                            diff = diff - 3;
                        }

                        if (diff < 2) //green
                        {
                            sch.Color = "#0BEA0B";
                            sch.ColorName = "Green";
                            sch.SortingStatus = 3;
                        }
                        else if (diff <= 2) //yellow
                        {
                            sch.Color = "#FBFB00";
                            sch.ColorName = "Yellow";
                            sch.SortingStatus = 2;
                        }
                        else if (diff > 2) //red
                        {
                            sch.ColorName = "Red";
                            sch.Color = "#E21717";
                            sch.SortingStatus = 1;
                        }
                    }

                    if (dr["Str_ETMID"] != null && dr["Str_ETMID"].ToString() != string.Empty)
                    {
                        sch.Str_ETMID = (dr["Str_ETMID"].ToString());
                    }

                    if (dr["ETMType"] != null && dr["ETMType"].ToString() != string.Empty)
                    {
                        sch.ETMType = (dr["ETMType"].ToString());
                    }

                    if (dr["int4_OperatorID"] != null && dr["int4_OperatorID"].ToString() != string.Empty)
                    {
                        sch.int4_OperatorID = (dr["int4_OperatorID"].ToString());
                    }
                    result.Add(sch);
                }
            }

            finally
            {
                myConnection.Close();
            }

            return result.Where(s => s.str_BusId.ToLower().Trim() != "0").OrderBy(s=>s.SortingStatus).ToList();
        }
    }
}
