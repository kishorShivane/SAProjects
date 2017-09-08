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
    public class SalesAnalysisService : BaseServices
    {
        public MonthyRevenueDataGraph GetMonthRevenueData(string connKey, string monthsSelected, string yearsSelected)
        {
            var data = new List<MonthyRevenueData>();
            var graphData = new MonthyRevenueDataGraph();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("EbusGetMonthRevenueData", myConnection)
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
                    var sch = new MonthyRevenueData();

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

                var seller = data.Where(s => s.int4_DutyID == 8000).GroupBy(s => s.MonthName).ToList();
                var driver = data.Where(s => s.int4_DutyID != 8000).GroupBy(s => s.MonthName).ToList();

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
            var ds = new DataSet();
            var table1 = YearlyBreakDownDataset();
            var finalData = new List<YearlyData>();

            var fromMonth = filter.FromMonthSelected == null ? new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" } : filter.FromMonthSelected;
            var fromMonthList = GetMonths().Where(s => fromMonth.Contains(s.Value)).Select(s => s.Text).ToList();
            var fromMonthNames = string.Join(",", fromMonthList);

            var toMonth = filter.ToMonthSelected == null ? new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" } : filter.ToMonthSelected;
            var toMonthList = GetMonths().Where(s => toMonth.Contains(s.Value)).Select(s => s.Text).ToList();
            var toMonthNames = string.Join(",", toMonthList);

            var title = string.Format("Months Selected: {0} - {1} vs {2} - {3} ", filter.ToYearSelected, toMonthNames, filter.FromYearSelected, fromMonthNames);

            var classFilter = filter.ClassesSelected == null ? "Class Selected: All Classes" : "Class Selected: " + string.Join(",", filter.ClassesSelected);
            var classesSelected = new List<Int32>();
            classesSelected = filter.ClassesSelected != null ? filter.ClassesSelected.Select(s => Convert.ToInt32(s)).ToList() : GetAllCalsses(connKey).Select(s => Convert.ToInt32(s.Value)).ToList();

            var data = GetYearlyBreakDownData(connKey, filter);

            foreach (var cls in classesSelected)
            {
                var cData = data.Where(s => s.ClassID.Equals(cls)).FirstOrDefault();
                var year2Data = data.Where(s => s.ClassID.Equals(cls) && s.Year.Equals(Convert.ToInt32(filter.ToYearSelected))).ToList();
                var year1Data = data.Where(s => s.ClassID.Equals(cls) && s.Year.Equals(Convert.ToInt32(filter.FromYearSelected))).ToList();
                var item = new YearlyData();

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

            var year2RevSum = finalData.Sum(s => s.Year2Revenue);
            var Year1RevSum = finalData.Sum(s => s.Year1Revenue);
            var RevDiffSum = finalData.Sum(s => s.RevenueDiff);
            var Year2NonRevSum = finalData.Sum(s => s.Year2NonRevenue);
            var Year1NonRevsum = finalData.Sum(s => s.Year1NonRevenue);
            var NonRevDiffSum = finalData.Sum(s => s.NonRevenueDiff);
            var Year2PsngSum = finalData.Sum(s => s.Year2Passenger);
            var Year1PsngSum = finalData.Sum(s => s.Year1Passenger);
            var PsngDiffSum = finalData.Sum(s => s.PassengerDiff);

            if (finalData.Any())
            {
                foreach (var item in finalData)
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
            var data = new List<YearlyBreakDownData>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("EbusYearlyBreakDownData", myConnection)
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
                    var sch = new YearlyBreakDownData();

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
            var ds = new DataSet();
            var table1 = YearlyBreakDownDataset();
            var finalData = new List<YearlyData>();

            var fromMonth = filter.FromMonthSelected == null ? new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" } : filter.FromMonthSelected;
            var fromMonthList = GetMonths().Where(s => fromMonth.Contains(s.Value)).Select(s => s.Text).ToList();
            var fromMonthNames = string.Join(",", fromMonthList);

            var toMonth = filter.ToMonthSelected == null ? new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" } : filter.ToMonthSelected;
            var toMonthList = GetMonths().Where(s => toMonth.Contains(s.Value)).Select(s => s.Text).ToList();
            var toMonthNames = string.Join(",", toMonthList);

            var title = string.Format("Months Selected: {0} - {1} vs {2} - {3} ", filter.ToYearSelected, toMonthNames, filter.FromYearSelected, fromMonthNames);

            var classFilter = filter.ClassesSelected == null ? "Class Selected: All Classes" : "Class Selected: " + string.Join(",", filter.ClassesSelected);
            var routesFilter = filter.RoutesSelected == null ? "Routes Selected: All Routes" : "Routes Selected: " + string.Join(",", filter.RoutesSelected);

            var classesSelected = new List<Int32>();
            classesSelected = filter.ClassesSelected != null ? filter.ClassesSelected.Select(s => Convert.ToInt32(s)).ToList() : GetAllCalsses(connKey).Select(s => Convert.ToInt32(s.Value)).ToList();

            var data = EbusYearlyBreakDownDataByRoute(connKey, filter);

            var routeGroupData = data.GroupBy(s => s.str_RouteID).ToList();

            if (routeGroupData.Any())
            {
                foreach (var gd in routeGroupData)
                {
                    var routeId = gd.Key;
                    var routeNameObj = gd.Where(s => !string.IsNullOrEmpty(s.RouteName)).FirstOrDefault();
                    var routeName = string.Empty;

                    if (routeNameObj != null)
                    {
                        routeName = routeNameObj.RouteName;
                    }

                    finalData = new List<YearlyData>();

                    foreach (var cls in classesSelected)
                    {
                        var cData = gd.Where(s => s.ClassID.Equals(cls)).FirstOrDefault();
                        var year2Data = gd.Where(s => s.ClassID.Equals(cls) && s.Year.Equals(Convert.ToInt32(filter.ToYearSelected))).ToList();
                        var year1Data = gd.Where(s => s.ClassID.Equals(cls) && s.Year.Equals(Convert.ToInt32(filter.FromYearSelected))).ToList();
                        var item = new YearlyData();

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

                    var year2RevSum = finalData.Sum(s => s.Year2Revenue);
                    var Year1RevSum = finalData.Sum(s => s.Year1Revenue);
                    var RevDiffSum = finalData.Sum(s => s.RevenueDiff);
                    var Year2NonRevSum = finalData.Sum(s => s.Year2NonRevenue);
                    var Year1NonRevsum = finalData.Sum(s => s.Year1NonRevenue);
                    var NonRevDiffSum = finalData.Sum(s => s.NonRevenueDiff);
                    var Year2PsngSum = finalData.Sum(s => s.Year2Passenger);
                    var Year1PsngSum = finalData.Sum(s => s.Year1Passenger);
                    var PsngDiffSum = finalData.Sum(s => s.PassengerDiff);


                    foreach (var item in finalData)
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
            var data = new List<YearlyBreakDownData>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("EbusYearlyBreakDownDataByRoute", myConnection)
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
                    var sch = new YearlyBreakDownData();

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
            var filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var filterClassesSelected = "Classes: Filter Not Selected";
            var filterRoutesSelected = "Routes: Filter Not Selected";
            var filterRouteTypeSelected = "RouteTypes: Filter Not Selected";

            if (filter.ClassesSelected != null && filter.ClassesSelected.Length > 0)
            {
                var classes = GetAllCalsses(connKey).Where(s => filter.ClassesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassesSelected = "Classes: " + string.Join(", ", classes);
            }

            if (filter.RoutesSelected != null && filter.RoutesSelected.Length > 0)
            {
                var routes = GetAllRoutes(connKey).Where(s => filter.RoutesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterRoutesSelected = "Routes: " + string.Join(", ", routes);
            }

            if (filter.RouteTypeSelected != null && filter.RouteTypeSelected.Length > 0)
            {
                var routestypes = GetAllRouteTypes(connKey).Where(s => filter.RouteTypeSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterRouteTypeSelected = "RouteTypes: " + string.Join(", ", routestypes);
            }

            var result = new DataSet();

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

                var newColumn = new DataColumn("CompanyName", typeof(string));
                newColumn.DefaultValue = companyName;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("DateSelected", typeof(string));
                newColumn.DefaultValue = filterDateRange;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("ClassSelected", typeof(string));
                newColumn.DefaultValue = filterClassesSelected;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("RoutesSelected", typeof(string));
                newColumn.DefaultValue = filterRoutesSelected;
                result.Tables[0].Columns.Add(newColumn);

                newColumn = new DataColumn("RouteTypesSelected", typeof(string));
                newColumn.DefaultValue = filterRouteTypeSelected;
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
            var ds = new DataSet();
            var table1 = SalesRouteAnalsisDataset();
            var filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);

            var filterClassesSelected = "Classes: Filter Not Selected";
            var filterRoutesSelected = "Routes: Filter Not Selected";
            var filterClassGroupsSelected = "Class Groups: Filter Not Selected";

            if (filter.ClassesSelected != null && filter.ClassesSelected.Length > 0)
            {
                var classes = GetAllCalsses(connKey).Where(s => filter.ClassesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassesSelected = "Classes: " + string.Join(", ", classes);
            }

            if (filter.RoutesSelected != null && filter.RoutesSelected.Length > 0)
            {
                var routes = GetAllRoutes(connKey).Where(s => filter.RoutesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterRoutesSelected = "Routes: " + string.Join(", ", routes);
            }

            if (filter.ClassGroupsSelected != null && filter.ClassGroupsSelected.Length > 0)
            {
                var cgrp = GetAllClassGroups(connKey).Where(s => filter.ClassGroupsSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassGroupsSelected = "Class Groups: " + string.Join(", ", cgrp);
            }

            var result = GetSalesAnalysisByRouteData(connKey, filter);

            if (result.Any())
            {
                foreach (var item in result)
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
                                (double)((item.Revenue / (item.TicketCount == 0 ? 1 : item.TicketCount)))
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

        public DataSet GetClassSummaryDataSet(string connKey, SalesAnalysisFilter filter, string companyName, string spName, bool isClassType)
        {
            var ds = new DataSet();
            var table1 = ClassSummaryDataset();
            var filterDateRange = string.Format("{0} :  {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);

            var filterClassesSelected = "Classes: Filter Not Selected";
            var filterRoutesSelected = "Routes: Filter Not Selected";
            var filterClassGroupsSelected = "Class Groups: Filter Not Selected";


            if (filter.ClassesSelected != null && filter.ClassesSelected.Length > 0)
            {
                var classes = GetAllCalsses(connKey).Where(s => filter.ClassesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassesSelected = "Classes: " + string.Join(", ", classes);
            }

            if (filter.RoutesSelected != null && filter.RoutesSelected.Length > 0)
            {
                var routes = GetAllRoutes(connKey).Where(s => filter.RoutesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterRoutesSelected = "Routes: " + string.Join(", ", routes);
            }

            if (filter.ClassGroupsSelected != null && filter.ClassGroupsSelected.Length > 0)
            {
                var cgrp = GetAllClassGroups(connKey).Where(s => filter.ClassGroupsSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterClassGroupsSelected = "Class Groups: " + string.Join(", ", cgrp);
            }

            var result = GetClassSummaryData(connKey, filter, spName, isClassType);

            if (result.Any())
            {
                foreach (var item in result)
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
                                (double)((item.Revenue / (item.TicketCount == 0 ? 1 : item.TicketCount)))
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

        private List<SalesAnalysisByRoute> GetSalesAnalysisByRouteData(string connKey, SalesAnalysisFilter filter)
        {
            var result = new List<SalesAnalysisByRoute>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("EbusSalesAnalysisByRouteReport", myConnection)
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
                    var sch = new SalesAnalysisByRoute();

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

            var last2m = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.AddMonths(-2).Month);
            var lastm = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.AddMonths(-1).Month);
            var current = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);

            filter.StartDate = DateTime.Now.AddMonths(-2).AddDays(-DateTime.Now.AddMonths(-2).Day + 1).ToString("dd-MM-yyyy");
            filter.EndDate = DateTime.Now.ToString("dd-MM-yyyy");

            var list = GetClassSummaryDataGraph(connKey, filter);

            var classTypeNameGroup = (from a in list
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
            var result = new List<ClassTypeGraph>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand("EbusClassSummaryByClassTypeGraph", myConnection)
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
                    var sch = new ClassTypeGraph();

                    if (dr["ClassTypeName"] != null && dr["ClassTypeName"].ToString() != string.Empty)
                    {
                        sch.ClassTypeName = (dr["ClassTypeName"].ToString());
                    }

                    if (dr["DatteSel"] != null && dr["DatteSel"].ToString() != string.Empty)
                    {
                        sch.DatteSel = (dr["DatteSel"].ToString());

                        var datePart = sch.DatteSel.ToString().Split(' ')[0];
                        var date = datePart.Split('/');
                        var mont = (date[0].Length == 1 ? "0" + date[0] : date[0]).Trim();

                        var fulldate = (date[1].Length == 1 ? "0" + date[1] : date[1]).Trim() + "-" + mont + "-" + date[2].Trim();

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
        //class summary data
        //isClassType = true => class is present
        //isClassType = false => classGroup is present
        private List<ClassSummaryData> GetClassSummaryData(string connKey, SalesAnalysisFilter filter, string spName, bool isClassType)
        {
            var result = new List<ClassSummaryData>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand(spName, myConnection)
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
                    var sch = new ClassSummaryData();

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
            var model = new SalesAnalysisFilter();

            model.Classes = GetAllCalsses(connKey);
            model.Routes = GetAllRoutes(connKey);
            model.ClassGroups = GetAllClassGroups(connKey);



            return model;
        }

        public SalesAnalysisFilter GetSalesAnalysisFilterForGraph(string connKey)
        {
            var model = new SalesAnalysisFilter();
            model.ClassGroups = GetAllClassGroups(connKey);
            model.ClassGroups.Add(new SelectListItem() { Selected = false, Text = "Both", Value = "all" });
            var driver = model.ClassGroups.Where(s => s.Text.ToLower() == "driver").FirstOrDefault();
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
            var res = new List<SelectListItem>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand()
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
                    var obj = new SelectListItem();

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

        public List<SelectListItem> GetAllCalsses(string connKey)
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

                cmd.CommandText = "select int2_Class,str50_LongName from class ;";

                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();

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
            var res = new List<SelectListItem>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand()
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
                    var obj = new SelectListItem();

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
            var res = new List<SelectListItem>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand()
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
                    var obj = new SelectListItem();

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

        private List<SelectListItem> GetMonths()
        {
            var months = new List<SelectListItem>();
            months.Add(new SelectListItem { Selected = false, Text = "Jan", Value = "1" });
            months.Add(new SelectListItem { Selected = false, Text = "Feb", Value = "2" });
            months.Add(new SelectListItem { Selected = false, Text = "Mar", Value = "3" });
            months.Add(new SelectListItem { Selected = false, Text = "Apr", Value = "4" });
            months.Add(new SelectListItem { Selected = false, Text = "May", Value = "5" });
            months.Add(new SelectListItem { Selected = false, Text = "Jun", Value = "6" });
            months.Add(new SelectListItem { Selected = false, Text = "Jul", Value = "7" });
            months.Add(new SelectListItem { Selected = false, Text = "Aug", Value = "8" });
            months.Add(new SelectListItem { Selected = false, Text = "Sep", Value = "9" });
            months.Add(new SelectListItem { Selected = false, Text = "Oct", Value = "10" });
            months.Add(new SelectListItem { Selected = false, Text = "Nov", Value = "11" });
            months.Add(new SelectListItem { Selected = false, Text = "Dec", Value = "12" });
            return months;
        }
    }


}