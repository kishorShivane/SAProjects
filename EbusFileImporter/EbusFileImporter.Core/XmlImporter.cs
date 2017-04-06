using EbusFileImporter.Core.Helpers;
using EbusFileImporter.Core.Interfaces;
using EbusFileImporter.DataProvider;
using EbusFileImporter.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EbusFileImporter.Core
{
    public class XmlImporter : IImporter
    {
        public static ILogService Logger { get; set; }
        Helper helper = null;
        EmailHelper emailHelper = null;
        public XmlImporter(ILogService logger)
        {
            Logger = logger;
            helper = new Helper(logger);
            emailHelper = new EmailHelper(logger);
        }
        static EbusImporterContext context = new EbusImporterContext();

        public bool PostImportProcessing(string filePath)
        {
            bool result = false;

            return result;
        }

        public bool PreImportProcessing(string filePath)
        {
            bool result = false;
            if (ValidateFile(filePath)) result = true;
            return result;
        }

        public bool ProcessFile(string filePath)
        {
            bool result = false;
            Logger.Info("***********************************************************");
            Logger.Info("Started Import");
            Logger.Info("***********************************************************");
            while (helper.IsFileLocked(filePath))
            {
                Thread.Sleep(500);
            }
            XDocument xdoc = XDocument.Load(filePath);
            var nodes = xdoc.Root.Elements().ToList();
            var todayDate = DateTime.Now;
            var splitFilepath = filePath.Split('\\');
            var dbName = splitFilepath[splitFilepath.Length - 3];

            #region Check for file completeness
            var fileClosureNode = nodes.Where((x => x.Attribute("STXID").Value.Equals("82")));
            if (fileClosureNode == null || !fileClosureNode.Any())
            {
                Logger.Info("***********************************************************");
                Logger.Info("No file closure found, Moving file to error folder");
                Logger.Info("***********************************************************");
                helper.MoveErrorFile(filePath, dbName);
                emailHelper.SendMail(filePath, dbName, "", EmailType.Error);
            }
            #endregion

            #region Process Module Information
            Module moduleDetail = null;
            var latestModuleID = 0;
            var node18 = nodes.Where(x => x.Attribute("STXID").Value.Equals("18")).FirstOrDefault();
            if (node18 != null)
            {
                var tempModule = context.Modules.OrderByDescending(x => x.id_Module).FirstOrDefault();
                latestModuleID = (tempModule == null ? 0 : tempModule.id_Module) + 1;
                moduleDetail = new Module();
                moduleDetail.id_Module = latestModuleID;
                moduleDetail.str_LocationCode = null;
                moduleDetail.int4_ModuleID = (int)node18.Element("ModuleESN");
                moduleDetail.int4_SignOnID = (int)node18.Element("DriverNumber1");
                moduleDetail.int4_OnReaderID = (int)node18.Element("HomeDepotID");
                var date = helper.ConvertToInsertDateString((string)node18.Element("SignOnDate"));
                moduleDetail.dat_SignOnDate = date;
                moduleDetail.dat_SignOnTime = helper.ConvertToInsertDateTimeString((string)node18.Element("SignOnDate"), (string)node18.Element("SignOnTime"));
                moduleDetail.int4_OffReaderID = (int)node18.Element("HomeDepotID");
                date = helper.ConvertToInsertDateString((string)node18.Element("SignOffDate"));
                moduleDetail.dat_SignOffDate = date;
                var time = helper.ConvertToInsertDateTimeString((string)node18.Element("SignOffDate"), (string)node18.Element("SignOffTime")); ;
                moduleDetail.dat_SignOffTime = time;
                moduleDetail.dat_TrafficDate = date;
                moduleDetail.dat_ModuleOutDate = date;
                moduleDetail.dat_ModuleOutTime = time;
                moduleDetail.int4_HdrModuleRevenue = (int)node18.Element("TotalNetCashPaid");
                moduleDetail.int4_HdrModuleTickets = (int)node18.Element("TotalNetCashPassengers");
                moduleDetail.int4_HdrModulePasses = null;//(int)node18.Element("DriverNumber1");
                moduleDetail.int4_ModuleRevenue = (int)node18.Element("TotalGrossCash");
                moduleDetail.int4_ModuleTickets = (int)node18.Element("TotalGrossCashPassengers");
                moduleDetail.int4_ModulePasses = 0;
                moduleDetail.int4_ModuleNonRevenue = 0;
                moduleDetail.int4_ModuleTransfer = 0;
                moduleDetail.dat_ImportStamp = todayDate.Date;
                moduleDetail.dat_RecordMod = todayDate;
                moduleDetail.int4_ImportModuleKey = null;
                moduleDetail.id_BatchNo = Convert.ToInt32((todayDate.Year.ToString() + todayDate.Month.ToString() + todayDate.Day.ToString()));
                moduleDetail.byt_IeType = null;
                moduleDetail.byt_ModuleType = (int)node18.Element("ModuleType");
            }
            #endregion

            #region Process Duty Information   
            Duty dutyDetail = null;
            var node151 = nodes.Where(x => x.Attribute("STXID").Value.Equals("151")).FirstOrDefault();
            var node122 = nodes.Where(x => x.Attribute("STXID").Value.Equals("122")).FirstOrDefault();
            var node154 = nodes.Where(x => x.Attribute("STXID").Value.Equals("154")).FirstOrDefault();
            var node155 = nodes.Where(x => x.Attribute("STXID").Value.Equals("155")).FirstOrDefault();
            var latestDutyID = 0;
            if (node151 != null && node122 != null && node154 != null && node155 != null)
            {
                var tempDuty = context.Duties.OrderByDescending(x => x.id_Duty).FirstOrDefault();
                latestDutyID = (tempDuty == null ? 0 : tempDuty.id_Duty) + 1;

                dutyDetail = new Duty();
                dutyDetail.id_Duty = latestDutyID;
                dutyDetail.id_Module = latestModuleID;
                dutyDetail.int4_DutyID = (int)node151.Element("DutyNo");
                dutyDetail.int4_OperatorID = (int)node151.Element("DriverNumber");
                dutyDetail.str_ETMID = (string)node122.Element("ETMNumber");
                dutyDetail.int4_GTValue = (int)node151.Element("ETMCashTotal");
                dutyDetail.int4_NextTicketNumber = (int)node151.Element("NextTicketNo");
                dutyDetail.int4_DutySeqNum = (int)node151.Element("DutySeqNo.");
                var date = helper.ConvertToInsertDateString((string)node151.Element("DutyDate"));
                dutyDetail.dat_DutyStartDate = date;
                dutyDetail.dat_DutyStartTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)node151.Element("DutyTime"));
                dutyDetail.dat_DutyStopDate = helper.ConvertToInsertDateString((string)node154.Element("SignOffDate"));
                dutyDetail.dat_DutyStopTime = helper.ConvertToInsertDateTimeString((string)node154.Element("SignOffDate"), (string)node154.Element("SignOffTime"));
                dutyDetail.dat_TrafficDate = date;
                dutyDetail.str_BusID = (string)node151.Element("FleetID");
                dutyDetail.int4_DutyRevenue = (int)node154.Element("DutyCashTotal");
                dutyDetail.int4_DutyTickets = (int)node154.Element("DutyTicketTotal");
                dutyDetail.int4_DutyPasses = 0;
                dutyDetail.int4_DutyNonRevenue = 0;
                dutyDetail.int4_DutyTransfer = 0;
                dutyDetail.str_FirstRouteID = (string)node155.Element("RouteVariantNo");
                dutyDetail.int2_FirstJourneyID = (short)node155.Element("JourneyNo");
                dutyDetail.dat_RecordMod = todayDate;
                dutyDetail.id_BatchNo = todayDate.Year + todayDate.Month + todayDate.Day;
                dutyDetail.byt_IeType = null;
                dutyDetail.str_EpromVersion = null;
                dutyDetail.str_OperatorVersion = (string)node151.Element("RunningBoard");
                dutyDetail.str_SpecialVersion = (string)node154.Element("SignOffMode");
                dutyDetail.int4_DutyAnnulCash = null;
                dutyDetail.int4_DutyAnnulCount = null;
            }
            //context.Duties.Add(dutyDetails);
            //context.SaveChanges();
            #endregion

            #region Process Journey Information
            Journey journeyDetail = null;
            List<Journey> journeyDetails = new List<Journey>();
            List<Stage> stageDetails = new List<Stage>();
            Stage stageDetail = null;
            Tran transDetail = null;
            List<Tran> transDetails = new List<Tran>(); ;
            PosTran posTransDetail = null;
            List<PosTran> posTransDetails = new List<PosTran>();
            var latestJourneyID = 0;
            var nodes156 = nodes.Where(x => x.Attribute("STXID").Value.Equals("156"));
            var nodes155 = nodes.Where(x => x.Attribute("STXID").Value.Equals("155"));
            if (nodes156 != null && nodes155 != null)
            {
                var tempJourney = context.Journeys.OrderByDescending(i => i.id_Journey).FirstOrDefault();
                latestJourneyID = (tempJourney == null ? 0 : tempJourney.id_Journey) + 1;
                nodes155.ToList().ForEach(x =>
                {
                    var startNode155 = x;
                    var endNode156 = nodes156.Where(i => Convert.ToInt32(i.Attribute("Position").Value) > Convert.ToInt32(x.Attribute("Position").Value)).OrderBy(i => Convert.ToInt32(i.Attribute("Position").Value)).FirstOrDefault();
                    var eachJourneyNodes = nodes.Where(i => Convert.ToInt32(i.Attribute("Position").Value) > Convert.ToInt32(startNode155.Attribute("Position").Value) && Convert.ToInt32(i.Attribute("Position").Value) < Convert.ToInt32(endNode156.Attribute("Position").Value)).ToList();
                    journeyDetail = new Journey();
                    journeyDetail.id_Journey = latestJourneyID;
                    journeyDetail.id_Duty = dutyDetail.id_Duty;
                    journeyDetail.id_Module = moduleDetail.id_Module;
                    journeyDetail.str_RouteID = (string)startNode155.Element("RouteVariantNo");
                    journeyDetail.int2_JourneyID = (short)startNode155.Element("JourneyNo");
                    journeyDetail.int2_Direction = (short)startNode155.Element("Direction");
                    var date = helper.ConvertToInsertDateString((string)startNode155.Element("StartDate"));
                    journeyDetail.dat_JourneyStartDate = date;
                    journeyDetail.dat_JourneyStartTime = helper.ConvertToInsertDateTimeString((string)startNode155.Element("StartDate"), (string)startNode155.Element("StartTime"));
                    var jourDate = helper.ConvertToInsertDateString((string)endNode156.Element("JourneyStopDate"));
                    journeyDetail.dat_JourneyStopDate = jourDate;
                    journeyDetail.dat_JourneyStopTime = helper.ConvertToInsertDateTimeString((string)endNode156.Element("JourneyStopDate"), (string)endNode156.Element("JourneyStopTime"));
                    journeyDetail.dat_TrafficDate = date;
                    journeyDetail.int4_Distance = 0;
                    journeyDetail.int4_Traveled = 0;
                    journeyDetail.int4_JourneyRevenue = (int)endNode156.Element("JourneyCashTotal");
                    journeyDetail.int4_JourneyTickets = (int)endNode156.Element("JourneyTicketTotal");
                    journeyDetail.int4_JourneyPasses = 0;
                    journeyDetail.int4_JourneyNonRevenue = 0;
                    journeyDetail.int4_JourneyTransfer = 0;
                    journeyDetail.dat_RecordMod = todayDate;
                    journeyDetail.id_BatchNo = Convert.ToInt32((todayDate.Year.ToString() + todayDate.Month.ToString() + todayDate.Day.ToString()));
                    journeyDetail.byt_IeType = null;
                    journeyDetail.dat_JourneyMoveTime = null;
                    journeyDetail.dat_JourneyArrivalTime = null;
                    journeyDetail.int4_GPSDistance = null;
                    journeyDetails.Add(journeyDetail);

                    #region Process Stage Information
                    var latestStageID = 0;
                    var nodes113 = eachJourneyNodes.Where(i => i.Attribute("STXID").Value.Equals("113"));

                    if (nodes113 != null)
                    {
                        var tempStage = context.Stages.OrderByDescending(i => i.id_Stage).FirstOrDefault();
                        latestStageID = (tempStage == null ? 0 : tempStage.id_Stage) + 1;
                        var listNodes113 = nodes113.ToList();
                        var count = listNodes113.Count();
                        for (int i = 0; i < count; i++)
                        {
                            var thisNode113 = listNodes113[i];
                            stageDetail = new Stage();
                            stageDetail.id_Journey = latestJourneyID;
                            stageDetail.id_Duty = latestDutyID;
                            stageDetail.id_Module = moduleDetail.id_Module;
                            stageDetail.int2_StageID = (short)thisNode113.Element("BoardingStage");
                            date = helper.ConvertToInsertDateString((string)startNode155.Element("StartDate"));
                            stageDetail.dat_StageDate = date;
                            stageDetail.dat_StageTime = helper.ConvertToInsertDateTimeString((string)startNode155.Element("StartDate"), (string)thisNode113.Element("Time"));
                            stageDetail.dat_RecordMod = todayDate;
                            stageDetail.id_BatchNo = Convert.ToInt32((todayDate.Year.ToString() + todayDate.Month.ToString() + todayDate.Day.ToString()));
                            stageDetails.Add(stageDetail);
                            latestStageID++;

                            #region Process Trans Information
                            var nextPosition = 0;
                            if ((i + 1) < count)
                                nextPosition = Convert.ToInt32(listNodes113[i + 1].Attribute("Position").Value);
                            else
                                nextPosition = Convert.ToInt32(endNode156.Attribute("Position").Value);

                            var eachStageNodes = eachJourneyNodes.Where(j => Convert.ToInt32(j.Attribute("Position").Value) > Convert.ToInt32(thisNode113.Attribute("Position").Value) && Convert.ToInt32(j.Attribute("Position").Value) < nextPosition).ToList();

                            var latestTransID = 0; var latestPosTransID = 0;
                            var cashTransNodes = eachStageNodes.Where(j => j.Attribute("STXID").Value.Equals("157"));
                            var smartCardTransNodes = eachStageNodes.Where(j => j.Attribute("STXID").Value.Equals("188"));

                            if ((cashTransNodes != null && cashTransNodes.Any()) || (smartCardTransNodes != null && smartCardTransNodes.Any()))
                            {
                                var tempPosTrans = context.PosTrans.OrderByDescending(j => j.id_PosTrans).FirstOrDefault();
                                latestPosTransID = (tempPosTrans == null ? 0 : tempPosTrans.id_PosTrans) + 1;
                                var tempTrans = context.Trans.OrderByDescending(j => j.id_Trans).FirstOrDefault();
                                latestTransID = (tempTrans == null ? 0 : tempTrans.id_Trans) + 1;

                                cashTransNodes.ToList().ForEach(t =>
                                {
                                    var thisTrans = t;
                                    transDetail = new Tran();
                                    var ticketType = t.Element("TicketType").Value.Trim();
                                    transDetail.id_Trans = latestTransID;
                                    transDetail.id_Stage = stageDetail.id_Stage;
                                    transDetail.id_Journey = journeyDetail.id_Journey;
                                    transDetail.id_Duty = dutyDetail.id_Duty;
                                    transDetail.id_Module = moduleDetail.id_Module;
                                    transDetail.str_LocationCode = null;
                                    transDetail.int2_BoardingStageID = stageDetail.int2_StageID;
                                    transDetail.int2_AlightingStageID = (short)thisTrans.Element("StageNo");
                                    transDetail.int2_Class = Convert.ToInt16(ticketType, 16);
                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                    transDetail.int4_NonRevenue = 0;
                                    transDetail.int2_TicketCount = 1;
                                    transDetail.int2_PassCount = 0;
                                    transDetail.int2_Transfers = 0;
                                    transDetail.dat_TransDate = helper.ConvertToInsertDateString((string)thisTrans.Element("IssueDate"));
                                    transDetail.dat_TransTime = helper.ConvertToInsertDateTimeString((string)thisTrans.Element("IssueDate"), (string)thisTrans.Element("IssueTime"));
                                    transDetail.str_SerialNumber = null;
                                    transDetail.int4_RevenueBal = 0;
                                    transDetail.int4_TripBal = 0;
                                    transDetail.int2_AnnulCount = null;
                                    transDetail.int4_AnnulCash = null;
                                    transDetail.id_SCTrans = null;
                                    transDetail.int4_TicketSerialNumber = (int)thisTrans.Element("TicketSerialNo");
                                    transDetails.Add(transDetail);
                                    latestTransID++;
                                });

                                smartCardTransNodes.ToList().ForEach(t =>
                                {
                                    var thisTrans = t;
                                    transDetail = new Tran();
                                    var ticketType = t.Element("TicketType").Value.Trim();
                                    var productData = (string)thisTrans.Element("Product1Data");
                                    transDetail.id_Trans = latestTransID;
                                    transDetail.id_Stage = stageDetail.id_Stage;
                                    transDetail.id_Journey = journeyDetail.id_Journey;
                                    transDetail.id_Duty = dutyDetail.id_Duty;
                                    transDetail.id_Module = moduleDetail.id_Module;
                                    transDetail.str_LocationCode = null;
                                    transDetail.int2_BoardingStageID = stageDetail.int2_StageID;
                                    transDetail.int2_AlightingStageID = (short)thisTrans.Element("StageNo");
                                    //transDetail.int2_Class = Convert.ToInt16(ticketType, 16);
                                    transDetail.dat_TransDate = helper.ConvertToInsertDateString((string)thisTrans.Element("IssueDate"));
                                    transDetail.dat_TransTime = helper.ConvertToInsertDateTimeString((string)thisTrans.Element("IssueDate"), (string)thisTrans.Element("IssueTime"));
                                    var serialNumber = Convert.ToInt32((string)thisTrans.Element("ESN"), 16).ToString();
                                    transDetail.str_SerialNumber = serialNumber;
                                    //transDetail.int4_TripBal = Helper.GetTripBalanceFromProductData(productData);
                                    transDetail.int2_AnnulCount = null;
                                    transDetail.int4_AnnulCash = null;
                                    transDetail.id_SCTrans = null;
                                    transDetail.int4_TicketSerialNumber = (int)thisTrans.Element("TicketSerialNo");

                                    #region Process Ticket Type 
                                    var nonRevenue = 0;
                                    switch (ticketType)
                                    {
                                        case "2328":
                                            transDetail.int2_Class = 995;
                                            transDetail.int4_Revenue = 0;
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 0;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 1;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            if (!helper.IsTransferTransaction(productData))
                                            {
                                                nonRevenue = GetNonRevenueFromPosTransTable(serialNumber);
                                                transDetail.int2_Class = 999;
                                                transDetail.int2_PassCount = 1;
                                                transDetail.int2_Transfers = 0;
                                            }
                                            transDetail.int4_NonRevenue = nonRevenue;

                                            break;
                                        case "2329":
                                            transDetail.int2_Class = 996;
                                            transDetail.int4_Revenue = 0;
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 0;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 1;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            if (!helper.IsTransferTransaction(productData))
                                            {
                                                nonRevenue = GetNonRevenueFromPosTransTable(serialNumber);
                                                transDetail.int2_Class = 997;
                                                transDetail.int2_PassCount = 1;
                                                transDetail.int2_Transfers = 0;
                                            }
                                            transDetail.int4_NonRevenue = nonRevenue;
                                            break;
                                        case "03F9":
                                            transDetail.int2_Class = 701;
                                            transDetail.int4_Revenue = 0;
                                            transDetail.int2_TicketCount = 0;
                                            transDetail.int2_PassCount = 1;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_TripBal = 0;
                                            transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                            transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                            break;
                                        case "03FA":
                                            transDetail.int2_Class = 703;
                                            transDetail.int4_Revenue = 0;
                                            transDetail.int2_TicketCount = 0;
                                            transDetail.int2_PassCount = 1;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_TripBal = 0;
                                            transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                            transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                            break;
                                        case "409":
                                            transDetail.int2_Class = 702;
                                            transDetail.int4_Revenue = 0;
                                            transDetail.int2_TicketCount = 0;
                                            transDetail.int2_PassCount = 1;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_TripBal = 0;
                                            transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                            transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                            break;
                                        case "40A":
                                            transDetail.int2_Class = 704;
                                            transDetail.int4_Revenue = 0;
                                            transDetail.int2_TicketCount = 0;
                                            transDetail.int2_PassCount = 1;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_TripBal = 0;
                                            transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                            transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                            break;
                                        case "2715":
                                            transDetail.int2_Class = 711;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2718":
                                            transDetail.int2_Class = 712;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2727":
                                            transDetail.int2_Class = 713;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2722":
                                            transDetail.int2_Class = 715;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2723":
                                            transDetail.int2_Class = 717;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2719":
                                            transDetail.int2_Class = 721;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "271C":
                                            transDetail.int2_Class = 722;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2726":
                                            transDetail.int2_Class = 741;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2728":
                                            transDetail.int2_Class = 742;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2714":
                                            transDetail.int2_Class = 743;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2710":
                                            transDetail.int2_Class = 744;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2711":
                                            transDetail.int2_Class = 745;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2712":
                                            transDetail.int2_Class = 746;
                                            transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                            transDetail.int4_TripBal = 0;
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2AF9":
                                            transDetail.int2_Class = 731;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = 0;
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        case "2AFA":
                                            transDetail.int2_Class = 732;
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;
                                            transDetail.int2_TicketCount = 1;
                                            transDetail.int2_PassCount = 0;
                                            transDetail.int2_Transfers = 0;
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = 0;
                                            posTransDetail = MapTransToPosTrans(transDetail);
                                            posTransDetail.id_PosTrans = latestPosTransID;
                                            break;
                                        default:
                                            break;
                                    }
                                    #endregion

                                    if (posTransDetail != null)
                                    {
                                        posTransDetails.Add(posTransDetail);
                                        latestPosTransID++;
                                    }
                                    if (transDetail != null)
                                    {
                                        transDetails.Add(transDetail);
                                        latestTransID++;
                                    }
                                });
                            }
                            #endregion
                        }
                    }
                    #endregion

                    latestJourneyID++;
                });
            }
            #endregion

            #region Process Staff Information
            var tempStaff = context.Staffs.Where(x => x.int4_StaffID.Equals(dutyDetail.int4_OperatorID));
            var isDriverExist = tempStaff == null ? false : true;
            Staff staffDetail = null;
            if (!isDriverExist)
            {

                var node125 = nodes.Where(x => x.Attribute("STXID").Value.Equals("125")).FirstOrDefault();
                if (node151 != null && node125 != null)
                {
                    staffDetail = new Staff();
                    staffDetail.int4_StaffID = dutyDetail.int4_OperatorID.Value;
                    staffDetail.str50_StaffName = "New Driver" + " - " + staffDetail.int4_StaffID;
                    staffDetail.bit_InUse = true;
                    staffDetail.int4_StaffTypeID = 1;
                    staffDetail.int4_StaffSubTypeID = 0;
                    var runningBoard = dutyDetail.str_OperatorVersion;
                    staffDetail.str4_LocationCode = runningBoard.Substring(runningBoard.Length - 4, 4);
                    staffDetail.str2_LocationCode = null;
                    var serialNumber = (string)node125.Element("DriverCardSerialNo");
                    staffDetail.SerialNumber = Convert.ToInt32(serialNumber, 16).ToString();
                }
            }
            #endregion

            #region Process Inspector Information

            Inspector inspectorDetail = null;
            List<Inspector> inspectorDetails = null;
            var latestInspectorID = 0;
            var nodes34 = nodes.Where(x => x.Attribute("STXID").Value.Equals("34"));
            if (nodes34 != null)
            {
                var tempInspector = context.Inspectors.OrderByDescending(x => x.id_Inspector).FirstOrDefault();
                latestInspectorID = (tempInspector == null ? 0 : tempInspector.id_Inspector) + 1;
                nodes34.ToList().ForEach(x =>
                {
                    var thisInspector = x;
                    var inspectorStage = nodes.Where(i => i.Attribute("STXID").Value.Equals("113") && Convert.ToInt32(i.Attribute("Position").Value) < Convert.ToInt32(thisInspector.Attribute("Position").Value)).OrderBy(i => Convert.ToInt32(i.Attribute("Position").Value)).FirstOrDefault();
                    inspectorDetail = new Inspector();
                    inspectorDetail.id_Inspector = latestInspectorID;
                    inspectorDetail.id_Stage = (int)inspectorStage.Element("BoardingStage");
                    inspectorDetail.id_InspectorID = (int)inspectorStage.Element("InspectorNo");
                    var date = helper.ConvertToInsertDateString((string)node151.Element("DutyDate"));
                    inspectorDetail.datTimeStamp = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)thisInspector.Element("Time")); ;
                    inspectorDetails.Add(inspectorDetail);
                    latestInspectorID++;
                });

            }
            #endregion

            #region Update Dependencies

            if (journeyDetails.Any())
            {
                journeyDetails.ForEach(x =>
                {
                    x.int4_JourneyPasses = transDetails.Where(i => i.id_Journey == x.id_Journey && i.int2_PassCount == 1).Count();
                    x.int4_JourneyNonRevenue = transDetails.Where(i => i.id_Journey == x.id_Journey).Sum(i => i.int4_NonRevenue ?? 0);
                    x.int4_JourneyTransfer = transDetails.Where(i => i.id_Journey == x.id_Journey && i.int2_Transfers == 1).Count();
                });
            }

            if (dutyDetail != null && journeyDetails.Any())
            {
                dutyDetail.int4_DutyPasses = journeyDetails.Where(i => i.id_Duty == dutyDetail.id_Duty && i.int4_JourneyPasses == 1).Count();
                dutyDetail.int4_DutyNonRevenue = journeyDetails.Where(i => i.id_Duty == dutyDetail.id_Duty).Sum(i => i.int4_JourneyNonRevenue ?? 0);
                dutyDetail.int4_DutyTransfer = journeyDetails.Where(i => i.id_Duty == dutyDetail.id_Duty && i.int4_JourneyTransfer == 1).Count(); ;
            }

            if (moduleDetail != null && dutyDetail != null)
            {
                var dutyDetails = new List<Duty>() { dutyDetail };
                moduleDetail.int4_ModulePasses = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module && i.int4_DutyPasses == 1).Count();
                moduleDetail.int4_ModuleNonRevenue = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyNonRevenue ?? 0);
                moduleDetail.int4_ModuleTransfer = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module && i.int4_DutyTransfer == 1).Count();
                moduleDetail.int4_HdrModulePasses = moduleDetail.int4_ModulePasses;
            }

            #endregion

            #region DB Insertion Section
            using (var dbContextTransaction = context.Database.BeginTransaction())
            {
                try
                {
                    Logger.Info("-------DB Transaction - Start-------");
                    if (moduleDetail != null)
                    {
                        context.Modules.Add(moduleDetail);
                        Logger.Info("Modules Inserted: 1");
                    }

                    if (dutyDetail != null)
                    {
                        context.Duties.Add(dutyDetail);
                        Logger.Info("Duties Inserted: 1");
                    }

                    if (journeyDetails != null && journeyDetails.Any())
                    {
                        context.Journeys.AddRange(journeyDetails);
                        Logger.Info("Journeys Inserted:  " + journeyDetails.Count().ToString());
                    }

                    if (stageDetails != null && stageDetails.Any())
                    {
                        context.Stages.AddRange(stageDetails);
                        Logger.Info("Stages Inserted:  " + stageDetails.Count().ToString());
                    }

                    if (staffDetail != null)
                    {
                        context.Staffs.Add(staffDetail);
                        Logger.Info("Staffs Inserted: 1");
                    }

                    if (inspectorDetails != null && inspectorDetails.Any())
                    {
                        context.Inspectors.AddRange(inspectorDetails);
                        Logger.Info("Inspectors Inserted: " + inspectorDetails.Count().ToString());
                    }

                    if (posTransDetails != null && posTransDetails.Any())
                    {
                        context.PosTrans.AddRange(posTransDetails);
                        Logger.Info("PosTrans Inserted: " + posTransDetails.Count().ToString());
                    }

                    if (transDetails != null && transDetails.Any())
                    {
                        context.Trans.AddRange(transDetails);
                        Logger.Info("Trans Inserted: " + transDetails.Count().ToString());
                    }

                    context.SaveChanges();
                    Logger.Info("Saved Changes");

                    dbContextTransaction.Commit();
                    Logger.Info("Commited Changes");
                    Logger.Info("-------DB Transaction - End-------");
                }
                catch (Exception ex)
                {
                    var exception = ex.InnerException == null ? "" : ex.InnerException.ToString();
                    Logger.Error("Exception:" + exception);
                    dbContextTransaction.Rollback();
                    Logger.Info("Changes Rolledback");
                    helper.MoveErrorFile(filePath, dbName);
                    emailHelper.SendMail(filePath, dbName, exception, EmailType.Error);
                }
            }
            #endregion

            return result;
        }

        private PosTran MapTransToPosTrans(Tran transDetail)
        {
            return new PosTran()
            {
                dat_TransDate = transDetail.dat_TransDate,
                id_Duty = transDetail.id_Duty,
                dat_TransTime = transDetail.dat_TransTime,
                id_Journey = transDetail.id_Journey,
                id_Module = transDetail.id_Module,
                id_SCTrans = transDetail.id_SCTrans,
                id_Stage = transDetail.id_Stage,
                int2_AlightingStageID = transDetail.int2_AlightingStageID,
                int2_AnnulCount = transDetail.int2_AnnulCount,
                int2_BoardingStageID = transDetail.int2_BoardingStageID,
                int2_Class = transDetail.int2_Class,
                int2_PassCount = transDetail.int2_PassCount,
                int2_TicketCount = transDetail.int2_TicketCount,
                int2_Transfers = transDetail.int2_Transfers,
                int4_AnnulCash = transDetail.int4_AnnulCash,
                int4_NonRevenue = transDetail.int4_NonRevenue,
                int4_Revenue = transDetail.int4_Revenue,
                int4_RevenueBal = transDetail.int4_RevenueBal,
                int4_TicketSerialNumber = transDetail.int4_TicketSerialNumber,
                int4_TripBal = transDetail.int4_TripBal,
                str_LocationCode = transDetail.str_LocationCode,
                str_SerialNumber = transDetail.str_SerialNumber
            };
        }

        private int GetNonRevenueFromPosTransTable(string serialNumber)
        {
            var nonRevenue = 0;
            var posTrans = context.PosTrans.Where(p => p.str_SerialNumber.Equals(serialNumber)).OrderByDescending(p => p.dat_TransTime).FirstOrDefault();
            if (posTrans != null && posTrans.int4_Revenue != null && posTrans.int4_TripBal != null)
                nonRevenue = (posTrans.int4_Revenue.Value / posTrans.int4_TripBal.Value);
            else
                nonRevenue = 5;
            return nonRevenue;
        }

        public bool ValidateFile(string filePath)
        {
            return !helper.IsFileLocked(filePath);
        }
    }


}
