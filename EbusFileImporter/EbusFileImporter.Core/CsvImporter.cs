﻿using EbusFileImporter.Core.Helpers;
using EbusFileImporter.Core.Interfaces;
using EbusFileImporter.DataProvider;
using EbusFileImporter.DataProvider.Models;
using EbusFileImporter.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EbusFileImporter.Core
{
    public class CsvImporter : IImporter
    {
        public static ILogService Logger { get; set; }
        Helper helper = null;
        EmailHelper emailHelper = null;
        DBService dbService = null;
        string uid = "";
        public CsvImporter(ILogService logger)
        {
            Logger = logger;
            helper = new Helper(logger);
            emailHelper = new EmailHelper(logger);
            dbService = new DBService(logger);
        }
        //static EbusImporterContext context = null;
        //static AtmalangEbusImporterContext atamalangContext = null;

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
            bool result = true;
            var todayDate = DateTime.Now;
            var splitFilepath = filePath.Split('\\');
            var dbName = splitFilepath[splitFilepath.Length - 3];
            uid = Guid.NewGuid().ToString();

            try
            {
                if (Constants.DetailedLogging)
                {
                    Logger.Info("***********************************************************");
                    Logger.Info("Started Import");
                    Logger.Info("***********************************************************");
                }
                string filename = Path.GetFileName(filePath);
                //Atamelang File chech
                if (filename.Contains("eBusCashier") == true)
                {
                    if (Constants.DetailedLogging) Logger.Info("Found eBusCashier CSV file - " + filename);
                    //gets all the cashier file names
                    string[] csvFiles = Directory.GetFiles(Constants.DirectoryPath + @"\" + dbName + @"\" + @"Out\", "*.csv").Select(path => Path.GetFileName(path)).ToArray();
                    if (csvFiles.Length == 0 || !csvFiles.Where(x => x.Equals(filename)).Any())
                    {
                        if (Constants.DetailedLogging) Logger.Info("Processing eBusCashier CSV file - " + filename);
                        LoadDataForAtamelang(filePath, dbName);
                        return true;
                    }
                    if (csvFiles.Where(x => x.Equals(filename)).Any())
                    {
                        if (Constants.DetailedLogging) Logger.Info("Duplicate eBusCashier CSV file found - " + filename);
                        helper.MoveDuplicateFile(filename, dbName);
                        return false;
                    }

                }
                //Import for all other clients
                else
                {
                    if (Constants.DetailedLogging) Logger.Info("Found other CSV file - " + filename);
                    //check file name in out folder first against file to import.
                    string[] csvFiles = Directory.GetFiles(Constants.DirectoryPath + @"\" + dbName + @"\" + @"Out\", "*.csv").Select(path => Path.GetFileName(path)).ToArray();
                    if (csvFiles.Length == 0 || !csvFiles.Where(x => x.Equals(filename)).Any())
                    {
                        if (Constants.DetailedLogging) Logger.Info("Processing other CSV file - " + filename);
                        LoadDataForOthers(filePath, dbName);
                        return true;
                    }
                    if (csvFiles.Where(x => x.Equals(filename)).Any())
                    {
                        if (Constants.DetailedLogging) Logger.Info("Duplicate file found - " + Path.GetFileName(filePath));
                        helper.MoveDuplicateFile(filename, dbName);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = JsonConvert.SerializeObject(ex).ToString(); ;
                if (Constants.DetailedLogging)
                {
                    Logger.Error("Failed in CSV ProcessFile");
                    Logger.Error("Exception:" + exception);
                }
                helper.MoveErrorFile(filePath, dbName);
                if (Constants.EnableEmailTrigger) emailHelper.SendMail(filePath, dbName, exception, EmailType.Error);
                return result;
            }

            return result;
        }

        public void LoadDataForAtamelang(string filePath, string dbName)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
                var todayDate = DateTime.Now;
                string fileName = Path.GetFileName(filePath);
                var lines = File.ReadAllLines(filePath);
                List<CashierStaffESN> cashierStaffESNDetails = new List<CashierStaffESN>();
                List<CashierDetail> cashierDetails = new List<CashierDetail>();
                List<CashierSigonSignoff> cashierSigonSignoffDetails = new List<CashierSigonSignoff>();
                List<Staff> staffDetails = new List<Staff>();
                Staff staffDetail = null;
                #region Check Duplicate file

                if (lines.Any())
                {
                    Logger.Info("Duplicate check started");
                    var lastLine = lines.LastOrDefault();
                    string[] values = lastLine.Split(',');
                    string receiptnumber = values[13].ToString().Trim();
                    string terminal = values[17].ToString().Replace("\"", "").Trim();
                    string casherID = fileName.Substring(34, 6);
                    if (dbService.DoesCSVRecordExist("CashierSigonSignoff", "WaybillNumber,Terminal,CashierID",  receiptnumber.Trim() + "," + terminal.Trim() + "," + casherID.Trim(), dbName))
                    {
                        Logger.Info("Duplicate file found - " + fileName);
                        helper.MoveDuplicateFile(filePath, dbName);
                        return;
                    }
                    Logger.Info("Duplicate check End");
                }

                #endregion
                #region Process lines in CSV

                lines.ToList().ForEach(x =>
                {
                    var thisItem = x;
                    string[] values = x.Split(',');
                    string Name = values[0].ToString().Replace("\"", "").Trim();
                    string staffID = values[1].ToString().Trim();
                    string Signondatetime = values[2].ToString().Trim();
                    string Signoffdatetime = values[3].ToString().Trim();
                    string activity = values[4].ToString().Trim();
                    string cashIntype = values[5].ToString().Trim();
                    string TransactionDatetime = values[6].ToString().Trim();
                    string driverTotal = values[7].ToString().Trim();
                    string cashCollected = values[8].ToString().Trim();
                    string shorts = values[9].ToString().Trim();
                    string overs = values[10].ToString().Trim();
                    string nettickets = values[11].ToString().Trim();
                    string netpasses = values[12].ToString().Trim();
                    string receiptnumber = values[13].ToString().Trim();
                    string casherID = fileName.Substring(34, 6);
                    string reason = values[14].ToString().Trim();
                    string esn = values[15].ToString().Trim();
                    string psn = values[16].ToString().Trim();
                    string terminal = values[17].ToString().Replace("\"", "").Trim();
                    int? oldDuty = null, newDuty = null;
                    if (values.Length > 18)
                    {
                        oldDuty = Convert.ToInt32(values[18]);
                        newDuty = Convert.ToInt32(values[19]);
                    }
                    //SignonDateTime sub string
                    string temp1 = Signondatetime.Substring(0, 8);
                    string temp2 = Signondatetime.Substring(8, 6);

                    string newSignOnDateTime = temp1 + " " + temp2;
                    string mySignOnDate = DateTime.ParseExact(newSignOnDateTime, "ddMMyyyy HHmmss", null).ToString("dd-MM-yyyy HH:mm:ss");

                    //SignOffDateTime sub string
                    string temp3 = Signoffdatetime.Substring(0, 8);
                    string temp4 = Signoffdatetime.Substring(8, 6);

                    string newSignOffDateTime = temp3 + " " + temp4;
                    string mySignOffDate = DateTime.ParseExact(newSignOffDateTime, "ddMMyyyy HHmmss", null).ToString("dd-MM-yyyy HH:mm:ss");

                    //Transaction Sub string
                    string temp5 = TransactionDatetime.Substring(0, 8);
                    string temp6 = TransactionDatetime.Substring(8, 6);

                    string newTransactionDateTime = temp5 + " " + temp6;
                    string myTransactionDate = DateTime.ParseExact(newTransactionDateTime, "ddMMyyyy HHmmss", null).ToString("dd-MM-yyyy HH:mm:ss");

                    if (!dbService.DoesRecordExist("CashierStaffESN", "ESN", esn, dbName) && !cashierStaffESNDetails.Where(i => i.ESN.Equals(esn)).Any())
                    {
                        cashierStaffESNDetails.Add(new CashierStaffESN()
                        {
                            ESN = esn,
                            PSN = Convert.ToInt64(psn),
                            OperatorID = Convert.ToInt32(staffID),
                            ImportDateTime = todayDate
                        });
                    }


                    switch (Name)
                    {
                        case "Seller":
                        case "Driver":
                            cashierDetails.Add(new CashierDetail()
                            {
                                StaffNumber = staffID,
                                SignOnDatTime = Convert.ToDateTime(mySignOnDate),
                                SignOffDateTime = Convert.ToDateTime(mySignOffDate),
                                Activity = activity,
                                CashInType = cashIntype,
                                TransactionDateTime = Convert.ToDateTime(myTransactionDate),
                                DriverTotal = driverTotal,
                                CashPaidIn = cashCollected,
                                Shorts = shorts,
                                NetTickets = Convert.ToInt32(nettickets),
                                NetPasses = Convert.ToInt32(netpasses),
                                CashInReceiptNo = Convert.ToInt32(receiptnumber),
                                CashierID = casherID,
                                ImportDateTime = todayDate,
                                Reason = reason,
                                Overs = overs,
                                Terminal = terminal,
                                UID = uid,
                                ESN = esn,
                                PSN = Convert.ToInt64(psn),
                                OldDuty = oldDuty,
                                NewDuty = newDuty
                            });
                            break;
                        case "Cashier":
                        case "Supervisor":
                            cashierSigonSignoffDetails.Add(new CashierSigonSignoff()
                            {
                                StaffNumber = staffID,
                                SignOnDatTime = Convert.ToDateTime(mySignOnDate),
                                SignOffDateTime = Convert.ToDateTime(mySignOffDate),
                                Activity = activity,
                                CashInType = cashIntype,
                                TransactionDateTime = Convert.ToDateTime(myTransactionDate),
                                DriverTotal = driverTotal,
                                CashCollected = cashCollected,
                                Shorts = shorts,
                                NetTickets = Convert.ToInt32(nettickets),
                                NetPasses = Convert.ToInt32(netpasses),
                                WaybillNumber = Convert.ToInt32(receiptnumber),
                                CashierID = casherID,
                                ImportDateTime = todayDate,
                                Overs = overs,
                                Terminal = terminal,
                                UID = uid,
                                ESN = esn,
                                PSN = Convert.ToInt64(psn),
                                OldDuty = oldDuty,
                                NewDuty = newDuty
                            });
                            break;
                    }

                    #region Process Staff Information
                    if (!dbService.DoesRecordExist("Staff", "int4_StaffID", staffID, dbName) && !staffDetails.Where(i => i.int4_StaffID.ToString().Equals(staffID)).Any())
                    {
                        staffDetail = new Staff();
                        staffDetail.int4_StaffID = Convert.ToInt32(staffID);
                        staffDetail.str50_StaffName = "New Staff" + " - " + staffDetail.int4_StaffID;
                        staffDetail.bit_InUse = true;
                        staffDetail.int4_StaffTypeID = 1;
                        staffDetail.int4_StaffSubTypeID = 0;
                        //var runningBoard = dutyDetail.str_OperatorVersion;
                        staffDetail.str4_LocationCode = "0001";//runningBoard.Substring(runningBoard.Length - 4, 4);
                        staffDetail.str2_LocationCode = null;
                        staffDetails.Add(staffDetail);
                    }

                    if (!dbService.DoesRecordExist("Staff", "int4_StaffID", casherID, dbName) && !staffDetails.Where(i => i.int4_StaffID.ToString().Equals(casherID)).Any())
                    {
                        staffDetail = new Staff();
                        staffDetail.int4_StaffID = Convert.ToInt32(casherID);
                        staffDetail.str50_StaffName = "New Staff" + " - " + staffDetail.int4_StaffID;
                        staffDetail.bit_InUse = true;
                        staffDetail.int4_StaffTypeID = 1;
                        staffDetail.int4_StaffSubTypeID = 0;
                        //var runningBoard = dutyDetail.str_OperatorVersion;
                        staffDetail.str4_LocationCode = "0001";//runningBoard.Substring(runningBoard.Length - 4, 4);
                        staffDetail.str2_LocationCode = null;
                        staffDetails.Add(staffDetail);
                    }
                    #endregion

                });

                #endregion

                #region DB Insertion Section
                var csvDataToImport = new CsvDataToImport()
                {
                    CashierDetails = cashierDetails,
                    CashierSigonSignoffs = cashierSigonSignoffDetails,
                    CashierStaffESNs = cashierStaffESNDetails,
                    Staffs = staffDetails
                };

                dbService.InsertCsvFileData(csvDataToImport, dbName);
                helper.MoveSuccessFile(filePath, dbName);

                #endregion
            }
            catch (Exception)
            {
                Logger.Error("Failed in LoadDataForAtamelang");
                throw;
            }
        }

        public void LoadDataForOthers(string filePath, string dbName)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
                var todayDate = DateTime.Now;
                string fileName = Path.GetFileName(filePath);
                string newFile = fileName.Split('.')[0];

                var lines = File.ReadAllLines(filePath);
                List<Cashier> cashierDetails = new List<Cashier>();
                List<Staff> staffDetails = new List<Staff>();
                Staff staffDetail = null;

                #region Check Duplicate file
                if (lines.Any())
                {
                    if (Constants.DetailedLogging) Logger.Info("Duplicate check started");
                    var firstLine = lines.FirstOrDefault();
                    string[] values = firstLine.Split(',');
                    string date = values[1].ToString().Trim();
                    string time = values[2].ToString().Trim();
                    string Employee = values[3].ToString().Trim();
                    string Revenue = values[5].ToString().Trim();
                    string CashierDate = DateTime.ParseExact(date, "yyyyMMdd", null).ToString("dd-MM-yyyy");
                    string tempTime = DateTime.ParseExact(time, "HHmmss", null).ToString("HH:mm:ss tt");
                    string CashierTime = CashierDate + " " + tempTime;
                    DateTime Time12 = DateTime.Parse(CashierTime);
                    if (dbService.DoesCashierRecordExist(Employee, Revenue, Time12, newFile, dbName))
                    {
                        if (Constants.DetailedLogging) Logger.Info("Duplicate file found - " + fileName);
                        helper.MoveDuplicateFile(filePath, dbName);
                        return;
                    }
                    if (Constants.DetailedLogging) Logger.Info("Duplicate check End");
                }

                #endregion

                #region Process lines in CSV
                lines.ToList().ForEach(x =>
                {
                    var thisItem = x;
                    string[] values = x.Split(',');
                    // Do the same for values
                    string date = values[1].ToString().Trim();
                    string time = values[2].ToString().Trim();
                    string Employee = values[3].ToString().Trim();
                    string Revenue = values[5].ToString().Trim();
                    string cashOnCard = "";
                    if (values.Length > 6)
                        cashOnCard = values[6].ToString().Trim();
                    //sets the date time 
                    string CashierDate = DateTime.ParseExact(date, "yyyyMMdd", null).ToString("dd-MM-yyyy");

                    string test = CashierDate;
                    //Parsing the date and setting the correct format
                    DateTime date23 = DateTime.ParseExact(test, "dd-MM-yyyy", null);
                    //Converting the date and time to string
                    string ImportDateTime = DateTime.Now.ToString();
                    //parsing the string to get the correct time in the correct format
                    string tempTime = DateTime.ParseExact(time, "HHmmss", null).ToString("HH:mm:ss tt");
                    //combining the date and time as one variable
                    string CashierTime = CashierDate + " " + tempTime;
                    //Parsing the Cashier Time
                    DateTime Time12 = DateTime.Parse(CashierTime);

                    if (values.Length > 6 || dbName == "ikhwezi60")
                    {
                        cashierDetails.Add(new Cashier()
                        {
                            StaffNumber = Employee,
                            Date = date23,
                            Time = Time12,
                            Revenue = Revenue,
                            CashierID = newFile,
                            ImportDateTime = todayDate.ToString(),
                            CashOnCard = cashOnCard
                        });
                    }
                    else
                    {
                        cashierDetails.Add(new Cashier()
                        {
                            StaffNumber = Employee,
                            Date = date23,
                            Time = Time12,
                            Revenue = Revenue,
                            CashierID = newFile,
                            ImportDateTime = todayDate.ToString()
                        });
                    }


                    #region Process Staff Information
                    if (!dbService.DoesRecordExist("Staff", "int4_StaffID", Employee, dbName))
                    {
                        staffDetail = new Staff();
                        staffDetail.int4_StaffID = Convert.ToInt32(Employee);
                        staffDetail.str50_StaffName = "New Staff" + " - " + staffDetail.int4_StaffID;
                        staffDetail.bit_InUse = true;
                        staffDetail.int4_StaffTypeID = 1;
                        staffDetail.int4_StaffSubTypeID = 0;
                        //var runningBoard = dutyDetail.str_OperatorVersion;
                        staffDetail.str4_LocationCode = "0001";//runningBoard.Substring(runningBoard.Length - 4, 4);
                        staffDetail.str2_LocationCode = null;
                        staffDetails.Add(staffDetail);
                    }
                    #endregion

                });

                #endregion

                #region Process Staff Information
                if (!dbService.DoesRecordExist("Staff", "int4_StaffID", newFile, dbName))
                {
                    staffDetail = new Staff();
                    staffDetail.int4_StaffID = Convert.ToInt32(newFile);
                    staffDetail.str50_StaffName = "New Staff" + " - " + staffDetail.int4_StaffID;
                    staffDetail.bit_InUse = true;
                    staffDetail.int4_StaffTypeID = 1;
                    staffDetail.int4_StaffSubTypeID = 0;
                    //var runningBoard = dutyDetail.str_OperatorVersion;
                    staffDetail.str4_LocationCode = "0001";//runningBoard.Substring(runningBoard.Length - 4, 4);
                    staffDetail.str2_LocationCode = null;
                    staffDetails.Add(staffDetail);
                }
                #endregion


                #region DB Insertion Section
                var csvDataToImport = new CsvDataToImport()
                {
                    Cashiers = cashierDetails,
                    Staffs = staffDetails
                };

                dbService.InsertCsvFileData(csvDataToImport, dbName);
                helper.MoveSuccessFile(filePath, dbName);
                #endregion
            }
            catch (Exception)
            {
                Logger.Error("Failed in LoadDataForOthers");
                throw;
            }
        }

        public bool ValidateFile(string filePath)
        {
            return true;//!helper.IsFileLocked(filePath);
        }
    }
}
