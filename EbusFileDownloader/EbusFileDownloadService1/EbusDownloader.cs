using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Timers;
using System.Net;
using System.Configuration;

namespace EbusFileDownloadService1
{
    public partial class EbusDownloader : ServiceBase
    {
        Timer timer = new Timer();
        List<EBusServerMappings> serverMappins = new List<EBusServerMappings>();

        public EbusDownloader()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);

            ReloadBackupFilesToInFolder();
            DownloadFilesFromFtp();

            var milliSeconds = Convert.ToDouble(GetConfigValueByKey("TimeInterval")) * 60000;
            timer.Interval = 1800000; //30 mins // 1 min = 60000 milli Sec
            timer.Enabled = true;
        }


        public void DeleteLocalFile(string filePath)
        {
            File.Delete(filePath);
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            ReloadBackupFilesToInFolder();
            DownloadFilesFromFtp();
        }

        protected override void OnStop()
        {
            timer.Enabled = false;
        }

        public static void DeleteFilesFromFtp(string ftpAddress, string fileName, string username, string password)
        {
            var request = (FtpWebRequest)WebRequest.Create(ftpAddress + "/" + Path.GetFileName(fileName));

            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Proxy = new WebProxy();

            //Upload file
            var response = request.GetResponse();
            response.Close();
            // Logger.Log(string.Format("deleting file on Server {0}, file name {1}", ftpAddress, fileName));
        }

        public static void MoveToOutFolder(string ftpAddress, string ftpAddressOut, string fileName, string username, string password)
        {
            var filePath = ftpAddress + "/" + Path.GetFileName(fileName);
            var request = (FtpWebRequest)WebRequest.Create(filePath);
            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.Rename;
            request.Proxy = new WebProxy();
            request.RenameTo = ftpAddressOut;
            var response = request.GetResponse();
            response.Close();
        }

        public string[] ListDirectory(string serverAddress, string username, string password)
        {
            var list = new List<string>();
            var request = WebRequest.Create(serverAddress);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(username, password);
            request.Proxy = new WebProxy();

            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream, true))
                        {
                            while (!reader.EndOfStream)
                            {
                                list.Add(reader.ReadLine());
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return list.ToArray();
        }

        // we will reload(Move) all files from back up to respective IN folders
        public void ReloadBackupFilesToInFolder()
        {
            var serverMappings = new EBusServerMappings().GetServerMappings();

            foreach (var item in serverMappings)
            {
                var targetPath = item.LocalFolderPath;
                var sourcePath = item.LocalBackupPathPath;
                // loop through each backup folder and move files to IN
                if (System.IO.Directory.Exists(sourcePath))
                {
                    string[] files = System.IO.Directory.GetFiles(sourcePath);

                    // Copy the files and overwrite destination files if they already exist.
                    foreach (string source in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        var fileName = Path.GetFileName(source);
                        var destFile = Path.Combine(targetPath, fileName);
                        if (!File.Exists(destFile))
                        {
                            File.Move(source, destFile);
                        }
                    }
                }
            }
        }

        // download files from server to local backup folder, then move to IN folder.
        public void DownloadFilesFromFtp()
        {
            var serverMappings = new EBusServerMappings().GetServerMappings();

            foreach (var item in serverMappings)
            {
                var ftpServerAddress = item.FtpServerAddress;
                var ftpServerAddressOut = item.FtpServerAddressOut;
                var localFolderPath = item.LocalFolderPath;
                var backUpFolderPath = item.LocalBackupPathPath;

                if (!string.IsNullOrEmpty(ftpServerAddress) && !string.IsNullOrEmpty(localFolderPath))
                {
                    //Logger.Log(string.Format("Attempting download from the ftp server {0}", ftpServerAddress));
                    var serverFileNames = ListDirectory(ftpServerAddress, item.UserName, item.Password);
                    //Logger.Log(string.Format("Number of file found on ftp server {0} = {1}", ftpServerAddress, serverFileNames.Count()));
                    if (serverFileNames.Length > 0)
                    {
                        foreach (var fileName in serverFileNames)
                        {
                            var backUpPath = backUpFolderPath + fileName;
                            var localpath = localFolderPath + fileName;

                            // in case if there is a file with same name wait till it is processed
                            if (File.Exists(backUpPath))
                            {
                                continue;
                                //System.Threading.Thread.Sleep(300000); ///sleep 5 mins if file stil exists!
                            }

                            int bytesRead = 0;
                            byte[] buffer = new byte[2048];

                            var request = FtpWebRequest.Create(ftpServerAddress + Path.GetFileName(fileName));
                            //Logger.Log(string.Format("downloading file from ftp server {0}, file name {1}", ftpServerAddress, fileName));
                            request.Method = WebRequestMethods.Ftp.DownloadFile;
                            request.Credentials = new NetworkCredential(item.UserName, item.Password);
                            request.Proxy = new WebProxy();

                            Stream reader = request.GetResponse().GetResponseStream();

                            var fileStream = new FileStream(backUpPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

                            while (true)
                            {
                                bytesRead = reader.Read(buffer, 0, buffer.Length);

                                if (bytesRead == 0)
                                    break;

                                fileStream.Write(buffer, 0, bytesRead);
                            }
                            fileStream.Close();

                            //first we back up and then we will move to IN folder
                            File.Copy(backUpPath, localpath, true);

                            //Logger.Log(string.Format("{0} Downloaded  Successfully.", fileName));
                            //delete after download, making sure if is downloaded !!
                            if (Directory.Exists(localFolderPath))
                            {
                                if (!string.IsNullOrEmpty(ftpServerAddressOut))
                                    MoveToOutFolder(ftpServerAddress, ftpServerAddressOut, fileName, item.UserName, item.Password);
                                else
                                    DeleteFilesFromFtp(ftpServerAddress, fileName, item.UserName, item.Password);
                            }
                        }
                    }
                }
            }
        }

        public string GetConfigValueByKey(string key)
        {
            var value = "";
            value = ConfigurationSettings.AppSettings.Get(key);
            return value;
        }
    }
}
