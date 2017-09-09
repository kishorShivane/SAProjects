using System;
using System.Linq;
using System.ServiceProcess;
using System.Net;
using System.Timers;
using System.IO;
using System.Configuration;

namespace EBusFileUploaderService
{
    public partial class EbusUploader : ServiceBase
    {
        Timer timer = new Timer();

        public EbusUploader()
        {
            this.ServiceName = "EbusFileUploader";
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);

            UploadFilesToFtp();
            var milliSeconds = Convert.ToDouble(GetConfigValueByKey("TimeInterval")) * 60000;
            timer.Interval = milliSeconds; // set 30 mins interval
            timer.Enabled = true;
        }

        //we dont delete here ..just move from local depo folder to back up folder
        public void DeleteLocalFile(string sourceFile)
        {
            var destinationDirectory = GetConfigValueByKey("fileBackUpPath");
            var destinationPath = "";
            try
            {
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }
                destinationPath = destinationDirectory + DateTime.Now.ToString("MM-dd-yyyy");
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }
                destinationPath = destinationPath + "\\" + Path.GetFileName(sourceFile);
                File.Move(sourceFile, destinationPath);
                Logger.Log(string.Format("moved file from  folder {0} into back up path {1} ", sourceFile, destinationPath));
            }
            catch (Exception ex)
            {
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);//in case of duplicates in single day.. delete old and keep new one
                    File.Move(sourceFile, destinationPath);
                }
                else
                {
                    Logger.Log(ex.Message);
                }
            }
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            UploadFilesToFtp();
        }

        protected override void OnStop()
        {
            timer.Enabled = false;
        }

        /// <summary>
        /// sree
        /// check if folder exists => check all files inside folder => if file exists upload, then delete it from local machine.
        /// </summary>
        public void UploadFilesToFtp()
        {
            string ftpServerAddress = GetConfigValueByKey("ebusFtpUploadAddress");
            string ftpServerAddressAlternative = GetConfigValueByKey("ebusFtpUploadAddressAlternative");

            var localFolderPath = GetConfigValueByKey("depoLocalFolderPath");

            var ftpUsername = GetConfigValueByKey("ebusFtpUserName");
            var ftpPassword = GetConfigValueByKey("ebusFtpUserPassword");

            if (Directory.Exists(localFolderPath))
            {
                var filePaths = Directory.GetFiles(localFolderPath, "*.*");

                Logger.Log("Number of Files : " + filePaths.Count());

                foreach (var filePath in filePaths)
                {
                    try
                    {
                        UploadFTPLocation(filePath, ftpServerAddress, ftpUsername, ftpPassword);
                        UploadFTPLocation(filePath, ftpServerAddressAlternative, ftpUsername, ftpPassword);
                        //delete local file, need to chk if file is uploaded successfully 
                        DeleteLocalFile(filePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message);
                    }
                }
            }
        }

        private void UploadFTPLocation(string filePath, string ftpServerAddress, string ftpUsername, string ftpPassword)
        {
            var request = WebRequest.Create(ftpServerAddress + Path.GetFileName(filePath));

            Logger.Log(string.Format("uploading file {0} into ftp path {1} ", filePath, ftpServerAddress));

            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            request.Proxy = new WebProxy();

            if (File.Exists(filePath))
            {
                try
                {
                    //read file
                    //FileStream stream = File.OpenRead(filePath);
                    //var buffer = new byte[stream.Length];
                    //stream.Read(buffer, 0, buffer.Length);
                    //stream.Close();
                    //stream.Dispose(); //1

                    byte[] buffer = File.ReadAllBytes(filePath);

                    //upload file
                    Stream reqStream = request.GetRequestStream();
                    reqStream.Write(buffer, 0, buffer.Length);
                    reqStream.Close();
                    reqStream.Dispose();//1

                    FtpWebResponse ftpResp = (FtpWebResponse)request.GetResponse();//1

                    Logger.Log("File Uploadded Successfully.");

                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }
            }
        }

        public string GetConfigValueByKey(string key)
        {
            var value = "";
            value = ConfigurationManager.AppSettings.Get(key);
            return value;
        }
    }
}
