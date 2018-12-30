﻿using System;
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
using EbusFileImporter.DataProvider.Helpers;

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

        public bool DoesCashierRecordExist(string employee, string revenue, DateTime dateTime, string cashierID, string connectionKey)
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
                Logger.Error("Failed in DoesRecordExist integer for table" + tableName);
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

        public bool IsNumeric(string input)
        {
            int test;
            return int.TryParse(input, out test);
        }

        public bool DoesCSVRecordExist(string tableName, string searchField, string searchString, string connectionKey)
        {
            var result = false;
            SqlConnection con = null;
            SqlCommand cmd = null;
            try
            {
                using (con = GetConnection(GetConnectionString(connectionKey)))
                {
                    con.Open();
                    var searchFieldList = searchField.Split(',');
                    var searchStringList = searchString.Split(',');
                    var whereClause = "";
                    if (searchFieldList.Count() == searchStringList.Count())
                    {
                        for (var i = 0; i < searchFieldList.Count(); i++)
                        {
                            var searchValue = searchStringList[i];
                            if (!IsNumeric(searchStringList[i]))
                            {
                                searchValue = "'" + searchValue + "'";
                            }

                            if (whereClause == string.Empty)
                            {
                                whereClause = searchFieldList[i] + " = " + searchValue;
                            }
                            else
                            {
                                whereClause = whereClause + " AND " + searchFieldList[i] + " = " + searchValue;
                            }
                        }
                    }

                    string query = "SELECT " + searchField + " FROM " + tableName + " WHERE " + whereClause + ";";
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
                Logger.Error("Failed in DoesRecordExist integer for table" + tableName);
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
                    string query = "SELECT TOP 1" + keyField + " FROM " + tableName + " ORDER BY " + keyField + " DESC;";
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
                Logger.Error("Failed in GetLatestIDUsed for " + tableName);
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
                    if (Constants.DetailedLogging) Logger.Info("-------DB Transaction - Start-------");

                    con.Open();
                    transaction = con.BeginTransaction();
                    if (xmlDataToImport.Modules.Any()) DbHelper.BulkCopyDataToTable<Module>("Module", xmlDataToImport.Modules, con, transaction);
                    Logger.Info("Inserted Module");
                    if (xmlDataToImport.Duties.Any()) DbHelper.BulkCopyDataToTable<Duty>("Duty", xmlDataToImport.Duties, con, transaction);
                    Logger.Info("Inserted Duty");
                    if (xmlDataToImport.Waybills.Any()) DbHelper.BulkCopyDataToTable<Waybill>("Waybill", xmlDataToImport.Waybills, con, transaction);
                    Logger.Info("Inserted Waybill");
                    if (xmlDataToImport.Journeys.Any()) DbHelper.BulkCopyDataToTable<Journey>("Journey", xmlDataToImport.Journeys, con, transaction);
                    Logger.Info("Inserted Journey");
                    if (xmlDataToImport.Stages.Any()) DbHelper.BulkCopyDataToTable<Stage>("Stage", xmlDataToImport.Stages, con, transaction);
                    Logger.Info("Inserted Stage");
                    if (xmlDataToImport.Staffs.Any()) DbHelper.BulkCopyDataToTable<Staff>("Staff", xmlDataToImport.Staffs, con, transaction);
                    Logger.Info("Inserted Staff");
                    if (xmlDataToImport.Inspectors.Any()) DbHelper.BulkCopyDataToTable<Inspector>("Inspector", xmlDataToImport.Inspectors, con, transaction);
                    Logger.Info("Inserted Inspector");
                    if (xmlDataToImport.Trans.Any()) DbHelper.BulkCopyDataToTable<Trans>("Trans", xmlDataToImport.Trans, con, transaction);
                    Logger.Info("Inserted Trans");
                    if (xmlDataToImport.PosTrans.Any()) DbHelper.BulkCopyDataToTable<PosTrans>("PosTrans", xmlDataToImport.PosTrans, con, transaction);
                    Logger.Info("Inserted PosTrans");
                    if (xmlDataToImport.AuditFileStatuss.Any()) DbHelper.BulkCopyDataToTable<AuditFileStatus>("AuditFileStatus", xmlDataToImport.AuditFileStatuss, con, transaction);
                    Logger.Info("Inserted AuditFileStatus");
                    if (xmlDataToImport.DiagnosticRecords.Any()) DbHelper.BulkCopyDataToTable<DiagnosticRecord>("DiagnosticRecord", xmlDataToImport.DiagnosticRecords, con, transaction);
                    Logger.Info("Inserted DiagnosticRecords");
                    if (xmlDataToImport.BusChecklistRecords.Any()) DbHelper.BulkCopyDataToTable<BusChecklist>("BusChecklist", xmlDataToImport.BusChecklistRecords, con, transaction);
                    Logger.Info("Inserted BusChecklistRecords");
                    if (xmlDataToImport.GPSCoordinates.Any()) DbHelper.BulkCopyDataToTable<GPSCoordinate>("GPSCoordinates", xmlDataToImport.GPSCoordinates, con, transaction);
                    Logger.Info("Inserted GPS Coordinates");
                    transaction.Commit();

                    if (Constants.DetailedLogging)
                    {
                        Logger.Info("Duties Inserted: " + xmlDataToImport.Duties.Count().ToString());
                        Logger.Info("Modules Inserted: " + xmlDataToImport.Modules.Count().ToString());
                        Logger.Info("Waybills Inserted:  " + xmlDataToImport.Waybills.Count().ToString());
                        Logger.Info("Journeys Inserted:  " + xmlDataToImport.Journeys.Count().ToString());
                        Logger.Info("Stages Inserted:  " + xmlDataToImport.Stages.Count().ToString());
                        Logger.Info("Staffs Inserted: " + xmlDataToImport.Staffs.Count().ToString());
                        Logger.Info("Inspectors Inserted: " + xmlDataToImport.Inspectors.Count().ToString());
                        Logger.Info("PosTrans Inserted: " + xmlDataToImport.PosTrans.Count().ToString());
                        Logger.Info("Trans Inserted: " + xmlDataToImport.Trans.Count().ToString());
                        Logger.Info("AuditFileStatus Inserted: " + xmlDataToImport.AuditFileStatuss.Count().ToString());
                        Logger.Info("DiagnosticRecords Inserted: " + xmlDataToImport.DiagnosticRecords.Count().ToString());
                        Logger.Info("BusChecklistRecords Inserted: " + xmlDataToImport.BusChecklistRecords.Count().ToString());
                        Logger.Info("Commited Changes");
                        Logger.Info("-------DB Transaction - End-------");
                    }
                }
                catch (Exception)
                {
                    if (Constants.DetailedLogging) Logger.Error("Failed in InsertXmlFileData");
                    transaction.Rollback();
                    if (Constants.DetailedLogging) Logger.Info("Transaction Rolledback");
                    if (Constants.DetailedLogging) Logger.Info("XML Data Insertion Failed in DB Service");
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
                    if (Constants.DetailedLogging) Logger.Info("-------DB Transaction - Start-------");

                    con.Open();
                    transaction = con.BeginTransaction();
                    if (csvDataToImport.Cashiers.Any()) DbHelper.BulkCopyDataToTable<Cashier>("Cashier", csvDataToImport.Cashiers, con, transaction);
                    if (csvDataToImport.CashierDetails.Any()) DbHelper.BulkCopyDataToTable<CashierDetail>("CashierDetail", csvDataToImport.CashierDetails, con, transaction);
                    if (csvDataToImport.CashierSigonSignoffs.Any()) DbHelper.BulkCopyDataToTable<CashierSigonSignoff>("CashierSigonSignoff", csvDataToImport.CashierSigonSignoffs, con, transaction);
                    if (csvDataToImport.CashierStaffESNs.Any()) DbHelper.BulkCopyDataToTable<CashierStaffESN>("CashierStaffESN", csvDataToImport.CashierStaffESNs, con, transaction);
                    if (csvDataToImport.Staffs.Any()) DbHelper.BulkCopyDataToTable<Staff>("Staff", csvDataToImport.Staffs, con, transaction);

                    transaction.Commit();
                    if (Constants.DetailedLogging)
                    {
                        Logger.Info("Cashiers Inserted:  " + csvDataToImport.Cashiers.Count().ToString());
                        Logger.Info("CashierStaffESNs Inserted: " + csvDataToImport.CashierStaffESNs.Count().ToString());
                        Logger.Info("CashierDetails Inserted:  " + csvDataToImport.CashierDetails.Count().ToString());
                        Logger.Info("CashierSigonSignoffs Inserted:  " + csvDataToImport.CashierSigonSignoffs.Count().ToString());
                        Logger.Info("Commited Changes");
                        Logger.Info("-------DB Transaction - End-------");
                    }
                }
                catch (Exception)
                {
                    if (Constants.DetailedLogging) Logger.Error("Failed in InsertCsvFileData");
                    transaction.Rollback();
                    if (Constants.DetailedLogging) Logger.Info("Transaction Rolledback");
                    if (Constants.DetailedLogging) Logger.Info("CSV Data Insertion Failed in DB Service");
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
            var result = Constants.DefaultNonRevenueValue;
            SqlConnection con = null;
            SqlCommand cmd = null;
            var classIDs = @"10002,10004,10000,10022,10024,10001,731,732,733‬,741,742,743,744,745,746";

            using (con = GetConnection(GetConnectionString(connectionKey)))
            {
                con.Open();
                //string query = @"SELECT (ISNULL(int4_Revenue,0)/(CASE WHEN ISNULL(int4_TripBal,1) = 0 THEN 1 ELSE ISNULL(int4_TripBal,1) END)) AS NonRevenue FROM PosTrans WHERE str_SerialNumber ='" + serialNumber + "' AND int2_Class NOT IN (" + classIDs + ") ORDER BY dat_TransTime DESC;";
                string query = @"SELECT TOP 1 int4_Revenue, TripsRecharged FROM PosTrans WHERE str_SerialNumber ='" + serialNumber + "' AND int2_Class NOT IN (" + classIDs + ") ORDER BY dat_TransTime DESC;";
                query = query.Replace("‬", "");

                try
                {
                    using (cmd = new SqlCommand(query, con))
                    {
                        var item = cmd.ExecuteReader();
                        while (item.Read())
                        {
                            int int4_Revenue = Convert.ToInt32(item["int4_Revenue"]);
                            int TripsRecharged = Convert.ToInt32(item["TripsRecharged"]);
                            result = int4_Revenue / TripsRecharged;
                        }
                    }
                }
                catch (DivideByZeroException)
                {
                    Logger.Error("Divide By Zero Exception in getting GetNonRevenueFormPosTrans:" + serialNumber + " setting default value to non revenue.");
                }
                catch (Exception)
                {
                    Logger.Error("Error in getting GetNonRevenueFormPosTrans:" + serialNumber);
                }
                finally
                {
                    con.Close();
                    con.Dispose();
                    cmd.Dispose();
                }
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
                    string query = "SELECT * FROM Duty WHERE int4_OperatorID = " + int4_OperatorID + " AND CONVERT(VARCHAR(10),dat_DutyStartTime, 103) + ' ' + CONVERT(VARCHAR(8),dat_DutyStartTime, 108) = CONVERT(VARCHAR(24),'" + dat_DutyStartTime + "',113) AND CONVERT(VARCHAR(10),dat_DutyStopTime, 103) + ' ' + CONVERT(VARCHAR(8),dat_DutyStopTime, 108) =  CONVERT(VARCHAR(24),'" + dat_DutyStopTime + "',113);";
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
