using EbusFileImporter.Core.Helpers;
using EbusFileImporter.Core.Interfaces;
using EbusFileImporter.DataProvider;
using EbusFileImporter.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        public CsvImporter(ILogService logger)
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
            var todayDate = DateTime.Now;
            var splitFilepath = filePath.Split('\\');
            var dbName = splitFilepath[splitFilepath.Length - 3];

            try
            {
                Logger.Info("***********************************************************");
                Logger.Info("Started Import");
                Logger.Info("***********************************************************");
                while (helper.IsFileLocked(filePath))
                {
                    Thread.Sleep(500);
                }

                string filename = Path.GetFileName(filePath);
                //Atamelang File chech
                if (filename.Contains("eBusCashier") == true)
                {
                    //gets all the cashier file names
                    string[] csvFiles = Directory.GetFiles(ConfigurationManager.AppSettings["FilePath"] + @"\" + dbName + @"\" + @"Out\", "*.csv").Select(path => Path.GetFileName(path)).ToArray();
                    if (csvFiles.Length == 0 || !csvFiles.Where(x => x.Equals(filename)).Any())
                    {
                        LoadDataForAtamelang(filePath, dbName);
                        return true;
                    }
                    if (csvFiles.Where(x => x.Equals(filename)).Any())
                    {
                        helper.MoveDuplicateFile(filename, dbName);
                        return false;
                    }

                }
                //Import for all other clients
                else
                {
                    //check file name in out folder first against file to import.
                    string[] csvFiles = Directory.GetFiles(ConfigurationManager.AppSettings["FilePath"] + @"\" + dbName + @"\" + @"Out\", "*.csv").Select(path => Path.GetFileName(path)).ToArray();
                    if (csvFiles.Length == 0 || !csvFiles.Where(x => x.Equals(filename)).Any())
                    {
                        LoadDataForOthers(filePath, dbName);
                        return true;
                    }
                    if (csvFiles.Where(x => x.Equals(filename)).Any())
                    {
                        helper.MoveDuplicateFile(filename, dbName);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = ex.InnerException == null ? "" : ex.InnerException.ToString();
                Logger.Error("Exception:" + exception);
                Logger.Info("Changes Rolledback");
                helper.MoveErrorFile(filePath, dbName);
                emailHelper.SendMail(filePath, dbName, exception, EmailType.Error);
            }

            return result;
        }

        public void LoadDataForAtamelang(string filePath, string dbName)
        {
            var todayDate = DateTime.Now;
            string fileName = Path.GetFileName(filePath);
            var lines = File.ReadAllLines(filePath);
            List<CashierStaffESN> cashierStaffESNDetails = new List<CashierStaffESN>();
            List<CashierDetail> cashierDetails = new List<CashierDetail>();
            List<CashierSigonSignoff> cashierSigonSignoffDetails = new List<CashierSigonSignoff>();
            #region Process lines in CSV

            lines.ToList().ForEach(x =>
            {
                var thisItem = x;
                string[] values = x.Split(',');
                string Name = values[0].ToString();
                string staffID = values[1].ToString();
                string Signondatetime = values[2].ToString();
                string Signoffdatetime = values[3].ToString();
                string activity = values[4].ToString();
                string cashIntype = values[5].ToString();
                string TransactionDatetime = values[6].ToString();
                string driverTotal = values[7].ToString();
                string cashCollected = values[8].ToString();
                string shorts = values[9].ToString();
                string overs = values[10].ToString();
                string nettickets = values[11].ToString();
                string netpasses = values[12].ToString();
                string receiptnumber = values[13].ToString();
                string casherID = fileName.Substring(34, 6);
                string reason = values[14].ToString();
                string esn = values[15].ToString();
                string psn = values[16].ToString();
                string terminal = values[17].ToString();
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

                cashierStaffESNDetails.Add(new CashierStaffESN()
                {
                    ESN = esn,
                    PSN = Convert.ToInt64(psn),
                    ImportDateTime = todayDate
                });
                switch (Name)
                {
                    case "Cashier":
                    case "Supervisor":
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
                            Terminal = terminal
                        });
                        break;
                    case "Seller":
                    case "Driver":
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
                            Terminal = terminal
                        });
                        break;
                }
            });

            #endregion

            #region DB Insertion Section
            using (var dbContextTransaction = context.Database.BeginTransaction())
            {
                try
                {
                    Logger.Info("-------DB Transaction - Start-------");
                    if (cashierStaffESNDetails.Any())
                    {
                        context.CashierStaffESNs.AddRange(cashierStaffESNDetails);
                        Logger.Info("CashierStaffESNs Inserted: " + cashierStaffESNDetails.Count().ToString());
                    }

                    if (cashierDetails.Any())
                    {
                        context.CashierDetails.AddRange(cashierDetails);
                        Logger.Info("CashierDetails Inserted:  " + cashierDetails.Count().ToString());
                    }

                    if (cashierSigonSignoffDetails.Any())
                    {
                        context.CashierSigonSignoffs.AddRange(cashierSigonSignoffDetails);
                        Logger.Info("CashierSigonSignoffs Inserted:  " + cashierSigonSignoffDetails.Count().ToString());
                    }

                    context.SaveChanges();
                    Logger.Info("Saved Changes");

                    dbContextTransaction.Commit();
                    Logger.Info("Commited Changes");
                    Logger.Info("-------DB Transaction - End-------");
                    helper.MoveSuccessFile(filePath, dbName);
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
        }

        public void LoadDataForOthers(string filePath, string dbName)
        {
            var todayDate = DateTime.Now;
            string fileName = Path.GetFileName(filePath);
            string newFile = fileName.Substring(0, 6);

            var lines = File.ReadAllLines(filePath);
            List<Cashier> cashierDetails = new List<Cashier>();
            #region Process lines in CSV

            lines.ToList().ForEach(x =>
            {
                var thisItem = x;
                string[] values = x.Split(',');
                // Do the same for values
                string date = values[1].ToString();
                string time = values[2].ToString();
                string Employee = values[3].ToString();
                string Revenue = values[5].ToString();

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

                cashierDetails.Add(new Cashier()
                {
                    StaffNumber = Employee,
                    Date = date23,
                    Time = Time12,
                    Revenue = Revenue,
                    CashierID = newFile,
                    ImportDateTime = todayDate.ToString()
                });
            });

            #endregion

            #region DB Insertion Section
            using (var dbContextTransaction = context.Database.BeginTransaction())
            {
                try
                {
                    Logger.Info("-------DB Transaction - Start-------");
                    if (cashierDetails.Any())
                    {
                        context.Cashiers.AddRange(cashierDetails);
                        Logger.Info("cashierDetails Inserted: " + cashierDetails.Count().ToString());
                    }

                    context.SaveChanges();
                    Logger.Info("Saved Changes");

                    dbContextTransaction.Commit();
                    Logger.Info("Commited Changes");
                    Logger.Info("-------DB Transaction - End-------");
                    helper.MoveSuccessFile(filePath, dbName);
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
        }

        public bool ValidateFile(string filePath)
        {
            return true;//!helper.IsFileLocked(filePath);
        }
    }
}
