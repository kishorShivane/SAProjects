using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbusFileImporter.App.Helpers
{
    public class AppConstants
    {
        public static string DirectoryPath = ConfigurationManager.AppSettings["DirectoryPath"];
        public static string LogPath = ConfigurationManager.AppSettings["LogPath"];
        public static string EmailTemplate = ConfigurationManager.AppSettings["EmailTemplate"];
        public static bool EnableEmailTrigger = ConfigurationManager.AppSettings["EnableEmailTrigger"] == "true" ? true : false;

    }
}
