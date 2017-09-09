using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace EbusFileDownloadService1
{
    public class EBusServerMappings
    {
        public string DepoName { get; set; }
        public string FtpServerAddress { get; set; }
        public string FtpServerAddressOut { get; set; }
        public string LocalFolderPath { get; set; }
        public string LocalBackupPathPath { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        //server address should end with "/"  Ex: "ftp://154.0.162.166/depo-abc/"
        //local address should end with "\" Ex: @"C:\MyFolder1\"
        //Ex:new EBusServerMappings{ DepoName="depo1", FtpServerAddress="ftp://154.0.162.166/depo-abc/", LocalFolderPath= @"C:\zz\", UserName="ebusftp", Password="e8us@ftp" },

        public List<EBusServerMappings> GetServerMappings()
        {
            var lst = new List<EBusServerMappings>();
            var listDepoName = GetConfigValueByKey("DepoName");
            var strDeopName = listDepoName.Split(',');

            foreach (var depo in strDeopName)
            {
                var depoLocal = depo.Trim();
                if (!string.IsNullOrEmpty(depo))
                    lst.Add(new EBusServerMappings { DepoName = depoLocal, FtpServerAddress = GetConfigValueByKey(depoLocal + "FtpServerAddress"), FtpServerAddressOut = GetConfigValueByKey(depoLocal + "FtpServerAddressOut"), LocalFolderPath = GetConfigValueByKey(depoLocal + "LocalFolderPath"), UserName = GetConfigValueByKey(depoLocal + "UserName"), Password = GetConfigValueByKey(depoLocal + "Password"), LocalBackupPathPath = GetConfigValueByKey(depoLocal + "LocalBackupPathPath") });
            }
            //var lst = new List<EBusServerMappings>
            //{
            //    new EBusServerMappings{ DepoName="IkhweziDepot", FtpServerAddress="ftp://154.0.162.166/Pavan/FTP-testing-upload/", LocalFolderPath= @"c:\Program Files\Questek\In\", UserName="ebusftp", Password="e8us@ftp", LocalBackupPathPath = @"c:\Program Files\Questek\backup\" },
            //    //new EBusServerMappings{ DepoName="Kuleka", FtpServerAddress="ftp://154.0.162.166/", LocalFolderPath= @"c:\Program Files\Questek\QMeritImport\Data\Ikhwezi\In\", UserName="ikhwezimodule", Password="e8us@3" , LocalBackupPathPath = "" },
            //    //new EBusServerMappings{ DepoName="BConection", FtpServerAddress="ftp://154.0.162.166/", LocalFolderPath= @"c:\Program Files\Questek\QMeritImport\Data\Ikhwezi\In\", UserName="ftpuser01", Password="e8us@5" , LocalBackupPathPath = "" },
            //    //new EBusServerMappings{ DepoName="John", FtpServerAddress="ftp://154.0.162.166/", LocalFolderPath= @"c:\Program Files\Questek\QMeritImport\Data\Ntambanana\In\", UserName="ntambanamodule", Password="e8us@1" , LocalBackupPathPath = "" },
            //    //new EBusServerMappings{ DepoName="CCF", FtpServerAddress="ftp://154.0.162.166/", LocalFolderPath= @"c:\Program Files\Questek\QMeritImport\Data\Ntambanana\In\", UserName="ntambanaconfig", Password="e8us@2" , LocalBackupPathPath = "" },
            //    //new EBusServerMappings{ DepoName="CASIM", FtpServerAddress="ftp://154.0.162.166/", LocalFolderPath= @"c:\Program Files\Questek\QMeritImport\Data\Ntambanana\In\", UserName="ftpuser02", Password="e8us@6" , LocalBackupPathPath = "" },
            //    //new EBusServerMappings{ DepoName="Mtunzi", FtpServerAddress="ftp://154.0.162.166/", LocalFolderPath= @"c:\Program Files\Questek\QMeritImport\Data\Ikhwezi\In\", UserName="ftpuser03", Password="astra123$" , LocalBackupPathPath = "" },
            //};
            return lst;
        }

        public string GetConfigValueByKey(string key)
        {
            var value = "";
            value = ConfigurationSettings.AppSettings.Get(key);
            return value;
        }
    }
}
