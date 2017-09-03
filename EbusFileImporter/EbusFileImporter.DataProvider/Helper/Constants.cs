﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbusFileImporter.DataProvider.Helpers
{
    public partial class Constants
    {
        public static string DirectoryPath = ConfigurationManager.AppSettings["DirectoryPath"];
        public static string LogPath = ConfigurationManager.AppSettings["LogPath"];
        public static string EmailTemplate = ConfigurationManager.AppSettings["EmailTemplate"];
        public static bool EnableEmailTrigger = ConfigurationManager.AppSettings["EnableEmailTrigger"] == "true" ? true : false;
        public static string ToEmail = ConfigurationManager.AppSettings["ToEmail"];
        public static string FromEmail = ConfigurationManager.AppSettings["FromEmail"];
        public static string DuplicateEmailSubject = ConfigurationManager.AppSettings["DuplicateEmailSubject"];
        public static string ErrorEmailSubject = ConfigurationManager.AppSettings["ErrorEmailSubject"];
        public static string DateProblemEmailSubject = ConfigurationManager.AppSettings["DateProblemEmailSubject"];
        public static string Host = ConfigurationManager.AppSettings["Host"];
        public static int Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
        public static string EmailUserName = ConfigurationManager.AppSettings["EmailUserName"];
        public static string EbusPassword = ConfigurationManager.AppSettings["EbusPassword"];
        public static bool DetailedLogging = ConfigurationManager.AppSettings["DetailedLogging"] == "true" ? true : false;
    }
}