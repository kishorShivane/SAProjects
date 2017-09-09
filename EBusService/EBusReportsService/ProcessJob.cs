using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using EBusReportsService.Models;

namespace EBusReportsService
{
    public class ProcessJob
    {
        public string AuditCommunicationReport(string conKey)
        {
            try
            {
                Helper.WriteToFile("Service Info: {0} " + "AuditCommunicationReport report processing Started");
                var path = Constants.EBusReportServiceFilesPath + conKey;
                Helper.CreateServiceFileDirectory(path);
                var filePath = path + "\\AuditCommunicationReport_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + ".html";
                List<AuditStatus> model = GetAuditCommunicationStatus(conKey);
                var date = DateTime.Now.Date;
                var greenCount = model.Count(s => s.ColorName.ToLower().Equals("green"));
                var yellowCount = model.Count(s => s.ColorName.ToLower().Equals("yellow"));
                var redCount = model.Count(s => s.ColorName.ToLower().Equals("red"));

                StringBuilder body = new StringBuilder();

                body.Append("<html><title></title><head>");
                body.Append("<script  src='https://code.jquery.com/jquery-3.2.1.min.js' integrity='sha256-hwg4gsxgFZhOsEEamdOYGBf13FyQuiTwlAQgxVSNgt4='  crossorigin='anonymous' ></script> ");
                body.Append("<script  src='https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js' integrity='sha384-Tc5IQib027qvyjSMfHjOMaLkfuWVxZxUPnCJA7l2mCWNIpG9mGCD8wGNIcPD7Txa='  crossorigin='anonymous' ></script> ");
                body.Append("<link href='https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css' rel='stylesheet'>");
                body.Append("<style>  body { -webkit - print - color - adjust: exact; }  td { align - content: center; text - align: center; }");
                body.Append("#legend tr td { text - align: left; } </style>");
                body.Append("</head><body></br></br>");
                body.Append("<div class='row'><table id='legend' style='text-align: left; margin-left: 20px;'><tr><td style='width: 70px;'><b style='color: red;'>Red : </b></td><td>Busses not communicated > 2 days.</td></tr><tr><td style='width: 70px;'><b style='color: yellow;'>Yellow : </b></td><td>Busses communicated 2 days ago.</td></tr><tr><td style='width: 70px;'><b style='color: green;'>Green : </b></td><td>Busses communicated yesterday.</td></tr></table></div><div class='row' style='height:10px;'></div></br><div class='row' style='height:20px;margin-left: 40px;'><table><tr><td style='width:70px;'>Red(" + redCount + ")</td><td style='width:70px;'>Yellow(" + yellowCount + ")</td><td style='width:70px;'>Geeen(" + greenCount + ")</td></tr></table></div>");
                body.Append("</br><div class='row'><table id='statusTable' style='width: 80%;' align='center'><thead><tr style='font-weight: bold; border-bottom: 1px solid black;'> <td>Bus Num</td><td>ETM Num</td><td>ETM Type</td><td>Latest Audit Date</td><td>Status</td></tr></thead><tr style='height: 5px;'><td colspan='4'></td></tr>");
                foreach (var item in model)
                {
                    body.Append("<tr><td style='width: 15%;'>" + item.str_BusId + "</td><td style='width: 15%;'>" + item.Str_ETMID + "</td><td style='width: 15%;'>" + item.ETMType + "</td><td style='width: 25%;'>" + item.LastestDate + "</td><td style='width:25%; background-color:" + item.Color + "'><label style='height:100%;width:100%;background-color:" + item.Color + ";color:" + item.Color + ";'>" + item.ColorName + "</label></td></tr><tr style='height: 5px;'><td colspan='4'></td></tr>");
                }
                body.Append("</br></br></body></html>");

                Helper.WriteToHtml(filePath, body.ToString());

                Helper.WriteToFile("Service Info: {0} " + "AuditCommunicationReport created");

                return filePath;
                //if (!File.Exists(filePath))
                //{
                //    Helper.WriteToFile("Service Info: {0} " + "AuditCommunicationReport file exist check in progress");
                //    Thread.Sleep(100);
                //}
                //Helper.WriteToFile("Service Info: {0} " + "AuditCommunicationReport file exist check passed");
                //EmailHelper.SendEmail(filePath, true);
                //Helper.WriteToFile("Service Info: {0} " + "AuditCommunicationReport mailed");
            }
            catch (Exception ex)
            {
                Helper.WriteToFile("Service Error on: {0} " + ex.Message + ex.StackTrace);
                throw;
            }

        }
        public string ScheduledVsNoOperated(string conKey, bool isAtamalang)
        {
            try
            {
                Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated report processing Started");
                var path = Constants.EBusReportServiceFilesPath + conKey;
                Helper.CreateServiceFileDirectory(path);
                var filePath = path + "\\ScheduledNotOperated_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + ".pdf";
                var ds = GetScheduledVsOperatedReport(conKey, DateTime.Now.AddDays(-1), "EbusScheduledbutnotWorked", Helper.GetCustomerDisplayName(conKey));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated report data retrived");
                    CrystalDecisions.CrystalReports.Engine.ReportDocument cryRpt = new CrystalDecisions.CrystalReports.Engine.ReportDocument();
                    if (isAtamalang)
                        cryRpt.Load(Constants.SchVsNotOperReportAtamalangPath);
                    else
                        cryRpt.Load(Constants.SchVsNotOperReportPath);

                    Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated report path " + Constants.SchVsNotOperReportPath);

                    CrystalDecisions.Shared.ExportOptions CrExportOptions;
                    Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated report ExportOptions set");
                    DiskFileDestinationOptions CrDiskFileDestinationOptions = new DiskFileDestinationOptions();
                    Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated report DiskFileDestinationOptions set");
                    PdfRtfWordFormatOptions CrFormatTypeOptions = new PdfRtfWordFormatOptions();
                    Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated report PdfRtfWordFormatOptions set");
                    CrDiskFileDestinationOptions.DiskFileName = filePath;
                    Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated report DiskFileName");
                    cryRpt.DataSourceConnections.Clear();
                    Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated report cleared");
                    cryRpt.SetDataSource(ds.Tables[0]);
                    Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated report SetDataSource");
                    CrExportOptions = cryRpt.ExportOptions;
                    {
                        CrExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                        CrExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
                        CrExportOptions.DestinationOptions = CrDiskFileDestinationOptions;
                        CrExportOptions.FormatOptions = CrFormatTypeOptions;
                    }
                    Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated report all setting done");
                    cryRpt.Export();
                    Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated created");

                    return filePath;
                    //if (!File.Exists(filePath))
                    //{
                    //    Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated file exist check in progress");
                    //    Thread.Sleep(100);
                    //}
                    //Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated file exist check passed");
                    //EmailHelper.SendEmail(filePath, false);
                    //Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated created");
                }
                else
                { Helper.WriteToFile("Service Info: {0} " + "ScheduledVsNoOperated report data does not exist for the date"); }
            }
            catch (Exception ex)
            {
                Helper.WriteToFile("Service Error on: {0} " + ex.Message + ex.StackTrace);
                throw;
            }
            return "";
        }

        public string GetConnectionString(string connecKey)
        {
            return ConfigurationSettings.AppSettings[connecKey];
        }

        public List<AuditStatus> GetAuditCommunicationStatus(string conKey)
        {
            var result = new List<AuditStatus>();

            var myConnection = new SqlConnection(GetConnectionString(conKey));

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
            catch (Exception ex)
            {
                Helper.WriteToFile("Service Error on: {0} " + ex.Message + ex.StackTrace);
                throw;
            }
            finally
            {
                myConnection.Close();
            }

            return result.Where(s => s.str_BusId.ToLower().Trim() != "0").OrderBy(s => s.SortingStatus).ToList();
        }

        public DataSet GetScheduledVsOperatedReport(string conKey, DateTime today, string spName, string companyName)
        {
            var ds = new DataSet();
            try
            {
                var table1 = ScheduleVsOperatedDataSet();

                //filter details
                var filterDateRange = string.Format(" {0}: {1} to {2}", "Date Range", today.ToString("dd-MM-yyyy"), today.ToString("dd-MM-yyyy"));

                var filterContractsRange = " Contracts: Filter Not Selected";
                var filterDuties = " Duties: Filter Not Selected";

                var result = GetScheduledVsOperatedData(conKey, today, spName);
                var filteredResult = new List<SchVsWorked>();

                result.ForEach(s =>
                {
                    var multiplePairExistRes = result.Where(r => r.str4_JourneyNo == s.str4_JourneyNo
                        && r.int4_OperatorID == s.int4_OperatorID && r.int4_DutyId == s.int4_DutyId
                        && r.dateSelected == s.dateSelected).Count();

                    if (multiplePairExistRes > 1)
                    {
                        var multiplePairExistFil = filteredResult.Where(r => r.str4_JourneyNo == s.str4_JourneyNo
                                            && r.int4_OperatorID == s.int4_OperatorID && r.int4_DutyId == s.int4_DutyId
                                            && r.dateSelected == s.dateSelected).Count();

                        if (multiplePairExistRes > 1)
                        {
                            filteredResult.RemoveAll(r => r.str4_JourneyNo == s.str4_JourneyNo
                                            && r.int4_OperatorID == s.int4_OperatorID && r.int4_DutyId == s.int4_DutyId
                                            && r.dateSelected == s.dateSelected
                                            && r.Int_TotalPassengers == 0); //remove all zero

                            var multiplePairExistFilNonZeroPsg = filteredResult.Where(r => r.str4_JourneyNo == s.str4_JourneyNo
                                        && r.int4_OperatorID == s.int4_OperatorID && r.int4_DutyId == s.int4_DutyId
                                        && r.dateSelected == s.dateSelected).Count();
                            if (multiplePairExistFilNonZeroPsg > 0 && s.Int_TotalPassengers <= 0)
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
                        res.float_OperDistance = res.TripStatus != null && res.TripStatus.ToLower() == "worked" ? res.float_Distance : 0;
                        table1.Rows.Add(
                                    res.dateSelected,
                                    res.int4_DutyId,
                                    res.str4_JourneyNo,
                                    res.DOTRouteNumber,
                                    res.float_Distance,
                                    res.str7_Contract,
                                    res.dat_StartTime,
                                    res.dat_EndTime,
                                    res.int4_OperatorID,
                                    res.dat_JourneyStartTime,
                                    res.dat_JourneyStopTime,
                                    res.int4_JourneyRevenue,
                                    res.int4_JourneyTickets,
                                    res.int4_JourneyPasses,
                                    res.int4_JourneyNonRevenue,
                                    res.int4_JourneyTransfer,
                                    res.TripStatus,
                                    res.str_BusNr,
                                    res.Int_TotalPassengers,
                                    res.float_OperDistance,
                                    companyName,
                                    filterDateRange,
                                    filterContractsRange,
                                    filterDuties
                            );
                    }
                }
                else
                {
                    //Empty row for blank report.
                    DataRow dr = table1.NewRow();
                    dr["filterDuties"] = filterDuties;
                    dr["DateRangeFilter"] = filterDateRange;
                    dr["filterContractsRange"] = filterContractsRange;
                    dr["companyName"] = companyName;

                    table1.Rows.Add(dr);
                }

                ds.Tables.Add(table1);
            }
            catch (Exception ex)
            {
                Helper.WriteToFile("Service Error on: {0} " + ex.Message + ex.StackTrace);
                throw;
            }
            return ds;
        }

        public List<SchVsWorked> GetScheduledVsOperatedData(string conKey, DateTime today, string spName)
        {
            var schs = new List<SchVsWorked>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand(spName, myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@DutyId", "");
                cmd.Parameters.AddWithValue("@Contracts", "");

                var dateRangeString = GetDateRangeString(today.ToString("dd-MM-yyyy"), today.ToString("dd-MM-yyyy"));

                if (dateRangeString == string.Empty)
                {
                    return new List<SchVsWorked>();
                }

                cmd.Parameters.AddWithValue("@datet", dateRangeString);
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new SchVsWorked();


                    if (dr["dateSelected"] != null && dr["dateSelected"].ToString() != string.Empty)
                    {
                        var datePart = dr["dateSelected"].ToString().Split(' ')[0];
                        var date = datePart.Split('/');
                        var mont = (date[0].Length == 1 ? "0" + date[0] : date[0]).Trim();

                        sch.dateSelected = (date[1].Length == 1 ? "0" + date[1] : date[1]).Trim() + "/" + mont + "/" + date[2].Trim();
                    }

                    if (dr["int4_DutyId"] != null && dr["int4_DutyId"].ToString() != string.Empty)
                    {
                        sch.int4_DutyId = dr["int4_DutyId"].ToString();
                    }

                    //if (isHomeScreen)
                    //{
                    //    if (dr["bit_ReveOrDead"] != null && dr["bit_ReveOrDead"].ToString() != string.Empty)
                    //    {
                    //        sch.bit_ReveOrDead = Convert.ToBoolean(dr["bit_ReveOrDead"]);
                    //    }
                    //}

                    if (dr["str4_JourneyNo"] != null && dr["str4_JourneyNo"].ToString() != string.Empty)
                    {
                        sch.str4_JourneyNo = dr["str4_JourneyNo"].ToString();
                    }

                    if (dr["DOTRouteNumber"] != null && dr["DOTRouteNumber"].ToString() != string.Empty)
                    {
                        sch.DOTRouteNumber = dr["DOTRouteNumber"].ToString();
                    }

                    if (dr["float_Distance"] != null && dr["float_Distance"].ToString() != string.Empty)
                    {
                        sch.float_Distance = float.Parse(dr["float_Distance"].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                    }

                    if (dr["str7_Contract"] != null && dr["str7_Contract"].ToString() != string.Empty)
                    {
                        sch.str7_Contract = dr["str7_Contract"].ToString();
                    }

                    if (dr["dat_StartTime"] != null && dr["dat_StartTime"].ToString() != string.Empty) //from schedule table => sched time
                    {
                        sch.dat_StartTime = dr["dat_StartTime"].ToString();
                    }

                    if (dr["dat_EndTime"] != null && dr["dat_EndTime"].ToString() != string.Empty)
                    {
                        sch.dat_EndTime = dr["dat_EndTime"].ToString();
                    }

                    if (dr["int4_OperatorID"] != null && dr["int4_OperatorID"].ToString() != string.Empty)
                    {
                        sch.int4_OperatorID = dr["int4_OperatorID"].ToString();
                    }

                    if (dr["dat_JourneyStartTime"] != null && dr["dat_JourneyStartTime"].ToString() != string.Empty) //from operated table=>used as book on
                    {
                        sch.dat_JourneyStartTime = dr["dat_JourneyStartTime"].ToString();
                    }

                    if (dr["dat_JourneyStopTime"] != null && dr["dat_JourneyStopTime"].ToString() != string.Empty)
                    {
                        sch.dat_JourneyStopTime = dr["dat_JourneyStopTime"].ToString();
                    }

                    if (dr["int4_JourneyRevenue"] != null && dr["int4_JourneyRevenue"].ToString() != string.Empty)
                    {
                        sch.int4_JourneyRevenue = dr["int4_JourneyRevenue"].ToString();
                    }

                    if (dr["int4_JourneyTickets"] != null && dr["int4_JourneyTickets"].ToString() != string.Empty)
                    {
                        sch.int4_JourneyTickets = dr["int4_JourneyTickets"].ToString();
                    }

                    if (dr["int4_JourneyPasses"] != null && dr["int4_JourneyPasses"].ToString() != string.Empty)
                    {
                        sch.int4_JourneyPasses = dr["int4_JourneyPasses"].ToString();
                    }

                    if (dr["int4_JourneyNonRevenue"] != null && dr["int4_JourneyNonRevenue"].ToString() != string.Empty)
                    {
                        sch.int4_JourneyNonRevenue = dr["int4_JourneyNonRevenue"].ToString();
                    }

                    if (dr["int4_JourneyTransfer"] != null && dr["int4_JourneyTransfer"].ToString() != string.Empty)
                    {
                        sch.int4_JourneyTransfer = dr["int4_JourneyTransfer"].ToString();
                    }

                    if (dr["TripStatus"] != null && dr["TripStatus"].ToString() != string.Empty)
                    {
                        sch.TripStatus = dr["TripStatus"].ToString();
                    }

                    //if (isformE && dr["routeName"] != null && dr["routeName"].ToString() != string.Empty)//only for Form-E
                    //{
                    //    sch.routeName = dr["routeName"].ToString();
                    //}

                    if (dr["Int_TotalPassengers"] != null && dr["Int_TotalPassengers"].ToString() != string.Empty)
                    {
                        sch.Int_TotalPassengers = Convert.ToInt64(dr["Int_TotalPassengers"].ToString());
                    }

                    if (dr["str_BusNr"] != null && dr["str_BusNr"].ToString() != string.Empty)
                    {
                        sch.str_BusNr = dr["str_BusNr"].ToString().Trim();
                    }

                    //if (dr["IsPosition"] != null && dr["IsPosition"].ToString() != string.Empty)
                    //{
                    //    sch.IsPosition = Convert.ToBoolean(dr["IsPosition"]);
                    //}

                    schs.Add(sch);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                myConnection.Close();
            }
            return schs;
        }

        public string GetDateRangeString(string fromDate, string tillDate)
        {
            var result = string.Empty;

            var fromDateTime = CustomDateTime.ConvertStringToDateSaFormat(fromDate);
            var tillDateTime = CustomDateTime.ConvertStringToDateSaFormat(tillDate);

            if (fromDateTime.Date == tillDateTime.Date)
            {
                result = fromDateTime.ToString("yyyy-MM-dd");
            }
            else if (tillDateTime.Date > fromDateTime.Date)
            {
                for (var i = fromDateTime.Date; i <= tillDateTime.Date; i = i.AddDays(1).Date)
                {
                    if (result == string.Empty)
                    {
                        result = i.Date.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        result += "," + i.Date.ToString("yyyy-MM-dd");
                    }
                }
            }
            return result;
        }

        public DataTable ScheduleVsOperatedDataSet()
        {
            var table1 = new DataTable("SchOpr");

            table1.Columns.Add("dateSelected"); //0

            table1.Columns.Add("int4_DutyId"); //1
            table1.Columns[1].DataType = typeof(double);

            table1.Columns.Add("str4_JourneyNo");//2

            table1.Columns.Add("DOTRouteNumber");//3

            table1.Columns.Add("float_Distance");//4
            table1.Columns[4].DataType = typeof(double);

            table1.Columns.Add("str7_Contract");//5
            table1.Columns.Add("dat_StartTime");//6
            table1.Columns.Add("dat_EndTime");//7
            table1.Columns.Add("int4_OperatorID");//8
            table1.Columns.Add("dat_JourneyStartTime");//9
            table1.Columns.Add("dat_JourneyStopTime");//10

            table1.Columns.Add("int4_JourneyRevenue");//11
            table1.Columns[11].DataType = typeof(double);

            table1.Columns.Add("int4_JourneyTickets");//12
            table1.Columns[12].DataType = typeof(double);

            table1.Columns.Add("int4_JourneyPasses");//13
            table1.Columns[13].DataType = typeof(double);

            table1.Columns.Add("int4_JourneyNonRevenue");//14
            table1.Columns[14].DataType = typeof(double);

            table1.Columns.Add("int4_JourneyTransfer");//15
            table1.Columns[15].DataType = typeof(double);

            table1.Columns.Add("TripStatus");//16
            table1.Columns.Add("str_BusNr");//17
            table1.Columns.Add("Int_TotalPassengers");//18
            table1.Columns[18].DataType = typeof(double);

            table1.Columns.Add("float_OperDistance");//19
            table1.Columns[19].DataType = typeof(double);

            table1.Columns.Add("companyName");//20 
            table1.Columns.Add("DateRangeFilter");//21 
            table1.Columns.Add("filterContractsRange");//22
            table1.Columns.Add("filterDuties");//23 
            table1.Columns.Add("filterTimeSelected");//24

            return table1;
        }
    }
}
