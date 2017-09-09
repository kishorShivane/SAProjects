using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace EBusReportsService
{
    public class Constants
    {
        public static string Mode = ConfigurationSettings.AppSettings["Mode"].ToString();
        public static string IntervalMinutes = ConfigurationSettings.AppSettings["IntervalMinutes"].ToString();
        public static string ScheduledTime = ConfigurationSettings.AppSettings["ScheduledTime"].ToString();
        public static string ToEmail = ConfigurationSettings.AppSettings["ToEmail"].ToString();
        public static string FromEmail = ConfigurationSettings.AppSettings["FromEmail"].ToString();
        public static string SMTP = ConfigurationSettings.AppSettings["SMTP"].ToString();
        public static string SMTPPort = ConfigurationSettings.AppSettings["SMTPPort"].ToString();
        public static string UserName = ConfigurationSettings.AppSettings["UserName"].ToString();
        public static string Password = ConfigurationSettings.AppSettings["Password"].ToString();
        public static string SchVsNotOperReportPath = ConfigurationSettings.AppSettings["SchVsNotOperReportPath"].ToString();
        public static string SchVsNotOperReportAtamalangPath = ConfigurationSettings.AppSettings["SchVsNotOperReportAtamalangPath"].ToString();
        public static string EnablesCustomersConStr = ConfigurationSettings.AppSettings["EnablesCustomersConStr"].ToString();
        public static string GmailSMTP = ConfigurationSettings.AppSettings["GmailSMTP"].ToString();
        public static string GmailSMTPPort = ConfigurationSettings.AppSettings["GmailSMTPPort"].ToString();
        public static string GmailUserName = ConfigurationSettings.AppSettings["GmailUserName"].ToString();
        public static string GmailPassword = ConfigurationSettings.AppSettings["GmailPassword"].ToString();
        public static string Gmail = ConfigurationSettings.AppSettings["Gmail"].ToString();
        public static string EmailSubject = ConfigurationSettings.AppSettings["EmailSubject"].ToString();
        public static string LogFilePath = ConfigurationSettings.AppSettings["LogFilePath"].ToString();
        public static string EBusReportServiceFilesPath = ConfigurationSettings.AppSettings["EBusReportServiceFilesPath"].ToString();
    }
}
