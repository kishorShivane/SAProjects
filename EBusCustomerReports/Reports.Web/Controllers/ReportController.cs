using CrystalDecisions.CrystalReports.Engine;
using Helpers;
using Helpers.Security;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Reports.Services;
using Reports.Services.Helpers;
using Reports.Services.Models;
using Reports.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;

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
        public const string sp8 = "EbusRevenueByDuty"; // Revenue By Duty
        public const string sp9 = "EbusJourneyAnalysisSummaryBySubRoute"; // Revenue By Duty
        public const string sp10 = "EbusFormEReference"; // Form-E Reference

        //Home
        public ActionResult Index()
        {
            UserSettings userset = GetUserSettings();
            TempData["userExpiredWarning"] = false;
            if (userset.WarningDate.HasValue && DateTime.Compare(userset.WarningDate.Value, DateTime.Now) <= 0)
            {
                TempData["userExpiredWarning"] = true;
                TempData["LastDate"] = userset.LastDate.Value.ToShortDateString();
                TempData["UserName"] = userset.Username;
            }
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            HomeViewModel model = new ReportsService().GetHomeScreenData(conKey);
            return View(model);
        }

        #region MonthRevenueGraph

        public ActionResult MonthRevenueGraph()
        {
            MonthyRevenueFilter model = new MonthyRevenueFilter();
            return View(model);
        }

        public JsonResult GetMonthRevenueGraphData(string years, string months)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            SalesAnalysisService service = new SalesAnalysisService();

            MonthyRevenueDataGraph list = service.GetMonthRevenueData(conKey, months, years);

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
            CashierReportSummaryFilter model = new CashierReportSummaryFilter();
            InspectorReportService service = new InspectorReportService();
            List<OperatorDetails> staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.Cashiers = staff.Where(s => s.OperatorType.ToLower() == "cashier".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.StaffList = staff.Where(s => s.OperatorType.ToLower() == "driver".ToLower().Trim() || s.OperatorType.ToLower() == "seller".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Terminals = service.GetAllTerminals(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("DailyAuditByCashierTerminal", model);
        }

        public ActionResult DailyAuditByCashierTerminalSummary()
        {
            CashierReportSummaryFilter model = new CashierReportSummaryFilter();
            InspectorReportService service = new InspectorReportService();
            List<OperatorDetails> staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.Cashiers = staff.Where(s => s.OperatorType.ToLower() == "cashier".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.StaffList = staff.Where(s => s.OperatorType.ToLower() == "driver".ToLower().Trim() || s.OperatorType.ToLower() == "seller".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Terminals = service.GetAllTerminals(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("DailyAuditByCashierTerminalSummary", model);
        }

        public ActionResult DownloadDailyAuditByCashierTerminalReport(CashierReportSummaryFilter filter)
        {
            UserSettings userset = GetUserSettings();
            DataSet ds = new SmartCardService().GetDailyAuditByCashierTerminalDataset(userset.ConnectionKey, filter, userset.CompanyName);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
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
            UserSettings userset = GetUserSettings();
            DataSet ds = new SmartCardService().GetDailyAuditByCashierTerminalDatasetSummary(userset.ConnectionKey, filter, userset.CompanyName);
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
            CashierReportSummaryFilter model = new CashierReportSummaryFilter();
            InspectorReportService service = new InspectorReportService();
            List<OperatorDetails> staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.Cashiers = staff.Where(s => s.OperatorType.ToLower() == "Cashier".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Terminals = service.GetAllTerminals(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("CashierSummaryReport", model);
        }

        public ActionResult DownloadCashierSummaryReport(CashierReportSummaryFilter filter)
        {
            UserSettings userset = GetUserSettings();
            DataSet ds = new SmartCardService().GetCashierSummaryReportDataset(userset.ConnectionKey, userset.CompanyName, filter);
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

        public ActionResult CashierReportMatatiele()
        {
            CashierReportSummaryFilter model = new CashierReportSummaryFilter();
            InspectorReportService service = new InspectorReportService();
            List<OperatorDetails> staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.Cashiers = staff.Where(s => s.OperatorType.ToLower() == "Cashier".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Terminals = service.GetAllTerminals(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("CashierReportMatatiele", model);
        }

        public ActionResult DownloadCashierReportMatatiele(CashierReportSummaryFilter filter)
        {
            UserSettings userset = GetUserSettings();
            DataSet ds = new SmartCardService().GetCashierSummaryReportDataset(userset.ConnectionKey, userset.CompanyName, filter);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadCashierReportMatatieleByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/Cashier/CashierReportMatatiele.rpt", ds, "CashierReport ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("CashierReportMatatiele", "Report");
            }
        }


        public ActionResult NewCashierReportAt()
        {
            CashierReportSummaryFilter model = new CashierReportSummaryFilter();
            InspectorReportService service = new InspectorReportService();
            List<OperatorDetails> staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.Cashiers = staff.Where(s => s.OperatorType.ToLower() == "Cashier".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Terminals = service.GetAllTerminals(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("NewCashierSummaryReport", model);
        }

        public ActionResult DownloadNewCashierSummaryReport(CashierReportSummaryFilter filter)
        {
            UserSettings userset = GetUserSettings();
            DataSet ds = new SmartCardService().GetCashierSummaryReportDataset(userset.ConnectionKey, userset.CompanyName, filter);
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
            CashierReportSummaryFilter model = new CashierReportSummaryFilter();
            InspectorReportService service = new InspectorReportService();
            List<OperatorDetails> staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.StaffList = staff.Where(s => s.OperatorType.ToLower() == "driver".ToLower().Trim() || s.OperatorType.ToLower() == "seller".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            return View("CashierReconciliationReport", model);
        }

        public ActionResult DownloadCashierReconciliationReport(CashierReportSummaryFilter filter)
        {
            UserSettings userset = GetUserSettings();
            DataSet ds = new SmartCardService().GetCashierReconciliationReportDataset(userset.ConnectionKey, userset.CompanyName, filter);
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
            EarlyLateRunningModel model = new ReportsService().GetEarlyLateRunningModel(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View(model);
        }

        public ActionResult DownloadEarlyLateRunning(EarlyLateRunningModel filter)
        {
            UserSettings userset = GetUserSettings();
            DataSet ds = new ReportsService().GetEarlyLateRunningReport(userset.ConnectionKey, filter, Convert.ToInt32(filter.TimeSelected), userset.CompanyName);
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
            CashVsSmartCardUsageByRouteFilter model = new CashVsSmartCardUsageByRouteFilter();
            UserSettings userset = GetUserSettings();
            model.Routes = new SmartCardService().GetAllDriverRoutes(userset.ConnectionKey);
            return View(model);
        }

        [HttpGet]
        public PartialViewResult GetCashVsSmartCardUsageByRouteData(string fromdate, string todate, string routes)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            SmartCardService service = new SmartCardService();

            if (string.IsNullOrEmpty(fromdate))
            {
                fromdate = DateTime.Now.AddDays(-30).ToString("dd-MM-yyyy");
            }

            if (string.IsNullOrEmpty(todate))
            {
                todate = DateTime.Now.AddDays(0).ToString("dd-MM-yyyy");
            }

            List<CashVsSmartCardUsageByRouteData> list = service.GetCashVsSmartCardUsageByRouteData(conKey, routes, fromdate, todate);

            return PartialView("CashVsSmartCardTable", list);
        }


        public ActionResult DownloadCashVsSmartCardUsageByRoute(string fromdate, string todate, string routes)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            SmartCardService service = new SmartCardService();

            if (string.IsNullOrEmpty(todate))
            {
                fromdate = DateTime.Now.AddDays(-7).ToString("dd-MM-yyyy");
            }

            if (string.IsNullOrEmpty(todate))
            {
                todate = DateTime.Now.AddDays(0).ToString("dd-MM-yyyy");
            }

            DataSet ds = service.GetCashVsSmartCardUsageByRouteDataSet(conKey, routes, fromdate, todate, comp);

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
            YearlyBreakDownFilter model = new YearlyBreakDownFilter
            {
                Classes = new SalesAnalysisService().GetAllCalsses(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey),
                RoutesList = new SalesAnalysisService().GetAllRoutes(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey)
            };
            return View(model);
        }

        public ActionResult YearlyBreakDownByRouteDownload(YearlyBreakDownFilter filter)
        {
            UserSettings userset = GetUserSettings();

            DataSet ds = new SalesAnalysisService().GetYearlyBreakDownByRouteDataSet(userset.ConnectionKey, userset.CompanyName, filter);
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
            YearlyBreakDownFilter model = new YearlyBreakDownFilter
            {
                Classes = new SalesAnalysisService().GetAllCalsses(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey)
            };
            return View(model);
        }

        public ActionResult YearlyBreakDownDownload(YearlyBreakDownFilter filter)
        {
            UserSettings userset = GetUserSettings();

            DataSet ds = new SalesAnalysisService().GetYearlyBreakDownDataSet(userset.ConnectionKey, userset.CompanyName, filter);
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
            SmartCardUsageFilter model = new SmartCardUsageFilter();

            return View(model);
        }

        public ActionResult SmartCardUsageDownload(SmartCardUsageFilter filter)
        {

            UserSettings userset = GetUserSettings();

            CashierServices service = new CashierServices();

            DataSet ds = new SmartCardService().GetSmartCardUsageData(userset.ConnectionKey, userset.CompanyName, filter.NumberOfTimesUsed.Value, filter.StartDate, filter.EndDate);
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
            DutySheetsViewModel model = new DutySheetsViewModel();
            UserSettings settings = GetUserSettings();
            List<SelectListItem> allDuties = new ReportsService().GetAllDuties(settings.ConnectionKey);
            List<string> allcons = new ReportsService().GetAllContacts(settings.ConnectionKey);

            model.Duties = allDuties;

            model.Contracts = (from f in allcons
                               select new SelectListItem { Selected = false, Text = f, Value = f }).ToList();

            return View(model);
        }

        public ActionResult TimeTableDownload(DutySheetsViewModel filter)
        {

            UserSettings userset = GetUserSettings();
            string duties = filter.DutiesSelected == null ? "" : string.Join(",", filter.DutiesSelected);
            string consSel = filter.ContractsSelected == null ? "" : string.Join(",", filter.ContractsSelected);

            CashierServices service = new CashierServices();

            DataSet ds = new DutySheetsService().GetTimeTableDetails(userset.ConnectionKey, userset.CompanyName, filter.ShowAllOperatingDays, filter.DutyDate, duties, consSel);
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
            DutySheetsViewModel model = new DutySheetsViewModel();
            UserSettings settings = GetUserSettings();
            List<SelectListItem> allDuties = new ReportsService().GetAllDuties(settings.ConnectionKey);
            model.Duties = allDuties;

            return View(model);
        }

        public ActionResult DutySheetsDownload(DutySheetsViewModel filter)
        {

            UserSettings userset = GetUserSettings();
            string duties = filter.DutiesSelected == null ? "" : string.Join(",", filter.DutiesSelected);

            CashierServices service = new CashierServices();

            DataSet ds = new DutySheetsService().GetDutySheetDetails(userset.ConnectionKey, userset.CompanyName, filter.ShowAllOperatingDays, filter.DutyDate, duties);
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
            SmartCardHotList model = new SmartCardHotList
            {
                Reasons = new SmartCardService().GetAllHotlistReasons(GetUserSettings().ConnectionKey)
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult SubmitCardForHotlist(SmartCardHotList model)
        {
            UserSettings settings = GetUserSettings();
            string message = "Request for Smart Card Hotlisting sent Sucessfully !";
            model.CreatedBy = settings.Username;
            model.CreatedDate = DateTime.Now;
            SmartCardHotList result = new SmartCardService().SaveSmartCardHolistingRequest(settings.ConnectionKey, model);

            if (result.IsDuplicate == true)
            {
                message = string.Format("Holisting request for this number is already sent by {0} on {1}", result.CreatedBy, result.CreatedDate.ToString("MM-dd-yyyy"));
            }
            else
            {
                List<SelectListItem> reasons = new SmartCardService().GetAllHotlistReasons(GetUserSettings().ConnectionKey);
                string reason = reasons.Where(x => x.Value.Equals(model.ReasonSelected)).Select(x => x.Text).FirstOrDefault().ToString();
                bool res = MailHelper.SendMailToEbus(model.SmartCardNubmer, reason, model.Comments, settings.Username, settings.CompanyName);
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
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            DriverDetailsFilter model = new InspectorReportService().GetDriverDetailsReportFilter(conKey);
            return View(model);
        }

        public ActionResult GetDriverTransactionDetailsDownload(DriverDetailsFilter filter)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            CashierServices service = new CashierServices();

            DataSet ds = new InspectorReportService().GetDriverTransactionDetails(conKey, CustomDateTime.ConvertStringToDateSaFormat(filter.StartDate), Convert.ToInt32(filter.DriversSelected), comp);
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
            SchVsOprViewModel model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("RevenueByDuty", model);
        }
        public ActionResult RevenueByDutyDownload(SchVsOprViewModel filters)
        {
            string key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            return DownloadRevenueByDutyReport(filters, Server.MapPath("~/CrystalReports/Rpt/RevenueByDuty.rpt"), "RevenueByDuty " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp8);
        }

        public ActionResult RevenueByDutyAll()
        {
            SchVsOprViewModel model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("RevenueByDutyAll", model);
        }
        public ActionResult RevenueByDutyAllDownload(SchVsOprViewModel filters)
        {
            string key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            return DownloadRevenueByDutyAllReport(filters, Server.MapPath("~/CrystalReports/Rpt/RevenueByDutyAll.rpt"), "RevenueByDuty " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp8);
        }

        //1
        public ActionResult ScheduledVsOperatedReport()
        {
            SchVsOprViewModel model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("ScheduledVsOperated", model);
        }
        public ActionResult ScheduledVsOperatedDownload(SchVsOprViewModel filters)
        {
            string key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            if (key.Equals("atamelang70"))
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/ScheduledVsOperatedOkAtamelangTGX.rpt"), "ScheduledVsOperated " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp2); }
            else
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/ScheduledVsOperatedOk.rpt"), "ScheduledVsOperated " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp2); }

        }

        //2
        public ActionResult NotScheduledButOperatedReport()
        {
            SchVsOprViewModel model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("NotScheduledButOperated", model);
        }
        public ActionResult NotScheduledButOperatedDownload(SchVsOprViewModel filters)
        {
            string key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            if (key.Equals("atamelang70"))
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/NotScheduledButOperatedAtamelangTGX.rpt"), "Operated Not Scheduled " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp4); }
            else
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/NotScheduledButOperated.rpt"), "Operated Not Scheduled " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp4); }
        }

        //3
        public ActionResult ScheduledNotOperatedReport()
        {
            SchVsOprViewModel model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("ScheduledNotOperated", model);
        }
        public ActionResult ScheduledNotOperatedDownload(SchVsOprViewModel filters)
        {
            return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/ScheduledNotOperated.rpt"), "ScheduledNotOperated " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp5);
        }

        public ActionResult DownloadReportGeneric(SchVsOprViewModel filters, string rptPath, string fileName, string spName, bool isFormE = false)
        {
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            string comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            ReportsService service = new ReportsService();

            DataSet ds = isFormE ? service.GetFormEReport(conKey, filters, spName, comp, isFormE) : service.GetScheduledVsOperatedReport(conKey, filters, spName, comp, isFormE);
            if (ds.Tables[0].Rows.Count > 0)
            {
                CrystalDecisions.Shared.ExportFormatType fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                if (fileName.Contains("FormE"))
                {
                    if (fileType == CrystalDecisions.Shared.ExportFormatType.Excel)
                    {
                        return GenerateFormESummaryExcelReport(ds.Tables[0]);
                    }
                }

                ReportClass rptH = new ReportClass
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
                {
                    return RedirectToAction("NotScheduledButOperatedReport", "Report");
                }

                if (fileName.Contains("ScheduledNotOperated"))
                {
                    return RedirectToAction("ScheduledNotOperatedReport", "Report");
                }

                if (fileName.Contains("ScheduledVsOperated"))
                {
                    return RedirectToAction("ScheduledVsOperatedReport", "Report");
                }

                if (fileName.Contains("FormE"))
                {
                    return RedirectToAction("FormEReport", "Report");
                }
            }
            return RedirectToAction("Index", "Report");
        }


        private ActionResult GenerateFormESummaryExcelReport(DataTable dt)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet Sheet = excelPackage.Workbook.Worksheets.Add("FormESummary");

                #region Declarations
                int scheduledTrip = 0;
                int operatedTrips = 0;
                int notOperatedTrips = 0;
                double scheduledKilometers = 0;
                double operatedKilometers = 0;
                int cashTickets = 0;
                int passes = 0;
                int transfers = 0;
                double cashRevenue = 0;
                double nonRevenue = 0;
                double totalRevenue = 0;
                int totalPassengers = 0;
                double averagePassenger = 0;
                double averageRevenue = 0;

                int scheduledTripTotal = 0;
                int operatedTripsTotal = 0;
                int notOperatedTripsTotal = 0;
                double scheduledKilometersTotal = 0;
                double operatedKilometersTotal = 0;
                int cashTicketsTotal = 0;
                int passesTotal = 0;
                int transfersTotal = 0;
                double cashRevenueTotal = 0;
                double nonRevenueTotal = 0;
                double totalRevenueTotal = 0;
                int totalPassengersTotal = 0;
                double averagePassengerTotal = 0;
                double averageRevenueTotal = 0;


                int colCount = 17;
                int perContractRowCount = 0;
                DataRow dr = dt.Rows[0];
                #endregion

                #region Header Section
                //Add Header section

                string filterDateRange = dr["DateRangeFilter"].ToString();
                string filterContractsRange = dr["ContractsFilter"].ToString();
                string companyName = dr["companyName"].ToString();

                Sheet.Cells[1, 1, 1, colCount].Merge = true;
                Sheet.Cells[1, 1, 1, colCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Sheet.Cells[1, 1, 1, colCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                Sheet.Cells[1, 1, 1, colCount].Style.Font.Size = 16;
                Sheet.Cells[1, 1, 1, colCount].Style.Font.Bold = true;
                Sheet.Cells[1, 1, 1, colCount].Value = "Form E Report";

                Sheet.Cells[2, 1, 2, colCount].Merge = true;
                Sheet.Cells[2, 1, 2, colCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Sheet.Cells[2, 1, 2, colCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                Sheet.Cells[2, 1, 2, colCount].Style.Font.Size = 20;
                Sheet.Cells[2, 1, 2, colCount].Style.Font.Bold = true;
                Sheet.Cells[2, 1, 2, colCount].Value = companyName;


                Sheet.Cells[3, 1, 3, colCount].Merge = true;
                Sheet.Cells[3, 1, 3, colCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Sheet.Cells[3, 1, 3, colCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                Sheet.Cells[4, 1, 4, colCount].Merge = true;
                Sheet.Cells[4, 1, 4, colCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Sheet.Cells[4, 1, 4, colCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                Sheet.Cells[4, 1, 4, colCount].Style.Font.Size = 12;
                Sheet.Cells[4, 1, 4, colCount].Value = filterDateRange;

                Sheet.Cells[5, 1, 5, colCount].Merge = true;
                Sheet.Cells[5, 1, 5, colCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Sheet.Cells[5, 1, 5, colCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                Sheet.Cells[5, 1, 5, colCount].Style.Font.Size = 12;
                Sheet.Cells[5, 1, 5, colCount].Value = filterContractsRange;

                Sheet.Cells[6, 1, 6, colCount].Merge = true;
                Sheet.Cells[6, 1, 6, colCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Sheet.Cells[6, 1, 6, colCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                #endregion

                #region Header Row
                Sheet.Cells["A7"].Value = "DOTRoute";
                Sheet.Cells["B7"].Value = "From";
                Sheet.Cells["C7"].Value = "To";
                Sheet.Cells["D7"].Value = "Scheduled Trips";
                Sheet.Cells["E7"].Value = "Operated  Trips";
                Sheet.Cells["F7"].Value = "Not Operated Trips";
                Sheet.Cells["G7"].Value = "Schedule  kilometres";
                Sheet.Cells["H7"].Value = "Operated Kilometres";
                Sheet.Cells["I7"].Value = "Cash Tickets";
                Sheet.Cells["J7"].Value = "Passes";
                Sheet.Cells["K7"].Value = "Transfers";
                Sheet.Cells["L7"].Value = "Total Pasengers";
                Sheet.Cells["M7"].Value = "Cash Revenue";
                Sheet.Cells["N7"].Value = "Non Revenue";
                Sheet.Cells["O7"].Value = "Total Revenue";
                Sheet.Cells["P7"].Value = "Avg Passenger/Trip";
                Sheet.Cells["Q7"].Value = "Avg Revenue/Trip";
                Sheet.Cells[$"A7:Q7"].Style.Font.Bold = true;
                Sheet.Cells[$"A7:Q7"].Style.Font.Size = 12;
                Sheet.Cells[$"A7:Q7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Sheet.Cells[$"A7:Q7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                #endregion




                //Sort datatable based on Contract

                dt.DefaultView.Sort = "Contract asc";
                dt = dt.DefaultView.ToTable();
                int currColumn = 8;
                string previousContract = string.Empty;
                foreach (DataRow row in dt.Rows)
                {
                    var currentContract = row["Contract"].ToString();
                    if (previousContract != currentContract)
                    {
                        if (!string.IsNullOrEmpty(previousContract))
                        {
                            // Add totals per contract
                            Sheet.Cells[$"A{currColumn}:C{currColumn}"].Merge = true;
                            Sheet.Cells[$"A{currColumn}:C{currColumn}"].Value = "Total: ";
                            Sheet.Cells[$"A{currColumn}:C{currColumn}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            Sheet.Cells[$"A{currColumn}:C{currColumn}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            Sheet.Cells[$"D{currColumn}"].Value = scheduledTrip;
                            Sheet.Cells[$"E{currColumn}"].Value = operatedTrips;
                            Sheet.Cells[$"F{currColumn}"].Value = notOperatedTrips;
                            Sheet.Cells[$"G{currColumn}"].Value = scheduledKilometers.ToString(".00");
                            Sheet.Cells[$"H{currColumn}"].Value = operatedKilometers;
                            Sheet.Cells[$"I{currColumn}"].Value = cashTickets;
                            Sheet.Cells[$"J{currColumn}"].Value = passes;
                            Sheet.Cells[$"K{currColumn}"].Value = transfers;
                            Sheet.Cells[$"L{currColumn}"].Value = totalPassengers;
                            Sheet.Cells[$"M{currColumn}"].Value = "R " + cashRevenue;
                            Sheet.Cells[$"N{currColumn}"].Value = "R " + nonRevenue;
                            Sheet.Cells[$"O{currColumn}"].Value = "R " + totalRevenue;
                            Sheet.Cells[$"P{currColumn}"].Value = (averagePassenger / perContractRowCount).ToString(".00");
                            Sheet.Cells[$"Q{currColumn}"].Value = "R " + (averageRevenue / perContractRowCount).ToString(".00");
                            Sheet.Cells[$"A{currColumn}:Q{currColumn}"].Style.Font.Bold = true;
                            Sheet.Cells[$"A{currColumn}:Q{currColumn}"].Style.Font.Size = 12;
                            Sheet.Cells[$"D{currColumn}:Q{currColumn}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            Sheet.Cells[$"D{currColumn}:Q{currColumn}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                            scheduledTripTotal += scheduledTrip; operatedTripsTotal += operatedTrips; notOperatedTripsTotal += notOperatedTrips;
                            scheduledKilometersTotal += scheduledKilometers; operatedKilometersTotal += operatedKilometers; cashTicketsTotal += cashTickets;
                            passesTotal += passes; transfersTotal += transfers; cashRevenueTotal += cashRevenue; nonRevenueTotal += nonRevenue;
                            totalRevenueTotal += totalRevenue; totalPassengersTotal += totalPassengers; averagePassengerTotal += averagePassenger;
                            averageRevenueTotal += averageRevenue;

                            scheduledTrip = 0; operatedTrips = 0; notOperatedTrips = 0; scheduledKilometers = 0; operatedKilometers = 0; cashTickets = 0; passes = 0; transfers = 0;
                            cashRevenue = 0; nonRevenue = 0; totalRevenue = 0; totalPassengers = 0; averagePassenger = 0; averageRevenue = 0;

                            perContractRowCount = 0; perContractRowCount++;
                            currColumn++;
                        }

                        //Add empty row
                        Sheet.Cells[currColumn, 1, currColumn, colCount].Merge = true;
                        Sheet.Cells[currColumn, 1, currColumn, colCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        Sheet.Cells[currColumn, 1, currColumn, colCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        currColumn++;

                        //Add contract details
                        Sheet.Cells[currColumn, 1, currColumn, colCount].Merge = true;
                        Sheet.Cells[currColumn, 1, currColumn, colCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        Sheet.Cells[currColumn, 1, currColumn, colCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        Sheet.Cells[currColumn, 1, currColumn, colCount].Style.Font.Size = 12;
                        Sheet.Cells[currColumn, 1, currColumn, colCount].Style.Font.Bold = true;
                        Sheet.Cells[currColumn, 1, currColumn, colCount].Value = $"Contract: {currentContract}";

                        currColumn++;
                    }

                    perContractRowCount++;

                    Sheet.Cells[$"A{currColumn}"].Value = row["DOTRoute"];
                    Sheet.Cells[$"B{currColumn}"].Value = row["From"];
                    Sheet.Cells[$"C{currColumn}"].Value = row["To"];
                    Sheet.Cells[$"D{currColumn}"].Value = Convert.ToDouble(row["ScheduledTrips"]).ToString(".00");
                    Sheet.Cells[$"E{currColumn}"].Value = row["OperatedTrips"];
                    Sheet.Cells[$"F{currColumn}"].Value = row["NotOperatedTrips"];
                    Sheet.Cells[$"G{currColumn}"].Value = row["Schedulekilometres"];
                    Sheet.Cells[$"H{currColumn}"].Value = row["OperatedKilometres"];
                    Sheet.Cells[$"I{currColumn}"].Value = row["Tickets"];
                    Sheet.Cells[$"J{currColumn}"].Value = row["Passes"];
                    Sheet.Cells[$"K{currColumn}"].Value = row["Transfers"];
                    Sheet.Cells[$"L{currColumn}"].Value = row["TotalPassengers"];
                    Sheet.Cells[$"M{currColumn}"].Value = "R " + row["Revenue"];
                    Sheet.Cells[$"N{currColumn}"].Value = "R " + row["NonRevenue"];
                    Sheet.Cells[$"O{currColumn}"].Value = "R " + row["TotalRevenue"];
                    Sheet.Cells[$"P{currColumn}"].Value = row["AvgPassengerPerTrip"];
                    Sheet.Cells[$"Q{currColumn}"].Value = "R " + Convert.ToDouble(row["AvgRevenuePerTrip"]).ToString(".00"); 

                    scheduledTrip += Convert.ToInt32(row["ScheduledTrips"]);
                    operatedTrips += Convert.ToInt32(row["OperatedTrips"]);
                    notOperatedTrips += Convert.ToInt32(row["NotOperatedTrips"]);
                    scheduledKilometers += Convert.ToDouble(row["Schedulekilometres"]);
                    operatedKilometers += Convert.ToDouble(row["OperatedKilometres"]);
                    cashTickets += Convert.ToInt32(row["Tickets"]);
                    passes += Convert.ToInt32(row["Passes"]);
                    transfers += Convert.ToInt32(row["Transfers"]);
                    totalPassengers += Convert.ToInt32(row["TotalPassengers"]);
                    cashRevenue += Convert.ToDouble(row["Revenue"]);
                    nonRevenue += Convert.ToDouble(row["NonRevenue"]);
                    totalRevenue += Convert.ToDouble(row["TotalRevenue"]);
                    averagePassenger += Convert.ToDouble(row["AvgPassengerPerTrip"]);
                    averageRevenue += Convert.ToDouble(row["AvgRevenuePerTrip"]);

                    previousContract = currentContract;
                    currColumn++;
                }


                // Add totals per contract
                Sheet.Cells[$"A{currColumn}:C{currColumn}"].Merge = true;
                Sheet.Cells[$"A{currColumn}:C{currColumn}"].Value = "Total: ";
                Sheet.Cells[$"A{currColumn}:C{currColumn}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Sheet.Cells[$"A{currColumn}:C{currColumn}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                Sheet.Cells[$"D{currColumn}"].Value = scheduledTrip;
                Sheet.Cells[$"E{currColumn}"].Value = operatedTrips;
                Sheet.Cells[$"F{currColumn}"].Value = notOperatedTrips;
                Sheet.Cells[$"G{currColumn}"].Value = scheduledKilometers.ToString(".00");
                Sheet.Cells[$"H{currColumn}"].Value = operatedKilometers;
                Sheet.Cells[$"I{currColumn}"].Value = cashTickets;
                Sheet.Cells[$"J{currColumn}"].Value = passes;
                Sheet.Cells[$"K{currColumn}"].Value = transfers;
                Sheet.Cells[$"L{currColumn}"].Value = totalPassengers;
                Sheet.Cells[$"M{currColumn}"].Value = "R " + cashRevenue;
                Sheet.Cells[$"N{currColumn}"].Value = "R " + nonRevenue;
                Sheet.Cells[$"O{currColumn}"].Value = "R " + totalRevenue;
                Sheet.Cells[$"P{currColumn}"].Value = (averagePassenger / perContractRowCount).ToString(".00");
                Sheet.Cells[$"Q{currColumn}"].Value = "R " + (averageRevenue / perContractRowCount).ToString(".00");
                Sheet.Cells[$"A{currColumn}:Q{currColumn}"].Style.Font.Bold = true;
                Sheet.Cells[$"A{currColumn}:Q{currColumn}"].Style.Font.Size = 12;
                Sheet.Cells[$"D{currColumn}:Q{currColumn}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Sheet.Cells[$"D{currColumn}:Q{currColumn}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                scheduledTripTotal += scheduledTrip; operatedTripsTotal += operatedTrips; notOperatedTripsTotal += notOperatedTrips;
                scheduledKilometersTotal += scheduledKilometers; operatedKilometersTotal += operatedKilometers; cashTicketsTotal += cashTickets;
                passesTotal += passes; transfersTotal += transfers; cashRevenueTotal += cashRevenue; nonRevenueTotal += nonRevenue;
                totalRevenueTotal += totalRevenue; totalPassengersTotal += totalPassengers; averagePassengerTotal += averagePassenger;
                averageRevenueTotal += averageRevenue;

                currColumn++;

                // Add grand totals 
                Sheet.Cells[$"A{currColumn}:C{currColumn}"].Merge = true;
                Sheet.Cells[$"A{currColumn}:C{currColumn}"].Value = "Grand Total: ";
                Sheet.Cells[$"A{currColumn}:C{currColumn}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Sheet.Cells[$"A{currColumn}:C{currColumn}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                Sheet.Cells[$"D{currColumn}"].Value = scheduledTripTotal;
                Sheet.Cells[$"E{currColumn}"].Value = operatedTripsTotal;
                Sheet.Cells[$"F{currColumn}"].Value = notOperatedTripsTotal;
                Sheet.Cells[$"G{currColumn}"].Value = scheduledKilometersTotal.ToString(".00");
                Sheet.Cells[$"H{currColumn}"].Value = operatedKilometersTotal;
                Sheet.Cells[$"I{currColumn}"].Value = cashTicketsTotal;
                Sheet.Cells[$"J{currColumn}"].Value = passesTotal;
                Sheet.Cells[$"K{currColumn}"].Value = transfersTotal;
                Sheet.Cells[$"L{currColumn}"].Value = totalPassengersTotal;
                Sheet.Cells[$"M{currColumn}"].Value = "R " + cashRevenueTotal;
                Sheet.Cells[$"N{currColumn}"].Value = "R " + nonRevenueTotal;
                Sheet.Cells[$"O{currColumn}"].Value = "R " + totalRevenueTotal;
                Sheet.Cells[$"A{currColumn}:Q{currColumn}"].Style.Font.Bold = true;
                Sheet.Cells[$"A{currColumn}:Q{currColumn}"].Style.Font.Size = 13;
                Sheet.Cells[$"D{currColumn}:Q{currColumn}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Sheet.Cells[$"D{currColumn}:Q{currColumn}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                Sheet.Cells["A:AZ"].AutoFitColumns();

                Sheet.Cells[$"A1:Q{currColumn}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                Sheet.Cells[$"A1:Q{currColumn}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                Sheet.Cells[$"A1:Q{currColumn}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                Sheet.Cells[$"A1:Q{currColumn}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                var fileStream = new MemoryStream();
                excelPackage.SaveAs(fileStream);
                fileStream.Position = 0;

                return new FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = $"FormE {DateTime.Now.ToString("dd-MM-yyyy H:mm:ss")}.xlsx" };
            }
        }

        public ActionResult DownloadFormEReference(SchVsOprViewModel filters, string rptPath, string fileName, string spName)
        {
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            string comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            ReportsService service = new ReportsService();

            DataSet ds = service.GetFormEReferenceReport(conKey, filters, spName, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                CrystalDecisions.Shared.ExportFormatType fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                ReportClass rptH = new ReportClass
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
                if (fileName.Contains("FormEReference"))
                {
                    return RedirectToAction("FormEReferenceReport", "Report");
                }
            }
            return RedirectToAction("Index", "Report");
        }


        public ActionResult DownloadFullDuty(SchVsOprViewModel filters, string rptPath, string fileName, string spName, bool isFormE = false)
        {
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            string comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            ReportsService service = new ReportsService();

            DataSet ds = service.FullDutySummary(conKey, filters, spName, comp, isFormE);//full duty report
            if (ds.Tables[0].Rows.Count > 0)
            {
                CrystalDecisions.Shared.ExportFormatType fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                ReportClass rptH = new ReportClass
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
            string imagePath = Server.MapPath("~/Images/logo_comp1.png");
            string logoContent = "data:image/" + Path.GetExtension(imagePath).TrimStart('.') + ";base64," + GetEmbedImageData(imagePath);
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
            htmlcontent.Append("<tr><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:50px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td></tr>");
            htmlcontent.Append("<tr><td colspan='10' align='center' style='width:750px;'><b> REVENUE BY DUTY</b></td><td rowspan='6' colspan='2' style='width:200px;' align='right'><img src='" + imagePath + "' alt='Loading Image' /></td></tr>");
            htmlcontent.Append("<tr><td colspan='10' align='center' style='width:750px;'><b>" + customer + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='10' style='width:750px;'><b>" + filterdate + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='10' style='width:750px;'><b>" + duties + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='10' style='width:750px;'></td></tr>");
            collection = collection.OrderByDescending(x => x.JourneyID).ThenBy(x => x.Total).ToList();
            //am getting my grid's column headers
            int columnscount = collection.Count;
            string prevDuty = ""; double prevTotal = 0;
            double revTotal = 0, nonRevTotal = 0;
            double adultRevTotal = 0, childRevTotal = 0, otherRevTotal = 0, adultNonRevTotal = 0, schNonRevTotal = 0, adultTrnsferTotal = 0, scholarTransferTotal = 0, storedValueTotal = 0;
            for (int i = 0; i < columnscount; i++)
            {
                RevenueByDuty curItem = collection[i];
                if (prevDuty != curItem.DutyID && prevTotal != curItem.Total)
                {
                    if (i != 0)
                    {
                        //last sub total row
                        htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-bottom: thin solid black;'><b>Sub Totals:</b></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultRevTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + childRevTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + otherRevTotal + "</u></td>");
                        htmlcontent.Append("<td align='right' style='width:100px;border-bottom: thin solid black;border-bottom: thin solid black;'><u>R " + revTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:50px;border-bottom: thin solid black;'></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultNonRevTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultTrnsferTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + schNonRevTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + scholarTransferTotal + "</u></td>");
                        htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + storedValueTotal + "</u></td>");
                        htmlcontent.Append("<td align='right' style='width:100px;border-right: thin solid black;border-bottom: thin solid black;'><u>R " + nonRevTotal + "</u></td></tr>");
                        revTotal = 0; nonRevTotal = 0; adultRevTotal = 0; childRevTotal = 0; adultNonRevTotal = 0; schNonRevTotal = 0; adultTrnsferTotal = 0; scholarTransferTotal = 0; otherRevTotal = 0; storedValueTotal = 0;
                    }
                    //New row started
                    htmlcontent.Append("<tr><td colspan='10' style='width:750px;'></td></tr>");
                    htmlcontent.Append("<tr><td colspan='2' style='width:200px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;'><b>Duty Number:</b>" + curItem.DutyID + "</td>");
                    htmlcontent.Append("<td align='left' style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:50px;border-top: thin solid black;border-bottom: thin solid black'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black' align='right' ><b>Total:</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'>R " + curItem.Total + "</td></tr>");

                    //Next to header
                    htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;' align='right'><b>Journey</b></td>");
                    htmlcontent.Append("<td colspan='4' align='center' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Revenue</b></td> ");
                    htmlcontent.Append("<td style='width:50px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'></td> ");
                    htmlcontent.Append("<td colspan='6' align='center' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Non Revenue</b></td></tr>");

                    //Row next to sub header 
                    htmlcontent.Append("<tr><td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Adult</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Child</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Other</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Cash</b></td>");
                    htmlcontent.Append("<td align='right' style='width:50px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Adult</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Adult Transfer</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Scholar</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Scholar Transfer</b></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-left: thin solid black;border-top: thin solid black;border-bottom: thin solid black;border-right: thin solid black;'><b>Stored Value</b></td>");
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
                otherRevTotal += curItem.OtherRevenue;
                storedValueTotal += curItem.StoredvalueTransfer;

                htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-bottom: thin solid black;'>" + curItem.JourneyID + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.AdultRevenue + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.ChildRevenue + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.OtherRevenue + "</td>");
                htmlcontent.Append("<td align='right' style='width:100px;border-bottom: thin solid black;'>R " + curItem.Cash + "</td>");
                htmlcontent.Append("<td style='width:50px;border-bottom: thin solid black;'></td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.AdultNonRevenue + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.AdultTransfer + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.SchlorNonRevenue + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.ScholarTransfer + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.StoredvalueTransfer + "</td>");
                htmlcontent.Append("<td align='right' style='width:100px;border-right: thin solid black;border-bottom: thin solid black;'>R " + curItem.Value + "</td></tr>");
                prevDuty = curItem.DutyID;
                prevTotal = curItem.Total;

                if (i == columnscount - 1)
                {
                    htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-bottom: thin solid black;'><b>Sub Totals:</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultRevTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + childRevTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + otherRevTotal + "</u></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-bottom: thin solid black;border-bottom: thin solid black;'><u>R " + revTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:50px;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultNonRevTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + adultTrnsferTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + schNonRevTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + scholarTransferTotal + "</u></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><u>" + storedValueTotal + "</u></td>");
                    htmlcontent.Append("<td align='right' style='width:100px;border-right: thin solid black;border-bottom: thin solid black;'><u>R " + nonRevTotal + "</u></td></tr>");
                    revTotal = 0; nonRevTotal = 0; adultRevTotal = 0; childRevTotal = 0; adultNonRevTotal = 0; schNonRevTotal = 0; adultTrnsferTotal = 0; scholarTransferTotal = 0; otherRevTotal = 0; storedValueTotal = 0;
                }
            }
            htmlcontent.Append("</Table>");
            htmlcontent.Append("</font>");

            System.Web.HttpContext.Current.Response.Write(htmlcontent.ToString());
            System.Web.HttpContext.Current.Response.Flush();
            System.Web.HttpContext.Current.Response.End();
        }

        public string GetEmbedImageData(string path)
        {
            using (Image img = Image.FromFile(path))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    img.Save(ms, img.RawFormat);
                    byte[] imgBytes = ms.ToArray();
                    return Convert.ToBase64String(imgBytes);
                }
            }
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
            htmlcontent.Append("<tr><td colspan='6' align='center' style='width:550px;'><b> REVENUE BY DUTY</b></td><td rowspan='6' colspan='2' style='width:200px;' align='right'><img src='~/Images/logo_comp1.png' /></td></tr>");
            htmlcontent.Append("<tr><td colspan='6' align='center' style='width:550px;'><b>" + customer + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='6' style='width:550px;'><b>" + filterdate + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='6' style='width:550px;'><b>" + duties + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='6' style='width:550px;'></td></tr>");
            collection.OrderBy(x => x.DutyID).ThenBy(x => x.Total);
            //am getting my grid's column headers
            int columnscount = collection.Count;
            string prevDuty = ""; double prevTotal = 0;
            double revTotal = 0, nonRevTotal = 0;
            double adultRevTotal = 0, childRevTotal = 0, adultNonRevTotal = 0, schNonRevTotal = 0;
            for (int i = 0; i < columnscount; i++)
            {
                RevenueByDuty curItem = collection[i];
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

        public void ExportCashierReportMatatieleToExcel(List<CashierReportMatatiele> collection, string fileName)
        {
            string cashiers = "", dateRange = "", locations = "", terminals = "", customer = "";
            if (collection.Any())
            {
                cashiers = collection.FirstOrDefault().Cashiers;
                dateRange = collection.FirstOrDefault().DateSelected;
                locations = collection.FirstOrDefault().Locations;
                terminals = collection.FirstOrDefault().Terminals;
                customer = collection.FirstOrDefault().CompanyName;
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
            htmlcontent.Append("<tr><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td></tr>");
            htmlcontent.Append("<tr><td colspan='7' align='center' style='width:700px;'><b>Cashier Report</b></td><td rowspan='6' colspan='2' style='width:200px;' align='right'><img src='~/Images/logo_comp1.png' /></td></tr>");
            htmlcontent.Append("<tr><td colspan='7' align='center' style='width:700px;'><b>" + customer + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='7' style='width:700px;'><b>" + cashiers + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='7' style='width:700px;'><b>" + dateRange + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='7' style='width:700px;'><b>" + locations + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='7' style='width:700px;'><b>" + terminals + "</b></td></tr>");
            htmlcontent.Append("<tr><td colspan='7' style='width:700px;'></td></tr>");
            collection.OrderBy(x => x.Date).ThenBy(x => x.Locations).ThenBy(x => x.Terminals).ThenBy(x => x.CashierID);
            //am getting my grid's column headers
            int columnscount = collection.Count;
            string prevDate = ""; string prevLocation = ""; string prevTerminal = ""; string prevCashier = "";
            double cashOnCard = 0, cashPaidIn = 0, difference = 0, netTickets = 0;
            double cashOnCardTotal = 0, cashPaidInTotal = 0, differenceTotal = 0, netTicketsTotal = 0;

            //Add headers
            htmlcontent.Append("<tr>");
            htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>Staff Number</td>");
            htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>Staff Name</td>");
            htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>CashPaid DateTime</td>");
            htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>Cash On Card</td>");
            htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>Cash Paid In</td>");
            htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>Difference</td>");
            htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>Reason</td>");
            htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>Net Tickets</td>");
            htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>Cash Paid Receipt No</td>");
            htmlcontent.Append("</tr>");
            for (int i = 0; i < columnscount; i++)
            {
                CashierReportMatatiele curItem = collection[i];
                if (i == 0)
                {
                    htmlcontent.Append("<tr><td colspan='9' style='width:900px;border-bottom: thin solid black;'><b>Date :</b>" + curItem.Date + "</td></tr>");
                    htmlcontent.Append("<tr><td colspan='9' style='width:900px;border-bottom: thin solid black;'><b>Locaion :</b>" + curItem.LocationDesc + "</td></tr>");
                    htmlcontent.Append("<tr><td colspan='9' style='width:900px;border-bottom: thin solid black;'><b>Terminal :</b>" + curItem.Terminal + "</td></tr>");
                    htmlcontent.Append("<tr><td colspan='9' style='width:900px;border-bottom: thin solid black;'><b>Cashier :</b>" + curItem.CashierName + " - " + curItem.CashierID + "</td></tr>");
                }
                if (i != 0 && (prevDate != curItem.Date || prevLocation != curItem.LocationDesc || prevTerminal != curItem.Terminal || prevCashier != curItem.CashierID))
                {
                    htmlcontent.Append("<tr>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>Cashier Total:</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + cashOnCard + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + cashPaidIn + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + difference + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + netTickets + "</b></td>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("</tr>");

                    htmlcontent.Append("<tr>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>Terminal Total:</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + cashOnCard + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + cashPaidIn + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + difference + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + netTickets + "</b></td>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("</tr>");

                    cashOnCardTotal += cashOnCard; cashOnCard = 0;
                    cashPaidInTotal += cashPaidIn; cashPaidIn = 0;
                    differenceTotal += difference; difference = 0;
                    netTicketsTotal += netTickets; netTickets = 0;

                    htmlcontent.Append("<tr><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td><td style='width:100px;'</td></tr>");
                    //New row started
                    htmlcontent.Append("<tr><td colspan='9' style='width:900px;border-bottom: thin solid black;'><b>Date :</b>" + curItem.Date + "</td></tr>");
                    htmlcontent.Append("<tr><td colspan='9' style='width:900px;border-bottom: thin solid black;'><b>Locaion :</b>" + curItem.LocationDesc + "</td></tr>");
                    htmlcontent.Append("<tr><td colspan='9' style='width:900px;border-bottom: thin solid black;'><b>Terminal :</b>" + curItem.Terminal + "</td></tr>");
                    htmlcontent.Append("<tr><td colspan='9' style='width:900px;border-bottom: thin solid black;'><b>Cashier :</b>" + curItem.CashierName + " - " + curItem.CashierID + "</td></tr>");
                }
                cashOnCard += curItem.DriverTotal;
                cashPaidIn += curItem.CashPaidIn;
                difference += curItem.Difference;
                netTickets += curItem.NetTickets;

                htmlcontent.Append("<tr><td style='width:100px;border-left: thin solid black;border-bottom: thin solid black;'>" + curItem.StaffNumber + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.StaffName + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.Date + " - " + curItem.Time + "</td>");
                htmlcontent.Append("<td align='right' style='width:100px;border-bottom: thin solid black;'>R " + curItem.DriverTotal + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>R" + curItem.CashPaidIn + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>R" + curItem.Difference + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.Reason + "</td>");
                htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'>" + curItem.NetTickets + "</td>");
                htmlcontent.Append("<td align='right' style='width:100px;border-right: thin solid black;border-bottom: thin solid black;'>R " + curItem.CashinReceiptNo + "</td></tr>");
                prevDate = curItem.Date;
                prevLocation = curItem.LocationDesc;
                prevTerminal = curItem.Terminal;
                prevCashier = curItem.CashierID;

                if (i == columnscount - 1)
                {
                    htmlcontent.Append("<tr>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>Cashier Total:</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + cashOnCard + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + cashPaidIn + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + difference + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + netTickets + "</b></td>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("</tr>");

                    htmlcontent.Append("<tr>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>Terminal Total:</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + cashOnCard + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + cashPaidIn + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + difference + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + netTickets + "</b></td>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("</tr>");

                    htmlcontent.Append("<tr>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>Grand Total:</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + cashOnCardTotal + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + cashPaidInTotal + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + differenceTotal + "</b></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'></td>");
                    htmlcontent.Append("<td style='width:100px;border-bottom: thin solid black;'><b>R" + netTicketsTotal + "</b></td>");
                    htmlcontent.Append("<td style='width:100px'></td>");
                    htmlcontent.Append("</tr>");
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
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            string comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            ReportsService service = new ReportsService();

            Tuple<DataSet, List<RevenueByDuty>> result = service.RevenueByDuty(conKey, filters, spName, comp);//full duty report
            DataSet ds = result.Item1;
            if (ds.Tables[0].Rows.Count > 0)
            {
                CrystalDecisions.Shared.ExportFormatType fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                if (fileType != CrystalDecisions.Shared.ExportFormatType.Excel)
                {
                    ReportClass rptH = new ReportClass
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
                    return RedirectToAction("RevenueByDuty", "Report");
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
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            string comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            ReportsService service = new ReportsService();

            Tuple<DataSet, List<RevenueByDuty>> result = service.RevenueByDutyAll(conKey, filters, spName, comp);//full duty report
            DataSet ds = result.Item1;
            if (ds.Tables[0].Rows.Count > 0)
            {
                CrystalDecisions.Shared.ExportFormatType fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                if (fileType != CrystalDecisions.Shared.ExportFormatType.Excel)
                {
                    ReportClass rptH = new ReportClass
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
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            DailyAuditViewModel model = new ReportsService().GetFilterForDailyAudit(conKey);
            return View("SellersAudit", model);
        }

        public ActionResult DownloadSellersDailyAuditReport(DailyAuditViewModel filters)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;


            DataSet ds = new ReportsService().GetDailyAuditReportForSellers(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                CrystalDecisions.Shared.ExportFormatType fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

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
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            DailyAuditViewModel model = new ReportsService().GetFilterForDailyAudit(conKey);
            return View("DailyAudit", model);
        }

        public ActionResult DailyAnalysisReport()
        {
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            DailyAuditViewModel model = new ReportsService().GetFilterForDailyAudit(conKey);
            return View("DailyAnalysis", model);
        }

        public ActionResult DownloadDailyAuditReport(DailyAuditViewModel filters)
        {
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            string comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            DataSet ds = new ReportsService().GetDailyAuditReport(conKey, filters, comp);

            CrystalDecisions.Shared.ExportFormatType fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

            ReportClass rptH = null;
            try
            {
                string fileName = "";
                string key = conKey.ToString().ToLower();
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
            CashierReportSummaryFilter model = new CashierReportSummaryFilter();
            InspectorReportService service = new InspectorReportService();
            List<OperatorDetails> staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.StaffList = staff.Where(s => s.OperatorType.ToLower() == "driver".ToLower().Trim() || s.OperatorType.ToLower() == "seller".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            return View("DailyAuditAtamelang", model);
        }

        public ActionResult DownloadDailyAuditAtamelangReport(CashierReportSummaryFilter filter)
        {
            UserSettings userset = GetUserSettings();
            filter.EndDate = filter.StartDate;
            DataSet ds = new SmartCardService().GetDailyAuditAtamelangReportDataset(userset.ConnectionKey, filter, userset.CompanyName);
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

        public ActionResult DailyAuditMatatieleReport()
        {
            CashierReportSummaryFilter model = new CashierReportSummaryFilter();
            InspectorReportService service = new InspectorReportService();
            List<OperatorDetails> staff = service.GetAllSatffDetails(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            model.StaffList = staff.Where(s => s.OperatorType.ToLower() == "driver".ToLower().Trim() || s.OperatorType.ToLower() == "seller".ToLower().Trim()).Select(s => new SelectListItem { Text = s.OperatorName + " - " + s.OperatorID, Value = s.OperatorID }).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.Classes = service.GetAllClasses(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            model.ClassTypes = service.GetAllClassTypes(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();

            return View("DailyAuditMatatiele", model);
        }

        public ActionResult DownloadDailyAuditMatatieleReport(CashierReportSummaryFilter filter)
        {
            UserSettings userset = GetUserSettings();
            filter.EndDate = filter.StartDate;
            DataSet ds = new SmartCardService().GetDailyAuditMatatieleReportDataset(userset.ConnectionKey, filter, userset.CompanyName);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filter.ExcelOrPDF, "~/CrystalReports/Rpt/DailyAudit/DailyAuditMatatiele.rpt", ds, "DailyAudit ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("DailyAuditMatatieleReport", "Report");
            }

        }

        #endregion

        #region JourneyAnalysisSummaryBySubRoute
        public ActionResult JourneyAnalysisSummaryBySubRouteReport()
        {
            JourneyAnalysisSummaryBySubRouteViewModel model = new ReportsService().GetJourneyAnalysisSummaryBySubRouteFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("JourneyAnalysisSummaryBySubRoute", model);
        }

        public ActionResult JourneyAnalysisSummaryBySubRouteDownload(JourneyAnalysisSummaryBySubRouteViewModel filters)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            ReportsService service = new ReportsService();

            DataSet ds = service.GetJourneyAnalysisSummaryBySubRouteDetails(conKey, filters, sp9, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
                return DownLoadReportByDataSet(filters.ExcelOrPDF, "~/CrystalReports/Rpt/FormE/JourneyAnalysisSummaryBySubRoute.rpt", ds, "JourneyAnalysisSummaryBySubRoute ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("JourneyAnalysisSummaryBySubRouteReport", "Report");
            }
        }
        #endregion

        #region FormEDetailed

        public ActionResult FormEDetailedReport()
        {
            SchVsOprViewModel model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("FormEDetailed", model);
        }


        public ActionResult FormEDetailedReportDownload(SchVsOprViewModel filters)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            ReportsService service = new ReportsService();

            DataSet ds = service.GetFormEDetailed(conKey, filters, sp7, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
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
            SchVsOprViewModel model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("FormE", model);
        }

        public ActionResult FormEReportDownload(SchVsOprViewModel filters)
        {
            string key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            if (key.Equals("atamelang70"))
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/FormE/FormEAtamelangTGX.rpt"), "FormE " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp7, true); }
            else
            { return DownloadReportGeneric(filters, Server.MapPath("~/CrystalReports/Rpt/FormE/FormE.rpt"), "FormE " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp7, true); }
        }

        public ActionResult FormEReferenceReport()
        {
            SchVsOprViewModel model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("FormEReference", model);
        }

        public ActionResult FormEReferenceReportDownload(SchVsOprViewModel filters)
        {
            string key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
            return DownloadFormEReference(filters, Server.MapPath("~/CrystalReports/Rpt/FormE/FormEReference.rpt"), "FormEReference " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"), sp10);
        }

        #endregion

        #region FormE2
        public ActionResult FullDutySummary()
        {
            SchVsOprViewModel model = new ReportsService().GetFilter(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey);
            return View("FullDutySummary", model);
        }

        public ActionResult FullDutySummaryDownload(SchVsOprViewModel filters)
        {
            string key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
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
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            string comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            SmartCardService service = new SmartCardService();

            DataSet ds = service.GetSmartCardTransDataSet(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                CrystalDecisions.Shared.ExportFormatType fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                ReportClass rptH = new ReportClass
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

        #region Passenger Transactions

        public ActionResult PassengerTransactions()
        {
            return View("PassengerTransactions", new PassengerTransFilter());
        }

        public ActionResult DownloadPassengerTrans(PassengerTransFilter filters)
        {
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            string comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            PassengerService service = new PassengerService();

            DataSet ds = service.GetPassengerTransDataSet(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                CrystalDecisions.Shared.ExportFormatType fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                ReportClass rptH = new ReportClass
                {
                    FileName = Server.MapPath("~/CrystalReports/Rpt/PassengerTransactions/PassengerTransactions.rpt")
                };

                try
                {
                    rptH.Load();

                    rptH.SetDataSource(ds.Tables[0]);

                    rptH.ExportToHttpResponse(fileType, System.Web.HttpContext.Current.Response, true, "Passenger Trans " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"));

                    return new DownloadPdfResult(rptH, "Passenger Trans " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"));
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
                return RedirectToAction("PassengerTransactions", "Report");
            }
        }

        #endregion

        #region DailyInspectorReport

        public ActionResult DailyInspectorReport()
        {
            InspectorReportService service = new InspectorReportService();
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            InspectorReportFilter model = service.GetInspectorReportFilter(conKey);

            return View("DailyInspectorReport", model);
        }

        public ActionResult DailyInspectorReportDownload(InspectorReportFilter filters)
        {
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            string comp = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName;

            InspectorReportService service = new InspectorReportService();

            DataSet ds = service.GetInspectorTransDataSet(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                CrystalDecisions.Shared.ExportFormatType fileType = filters.ExcelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

                ReportClass rptH = new ReportClass
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
            SalesAnalysisService service = new SalesAnalysisService();
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            SalesAnalysisFilter model = service.GetSalesAnalysisFilter(conKey);
            model.RouteTypes = service.GetAllRouteTypes(conKey);

            return View("OriginAnalysisByRoute", model);
        }

        public ActionResult OriginAnalysisByRouteReportDownload(SalesAnalysisFilter filters)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            SalesAnalysisService service = new SalesAnalysisService();

            DataSet ds = service.GetOriginAnalysisByRouteDataSet(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string key = ((EBusPrinciple)System.Threading.Thread.CurrentPrincipal).Properties.ConnKey.ToString().ToLower();
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
            SalesAnalysisService service = new SalesAnalysisService();
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            SalesAnalysisFilter model = service.GetSalesAnalysisFilter(conKey);

            return View("SalesAnalysisByRoute", model);
        }

        public ActionResult SalesAnalysisByRouteReportDownload(SalesAnalysisFilter filters)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            SalesAnalysisService service = new SalesAnalysisService();

            DataSet ds = service.GetSalesAnalysisByRouteDataSet(conKey, filters, comp);
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

        #region Sales AnalysisByRoute Summary

        public ActionResult SalesAnalysisByRouteSummary()
        {
            SalesAnalysisService service = new SalesAnalysisService();
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            SalesAnalysisFilter model = service.GetSalesAnalysisSummaryFilter(conKey);

            return View("SalesAnalysisByRouteSummary", model);
        }

        public ActionResult SalesAnalysisByRouteSummaryReportDownload(SalesAnalysisFilter filters)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            SalesAnalysisService service = new SalesAnalysisService();

            DataSet ds = service.GetSalesAnalysisByRouteSummaryDataSet(conKey, filters, comp);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DownLoadReportByDataSet(filters.ExcelOrPDF, "~/CrystalReports/Rpt/SalesAnalysis/SalesAnalysisByRouteSummary.rpt", ds, "SalesAnalysis ByRoute Summary ");
            }
            else
            {
                TempData["AlertMessage"] = "show";
                return RedirectToAction("SalesAnalysisByRouteSummary", "Report");
            }

        }

        #endregion

        #region ClassSummaryByClassType

        public ActionResult ClassSummaryByClassType()
        {
            SalesAnalysisService service = new SalesAnalysisService();
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            SalesAnalysisFilter model = service.GetSalesAnalysisFilter(conKey);

            return View("ClassSummaryByClassType", model);
        }

        public ActionResult ClassSummaryByClassTypeGraph()
        {
            SalesAnalysisService service = new SalesAnalysisService();
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            SalesAnalysisFilter model = service.GetSalesAnalysisFilterForGraph(conKey);

            return View("ClassSummaryByClassTypeGraph", model);
        }

        public JsonResult GetClassSummaryByClassTypeGraphData(string classGroupId)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;
            SalesAnalysisFilter filters = new SalesAnalysisFilter();

            if (!string.IsNullOrEmpty(classGroupId) && classGroupId.ToLower() != "all")
            {
                filters.ClassGroupsSelected = new string[] { classGroupId };
            }

            SalesAnalysisService service = new SalesAnalysisService();

            List<ClassTypeGraph> list = service.GetClassSummaryByClassTypeGraphData(conKey, filters);

            var jsonData = new
            {
                Data = list,
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ClassSummaryByClassTypeDownload(SalesAnalysisFilter filters)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            SalesAnalysisService service = new SalesAnalysisService();

            DataSet ds = service.GetClassSummaryDataSet(conKey, filters, comp, "EbusClassSummaryByClassType", true);
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

            AuditStatusService service = new AuditStatusService();
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            AuditStatusModel model = new AuditStatusModel
            {
                AuditStatuses = service.GetAuditStatusForGraph(conKey),
                Reasons = service.GetAuditComReasons(conKey)
            };
            return View("AuditStatus", model);
        }

        [HttpPost]
        public string UpdateAuditComStatus(string busID, string reasonID)
        {
            AuditStatusService service = new AuditStatusService();
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;
            service.UpdateAuditComStatus(busID, reasonID, conKey);
            return "Record Updated Successfully!!";
        }

        #endregion

        #region Sales Analysis by Class

        public ActionResult SalesAnalysisByClass()
        {
            SalesAnalysisService service = new SalesAnalysisService();
            string conKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey;

            SalesAnalysisFilter model = service.GetSalesAnalysisFilter(conKey);

            return View("SalesAnalysisByClass", model);
        }

        public ActionResult SalesAnalysisByClassDownload(SalesAnalysisFilter filters)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            SalesAnalysisService service = new SalesAnalysisService();

            DataSet ds = service.GetClassSummaryDataSet(conKey, filters, comp, "EbusSalesAnalysisByClass", false);
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
            CashierFilter model = new CashierFilter();
            InspectorReportService service = new InspectorReportService();
            model.Locations = service.GetAllLocations(((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey).OrderBy(s => Convert.ToInt32(s.Value)).ToList();
            return View("CashierReport", model);
        }

        public ActionResult CashierReportDownload(CashierFilter filters)
        {
            UserSettings userset = GetUserSettings();
            string conKey = userset.ConnectionKey;
            string comp = userset.CompanyName;

            CashierServices service = new CashierServices();

            DataSet ds = service.GetCashierDataSet(conKey, filters, comp);
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
            CrystalDecisions.Shared.ExportFormatType fileType = excelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;

            ReportClass rptH = new ReportClass
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

        private ActionResult DownLoadCashierReportMatatieleByDataSet(bool excelOrPDF, string rptPath, DataSet ds, string reportName)
        {
            CrystalDecisions.Shared.ExportFormatType fileType = excelOrPDF ? CrystalDecisions.Shared.ExportFormatType.PortableDocFormat : CrystalDecisions.Shared.ExportFormatType.Excel;
            if (fileType == CrystalDecisions.Shared.ExportFormatType.PortableDocFormat)
            {
                ReportClass rptH = new ReportClass
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
            else
            {
                List<CashierReportMatatiele> collection = ConvertDataTableToCashierReportMatatieleData(ds.Tables[0]);
                ExportCashierReportMatatieleToExcel(collection, reportName + " " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"));
                return RedirectToAction("CashierReportMatatiele", "Report");
            }


        }

        private List<CashierReportMatatiele> ConvertDataTableToCashierReportMatatieleData(DataTable dataTable)
        {
            List<CashierReportMatatiele> collection = new List<CashierReportMatatiele>();
            if (dataTable.Rows.Count > 0)
            {
                foreach (DataRow item in dataTable.Rows)
                {
                    collection.Add(new CashierReportMatatiele()
                    {
                        StaffNumber = item["StaffNumber"].ToString(),
                        StaffName = item["str50_StaffName"].ToString(),
                        Date = Convert.ToDateTime(item["Date"]).ToString("dd-MM-yyyy"),
                        Time = Convert.ToDateTime(item["Time"]).ToString("HH:mm:ss"),
                        DriverTotal = Convert.ToInt32(item["DriverTotal"]),
                        CashPaidIn = Convert.ToInt32(item["CashPaidIn"]),
                        Difference = Convert.ToInt32(item["Difference"]),
                        Reason = item["Reason"].ToString(),
                        NetTickets = Convert.ToInt32(item["NetTickets"]),
                        CashinReceiptNo = Convert.ToInt32(item["CashinReceiptNo"]),
                        CompanyName = item["CompanyName"].ToString(),
                        Cashiers = item["Cashiers"].ToString(),
                        DateSelected = item["DateSelected"].ToString(),
                        Locations = item["Locations"].ToString(),
                        Terminals = item["Terminals"].ToString(),
                        LocationDesc = item["str50_LocationDesc"].ToString(),
                        Terminal = item["Terminal"].ToString(),
                        CashierID = item["CashierID"].ToString(),
                        CashierName = item["str50_CashierName"].ToString()
                    });
                }
            }
            return collection;
        }

        private UserSettings GetUserSettings()
        {
            UserSettings res = new UserSettings
            {
                ConnectionKey = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.ConnKey,
                CompanyName = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.CompanyName,
                Username = HttpContext.User.Identity.Name,
                WarningDate = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.WarningDate,
                LastDate = ((EBusPrinciple)Thread.CurrentPrincipal).Properties.LastDate,
            };
            return res;
        }

        #endregion
    }
}
