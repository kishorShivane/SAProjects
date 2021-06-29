using EbusFileImporter.Core.Helpers;
using EbusFileImporter.Core.Interfaces;
using EbusFileImporter.DataProvider;
using EbusFileImporter.Logger;
using Newtonsoft.Json;
using System;
using System.IO;

namespace EbusFileImporter.Core
{
    public class StatusImporter : IImporter
    {
        public static ILogService Logger { get; set; }

        private Helper helper = null;
        private EmailHelper emailHelper = null;
        private DBService dbService = null;
        public static object thisLock = new object();

        public StatusImporter(ILogService logger)
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
            //MessageBox.Show("ProcessFile");
            bool result = false;
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

                DateTime lastModified = System.IO.File.GetLastWriteTime(filePath);
                string previousLine = "";
                using (StreamReader reader = new StreamReader(filePath))
                {
                    if (reader.BaseStream.Length > 1024)
                    {
                        reader.BaseStream.Seek(-1024, SeekOrigin.End);
                    }
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        previousLine = line;
                    }
                }
                previousLine = previousLine.TrimStart();
                previousLine = previousLine.TrimEnd();
                Logger.Info("Line to be processed: " + previousLine);
                int length = previousLine.Length;

                //00036754324410.10.10.59     10000001301212144VCF - ES20EGIA 469POSBOT51XXXXXXXXEBS000012021062802: 30:142021062802:59:0900000005123456789abcdef

                Status status = new Status();

                status.ETMNum = previousLine.Substring(0, 6);
                status.ModuleNum = previousLine.Substring(6, 6);
                status.IPAddress = previousLine.Substring(12, 16);
                status.EquipmentTy = Convert.ToInt32(previousLine.Substring(28, 1));
                status.BusNum = previousLine.Substring(29, 6);
                status.DistrictId = Convert.ToInt32(previousLine.Substring(35, 2));
                status.GarageId = Convert.ToInt32(previousLine.Substring(37, 2));
                status.CustomerCode = Convert.ToInt32(previousLine.Substring(39, 4));
                status.SubCustomer = Convert.ToInt32(previousLine.Substring(43, 2));
                status.VCFVersion = previousLine.Substring(45, 8);
                status.CodeVersion = previousLine.Substring(53, 8);
                status.FLUVersion = previousLine.Substring(61, 8);
                status.FileMan= previousLine.Substring(69, 8);
                status.OptionsVer = previousLine.Substring(77, 8);
                status.LastGoodCalDate = previousLine.Substring(85, 8);
                status.LastGoodCalTime = previousLine.Substring(93, 8);
                status.ETMDate = previousLine.Substring(101, 8);
                status.ETMTime = previousLine.Substring(109, 8);
                status.LastAuditSeqNum = previousLine.Substring(117, 8);
                status.SIMID = previousLine.Substring(125, 15);

                if (dbService.InsertOrUpdateStatus(status, dbService.DoesRecordExist("Status", "ETMNum", status.ETMNum.ToString(), dbName), dbName))
                {
                    helper.MoveSuccessStatusFile(filePath, dbName);
                    Logger.Info("**************Successfully Processed:" + filePath + "**************");
                }
                else
                {
                    Logger.Error("Issue while inserting or updating Asset ETM record with ETMID: " + status.ETMNum);
                    helper.MoveErrorStatusFile(filePath, dbName);
                }
            }
            catch (Exception ex)
            {
                string exception = JsonConvert.SerializeObject(ex).ToString();
                if (Constants.DetailedLogging)
                {
                    Logger.Error("Failed in Status ProcessFile");
                    Logger.Error("Exception:" + exception);
                }

                Logger.Error("Exception:" + exception);
                helper.MoveErrorStatusFile(filePath, dbName);

                if (Constants.EnableEmailTrigger)
                {
                    emailHelper.SendMail(filePath, dbName, exception, EmailType.Error);
                }
                
                return result;
            }
            return result;
        }

        public bool ValidateFile(string filePath)
        {
            return !helper.IsFileLocked(filePath);
        }
    }
}
