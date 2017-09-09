using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EBusReportsService
{
    public class Helper
    {
        public static void WriteToHtml(string filePath, string fileContent)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                    {
                        w.WriteLine(fileContent);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile("Service Error on: {0} " + ex.Message + ex.StackTrace);
                throw;
            }

        }

        public static void CreateServiceFileDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                Helper.WriteToFile("Service Error on: {0} " + ex.Message + ex.StackTrace);
                throw;
            }

        }

        public static void WriteToFile(string text)
        {
            string path = Constants.LogFilePath + "\\EBusReportServiceLog.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }

        public static string GetCustomerDisplayName(string customer)
        {
            switch (customer)
            {
                case "tabanana30":
                    return "Ntambanana";

                case "mokopane10":
                    return "GNTMokopane";

                case "marblehall20":
                    return "GNTMarbleHall";

                case "gautengcoaches40":
                    return "GautengCoaches";

                case "ugu50":
                    return "UGU";

                case "ikhwezi60":
                    return "Ikhwezi";

                case "atamelang70":
                    return "AtamelangTGX";

                case "sihlangene80":
                    return "Sihlangene";

                case "ezakheni90":
                    return "EzakheniTGX";

                default:
                    return "Ikhwezi";
            }
        }
    }
}
