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

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
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
        public void DeleteLocalFile(string sourceFile, string fileBackUpPath)
        {
            var destinationDirectory = fileBackUpPath;
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
            var localFolderPath = GetConfigValueByKey("depoLocalFolderPath");

            string ftpServerAddress = GetConfigValueByKey("DefaultFTP");
            string ftpServerAddressAlternative = GetConfigValueByKey("DefaultFTPAlternative");
            string ftpUsername = GetConfigValueByKey("DefaultUserName");
            string ftpPassword = GetConfigValueByKey("DefaultPassword");
            string fileBackUpPath = GetConfigValueByKey("DefaultFileBackUpPath");
            string filePathToUpload = "";

            if (Directory.Exists(localFolderPath))
            {
                var filePaths = Directory.GetFiles(localFolderPath, "*.*");
                if (filePaths.Any())
                {
                    Logger.Log("Number of Files : " + filePaths.Count());
                    var fileNameMatchingID = GetConfigValueByKey("FileNameMatchingID").Split(',');
                    if (fileNameMatchingID.Any())
                    {
                        foreach (var filePath in filePaths)
                        {
                            filePathToUpload = filePath.Trim();
                            for (var i = 0; i < fileNameMatchingID.Count(); i++)
                            {
                                var localID = fileNameMatchingID[i].Trim();
                                if (filePath.Contains(localID))
                                {

                                    ftpServerAddress = GetConfigValueByKey(localID + "FTP");
                                    ftpServerAddressAlternative = GetConfigValueByKey(localID + "FTPAlternative");
                                    ftpUsername = GetConfigValueByKey(localID + "UserName");
                                    ftpPassword = GetConfigValueByKey(localID + "Password");
                                    fileBackUpPath = GetConfigValueByKey(localID + "FileBackUpPath");
                                    break;
                                }
                            }

                            try
                            {
                                if (!CheckIfFileExistsOnFTP(Path.GetFileName(filePathToUpload).Trim(), ftpServerAddress, ftpUsername, ftpPassword))
                                {
                                    if (ftpServerAddress != string.Empty) UploadFTPLocation(filePathToUpload, ftpServerAddress, ftpUsername, ftpPassword);
                                    if (ftpServerAddressAlternative != string.Empty) UploadFTPLocation(filePathToUpload, ftpServerAddressAlternative, ftpUsername, ftpPassword);
                                    //delete local file, need to chk if file is uploaded successfully 
                                    DeleteLocalFile(filePathToUpload, fileBackUpPath);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(ex.Message);
                            }
                        }
                    }


                }
            }
        }

        private void UploadFTPLocation(string filePath, string ftpServerAddress, string ftpUsername, string ftpPassword)
        {
            var uri = new Uri(ftpServerAddress + "/" + Path.GetFileName(filePath));
            if (uri.IsWellFormedOriginalString())
            {
                var request = WebRequest.Create(uri);

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
        }

        private bool CheckIfFileExistsOnFTP(string fileName, string ftpServerAddress, string ftpUsername, string ftpPassword)
        {
            var uri = new Uri(ftpServerAddress + "/" + fileName);
            if (uri.IsWellFormedOriginalString())
            {
                var request = (FtpWebRequest)WebRequest.Create(uri);
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                request.Method = WebRequestMethods.Ftp.GetFileSize;

                try
                {
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    return true;
                }
                catch (WebException ex)
                {
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    Logger.Log(string.Format("File {0} exist on FTP {1} ", fileName, ftpServerAddress));
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        return false;
                }
            }
            return false;
        }


        public string GetConfigValueByKey(string key)
        {
            var value = "";
            value = ConfigurationManager.AppSettings.Get(key);
            return value.Trim();
        }
    }
}
