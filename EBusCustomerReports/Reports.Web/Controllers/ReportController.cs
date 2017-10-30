using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;
using Helpers;
using Reports.Services;
using Helpers.Security;
using System.Threading;
using Reports.Services.Models;
using System.Data;
using Reports.Services.Helpers;
using Reports.Web.Helpers;
using System.Configuration;
using System.Text;

namespace Reports.Web.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        public const string sp2 = "EbusScheduledVsOpr"; // Sch vs Opr
        public const string sp3 = "EbusDailuAudit"; // Daily audit
        public const string sp4 = "EbusNotScheduledbutWorked";//"Sample4"; // Not Scheduled, but Worked
        public const string sp5 = "EbusScheduledbutnotWorked";//"Sample5"; // Scheduled, but not Worked
        public const string sp6 = "EbusHomeScreen";//"Sample6"; // Home Screen
        public const string sp7 = "EbusFormE"; // Form-E
        public const string sp8 = "EbusRevenueByDuty"; // Form-E

        //Home
        public ActionResult Index()
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var model = new ReportsService().GetHomeScreenData(conKey);
            return View(model);
        }

        #region MonthRevenueGraph

        public ActionResult MonthRevenueGraph()
        {
            var model = new MonthyRevenueFilter();
            return View(model);
        }

        public JsonResult GetMonthRevenueGraphData(string years, string months)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;

            var service = new SalesAnalysisService();

            var list = service.GetMonthRevenueData(conKey, months, years);

            var jsonData = new
            {
                Data = list,
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region DailyAuditByCashierTerminal
        public ActionResult DailyAuditByCashierTerminal()
        {
            var model = new CashierReportSummaryFilter();
            var service = new InspectorReportService();
            var staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.Cashiers = staff.Where(s => s.OperatorType.ToLower() == "cashier".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.StaffList = staff.Where(s => s.OperatorType.ToLower() == "driver".ToLower().Trim() || s.OperatorType.ToLower() == "seller".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Terminals = service.GetAllTerminals(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("DailyAuditByCashierTerminal", model);
        }

        public ActionResult DailyAuditByCashierTerminalSummary()
        {
            var model = new CashierReportSummaryFilter();
            var service = new InspectorReportService();
            var staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.Cashiers = staff.Where(s => s.OperatorType.ToLower() == "cashier".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.StaffList = staff.Where(s => s.OperatorType.ToLower() == "driver".ToLower().Trim() || s.OperatorType.ToLower() == "seller".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Terminals = service.GetAllTerminals(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("DailyAuditByCashierTerminalSummary", model);
        }

        public ActionResult DownloadDailyAuditByCashierTerminalReport(CashierReportSummaryFilter filter)
        {
            var userset = GetUserSettings();
            var ds = new SmartCardService().GetDailyAuditByCashierTerminalDataset(userset.ConnectionKey, filter, userset.CompanyName);
            if (ds.Tables[0].Rows.Count > 0)
            {
                var key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
                if (key.Equals("atamelang70"))
                { return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/Cashier/DailyAuditByCashierTerminalAtamelangTGX.rpt", ds, "DailyAuditByCashierTerminal "); }
                else
                { return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/Cashier/DailyAuditByCashierTerminal.rpt", ds, "DailyAuditByCashierTerminal "); }

            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("DailyAuditByCashierTerminal", "Report");
            }

        }

        public ActionResult DownloadDailyAuditByCashierTerminalReportSummary(CashierReportSummaryFilter filter)
        {
            var userset = GetUserSettings();
            var ds = new SmartCardService().GetDailyAuditByCashierTerminalDatasetSummary(userset.ConnectionKey, filter, userset.CompanyName);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/Cashier/DailyAuditByCashierTerminalSummary.rpt", ds, "DailyAuditByCashierTerminal ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("DailyAuditByCashierTerminalSummary", "Report");
            }
        }

        #endregion

        #region CashierSummaryReport

        public ActionResult CashierReportAt()
        {
            var model = new CashierReportSummaryFilter();
            var service = new InspectorReportService();
            var staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.Cashiers = staff.Where(s => s.OperatorType.ToLower() == "Cashier".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Terminals = service.GetAllTerminals(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("CashierSummaryReport", model);
        }

        public ActionResult DownloadCashierSummaryReport(CashierReportSummaryFilter filter)
        {
            var userset = GetUserSettings();
            var ds = new SmartCardService().GetCashierSummaryReportDataset(userset.ConnectionKey, userset.CompanyName, filter);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/Cashier/CashierSummary.rpt", ds, "CashierReport ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("CashierReportAt", "Report");
            }
        }


        public ActionResult NewCashierReportAt()
        {
            var model = new CashierReportSummaryFilter();
            var service = new InspectorReportService();
            var staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.Cashiers = staff.Where(s => s.OperatorType.ToLower() == "Cashier".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Terminals = service.GetAllTerminals(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("NewCashierSummaryReport", model);
        }

        public ActionResult DownloadNewCashierSummaryReport(CashierReportSummaryFilter filter)
        {
            var userset = GetUserSettings();
            var ds = new SmartCardService().GetCashierSummaryReportDataset(userset.ConnectionKey, userset.CompanyName, filter);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/Cashier/NewCashierSummary.rpt", ds, "NewCashierReport ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("CashierReportAt", "Report");
            }

        }
        #endregion

        #region CashierSummaryReport

        public ActionResult CashierReconciliation()
        {
            var model = new CashierReportSummaryFilter();
            var service = new InspectorReportService();
            var staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.StaffList = staff.Where(s => s.OperatorType.ToLower() == "driver".ToLower().Trim() || s.OperatorType.ToLower() == "seller".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            return View("CashierReconciliationReport", model);
        }


        public ActionResult DownloadCashierReconciliationReport(CashierReportSummaryFilter filter)
        {
            var userset = GetUserSettings();
            var ds = new SmartCardService().GetCashierReconciliationReportDataset(userset.ConnectionKey, userset.CompanyName, filter);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/Cashier/CashierReconciliationSummay.rpt", ds, "CashierReconciliation ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("CashierReconciliation", "Report");
            }

        }

        #endregion

        #region EarlyLateRunning

        public ActionResult EarlyLateRunning()
        {
            var model = new ReportsService().GetEarlyLateRunningModel(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View(model);
        }

        public ActionResult DownloadEarlyLateRunning(EarlyLateRunningModel filter)
        {
            var userset = GetUserSettings();
            var ds = new ReportsService().GetEarlyLateRunningReport(userset.ConnectionKey, filter, Convert.ToInt32(filter.TimeSelected), userset.CompanyName);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(true, "~/CrystalReports/Rpt/EarlyLateRunning.rpt", ds, "EarlyLateRunning ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("EarlyLateRunning", "Report");
            }

        }

        #endregion

        #region CashVsSmartCardUsageByRoute

        public ActionResult CashVsSmartCardUsageByRoute()
        {
            var model = new CashVsSmartCardUsageByRouteFilter();
            var userset = GetUserSettings();
            model.Routes = new SmartCardService().GetAllDriverRoutes(userset.ConnectionKey);
            return View(model);
        }

        [HttpGet]
        public PartialViewResult GetCashVsSmartCardUsageByRouteData(string fromdate, string todate, string routes)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;

            var service = new SmartCardService();

            if (string.IsNullOrEmpty(fromdate))
            {
                fromdate = DateTime.Now.AddDays(-30).ToString("dd-MM-yyyy");
            }

            if (string.IsNullOrEmpty(todate))
            {
                todate = DateTime.Now.AddDays(0).ToString("dd-MM-yyyy");
            }

            var list = service.GetCashVsSmartCardUsageByRouteData(conKey, routes, fromdate, todate);

            return PartialView("CashVsSmartCardTable", list);
        }


        public ActionResult DownloadCashVsSmartCardUsageByRoute(string fromdate, string todate, string routes)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;

            var service = new SmartCardService();

            if (string.IsNullOrEmpty(todate))
            {
                fromdate = DateTime.Now.AddDays(-7).ToString("dd-MM-yyyy");
            }

            if (string.IsNullOrEmpty(todate))
            {
                todate = DateTime.Now.AddDays(0).ToString("dd-MM-yyyy");
            }

            var ds = service.GetCashVsSmartCardUsageByRouteDataSet(conKey, routes, fromdate, todate, comp);

            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(true, "~/CrystalReports/Rpt/SalesAnalysis/CashVsSmartCardUsageByRoute.rpt", ds, "CashVsSmartCardUsageByRoute ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("CashVsSmartCardUsageByRoute", "Report");
            }
        }

        #endregion

        #region YearlyBreakDown

        public ActionResult YearlyBreakDownByRoute()
        {
            var model = new YearlyBreakDownFilter();
            model.Classes = new SalesAnalysisService().GetAllCalsses(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.RoutesList = new SalesAnalysisService().GetAllRoutes(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View(model);
        }

        public ActionResult YearlyBreakDownByRouteDownload(YearlyBreakDownFilter filter)
        {
            var userset = GetUserSettings();

            var ds = new SalesAnalysisService().GetYearlyBreakDownByRouteDataSet(userset.ConnectionKey, userset.CompanyName, filter);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/SalesAnalysis/YearlyBreakDownByRoute.rpt", ds, "YearlyBreakDownByRoute ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("YearlyBreakDownByRoute", "Report");
            }

        }

        public ActionResult YearlyBreakDown()
        {
            var model = new YearlyBreakDownFilter();
            model.Classes = new SalesAnalysisService().GetAllCalsses(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View(model);
        }

        public ActionResult YearlyBreakDownDownload(YearlyBreakDownFilter filter)
        {
            var userset = GetUserSettings();

            var ds = new SalesAnalysisService().GetYearlyBreakDownDataSet(userset.ConnectionKey, userset.CompanyName, filter);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/SalesAnalysis/YearlyBreakDown.rpt", ds, "YearlyBreakDown ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("YearlyBreakDown", "Report");
            }

        }

        #endregion

        #region Rpt

        public ActionResult SmartCardUsage()
        {
            var model = new SmartCardUsageFilter();

            return View(model);
        }


        public ActionResult SmartCardUsageDownload(SmartCardUsageFilter filter)
        {

            var userset = GetUserSettings();

            var service = new CashierServices();

            var ds = new SmartCardService().GetSmartCardUsageData(userset.ConnectionKey, userset.CompanyName, filter.NumberOfTimesUsed.Value, filter.StartDate, filter.EndDate);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/SmartCardTransaction/SmartCardUsage.rpt", ds, "Smart card usage");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("SmartCardUsage", "Report");
            }

        }
        #endregion

        #region DutyTable

        public ActionResult TimeTable()
        {
            var model = new DutySheetsViewModel();
            var settings = GetUserSettings();
            var allDuties = new ReportsService().GetAllDuties(settings.ConnectionKey);
            var allcons = new ReportsService().GetAllContacts(settings.ConnectionKey);

            model.Duties = allDuties;

            model.Contracts = (from f in allcons
                               select new SelectListItem { Selected = false, Text = f, Value = f }).ToList();

            return View(model);
        }

        public ActionResult TimeTableDownload(DutySheetsViewModel filter)
        {

            var userset = GetUserSettings();
            var duties = filter.DutiesSelected == null ? "" : string.Join(",", filter.DutiesSelected);
            var consSel = filter.ContractsSelected == null ? "" : string.Join(",", filter.ContractsSelected);

            var service = new CashierServices();

            var ds = new DutySheetsService().GetTimeTableDetails(userset.ConnectionKey, userset.CompanyName, filter.ShowAllOperatingDays, filter.DutyDate, duties, consSel);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/DriverTrans/TimeTable.rpt", ds, "Time Table");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("TimeTable", "Report");
            }

        }

        #endregion

        #region DutySheetsReport

        public ActionResult DutySheets()
        {
            var model = new DutySheetsViewModel();
            var settings = GetUserSettings();
            var allDuties = new ReportsService().GetAllDuties(settings.ConnectionKey);
            model.Duties = allDuties;

            return View(model);
        }

        public ActionResult DutySheetsDownload(DutySheetsViewModel filter)
        {

            var userset = GetUserSettings();
            var duties = filter.DutiesSelected == null ? "" : string.Join(",", filter.DutiesSelected);

            var service = new CashierServices();

            var ds = new DutySheetsService().GetDutySheetDetails(userset.ConnectionKey, userset.CompanyName, filter.ShowAllOperatingDays, filter.DutyDate, duties);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/DriverTrans/DutySheets.rpt", ds, "DutySheets");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("DutySheets", "Report");
            }

        }

        #endregion

        #region SmartCardHotlisting

        public ActionResult SmartCardHotlisting()
        {
            var model = new SmartCardHotList();
            model.Reasons = new SmartCardService().GetAllHotlistReasons(GetUserSettings().ConnectionKey);
            return View(model);
        }

        [HttpPost]
        public ActionResult SubmitCardForHotlist(SmartCardHotList model)
        {
            var settings = GetUserSettings();
            var message = "Request for Smart Card Hotlisting sent Sucessfully !";
            model.CreatedBy = settings.Username;
            model.CreatedDate = DateTime.Now;
            var result = new SmartCardService().SaveSmartCardHolistingRequest(settings.ConnectionKey, model);

            if (result.IsDuplicate == true)
            {
                message = string.Format("Holisting request for this number is already sent by {0} on {1}", result.CreatedBy, result.CreatedDate.ToString("MM-dd-yyyy"));
            }
            else
            {
                var reasons = new SmartCardService().GetAllHotlistReasons(GetUserSettings().ConnectionKey);
                var reason = reasons.Where(x => x.Value.Equals(model.ReasonSelected)).Select(x => x.Text).FirstOrDefault().ToString();
                var res = MailHelper.SendMailToEbus(model.SmartCardNubmer, reason, model.Comments, settings.Username, settings.CompanyName);
                if (res == false)
                {
                    message = "Couldn't send request, Please try after some time !";
                }
            }

            return Json(message, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region DriverTransactionDetailsReport

        public ActionResult DriverDetails()
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var model = new InspectorReportService().GetDriverDetailsReportFilter(conKey);
            return View(model);
        }

        public ActionResult GetDriverTransactionDetailsDownload(DriverDetailsFilter filter)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;

            var service = new CashierServices();

            var ds = new InspectorReportService().GetDriverTransactionDetails(conKey, CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate), Convert.ToInt32(filter.DriversSelected), comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/DriverTrans/DriverTransDetails.rpt", ds, "DriverDetails");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("DriverDetails", "Report");
            }

        }

        #endregion

        #region ScheduledVsOperatedReport

        public ActionResult RevenueByDuty()
        {
            var model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("RevenueByDuty", model);
        }
        public ActionResult RevenueByDutyDownload(SchVsOprViewModel filters)
        {
            var key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            return DownloadRevenueByDutyReport(filters, Server.MapPath("~/CrystalReports/Rpt/RevenueByDuty.rpt"), "RevenueByDuty " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp8);
        }

        public ActionResult RevenueByDutyAll()
        {
            var model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("RevenueByDutyAll", model);
        }
        public ActionResult RevenueByDutyAllDownload(SchVsOprViewModel filters)
        {
            var key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            return DownloadRevenueByDutyAllReport(filters, Server.MapPath("~/CrystalReports/Rpt/RevenueByDutyAll.rpt"), "RevenueByDuty " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp8);
        }

        //1
        public ActionResult ScheduledVsOperatedReport()
        {
            var model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("ScheduledVsOperated", model);
        }
        public ActionResult ScheduledVsOperatedDownload(SchVsOprViewModel filters)
        {
            var key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            if (key.Equals("atamelang70"))
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/ScheduledVsOperatedOkAtamelangTGX.rpt"), "ScheduledVsOperated " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp2); }
            else
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/ScheduledVsOperatedOk.rpt"), "ScheduledVsOperated " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp2); }

        }

        //2
        public ActionResult NotScheduledButOperatedReport()
        {
            var model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("NotScheduledButOperated", model);
        }
        public ActionResult NotScheduledButOperatedDownload(SchVsOprViewModel filters)
        {
            var key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            if (key.Equals("atamelang70"))
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/NotScheduledButOperatedAtamelangTGX.rpt"), "Operated Not Scheduled " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp4); }
            else
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/NotScheduledButOperated.rpt"), "Operated Not Scheduled " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp4); }
        }

        //3
        public ActionResult ScheduledNotOperatedReport()
        {
            var model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("ScheduledNotOperated", model);
        }
        public ActionResult ScheduledNotOperatedDownload(SchVsOprViewModel filters)
        {
            return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/ScheduledNotOperated.rpt"), "ScheduledNotOperated " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp5);
        }

        public ActionResult DownloadReportGeneric(SchVsOprViewModel filters, string rptPath, string fileName, string spName, bool isFormE = false)
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            var service = new ReportsService();

            var ds = isFormE ? service.GetFormEReport(conKey, filters, spName, comp, isFormE) : service.GetScheduledVsOperatedReport(conKey, filters, spName, comp, isFormE);
            if (ds.Tables[0].Rows.Count > 0)
            {
                var fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                var rptH = new ReportClass
                {
                    FileName = rptPath
                };

                try
                {
                    rptH.Load();

                    rptH.SetDataSource(ds.Tables[0]);

                    rptH.ExportToHttpResponse(fileType, System.Web.HttpContext.Current.Response, true, fileName);

                    return new DownloadPdfResult(rptH, fileName);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    rptH.Close();
                    rptH.Dispose();
                }

            }
            else
            {
                TempData["AlertMessage"] = "show";
                if (fileName.Contains("Operated Not Scheduled"))
                    return RedirectToAction("NotScheduledButOperatedReport", "Report");
                if (fileName.Contains("ScheduledNotOperated"))
                    return RedirectToAction("ScheduledNotOperatedReport", "Report");
                if (fileName.Contains("ScheduledVsOperated"))
                    return RedirectToAction("ScheduledVsOperatedReport", "Report");
                if (fileName.Contains("FormE"))
                    return RedirectToAction("FormEReport", "Report");
            }
            return RedirectToAction("Index", "Report");
        }

        public ActionResult DownloadFullDuty(SchVsOprViewModel filters, string rptPath, string fileName, string spName, bool isFormE = false)
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            var service = new ReportsService();

            var ds = service.FullDutySummary(conKey, filters, spName, comp, isFormE);//full duty report
            if (ds.Tables[0].Rows.Count > 0)
            {
                var fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                var rptH = new ReportClass
                {
                    FileName = rptPath
                };

                try
                {
                    rptH.Load();

                    rptH.SetDataSource(ds.Tables[0]);

                    rptH.ExportToHttpResponse(fileType, System.Web.HttpContext.Current.Response, true, fileName);

                    return new DownloadPdfResult(rptH, fileName);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    rptH.Close();
                    rptH.Dispose();
                }

            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("FullDutySummary", "Report");
            }
        }

        public void ExportRevenueByDutyAllToExcel(List<RevenueByDuty> collection, string fileName)
        {
            string duties = "No Filter Applied", filterdate = "", customer = "";
            if (collection.Any())
            {
                duties = collection.FirstOrDefault().DutyFilter;
                filterdate = collection.FirstOrDefault().DateRangeFilter;
                customer = collection.FirstOrDefault().companyName;
            }
            StringBuilder htmlcontent = new StringBuilder();
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.ClearContent();
            System.Web.HttpContext.Current.Response.ClearHeaders();
            System.Web.HttpContext.Current.Response.Buffer = true;
            System.Web.HttpContext.Current.Response.ContentType = "application/ms-excel";
            System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName + ".xls");

            System.Web.HttpContext.Current.Response.Charset = "utf-8";
            System.Web.HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1250");
            htmlcontent.Append(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");
            ////sets font
            htmlcontent.Append("<font style='font-size:10.0pt; font-family:Calibri;'>");
            htmlcontent.Append("<BR><BR><BR>");
            //sets the table border, cell spacing, border color, font of the text, background, foreground, font height
            htmlcontent.Append("<Table>");
            htmlcontent.Append("<tr><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:50px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td></tr>");
            htmlcontent.Append("<tr><td colspan='8' align='center' style='width:750px;'><b> REVENUE BY DUTY</b></td><td rowspan='6' colspan='2' style='width:200px;' align='right'><img src='http://41.76.211.195/EBusBackOffice/Images/logo_comp1.png' /></td></tr>");
            htmlcontent.Append("<tr><td colspan='8' align='center' style='width:750px;'><b>" + customer + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='8' style='width:750px;'><b>" + filterdate + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='8' style='width:750px;'><b>" + duties + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='8' style='width:750px;'></td></tr>");
            collection.OrderBy(x => x.DutyID).ThenBy(x => x.Total);
            //am getting my grid's column headers
            int columnscount = collection.Count;
            var prevDuty = ""; double prevTotal = 0;
            double revTotal = 0, nonRevTotal = 0;
            double adultRevTotal = 0, childRevTotal = 0, adultNonRevTotal = 0, schNonRevTotal = 0, adultTrnsferTotal = 0, scholarTransferTotal = 0;
            for (var i = 0; i < columnscount; i++)
            {
                var curItem = collection[i];
                if (prevDuty != curItem.DutyID && prevTotal != curItem.Total)
                {
                    if (i != 0)
                    {
                        //last sub total row
                        htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-bottom: thin solid black;'><b>Sub Totals:</b></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultRevTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + childRevTotal + "</u></td>");
                        htmlcontent.Append("<td align='right' style='width:100px;border-bottom: thin solid black;border-bottom: thin solid black;'><u>R " + revTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:50px;border-bottom: thin solid black;'></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultNonRevTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultTrnsferTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + schNonRevTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + scholarTransferTotal + "</u></td>");
                        htmlcontent.Append("<td align='right' style='width:100px;border-right: thin solid black;border-bottom: thin solid black;'><u>R " + nonRevTotal + "</u></td></tr>");
                        revTotal = 0; nonRevTotal = 0; adultRevTotal = 0; childRevTotal = 0; adultNonRevTotal = 0; schNonRevTotal = 0; adultTrnsferTotal = 0; scholarTransferTotal = 0;
                    }
                    //New row started
                    htmlcontent.Append("<tr><td colspan='10' style='width:750px;'></td></tr>");
                    htmlcontent.Append("<tr><td colspan='2' style='width:200px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;'><b>Duty Number:</b>" + curItem.DutyID + "</td>");
                    htmlcontent.Append("<td align='left' style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:50px;border-top: thin solid black;border-bottom: thin solid black'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black' align='right' ><b>Total:</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'>R " + curItem.Total + "</td></tr>");

                    //Next to header
                    htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;' align='right'><b>Journey</b></td>");
                    htmlcontent.Append("<td colspan='3' align='center' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Revenue</b></td> ");
                    htmlcontent.Append("<td style='width:50px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'></td> ");
                    htmlcontent.Append("<td colspan='5' align='center' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Non Revenue</b></td></tr>");

                    //Row next to sub header 
                    htmlcontent.Append("<tr><td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Adult</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Child</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Cash</b></td>");
                    htmlcontent.Append("<td align='right' style='width:50px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Adult</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Adult Transfer</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Scholar</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Scholar Transfer</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Value</b></td></tr>");
                }
                revTotal += curItem.Cash;
                nonRevTotal += curItem.Value;
                adultRevTotal += curItem.AdultRevenue;
                childRevTotal += curItem.ChildRevenue;
                adultNonRevTotal += curItem.AdultNonRevenue;
                schNonRevTotal += curItem.SchlorNonRevenue;
                adultTrnsferTotal += curItem.AdultTransfer;
                scholarTransferTotal += curItem.ScholarTransfer;

                htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-bottom: thin solid black;'>" + curItem.JourneyID + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.AdultRevenue + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.ChildRevenue + "</td>");
                htmlcontent.Append("<td align='right' style='width:100px;border-bottom: thin solid black;'>R " + curItem.Cash + "</td>");
                htmlcontent.Append("<td style='width:50px;border-bottom: thin solid black;'></td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.AdultNonRevenue + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.AdultTransfer + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.SchlorNonRevenue + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.ScholarTransfer + "</td>");
                htmlcontent.Append("<td align='right' style='width:100px;border-right: thin solid black;border-bottom: thin solid black;'>R " + curItem.Value + "</td></tr>");
                prevDuty = curItem.DutyID;
                prevTotal = curItem.Total;

                if (i == columnscount - 1)
                {
                    htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-bottom: thin solid black;'><b>Sub Totals:</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultRevTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + childRevTotal + "</u></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-bottom: thin solid black;border-bottom: thin solid black;'><u>R " + revTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:50px;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultNonRevTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultTrnsferTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + schNonRevTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + scholarTransferTotal + "</u></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-right: thin solid black;border-bottom: thin solid black;'><u>R " + nonRevTotal + "</u></td></tr>");
                    revTotal = 0; nonRevTotal = 0; adultRevTotal = 0; childRevTotal = 0; adultNonRevTotal = 0; schNonRevTotal = 0;adultTrnsferTotal = 0; scholarTransferTotal = 0;
                }
            }
            htmlcontent.Append("</Table>");
            htmlcontent.Append("</font>");
            System.Web.HttpContext.Current.Response.Write(htmlcontent.ToString());
            System.Web.HttpContext.Current.Response.Flush();
            System.Web.HttpContext.Current.Response.End();
        }

        public void ExportRevenueByDutyToExcel(List<RevenueByDuty> collection, string fileName)
        {
            string duties = "No Filter Applied", filterdate = "", customer = "";
            if (collection.Any())
            {
                duties = collection.FirstOrDefault().DutyFilter;
                filterdate = collection.FirstOrDefault().DateRangeFilter;
                customer = collection.FirstOrDefault().companyName;
            }
            StringBuilder htmlcontent = new StringBuilder();
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.ClearContent();
            System.Web.HttpContext.Current.Response.ClearHeaders();
            System.Web.HttpContext.Current.Response.Buffer = true;
            System.Web.HttpContext.Current.Response.ContentType = "application/ms-excel";
            System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName + ".xls");

            System.Web.HttpContext.Current.Response.Charset = "utf-8";
            System.Web.HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1250");
            htmlcontent.Append(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");
            ////sets font
            htmlcontent.Append("<font style='font-size:10.0pt; font-family:Calibri;'>");
            htmlcontent.Append("<BR><BR><BR>");
            //sets the table border, cell spacing, border color, font of the text, background, foreground, font height
            htmlcontent.Append("<Table>");
            htmlcontent.Append("<tr><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:50px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td></tr>");
            htmlcontent.Append("<tr><td colspan='6' align='center' style='width:550px;'><b> REVENUE BY DUTY</b></td><td rowspan='6' colspan='2' style='width:200px;' align='right'><img src='http://41.76.211.195/EBusBackOffice/Images/logo_comp1.png' /></td></tr>");
            htmlcontent.Append("<tr><td colspan='6' align='center' style='width:550px;'><b>" + customer + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='6' style='width:550px;'><b>" + filterdate + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='6' style='width:550px;'><b>" + duties + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='6' style='width:550px;'></td></tr>");
            collection.OrderBy(x => x.DutyID).ThenBy(x => x.Total);
            //am getting my grid's column headers
            int columnscount = collection.Count;
            var prevDuty = ""; double prevTotal = 0;
            double revTotal = 0, nonRevTotal = 0;
            double adultRevTotal = 0, childRevTotal = 0, adultNonRevTotal = 0, schNonRevTotal = 0;
            for (var i = 0; i < columnscount; i++)
            {
                var curItem = collection[i];
                if (prevDuty != curItem.DutyID && prevTotal != curItem.Total)
                {
                    if (i != 0)
                    {
                        //last sub total row
                        htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-bottom: thin solid black;'><b>Sub Totals:</b></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultRevTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + childRevTotal + "</u></td>");
                        htmlcontent.Append("<td align='right' style='width:100px;border-bottom: thin solid black;border-bottom: thin solid black;'><u>R " + revTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:50px;border-bottom: thin solid black;'></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultNonRevTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + schNonRevTotal + "</u></td>");
                        htmlcontent.Append("<td align='right' style='width:100px;border-right: thin solid black;border-bottom: thin solid black;'><u>R " + nonRevTotal + "</u></td></tr>");
                        revTotal = 0; nonRevTotal = 0; adultRevTotal = 0; childRevTotal = 0; adultNonRevTotal = 0; schNonRevTotal = 0;
                    }
                    //New row started
                    htmlcontent.Append("<tr><td colspan='8' style='width:750px;'></td></tr>");
                    htmlcontent.Append("<tr><td colspan='2' style='width:200px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;'><b>Duty Number:</b>" + curItem.DutyID + "</td>");
                    htmlcontent.Append("<td align='left' style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:50px;border-top: thin solid black;border-bottom: thin solid black'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black' align='right' ><b>Total:</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'>R " + curItem.Total + "</td></tr>");

                    //Next to header
                    htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;' align='right'><b>Journey</b></td>");
                    htmlcontent.Append("<td colspan='3' align='center' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Revenue</b></td> ");
                    htmlcontent.Append("<td style='width:50px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'></td> ");
                    htmlcontent.Append("<td colspan='3' align='center' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Non Revenue</b></td></tr>");

                    //Row next to sub header 
                    htmlcontent.Append("<tr><td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Adult</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Child</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Cash</b></td>");
                    htmlcontent.Append("<td align='right' style='width:50px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Adult</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Scholar</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Value</b></td></tr>");
                }
                revTotal += curItem.Cash;
                nonRevTotal += curItem.Value;
                adultRevTotal += curItem.AdultRevenue;
                childRevTotal += curItem.ChildRevenue;
                adultNonRevTotal += curItem.AdultNonRevenue;
                schNonRevTotal += curItem.SchlorNonRevenue;

                htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-bottom: thin solid black;'>" + curItem.JourneyID + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.AdultRevenue + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.ChildRevenue + "</td>");
                htmlcontent.Append("<td align='right' style='width:100px;border-bottom: thin solid black;'>R " + curItem.Cash + "</td>");
                htmlcontent.Append("<td style='width:50px;border-bottom: thin solid black;'></td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.AdultNonRevenue + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.SchlorNonRevenue + "</td>");
                htmlcontent.Append("<td align='right' style='width:100px;border-right: thin solid black;border-bottom: thin solid black;'>R " + curItem.Value + "</td></tr>");
                prevDuty = curItem.DutyID;
                prevTotal = curItem.Total;

                if (i == columnscount - 1)
                {
                    htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-bottom: thin solid black;'><b>Sub Totals:</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultRevTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + childRevTotal + "</u></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-bottom: thin solid black;border-bottom: thin solid black;'><u>R " + revTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:50px;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultNonRevTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + schNonRevTotal + "</u></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-right: thin solid black;border-bottom: thin solid black;'><u>R " + nonRevTotal + "</u></td></tr>");
                    revTotal = 0; nonRevTotal = 0; adultRevTotal = 0; childRevTotal = 0; adultNonRevTotal = 0; schNonRevTotal = 0;
                }
            }
            htmlcontent.Append("</Table>");
            htmlcontent.Append("</font>");
            System.Web.HttpContext.Current.Response.Write(htmlcontent.ToString());
            System.Web.HttpContext.Current.Response.Flush();
            System.Web.HttpContext.Current.Response.End();
        }

        public ActionResult DownloadRevenueByDutyReport(SchVsOprViewModel filters, string rptPath, string fileName, string spName)
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            var service = new ReportsService();

            var result = service.RevenueByDuty(conKey, filters, spName, comp);//full duty report
            var ds = result.Item1;
            if (ds.Tables[0].Rows.Count > 0)
            {
                var fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                if (fileType != CrystalDecisions.Shared.ExportFormatType.Excel)
                {
                    var rptH = new ReportClass
                    {
                        FileName = rptPath
                    };

                    try
                    {
                        rptH.Load();

                        rptH.SetDataSource(ds.Tables[0]);

                        rptH.ExportToHttpResponse(fileType, System.Web.HttpContext.Current.Response, true, fileName);

                        return new DownloadPdfResult(rptH, fileName);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        rptH.Close();
                        rptH.Dispose();
                    }
                }
                else
                {
                    ExportRevenueByDutyToExcel(result.Item2, fileName);
                    return RedirectToAction("RevenueByDuty", "Report"); ;
                }
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("RevenueByDuty", "Report");
            }
        }

        public ActionResult DownloadRevenueByDutyAllReport(SchVsOprViewModel filters, string rptPath, string fileName, string spName)
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            var service = new ReportsService();

            var result = service.RevenueByDutyAll(conKey, filters, spName, comp);//full duty report
            var ds = result.Item1;
            if (ds.Tables[0].Rows.Count > 0)
            {
                var fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                if (fileType != CrystalDecisions.Shared.ExportFormatType.Excel)
                {
                    var rptH = new ReportClass
                    {
                        FileName = rptPath
                    };

                    try
                    {
                        rptH.Load();

                        rptH.SetDataSource(ds.Tables[0]);

                        rptH.ExportToHttpResponse(fileType, System.Web.HttpContext.Current.Response, true, fileName);

                        return new DownloadPdfResult(rptH, fileName);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        rptH.Close();
                        rptH.Dispose();
                    }
                }
                else
                {
                    ExportRevenueByDutyAllToExcel(result.Item2, fileName);
                    return RedirectToAction("RevenueByDutyAll", "Report"); ;
                }
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("RevenueByDutyAll", "Report");
            }
        }

        #endregion

        #region DailyAudit

        public ActionResult SellersDailyAuditReport()
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var model = new ReportsService().GetFilterForDailyAudit(conKey);
            return View("SellersAudit", model);
        }

        public ActionResult DownloadSellersDailyAuditReport(DailyAuditViewModel filters)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;


            var ds = new ReportsService().GetDailyAuditReportForSellers(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                var fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                return DownLoadReportByDataSet(filters.ExcelOrPDF, "~/CrystalReports/Rpt/DailyAudit/SellersDailyAudit.rpt", ds, "SellersDailyAudit ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("SellersDailyAuditReport", "Report");
            }

        }


        public ActionResult DailyAuditReport()
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var model = new ReportsService().GetFilterForDailyAudit(conKey);
            return View("DailyAudit", model);
        }

        public ActionResult DailyAnalysisReport()
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var model = new ReportsService().GetFilterForDailyAudit(conKey);
            return View("DailyAnalysis", model);
        }

        public ActionResult DownloadDailyAuditReport(DailyAuditViewModel filters)
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            var ds = new ReportsService().GetDailyAuditReport(conKey, filters, comp);

            var fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

            ReportClass rptH = null;
            try
            {
                var fileName = "";
                var key = conKey.ToString().ToLower();
                if (key.Equals("atamelang70"))
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        rptH = new ReportClass
                        {
                            FileName = Server.MapPath("~/CrystalReports/Rpt/DailyAudit/DailyAnalysis.rpt")
                        };
                        rptH.Load();

                        rptH.SetDataSource(ds.Tables[0]);
                        fileName = "Daily Analysis " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss");

                        rptH.ExportToHttpResponse(fileType, System.Web.HttpContext.Current.Response, true, fileName);
                    }
                    else
                    {
                        TempData["AlertMessage"] = "show";
                        return RedirectToAction("DailyAnalysisReport", "Report");
                    }
                }
                else
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        rptH = new ReportClass
                        {
                            FileName = Server.MapPath("~/CrystalReports/Rpt/DailyAudit/DailyAudit.rpt")
                        };

                        rptH.Load();

                        rptH.SetDataSource(ds.Tables[0]);
                        fileName = "Daily Audit " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss");

                        rptH.ExportToHttpResponse(fileType, System.Web.HttpContext.Current.Response, true, fileName);
                    }
                    else
                    {
                        TempData["AlertMessage"] = "show";
                        return RedirectToAction("DailyAuditReport", "Report");
                    }
                }

                return new DownloadPdfResult(rptH, fileName);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                rptH.Close();
                rptH.Dispose();
            }
        }

        public ActionResult DailyAuditAtamelangReport()
        {
            var model = new CashierReportSummaryFilter();
            var service = new InspectorReportService();
            var staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.StaffList = staff.Where(s => s.OperatorType.ToLower() == "driver".ToLower().Trim() || s.OperatorType.ToLower() == "seller".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            return View("DailyAuditAtamelang", model);
        }

        public ActionResult DownloadDailyAuditAtamelangReport(CashierReportSummaryFilter filter)
        {
            var userset = GetUserSettings();
            filter.EndDate = filter.StartDate;
            var ds = new SmartCardService().GetDailyAuditAtamelangReportDataset(userset.ConnectionKey, filter, userset.CompanyName);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/DailyAudit/DailyAuditAtamelang.rpt", ds, "DailyAudit ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("DailyAuditAtamelangReport", "Report");
            }

        }

        #endregion

        #region FormEDetailed

        public ActionResult FormEDetailedReport()
        {
            var model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("FormEDetailed", model);
        }


        public ActionResult FormEDetailedReportDownload(SchVsOprViewModel filters)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;

            var service = new ReportsService();

            var ds = service.GetFormEDetailed(conKey, filters, sp7, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                var key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
                if (key.Equals("atamelang70"))
                { return DownLoadReportByDataSet(filters.ExcelOrPDF, "~/CrystalReports/Rpt/FormE/FormEDetailedAtamelangTGX.rpt", ds, "FormE Detailed "); }
                else
                { return DownLoadReportByDataSet(filters.ExcelOrPDF, "~/CrystalReports/Rpt/FormE/FormEDetailed.rpt", ds, "FormE Detailed "); }
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("FormEDetailedReport", "Report");
            }
        }

        #endregion

        #region FormE
        public ActionResult FormEReport()
        {
            var model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("FormE", model);
        }

        public ActionResult FormEReportDownload(SchVsOprViewModel filters)
        {
            var key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            if (key.Equals("atamelang70"))
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/FormE/FormEAtamelangTGX.rpt"), "FormE " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp7, true); }
            else
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/FormE/FormE.rpt"), "FormE " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp7, true); }
        }

        #endregion

        #region FormE2
        public ActionResult FullDutySummary()
        {
            var model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("FullDutySummary", model);
        }

        public ActionResult FullDutySummaryDownload(SchVsOprViewModel filters)
        {
            var key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            if (key.Equals("atamelang70"))
            { return DownloadFullDuty(filters, Server.MapPath("~/CrystalReports/Rpt/FormE/FullDutySummaryAtamelangTGX.rpt"), "FullDuty Summary " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp7, true); }
            else
            { return DownloadFullDuty(filters, Server.MapPath("~/CrystalReports/Rpt/FormE/FullDutySummary.rpt"), "FullDuty Summary " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp7, true); }
        }

        #endregion

        #region SmartCard Transactions

        public ActionResult SmartCardTransactions()
        {
            return View("SmartCardTransactions", new SmartCardTransFilter());
        }

        public ActionResult DownloadSmartCardTrans(SmartCardTransFilter filters)
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            var service = new SmartCardService();

            var ds = service.GetSmartCardTransDataSet(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                var fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                var rptH = new ReportClass
                {
                    FileName = Server.MapPath("~/CrystalReports/Rpt/SmartCardTransaction/SmartCardAudit.rpt")
                };

                try
                {
                    rptH.Load();

                    rptH.SetDataSource(ds.Tables[0]);

                    rptH.ExportToHttpResponse(fileType, System.Web.HttpContext.Current.Response, true, "Smart Card Trans " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"));

                    return new DownloadPdfResult(rptH, "Smart Card Trans " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"));
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    rptH.Close();
                    rptH.Dispose();
                }
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("SmartCardTransactions", "Report");
            }
        }

        #endregion

        #region DailyInspectorReport

        public ActionResult DailyInspectorReport()
        {
            var service = new InspectorReportService();
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            var model = service.GetInspectorReportFilter(conKey);

            return View("DailyInspectorReport", model);
        }

        public ActionResult DailyInspectorReportDownload(InspectorReportFilter filters)
        {
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            var comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            var service = new InspectorReportService();

            var ds = service.GetInspectorTransDataSet(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                var fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                var rptH = new ReportClass
                {
                    FileName = Server.MapPath("~/CrystalReports/Rpt/DailyInspector/DailyInspector.rpt")
                };

                try
                {
                    rptH.Load();

                    rptH.SetDataSource(ds.Tables[0]);

                    rptH.ExportToHttpResponse(fileType, System.Web.HttpContext.Current.Response, true, "Daily Inspector " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"));

                    return new DownloadPdfResult(rptH, "Daily Inspector " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"));
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    rptH.Close();
                    rptH.Dispose();
                }

            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("DailyInspectorReport", "Report");
            }

        }

        #endregion

        #region Origin AnalysisByRoute

        public ActionResult OriginAnalysisByRoute()
        {
            var service = new SalesAnalysisService();
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            var model = service.GetSalesAnalysisFilter(conKey);
            model.RouteTypes = service.GetAllRouteTypes(conKey);

            return View("OriginAnalysisByRoute", model);
        }

        public ActionResult OriginAnalysisByRouteReportDownload(SalesAnalysisFilter filters)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;

            var service = new SalesAnalysisService();

            var ds = service.GetOriginAnalysisByRouteDataSet(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                var key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
                if (key.Equals("atamelang70"))
                { return DownLoadReportByDataSet(filters.ExcelOrPDF, "~/CrystalReports/Rpt/SalesAnalysis/OriginAnalysisByRouteAtamelangTGX.rpt", ds, "OriginAnalysis ByRoute "); }
                else
                { return DownLoadReportByDataSet(filters.ExcelOrPDF, "~/CrystalReports/Rpt/SalesAnalysis/OriginAnalysisByRoute.rpt", ds, "OriginAnalysis ByRoute "); }

            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("OriginAnalysisByRoute", "Report");
            }

        }

        #endregion

        #region Sales AnalysisByRoute

        public ActionResult SalesAnalysisByRoute()
        {
            var service = new SalesAnalysisService();
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            var model = service.GetSalesAnalysisFilter(conKey);

            return View("SalesAnalysisByRoute", model);
        }

        public ActionResult SalesAnalysisByRouteReportDownload(SalesAnalysisFilter filters)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;

            var service = new SalesAnalysisService();

            var ds = service.GetSalesAnalysisByRouteDataSet(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filters.ExcelOrPDF, "~/CrystalReports/Rpt/SalesAnalysis/SalesAnalysisByRoute.rpt", ds, "SalesAnalysis ByRoute ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("SalesAnalysisByRoute", "Report");
            }

        }

        #endregion

        #region ClassSummaryByClassType

        public ActionResult ClassSummaryByClassType()
        {
            var service = new SalesAnalysisService();
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            var model = service.GetSalesAnalysisFilter(conKey);

            return View("ClassSummaryByClassType", model);
        }

        public ActionResult ClassSummaryByClassTypeGraph()
        {
            var service = new SalesAnalysisService();
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            var model = service.GetSalesAnalysisFilterForGraph(conKey);

            return View("ClassSummaryByClassTypeGraph", model);
        }

        public JsonResult GetClassSummaryByClassTypeGraphData(string classGroupId)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;
            var filters = new SalesAnalysisFilter();

            if (!string.IsNullOrEmpty(classGroupId) && classGroupId.ToLower() != "all")
            {
                filters.ClassGroupsSelected = new string[] { classGroupId };
            }

            var service = new SalesAnalysisService();

            var list = service.GetClassSummaryByClassTypeGraphData(conKey, filters);

            var jsonData = new
            {
                Data = list,
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ClassSummaryByClassTypeDownload(SalesAnalysisFilter filters)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;

            var service = new SalesAnalysisService();

            var ds = service.GetClassSummaryDataSet(conKey, filters, comp, "EbusClassSummaryByClassType", true);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filters.ExcelOrPDF, "~/CrystalReports/Rpt/ClassSummary/ClassSummaryByClassType.rpt", ds, "ClassSummary By ClassType ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("ClassSummaryByClassType", "Report");
            }

        }

        #endregion

        #region Audit status

        public ActionResult AuditStatus()
        {
            var service = new AuditStatusService();
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            var model = service.GetAuditStatusForGraph(conKey);

            return View("AuditStatus", model);
        }

        #endregion

        #region Sales Analysis by Class

        public ActionResult SalesAnalysisByClass()
        {
            var service = new SalesAnalysisService();
            var conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            var model = service.GetSalesAnalysisFilter(conKey);

            return View("SalesAnalysisByClass", model);
        }

        public ActionResult SalesAnalysisByClassDownload(SalesAnalysisFilter filters)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;

            var service = new SalesAnalysisService();

            var ds = service.GetClassSummaryDataSet(conKey, filters, comp, "EbusSalesAnalysisByClass", false);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filters.ExcelOrPDF, "~/CrystalReports/Rpt/ClassSummary/SalesAnalysisByClass.rpt", ds, "Sales Analysis By Class ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("SalesAnalysisByClass", "Report");
            }

        }

        #endregion

        #region Cahier Report

        public ActionResult CashierReport()
        {
            var model = new CashierFilter();
            var service = new InspectorReportService();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            return View("CashierReport", model);
        }

        public ActionResult CashierReportDownload(CashierFilter filters)
        {
            var userset = GetUserSettings();
            var conKey = userset.ConnectionKey;
            var comp = userset.CompanyName;

            var service = new CashierServices();

            var ds = service.GetCashierDataSet(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filters.ExcelOrPDF, "~/CrystalReports/Rpt/Cashier/CashierReport.rpt", ds, "CashierReport ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("CashierReport", "Report");
            }

        }

        #endregion

        #region Helpers

        private ActionResult DownLoadReportByDataSet(bool excelOrPDF, string rptPath, DataSet ds, string reportName)
        {
            var fileType = excelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

            var rptH = new ReportClass
            {
                FileName = Server.MapPath(rptPath)
            };

            try
            {
                rptH.Load();

                rptH.SetDataSource(ds.Tables[0]);

                rptH.ExportToHttpResponse(fileType, System.Web.HttpContext.Current.Response, true, reportName + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"));

                return new DownloadPdfResult(rptH, reportName + " " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"));
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                rptH.Close();
                rptH.Dispose();
            }

        }

        private UserSettings GetUserSettings()
        {
            var res = new UserSettings();
            res.ConnectionKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            res.CompanyName = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;
            res.Username = HttpContext.User.Identity.Name;
            return res;
        }

        #endregion
    }
}
