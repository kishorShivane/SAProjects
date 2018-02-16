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
    public class ReportsService : BaseServices
    {
        public const string sp2 = "EbusScheduledVsOpr"; // Sch vs Opr
        public const string sp3 = "EbusDailuAudit"; // Daily audit
        public const string sp4 = "EbusNotScheduledbutWorked";//"Sample4"; // Not Scheduled, but Worked
        public const string sp5 = "EbusScheduledbutnotWorked";//"Sample5"; // Scheduled, but not Worked
        public const string sp6 = "EbusHomeScreen";//"Sample6"; // Home Screen
        public const string sp7 = "EbusFormE"; // Form-E
        public const string sp8 = "EbusEarlyLateRunning"; // Form-E

        public HomeViewModel GetHomeScreenData(string connKey)
        {
            var result = new HomeViewModel();

            var filter = new SchVsOprViewModel();
            //filter.DutiesSelected = new string[1] { "8000" };
            filter.StartDate = DateTime.Now.Date.AddDays(-7 - ((int)DateTime.Now.DayOfWeek)).ToString("dd-MM-yyyy");
            filter.EndDate = DateTime.Now.Date.AddDays(-1).ToString("dd-MM-yyyy");
            //sunday=0,...sat =6
            var data = GetListData(connKey, filter, sp6, false, true);

            if (!data.Any())
            {
                return new HomeViewModel();
            }

            var yestDate = filter.EndDate = DateTime.Now.Date.AddDays(-1).ToString("dd/MM/yyyy");
            var yestData = data.Where(s => s.dateSelected == yestDate);

            var graphData = (from a in data
                             group a by a.dateSelected into g
                             select new DayRevenue
                             {
                                 dateSelected = g.Key,
                                 revenueFromdrivers = g.Where(s => s.int4_DutyId != "8000").Sum(s => Convert.ToDecimal(s.int4_JourneyRevenue)),
                                 revenueFromSellers = g.Where(s => s.int4_DutyId == "8000").Sum(s => Convert.ToDecimal(s.int4_JourneyRevenue))
                             }).ToList();

            graphData.ForEach(s =>
            {
                s.dayOfWeek = CustomDateTime.ConvertStringToDateSaFormat(s.dateSelected).Date.DayOfWeek.ToString();
            });

            if (yestData.Any())
            {
                result = (from a in yestData
                          group a by a.dateSelected into g
                          select new HomeViewModel
                          {
                              RevenueFromDrivers = g.Where(s => s.int4_OperatorID != null && s.int4_DutyId != "8000").Sum(s => Convert.ToDecimal(s.int4_JourneyRevenue)).ToString("N", CultureInfo.InvariantCulture),
                              RevenueFromSellers = g.Where(s => s.int4_DutyId == "8000").Sum(s => Convert.ToDecimal(s.int4_JourneyRevenue)).ToString("N", CultureInfo.InvariantCulture),

                              DriversCount = g.Where(s => s.int4_OperatorID != null && s.int4_DutyId != "8000").Select(r => r.int4_OperatorID).Distinct().Count(),
                              SellersCount = g.Where(s => s.int4_DutyId == "8000").Select(r => r.int4_OperatorID).Distinct().Count(),

                              TotalCashPassengers = g.Where(s => s.int4_DutyId != "8000").Sum(s => Convert.ToInt32(s.int4_JourneyTickets)),
                              TotalPasses = g.Where(s => s.int4_DutyId != "8000").Sum(s => Convert.ToInt32(s.int4_JourneyPasses)),
                              TotalTransfers = g.Where(s => s.int4_DutyId != "8000").Sum(s => Convert.ToInt32(s.int4_JourneyTransfer)),

                              //ignore dead km
                              ScheduledDistance = (int)g.Where(s => s.TripStatus.ToLower() != "not scheduled, but worked" && s.int4_DutyId != "8000" && s.bit_ReveOrDead == true).Sum(s => s.float_Distance),
                              OperatedDistance = (int)g.Where(s => s.TripStatus.ToLower() == "worked" && s.int4_DutyId != "8000" && s.bit_ReveOrDead == true).Sum(s => s.float_Distance),

                              //ignore dead trips
                              ScheduledTripsCount = g.Where(s => s.TripStatus.ToLower() != "not scheduled, but worked" && s.int4_DutyId != "8000" && s.bit_ReveOrDead == true).Count(),
                              OperatedTripsCount = g.Where(s => s.TripStatus.ToLower() == "worked" && s.int4_DutyId != "8000" && s.bit_ReveOrDead == true).Count()

                          }).FirstOrDefault();

                result.RevenueSum = Convert.ToDecimal(result.RevenueFromDrivers) + Convert.ToDecimal(result.RevenueFromSellers);
            }

            result.DaysString = string.Join(",", graphData.Select(s => s.dayOfWeek).ToList());
            result.DaysRevenueString = string.Join(",", graphData.Select(s => s.revenueFromdrivers).ToList());
            result.DaysSellersRevenueString = string.Join(",", graphData.Select(s => s.revenueFromSellers).ToList());

            return result;
        }

        public SchVsOprViewModel GetFilter(string connKey)
        {
            var res = new SchVsOprViewModel();

            var allContracts = GetAllContacts(connKey);
            var allDuties = GetAllDuties(connKey);

            res.Contracts = (from f in allContracts
                             select new SelectListItem { Selected = false, Text = f, Value = f }).ToList();

            res.Duties = allDuties;

            return res;
        }

        public JourneyAnalysisSummaryBySubRouteViewModel GetJourneyAnalysisSummaryBySubRouteFilter(string connKey)
        {
            var res = new JourneyAnalysisSummaryBySubRouteViewModel();
            var salesAnalysisService = new SalesAnalysisService();
            var service = new InspectorReportService();
            var allContracts = GetAllContacts(connKey);
            res.Classes = salesAnalysisService.GetAllCalsses(connKey);
            res.Routes = salesAnalysisService.GetAllRoutes(connKey);
            res.ClassesTypes = service.GetAllClassTypes(connKey);

            res.Contracts = (from f in allContracts
                             select new SelectListItem { Selected = false, Text = f, Value = f }).ToList();
            return res;
        }


        public EarlyLateRunningModel GetEarlyLateRunningModel(string connKey)
        {
            var res = new EarlyLateRunningModel();
            var allContracts = GetAllContacts(connKey);
            var allDuties = GetAllDuties(connKey);

            res.Contracts = (from f in allContracts
                             select new SelectListItem { Selected = false, Text = f, Value = f }).ToList();

            res.Duties = allDuties;

            return res;
        }


        public DailyAuditViewModel GetFilterForSellersDailyAudit(string connKey)
        {
            var res = new DailyAuditViewModel();

            res.AllStaffs = GetAllDrivers(connKey);
            res.Locations = GetAllLocations(connKey);
            res.StaffTypes = GetAllStaffTypes(connKey).Where(s => s.Text.ToLower().Contains("seller")).ToList();

            return res;
        }

        public DailyAuditViewModel GetFilterForDailyAudit(string connKey)
        {
            var res = new DailyAuditViewModel();

            res.AllStaffs = GetAllDrivers(connKey);
            res.Locations = GetAllLocations(connKey);
            res.StaffTypes = GetAllStaffTypes(connKey).Where(s => s.Text.ToLower().Contains("driver") || s.Text.ToLower().Contains("seller")).ToList();

            return res;
        }


        public List<SchVsWorked> GetListData(string connKey, SchVsOprViewModel filter, string spName, bool isFormE = false, bool isHomeScreen = false)
        {
            var result = GetScheduledVsOperatedData(connKey, filter, spName, isFormE, isHomeScreen);
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
            return filteredResult;


        }

        public DataSet GetEarlyLateRunningReport(string connKey, SchVsOprViewModel filter, Int32 timeSelected, string companyName)
        {
            var model = new EarlyLateRunningModel();
            var ds = new DataSet();
            var table1 = ScheduleVsOperatedDataSet();
            var min = 0;
            var max = 0;
            //5 10 15 -15 -10 -5

            if (timeSelected == 5)
            {  //bit early(+5) between (5 min to 10 min)
                min = 5; max = 10000;
            }
            else if (timeSelected == 10)
            { //Early(+10) between (11 min to 15 min)
                min = 10; max = 10000;
            }
            else if (timeSelected == 15) //ver early(+15)  (15 mins are more )
            {
                min = 15; max = 10000;
            }
            else if (timeSelected == -15)// very late(-15)  (-15 mins are less)
            {
                min = -1000; max = -15;
            }
            else if (timeSelected == -10)// late(-10) between (-11 min to 15 min)
            {
                min = -1000; max = -10;
            }
            else if (timeSelected == -5)// bit late(-5) between (-5 min to -10 min)
            {
                min = -1000; max = -5;
            }

            //filter details
            var filterDateRange = string.Format(" {0}: {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);

            var filterContractsRange = " Contracts: Filter Not Selected";
            var filterDuties = " Duties: Filter Not Selected";
            var filterTimeSelected = " Time Selected : " + model.TimeList.Where(s => s.Value.Equals(timeSelected.ToString())).FirstOrDefault().Text;

            if (filter.ContractsSelected != null && filter.ContractsSelected.Length > 0)
            {
                filterContractsRange = " Contracts: " + string.Join(", ", filter.ContractsSelected);
            }

            if (filter.DutiesSelected != null && filter.DutiesSelected.Length > 0)
            {
                filterDuties = " Duties: " + string.Join(", ", filter.DutiesSelected);
            }

            var result = GetScheduledVsOperatedData(connKey, filter, sp8, false);
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

            filteredResult = filteredResult.Where(s => s.JourneyDiffInMins.HasValue && s.JourneyDiffInMins >= min && s.JourneyDiffInMins <= max).ToList();

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
                                filterDuties,
                                filterTimeSelected
                        );
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["companyName"] = companyName;
                dr["DateRangeFilter"] = filterDateRange;
                dr["filterContractsRange"] = filterContractsRange;
                dr["filterDuties"] = filterDuties;
                dr["filterTimeSelected"] = filterTimeSelected;
                table1.Rows.Add(dr);
            }

            ds.Tables.Add(table1);
            return ds;
        }

        public DataSet GetScheduledVsOperatedReport(string connKey, SchVsOprViewModel filter, string spName, string companyName, bool isFormEReport = false)
        {

            var ds = new DataSet();
            var table1 = ScheduleVsOperatedDataSet();

            //filter details
            var filterDateRange = string.Format(" {0}: {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);

            var filterContractsRange = " Contracts: Filter Not Selected";
            var filterDuties = " Duties: Filter Not Selected";

            if (filter.ContractsSelected != null && filter.ContractsSelected.Length > 0)
            {
                filterContractsRange = " Contracts: " + string.Join(", ", filter.ContractsSelected);
            }

            if (filter.DutiesSelected != null && filter.DutiesSelected.Length > 0)
            {
                filterDuties = " Duties: " + string.Join(", ", filter.DutiesSelected);
            }

            var result = GetScheduledVsOperatedData(connKey, filter, spName, isFormEReport);
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
            return ds;
        }

        public int getCountOfTypeTrips(List<SchVsWorked> items, string tripType, bool isEqual)
        {
            var count = 0;
            count = (from c in items
                     where (isEqual && c.TripStatus.ToLower() == tripType) || (!isEqual && c.TripStatus.ToLower() != tripType)
                     group c by new
                     {
                         c.str4_JourneyNo,
                         //c.int4_OperatorID,
                         c.int4_DutyId,
                         c.dateSelected
                     } into grp
                     select grp).ToList().Count();
            //count = items.Where(r => r.str4_JourneyNo == s.str4_JourneyNo
            //                            && r.int4_OperatorID == s.int4_OperatorID && r.int4_DutyId == s.int4_DutyId
            //                            && r.dateSelected == s.dateSelected)
            return count;
        }

        public int getCountOfTypeTrips(List<JourneyAnalysisSummaryBySubRoute> items, string tripType, bool isEqual)
        {
            var count = 0;
            count = (from c in items
                     where (isEqual && c.TripStatus.ToLower() == tripType) || (!isEqual && c.TripStatus.ToLower() != tripType)
                     group c by new
                     {
                         c.str4_JourneyNo,
                         c.dateSelected
                     } into grp
                     select grp).ToList().Count();
            return count;
        }

        public DataSet GetFormEReport(string connKey, SchVsOprViewModel filter, string spName, string companyName, bool isFormEReport = false)
        {
            //filter details
            var filterDateRange = string.Format(" {0}: {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var filterContractsRange = " Contracts: Filter Not Selected";
            if (filter.ContractsSelected != null && filter.ContractsSelected.Length > 0)
            {
                filterContractsRange = " Contracts: " + string.Join(", ", filter.ContractsSelected);
            }

            var ds = new DataSet();
            var table1 = FormEDataset();
            //filter.DutiesSelected = new string[1] { "702" };

            var data = GetListData(connKey, filter, spName, true);
            //first group by contract
            var groupByContract = (from a in data
                                   group a by a.str7_Contract into g
                                   select g).ToList();

            var result = new List<FormEData>();
            //then group by DOT nummber
            foreach (var contract in groupByContract)
            {
                var groupByDot = from c in contract
                                 group c by c.DOTRouteNumber into grp
                                 select new FormEData
                                 {
                                     DOTRoute = grp.Key,
                                     Contract = grp.Any() ? grp.FirstOrDefault().str7_Contract : string.Empty,

                                     From = grp.Any() ? grp.FirstOrDefault().routeName : string.Empty,
                                     To = grp.Any() ? grp.FirstOrDefault().routeName : string.Empty,

                                     //ScheduledTrips = grp.Any() ? grp.Where(s => s.TripStatus.ToLower() != "not scheduled, but worked").Count().ToString() : string.Empty,
                                     //OperatedTrips = grp.Any() ? grp.Where(s => s.TripStatus.ToLower() == "worked").Count().ToString() : string.Empty,
                                     //NotOperatedTrips = grp.Any() ? grp.Where(s => s.TripStatus.ToLower() == "scheduled, but not worked").Count().ToString() : string.Empty,

                                     ScheduledTrips = grp.Any() ? getCountOfTypeTrips(grp.ToList(), "not scheduled, but worked", false).ToString() : string.Empty,
                                     OperatedTrips = grp.Any() ? getCountOfTypeTrips(grp.ToList(), "worked", true).ToString() : string.Empty,
                                     NotOperatedTrips = grp.Any() ? getCountOfTypeTrips(grp.ToList(), "scheduled, but not worked", true).ToString() : string.Empty,

                                     Schedulekilometres = grp.Sum(s => s.float_Distance).ToString(),
                                     OperatedKilometres = grp.Where(s => s.TripStatus.ToLower() == "worked").Sum(s => s.float_Distance).ToString(),

                                     Tickets = grp.Sum(s => Convert.ToInt32(s.int4_JourneyTickets)).ToString(),
                                     Passes = grp.Sum(s => Convert.ToInt32(s.int4_JourneyPasses)).ToString(),
                                     Transfers = grp.Sum(s => Convert.ToInt32(s.int4_JourneyTransfer)).ToString(),
                                     TotalPassengers = grp.Sum(s => Convert.ToInt32(s.Int_TotalPassengers)).ToString(),

                                     Revenue = grp.Sum(s => Convert.ToDecimal(s.int4_JourneyRevenue)).ToString(),
                                     NonRevenue = grp.Sum(s => Convert.ToDecimal(s.int4_JourneyNonRevenue)).ToString(),

                                     AvgPassengerPerTrip = string.Empty,
                                     AvgRevenuePerTrip = grp.Any() ? grp.FirstOrDefault().str7_Contract : string.Empty
                                 };

                result.AddRange(groupByDot);
            }

            if (isFormEReport == true)
            {
                result = result.Where(s => !string.IsNullOrEmpty(s.Contract)).ToList();
            }

            if (result.Any())
            {
                foreach (var res in result)
                {
                    if (string.IsNullOrEmpty(res.From) == false && res.From.Contains(" to") == true)
                    {
                        var str = res.From.Split(new string[] { " to" }, StringSplitOptions.None);
                        res.From = str[0];
                        res.To = str.Length > 0 ? str[1] : "";
                    }
                    else
                    {
                        res.To = res.From;
                    }
                    res.TotalRevenue = (Convert.ToDouble(res.Revenue) + Convert.ToDouble(res.NonRevenue)).ToString();
                    if (!string.IsNullOrEmpty(res.OperatedTrips) && Convert.ToInt32(res.OperatedTrips) > 0)
                    {
                        res.AvgPassengerPerTrip = (Convert.ToInt32(res.TotalPassengers) / Convert.ToInt32(res.OperatedTrips)).ToString();
                        res.AvgRevenuePerTrip = (Convert.ToDouble(res.TotalRevenue) / Convert.ToDouble(res.OperatedTrips)).ToString();
                    }
                    else
                    {
                        res.AvgPassengerPerTrip = "0";
                        res.AvgRevenuePerTrip = "0";
                    }
                    res.DateRangeFilter = filterDateRange;
                    res.ContractsFilter = filterContractsRange;
                    res.companyName = companyName;
                    table1.Rows.Add(
                            res.Contract,
                            res.DOTRoute,
                            res.From,
                            res.To,
                            res.ScheduledTrips,
                            res.OperatedTrips,
                            res.NotOperatedTrips,
                            res.Schedulekilometres,
                            res.OperatedKilometres,
                            res.Tickets,
                            res.Passes,
                            res.Transfers,
                            res.TotalPassengers,
                            res.Revenue,
                            res.NonRevenue,
                            res.TotalRevenue,
                            res.AvgPassengerPerTrip,
                            res.AvgRevenuePerTrip,
                            res.DateRangeFilter,
                            res.ContractsFilter,
                            res.companyName
                        );
                }

            }
            else
            {
                //Empty row for blank report.
                DataRow dr = table1.NewRow();
                dr["DateRangeFilter"] = filterDateRange;
                dr["ContractsFilter"] = filterContractsRange;
                dr["companyName"] = companyName;
                table1.Rows.Add(dr);
            }


            ds.Tables.Add(table1);
            return ds;
        }

        public DataSet GetFormEDetailed(string connKey, SchVsOprViewModel filter, string spName, string companyName)
        {
            //filter details
            var filterDateRange = string.Format(" {0}: {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var filterContractsRange = " Contracts: Filter Not Selected";
            if (filter.ContractsSelected != null && filter.ContractsSelected.Length > 0)
            {
                filterContractsRange = " Contracts: " + string.Join(", ", filter.ContractsSelected);
            }

            var ds = new DataSet();
            var table1 = FormEDataset();

            var data = GetListData(connKey, filter, spName, true);
            //first group by contract
            var groupByDate = (from a in data
                               group a by a.dateSelected into g
                               select g).ToList().OrderBy(g => g.Key);

            var result = new List<FormEData>();
            //then group by DOT nummber
            foreach (var date in groupByDate)
            {
                var groupByContract = (from a in date
                                       group a by a.str7_Contract into g
                                       select g).ToList();

                foreach (var contract in groupByContract)
                {
                    var groupByDot = from c in contract
                                     group c by c.DOTRouteNumber into grp
                                     select new FormEData
                                     {

                                         DateSelected = date.Key,
                                         DOTRoute = grp.Key,
                                         Contract = grp.Any() ? grp.FirstOrDefault().str7_Contract : string.Empty,

                                         From = grp.Any() ? grp.FirstOrDefault().routeName : string.Empty,
                                         To = grp.Any() ? grp.FirstOrDefault().routeName : string.Empty,

                                         ScheduledTrips = grp.Any() ? grp.Where(s => s.TripStatus.ToLower() != "not scheduled, but worked").Count().ToString() : string.Empty,
                                         OperatedTrips = grp.Any() ? grp.Where(s => s.TripStatus.ToLower() == "worked").Count().ToString() : string.Empty,
                                         NotOperatedTrips = grp.Any() ? grp.Where(s => s.TripStatus.ToLower() == "scheduled, but not worked").Count().ToString() : string.Empty,

                                         Schedulekilometres = grp.Sum(s => s.float_Distance).ToString(),
                                         OperatedKilometres = grp.Where(s => s.TripStatus.ToLower() == "worked").Sum(s => s.float_Distance).ToString(),

                                         Tickets = grp.Sum(s => Convert.ToInt32(s.int4_JourneyTickets)).ToString(),
                                         Passes = grp.Sum(s => Convert.ToInt32(s.int4_JourneyPasses)).ToString(),
                                         Transfers = grp.Sum(s => Convert.ToInt32(s.int4_JourneyTransfer)).ToString(),
                                         TotalPassengers = grp.Sum(s => Convert.ToInt32(s.Int_TotalPassengers)).ToString(),

                                         Revenue = grp.Sum(s => Convert.ToDouble(s.int4_JourneyRevenue)).ToString(),
                                         NonRevenue = grp.Sum(s => Convert.ToDouble(s.int4_JourneyNonRevenue)).ToString(),

                                         AvgPassengerPerTrip = string.Empty,
                                         AvgRevenuePerTrip = grp.Any() ? grp.FirstOrDefault().str7_Contract : string.Empty
                                     };
                    result.AddRange(groupByDot);
                }
            }


            result = result.Where(s => !string.IsNullOrEmpty(s.Contract)).ToList();

            if (result.Any())
            {
                foreach (var res in result)
                {
                    if (string.IsNullOrEmpty(res.From) == false && res.From.Contains(" to") == true)
                    {
                        var str = res.From.Split(new string[] { " to" }, StringSplitOptions.None);
                        res.From = str[0];
                        res.To = str.Length > 0 ? str[1] : "";
                    }
                    else
                    {
                        res.To = res.From;
                    }
                    res.TotalRevenue = (Convert.ToDouble(res.Revenue) + Convert.ToDouble(res.NonRevenue)).ToString();
                    if (!string.IsNullOrEmpty(res.OperatedTrips) && Convert.ToInt32(res.OperatedTrips) > 0)
                    {
                        res.AvgPassengerPerTrip = (Convert.ToInt32(res.TotalPassengers) / Convert.ToInt32(res.OperatedTrips)).ToString();
                        res.AvgRevenuePerTrip = (Convert.ToDouble(res.TotalRevenue) / Convert.ToDouble(res.OperatedTrips)).ToString();
                    }
                    else
                    {
                        res.AvgPassengerPerTrip = "0";
                        res.AvgRevenuePerTrip = "0";
                    }
                    res.DateRangeFilter = filterDateRange;
                    res.ContractsFilter = filterContractsRange;
                    res.companyName = companyName;
                    table1.Rows.Add(
                            res.Contract,
                            res.DOTRoute,
                            res.From,
                            res.To,
                            res.ScheduledTrips,
                            res.OperatedTrips,
                            res.NotOperatedTrips,
                            res.Schedulekilometres,
                            res.OperatedKilometres,
                            res.Tickets,
                            res.Passes,
                            res.Transfers,
                            res.TotalPassengers,
                            res.Revenue,
                            res.NonRevenue,
                            res.TotalRevenue,
                            res.AvgPassengerPerTrip,
                            res.AvgRevenuePerTrip,
                            res.DateRangeFilter,
                            res.ContractsFilter,
                            res.companyName,
                            res.DateSelected
                        );
                }
            }
            else
            {
                //Empty row for blank report.
                DataRow dr = table1.NewRow();
                dr["DateRangeFilter"] = filterDateRange;
                dr["ContractsFilter"] = filterContractsRange;

                table1.Rows.Add(dr);
            }



            ds.Tables.Add(table1);
            return ds;
        }

        public DataSet GetJourneyAnalysisSummaryBySubRouteDetails(string connKey, JourneyAnalysisSummaryBySubRouteViewModel filter, string spName, string companyName)
        {
            //filter details
            var filterDateRange = string.Format(" {0}: {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var filterContractsRange = " Contracts: Filter Not Selected";
            var filterClassesRange = " Classes: Filter Not Selected";
            var filterClassesTypeRange = " Classes Types: Filter Not Selected";
            var filterRoutesRange = " Routes: Filter Not Selected";

            if (filter.ContractsSelected != null && filter.ContractsSelected.Length > 0)
            {
                filterContractsRange = " Contracts: " + string.Join(", ", filter.ContractsSelected);
            }

            if (filter.ClassesSelected != null && filter.ClassesSelected.Length > 0)
            {
                filterClassesRange = " Classes: " + string.Join(", ", filter.ClassesSelected);
            }

            if (filter.ClassesTypeSelected != null && filter.ClassesTypeSelected.Length > 0)
            {
                filterClassesTypeRange = " Classes Types: " + string.Join(", ", filter.ClassesTypeSelected);
            }

            if (filter.RoutesSelected != null && filter.RoutesSelected.Length > 0)
            {
                filterRoutesRange = " Routes: " + string.Join(", ", filter.RoutesSelected);
            }

            var ds = new DataSet();
            var table1 = JourneyAnalysisSummaryBySubRouteDataSet();

            var rawData = GetJourneyAnalysisSummaryBySubRouteData(connKey, filter, spName);
            var filteredResult = new List<JourneyAnalysisSummaryBySubRoute>();

            rawData.ForEach(s =>
            {
                var multiplePairExistRes = rawData.Where(r => r.str4_JourneyNo == s.str4_JourneyNo
                    && r.DutyID == s.DutyID && r.dateSelected == s.dateSelected
                    ).Count();

                if (multiplePairExistRes > 1)
                {
                    var multiplePairExistFil = filteredResult.Where(r => r.str4_JourneyNo == s.str4_JourneyNo
                                        && r.DutyID == s.DutyID && r.dateSelected == s.dateSelected
                                        ).Count();

                    if (multiplePairExistRes > 1)
                    {
                        filteredResult.RemoveAll(r => r.str4_JourneyNo == s.str4_JourneyNo
                                        && r.DutyID == s.DutyID && r.dateSelected == s.dateSelected
                                        && r.Int_TotalPassengers == 0); //remove all zero

                        var multiplePairExistFilNonZeroPsg = filteredResult.Where(r => r.str4_JourneyNo == s.str4_JourneyNo
                                        && r.DutyID == s.DutyID && r.dateSelected == s.dateSelected
                                        ).Count();
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

            var data = filteredResult.Where(s => !string.IsNullOrEmpty(s.Contract)).ToList();

            //first group by contract
            var groupByContract = (from a in data
                                   group a by a.Contract into g
                                   select g).ToList();
            var result = new List<JourneyAnalysisSummaryBySubRoute>();
            foreach (var contract in groupByContract)
            {
                result.AddRange((from a in contract
                                 group a by new { a.RouteNumber, a.str4_JourneyNo } into grp
                                 select new JourneyAnalysisSummaryBySubRoute
                                 {
                                     str4_JourneyNo = grp.Key.str4_JourneyNo,
                                     routeName = grp.FirstOrDefault().routeName,
                                     DutyID = grp.FirstOrDefault().DutyID,
                                     RouteNumber = grp.Key.RouteNumber,
                                     Contract = grp.Any() ? grp.FirstOrDefault().Contract : string.Empty,

                                     ScheduledTrips = grp.Any() ? getCountOfTypeTrips(grp.ToList(), "not scheduled, but worked", false).ToString() : string.Empty,
                                     OperatedTrips = grp.Any() ? getCountOfTypeTrips(grp.ToList(), "worked", true).ToString() : string.Empty,
                                     NotOperatedTrips = grp.Any() ? getCountOfTypeTrips(grp.ToList(), "scheduled, but not worked", true).ToString() : string.Empty,

                                     Tickets = grp.Sum(s => Convert.ToInt32(s.int4_JourneyTickets)).ToString(),
                                     Passes = grp.Sum(s => Convert.ToInt32(s.int4_JourneyPasses)).ToString(),
                                     TotalPassengers = grp.Sum(s => Convert.ToInt32(s.Int_TotalPassengers)).ToString(),
                                     TotalRevenue = grp.Sum(s => Convert.ToDouble(s.JourneyRevenue)).ToString(),

                                 }).ToList());
            }

            if (result.Any())
            {
                foreach (var res in result)
                {
                    table1.Rows.Add(
                            res.str4_JourneyNo,
                            res.RouteNumber + " - " + res.routeName,
                            res.ScheduledTrips,
                            res.OperatedTrips,
                            res.NotOperatedTrips,
                            res.Tickets,
                            res.Passes,
                            res.TotalPassengers,
                            res.TotalRevenue,
                            filterDateRange,
                            filterContractsRange,
                            filterClassesRange,
                            filterClassesTypeRange,
                            filterRoutesRange,
                            companyName,
                            res.Contract
                        );
                }
            }
            else
            {
                //Empty row for blank report.
                DataRow dr = table1.NewRow();
                dr["DateRangeFilter"] = filterDateRange;
                dr["ContractsFilter"] = filterContractsRange;
                dr["ClassesFilter"] = filterClassesRange;
                dr["ClassesTypeFilter"] = filterClassesTypeRange;
                dr["RouteFilter"] = filterRoutesRange;
                dr["companyName"] = companyName;

                table1.Rows.Add(dr);
            }



            ds.Tables.Add(table1);
            return ds;
        }


        //formE as sc vs Opr
        public DataSet FullDutySummary(string connKey, SchVsOprViewModel filter, string spName, string companyName, bool isFormEReport = false)
        {
            //filter details
            var filterDateRange = string.Format(" {0}: {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var filterContractsRange = " Contracts: Filter Not Selected";
            var filterDuties = " Duties: Filter Not Selected";

            if (filter.ContractsSelected != null && filter.ContractsSelected.Length > 0)
            {
                filterContractsRange = " Contracts: " + string.Join(", ", filter.ContractsSelected);
            }

            if (filter.DutiesSelected != null && filter.DutiesSelected.Length > 0)
            {
                filterDuties = " Duties: " + string.Join(", ", filter.DutiesSelected);
            }

            var ds = new DataSet();
            var table1 = FormE2Dataset();
            //filter.DutiesSelected = new string[1] { "702" };

            var data = GetListData(connKey, filter, spName, true).Where(s => !string.IsNullOrEmpty(s.str7_Contract)).ToList();
            //first group by Duty
            var groupByDuty = (from a in data
                               group a by a.int4_DutyId into g
                               select g).ToList();

            var result = new List<FormEData>();
            //then group by Journey
            foreach (var contract in groupByDuty)
            {
                var groupByDot = from c in contract
                                 group c by c.str4_JourneyNo into grp
                                 select new FormEData
                                 {
                                     DutyId = grp.Any() ? grp.FirstOrDefault().int4_DutyId : string.Empty,
                                     IntDutyId = Convert.ToInt32(grp.FirstOrDefault().int4_DutyId),
                                     Contract = grp.Any() ? grp.FirstOrDefault().str7_Contract : string.Empty,
                                     JourneyId = grp.Key,

                                     From = grp.Any() ? grp.FirstOrDefault().routeName : string.Empty,
                                     To = grp.Any() ? grp.FirstOrDefault().routeName : string.Empty,

                                     ScheduledTrips = grp.Any() ? grp.Where(s => s.TripStatus.ToLower() != "not scheduled, but worked").Count().ToString() : string.Empty,
                                     OperatedTrips = grp.Any() ? grp.Where(s => s.TripStatus.ToLower() == "worked").Count().ToString() : string.Empty,
                                     NotOperatedTrips = grp.Any() ? grp.Where(s => s.TripStatus.ToLower() == "scheduled, but not worked").Count().ToString() : string.Empty,

                                     Schedulekilometres = grp.Sum(s => s.float_Distance).ToString(),
                                     OperatedKilometres = grp.Where(s => s.TripStatus.ToLower() == "worked").Sum(s => s.float_Distance).ToString(),

                                     Tickets = grp.Sum(s => Convert.ToInt32(s.int4_JourneyTickets)).ToString(),
                                     Passes = grp.Sum(s => Convert.ToInt32(s.int4_JourneyPasses)).ToString(),
                                     Transfers = grp.Sum(s => Convert.ToInt32(s.int4_JourneyTransfer)).ToString(),
                                     TotalPassengers = grp.Sum(s => Convert.ToInt32(s.Int_TotalPassengers)).ToString(),

                                     Revenue = grp.Sum(s => Convert.ToDouble(s.int4_JourneyRevenue)).ToString(),
                                     NonRevenue = grp.Sum(s => Convert.ToDouble(s.int4_JourneyNonRevenue)).ToString(),

                                     AvgPassengerPerTrip = string.Empty,
                                     AvgRevenuePerTrip = grp.Any() ? grp.FirstOrDefault().str7_Contract : string.Empty
                                 };

                result.AddRange(groupByDot);
            }

            if (isFormEReport == true)
            {
                result = result.Where(s => !string.IsNullOrEmpty(s.Contract)).ToList();
            }

            var ordered = result.OrderBy(s => s.IntDutyId);

            if (ordered.Any())
            {
                foreach (var res in ordered)
                {
                    if (string.IsNullOrEmpty(res.From) == false && res.From.Contains(" to") == true)
                    {
                        var str = res.From.Split(new string[] { " to" }, StringSplitOptions.None);
                        res.From = str[0];
                        res.To = str.Length > 0 ? str[1] : "";
                    }
                    else
                    {
                        res.To = res.From;
                    }
                    res.TotalRevenue = (Convert.ToDouble(res.Revenue) + Convert.ToDouble(res.NonRevenue)).ToString();
                    if (!string.IsNullOrEmpty(res.OperatedTrips) && Convert.ToInt32(res.OperatedTrips) > 0)
                    {
                        res.AvgPassengerPerTrip = (Convert.ToInt32(res.TotalPassengers) / Convert.ToInt32(res.OperatedTrips)).ToString();
                        res.AvgRevenuePerTrip = (Convert.ToDouble(res.TotalRevenue) / Convert.ToDouble(res.OperatedTrips)).ToString();
                    }
                    else
                    {
                        res.AvgPassengerPerTrip = "0";
                        res.AvgRevenuePerTrip = "0";
                    }
                    res.DateRangeFilter = filterDateRange;
                    res.ContractsFilter = filterContractsRange;
                    res.companyName = companyName;
                    table1.Rows.Add(
                            res.Contract,
                            res.DOTRoute,
                            res.From,
                            res.To,
                            res.ScheduledTrips,
                            res.OperatedTrips,
                            res.NotOperatedTrips,
                            res.Schedulekilometres,
                            res.OperatedKilometres,
                            res.Tickets,
                            res.Passes,
                            res.Transfers,
                            res.TotalPassengers,
                            res.Revenue,
                            res.NonRevenue,
                            res.TotalRevenue,
                            res.AvgPassengerPerTrip,
                            res.AvgRevenuePerTrip,
                            res.DateRangeFilter,
                            res.ContractsFilter,
                            res.companyName,
                            "",
                            res.DutyId,
                            res.JourneyId,
                            filterDuties
                        );
                }
            }
            else
            {
                //Empty row for blank report.
                DataRow dr = table1.NewRow();
                dr["DutiesFilter"] = filterDuties;
                dr["DateRangeFilter"] = filterDateRange;
                dr["ContractsFilter"] = filterContractsRange;

                table1.Rows.Add(dr);
            }

            ds.Tables.Add(table1);
            return ds;
        }

        public Tuple<DataSet, List<RevenueByDuty>> RevenueByDuty(string connKey, SchVsOprViewModel filter, string spName, string companyName)
        {
            //filter details
            var filterDateRange = string.Format(" {0}: {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var filterDuties = " Duties: Filter Not Selected";

            if (filter.DutiesSelected != null && filter.DutiesSelected.Length > 0)
            {
                filterDuties = " Duties: " + string.Join(", ", filter.DutiesSelected);
            }

            var ds = new DataSet();
            var table1 = RevenueByDutyDataSet();
            //filter.DutiesSelected = new string[1] { "702" };

            var data = GetRevenueByDutyDetails(connKey, filter, spName);
            //first group by Duty
            var groupByDuty = (from a in data
                               group a by a.DutyID into g
                               select g).ToList();

            var result = new List<RevenueByDuty>();
            //then group by Journey
            foreach (var contract in groupByDuty)
            {
                var groupByDot = from c in contract
                                 group c by c.JourneyID into grp
                                 select new RevenueByDuty
                                 {
                                     DutyID = grp.Any() ? grp.FirstOrDefault().DutyID : string.Empty,
                                     JourneyID = grp.Any() ? grp.FirstOrDefault().JourneyID : string.Empty,
                                     Cash = Math.Round(grp.Sum(s => Convert.ToDouble(s.Revenue) == 0 ? 0 : Convert.ToDouble(s.Revenue) / 100), 4),
                                     Value = Math.Round(grp.Sum(s => Convert.ToDouble(s.NonRevenue) == 0 ? 0 : Convert.ToDouble(s.NonRevenue) / 100), 4),
                                     AdultRevenue = grp.Where(s => s.NonRevenue.Equals(0)).Count(s => s.Class.Equals(17)),
                                     ChildRevenue = grp.Where(s => s.NonRevenue.Equals(0)).Count(s => s.Class.Equals(33)),
                                     AdultNonRevenue = grp.Where(s => s.Revenue.Equals(0)).Count(s => s.Class.Equals(999)),
                                     SchlorNonRevenue = grp.Where(s => s.Revenue.Equals(0)).Count(s => s.Class.Equals(997)),
                                 };
                result.AddRange(groupByDot);
            }

            result.ForEach(x => x.Total = Math.Round((Convert.ToDouble(result.Where(q => q.DutyID.Equals(x.DutyID)).Sum(e => (e.Cash)))) + (Convert.ToDouble(result.Where(q => q.DutyID.Equals(x.DutyID)).Sum(e => (e.Value)))), 4));

            var ordered = result.OrderBy(s => s.DutyID);

            if (ordered.Any())
            {
                foreach (var res in ordered)
                {
                    res.DateRangeFilter = filterDateRange;
                    res.DutyFilter = filterDuties;
                    res.companyName = companyName;
                    table1.Rows.Add(
                            res.DutyID,
                            res.JourneyID,
                           res.Cash,
                           res.Value,
                            res.AdultRevenue,
                            res.ChildRevenue,
                            res.AdultNonRevenue,
                            res.SchlorNonRevenue,
                            res.AdultTransfer,
                            res.ScholarTransfer,
                            res.companyName,
                            res.DateRangeFilter,
                            res.DutyFilter,
                            res.Total
                        );
                }
            }
            else
            {
                //Empty row for blank report.
                DataRow dr = table1.NewRow();
                dr["filterDuties"] = filterDuties;
                dr["DateRangeFilter"] = filterDateRange;
                table1.Rows.Add(dr);
            }

            ds.Tables.Add(table1);
            return Tuple.Create(ds, ordered.ToList());
        }

        public Tuple<DataSet, List<RevenueByDuty>> RevenueByDutyAll(string connKey, SchVsOprViewModel filter, string spName, string companyName)
        {
            //filter details
            var filterDateRange = string.Format(" {0}: {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var filterDuties = " Duties: Filter Not Selected";

            if (filter.DutiesSelected != null && filter.DutiesSelected.Length > 0)
            {
                filterDuties = " Duties: " + string.Join(", ", filter.DutiesSelected);
            }

            var ds = new DataSet();
            var table1 = RevenueByDutyDataSet();
            //filter.DutiesSelected = new string[1] { "702" };

            var data = GetRevenueByDutyAllDetails(connKey, filter, spName);
            //first group by Duty
            var groupByDuty = (from a in data
                               group a by a.DutyID into g
                               select g).ToList();

            var result = new List<RevenueByDuty>();
            //then group by Journey
            foreach (var contract in groupByDuty)
            {
                var groupByDot = from c in contract
                                 group c by c.JourneyID into grp
                                 select new RevenueByDuty
                                 {
                                     DutyID = grp.Any() ? grp.FirstOrDefault().DutyID : string.Empty,
                                     JourneyID = grp.Any() ? grp.FirstOrDefault().JourneyID : string.Empty,
                                     Cash = Math.Round(grp.Sum(s => Convert.ToDouble(s.Revenue) == 0 ? 0 : Convert.ToDouble(s.Revenue) / 100), 4),
                                     Value = Math.Round(grp.Sum(s => Convert.ToDouble(s.NonRevenue) == 0 ? 0 : Convert.ToDouble(s.NonRevenue) / 100), 4),
                                     AdultRevenue = grp.Where(s => s.NonRevenue.Equals(0)).Count(s => s.Class.Equals(17)),
                                     ChildRevenue = grp.Where(s => s.NonRevenue.Equals(0)).Count(s => s.Class.Equals(33)),
                                     AdultNonRevenue = grp.Where(s => s.Revenue.Equals(0)).Count(s => s.Class.Equals(999)),
                                     SchlorNonRevenue = grp.Where(s => s.Revenue.Equals(0)).Count(s => s.Class.Equals(997)),
                                     AdultTransfer = grp.Where(s => s.Transfers.Equals(1)).Count(s => s.Class.Equals(995)),
                                     ScholarTransfer = grp.Where(s => s.Transfers.Equals(1)).Count(s => s.Class.Equals(996)),
                                 };
                result.AddRange(groupByDot);
            }

            result.ForEach(x => x.Total = Math.Round((Convert.ToDouble(result.Where(q => q.DutyID.Equals(x.DutyID)).Sum(e => (e.Cash)))) + (Convert.ToDouble(result.Where(q => q.DutyID.Equals(x.DutyID)).Sum(e => (e.Value)))), 4));

            var ordered = result.OrderBy(s => s.DutyID);

            if (ordered.Any())
            {
                foreach (var res in ordered)
                {
                    res.DateRangeFilter = filterDateRange;
                    res.DutyFilter = filterDuties;
                    res.companyName = companyName;
                    table1.Rows.Add(
                            res.DutyID,
                            res.JourneyID,
                            res.Cash,
                            res.Value,
                            res.AdultRevenue,
                            res.ChildRevenue,
                            res.AdultNonRevenue,
                            res.SchlorNonRevenue,
                            res.AdultTransfer,
                            res.ScholarTransfer,
                            res.companyName,
                            res.DateRangeFilter,
                            res.DutyFilter,
                            res.Total
                        );
                }
            }
            else
            {
                //Empty row for blank report.
                DataRow dr = table1.NewRow();
                dr["filterDuties"] = filterDuties;
                dr["DateRangeFilter"] = filterDateRange;
                table1.Rows.Add(dr);
            }

            ds.Tables.Add(table1);
            return Tuple.Create(ds, ordered.ToList());
        }

        public List<RevenueByDuty> GetRevenueByDutyDetails(string connKey, SchVsOprViewModel filter, string spName)
        {
            var schs = new List<RevenueByDuty>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand(spName, myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@dutyIds", filter.DutiesSelected != null ? string.Join(",", filter.DutiesSelected) : "");
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new RevenueByDuty();

                    if (dr["int4_DutyId"] != null && dr["int4_DutyId"].ToString() != string.Empty)
                    {
                        sch.DutyID = dr["int4_DutyId"].ToString();
                    }

                    if (dr["int2_JourneyID"] != null && dr["int2_JourneyID"].ToString() != string.Empty)
                    {
                        sch.JourneyID = dr["int2_JourneyID"].ToString();
                    }

                    if (dr["int4_Revenue"] != null && dr["int4_Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = float.Parse(dr["int4_Revenue"].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                    }

                    if (dr["int4_NonRevenue"] != null && dr["int4_NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = float.Parse(dr["int4_NonRevenue"].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                    }

                    if (dr["int2_Class"] != null && dr["int2_Class"].ToString() != string.Empty)
                    {
                        sch.Class = Convert.ToInt32(dr["int2_Class"]);
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

        public List<RevenueByDuty> GetRevenueByDutyAllDetails(string connKey, SchVsOprViewModel filter, string spName)
        {
            var schs = new List<RevenueByDuty>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand(spName, myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@fromDate", CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", CustomDateTime.ConvertStringToDateSaFormat(filter.EndDate).ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@dutyIds", filter.DutiesSelected != null ? string.Join(",", filter.DutiesSelected) : "");
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new RevenueByDuty();

                    if (dr["int4_DutyId"] != null && dr["int4_DutyId"].ToString() != string.Empty)
                    {
                        sch.DutyID = dr["int4_DutyId"].ToString();
                    }

                    if (dr["int2_JourneyID"] != null && dr["int2_JourneyID"].ToString() != string.Empty)
                    {
                        sch.JourneyID = dr["int2_JourneyID"].ToString();
                    }

                    if (dr["int4_Revenue"] != null && dr["int4_Revenue"].ToString() != string.Empty)
                    {
                        sch.Revenue = float.Parse(dr["int4_Revenue"].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                    }

                    if (dr["int4_NonRevenue"] != null && dr["int4_NonRevenue"].ToString() != string.Empty)
                    {
                        sch.NonRevenue = float.Parse(dr["int4_NonRevenue"].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                    }

                    if (dr["int2_Transfers"] != null && dr["int2_Transfers"].ToString() != string.Empty)
                    {
                        sch.Transfers = float.Parse(dr["int2_Transfers"].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                    }

                    if (dr["int2_Class"] != null && dr["int2_Class"].ToString() != string.Empty)
                    {
                        sch.Class = Convert.ToInt32(dr["int2_Class"]);
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

        public DataSet GetDailyAuditReportForSellers(string connKey, DailyAuditViewModel filter, string companyName)
        {
            var ds = new DataSet();
            var table1 = DailyAuditDataset();
            ds.Tables.Add(table1);

            filter.StaffsSelected = null;
            filter.StaffTypesSelected = null;
            filter.StaffTypesSelected = null;

            return GetDailyAuditReport(connKey, filter, companyName, true);
        }


        public DataSet GetDailyAuditReport(string connKey, DailyAuditViewModel filter, string companyName, bool isSeller = false)
        {
            var result = GetDailyAuditReportData(connKey, filter);

            if (isSeller == true)
            {
                result = result.Where(s => s.Duty.Equals(8000)).ToList();
            }

            //filter details
            var filterDateRange = string.Format("{0}: {1} to {2}", "Date Range", filter.StartDate, filter.EndDate);
            var filterStaffsSelected = "Staffs: Filter Not Selected";
            var filterStaffTypesSelected = "Staff Types: Filter Not Selected";
            var filterLocation = string.Format("Locations : {0} ", filter.LocationsSelected != null ? string.Join(",", filter.LocationsSelected) : "All");

            var ress = new DailyAuditViewModel();
            ress.AllStaffs = GetAllDrivers(connKey);
            ress.StaffTypes = GetAllStaffTypes(connKey);

            if (filter.StaffsSelected != null && filter.StaffsSelected.Length > 0)
            {
                var staffs = ress.AllStaffs.Where(s => filter.StaffsSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterStaffsSelected = "Staffs: " + string.Join(", ", staffs);
            }

            if (filter.StaffTypesSelected != null && filter.StaffTypesSelected.Length > 0)
            {
                var stafftypes = ress.StaffTypes.Where(s => filter.StaffTypesSelected.Contains(s.Value)).Select(s => s.Text).ToList();
                filterStaffTypesSelected = "Staff Types: " + string.Join(", ", stafftypes);
            }

            var ds = new DataSet();
            var table1 = DailyAuditDataset();

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

            var newfilteredResult = filteredResult.Where(s => s.Duty == 8000).ToList();
            newfilteredResult.AddRange(filteredResult.Where(s => s.Duty != 8000).ToList());

            if (newfilteredResult.Any())
            {
                foreach (var res in newfilteredResult)
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
                        filterStaffsSelected,
                        filterStaffTypesSelected,
                        res.TotalPs,
                        filterLocation);
                }
            }
            else
            {
                DataRow dr = table1.NewRow();
                dr["companyName"] = companyName;
                dr["DateRangeFilter"] = filterDateRange;
                dr["StaffsSelected"] = filterStaffsSelected;
                dr["StaffTypesSelected"] = filterStaffTypesSelected;
                dr["LocationSelected"] = filterLocation;
                table1.Rows.Add(dr);
            }



            ds.Tables.Add(table1);
            return ds;
        }

        public List<DailyAuditData> GetDailyAuditReportData(string connKey, DailyAuditViewModel filter)
        {
            var schs = new List<DailyAuditData>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand(sp3, myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@StafIds", filter.StaffsSelected == null ? "" : string.Join(",", filter.StaffsSelected));
                cmd.Parameters.AddWithValue("@StaffType", filter.StaffTypesSelected == null ? "" : string.Join(",", string.Join(",", filter.StaffTypesSelected)));
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

                    schs.Add(sch);
                }
            }
            finally
            {
                myConnection.Close();
            }
            return schs;
        }

        public List<SchVsWorked> GetScheduledVsOperatedData(string connKey, SchVsOprViewModel filter, string spName, bool isformE = false, bool isHomeScreen = false)
        {
            var schs = new List<SchVsWorked>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand(spName, myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@DutyId", filter.DutiesSelected == null ? "" : string.Join(",", filter.DutiesSelected));
                cmd.Parameters.AddWithValue("@Contracts", filter.ContractsSelected == null ? "" : string.Join(",", string.Join(",", filter.ContractsSelected)));

                var dateRangeString = GetDateRangeString(filter.StartDate, filter.EndDate);

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

                    if (isHomeScreen)
                    {
                        if (dr["bit_ReveOrDead"] != null && dr["bit_ReveOrDead"].ToString() != string.Empty)
                        {
                            sch.bit_ReveOrDead = Convert.ToBoolean(dr["bit_ReveOrDead"]);
                        }
                    }

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

                    if (isformE && dr["routeName"] != null && dr["routeName"].ToString() != string.Empty)//only for Form-E
                    {
                        sch.routeName = dr["routeName"].ToString();
                    }

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
            finally
            {
                myConnection.Close();
            }
            return schs;
        }

        public List<JourneyAnalysisSummaryBySubRoute> GetJourneyAnalysisSummaryBySubRouteData(string connKey, JourneyAnalysisSummaryBySubRouteViewModel filter, string spName)
        {
            var schs = new List<JourneyAnalysisSummaryBySubRoute>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand(spName, myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Classes", filter.ClassesSelected == null ? "" : string.Join(",", filter.ClassesSelected));
                cmd.Parameters.AddWithValue("@ClassesType", filter.ClassesTypeSelected == null ? "" : string.Join(",", filter.ClassesTypeSelected));
                cmd.Parameters.AddWithValue("@Routes", filter.RoutesSelected == null ? "" : string.Join(",", filter.RoutesSelected));
                cmd.Parameters.AddWithValue("@Contracts", filter.ContractsSelected == null ? "" : string.Join(",", string.Join(",", filter.ContractsSelected)));

                var dateRangeString = GetDateRangeString(filter.StartDate, filter.EndDate);

                if (dateRangeString == string.Empty)
                {
                    return new List<JourneyAnalysisSummaryBySubRoute>();
                }

                cmd.Parameters.AddWithValue("@dateSelected", dateRangeString);
                cmd.CommandTimeout = 500000;

                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var sch = new JourneyAnalysisSummaryBySubRoute();

                    if (dr["int4_DutyID"] != null && dr["int4_DutyID"].ToString() != string.Empty)
                    {
                        sch.DutyID = dr["int4_DutyID"].ToString();
                    }

                    if (dr["str4_JourneyNo"] != null && dr["str4_JourneyNo"].ToString() != string.Empty)
                    {
                        sch.str4_JourneyNo = dr["str4_JourneyNo"].ToString();
                    }

                    if (dr["RouteNumber"] != null && dr["RouteNumber"].ToString() != string.Empty && dr["SubRouteNumber"] != null && dr["SubRouteNumber"].ToString() != string.Empty)
                    {
                        sch.RouteNumber = dr["RouteNumber"].ToString() + " - " + dr["SubRouteNumber"].ToString();
                    }

                    if (dr["int4_JourneyTickets"] != null && dr["int4_JourneyTickets"].ToString() != string.Empty)
                    {
                        sch.int4_JourneyTickets = dr["int4_JourneyTickets"].ToString();
                    }

                    if (dr["int4_JourneyPasses"] != null && dr["int4_JourneyPasses"].ToString() != string.Empty)
                    {
                        sch.int4_JourneyPasses = dr["int4_JourneyPasses"].ToString();
                    }

                    if (dr["TripStatus"] != null && dr["TripStatus"].ToString() != string.Empty)
                    {
                        sch.TripStatus = dr["TripStatus"].ToString();
                    }

                    if (dr["Int_TotalPassengers"] != null && dr["Int_TotalPassengers"].ToString() != string.Empty)
                    {
                        sch.Int_TotalPassengers = Convert.ToInt64(dr["Int_TotalPassengers"].ToString());
                    }

                    if (dr["SubRouteName"] != null && dr["SubRouteName"].ToString() != string.Empty)
                    {
                        sch.routeName = dr["SubRouteName"].ToString().Trim();
                    }

                    if (dr["int4_JourneyRevenue"] != null && dr["int4_JourneyRevenue"].ToString() != string.Empty)
                    {
                        sch.JourneyRevenue = dr["int4_JourneyRevenue"].ToString();
                    }

                    if (dr["str7_Contract"] != null && dr["str7_Contract"].ToString() != string.Empty)
                    {
                        sch.Contract = dr["str7_Contract"].ToString();
                    }

                    if (dr["dat_JourneyStartDate"] != null && dr["dat_JourneyStartDate"].ToString() != string.Empty)
                    {
                        var datePart = dr["dat_JourneyStartDate"].ToString().Split(' ')[0];
                        var date = datePart.Split('/');
                        var mont = (date[0].Length == 1 ? "0" + date[0] : date[0]).Trim();

                        sch.dateSelected = (date[1].Length == 1 ? "0" + date[1] : date[1]).Trim() + "/" + mont + "/" + date[2].Trim();
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

        private string GetDateRangeString(string fromDate, string tillDate)
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

        public List<string> GetAllContacts(string connKey)
        {
            var res = new List<string>();
            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "select distinct(str7_contract) as 'Contracts' from AllDutyScheduledInfo;";
                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var str = string.Empty;
                    if (dr["Contracts"] != null && dr["Contracts"].ToString() != string.Empty)
                    {
                        str = dr["Contracts"].ToString();
                        res.Add(str);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return res;
        }

        private List<SelectListItem> GetAllLocations(string connKey)
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

                cmd.CommandText = "select  * from locationgroup";
                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();

                    if (dr["str50_LocationGroupName"] != null && dr["str50_LocationGroupName"].ToString() != string.Empty)
                    {
                        obj.Text = dr["str50_LocationGroupName"].ToString();
                    }
                    if (dr["str4_LocationCode"] != null && dr["str4_LocationCode"].ToString() != string.Empty)
                    {
                        obj.Value = dr["str4_LocationCode"].ToString();
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

        public List<SelectListItem> GetAllDrivers(string connKey)
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

                cmd.CommandText = "select int4_StaffID, str50_StaffName from staff";
                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();

                    if (dr["str50_StaffName"] != null && dr["str50_StaffName"].ToString() != string.Empty)
                    {
                        obj.Text = dr["str50_StaffName"].ToString() + " - " + dr["int4_StaffID"].ToString(); ;
                    }
                    if (dr["int4_StaffID"] != null && dr["int4_StaffID"].ToString() != string.Empty)
                    {
                        obj.Value = dr["int4_StaffID"].ToString();
                    }
                    res.Add(obj);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return res.OrderBy(s => Convert.ToInt32(s.Value)).ToList();
        }

        public List<SelectListItem> GetAllStaffTypes(string connKey)
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

                cmd.CommandText = "select int4_StaffTypeID,str25_Description from stafftype";
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

        public List<SelectListItem> GetAllDuties(string connKey)
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

                cmd.CommandText = @"select int4_dutyid as 'Duties',str30_Description as 'desc' from qdduty
                                    where int4_dutyid >0
                                    order by int4_dutyid;";
                cmd.CommandTimeout = 500000;

                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();
                    if (dr["Duties"] != null)
                    {
                        obj.Value = dr["Duties"].ToString();
                    }
                    if (dr["desc"] != null)
                    {
                        obj.Text = obj.Value + " - " + dr["desc"].ToString();
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