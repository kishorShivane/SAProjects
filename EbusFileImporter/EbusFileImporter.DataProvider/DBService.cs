using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbusFileImporter.DataProvider.Models;
using EbusFileImporter.Logger;
using System.Data;
using System.ComponentModel;

namespace EbusFileImporter.DataProvider
{
    public class DBService
    {
        private static ILogService Logger;
        public DBService(ILogService logger)
        {
            Logger = logger;
        }

        public string GetConnectionString(string connecKey)
        {
            return ConfigurationManager.ConnectionStrings[connecKey].ConnectionString;
        }

        public SqlConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public bool DoesRecordExist(string tableName, string searchField, string searchString, string connectionKey)
        {
            var result = false;
            SqlConnection con = null;
            SqlCommand cmd = null;
            try
            {
                using (con = GetConnection(GetConnectionString(connectionKey)))
                {
                    con.Open();
                    string query = "SELECT " + searchField + " FROM " + tableName + " WHERE " + searchField + " ='" + searchString + "';";
                    using (cmd = new SqlCommand(query, con))
                    {
                        var item = cmd.ExecuteScalar();
                        if (item != null)
                        {
                            result = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                Logger.Error("Failed in DoesRecordExist");
                throw;
            }
            finally
            {
                con.Close();
                con.Dispose();
                cmd.Dispose();
            }

            return result;
        }

        public bool DoesCashierRecordExist(string employee, string revenue, DateTime dateTime,string cashierID, string connectionKey)
        {
            var result = false;
            SqlConnection con = null;
            SqlCommand cmd = null;
            try
            {
                using (con = GetConnection(GetConnectionString(connectionKey)))
                {
                    con.Open();
                    string query = "SELECT * FROM Cashier WHERE StaffNumber ='" + employee + "' AND Revenue ='" + revenue + "' AND CashierID ='" + cashierID + "' AND CONVERT(VARCHAR(24),ImportDateTime,113) = CONVERT(VARCHAR(24),'" + dateTime + "',113);";
                    using (cmd = new SqlCommand(query, con))
                    {
                        var item = cmd.ExecuteScalar();
                        if (item != null)
                        {
                            result = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                Logger.Error("Failed in DoesRecordExist");
                throw;
            }
            finally
            {
                con.Close();
                con.Dispose();
                cmd.Dispose();
            }

            return result;
        }

        public bool DoesRecordExist(string tableName, string searchField, int searchString, string connectionKey)
        {
            var result = false;
            SqlConnection con = null;
            SqlCommand cmd = null;
            try
            {
                using (con = GetConnection(GetConnectionString(connectionKey)))
                {
                    con.Open();
                    string query = "SELECT " + searchField + " FROM " + tableName + " WHERE " + searchField + " =" + searchString + ";";
                    using (cmd = new SqlCommand(query, con))
                    {
                        var item = cmd.ExecuteScalar();
                        if (item != null)
                        {
                            result = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                Logger.Error("Failed in DoesRecordExist integer");
                throw;
            }
            finally
            {
                con.Close();
                con.Dispose();
                cmd.Dispose();
            }
            return result;
        }

        public int GetLatestIDUsed(string tableName, string keyField, string connectionKey)
        {
            var result = 0;
            SqlConnection con = null;
            SqlCommand cmd = null;
            try
            {

                using (con = GetConnection(GetConnectionString(connectionKey)))
                {
                    con.Open();
                    string query = "SELECT " + keyField + " FROM " + tableName + " ORDER BY " + keyField + " DESC;";
                    using (cmd = new SqlCommand(query, con))
                    {
                        var item = cmd.ExecuteScalar();
                        if (item != null)
                        {
                            result = Convert.ToInt32(item);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Logger.Error("Failed in GetLatestIDUsed");
                throw;
            }
            finally
            {
                con.Close();
                con.Dispose();
                cmd.Dispose();
            }
            return ++result;
        }

        public bool InsertXmlFileData(XmlDataToImport xmlDataToImport, string connectionKey)
        {
            var result = false;
            SqlTransaction transaction = null;
            SqlConnection con = null;
            using (con = GetConnection(GetConnectionString(connectionKey)))
            {
                try
                {
                    Logger.Info("-------DB Transaction - Start-------");

                    con.Open();
                    transaction = con.BeginTransaction();
                    if (xmlDataToImport.Modules.Any()) DbHelper.BulkCopyDataToTable<Module>("Module", xmlDataToImport.Modules, con, transaction);
                    if (xmlDataToImport.Duties.Any()) DbHelper.BulkCopyDataToTable<Duty>("Duty", xmlDataToImport.Duties, con, transaction);
                    if (xmlDataToImport.Waybills.Any()) DbHelper.BulkCopyDataToTable<Waybill>("Waybill", xmlDataToImport.Waybills, con, transaction);
                    if (xmlDataToImport.Journeys.Any()) DbHelper.BulkCopyDataToTable<Journey>("Journey", xmlDataToImport.Journeys, con, transaction);
                    if (xmlDataToImport.Stages.Any()) DbHelper.BulkCopyDataToTable<Stage>("Stage", xmlDataToImport.Stages, con, transaction);
                    if (xmlDataToImport.Staffs.Any()) DbHelper.BulkCopyDataToTable<Staff>("Staff", xmlDataToImport.Staffs, con, transaction);
                    if (xmlDataToImport.Inspectors.Any()) DbHelper.BulkCopyDataToTable<Inspector>("Inspector", xmlDataToImport.Inspectors, con, transaction);
                    if (xmlDataToImport.Trans.Any()) DbHelper.BulkCopyDataToTable<Trans>("Trans", xmlDataToImport.Trans, con, transaction);
                    if (xmlDataToImport.PosTrans.Any()) DbHelper.BulkCopyDataToTable<PosTrans>("PosTrans", xmlDataToImport.PosTrans, con, transaction);
                    transaction.Commit();

                    Logger.Info("Duties Inserted: " + xmlDataToImport.Duties.Count().ToString());
                    Logger.Info("Modules Inserted: " + xmlDataToImport.Modules.Count().ToString());
                    Logger.Info("Waybills Inserted:  " + xmlDataToImport.Waybills.Count().ToString());
                    Logger.Info("Journeys Inserted:  " + xmlDataToImport.Journeys.Count().ToString());
                    Logger.Info("Stages Inserted:  " + xmlDataToImport.Stages.Count().ToString());
                    Logger.Info("Staffs Inserted: " + xmlDataToImport.Staffs.Count().ToString());
                    Logger.Info("Inspectors Inserted: " + xmlDataToImport.Inspectors.Count().ToString());
                    Logger.Info("PosTrans Inserted: " + xmlDataToImport.PosTrans.Count().ToString());
                    Logger.Info("Trans Inserted: " + xmlDataToImport.Trans.Count().ToString());
                    Logger.Info("Commited Changes");
                    Logger.Info("-------DB Transaction - End-------");
                }
                catch (Exception)
                {
                    Logger.Error("Failed in InsertXmlFileData");
                    transaction.Rollback();
                    Logger.Info("Transaction Rolledback");
                    Logger.Info("XML Data Insertion Failed in DB Service");
                    throw;
                }
                finally
                {
                    con.Close();
                    con.Dispose();
                    transaction.Dispose();
                }
            }
            return result;
        }

        public bool InsertCsvFileData(CsvDataToImport csvDataToImport, string connectionKey)
        {
            var result = false;
            SqlTransaction transaction = null;
            SqlConnection con = null;
            using (con = GetConnection(GetConnectionString(connectionKey)))
            {
                try
                {
                    Logger.Info("-------DB Transaction - Start-------");

                    con.Open();
                    transaction = con.BeginTransaction();
                    if (csvDataToImport.Cashiers.Any()) DbHelper.BulkCopyDataToTable<Cashier>("Cashier", csvDataToImport.Cashiers, con, transaction);
                    if (csvDataToImport.CashierDetails.Any()) DbHelper.BulkCopyDataToTable<CashierDetail>("CashierDetail", csvDataToImport.CashierDetails, con, transaction);
                    if (csvDataToImport.CashierSigonSignoffs.Any()) DbHelper.BulkCopyDataToTable<CashierSigonSignoff>("CashierSigonSignoff", csvDataToImport.CashierSigonSignoffs, con, transaction);
                    if (csvDataToImport.CashierStaffESNs.Any()) DbHelper.BulkCopyDataToTable<CashierStaffESN>("CashierStaffESN", csvDataToImport.CashierStaffESNs, con, transaction);
                    if (csvDataToImport.Staffs.Any()) DbHelper.BulkCopyDataToTable<Staff>("Staff", csvDataToImport.Staffs, con, transaction);

                    transaction.Commit();

                    Logger.Info("Cashiers Inserted:  " + csvDataToImport.Cashiers.Count().ToString());
                    Logger.Info("CashierStaffESNs Inserted: " + csvDataToImport.CashierStaffESNs.Count().ToString());
                    Logger.Info("CashierDetails Inserted:  " + csvDataToImport.CashierDetails.Count().ToString());
                    Logger.Info("CashierSigonSignoffs Inserted:  " + csvDataToImport.CashierSigonSignoffs.Count().ToString());
                    Logger.Info("Commited Changes");
                    Logger.Info("-------DB Transaction - End-------");
                }
                catch (Exception)
                {
                    Logger.Error("Failed in InsertCsvFileData");
                    transaction.Rollback();
                    Logger.Info("Transaction Rolledback");
                    Logger.Info("CSV Data Insertion Failed in DB Service");
                    throw;
                }
                finally
                {
                    con.Close();
                    con.Dispose();
                    transaction.Dispose();
                }
            }
            return result;
        }

        public int GetNonRevenueFromPosTransTable(string serialNumber, string connectionKey)
        {
            var result = 800;
            SqlConnection con = null;
            SqlCommand cmd = null;
            try
            {

                using (con = GetConnection(GetConnectionString(connectionKey)))
                {
                    con.Open();
                    string query = "SELECT (ISNULL(int4_Revenue,0)/ISNULL(int4_TripBal,1)) AS NonRevenue FROM PosTrans WHERE str_SerialNumber ='" + serialNumber + "' ORDER BY dat_TransTime DESC;";
                    using (cmd = new SqlCommand(query, con))
                    {
                        var item = cmd.ExecuteScalar();
                        if (item != null)
                        {
                            result = Convert.ToInt32(item);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Logger.Error("Failed in GetNonRevenueFormPosTrans");
                throw;
            }
            finally
            {
                con.Close();
                con.Dispose();
                cmd.Dispose();
            }
            return result;
        }

        public bool CheckForDutyDuplicates(int int4_OperatorID, DateTime dat_DutyStartTime, DateTime dat_DutyStopTime, string connectionKey)
        {
            var result = false;
            SqlConnection con = null;
            SqlCommand cmd = null;
            try
            {
                using (con = GetConnection(GetConnectionString(connectionKey)))
                {
                    con.Open();
                    string query = "SELECT * FROM Duty WHERE int4_OperatorID = " + int4_OperatorID + " AND CONVERT(VARCHAR(24),dat_DutyStartTime,113) = CONVERT(VARCHAR(24),'" + dat_DutyStartTime + "',113) AND CONVERT(VARCHAR(24),dat_DutyStopTime,113) =  CONVERT(VARCHAR(24),'" + dat_DutyStopTime + "',113);";
                    using (cmd = new SqlCommand(query, con))
                    {
                        var item = cmd.ExecuteScalar();
                        if (item != null)
                        {
                            result = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                Logger.Error("Failed in DoesRecordExist integer");
                throw;
            }
            finally
            {
                con.Close();
                con.Dispose();
                cmd.Dispose();
            }
            return result;
        }
    }
}
