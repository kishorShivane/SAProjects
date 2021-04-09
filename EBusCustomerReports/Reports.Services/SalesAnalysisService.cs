using Reports.Services.Helpers;
using Reports.Services.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Reports.Services
{
    public class SalesAnalysisService : BaseServices
    {
        public MonthyRevenueDataGraph GetMonthRevenueData(string connKey, string monthsSelected, string yearsSelected)
        {
            List<MonthyRevenueData> data = new List<MonthyRevenueData>();
            MonthyRevenueDataGraph graphData = new MonthyRevenueDataGraph();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand("EbusGetMonthRevenueData", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MonthsSelected", string.IsNullOrEmpty(monthsSelected) ? "1,2,3,4,5,6,7,8,9,10,11,12" : monthsSelected);
                cmd.Parameters.AddWithValue("@YearSelected", yearsSelected);

                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    MonthyRevenueData sch = new MonthyRevenueData();

                    if (dr["MonthName"] != null && dr["MonthName"].ToString() != string.Empty)
                    {
                        sch.MonthName = (dr["MonthName"].ToString());
                    }
                    if (dr["MonthNum"] != null && dr["MonthNum"].ToString() != string.Empty)
                    {
                        sch.MonthNum = Convert.ToInt32(dr["MonthNum"]);
                    }

                    if (dr["int4_DutyID"] != null && dr["int4_DutyID"].ToString() != string.Empty)
                    {
                        sch.int4_DutyID = Convert.ToInt32(dr["int4_DutyID"]);
                    }

                    if (dr["int4_DutyRevenue"] != null && dr["int4_DutyRevenue"].ToString() != string.Empty)
                    {
                        sch.int4_DutyRevenue = Convert.ToDecimal(dr["int4_DutyRevenue"]);
                    }
                    if (dr["int4_DutyNonRevenue"] != null && dr["int4_DutyNonRevenue"].ToString() != string.Empty)
                    {
                        sch.int4_DutyNonRevenue = Convert.ToDecimal(dr["int4_DutyNonRevenue"]);
                    }

                    data.Add(sch);
                }

                List<IGrouping<string, MonthyRevenueData>> seller = data.Where(s => s.int4_DutyID == 8000).GroupBy(s => s.MonthName).ToList();
                List<IGrouping<string, MonthyRevenueData>> driver = data.Where(s => s.int4_DutyID != 8000).GroupBy(s => s.MonthName).ToList();

                graphData.SellerRevMonth = seller.Select(s => new MonthyRevenueData { MonthName = s.Key, MonthNum = s.First().MonthNum, int4_DutyID = s.First().int4_DutyID, int4_DutyNonRevenue = s.Sum(g => g.int4_DutyNonRevenue), int4_DutyRevenue = s.Sum(g => g.int4_DutyRevenue) }).OrderBy(s => s.MonthNum).ToList();
                graphData.DriverRevMonth = driver.Select(s => new MonthyRevenueData { MonthName = s.Key, MonthNum = s.First().MonthNum, int4_DutyID = s.First().int4_DutyID, int4_DutyNonRevenue = s.Sum(g => g.int4_DutyNonRevenue), int4_DutyRevenue = s.Sum(g => g.int4_DutyRevenue) }).OrderBy(s => s.MonthNum).ToList();
                graphData.SellerRevMonthYear = graphData.SellerRevMonth.Sum(s => s.int4_DutyRevenue);
                graphData.DriverRevMonthYear = graphData.DriverRevMonth.Sum(s => s.int4_DutyRevenue);
            }


            finally
            {
                myConnection.Close();
            }

            return graphData;
        }

        public DataSet GetYearlyBreakDownDataSet(string connKey, string companyName, YearlyBreakDownFilter filter)
        {
            DataSet ds = new DataSet();
            DataTable table1 = YearlyBreakDownDataset();
            List<YearlyData> finalData = new List<YearlyData>();

            string[] fromMonth = filter.FromMonthSelected == null ? new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" } : filter.FromMonthSelected;
            List<string> fromMonthList = GetMonths().Where(s => fromMonth.Contains(s.Value)).Select(s => s.Text).ToList();
            string fromMonthNames = string.Join(",", fromMonthList);

            string[] toMonth = filter.ToMonthSelected == null ? new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" } : filter.ToMonthSelected;
            List<string> toMonthList = GetMonths().Where(s => toMonth.Contains(s.Value)).Select(s => s.Text).ToList();
            string toMonthNames = string.Join(",", toMonthList);

            string title = string.Format("Months Selected: {0} - {1} vs {2} - {3} ", filter.ToYearSelected, toMonthNames, filter.FromYearSelected, fromMonthNames);

            string classFilter = filter.ClassesSelected == null ? "Class Selected: All Classes" : "Class Selected: " + string.Join(",", filter.ClassesSelected);
            List<int> classesSelected = new List<int>();
            classesSelected = filter.ClassesSelected != null ? filter.ClassesSelected.Select(s => Convert.ToInt32(s)).ToList() : GetAllClasses(connKey).Select(s => Convert.ToInt32(s.Value)).ToList();

            List<YearlyBreakDownData> data = GetYearlyBreakDownData(connKey, filter);

            foreach (int cls in classesSelected)
            {
                YearlyBreakDownData cData = data.Where(s => s.ClassID.Equals(cls)).FirstOrDefault();
                List<YearlyBreakDownData> year2Data = data.Where(s => s.ClassID.Equals(cls) && s.Year.Equals(Convert.ToInt32(filter.ToYearSelected))).ToList();
                List<YearlyBreakDownData> year1Data = data.Where(s => s.ClassID.Equals(cls) && s.Year.Equals(Convert.ToInt32(filter.FromYearSelected))).ToList();
                YearlyData item = new YearlyData();

                if (cData != null)
                {
                    item.MonthsSelected = title;
                    item.CompanySelected = companyName;
                    item.ClassFilterSelected = classFilter;
                    item.Year2Selected = Convert.ToInt32(filter.ToYearSelected);
                    item.Year1Selected = Convert.ToInt32(filter.FromYearSelected);
                    item.Class = cData.ClassNameFull;

                    item.Year2Revenue = year2Data.Sum(s => s.Revenue);
                    item.Year1Revenue = year1Data.Sum(s => s.Revenue);
                    item.RevenueDiff = item.Year2Revenue - item.Year1Revenue;
                    item.RevenueDiffPer = item.Year1Revenue > 0 ? item.RevenueDiff / item.Year1Revenue : item.RevenueDiff / 100.00;

                    item.Year2NonRevenue = year2Data.Sum(s => s.NonRevenue);
                    item.Year1NonRevenue = year1Data.Sum(s => s.NonRevenue);
                    item.NonRevenueDiff = item.Year2NonRevenue - item.Year1NonRevenue;
                    item.NonRevenueDiffPer = item.Year1NonRevenue > 0 ? item.NonRevenueDiff / item.Year1NonRevenue : item.NonRevenueDiff / 100.00;
                    item.Year2Passenger = year2Data.Sum(s => s.Passengers);
                    item.Year1Passenger = year1Data.Sum(s => s.Passengers);
                    item.PassengerDiff = item.Year2Passenger - item.Year1Passenger;
                    item.PassengerDiffPer = item.Year1Passenger > 0 ? item.PassengerDiff / item.Year1Passenger : item.PassengerDiff / 100.00;
                    finalData.Add(item);
                }
            }

            double year2RevSum = finalData.Sum(s => s.Year2Revenue);
            double Year1RevSum = finalData.Sum(s => s.Year1Revenue);
            double RevDiffSum = finalData.Sum(s => s.RevenueDiff);
            double Year2NonRevSum = finalData.Sum(s => s.Year2NonRevenue);
            double Year1NonRevsum = finalData.Sum(s => s.Year1NonRevenue);
            double NonRevDiffSum = finalData.Sum(s => s.NonRevenueDiff);
            int Year2PsngSum = finalData.Sum(s => s.Year2Passenger);
            int Year1PsngSum = finalData.Sum(s => s.Year1Passenger);
            double PsngDiffSum = finalData.Sum(s => s.PassengerDiff);

            if (finalData.Any())
            {
                foreach (YearlyData item in finalData)
                {
                    table1.Rows.Add(item.MonthsSelected, item.CompanySelected, item.ClassFilterSelected, item.Year2Selected, item.Year1Selected, item.Class,
                        item.Year2Revenue, item.Year1Revenue, item.RevenueDiff, Math.Round(item.RevenueDiffPer, 2) * 100, item.Year2NonRevenue, item.Year1NonRevenue, item.NonRevenueDiff,
                        Math.Round(item.NonRevenueDiffPer, 2) * 100, item.Year2Passenger, item.Year1Passenger, item.PassengerDiff, Math.Round(item.PassengerDiffPer, 2) * 100, 0,
                        year2RevSum,
                        Year1RevSum,
                        RevDiffSum,
                        Year2NonRevSum,
                        Year1NonRevsum,
                        NonRevDiffSum,
                        Year2PsngSum,
                        Year1PsngSum,
                        PsngDiffSum, " ", " ", " "
                        );
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["MonthsSelected"] = title;
                dr["CompanySelected"] = companyName;
                dr["ClassFilterSelected"] = classFilter;
                dr["Year2Selected"] = Convert.ToInt32(filter.ToYearSelected);
                dr["Year1Selected"] = Convert.ToInt32(filter.FromYearSelected);

                table1.Rows.Add(dr);
            }

            ds.Tables.Add(table1);
            return ds;
        }

        private List<YearlyBreakDownData> GetYearlyBreakDownData(string connKey, YearlyBreakDownFilter filter)
        {
            List<YearlyBreakDownData> data = new List<YearlyBreakDownData>();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand("EbusYearlyBreakDownData", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@classIds", filter.ClassesSelected == null ? "" : string.Join(",", filter.ClassesSelected));
                cmd.Parameters.AddWithValue("@FromMonths", filter.FromMonthSelected == null ? string.Join(",", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" }) : string.Join(",", filter.FromMonthSelected));
                cmd.Parameters.AddWithValue("@ToMonths", filter.ToMonthSelected == null ? string.Join(",", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" }) : string.Join(",", filter.ToMonthSelected));
                cmd.Parameters.AddWithValue("@FromYear", filter.FromYearSelected);
                cmd.Parameters.AddWithValue("@ToYear", filter.ToYearSelected);

                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    YearlyBreakDownData sch = new YearlyBreakDownData();

                    if (dr["ClassName"] != null && dr["ClassName"].ToString() != string.Empty)
                    {
                        sch.ClassName = (dr["ClassName"].ToString());
                    }
                    if (dr["ClassNameFull"] != null && dr["ClassNameFull"].ToString() != string.Empty)
                    {
                        sch.ClassNameFull = (dr["ClassNameFull"].ToString());
                    }
                    if (dr["Year"] != null && dr["Year"].ToString() != string.Empty)
                    {
                        sch.Year = Convert.ToInt32(dr["Year"].ToString());
                    }
                    if (dr["Passengers"] != null && dr["Passengers"].ToString() != string.Empty)
                    {
                        sch.Passengers = Convert.ToInt32(dr["Passengers"].ToString());
                    }
                    if (dr["ClassID"] != null && dr["ClassID"].ToString() != string.Empty)
                    {
                        sch.ClassID = Convert.ToInt32(dr["ClassID"].ToString());
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = Convert.ToDouble(dr["Revenue"].ToString());
                    }
                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = Convert.ToDouble(dr["NonRevenue"].ToString());
                    }
                    data.Add(sch);
                }
            }

            finally
            {
                myConnection.Close();
            }

            return data;
        }

        public DataSet GetYearlyBreakDownByRouteDataSet(string connKey, string companyName, YearlyBreakDownFilter filter)
        {
            DataSet ds = new DataSet();
            DataTable table1 = YearlyBreakDownDataset();
            List<YearlyData> finalData = new List<YearlyData>();

            string[] fromMonth = filter.FromMonthSelected == null ? new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" } : filter.FromMonthSelected;
            List<string> fromMonthList = GetMonths().Where(s => fromMonth.Contains(s.Value)).Select(s => s.Text).ToList();
            string fromMonthNames = string.Join(",", fromMonthList);

            string[] toMonth = filter.ToMonthSelected == null ? new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" } : filter.ToMonthSelected;
            List<string> toMonthList = GetMonths().Where(s => toMonth.Contains(s.Value)).Select(s => s.Text).ToList();
            string toMonthNames = string.Join(",", toMonthList);

            string title = string.Format("Months Selected: {0} - {1} vs {2} - {3} ", filter.ToYearSelected, toMonthNames, filter.FromYearSelected, fromMonthNames);

            string classFilter = filter.ClassesSelected == null ? "Class Selected: All Classes" : "Class Selected: " + string.Join(",", filter.ClassesSelected);
            string routesFilter = filter.RoutesSelected == null ? "Routes Selected: All Routes" : "Routes Selected: " + string.Join(",", filter.RoutesSelected);

            List<int> classesSelected = new List<int>();
            classesSelected = filter.ClassesSelected != null ? filter.ClassesSelected.Select(s => Convert.ToInt32(s)).ToList() : GetAllClasses(connKey).Select(s => Convert.ToInt32(s.Value)).ToList();

            List<YearlyBreakDownData> data = EbusYearlyBreakDownDataByRoute(connKey, filter);

            List<IGrouping<string, YearlyBreakDownData>> routeGroupData = data.GroupBy(s => s.str_RouteID).ToList();

            if (routeGroupData.Any())
            {
                foreach (IGrouping<string, YearlyBreakDownData> gd in routeGroupData)
                {
                    string routeId = gd.Key;
                    YearlyBreakDownData routeNameObj = gd.Where(s => !string.IsNullOrEmpty(s.RouteName)).FirstOrDefault();
                    string routeName = string.Empty;

                    if (routeNameObj != null)
                    {
                        routeName = routeNameObj.RouteName;
                    }

                    finalData = new List<YearlyData>();

                    foreach (int cls in classesSelected)
                    {
                        YearlyBreakDownData cData = gd.Where(s => s.ClassID.Equals(cls)).FirstOrDefault();
                        List<YearlyBreakDownData> year2Data = gd.Where(s => s.ClassID.Equals(cls) && s.Year.Equals(Convert.ToInt32(filter.ToYearSelected))).ToList();
                        List<YearlyBreakDownData> year1Data = gd.Where(s => s.ClassID.Equals(cls) && s.Year.Equals(Convert.ToInt32(filter.FromYearSelected))).ToList();
                        YearlyData item = new YearlyData();

                        if (cData != null)
                        {
                            item.MonthsSelected = title;
                            item.CompanySelected = companyName;
                            item.ClassFilterSelected = classFilter;
                            item.Year2Selected = Convert.ToInt32(filter.ToYearSelected);
                            item.Year1Selected = Convert.ToInt32(filter.FromYearSelected);
                            item.Class = cData.ClassName;

                            item.Year2Revenue = year2Data.Sum(s => s.Revenue);
                            item.Year1Revenue = year1Data.Sum(s => s.Revenue);
                            item.RevenueDiff = item.Year2Revenue - item.Year1Revenue;
                            item.RevenueDiffPer = item.Year1Revenue > 0 ? item.RevenueDiff / item.Year1Revenue : item.RevenueDiff / 100.00;

                            item.Year2NonRevenue = year2Data.Sum(s => s.NonRevenue);
                            item.Year1NonRevenue = year1Data.Sum(s => s.NonRevenue);
                            item.NonRevenueDiff = item.Year2NonRevenue - item.Year1NonRevenue;
                            item.NonRevenueDiffPer = item.Year1NonRevenue > 0 ? item.NonRevenueDiff / item.Year1NonRevenue : item.NonRevenueDiff / 100.00;
                            item.Year2Passenger = year2Data.Sum(s => s.Passengers);
                            item.Year1Passenger = year1Data.Sum(s => s.Passengers);
                            item.PassengerDiff = item.Year2Passenger - item.Year1Passenger;
                            item.PassengerDiffPer = item.Year1Passenger > 0 ? item.PassengerDiff / item.Year1Passenger : item.PassengerDiff / 100.00;
                            finalData.Add(item);
                        }
                    }

                    double year2RevSum = finalData.Sum(s => s.Year2Revenue);
                    double Year1RevSum = finalData.Sum(s => s.Year1Revenue);
                    double RevDiffSum = finalData.Sum(s => s.RevenueDiff);
                    double Year2NonRevSum = finalData.Sum(s => s.Year2NonRevenue);
                    double Year1NonRevsum = finalData.Sum(s => s.Year1NonRevenue);
                    double NonRevDiffSum = finalData.Sum(s => s.NonRevenueDiff);
                    int Year2PsngSum = finalData.Sum(s => s.Year2Passenger);
                    int Year1PsngSum = finalData.Sum(s => s.Year1Passenger);
                    double PsngDiffSum = finalData.Sum(s => s.PassengerDiff);


                    foreach (YearlyData item in finalData)
                    {
                        table1.Rows.Add(item.MonthsSelected, item.CompanySelected, item.ClassFilterSelected, item.Year2Selected, item.Year1Selected, item.Class,
                            item.Year2Revenue, item.Year1Revenue, item.RevenueDiff, Math.Round(item.RevenueDiffPer, 2) * 100, item.Year2NonRevenue, item.Year1NonRevenue, item.NonRevenueDiff,
                            Math.Round(item.NonRevenueDiffPer, 2) * 100, item.Year2Passenger, item.Year1Passenger, item.PassengerDiff, Math.Round(item.PassengerDiffPer, 2) * 100, 0,
                            year2RevSum,
                            Year1RevSum,
                            RevDiffSum,
                            Year2NonRevSum,
                            Year1NonRevsum,
                            NonRevDiffSum,
                            Year2PsngSum,
                            Year1PsngSum,
                            PsngDiffSum,
                            routesFilter,
                            routeId,
                            routeName
                            );
                    }
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["MonthsSelected"] = title;
                dr["CompanySelected"] = companyName;
                dr["ClassFilterSelected"] = classFilter;
                dr["Year2Selected"] = Convert.ToInt32(filter.ToYearSelected);
                dr["Year1Selected"] = Convert.ToInt32(filter.FromYearSelected);
                dr["RouteFilter"] = routesFilter;
                table1.Rows.Add(dr);
            }


            ds.Tables.Add(table1);
            return ds;
        }

        private List<YearlyBreakDownData> EbusYearlyBreakDownDataByRoute(string connKey, YearlyBreakDownFilter filter)
        {
            List<YearlyBreakDownData> data = new List<YearlyBreakDownData>();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand("EbusYearlyBreakDownDataByRoute", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@classIds", filter.ClassesSelected == null ? "" : string.Join(",", filter.ClassesSelected));
                cmd.Parameters.AddWithValue("@FromMonths", filter.FromMonthSelected == null ? string.Join(",", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" }) : string.Join(",", filter.FromMonthSelected));
                cmd.Parameters.AddWithValue("@ToMonths", filter.ToMonthSelected == null ? string.Join(",", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" }) : string.Join(",", filter.ToMonthSelected));
                cmd.Parameters.AddWithValue("@FromYear", filter.FromYearSelected);
                cmd.Parameters.AddWithValue("@ToYear", filter.ToYearSelected);
                cmd.Parameters.AddWithValue("@RouteIds", filter.RoutesSelected == null ? "" : string.Join(",", filter.RoutesSelected));

                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    YearlyBreakDownData sch = new YearlyBreakDownData();

                    if (dr["ClassName"] != null && dr["ClassName"].ToString() != string.Empty)
                    {
                        sch.ClassName = (dr["ClassName"].ToString());
                    }
                    if (dr["ClassNameFull"] != null && dr["ClassNameFull"].ToString() != string.Empty)
                    {
                        sch.ClassNameFull = (dr["ClassNameFull"].ToString());
                    }
                    if (dr["Year"] != null && dr["Year"].ToString() != string.Empty)
                    {
                        sch.Year = Convert.ToInt32(dr["Year"].ToString());
                    }
                    if (dr["Passengers"] != null && dr["Passengers"].ToString() != string.Empty)
                    {
                        sch.Passengers = Convert.ToInt32(dr["Passengers"].ToString());
                    }
                    if (dr["ClassID"] != null && dr["ClassID"].ToString() != string.Empty)
                    {
                        sch.ClassID = Convert.ToInt32(dr["ClassID"].ToString());
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = Convert.ToDouble(dr["Revenue"].ToString());
                    }
                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = Convert.ToDouble(dr["NonRevenue"].ToString());
                    }

                    if (dr["str_RouteID"] != null && dr["str_RouteID"].ToString() != string.Empty)
                    {
                        sch.str_RouteID = (dr["str_RouteID"].ToString());
                    }
                    if (dr["RouteName"] != null && dr["RouteName"].ToString() != string.Empty)
                    {
                        sch.RouteName = (dr["RouteName"].ToString());
                    }
                    data.Add(sch);
                }
            }

            finally
            {
                myConnection.Close();
            }

            return data;
        }

        public DataSet GetOriginAnalysisByRouteDataSet(string connKey, SalesAnalysisFilter filter, string companyName)
        {
            string filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            string filterClassesSelected = "Classes: Filter Not Selected";
            string filterRoutesSelected = "Routes: Filter Not Selected";
            string filterRouteTypeSelected = "RouteTypes: Filter Not Selected";

            if (filter.ClassesSelected != null && filter.ClassesSelected.Length > 0)
            {
                List<string> classes = GetAllClasses(connKey).Where(s => filter.ClassesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassesSelected = "Classes: " + string.Join(", ", classes);
            }

            if (filter.RoutesSelected != null && filter.RoutesSelected.Length > 0)
            {
                List<string> routes = GetAllRoutes(connKey).Where(s => filter.RoutesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterRoutesSelected = "Routes: " + string.Join(", ", routes);
            }

            if (filter.RouteTypeSelected != null && filter.RouteTypeSelected.Length > 0)
            {
                List<string> routestypes = GetAllRouteTypes(connKey).Where(s => filter.RouteTypeSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterRouteTypeSelected = "RouteTypes: " + string.Join(", ", routestypes);
            }

            DataSet result = new DataSet();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));
            try
            {
                myConnection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = myConnection;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "EbusOriginAnalysisByRouteReport";

                    cmd.Parameters.AddWithValue("@routeIds", filter.RoutesSelected == null ? "" : string.Join(",", filter.RoutesSelected));
                    cmd.Parameters.AddWithValue("@classIds", filter.ClassesSelected == null ? "" : string.Join(",", filter.ClassesSelected));
                    cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate));
                    cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate));
                    cmd.Parameters.AddWithValue("@routeType", filter.RouteTypeSelected == null ? "" : string.Join(",", filter.RouteTypeSelected));

                    cmd.CommandTimeout = 500000;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(result);
                    }
                }

                DataColumn newColumn = new DataColumn("CompanyName", typeof(string))
                {
                    DefaultValue = companyName
                };
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("DateSelected", typeof(string))
                {
                    DefaultValue = filterDateRange
                };
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("ClassSelected", typeof(string))
                {
                    DefaultValue = filterClassesSelected
                };
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("RoutesSelected", typeof(string))
                {
                    DefaultValue = filterRoutesSelected
                };
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("RouteTypesSelected", typeof(string))
                {
                    DefaultValue = filterRouteTypeSelected
                };
                result.Tables[0].Columns.Add(newColumn);

                result = MasterHelper.FillDefaultValuesForEmptyDataSet(result);
            }
            finally
            {
                myConnection.Close();
            }
            return result;
        }

        public DataSet GetSalesAnalysisByRouteDataSet(string connKey, SalesAnalysisFilter filter, string companyName)
        {
            DataSet ds = new DataSet();
            DataTable table1 = SalesRouteAnalsisDataset();
            string filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);

            string filterClassesSelected = "Classes: Filter Not Selected";
            string filterRoutesSelected = "Routes: Filter Not Selected";
            string filterClassGroupsSelected = "Class Groups: Filter Not Selected";

            if (filter.ClassesSelected != null && filter.ClassesSelected.Length > 0)
            {
                List<string> classes = GetAllClasses(connKey).Where(s => filter.ClassesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassesSelected = "Classes: " + string.Join(", ", classes);
            }

            if (filter.RoutesSelected != null && filter.RoutesSelected.Length > 0)
            {
                List<string> routes = GetAllRoutes(connKey).Where(s => filter.RoutesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterRoutesSelected = "Routes: " + string.Join(", ", routes);
            }

            if (filter.ClassGroupsSelected != null && filter.ClassGroupsSelected.Length > 0)
            {
                List<string> cgrp = GetAllClassGroups(connKey).Where(s => filter.ClassGroupsSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassGroupsSelected = "Class Groups: " + string.Join(", ", cgrp);
            }

            List<SalesAnalysisByRoute> result = GetSalesAnalysisByRouteData(connKey, filter);

            if (result.Any())
            {
                foreach (SalesAnalysisByRoute item in result)
                {
                    item.CompanyName = companyName;
                    item.dateRange = filterDateRange;
                    item.ClassIdFilters = filterClassesSelected;
                    item.RoutesFilters = filterRoutesSelected;
                    item.ClassGroupFilter = filterClassGroupsSelected;
                    table1.Rows.Add(
                                item.RouteID,
                                item.RouteName,
                                item.ClassID,
                                item.ClassName,
                                item.ClassType,
                                item.Revenue,
                                item.NonRevenue,
                                item.AnnulCash,
                                item.TxCount,
                                item.TicketCount,
                                item.dateRange,
                                item.RoutesFilters,
                                item.ClassIdFilters,
                                item.CompanyName,
                                item.ClassGroupFilter,
                                (item.Revenue / (item.TicketCount == 0 ? 1 : item.TicketCount))
                        );
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["CompanyName"] = companyName;
                dr["dateRange"] = filterDateRange;
                dr["ClassIdFilters"] = filterClassesSelected;
                dr["RoutesFilters"] = filterRoutesSelected;
                dr["ClassGroupFilter"] = filterClassGroupsSelected;

                table1.Rows.Add(dr);
            }


            ds.Tables.Add(table1);
            return ds;
        }

        public DataSet GetSalesAnalysisByRouteSummaryDataSet(string connKey, SalesAnalysisFilter filter, string companyName)
        {
            DataSet ds = new DataSet();
            DataTable table1 = SalesRouteAnalsisDataset();
            InspectorReportService service = new InspectorReportService();
            string filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);

            string filterClassTypesSelected = "ClassTypes: Filter Not Selected";
            string filterRoutesSelected = "Routes: Filter Not Selected";
            string filterClassGroupsSelected = "Class Groups: Filter Not Selected";

            if (filter.ClassTypesSelected != null && filter.ClassTypesSelected.Length > 0)
            {
                List<string> classes = service.GetAllClassTypes(connKey).Where(s => filter.ClassTypesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassTypesSelected = "Classes: " + string.Join(", ", classes);
            }

            if (filter.RoutesSelected != null && filter.RoutesSelected.Length > 0)
            {
                List<string> routes = GetAllRoutes(connKey).Where(s => filter.RoutesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterRoutesSelected = "Routes: " + string.Join(", ", routes);
            }

            if (filter.ClassGroupsSelected != null && filter.ClassGroupsSelected.Length > 0)
            {
                List<string> cgrp = GetAllClassGroups(connKey).Where(s => filter.ClassGroupsSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassGroupsSelected = "Class Groups: " + string.Join(", ", cgrp);
            }

            List<SalesAnalysisByRoute> result = GetSalesAnalysisByRouteSummaryData(connKey, filter);

            result = (from e in result
                      group e by new { e.RouteID, e.ClassType } into grp
                      select new SalesAnalysisByRoute()
                      {
                          ClassType = grp.Key.ClassType,
                          RouteID = grp.Key.RouteID,
                          RouteName = grp.ToList().FirstOrDefault().RouteName,
                          Revenue = grp.ToList().Sum(x => x.Revenue),
                          NonRevenue = grp.ToList().Sum(x => x.NonRevenue),
                          TxCount = grp.ToList().Sum(x => x.TxCount),
                          TicketCount = grp.ToList().Sum(x => x.TicketCount),
                      }).ToList();

            if (result.Any())
            {
                foreach (SalesAnalysisByRoute item in result)
                {
                    item.CompanyName = companyName;
                    item.dateRange = filterDateRange;
                    item.ClassIdFilters = filterClassTypesSelected;
                    item.RoutesFilters = filterRoutesSelected;
                    item.ClassGroupFilter = filterClassGroupsSelected;
                    table1.Rows.Add(
                                item.RouteID,
                                item.RouteName,
                                item.ClassID,
                                item.ClassName,
                                item.ClassType,
                                item.Revenue,
                                item.NonRevenue,
                                item.AnnulCash,
                                item.TxCount,
                                item.TicketCount,
                                item.dateRange,
                                item.RoutesFilters,
                                item.ClassIdFilters,
                                item.CompanyName,
                                item.ClassGroupFilter,
                                (item.Revenue / (item.TicketCount == 0 ? 1 : item.TicketCount))
                        );
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["CompanyName"] = companyName;
                dr["dateRange"] = filterDateRange;
                dr["ClassIdFilters"] = filterClassTypesSelected;
                dr["RoutesFilters"] = filterRoutesSelected;
                dr["ClassGroupFilter"] = filterClassGroupsSelected;

                table1.Rows.Add(dr);
            }


            ds.Tables.Add(table1);
            return ds;
        }


        public DataSet GetClassSummaryDataSet(string connKey, SalesAnalysisFilter filter, string companyName, string spName, bool isClassType)
        {
            DataSet ds = new DataSet();
            DataTable table1 = ClassSummaryDataset();
            string filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);

            string filterClassesSelected = "Classes: Filter Not Selected";
            string filterRoutesSelected = "Routes: Filter Not Selected";
            string filterClassGroupsSelected = "Class Groups: Filter Not Selected";


            if (filter.ClassesSelected != null && filter.ClassesSelected.Length > 0)
            {
                List<string> classes = GetAllClasses(connKey).Where(s => filter.ClassesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassesSelected = "Classes: " + string.Join(", ", classes);
            }

            if (filter.RoutesSelected != null && filter.RoutesSelected.Length > 0)
            {
                List<string> routes = GetAllRoutes(connKey).Where(s => filter.RoutesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterRoutesSelected = "Routes: " + string.Join(", ", routes);
            }

            if (filter.ClassGroupsSelected != null && filter.ClassGroupsSelected.Length > 0)
            {
                List<string> cgrp = GetAllClassGroups(connKey).Where(s => filter.ClassGroupsSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassGroupsSelected = "Class Groups: " + string.Join(", ", cgrp);
            }

            List<ClassSummaryData> result = GetClassSummaryData(connKey, filter, spName, isClassType);

            if (result.Any())
            {
                foreach (ClassSummaryData item in result)
                {
                    item.CompanyName = companyName;
                    item.DateRange = filterDateRange;
                    item.ClassFilter = filterClassesSelected;
                    item.RouteFilter = filterRoutesSelected;
                    item.ClassGroupFilter = filterClassGroupsSelected;

                    table1.Rows.Add(
                                item.ClassTypeName,
                                item.Class,
                                item.ClassGroup,
                                item.Revenue,
                                item.NonRevenue,
                                item.TicketCount,
                                item.TripCount,
                                item.DateRange,
                                item.ClassFilter,
                                item.ClassGroupFilter,
                                item.RouteFilter,
                                item.CompanyName,
                                (item.Revenue / (item.TicketCount == 0 ? 1 : item.TicketCount))
                        );
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["CompanyName"] = companyName;
                dr["DateRange"] = filterDateRange;
                dr["ClassFilter"] = filterClassesSelected;
                dr["RouteFilter"] = filterRoutesSelected;
                dr["ClassGroupFilter"] = filterClassGroupsSelected;

                table1.Rows.Add(dr);
            }


            ds.Tables.Add(table1);
            return ds;
        }

        public DataSet GetSellerSummaryDataSet(string connKey, SalesAnalysisFilter filter, string companyName, string spName)
        {
            DataSet ds = new DataSet();
            DataTable table1 = SellerSummaryDataset();
            string filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);

            string filterClassesSelected = "Classes: Filter Not Selected";
            string filterRoutesSelected = "Staffs: Filter Not Selected";
            string filterClassTypesSelected = "Class Types: Filter Not Selected";


            if (filter.ClassesSelected != null && filter.ClassesSelected.Length > 0)
            {
                List<string> classes = GetAllClasses(connKey).Where(s => filter.ClassesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassesSelected = "Classes: " + string.Join(", ", classes);
            }

            if (filter.StaffsSelected != null && filter.StaffsSelected.Length > 0)
            {
                List<string> staffs = GetAllStaffs(connKey).Where(s => filter.StaffsSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterRoutesSelected = "Staffs: " + string.Join(", ", staffs);
            }

            if (filter.ClassTypesSelected != null && filter.ClassTypesSelected.Length > 0)
            {
                List<string> clsTypes = GetAllClassTypes(connKey).Where(s => filter.ClassTypesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassTypesSelected = "Class Types: " + string.Join(", ", clsTypes);
            }

            List<SellerSummaryData> result = GetSellerSummaryData(connKey, filter, spName);

            if (result.Any())
            {
                foreach (SellerSummaryData item in result)
                {
                    item.CompanyName = companyName;
                    item.DateRange = filterDateRange;
                    item.ClassFilter = filterClassesSelected;
                    item.StaffFilter = filterRoutesSelected;
                    item.ClassTypeFilter = filterClassTypesSelected;

                    table1.Rows.Add(
                                item.ClassTypeName,
                                item.Class,
                                item.Revenue,
                                item.NonRevenue,
                                item.TicketCount,
                                item.TripCount,
                                item.DateRange,
                                item.ClassFilter,
                                item.ClassTypeFilter,
                                item.StaffFilter,
                                item.CompanyName,
                                (item.Revenue / (item.TicketCount == 0 ? 1 : item.TicketCount)),
                                item.Staff,
                                item.TransDate,
                                item.StartTime,
                                item.StopTime,
                                item.EtmID
                        );
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["CompanyName"] = companyName;
                dr["DateRange"] = filterDateRange;
                dr["ClassFilter"] = filterClassesSelected;
                dr["StaffFilter"] = filterRoutesSelected;
                dr["ClassTypeFilter"] = filterClassTypesSelected;

                table1.Rows.Add(dr);
            }


            ds.Tables.Add(table1);
            return ds;
        }

        private List<SalesAnalysisByRoute> GetSalesAnalysisByRouteData(string connKey, SalesAnalysisFilter filter)
        {
            List<SalesAnalysisByRoute> result = new List<SalesAnalysisByRoute>();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand("EbusSalesAnalysisByRouteReport", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@routeIds", filter.RoutesSelected == null ? "" : string.Join(",", filter.RoutesSelected));
                cmd.Parameters.AddWithValue("@classIds", filter.ClassesSelected == null ? "" : string.Join(",", filter.ClassesSelected));
                cmd.Parameters.AddWithValue("@classgroupIds", filter.ClassGroupsSelected == null ? "" : string.Join(",", filter.ClassGroupsSelected));
                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    SalesAnalysisByRoute sch = new SalesAnalysisByRoute();

                    if (dr["RouteID"] != null && dr["RouteID"].ToString() != string.Empty)
                    {
                        sch.RouteID = (dr["RouteID"].ToString());
                    }

                    if (dr["RouteName"] != null && dr["RouteName"].ToString() != string.Empty)
                    {
                        sch.RouteName = (dr["RouteName"].ToString());
                    }

                    if (dr["ClassName"] != null && dr["ClassName"].ToString() != string.Empty)
                    {
                        sch.ClassName = (dr["ClassName"].ToString());
                    }

                    if (dr["ClassType"] != null && dr["ClassType"].ToString() != string.Empty)
                    {
                        sch.ClassType = (dr["ClassType"].ToString());
                    }

                    if (dr["ClassID"] != null && dr["ClassID"].ToString() != string.Empty)
                    {
                        sch.ClassID = Convert.ToInt32(dr["ClassID"].ToString());
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = Convert.ToDouble(dr["Revenue"].ToString());
                    }

                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = Convert.ToDouble(dr["NonRevenue"].ToString());
                    }

                    if (dr["AnnulCash"] != null && dr["AnnulCash"].ToString() != string.Empty)
                    {
                        sch.AnnulCash = Convert.ToDouble(dr["AnnulCash"].ToString());
                    }

                    if (dr["TxCount"] != null && dr["TxCount"].ToString() != string.Empty)
                    {
                        sch.TxCount = Convert.ToInt32(dr["TxCount"].ToString());
                    }

                    if (dr["TicketCount"] != null && dr["TicketCount"].ToString() != string.Empty)
                    {
                        sch.TicketCount = Convert.ToInt32(dr["TicketCount"].ToString());
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

        private List<SalesAnalysisByRoute> GetSalesAnalysisByRouteSummaryData(string connKey, SalesAnalysisFilter filter)
        {
            List<SalesAnalysisByRoute> result = new List<SalesAnalysisByRoute>();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand("EbusSalesAnalysisByRouteSummaryReport", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@routeIds", filter.RoutesSelected == null ? "" : string.Join(",", filter.RoutesSelected));
                cmd.Parameters.AddWithValue("@classTypeIds", filter.ClassTypesSelected == null ? "" : string.Join(",", filter.ClassTypesSelected));
                cmd.Parameters.AddWithValue("@classgroupIds", filter.ClassGroupsSelected == null ? "" : string.Join(",", filter.ClassGroupsSelected));
                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    SalesAnalysisByRoute sch = new SalesAnalysisByRoute();

                    if (dr["RouteID"] != null && dr["RouteID"].ToString() != string.Empty)
                    {
                        sch.RouteID = (dr["RouteID"].ToString());
                    }

                    if (dr["RouteName"] != null && dr["RouteName"].ToString() != string.Empty)
                    {
                        sch.RouteName = (dr["RouteName"].ToString());
                    }

                    if (dr["ClassName"] != null && dr["ClassName"].ToString() != string.Empty)
                    {
                        sch.ClassName = (dr["ClassName"].ToString());
                    }

                    if (dr["ClassType"] != null && dr["ClassType"].ToString() != string.Empty)
                    {
                        sch.ClassType = (dr["ClassType"].ToString());
                    }

                    if (dr["ClassID"] != null && dr["ClassID"].ToString() != string.Empty)
                    {
                        sch.ClassID = Convert.ToInt32(dr["ClassID"].ToString());
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = Convert.ToDouble(dr["Revenue"].ToString());
                    }

                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = Convert.ToDouble(dr["NonRevenue"].ToString());
                    }

                    if (dr["AnnulCash"] != null && dr["AnnulCash"].ToString() != string.Empty)
                    {
                        sch.AnnulCash = Convert.ToDouble(dr["AnnulCash"].ToString());
                    }

                    if (dr["TxCount"] != null && dr["TxCount"].ToString() != string.Empty)
                    {
                        sch.TxCount = Convert.ToInt32(dr["TxCount"].ToString());
                    }

                    if (dr["TicketCount"] != null && dr["TicketCount"].ToString() != string.Empty)
                    {
                        sch.TicketCount = Convert.ToInt32(dr["TicketCount"].ToString());
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

        public List<ClassTypeGraph> GetClassSummaryByClassTypeGraphData(string connKey, SalesAnalysisFilter filter)
        {
            //get last 3 month data

            string last2m = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.AddMonths(-2).Month);
            string lastm = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.AddMonths(-1).Month);
            string current = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);

            filter.StartDate = DateTime.Now.AddMonths(-2).AddDays(-DateTime.Now.AddMonths(-2).Day + 1).ToString("dd-MM-yyyy");
            filter.EndDate = DateTime.Now.ToString("dd-MM-yyyy");

            List<ClassTypeGraph> list = GetClassSummaryDataGraph(connKey, filter);

            List<ClassTypeGraph> classTypeNameGroup = (from a in list
                                                       group a by a.ClassTypeName into g
                                                       select new ClassTypeGraph
                                                       {
                                                           ClassTypeName = g.Key,
                                                           Last2MonthName = last2m,
                                                           Last2Revenue = (int)g.Where(s => s.MonthName == last2m).Sum(s => s.Revenue),

                                                           LastMonthName = lastm,
                                                           LastRevenue = (int)g.Where(s => s.MonthName == lastm).Sum(s => s.Revenue),

                                                           CurrentMonthName = current,
                                                           CurrentRevenue = (int)g.Where(s => s.MonthName == current).Sum(s => s.Revenue)
                                                       }).ToList();

            return classTypeNameGroup;
        }

        private List<ClassTypeGraph> GetClassSummaryDataGraph(string connKey, SalesAnalysisFilter filter)
        {
            List<ClassTypeGraph> result = new List<ClassTypeGraph>();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand("EbusClassSummaryByClassTypeGraph", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@routeIds", filter.RoutesSelected == null ? "" : string.Join(",", filter.RoutesSelected));
                cmd.Parameters.AddWithValue("@classIds", filter.ClassesSelected == null ? "" : string.Join(",", filter.ClassesSelected));
                cmd.Parameters.AddWithValue("@classgroupIds", filter.ClassGroupsSelected == null ? "" : string.Join(",", filter.ClassGroupsSelected));
                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    ClassTypeGraph sch = new ClassTypeGraph();

                    if (dr["ClassTypeName"] != null && dr["ClassTypeName"].ToString() != string.Empty)
                    {
                        sch.ClassTypeName = (dr["ClassTypeName"].ToString());
                    }

                    if (dr["DatteSel"] != null && dr["DatteSel"].ToString() != string.Empty)
                    {
                        sch.DatteSel = (dr["DatteSel"].ToString());

                        string datePart = sch.DatteSel.ToString().Split(' ')[0];
                        string[] date = datePart.Split('/');
                        string mont = (date[0].Length == 1 ? "0" + date[0] : date[0]).Trim();

                        string fulldate = (date[1].Length == 1 ? "0" + date[1] : date[1]).Trim() + "-" + mont + "-" + date[2].Trim();

                        sch.MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(CustomDateTime.ConvertStringToDateSaFormat(fulldate).Month);
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = Convert.ToDouble(dr["Revenue"].ToString());
                    }

                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = Convert.ToDouble(dr["NonRevenue"].ToString());
                        if (sch.Revenue == 0)
                        {
                            sch.Revenue = sch.NonRevenue;
                        }
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

        private List<SellerSummaryData> GetSellerSummaryData(string connKey, SalesAnalysisFilter filter, string spName)
        {
            List<SellerSummaryData> result = new List<SellerSummaryData>();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand(spName, myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@staffIds", filter.StaffsSelected == null ? "" : string.Join(",", filter.StaffsSelected));
                cmd.Parameters.AddWithValue("@classIds", filter.ClassesSelected == null ? "" : string.Join(",", filter.ClassesSelected));
                cmd.Parameters.AddWithValue("@classTypeIds", filter.ClassTypesSelected == null ? "" : string.Join(",", filter.ClassTypesSelected));
                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                SellerSummaryData sch = null;
                while (dr.Read())
                {
                    sch = new SellerSummaryData();

                    if (dr["TransDate"] != null && dr["TransDate"].ToString() != string.Empty)
                    {
                        sch.TransDate = (dr["TransDate"].ToString());
                    }

                    if (dr["StartTime"] != null && dr["StartTime"].ToString() != string.Empty)
                    {
                        sch.StartTime = Convert.ToDateTime(dr["StartTime"].ToString()).ToString("HH:mm"); 
                    }

                    if (dr["StopTime"] != null && dr["StopTime"].ToString() != string.Empty)
                    {
                        sch.StopTime = Convert.ToDateTime(dr["StopTime"].ToString()).ToString("HH:mm"); 
                    }

                    if (dr["EtmID"] != null && dr["EtmID"].ToString() != string.Empty)
                    {
                        sch.EtmID = (dr["EtmID"].ToString());
                    }

                    if (dr["Staff"] != null && dr["Staff"].ToString() != string.Empty)
                    {
                        sch.Staff = (dr["Staff"].ToString());
                    }

                    if (dr["ClassTypeName"] != null && dr["ClassTypeName"].ToString() != string.Empty)
                    {
                        sch.ClassTypeName = (dr["ClassTypeName"].ToString());
                    }

                    if (dr["Class"] != null && dr["Class"].ToString() != string.Empty)
                    {
                        sch.Class = (dr["Class"].ToString());
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = Convert.ToDouble(dr["Revenue"].ToString());
                    }

                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = Convert.ToDouble(dr["NonRevenue"].ToString());
                    }

                    if (dr["TripCount"] != null && dr["TripCount"].ToString() != string.Empty)
                    {
                        sch.TripCount = Convert.ToInt32(dr["TripCount"].ToString());
                    }

                    if (dr["TicketCount"] != null && dr["TicketCount"].ToString() != string.Empty)
                    {
                        sch.TicketCount = Convert.ToInt32(dr["TicketCount"].ToString());
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


        //class summary data
        //isClassType = true => class is present
        //isClassType = false => classGroup is present
        private List<ClassSummaryData> GetClassSummaryData(string connKey, SalesAnalysisFilter filter, string spName, bool isClassType)
        {
            List<ClassSummaryData> result = new List<ClassSummaryData>();

            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand(spName, myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@routeIds", filter.RoutesSelected == null ? "" : string.Join(",", filter.RoutesSelected));
                cmd.Parameters.AddWithValue("@classIds", filter.ClassesSelected == null ? "" : string.Join(",", filter.ClassesSelected));
                cmd.Parameters.AddWithValue("@classgroupIds", filter.ClassGroupsSelected == null ? "" : string.Join(",", filter.ClassGroupsSelected));
                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    ClassSummaryData sch = new ClassSummaryData();

                    if (dr["ClassTypeName"] != null && dr["ClassTypeName"].ToString() != string.Empty)
                    {
                        sch.ClassTypeName = (dr["ClassTypeName"].ToString());
                    }

                    if (isClassType == false && dr["Class"] != null && dr["Class"].ToString() != string.Empty)
                    {
                        sch.Class = (dr["Class"].ToString());
                    }

                    if (isClassType == true && dr["ClassGroup"] != null && dr["ClassGroup"].ToString() != string.Empty)
                    {
                        sch.ClassGroup = (dr["ClassGroup"].ToString());
                    }

                    if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = Convert.ToDouble(dr["Revenue"].ToString());
                    }

                    if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = Convert.ToDouble(dr["NonRevenue"].ToString());
                    }

                    if (dr["TripCount"] != null && dr["TripCount"].ToString() != string.Empty)
                    {
                        sch.TripCount = Convert.ToInt32(dr["TripCount"].ToString());
                    }

                    if (dr["TicketCount"] != null && dr["TicketCount"].ToString() != string.Empty)
                    {
                        sch.TicketCount = Convert.ToInt32(dr["TicketCount"].ToString());
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

        public SalesAnalysisFilter GetSalesAnalysisFilter(string connKey)
        {
            SalesAnalysisFilter model = new SalesAnalysisFilter
            {
                Classes = GetAllClasses(connKey),
                Routes = GetAllRoutes(connKey),
                ClassGroups = GetAllClassGroups(connKey)
            };
            return model;
        }

        public SalesAnalysisFilter GetSalesAnalysisBySellerFilter(string connKey)
        {
            SalesAnalysisFilter model = new SalesAnalysisFilter
            {
                Staffs = GetAllStaffs(connKey),
                Classes = GetAllClasses(connKey),
                Routes = GetAllRoutes(connKey),
                ClassTypes = GetAllClassTypes(connKey)
            };

            return model;
        }


        public SalesAnalysisFilter GetSalesAnalysisSummaryFilter(string connKey)
        {
            SalesAnalysisFilter model = new SalesAnalysisFilter();
            InspectorReportService service = new InspectorReportService();
            model.ClassTypes = service.GetAllClassTypes(connKey);
            model.Routes = GetAllRoutes(connKey);
            model.ClassGroups = GetAllClassGroups(connKey);
            return model;
        }


        public SalesAnalysisFilter GetSalesAnalysisFilterForGraph(string connKey)
        {
            SalesAnalysisFilter model = new SalesAnalysisFilter
            {
                ClassGroups = GetAllClassGroups(connKey)
            };
            model.ClassGroups.Add(new SelectListItem() { Selected = false, Text = "Both", Value = "all" });
            SelectListItem driver = model.ClassGroups.Where(s => s.Text.ToLower() == "driver").FirstOrDefault();
            if (driver != null)
            {
                model.ClassGroups.Where(s => s.Text.ToLower() == "driver").FirstOrDefault().Selected = true;
            }
            else
            {
                if (model.ClassGroups.Any())
                {
                    model.ClassGroups.FirstOrDefault().Selected = true;
                }
            }
            return model;
        }

        public List<SelectListItem> GetAllRoutes(string connKey)
        {
            List<SelectListItem> res = new List<SelectListItem>();
            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "select str4_RouteNumber,str50_RouteName from qdMainRoutes union select str4_RouteNumber,str50_RouteName from slMainRoutes";

                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    SelectListItem obj = new SelectListItem();

                    if (dr["str4_RouteNumber"] != null && dr["str4_RouteNumber"].ToString() != string.Empty)
                    {
                        obj.Text = dr["str4_RouteNumber"].ToString() + " - " + dr["str50_RouteName"].ToString();
                        obj.Value = dr["str4_RouteNumber"].ToString();
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

        public List<SelectListItem> GetAllStaffs(string connKey)
        {
            InspectorReportService service = new InspectorReportService();

            List<OperatorDetails> staff = service.GetAllSatffDetails(connKey);

            return staff.Where(s => s.OperatorType.ToLower() == "Seller".ToLower().Trim()).Select(s => new SelectListItem { Text = string.Format("{0} - {1} ", s.OperatorName, s.OperatorID), Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
        }

        public List<SelectListItem> GetAllClasses(string connKey)
        {
            List<SelectListItem> res = new List<SelectListItem>();
            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "select int2_Class,str50_LongName from class ;";

                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    SelectListItem obj = new SelectListItem();

                    if (dr["str50_LongName"] != null && dr["str50_LongName"].ToString() != string.Empty)
                    {
                        obj.Text = dr["str50_LongName"].ToString();
                    }

                    if (dr["int2_Class"] != null && dr["int2_Class"].ToString() != string.Empty)
                    {
                        obj.Value = dr["int2_Class"].ToString();
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

        public List<SelectListItem> GetAllRouteTypes(string connKey)
        {
            List<SelectListItem> res = new List<SelectListItem>();
            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "select int4_StaffTypeID,str25_Description from stafftype where str25_description in ('driver','seller') ;";

                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    SelectListItem obj = new SelectListItem();

                    if (dr["str25_Description"] != null && dr["str25_Description"].ToString() != string.Empty)
                    {
                        obj.Text = dr["str25_Description"].ToString();
                    }

                    if (dr["int4_StaffTypeID"] != null && dr["int4_StaffTypeID"].ToString() != string.Empty)
                    {
                        obj.Value = dr["int4_StaffTypeID"].ToString();
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

        private List<SelectListItem> GetAllClassGroups(string connKey)
        {
            List<SelectListItem> res = new List<SelectListItem>();
            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "select int4_ID,str30_Description from classgroupdescription ;";

                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    SelectListItem obj = new SelectListItem();

                    if (dr["str30_Description"] != null && dr["str30_Description"].ToString() != string.Empty)
                    {
                        obj.Text = dr["str30_Description"].ToString();
                    }

                    if (dr["int4_ID"] != null && dr["int4_ID"].ToString() != string.Empty)
                    {
                        obj.Value = dr["int4_ID"].ToString();
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

        private List<SelectListItem> GetAllClassTypes(string connKey)
        {
            List<SelectListItem> res = new List<SelectListItem>();
            SqlConnection myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "SELECT int_TypeID,str30_Description FROM ClassType;";

                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    SelectListItem obj = new SelectListItem();

                    if (dr["str30_Description"] != null && dr["str30_Description"].ToString() != string.Empty)
                    {
                        obj.Text = dr["str30_Description"].ToString();
                    }

                    if (dr["int_TypeID"] != null && dr["int_TypeID"].ToString() != string.Empty)
                    {
                        obj.Value = dr["int_TypeID"].ToString();
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

        private List<SelectListItem> GetMonths()
        {
            List<SelectListItem> months = new List<SelectListItem>
            {
                new SelectListItem { Selected = false, Text = "Jan", Value = "1" },
                new SelectListItem { Selected = false, Text = "Feb", Value = "2" },
                new SelectListItem { Selected = false, Text = "Mar", Value = "3" },
                new SelectListItem { Selected = false, Text = "Apr", Value = "4" },
                new SelectListItem { Selected = false, Text = "May", Value = "5" },
                new SelectListItem { Selected = false, Text = "Jun", Value = "6" },
                new SelectListItem { Selected = false, Text = "Jul", Value = "7" },
                new SelectListItem { Selected = false, Text = "Aug", Value = "8" },
                new SelectListItem { Selected = false, Text = "Sep", Value = "9" },
                new SelectListItem { Selected = false, Text = "Oct", Value = "10" },
                new SelectListItem { Selected = false, Text = "Nov", Value = "11" },
                new SelectListItem { Selected = false, Text = "Dec", Value = "12" }
            };
            return months;
        }
    }


}