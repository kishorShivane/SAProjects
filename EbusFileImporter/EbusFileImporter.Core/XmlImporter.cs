using EbusFileImporter.Core.Helpers;
using EbusFileImporter.Core.Interfaces;
using EbusFileImporter.DataProvider;
using EbusFileImporter.DataProvider.Models;
using EbusFileImporter.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace EbusFileImporter.Core
{
    public class XmlImporter : IImporter
    {
        public static ILogService Logger { get; set; }

        private Helper helper = null;
        private EmailHelper emailHelper = null;
        private DBService dbService = null;
        public static object thisLock = new object();
        public XmlImporter(ILogService logger)
        {
            Logger = logger;
            helper = new Helper(logger);
            emailHelper = new EmailHelper(logger);
            dbService = new DBService(logger);
        }

        public bool PostImportProcessing(string filePath)
        {
            bool result = false;

            return result;
        }

        public bool PreImportProcessing(string filePath)
        {
            bool result = false;
            if (ValidateFile(filePath))
            {
                result = true;
            }

            return result;
        }

        public bool ProcessFile(string filePath)
        {
            bool result = true;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            DateTime todayDate = DateTime.Now;
            int batch = Convert.ToInt32(todayDate.Day.ToString().PadLeft(2, '0') + todayDate.Month.ToString().PadLeft(2, '0') + todayDate.Year.ToString());
            string[] splitFilepath = filePath.Split('\\');
            string dbName = splitFilepath[splitFilepath.Length - 3];
            try
            {
                if (Constants.DetailedLogging)
                {
                    Logger.Info("***********************************************************");
                    Logger.Info("Started Import");
                    Logger.Info("***********************************************************");
                }


                XDocument xdoc = XDocument.Load(filePath);
                List<XElement> nodes = xdoc.Root.Elements().ToList();

                #region Check for file completeness
                IEnumerable<XElement> fileClosureNode = nodes.Where((x => x.Attribute("STXID").Value.Equals("82")));
                if (fileClosureNode == null || !fileClosureNode.Any())
                {
                    Logger.Info("No file closure found, Moving file to error folder");
                    helper.MoveErrorFile(filePath, dbName);
                    if (Constants.EnableEmailTrigger)
                    {
                        emailHelper.SendMail(filePath, dbName, "", EmailType.Error);
                    }

                    return false;
                }
                #endregion

                #region Check for validity of ExtendedWaybill date
                if (Constants.DetailedLogging)
                { Logger.Info("Check for validity of ExtendedWaybill date - Start"); }
                IEnumerable<XElement> extendedWaybill = nodes.Where((x => x.Attribute("STXID").Value.Equals("122")));
                XElement tempDutyDetail = nodes.Where((x => x.Attribute("STXID").Value.Equals("151"))).FirstOrDefault();
                if (tempDutyDetail != null && extendedWaybill != null && extendedWaybill.Any())
                {
                    bool foundDataProblem = false;
                    DateTime dutyDate = helper.ConvertToInsertDateTimeString(tempDutyDetail.Element("DutyDate").Value, tempDutyDetail.Element("DutyTime").Value);
                    List<XElement> tempList = extendedWaybill.ToList();
                    for (int i = 0; i <= tempList.Count() - 1; i++)
                    {
                        DateTime? tempNextStartDate = null;
                        DateTime startDateTime = helper.ConvertToInsertDateTimeString(tempList[i].Element("StartDate").Value, tempList[i].Element("StartTime").Value);
                        DateTime stopDateTime = helper.ConvertToInsertDateTimeString(tempList[i].Element("StopDate").Value, tempList[i].Element("StopTime").Value);
                        if ((i + 1) <= tempList.Count() - 1)
                        {
                            tempNextStartDate = helper.ConvertToInsertDateTimeString(tempList[i + 1].Element("StartDate").Value, tempList[i + 1].Element("StartTime").Value);
                        }

                        if (DateTime.Compare(startDateTime, stopDateTime) >= 0)
                        {
                            foundDataProblem = true;
                        }

                        if (i == 0 && DateTime.Compare(startDateTime, dutyDate) >= 0)
                        {
                            foundDataProblem = true;
                        }

                        if (tempNextStartDate != null && DateTime.Compare(tempNextStartDate.Value, stopDateTime) >= 0)
                        {
                            foundDataProblem = true;
                        }

                        if (foundDataProblem)
                        {
                            Logger.Info("Date Problem found, Moving file to error folder.");
                            helper.MoveDateProblemFile(filePath, dbName);
                            if (Constants.EnableEmailTrigger)
                            {
                                emailHelper.SendMail(filePath, dbName, "", EmailType.DateProblem);
                            }

                            return false;
                        }
                    }
                }
                if (Constants.DetailedLogging)
                { Logger.Info("Check for validity of ExtendedWaybill date - End"); }
                #endregion

                #region Initialize Variables
                int latestModuleID = 0;
                int latestDutyID = 0;
                int latestJourneyID = 0;
                int latestStageID = 0;
                int latestTransID = 0;
                int latestPosTransID = 0;
                int latestInspectorID = 0;
                int latestAuditFileStatus = 0;
                int latestdiagnosticRecord = 0;
                int latestBusChecklistID = 0;

                Module moduleDetail = null;
                List<Waybill> wayBillDetails = new List<Waybill>();
                Duty dutyDetail = null;
                AuditFileStatus auditFileDetail = null;
                List<AuditFileStatus> auditFileDetails = new List<AuditFileStatus>();
                DiagnosticRecord diagnosticRecord = null;
                List<DiagnosticRecord> diagnosticRecords = new List<DiagnosticRecord>();
                Journey journeyDetail = null;
                List<Journey> journeyDetails = new List<Journey>();
                List<Stage> stageDetails = new List<Stage>();
                Stage stageDetail = null;
                List<TempStage> tempStageDetails = new List<TempStage>();
                TempStage tempStage = null;
                Trans transDetail = null;
                GPSCoordinate gPSCoordinate = null;
                List<GPSCoordinate> gPSCoordinates = new List<GPSCoordinate>();
                List<Trans> transDetails = new List<Trans>();
                PosTrans posTransDetail = null;
                List<PosTrans> posTransDetails = new List<PosTrans>();
                Staff staffDetail = null;
                Inspector inspectorDetail = null;
                List<Inspector> inspectorDetails = new List<Inspector>();
                BusChecklist busChecklistDetail = null;
                List<BusNumberList> busNumberLists = new List<BusNumberList>();
                BusNumberList busNumberList = null;
                #endregion

                XAttribute way6OrTGXCheck = nodes.Where(x => x.Attribute("STXID").Value.Equals("18")).FirstOrDefault().Attribute("Position");

                if (way6OrTGXCheck != null)
                {
                    #region TGX file processing

                    #region Process Module Information

                    XElement node18 = nodes.Where(x => x.Attribute("STXID").Value.Equals("18")).FirstOrDefault();

                    if (node18 != null)
                    {

                        if (node18.Element("NotUsed1") != null && (int)node18.Element("NotUsed1") == 1)
                        {
                            return ProcessRetailPOSCashier(nodes, dbName, filePath);
                        }

                        moduleDetail = new Module
                        {
                            id_Module = latestModuleID,
                            str_LocationCode = null,
                            int4_ModuleID = (int)node18.Element("ModuleESN"),
                            int4_SignOnID = (int)node18.Element("DriverNumber"),
                            int4_OnReaderID = (int)node18.Element("HomeDepotID")
                        };
                        DateTime date = helper.ConvertToInsertDateString((string)node18.Element("SignOnDate"));
                        moduleDetail.dat_SignOnDate = date;
                        moduleDetail.dat_SignOnTime = helper.ConvertToInsertDateTimeString((string)node18.Element("SignOnDate"), (string)node18.Element("SignOnTime"));
                        moduleDetail.int4_OffReaderID = (int)node18.Element("HomeDepotID");
                        date = helper.ConvertToInsertDateString((string)node18.Element("SignOffDate"));
                        moduleDetail.dat_SignOffDate = date;
                        DateTime time = helper.ConvertToInsertDateTimeString((string)node18.Element("SignOffDate"), (string)node18.Element("SignOffTime"));
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
                        moduleDetail.id_BatchNo = batch;
                        moduleDetail.byt_IeType = null;
                        moduleDetail.byt_ModuleType = (int)node18.Element("ModuleType");
                    }
                    #endregion

                    #region Process Waybill Information

                    IEnumerable<XElement> nodes122 = nodes.Where(x => x.Attribute("STXID").Value.Equals("122"));
                    if (nodes122 != null && nodes122.Any())
                    {
                        nodes122.ToList().ForEach(x =>
                        {
                            XElement thisNode = x;
                            wayBillDetails.Add(new Waybill()
                            {
                                ModuleID = (int)thisNode.Element("ModuleNumber"),
                                id_Module = moduleDetail.id_Module,
                                dat_Start = helper.ConvertToInsertDateTimeString((string)thisNode.Element("StartDate"), (string)thisNode.Element("StartTime")),
                                dat_End = helper.ConvertToInsertDateTimeString((string)thisNode.Element("StopDate"), (string)thisNode.Element("StopTime")),
                                int4_Operator = (int)thisNode.Element("OperatorNumber"),
                                str8_BusID = (string)thisNode.Element("BusNumber"),
                                str6_EtmID = (string)thisNode.Element("ETMNumber"),
                                int4_EtmGrandTotal = (int)thisNode.Element("ETMTotal"),
                                int4_Revenue = (int)thisNode.Element("DutyRevenue"),
                                dat_Match = null,
                                dat_Actual = null,
                                Imported_Operator = null,
                                Imported_BusID = null,
                                Imported_ETMID = null,
                                Imported_GT = null,
                                Imported_Revenue = null

                            });
                        });
                    }
                    #endregion

                    #region Process Duty Information   

                    XElement node151 = nodes.Where(x => x.Attribute("STXID").Value.Equals("151")).FirstOrDefault();
                    XElement node122 = nodes.Where(x => x.Attribute("STXID").Value.Equals("122")).FirstOrDefault();
                    XElement node154 = nodes.Where(x => x.Attribute("STXID").Value.Equals("154")).FirstOrDefault();
                    XElement node155 = nodes.Where(x => x.Attribute("STXID").Value.Equals("155")).FirstOrDefault();

                    if (node151 != null && node154 != null && node155 != null)
                    {
                        //if ((int)node151.Element("DriverNumber") == 0)
                        //{
                        //    Logger.Info("Error: Driver Number is 0 in node with Position value: " + node151.Attribute("Position").Value);
                        //    helper.MoveErrorFile(filePath, dbName);
                        //    return false;
                        //}

                        dutyDetail = new Duty
                        {
                            id_Duty = latestDutyID,
                            id_Module = moduleDetail.id_Module,
                            int4_DutyID = (int)node151.Element("DutyNo"),
                            int4_OperatorID = (int)node151.Element("DriverNumber"),
                            str_ETMID = (string)node122?.Element("ETMNumber"),
                            int4_GTValue = (int)node151.Element("ETMCashTotal"),
                            int4_NextTicketNumber = (int)node151.Element("NextTicketNo"),
                            int4_DutySeqNum = (int)node151.Element("DutySeqNo.")
                        };
                        DateTime date = helper.ConvertToInsertDateString((string)node151.Element("DutyDate"));
                        dutyDetail.dat_DutyStartDate = date;
                        dutyDetail.dat_DutyStartTime = helper.ConvertToInsertDateTimeStringWithOutSeconds((string)node151.Element("DutyDate"), (string)node151.Element("DutyTime"));
                        dutyDetail.dat_DutyStopDate = helper.ConvertToInsertDateString((string)node154.Element("SignOffDate"));
                        dutyDetail.dat_DutyStopTime = helper.ConvertToInsertDateTimeStringWithOutSeconds((string)node154.Element("SignOffDate"), (string)node154.Element("SignOffTime"));
                        dutyDetail.dat_TrafficDate = date;
                        dutyDetail.str_BusID = node151.Element("FleetID").Value.TrimStart('0') == "" ? "0" : node151.Element("FleetID").Value.TrimStart('0');
                        dutyDetail.int4_DutyRevenue = (int)node154.Element("DutyCashTotal");
                        dutyDetail.int4_DutyTickets = (int)node154.Element("DutyTicketTotal");
                        dutyDetail.int4_DutyPasses = 0;
                        dutyDetail.int4_DutyNonRevenue = 0;
                        dutyDetail.int4_DutyTransfer = 0;
                        dutyDetail.str_FirstRouteID = (string)node155.Element("RouteVariantNo");
                        dutyDetail.int2_FirstJourneyID = (short)node155.Element("JourneyNo");
                        dutyDetail.dat_RecordMod = todayDate;
                        dutyDetail.id_BatchNo = batch;
                        dutyDetail.byt_IeType = null;
                        dutyDetail.str_EpromVersion = (string)node18.Element("ETMptr"); ;
                        dutyDetail.str_OperatorVersion = (string)node151.Element("RunningBoard");
                        dutyDetail.str_SpecialVersion = (string)node18.Element("SFptr");
                        dutyDetail.int4_DutyAnnulCash = null;
                        dutyDetail.int4_DutyAnnulCount = null;
                    }

                    if (dbService.CheckForDutyDuplicates(dutyDetail.int4_OperatorID.Value, dutyDetail.dat_DutyStartTime.Value, dutyDetail.dat_DutyStopTime.Value, dbName))
                    {
                        Logger.Info("Error: Duplicate file found");
                        helper.MoveDuplicateFile(filePath, dbName);
                        return false;
                    }

                    if (!dbService.DoesRecordExist("BusNumberList", "Bus_ID", dutyDetail.str_BusID, dbName))
                    {
                        busNumberList = new BusNumberList
                        {
                            Bus_ID = Convert.ToInt32(dutyDetail.str_BusID),
                            ETMType = dutyDetail.int4_DutyID == 8000 ? "Seller" : "Driver",
                            int4_ReasonID = 6
                        };
                        busNumberLists.Add(busNumberList);
                    }

                    #endregion

                    #region Process AuditFileStatus Information

                    IEnumerable<XElement> nodes125 = nodes.Where(x => x.Attribute("STXID").Value.Equals("125"));
                    if (nodes125 != null && nodes125.Count() == 2)
                    {
                        auditFileDetail = new AuditFileStatus();

                        XElement thisNode = nodes125.ToList()[0];
                        XElement nextnode = nodes125.ToList()[1];
                        auditFileDetail.Id_Status = latestAuditFileStatus;
                        auditFileDetail.id_duty = dutyDetail.id_Duty;
                        auditFileDetail.DriverAuditStatus1 = (int)thisNode.Element("AuditStatus");
                        auditFileDetail.DriverNumber1 = (int)thisNode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber1 = (string)thisNode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus1DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)thisNode.Element("Time"));
                        auditFileDetail.DriverAuditStatus2 = (int)nextnode.Element("AuditStatus");
                        auditFileDetail.DriverNumber2 = (int)nextnode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber2 = (string)nextnode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus2DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)nextnode.Element("Time"));
                        auditFileDetail.DutySignOffMode = (int)node154.Element("SignOffMode");
                        auditFileDetail.RecordModified = todayDate;
                        auditFileDetail.AuditFileName = Path.GetFileName(filePath);
                        auditFileDetails.Add(auditFileDetail);
                    }
                    else if (nodes125.Count() == 1)
                    {
                        auditFileDetail = new AuditFileStatus();
                        string lastEndOfJourneyTime = nodes.Where(x => x.Attribute("STXID").Value.Equals("156")).LastOrDefault() == null ? "000000" : nodes.Where(x => x.Attribute("STXID").Value.Equals("156")).LastOrDefault().Element("JourneyStopTime").Value;
                        XElement thisNode = nodes125.ToList()[0];
                        XElement nextnode = nodes125.ToList()[0];
                        auditFileDetail.Id_Status = latestAuditFileStatus;
                        auditFileDetail.id_duty = dutyDetail.id_Duty;
                        auditFileDetail.DriverAuditStatus1 = (int)thisNode.Element("AuditStatus");
                        auditFileDetail.DriverNumber1 = (int)thisNode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber1 = (string)thisNode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus1DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)thisNode.Element("Time"));
                        auditFileDetail.DriverAuditStatus2 = 02;
                        auditFileDetail.DriverNumber2 = (int)nextnode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber2 = (string)nextnode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus2DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), lastEndOfJourneyTime);
                        auditFileDetail.DutySignOffMode = (int)node154.Element("SignOffMode");
                        auditFileDetail.RecordModified = todayDate;
                        auditFileDetail.AuditFileName = Path.GetFileName(filePath);
                        auditFileDetails.Add(auditFileDetail);
                        Logger.Info("AuditFileStatus Correction: No AuditFileStatus node found at the end, data is auto corrected");
                    }
                    else
                    {
                        Logger.Info("Error: No AuditFileStatus node found, Moving file to error folder");
                        helper.MoveErrorFile(filePath, dbName);
                        if (Constants.EnableEmailTrigger)
                        {
                            emailHelper.SendMail(filePath, dbName, "", EmailType.Error);
                        }

                        return false;
                    }

                    #endregion

                    #region Process Diagnostic Record

                    IEnumerable<XElement> nodes50 = nodes.Where(x => x.Attribute("STXID").Value.Equals("50"));
                    if (nodes50 != null && nodes50.Any())
                    {
                        nodes50.ToList().ForEach(x =>
                        {
                            diagnosticRecord = new DiagnosticRecord();
                            XElement thisRecord = x;
                            diagnosticRecord.Id_DiagnosticRecord = latestdiagnosticRecord;
                            diagnosticRecord.Id_Status = latestAuditFileStatus;
                            diagnosticRecord.TSN = (string)thisRecord.Element("TSN");
                            diagnosticRecord.EquipmentType = (string)thisRecord.Element("EquipmentType");
                            diagnosticRecord.DiagCode = (string)thisRecord.Element("DiagCode");
                            diagnosticRecord.DiagInfo = (string)thisRecord.Element("DiagInfo");
                            diagnosticRecord.Time = helper.ConvertToInsertTimeString((string)thisRecord.Element("Time"));
                            diagnosticRecords.Add(diagnosticRecord);
                            latestdiagnosticRecord++;
                        });
                    }
                    #endregion

                    #region Process Journey Information

                    IEnumerable<XElement> nodes156 = nodes.Where(x => x.Attribute("STXID").Value.Equals("156"));
                    IEnumerable<XElement> nodes155 = nodes.Where(x => x.Attribute("STXID").Value.Equals("155"));

                    if (nodes156 != null && nodes155 != null)
                    {
                        foreach (XElement item in nodes155)
                        {
                            XElement startNode155 = item;
                            XElement endNode156 = nodes156.Where(i => Convert.ToInt32(i.Attribute("Position").Value) > Convert.ToInt32(item.Attribute("Position").Value)).OrderBy(i => Convert.ToInt32(i.Attribute("Position").Value)).FirstOrDefault();

                            if (endNode156 == null)
                            {
                                Logger.Info("Error: No end of journey node found for journey node with position-" + item.Attribute("Position").Value + ", Moving file to error folder");
                                helper.MoveErrorFile(filePath, dbName);
                                if (Constants.EnableEmailTrigger)
                                {
                                    emailHelper.SendMail(filePath, dbName, "", EmailType.Error);
                                }
                                return false;
                            }

                            List<XElement> eachJourneyNodes = nodes.Where(i => Convert.ToInt32(i.Attribute("Position").Value) > Convert.ToInt32(startNode155.Attribute("Position").Value) && Convert.ToInt32(i.Attribute("Position").Value) < Convert.ToInt32(endNode156.Attribute("Position").Value)).ToList();

                            journeyDetail = new Journey
                            {
                                id_Journey = latestJourneyID,
                                id_Duty = dutyDetail.id_Duty,
                                id_Module = moduleDetail.id_Module,
                                str_RouteID = (string)startNode155.Element("RouteVariantNo"),
                                int2_JourneyID = (short)startNode155.Element("JourneyNo"),
                                int2_Direction = (short)startNode155.Element("Direction")
                            };
                            DateTime date = helper.ConvertToInsertDateString((string)startNode155.Element("StartDate"));
                            journeyDetail.dat_JourneyStartDate = date;
                            journeyDetail.dat_JourneyStartTime = helper.ConvertToInsertDateTimeString((string)startNode155.Element("StartDate"), (string)startNode155.Element("StartTime"));
                            DateTime jourDate = helper.ConvertToInsertDateString((string)endNode156.Element("JourneyStopDate"));
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
                            journeyDetail.id_BatchNo = batch;
                            journeyDetail.byt_IeType = null;
                            journeyDetail.dat_JourneyMoveTime = null;
                            journeyDetail.dat_JourneyArrivalTime = null;
                            journeyDetail.int4_GPSDistance = null;
                            journeyDetails.Add(journeyDetail);

                            #region Process Stage Information

                            IEnumerable<XElement> nodes113 = eachJourneyNodes.Where(i => i.Attribute("STXID").Value.Equals("113"));

                            if (nodes113 != null)
                            {
                                List<XElement> listNodes113 = nodes113.ToList();
                                int count = listNodes113.Count();
                                for (int i = 0; i < count; i++)
                                {
                                    XElement thisNode113 = listNodes113[i];
                                    stageDetail = new Stage();
                                    tempStage = new TempStage();
                                    stageDetail.id_Stage = latestStageID;
                                    stageDetail.id_Journey = journeyDetail.id_Journey;
                                    stageDetail.id_Duty = dutyDetail.id_Duty;
                                    stageDetail.id_Module = moduleDetail.id_Module;
                                    stageDetail.int2_StageID = (short)thisNode113.Element("BoardingStage");
                                    date = helper.ConvertToInsertDateString((string)startNode155.Element("StartDate"));
                                    stageDetail.dat_StageDate = date;
                                    stageDetail.dat_StageTime = helper.ConvertToInsertDateTimeString((string)startNode155.Element("StartDate"), (string)thisNode113.Element("Time"));
                                    stageDetail.dat_RecordMod = todayDate;
                                    stageDetail.id_BatchNo = batch;
                                    stageDetails.Add(stageDetail);

                                    tempStage.id_Stage = latestStageID;
                                    tempStage.TSN = (string)thisNode113.Element("TSN");
                                    tempStage.RecordedTime = (string)thisNode113.Element("Time");
                                    tempStageDetails.Add(tempStage);

                                    latestStageID++;

                                    #region Process Trans Information
                                    int nextPosition = 0;
                                    if ((i + 1) < count)
                                    {
                                        nextPosition = Convert.ToInt32(listNodes113[i + 1].Attribute("Position").Value);
                                    }
                                    else
                                    {
                                        nextPosition = Convert.ToInt32(endNode156.Attribute("Position").Value);
                                    }

                                    List<XElement> eachStageNodes = eachJourneyNodes.Where(j => Convert.ToInt32(j.Attribute("Position").Value) > Convert.ToInt32(thisNode113.Attribute("Position").Value) && Convert.ToInt32(j.Attribute("Position").Value) < nextPosition).ToList();

                                    IEnumerable<XElement> cashTransNodes = eachStageNodes.Where(j => j.Attribute("STXID").Value.Equals("157"));
                                    IEnumerable<XElement> smartCardTransNodes = eachStageNodes.Where(j => j.Attribute("STXID").Value.Equals("188"));

                                    if ((cashTransNodes != null && cashTransNodes.Any()) || (smartCardTransNodes != null && smartCardTransNodes.Any()))
                                    {
                                        cashTransNodes.ToList().ForEach(t =>
                                        {
                                            XElement thisTrans = t;
                                            transDetail = new Trans();
                                            string ticketType = t.Element("TicketType").Value.Trim();
                                            transDetail.id_Trans = latestTransID;
                                            transDetail.id_Stage = stageDetail.id_Stage;
                                            transDetail.id_Journey = journeyDetail.id_Journey;
                                            transDetail.id_Duty = dutyDetail.id_Duty;
                                            transDetail.id_Module = moduleDetail.id_Module;
                                            transDetail.str_LocationCode = dutyDetail.str_OperatorVersion.TrimStart('0').Length >= 2 ? dutyDetail.str_OperatorVersion.TrimStart('0').Substring(0, 2) : dutyDetail.str_OperatorVersion.TrimStart('0'); ;
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
                                            transDetail.str_SerialNumber = "";
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = 0;
                                            transDetail.int2_AnnulCount = null;
                                            transDetail.int4_AnnulCash = null;
                                            //transDetail.id_SCTrans = null;
                                            transDetail.int4_TicketSerialNumber = (int)thisTrans.Element("TicketSerialNo");
                                            transDetails.Add(transDetail);
                                            latestTransID++;
                                        });

                                        smartCardTransNodes.ToList().ForEach(t =>
                                        {
                                            XElement thisTrans = t;
                                            transDetail = new Trans();
                                            string ticketType = t.Element("TicketType").Value.Trim();
                                            string productData = (string)thisTrans.Element("Product1Data");
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
                                            string serialNumber = (string)thisTrans.Element("ESN");
                                            transDetail.str_SerialNumber = serialNumber;
                                            //transDetail.int4_TripBal = Helper.GetTripBalanceFromProductData(productData);
                                            transDetail.int2_AnnulCount = null;
                                            transDetail.int4_AnnulCash = null;
                                            //transDetail.id_SCTrans = null;
                                            transDetail.int4_TicketSerialNumber = (int)thisTrans.Element("TicketSerialNo");

                                            #region GPS CO-Ordinates
                                            if ((t.Element("Latitude") != null && t.Element("Longitude") != null) && (t.Element("Latitude").Value.Trim() != string.Empty && t.Element("Longitude").Value.Trim() != string.Empty))
                                            {
                                                string latitude = t.Element("Latitude").Value.Trim();
                                                string longitude = t.Element("Longitude").Value.Trim();
                                                if (latitude.Length > 9 || longitude.Length > 9)
                                                {
                                                    latitude = latitude.TrimStart('0');
                                                    longitude = longitude.TrimStart('0');
                                                }
                                                gPSCoordinate = new GPSCoordinate
                                                {
                                                    id_Trans = latestTransID,
                                                    id_Stage = stageDetail.id_Stage,
                                                    id_Journey = journeyDetail.id_Journey,
                                                    id_Duty = dutyDetail.id_Duty,
                                                    id_Module = moduleDetail.id_Module,
                                                    Latitude = latitude,
                                                    LatDegree = Convert.ToInt32(latitude.Substring(0, 2)),
                                                    LatMinutes = Convert.ToInt32(latitude.Substring(2, 2)),
                                                    LatSeconds = (Convert.ToDecimal(latitude.Substring(5, 3)) / 1000) * 60,
                                                    LatDir = latitude.Substring(8, 1),
                                                    Longitude = longitude,
                                                    LongDegree = Convert.ToInt32(longitude.Substring(0, 2)),
                                                    LongMinutes = Convert.ToInt32(longitude.Substring(2, 2)),
                                                    LongSeconds = (Convert.ToDecimal(longitude.Substring(5, 3)) / 1000) * 60,
                                                    LongDir = longitude.Substring(8, 1)
                                                };
                                                gPSCoordinates.Add(gPSCoordinate);
                                            }
                                            #endregion

                                            #region Process Ticket Type 
                                            int nonRevenue = 0;
                                            switch (ticketType)
                                            {
                                                case "2328":
                                                    //AdulT MJ Cancelation
                                                    transDetail.int2_Class = 995;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    transDetail.SmartCardExipry = helper.GetSmartCardExipryFromProductDate(productData);
                                                    if (!helper.IsTransferTransaction(productData))
                                                    {
                                                        //Adult MJ Transfer
                                                        nonRevenue = dbService.GetNonRevenueFromPosTransTable(serialNumber, dbName);
                                                        transDetail.int2_Class = 999;
                                                        transDetail.int2_PassCount = 1;
                                                        transDetail.int2_Transfers = 0;
                                                    }
                                                    transDetail.int4_NonRevenue = nonRevenue;

                                                    break;
                                                case "2329":
                                                    //Scholar MJ Transfer
                                                    transDetail.int2_Class = 996;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    transDetail.SmartCardExipry = helper.GetSmartCardExipryFromProductDate(productData);
                                                    if (!helper.IsTransferTransaction(productData))
                                                    {
                                                        //Scholar MJ Transfer
                                                        nonRevenue = dbService.GetNonRevenueFromPosTransTable(serialNumber, dbName);
                                                        transDetail.int2_Class = 997;
                                                        transDetail.int2_PassCount = 1;
                                                        transDetail.int2_Transfers = 0;
                                                    }
                                                    transDetail.int4_NonRevenue = nonRevenue;
                                                    break;
                                                case "232B":
                                                    //Disabled MJ Cancelation
                                                    transDetail.int2_Class = 994;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    transDetail.SmartCardExipry = helper.GetSmartCardExipryFromProductDate(productData);
                                                    if (!helper.IsTransferTransaction(productData))
                                                    {
                                                        //Disabled MJ Transfer
                                                        nonRevenue = dbService.GetNonRevenueFromPosTransTable(serialNumber, dbName);
                                                        transDetail.int2_Class = 993;
                                                        transDetail.int2_PassCount = 1;
                                                        transDetail.int2_Transfers = 0;
                                                    }
                                                    transDetail.int4_NonRevenue = nonRevenue;
                                                    break;
                                                case "03F9":
                                                    //Stored Value Adult Cancelation
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
                                                    //Stored Value Pensioner Cancelation
                                                    transDetail.int2_Class = 703;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "0409":
                                                    //Stored Value Child Cancelation
                                                    transDetail.int2_Class = 702;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "040A":
                                                    //Stored Value Scholar Cancelation
                                                    transDetail.int2_Class = 704;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "0429":
                                                    //Stored Value Parcel 1
                                                    transDetail.int2_Class = 705;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "042A":
                                                    //Stored Value Parcel 2
                                                    transDetail.int2_Class = 706;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "042B":
                                                    //Stored Value Parcel 3
                                                    transDetail.int2_Class = 707;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "042F":
                                                    //SV Penalty 1
                                                    transDetail.int2_Class = 708;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "0430":
                                                    //SV Penalty 2
                                                    transDetail.int2_Class = 709;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "2715":
                                                    //Adult MJ 10 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2718":
                                                    //Adult MJ 12 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2727":
                                                    //Adult MJ 14 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "271B":
                                                    //Adult MJ 40 Recharge
                                                    transDetail.int2_Class = 714;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2722":
                                                    //Adult MJ 44 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "271E":
                                                    //Adult MJ 48 Recharge
                                                    transDetail.int2_Class = 716;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2723":
                                                    //Adult MJ 52 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2B18":
                                                    //Adult MJ 48 Recharge
                                                    transDetail.int2_Class = 718;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2B19":
                                                    //Adult MJ 48 Recharge
                                                    transDetail.int2_Class = 719;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2716":
                                                    //Scholar MJ 10 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2717":
                                                    //Scholar MJ 10 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "271C":
                                                    //Scholar MJ 44 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2729":
                                                    //SV 5 Recharge
                                                    transDetail.int2_Class = 752;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2726":
                                                    //SV 10 Recharge
                                                    transDetail.int2_Class = 741;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2728":
                                                    //SV 20 Recharge
                                                    transDetail.int2_Class = 742;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2714":
                                                    //SV 50 Recharge
                                                    transDetail.int2_Class = 743;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "272A":
                                                    //SV 65 Recharge
                                                    transDetail.int2_Class = 753;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2721":
                                                    //SV 80 Recharge
                                                    transDetail.int2_Class = 748;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2710":
                                                    //SV 100 Recharge
                                                    transDetail.int2_Class = 744;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "271F":
                                                    //SV 150 Recharge
                                                    transDetail.int2_Class = 744;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2711":
                                                    //SV 200 Recharge
                                                    transDetail.int2_Class = 745;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2712":
                                                    //SV 300 Recharge
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
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2713":
                                                    //SV 400 Recharge
                                                    transDetail.int2_Class = 747;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2720":
                                                    //SV 500 Recharge
                                                    transDetail.int2_Class = 750;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2AF9":
                                                    //Adult MJ Deposit
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
                                                    //Scholar MJ Deposit
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
                                                case "2AFE":
                                                    transDetail.int2_Class = 733;
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
                                                case "2B17":
                                                    transDetail.int2_Class = 751;
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
                                                posTransDetail = null;
                                            }
                                            if (transDetail != null && ticketType.Trim() != "2AF8")
                                            {
                                                transDetails.Add(transDetail);
                                                latestTransID++;
                                                transDetail = null;
                                            }
                                        });
                                    }
                                    #endregion
                                }
                            }
                            #endregion

                            latestJourneyID++;
                        }
                    }
                    #endregion

                    #region Process Staff Information

                    if (!dbService.DoesRecordExist("Staff", "int4_StaffID", dutyDetail.int4_OperatorID.Value, dbName))
                    {

                        XElement node125 = nodes.Where(x => x.Attribute("STXID").Value.Equals("125")).FirstOrDefault();
                        if (node151 != null && node125 != null)
                        {
                            staffDetail = new Staff
                            {
                                int4_StaffID = dutyDetail.int4_OperatorID.Value
                            };
                            staffDetail.str50_StaffName = "New Staff" + " - " + staffDetail.int4_StaffID;
                            staffDetail.bit_InUse = true;
                            staffDetail.int4_StaffTypeID = 1;
                            staffDetail.int4_StaffSubTypeID = 0;
                            staffDetail.str4_LocationCode = "0001";
                            staffDetail.str2_LocationCode = null;
                            string serialNumber = (string)node125.Element("DriverCardSerialNo");
                            staffDetail.SerialNumber = Convert.ToInt32(serialNumber, 16).ToString();
                            staffDetail.dat_RecordMod = DateTime.Now;
                        }
                    }
                    #endregion

                    #region Process Inspector Information

                    IEnumerable<XElement> nodes34 = nodes.Where(x => x.Attribute("STXID").Value.Equals("34"));
                    if (nodes34 != null)
                    {
                        nodes34.ToList().ForEach(x =>
                        {
                            XElement thisInspector = x;
                            XElement inspectorStage = nodes.Where(i => i.Attribute("STXID").Value.Equals("113") && Convert.ToInt32(i.Attribute("Position").Value) < Convert.ToInt32(thisInspector.Attribute("Position").Value)).OrderByDescending(i => Convert.ToInt32(i.Attribute("Position").Value)).FirstOrDefault();
                            int tempStageID = tempStageDetails.Where(i => i.TSN.Equals(inspectorStage.Element("TSN").Value) && i.RecordedTime.Equals(inspectorStage.Element("Time").Value)).FirstOrDefault().id_Stage;
                            inspectorDetail = new Inspector
                            {
                                id_Inspector = latestInspectorID,
                                id_Stage = tempStageID,
                                id_InspectorID = (int)thisInspector.Element("InspectorNo")
                            };
                            DateTime date = helper.ConvertToInsertDateString((string)node151.Element("DutyDate"));
                            inspectorDetail.datTimeStamp = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)thisInspector.Element("Time"));
                            inspectorDetails.Add(inspectorDetail);
                            latestInspectorID++;
                        });

                    }
                    #endregion

                    #endregion
                }
                else
                {
                    #region Way6 file processing

                    #region Process Module Information
                    XElement node18 = nodes.Where(x => x.Attribute("STXID").Value.Equals("18")).FirstOrDefault();
                    if (node18 != null)
                    {
                        moduleDetail = new Module
                        {
                            id_Module = latestModuleID,
                            str_LocationCode = null,
                            int4_ModuleID = (int)node18.Element("ModuleESN"),
                            int4_SignOnID = (int)node18.Element("DriverNumber1"),
                            int4_OnReaderID = (int)node18.Element("HomeDepotID")
                        };
                        DateTime date = helper.ConvertToInsertDateString((string)node18.Element("SignOnDate"));
                        moduleDetail.dat_SignOnDate = date;
                        moduleDetail.dat_SignOnTime = helper.ConvertToInsertDateTimeString((string)node18.Element("SignOnDate"), (string)node18.Element("SignOnTime"));
                        moduleDetail.int4_OffReaderID = (int)node18.Element("HomeDepotID");
                        date = helper.ConvertToInsertDateString((string)node18.Element("SignOffDate"));
                        moduleDetail.dat_SignOffDate = date;
                        DateTime time = helper.ConvertToInsertDateTimeString((string)node18.Element("SignOffDate"), (string)node18.Element("SignOffTime"));
                        moduleDetail.dat_SignOffTime = time;
                        moduleDetail.dat_TrafficDate = date;
                        moduleDetail.dat_ModuleOutDate = date;
                        moduleDetail.dat_ModuleOutTime = time;
                        moduleDetail.int4_HdrModuleRevenue = (int)node18.Element("TotalNetCashPaid");
                        moduleDetail.int4_HdrModuleTickets = (int)node18.Element("TotalNetCashPassengers");
                        moduleDetail.int4_HdrModulePasses = null;//(int)node18.Element("DriverNumber1");
                        moduleDetail.int4_ModuleRevenue = (int)node18.Element("TotalNetCashPaid");
                        moduleDetail.int4_ModuleTickets = (int)node18.Element("TotalNetCashPassengers");
                        moduleDetail.int4_ModulePasses = 0;
                        moduleDetail.int4_ModuleNonRevenue = 0;
                        moduleDetail.int4_ModuleTransfer = 0;
                        moduleDetail.dat_ImportStamp = todayDate.Date;
                        moduleDetail.dat_RecordMod = todayDate;
                        moduleDetail.int4_ImportModuleKey = null;
                        moduleDetail.id_BatchNo = batch;
                        moduleDetail.byt_IeType = null;
                        moduleDetail.byt_ModuleType = 0;
                    }
                    #endregion

                    #region Process Waybill Information
                    IEnumerable<XElement> nodes122 = nodes.Where(x => x.Attribute("STXID").Value.Equals("122"));
                    if (nodes122 != null && nodes122.Any())
                    {
                        nodes122.ToList().ForEach(x =>
                        {
                            XElement thisNode = x;
                            wayBillDetails.Add(new Waybill()
                            {
                                ModuleID = (int)thisNode.Element("OperatorNumber"),
                                id_Module = moduleDetail.id_Module,
                                dat_Start = helper.ConvertToInsertDateTimeString((string)thisNode.Element("StartDate"), (string)thisNode.Element("StartTime")),
                                dat_End = helper.ConvertToInsertDateTimeString((string)thisNode.Element("StopDate"), (string)thisNode.Element("StopTime")),
                                int4_Operator = (int)thisNode.Element("OperatorNumber"),
                                str8_BusID = (string)thisNode.Element("BusNumber"),
                                str6_EtmID = (string)thisNode.Element("ETMNumber"),
                                int4_EtmGrandTotal = (int)thisNode.Element("ETMTotal"),
                                int4_Revenue = (int)thisNode.Element("DutyRevenue"),
                                dat_Match = null,
                                dat_Actual = null,
                                Imported_Operator = null,
                                Imported_BusID = null,
                                Imported_ETMID = null,
                                Imported_GT = null,
                                Imported_Revenue = null

                            });
                        });
                    }
                    #endregion

                    #region Process Duty Information   
                    XElement node151 = nodes.Where(x => x.Attribute("STXID").Value.Equals("151")).FirstOrDefault();
                    XElement node122 = nodes.Where(x => x.Attribute("STXID").Value.Equals("122")).FirstOrDefault();
                    XElement node154 = nodes.Where(x => x.Attribute("STXID").Value.Equals("154")).FirstOrDefault();
                    XElement node155 = nodes.Where(x => x.Attribute("STXID").Value.Equals("155")).FirstOrDefault();

                    if (node151 != null && node154 != null && node155 != null)
                    {
                        dutyDetail = new Duty
                        {
                            id_Duty = latestDutyID,
                            id_Module = moduleDetail.id_Module,
                            int4_DutyID = (int)node151.Element("DutyNo"),
                            int4_OperatorID = (int)node151.Element("DriverNumber"),
                            str_ETMID = (string)node122?.Element("ETMNumber"),
                            int4_GTValue = (int)node151.Element("ETMCashTotal"),
                            int4_NextTicketNumber = (int)node151.Element("NextTicketNo"),
                            int4_DutySeqNum = (int)node151.Element("DutySeqNo.")
                        };
                        DateTime date = helper.ConvertToInsertDateString((string)node151.Element("DutyDate"));
                        dutyDetail.dat_DutyStartDate = date;
                        dutyDetail.dat_DutyStartTime = helper.ConvertToInsertDateTimeStringWithOutSeconds((string)node151.Element("DutyDate"), (string)node151.Element("DutyTime"));
                        dutyDetail.dat_DutyStopDate = helper.ConvertToInsertDateString((string)node154.Element("SignOffDate"));
                        dutyDetail.dat_DutyStopTime = helper.ConvertToInsertDateTimeStringWithOutSeconds((string)node154.Element("SignOffDate"), (string)node154.Element("SignOffTime"));
                        dutyDetail.dat_TrafficDate = date;
                        dutyDetail.str_BusID = node151.Element("FleetID").Value.TrimStart('0') == "" ? "0" : node151.Element("FleetID").Value.TrimStart('0');
                        dutyDetail.int4_DutyRevenue = (int)node154.Element("DutyCashTotal");
                        dutyDetail.int4_DutyTickets = (int)node154.Element("DutyTicketTotal");
                        dutyDetail.int4_DutyPasses = 0;
                        dutyDetail.int4_DutyNonRevenue = 0;
                        dutyDetail.int4_DutyTransfer = 0;
                        dutyDetail.str_FirstRouteID = (string)node155.Element("RouteVariantNo");
                        dutyDetail.int2_FirstJourneyID = (short)node155.Element("JourneyNo");
                        dutyDetail.dat_RecordMod = todayDate;
                        dutyDetail.id_BatchNo = batch;
                        dutyDetail.byt_IeType = null;
                        dutyDetail.str_EpromVersion = (string)node18.Element("ETMptr"); ;
                        dutyDetail.str_OperatorVersion = (string)node151.Element("RunningBoardAlpha");
                        dutyDetail.str_SpecialVersion = (string)node18.Element("SFptr");
                        dutyDetail.int4_DutyAnnulCash = null;
                        dutyDetail.int4_DutyAnnulCount = null;
                    }

                    if (dbService.CheckForDutyDuplicates(dutyDetail.int4_OperatorID.Value, dutyDetail.dat_DutyStartTime.Value, dutyDetail.dat_DutyStopTime.Value, dbName))
                    {
                        Logger.Info("Error: Duplicate file found");
                        helper.MoveDuplicateFile(filePath, dbName);
                        return false;
                    }

                    if (!dbService.DoesRecordExist("BusNumberList", "Bus_ID", dutyDetail.str_BusID, dbName))
                    {
                        busNumberList = new BusNumberList
                        {
                            Bus_ID = Convert.ToInt32(dutyDetail.str_BusID),
                            ETMType = dutyDetail.int4_DutyID == 8000 ? "Seller" : "Driver",
                            int4_ReasonID = 6
                        };
                        busNumberLists.Add(busNumberList);
                    }

                    #endregion

                    #region Process AuditFileStatus Information
                    IEnumerable<XElement> nodes125 = nodes.Where(x => x.Attribute("STXID").Value.Equals("125"));
                    if (nodes125 != null && nodes125.Count() == 2)
                    {
                        auditFileDetail = new AuditFileStatus();

                        XElement thisNode = nodes125.ToList()[0];
                        XElement nextnode = nodes125.ToList()[1];
                        auditFileDetail.Id_Status = latestAuditFileStatus;
                        auditFileDetail.id_duty = dutyDetail.id_Duty;
                        auditFileDetail.DriverAuditStatus1 = (int)thisNode.Element("AuditStatus");
                        auditFileDetail.DriverNumber1 = (int)thisNode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber1 = (string)thisNode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus1DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)thisNode.Element("Time"));
                        auditFileDetail.DriverAuditStatus2 = (int)nextnode.Element("AuditStatus");
                        auditFileDetail.DriverNumber2 = (int)nextnode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber2 = (string)nextnode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus2DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)nextnode.Element("Time"));
                        auditFileDetail.DutySignOffMode = (int)node154.Element("SignOffMode");
                        auditFileDetail.RecordModified = todayDate;
                        auditFileDetail.AuditFileName = Path.GetFileName(filePath);
                        auditFileDetails.Add(auditFileDetail);
                    }
                    else if (nodes125.Count() == 1)
                    {
                        auditFileDetail = new AuditFileStatus();
                        string lastEndOfJourneyTime = nodes.Where(x => x.Attribute("STXID").Value.Equals("156")).LastOrDefault() == null ? "000000" : nodes.Where(x => x.Attribute("STXID").Value.Equals("156")).LastOrDefault().Element("JourneyStopTime").Value;
                        XElement thisNode = nodes125.ToList()[0];
                        XElement nextnode = nodes125.ToList()[0];
                        auditFileDetail.Id_Status = latestAuditFileStatus;
                        auditFileDetail.id_duty = dutyDetail.id_Duty;
                        auditFileDetail.DriverAuditStatus1 = (int)thisNode.Element("AuditStatus");
                        auditFileDetail.DriverNumber1 = (int)thisNode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber1 = (string)thisNode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus1DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)thisNode.Element("Time"));
                        auditFileDetail.DriverAuditStatus2 = 02;
                        auditFileDetail.DriverNumber2 = (int)nextnode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber2 = (string)nextnode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus2DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), lastEndOfJourneyTime);
                        auditFileDetail.DutySignOffMode = (int)node154.Element("SignOffMode");
                        auditFileDetail.RecordModified = todayDate;
                        auditFileDetail.AuditFileName = Path.GetFileName(filePath);
                        auditFileDetails.Add(auditFileDetail);
                        Logger.Info("AuditFileStatus Correction: No AuditFileStatus node found at the end, data is auto corrected");
                    }
                    else
                    {
                        Logger.Info("Error: No AuditFileStatus node found, Moving file to error folder");
                        helper.MoveErrorFile(filePath, dbName);
                        if (Constants.EnableEmailTrigger)
                        {
                            emailHelper.SendMail(filePath, dbName, "", EmailType.Error);
                        }

                        return false;
                    }

                    #endregion

                    #region Process Diagnostic Record

                    IEnumerable<XElement> nodes50 = nodes.Where(x => x.Attribute("STXID").Value.Equals("50"));
                    if (nodes50 != null && nodes50.Any())
                    {
                        diagnosticRecord = new DiagnosticRecord();

                        nodes50.ToList().ForEach(x =>
                        {
                            XElement thisRecord = x;
                            diagnosticRecord.Id_DiagnosticRecord = latestdiagnosticRecord;
                            diagnosticRecord.Id_Status = latestAuditFileStatus;
                            diagnosticRecord.TSN = (string)thisRecord.Element("TSN");
                            diagnosticRecord.EquipmentType = (string)thisRecord.Element("EquipmentType");
                            diagnosticRecord.DiagCode = (string)thisRecord.Element("DiagCode");
                            diagnosticRecord.DiagInfo = (string)thisRecord.Element("DiagInfo");
                            diagnosticRecord.Time = helper.ConvertToInsertTimeString((string)thisRecord.Element("IssueTime"));
                            diagnosticRecords.Add(diagnosticRecord);
                            latestdiagnosticRecord++;
                        });
                    }
                    #endregion

                    #region Process Journey Information

                    IEnumerable<XElement> nodes156 = nodes.Where(x => x.Attribute("STXID").Value.Equals("156"));
                    IEnumerable<XElement> nodes155 = nodes.Where(x => x.Attribute("STXID").Value.Equals("155"));
                    if (nodes156 != null && nodes155 != null)
                    {
                        foreach (XElement item in nodes155)
                        {
                            XElement startNode155 = item;
                            XElement endNode156 = nodes156.Where(i => Convert.ToInt32(i.Element("TSN").Value) > Convert.ToInt32(item.Element("TSN").Value)).OrderBy(i => Convert.ToInt32(i.Element("TSN").Value)).FirstOrDefault();
                            IEnumerable<XElement> nodesWithOutModules = nodes.Where(i => !i.Attribute("STXID").Value.Equals("18"));
                            List<XElement> eachJourneyNodes = nodesWithOutModules.Where(i => Convert.ToInt32(i.Element("TSN").Value) > Convert.ToInt32(startNode155.Element("TSN").Value) && Convert.ToInt32(i.Element("TSN").Value) < Convert.ToInt32(endNode156.Element("TSN").Value)).ToList();
                            if (endNode156 == null)
                            {
                                Logger.Info("Error: No end of journey node found for journey node with TSN-" + item.Element("TSN").Value + ", Moving file to error folder");
                                helper.MoveErrorFile(filePath, dbName);
                                if (Constants.EnableEmailTrigger)
                                {
                                    emailHelper.SendMail(filePath, dbName, "", EmailType.Error);
                                }

                                return false;
                            }
                            journeyDetail = new Journey
                            {
                                id_Journey = latestJourneyID,
                                id_Duty = dutyDetail.id_Duty,
                                id_Module = moduleDetail.id_Module,
                                str_RouteID = (string)startNode155.Element("RouteVariantNo"),
                                int2_JourneyID = (short)startNode155.Element("JourneyNo"),
                                int2_Direction = (short)startNode155.Element("Direction")
                            };
                            DateTime date = helper.ConvertToInsertDateString((string)startNode155.Element("StartDate"));
                            journeyDetail.dat_JourneyStartDate = date;
                            journeyDetail.dat_JourneyStartTime = helper.ConvertToInsertDateTimeString((string)startNode155.Element("StartDate"), (string)startNode155.Element("StartTime"));
                            DateTime jourDate = helper.ConvertToInsertDateString((string)endNode156.Element("JourneyStopDate"));
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
                            journeyDetail.id_BatchNo = batch;
                            journeyDetail.byt_IeType = null;
                            journeyDetail.dat_JourneyMoveTime = null;
                            journeyDetail.dat_JourneyArrivalTime = null;
                            journeyDetail.int4_GPSDistance = null;
                            journeyDetails.Add(journeyDetail);

                            #region Process Stage Information

                            IEnumerable<XElement> nodes113 = eachJourneyNodes.Where(i => i.Attribute("STXID").Value.Equals("113"));

                            if (nodes113 != null)
                            {
                                List<XElement> listNodes113 = nodes113.ToList();
                                int count = listNodes113.Count();
                                for (int i = 0; i < count; i++)
                                {
                                    XElement thisNode113 = listNodes113[i];
                                    stageDetail = new Stage
                                    {
                                        id_Stage = latestStageID,
                                        id_Journey = journeyDetail.id_Journey,
                                        id_Duty = dutyDetail.id_Duty,
                                        id_Module = moduleDetail.id_Module,
                                        int2_StageID = (short)thisNode113.Element("BoardingStage")
                                    };
                                    date = helper.ConvertToInsertDateString((string)startNode155.Element("StartDate"));
                                    stageDetail.dat_StageDate = date;
                                    stageDetail.dat_StageTime = helper.ConvertToInsertDateTimeString((string)startNode155.Element("StartDate"), (string)thisNode113.Element("Time"));
                                    stageDetail.dat_RecordMod = todayDate;
                                    stageDetail.id_BatchNo = batch;
                                    stageDetails.Add(stageDetail);

                                    tempStage = new TempStage
                                    {
                                        TSN = (string)thisNode113.Element("TSN"),
                                        RecordedTime = (string)thisNode113.Element("Time"),
                                        id_Stage = latestStageID
                                    };
                                    tempStageDetails.Add(tempStage);
                                    latestStageID++;

                                    #region Process Trans Information
                                    int nextPosition = 0;
                                    if ((i + 1) < count)
                                    {
                                        nextPosition = Convert.ToInt32(listNodes113[i + 1].Element("TSN").Value);
                                    }
                                    else
                                    {
                                        nextPosition = Convert.ToInt32(endNode156.Element("TSN").Value);
                                    }

                                    List<XElement> eachStageNodes = eachJourneyNodes.Where(j => Convert.ToInt32(j.Element("TSN").Value) > Convert.ToInt32(thisNode113.Element("TSN").Value) && Convert.ToInt32(j.Element("TSN").Value) < nextPosition).ToList();

                                    IEnumerable<XElement> cashTransNodes = eachStageNodes.Where(j => j.Attribute("STXID").Value.Equals("157"));
                                    IEnumerable<XElement> smartCardTransNodes = eachStageNodes.Where(j => j.Attribute("STXID").Value.Equals("188"));
                                    IEnumerable<XElement> groupTransNodes = eachStageNodes.Where(j => j.Attribute("STXID").Value.Equals("170"));
                                    if ((cashTransNodes != null && cashTransNodes.Any()) || (smartCardTransNodes != null && smartCardTransNodes.Any()))
                                    {
                                        cashTransNodes.ToList().ForEach(t =>
                                        {
                                            XElement thisTrans = t;
                                            transDetail = new Trans();
                                            string ticketType = t.Element("TicketType").Value.Trim();
                                            transDetail.id_Trans = latestTransID;
                                            transDetail.id_Stage = stageDetail.id_Stage;
                                            transDetail.id_Journey = journeyDetail.id_Journey;
                                            transDetail.id_Duty = dutyDetail.id_Duty;
                                            transDetail.id_Module = moduleDetail.id_Module;
                                            transDetail.str_LocationCode = dutyDetail.str_OperatorVersion.TrimStart('0').Length >= 2 ? dutyDetail.str_OperatorVersion.TrimStart('0').Substring(0, 2) : dutyDetail.str_OperatorVersion.TrimStart('0'); ;
                                            transDetail.int2_BoardingStageID = stageDetail.int2_StageID;
                                            transDetail.int2_AlightingStageID = (short)thisTrans.Element("StageNo");
                                            transDetail.int2_Class = Convert.ToInt16(ticketType, 16);
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            if (transDetail.int4_Revenue == 0 || thisTrans.Element("ClassValue").Value == "A0")
                                            {
                                                transDetail.int4_NonRevenue = 0;
                                                transDetail.int2_TicketCount = 0;
                                                transDetail.int2_PassCount = 1;
                                                transDetail.int2_Transfers = 0;
                                            }
                                            else
                                            {
                                                transDetail.int4_NonRevenue = 0;
                                                transDetail.int2_TicketCount = 1;
                                                transDetail.int2_PassCount = 0;
                                                transDetail.int2_Transfers = 0;
                                            }
                                            transDetail.dat_TransDate = helper.ConvertToInsertDateString((string)thisTrans.Element("IssueDate"));
                                            transDetail.dat_TransTime = helper.ConvertToInsertDateTimeString((string)thisTrans.Element("IssueDate"), (string)thisTrans.Element("IssueTime"));
                                            transDetail.str_SerialNumber = "";
                                            transDetail.int4_RevenueBal = 0;
                                            transDetail.int4_TripBal = 0;
                                            transDetail.int2_AnnulCount = null;
                                            transDetail.int4_AnnulCash = null;
                                            //transDetail.id_SCTrans = null;
                                            transDetail.int4_TicketSerialNumber = (int)thisTrans.Element("TicketSerialNo");
                                            transDetails.Add(transDetail);
                                            latestTransID++;
                                        });

                                        groupTransNodes.ToList().ForEach(t =>
                                        {
                                            XElement thisTrans = t;
                                            int class1 = (int)t.Element("Class1");
                                            int class2 = (int)t.Element("Class2");
                                            int count1 = (int)t.Element("Count1");
                                            int count2 = (int)t.Element("Count2");
                                            if (class1 != 0 & count1 != 0)
                                            {
                                                transDetail = new Trans
                                                {
                                                    id_Stage = stageDetail.id_Stage,
                                                    id_Journey = journeyDetail.id_Journey,
                                                    id_Duty = dutyDetail.id_Duty,
                                                    id_Module = moduleDetail.id_Module,
                                                    str_LocationCode = dutyDetail.str_OperatorVersion.TrimStart('0').Substring(0, 2),
                                                    int2_BoardingStageID = stageDetail.int2_StageID,
                                                    int2_AlightingStageID = (short)thisTrans.Element("AlightingStage"),
                                                    int2_Class = Convert.ToInt16(class1.ToString(), 16),
                                                    int4_Revenue = (int)thisTrans.Element("Fare1")
                                                };
                                                switch (transDetail.int4_Revenue)
                                                {
                                                    case 0:
                                                        transDetail.int4_NonRevenue = 0;
                                                        transDetail.int2_TicketCount = 0;
                                                        transDetail.int2_PassCount = 1;
                                                        transDetail.int2_Transfers = 0;
                                                        break;
                                                    default:
                                                        transDetail.int4_NonRevenue = 0;
                                                        transDetail.int2_TicketCount = 1;
                                                        transDetail.int2_PassCount = 0;
                                                        transDetail.int2_Transfers = 0;
                                                        break;
                                                }
                                                transDetail.dat_TransDate = helper.ConvertToInsertDateString((string)thisTrans.Element("IssueDate"));
                                                transDetail.dat_TransTime = helper.ConvertToInsertDateTimeString((string)thisTrans.Element("IssueDate"), (string)thisTrans.Element("IssueTime"));
                                                transDetail.str_SerialNumber = "";
                                                transDetail.int4_RevenueBal = 0;
                                                transDetail.int4_TripBal = 0;
                                                transDetail.int2_AnnulCount = null;
                                                transDetail.int4_AnnulCash = null;
                                                //transDetail.id_SCTrans = null;
                                                transDetail.int4_TicketSerialNumber = (int)thisTrans.Element("TicketSerialNo");
                                                for (int rec = 0; rec < count1; rec++)
                                                {
                                                    Trans newTrans = new Trans();
                                                    CopyPropertyValues(transDetail, newTrans);
                                                    newTrans.id_Trans = latestTransID;
                                                    transDetails.Add(newTrans);
                                                    latestTransID++;
                                                }
                                            }

                                            if (class2 != 0 & count2 != 0)
                                            {
                                                transDetail = new Trans
                                                {
                                                    id_Trans = latestTransID,
                                                    id_Stage = stageDetail.id_Stage,
                                                    id_Journey = journeyDetail.id_Journey,
                                                    id_Duty = dutyDetail.id_Duty,
                                                    id_Module = moduleDetail.id_Module,
                                                    str_LocationCode = dutyDetail.str_OperatorVersion.TrimStart('0').Substring(0, 2),
                                                    int2_BoardingStageID = stageDetail.int2_StageID,
                                                    int2_AlightingStageID = (short)thisTrans.Element("AlightingStage"),
                                                    int2_Class = Convert.ToInt16(class2.ToString(), 16),
                                                    int4_Revenue = (int)thisTrans.Element("Fare2")
                                                };
                                                switch (transDetail.int4_Revenue)
                                                {
                                                    case 0:
                                                        transDetail.int4_NonRevenue = 0;
                                                        transDetail.int2_TicketCount = 0;
                                                        transDetail.int2_PassCount = 1;
                                                        transDetail.int2_Transfers = 0;
                                                        break;
                                                    default:
                                                        transDetail.int4_NonRevenue = 0;
                                                        transDetail.int2_TicketCount = 1;
                                                        transDetail.int2_PassCount = 0;
                                                        transDetail.int2_Transfers = 0;
                                                        break;
                                                }
                                                transDetail.dat_TransDate = helper.ConvertToInsertDateString((string)thisTrans.Element("IssueDate"));
                                                transDetail.dat_TransTime = helper.ConvertToInsertDateTimeString((string)thisTrans.Element("IssueDate"), (string)thisTrans.Element("IssueTime"));
                                                transDetail.str_SerialNumber = "";
                                                transDetail.int4_RevenueBal = 0;
                                                transDetail.int4_TripBal = 0;
                                                transDetail.int2_AnnulCount = null;
                                                transDetail.int4_AnnulCash = null;
                                                //transDetail.id_SCTrans = null;
                                                transDetail.int4_TicketSerialNumber = (int)thisTrans.Element("TicketSerialNo");
                                                for (int rec = 0; rec < count2; rec++)
                                                {
                                                    Trans newTrans = new Trans();
                                                    CopyPropertyValues(transDetail, newTrans);
                                                    newTrans.id_Trans = latestTransID;
                                                    transDetails.Add(newTrans);
                                                    latestTransID++;
                                                }
                                            }
                                        });

                                        smartCardTransNodes.ToList().ForEach(t =>
                                        {
                                            XElement thisTrans = t;
                                            transDetail = new Trans();
                                            string ticketType = t.Element("TicketType").Value.Trim();
                                            string productData = (string)thisTrans.Element("Product1Data");
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
                                            string serialNumber = (string)thisTrans.Element("ESN");
                                            transDetail.str_SerialNumber = serialNumber;
                                            //transDetail.int4_TripBal = Helper.GetTripBalanceFromProductData(productData);
                                            transDetail.int2_AnnulCount = null;
                                            transDetail.int4_AnnulCash = null;
                                            //transDetail.id_SCTrans = null;
                                            transDetail.int4_TicketSerialNumber = (int)thisTrans.Element("TicketSerialNo");

                                            #region Process Ticket Type 
                                            int nonRevenue = 0;
                                            switch (ticketType)
                                            {
                                                case "2328":
                                                    //AdulT MJ Cancelation
                                                    transDetail.int2_Class = 995;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    transDetail.SmartCardExipry = helper.GetSmartCardExipryFromProductDate(productData);
                                                    if (!helper.IsTransferTransaction(productData))
                                                    {
                                                        //Adult MJ Transfer
                                                        nonRevenue = dbService.GetNonRevenueFromPosTransTable(serialNumber, dbName);
                                                        transDetail.int2_Class = 999;
                                                        transDetail.int2_PassCount = 1;
                                                        transDetail.int2_Transfers = 0;
                                                    }
                                                    transDetail.int4_NonRevenue = nonRevenue;

                                                    break;
                                                case "2329":
                                                    //Scholar MJ Transfer
                                                    transDetail.int2_Class = 996;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    transDetail.SmartCardExipry = helper.GetSmartCardExipryFromProductDate(productData);
                                                    if (!helper.IsTransferTransaction(productData))
                                                    {
                                                        //Scholar MJ Transfer
                                                        nonRevenue = dbService.GetNonRevenueFromPosTransTable(serialNumber, dbName);
                                                        transDetail.int2_Class = 997;
                                                        transDetail.int2_PassCount = 1;
                                                        transDetail.int2_Transfers = 0;
                                                    }
                                                    transDetail.int4_NonRevenue = nonRevenue;
                                                    break;
                                                case "232B":
                                                    //Disabled MJ Cancelation
                                                    transDetail.int2_Class = 994;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    transDetail.SmartCardExipry = helper.GetSmartCardExipryFromProductDate(productData);
                                                    if (!helper.IsTransferTransaction(productData))
                                                    {
                                                        //Disabled MJ Transfer
                                                        nonRevenue = dbService.GetNonRevenueFromPosTransTable(serialNumber, dbName);
                                                        transDetail.int2_Class = 993;
                                                        transDetail.int2_PassCount = 1;
                                                        transDetail.int2_Transfers = 0;
                                                    }
                                                    transDetail.int4_NonRevenue = nonRevenue;
                                                    break;
                                                case "03F9":
                                                    //Stored Value Adult Cancelation
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
                                                    //Stored Value Pensioner Cancelation
                                                    transDetail.int2_Class = 703;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "0409":
                                                    //Stored Value Child Cancelation
                                                    transDetail.int2_Class = 702;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "040A":
                                                    //Stored Value Scholar Cancelation
                                                    transDetail.int2_Class = 704;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "0429":
                                                    //Stored Value Parcel 1
                                                    transDetail.int2_Class = 705;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "042A":
                                                    //Stored Value Parcel 2
                                                    transDetail.int2_Class = 706;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "042B":
                                                    //Stored Value Parcel 3
                                                    transDetail.int2_Class = 707;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_TripBal = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "2715":
                                                    //Adult MJ 10 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2718":
                                                    //Adult MJ 12 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2727":
                                                    //Adult MJ 14 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "271B":
                                                    //Adult MJ 40 Recharge
                                                    transDetail.int2_Class = 714;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2722":
                                                    //Adult MJ 44 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "271E":
                                                    //Adult MJ 48 Recharge
                                                    transDetail.int2_Class = 716;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2723":
                                                    //Adult MJ 52 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2B18":
                                                    //Adult MJ 48 Recharge
                                                    transDetail.int2_Class = 718;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2B19":
                                                    //Adult MJ 48 Recharge
                                                    transDetail.int2_Class = 719;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    transDetail.int4_TripBal = helper.GetTripBalanceFromProductData(productData);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2716":
                                                    //Scholar MJ 10 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2717":
                                                    //Scholar MJ 10 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "271C":
                                                    //Scholar MJ 44 Recharge
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
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2726":
                                                    //SV 10 Recharge
                                                    transDetail.int2_Class = 741;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2728":
                                                    //SV 20 Recharge
                                                    transDetail.int2_Class = 742;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2714":
                                                    //SV 50 Recharge
                                                    transDetail.int2_Class = 743;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2710":
                                                    //SV 100 Recharge
                                                    transDetail.int2_Class = 744;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2711":
                                                    //SV 200 Recharge
                                                    transDetail.int2_Class = 745;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2712":
                                                    //SV 300 Recharge
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
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2713":
                                                    //SV 400 Recharge
                                                    transDetail.int2_Class = 747;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    transDetail.int4_TripBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    break;
                                                case "2AF9":
                                                    //Adult MJ Deposit
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
                                                    //Scholar MJ Deposit
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
                                                case "2AFE":
                                                    transDetail.int2_Class = 733;
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
                                                case "2B17":
                                                    transDetail.int2_Class = 751;
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
                                                posTransDetail = null;
                                            }
                                            if (transDetail != null && ticketType.Trim() != "2AF8")
                                            {
                                                transDetails.Add(transDetail);
                                                latestTransID++;
                                                transDetail = null;
                                            }
                                        });
                                    }
                                    #endregion
                                }
                            }
                            #endregion

                            latestJourneyID++;
                        }
                    }
                    #endregion

                    #region Process Staff Information

                    if (!dbService.DoesRecordExist("Staff", "int4_StaffID", dutyDetail.int4_OperatorID.Value, dbName))
                    {
                        XElement node125 = nodes.Where(x => x.Attribute("STXID").Value.Equals("125")).FirstOrDefault();
                        if (node151 != null && node125 != null)
                        {
                            staffDetail = new Staff
                            {
                                int4_StaffID = dutyDetail.int4_OperatorID.Value
                            };
                            staffDetail.str50_StaffName = "New Staff" + " - " + staffDetail.int4_StaffID;
                            staffDetail.bit_InUse = true;
                            staffDetail.int4_StaffTypeID = 1;
                            staffDetail.int4_StaffSubTypeID = 0;
                            //var runningBoard = dutyDetail.str_OperatorVersion;
                            staffDetail.str4_LocationCode = "0001";//runningBoard.Substring(runningBoard.Length - 4, 4);
                            staffDetail.str2_LocationCode = null;
                            string serialNumber = (string)node125.Element("DriverCardSerialNo");
                            staffDetail.SerialNumber = Convert.ToInt32(serialNumber, 16).ToString();
                        }
                    }
                    #endregion

                    #region Process Inspector Information
                    IEnumerable<XElement> nodes34 = nodes.Where(x => x.Attribute("STXID").Value.Equals("34"));
                    if (nodes34 != null)
                    {
                        nodes34.ToList().ForEach(x =>
                        {
                            XElement thisInspector = x;
                            XElement inspectorStage = nodes.Where(i => i.Attribute("STXID").Value.Equals("113") && Convert.ToInt32(i.Element("TSN").Value) < Convert.ToInt32(thisInspector.Element("TSN").Value)).OrderBy(i => Convert.ToInt32(i.Element("TSN").Value)).FirstOrDefault();
                            int tempStageID = tempStageDetails.Where(i => i.TSN.Equals(inspectorStage.Element("TSN").Value) && i.RecordedTime.Equals(inspectorStage.Element("Time").Value)).FirstOrDefault().id_Stage;
                            inspectorDetail = new Inspector
                            {
                                id_Inspector = latestInspectorID,
                                id_Stage = tempStageID,
                                id_InspectorID = (int)thisInspector.Element("InspectorNo")
                            };
                            DateTime date = helper.ConvertToInsertDateString((string)node151.Element("DutyDate"));
                            inspectorDetail.datTimeStamp = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)thisInspector.Element("Time"));
                            inspectorDetails.Add(inspectorDetail);
                            latestInspectorID++;
                        });

                    }
                    #endregion

                    #region Process Bus Defect/Bus Checklist

                    List<XElement> nodes208 = nodes.Where(x => x.Attribute("STXID").Value.Equals("208")).ToList();
                    if (nodes208 != null)
                    {
                        if (nodes208.Count() > 1)
                        {
                            XElement thisBusChecklist = nodes208[0];
                            XElement lastBusChecklist = nodes208[1];
                            busChecklistDetail = new BusChecklist
                            {
                                id_Duty = dutyDetail.id_Duty,
                                id_Module = moduleDetail.id_Module
                            };
                            IEnumerable<XElement> checklistItems = thisBusChecklist.Element("CheckItems")?.Elements("CheckItem");
                            if (checklistItems != null)
                            {
                                checklistItems.ToList().ForEach(x =>
                                {
                                    switch ((int)x.Attribute("ItemID"))
                                    {
                                        case 1:
                                            busChecklistDetail.SignOnCheckItem1 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 2:
                                            busChecklistDetail.SignOnCheckItem2 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 3:
                                            busChecklistDetail.SignOnCheckItem3 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 4:
                                            busChecklistDetail.SignOnCheckItem4 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 5:
                                            busChecklistDetail.SignOnCheckItem5 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 6:
                                            busChecklistDetail.SignOnCheckItem6 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 7:
                                            busChecklistDetail.SignOnCheckItem7 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 8:
                                            busChecklistDetail.SignOnCheckItem8 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        default:
                                            break;
                                    }
                                });
                            }
                            else if (!Constants.IgnoreCheckList)
                            {
                                Logger.Info("Error: No checklist node found in bus checklist, Moving file to error folder");
                                helper.MoveErrorFile(filePath, dbName);
                                if (Constants.EnableEmailTrigger)
                                {
                                    emailHelper.SendMail(filePath, dbName, "", EmailType.Error);
                                }

                                return false;
                            }

                            busChecklistDetail.SignOnDeviceDefective = (int)thisBusChecklist.Element("DeviceDefective");
                            busChecklistDetail.SignOnTime = helper.ConvertToInsertTimeString((string)thisBusChecklist.Element("Time"));
                            checklistItems = lastBusChecklist.Element("CheckItems")?.Elements("CheckItem");
                            if (checklistItems != null)
                            {
                                checklistItems.ToList().ForEach(x =>
                                {
                                    switch ((int)x.Attribute("ItemID"))
                                    {
                                        case 1:
                                            busChecklistDetail.SignOnCheckItem1 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 2:
                                            busChecklistDetail.SignOnCheckItem2 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 3:
                                            busChecklistDetail.SignOnCheckItem3 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 4:
                                            busChecklistDetail.SignOnCheckItem4 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 5:
                                            busChecklistDetail.SignOnCheckItem5 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 6:
                                            busChecklistDetail.SignOnCheckItem6 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 7:
                                            busChecklistDetail.SignOnCheckItem7 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        case 8:
                                            busChecklistDetail.SignOnCheckItem8 = (string)x.Attribute("ItemOK") == "true" ? true : false;
                                            break;
                                        default:
                                            break;
                                    }
                                });
                            }
                            else if (!Constants.IgnoreCheckList)
                            {
                                Logger.Info("Error: No checklist node found in bus checklist, Moving file to error folder");
                                helper.MoveErrorFile(filePath, dbName);
                                if (Constants.EnableEmailTrigger)
                                {
                                    emailHelper.SendMail(filePath, dbName, "", EmailType.Error);
                                }

                                return false;
                            }

                            busChecklistDetail.SignOffDeviceDefective = (int)lastBusChecklist.Element("DeviceDefective");
                            busChecklistDetail.SignOffTime = helper.ConvertToInsertTimeString((string)lastBusChecklist.Element("Time"));
                            busChecklistDetail.id_BusChecklist = latestBusChecklistID;
                            latestBusChecklistID++;
                        }
                    }
                    #endregion
                    #endregion
                }

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
                    dutyDetail.int4_DutyPasses = journeyDetails.Where(i => i.id_Duty == dutyDetail.id_Duty).Sum(i => i.int4_JourneyPasses ?? 0);
                    dutyDetail.int4_DutyNonRevenue = journeyDetails.Where(i => i.id_Duty == dutyDetail.id_Duty).Sum(i => i.int4_JourneyNonRevenue ?? 0);
                    dutyDetail.int4_DutyTransfer = journeyDetails.Where(i => i.id_Duty == dutyDetail.id_Duty).Sum(i => i.int4_JourneyTransfer ?? 0);
                }

                if (moduleDetail != null && dutyDetail != null)
                {
                    List<Duty> dutyDetails = new List<Duty>() { dutyDetail };
                    moduleDetail.int4_ModulePasses = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyPasses ?? 0);
                    moduleDetail.int4_ModuleNonRevenue = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyNonRevenue ?? 0);
                    moduleDetail.int4_ModuleTransfer = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyTransfer ?? 0);
                    if (posTransDetails != null && posTransDetails.Any())
                    {
                        moduleDetail.int4_ModuleTickets = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyTickets ?? 0);
                        moduleDetail.int4_HdrModuleTickets = moduleDetail.int4_ModuleTickets;
                    }
                    moduleDetail.int4_HdrModulePasses = moduleDetail.int4_ModulePasses;

                }

                #endregion

                #region DB Insertion Section

                lock (thisLock)
                {
                    latestModuleID = dbService.GetLatestIDUsed("Module", "id_Module", dbName);
                    latestDutyID = dbService.GetLatestIDUsed("Duty", "id_Duty", dbName);
                    latestJourneyID = dbService.GetLatestIDUsed("Journey", "id_Journey", dbName);
                    latestStageID = dbService.GetLatestIDUsed("Stage", "id_Stage", dbName);
                    latestTransID = dbService.GetLatestIDUsed("Trans", "id_Trans", dbName);
                    latestPosTransID = dbService.GetLatestIDUsed("PosTrans", "id_PosTrans", dbName);
                    latestInspectorID = dbService.GetLatestIDUsed("Inspector", "id_Inspector", dbName);
                    latestAuditFileStatus = dbService.GetLatestIDUsed("AuditFileStatus", "Id_Status", dbName);
                    latestdiagnosticRecord = dbService.GetLatestIDUsed("DiagnosticRecord", "Id_DiagnosticRecord", dbName);
                    if (latestBusChecklistID > 0)
                    {
                        latestBusChecklistID = dbService.GetLatestIDUsed("BusChecklist", "Id_BusChecklist", dbName);
                    }

                    #region Update Actual ID's
                    moduleDetail.id_Module = moduleDetail.id_Module + latestModuleID;
                    dutyDetail.id_Module = dutyDetail.id_Module + latestModuleID;
                    dutyDetail.id_Duty = dutyDetail.id_Duty + latestDutyID;
                    if (busChecklistDetail != null)
                    {
                        busChecklistDetail.id_BusChecklist = busChecklistDetail.id_BusChecklist + latestBusChecklistID;
                    }

                    wayBillDetails.ForEach(x =>
                    {
                        x.id_Module = x.id_Module + latestModuleID;
                    });
                    journeyDetails.ForEach(x =>
                    {
                        x.id_Journey = x.id_Journey + latestJourneyID;
                        x.id_Module = x.id_Module + latestModuleID;
                        x.id_Duty = x.id_Duty + latestDutyID;
                    });

                    stageDetails.ForEach(x =>
                    {
                        x.id_Stage = x.id_Stage + latestStageID;
                        x.id_Duty = x.id_Duty + latestDutyID;
                        x.id_Journey = x.id_Journey + latestJourneyID;
                        x.id_Module = x.id_Module + latestModuleID;
                    });

                    transDetails.ForEach(x =>
                    {
                        x.id_Trans = x.id_Trans + latestTransID;
                        x.id_Module = x.id_Module + latestModuleID;
                        x.id_Duty = x.id_Duty + latestDutyID;
                        x.id_Journey = x.id_Journey + latestJourneyID;
                        x.id_Stage = x.id_Stage + latestStageID;
                    });

                    gPSCoordinates.ForEach(x =>
                    {
                        x.id_Trans = x.id_Trans + latestTransID;
                        x.id_Module = x.id_Module + latestModuleID;
                        x.id_Duty = x.id_Duty + latestDutyID;
                        x.id_Journey = x.id_Journey + latestJourneyID;
                        x.id_Stage = x.id_Stage + latestStageID;
                    });


                    posTransDetails.ForEach(x =>
                    {
                        x.id_PosTrans = x.id_PosTrans + latestPosTransID;
                        x.id_Module = x.id_Module + latestModuleID;
                        x.id_Duty = x.id_Duty + latestDutyID;
                        x.id_Journey = x.id_Journey + latestJourneyID;
                        x.id_Stage = x.id_Stage + latestStageID;
                    });

                    inspectorDetails.ForEach(x =>
                    {
                        x.id_Stage = x.id_Stage + latestStageID;
                        x.id_Inspector = x.id_Inspector + latestInspectorID;
                    });

                    auditFileDetails.ForEach(x =>
                    {
                        x.Id_Status = x.Id_Status + latestAuditFileStatus;
                        x.id_duty = x.id_duty + latestDutyID;
                    });

                    diagnosticRecords.ForEach(x =>
                    {
                        x.Id_Status = x.Id_Status + latestAuditFileStatus;
                        x.Id_DiagnosticRecord = x.Id_DiagnosticRecord + latestdiagnosticRecord;
                    });
                    #endregion

                    XmlDataToImport xmlDataToImport = new XmlDataToImport()
                    {
                        Modules = moduleDetail != null ? new List<Module>() { moduleDetail } : new List<Module>(),
                        Duties = dutyDetail != null ? new List<Duty>() { dutyDetail } : new List<Duty>(),
                        Waybills = wayBillDetails,
                        Journeys = journeyDetails,
                        Stages = stageDetails,
                        Trans = transDetails,
                        PosTrans = posTransDetails,
                        Staffs = staffDetail != null ? new List<Staff>() { staffDetail } : new List<Staff>(),
                        Inspectors = inspectorDetails,
                        AuditFileStatuss = auditFileDetails,
                        DiagnosticRecords = diagnosticRecords,
                        BusChecklistRecords = busChecklistDetail != null ? new List<BusChecklist>() { busChecklistDetail } : new List<BusChecklist>(),
                        GPSCoordinates = gPSCoordinates,
                        BusNumberLists = busNumberLists
                    };

                    result = dbService.InsertXmlFileData(xmlDataToImport, dbName);
                }

                helper.MoveSuccessFile(filePath, dbName);
                #endregion
            }
            catch (Exception ex)
            {
                string exception = JsonConvert.SerializeObject(ex).ToString();
                if (Constants.DetailedLogging)
                {
                    Logger.Error("Failed in XML ProcessFile");
                    Logger.Error("Exception:" + exception);
                }
                Logger.Error("Exception:" + exception);
                helper.MoveErrorFile(filePath, dbName);
                if (Constants.EnableEmailTrigger)
                {
                    emailHelper.SendMail(filePath, dbName, exception, EmailType.Error);
                }

                return result;
            }
            return result;
        }

        private bool ProcessRetailPOSCashier(List<XElement> nodes, string dbName, string filePath)
        {
            try
            {
                if (nodes != null)
                {
                    List<Cashier> cashierDetails = new List<Cashier>();
                    List<Staff> staffDetails = new List<Staff>();
                    Staff staffDetail = null;
                    Cashier cashier = null;

                    IEnumerable<XElement> nodes177 = nodes.Where(x => x.Attribute("STXID").Value.Equals("177"));
                    XElement node125 = nodes.Where(x => x.Attribute("STXID").Value.Equals("125")).FirstOrDefault();

                    if (nodes177 != null)
                    {
                        foreach (var node177 in nodes177)
                        {
                            cashier.StaffNumber = (string)node177.Element("SellerNumber");

                            #region Process Staff Information
                            if (!dbService.DoesRecordExist("Staff", "int4_StaffID", cashier.StaffNumber, dbName))
                            {
                                staffDetail = new Staff();
                                staffDetail.int4_StaffID = Convert.ToInt32(cashier.StaffNumber);
                                staffDetail.str50_StaffName = "New Staff" + " - " + staffDetail.int4_StaffID;
                                staffDetail.bit_InUse = true;
                                staffDetail.int4_StaffTypeID = 1;
                                staffDetail.int4_StaffSubTypeID = 0;
                                staffDetail.str4_LocationCode = "0001";
                                staffDetail.str2_LocationCode = null;
                                staffDetails.Add(staffDetail);
                            }
                            #endregion

                            string cashierDate = DateTime.ParseExact((string)node177.Element("CashInDate"), "yyyyMMdd", null).ToString("dd-MM-yyyy");
                            string tempTime = DateTime.ParseExact((string)node177.Element("CashInTime"), "HHmmss", null).ToString("HH:mm:ss tt");
                            string cashierTime = cashierDate + " " + tempTime;
                            DateTime Time12 = DateTime.Parse(cashierTime);

                            cashier.Date = DateTime.ParseExact(cashierDate, "dd-MM-yyyy", null);
                            cashier.Time = DateTime.Parse(cashierTime);
                            cashier.Revenue = (string)node177.Element("CashPaid");
                            cashier.CashOnCard = (string)node177.Element("CashOnCard");
                            cashier.ImportDateTime = DateTime.Now.ToLongDateString();


                            if (node125 != null)
                            {
                                cashier.CashierID = (string)node177.Element("DriverNumber");

                                #region Process Staff Information
                                if (!dbService.DoesRecordExist("Staff", "int4_StaffID", cashier.CashierID, dbName))
                                {
                                    staffDetail = new Staff();
                                    staffDetail.int4_StaffID = Convert.ToInt32(cashier.CashierID);
                                    staffDetail.str50_StaffName = "New Staff" + " - " + staffDetail.int4_StaffID;
                                    staffDetail.bit_InUse = true;
                                    staffDetail.int4_StaffTypeID = 1;
                                    staffDetail.int4_StaffSubTypeID = 0;
                                    staffDetail.str4_LocationCode = "0001";
                                    staffDetail.str2_LocationCode = null;
                                    staffDetails.Add(staffDetail);
                                }
                                #endregion
                            }

                            if (dbService.DoesCashierRecordExist(cashier.StaffNumber, cashier.Revenue, cashier.Time.Value, cashier.CashierID, dbName))
                            {
                                if (Constants.DetailedLogging)
                                {
                                    Logger.Info("Duplicate file found - " + Path.GetFileName(filePath));
                                }

                                helper.MoveDuplicateFile(filePath, dbName);
                                return false;
                            }

                        }

                    }

                    #region DB Insertion Section

                    var csvDataToImport = new CsvDataToImport()
                    {
                        Cashiers = cashierDetails,
                        Staffs = staffDetails
                    };

                    dbService.InsertCsvFileData(csvDataToImport, dbName);
                    helper.MoveSuccessFile(filePath, dbName);

                    return true;
                    #endregion
                }
            }
            catch
            {
                throw;
            }

            return false;
        }

        public void CopyPropertyValues(object source, object destination)
        {
            System.Reflection.PropertyInfo[] destProperties = destination.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo sourceProperty in source.GetType().GetProperties())
            {
                foreach (System.Reflection.PropertyInfo destProperty in destProperties)
                {
                    if (destProperty.Name == sourceProperty.Name &&
                destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                    {
                        destProperty.SetValue(destination, sourceProperty.GetValue(
                            source, new object[] { }), new object[] { });

                        break;
                    }
                }
            }
        }

        private PosTrans MapTransToPosTrans(Trans transDetail)
        {
            return new PosTrans()
            {
                dat_TransDate = transDetail.dat_TransDate,
                id_Duty = transDetail.id_Duty,
                dat_TransTime = transDetail.dat_TransTime,
                id_Journey = transDetail.id_Journey,
                id_Module = transDetail.id_Module,
                //id_SCTrans = transDetail.id_SCTrans,
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

        public bool ValidateFile(string filePath)
        {
            return !helper.IsFileLocked(filePath);
        }
    }


}
