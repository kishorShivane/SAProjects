
using EbusFileImporter.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbusFileImporter.Core.Helpers
{
    public class Helper
    {
        private static ILogService Log;
        EmailHelper emailHelper = null;
        public Helper(ILogService logger)
        {
            Log = logger;
            emailHelper = new EmailHelper(logger);
        }


        public bool IsFileLocked(string filepath)
        {
            FileInfo file = new FileInfo(filepath);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                Log.Info("File is locked" + filepath);
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public DateTime ConvertToInsertDateString(string date)
        {
            return Convert.ToDateTime(DateTime.ParseExact(date, "ddMMyy", null).ToString("yyyy-MM-dd"));
        }

        public DateTime ConvertToInsertDateTimeString(string strDate, string strTime)
        {
            if (strTime.Length < 6) strTime = strTime.PadRight(6, '0');
            string time = DateTime.ParseExact(strTime, "HHmmss", null).ToString("HH:mm");
            return Convert.ToDateTime(DateTime.ParseExact(strDate, "ddMMyy", null).ToString("yyyy-MM-dd") + " " + time);
        }

        public void MoveErrorFile(string currentpath, string dbname)
        {
            try
            {
                //Declarations
                string path = currentpath;
                string file = Path.GetFileName(currentpath);
                //Current Path
                string dirPath1 = ConfigurationManager.AppSettings["FilePath"] + @"\" + dbname + @"\" + @"Error\" + DateTime.Now.Year + @"\" + "0" + DateTime.Now.Month + @"\" + DateTime.Now.Day;
                //New Path
                string path2 = dirPath1 + @"\" + file;

                //Checks if path exists and creates it if not
                if (!Directory.Exists(dirPath1))
                {
                    Directory.CreateDirectory(dirPath1);
                }

                //Checks if path exists and creates it if not
                File.Copy(path, path2);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                var exception = ex.InnerException == null ? "" : ex.InnerException.ToString();
                Log.Info("File move to error folder failed:" + exception);
                emailHelper.SendMail(currentpath, dbname, exception, EmailType.Error);
            }
        }

        public void MoveDuplicateFile(string currentpath, string dbname)
        {
            try
            {
                //Declarations
                string path = currentpath;
                string file = Path.GetFileName(currentpath);
                string path2 = ConfigurationManager.AppSettings["FilePath"] + @"\" + dbname + @"\" + @"Duplicate\" + file;

                //Checks if files exists and deletes if they do
                if (File.Exists(path2))
                {
                    File.Delete(path2);
                }
                File.Copy(path, path2);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                var exception = ex.InnerException == null ? "" : ex.InnerException.ToString();
                Log.Info("File move to duplicate folder failed" + ex.InnerException ?? "");
                emailHelper.SendMail(currentpath, dbname, exception, EmailType.Error);
            }
        }

        public void MoveSuccessFile(string currentpath, string dbname)
        {
            try
            {
                //Declarations 
                string result = Path.GetFileNameWithoutExtension(currentpath);
                string path = currentpath;
                string file = Path.GetFileName(currentpath);

                string dirPath1 = ConfigurationManager.AppSettings["FilePath"] + @"\" + dbname + @"\" + @"Out\" + DateTime.Now.Year + @"\" + "0" + DateTime.Now.Month + @"\" + DateTime.Now.Day;
                string path2 = dirPath1 + @"\" + file;

                //Checks directory or create it
                if (!Directory.Exists(dirPath1))
                {
                    Directory.CreateDirectory(dirPath1);
                }
                //Checks if files exists and deletes if they do
                if (File.Exists(path2))
                {
                    File.Delete(path2);
                }
                File.Copy(path, path2);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Log.Info("File move to success folder failed" + ex.InnerException ?? "");
            }
        }

        public int GetTripBalanceFromProductData(string productData)
        {
            var result = 0;
            if (productData.Length >= 12)
            {
                var charArrayProdData = productData.ToCharArray();
                var tripBalance = charArrayProdData[2].ToString() + charArrayProdData[3].ToString();
                result = Convert.ToInt32(tripBalance, 16);
            }
            return result;
        }

        public bool IsTransferTransaction(string productData)
        {
            var result = false;
            if (productData.Length >= 12)
            {
                var charArrayProdData = productData.ToCharArray();
                var transfer = charArrayProdData[4].ToString() + charArrayProdData[5].ToString();
                result = (Convert.ToInt32(transfer, 16) > 0) ? true : false;
            }
            return result;
        }

        public int GetNonRevenueFromProductData(string productData)
        {
            var result = 0;
            if (productData.Length >= 12)
            {
                var nonRevenue = productData.Substring(2, 4);
                result = Convert.ToInt32(nonRevenue);
            }
            return result;
        }

        public int GetRevenueBalanceFromProductData(string productData)
        {
            var result = 0;
            if (productData.Length >= 12)
            {
                var revenueBalance = productData.Substring(8, 4);
                result = Convert.ToInt32(revenueBalance);
            }
            return result;
        }

        public int GetHalfProductData(string productData, bool isFirstHalf)
        {
            var result = 0;
            if (productData.Length >= 12)
            {
                var revenueBalance = "";
                if (isFirstHalf) revenueBalance = productData.Substring(0, 6); else revenueBalance = productData.Substring(6, 6);
                result = Convert.ToInt32(revenueBalance);
            }
            return result;
        }

        public List<string> DirSearch(string sDir)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(sDir + @"\" + f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return files;
        }
    }
}
