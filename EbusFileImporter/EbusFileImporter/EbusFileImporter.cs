﻿using EbusFileImporter.Core;
using EbusFileImporter.Core.Helpers;
using EbusFileImporter.Core.Interfaces;
using EbusFileImporter.Helpers;
using EbusFileImporter.Logger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace EbusFileImporter.App
{
    public partial class EbusFileImporter : Form
    {
        private static ILogService logger = null;
        private Helper helper = null;
        private IImporter importerEngine;
        private BackgroundWorker worker;
        public static object thisLock = new object();
        private ReaderWriterLockSlim fileLock = new ReaderWriterLockSlim();
        private DateTime currentDate = DateTime.Now;

        public EbusFileImporter()
        {
            InitializeComponent();
            logger = new FileLogService(Constants.LogPath);
            helper = new Helper(logger);
        }

        private void EbusFileImporter_Load(object sender, EventArgs e)
        {
            try
            {
                worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler(StartProcess);
                //LoadGridData();
                InitializeRefreshTimer();
                //DirectoryMonitor dirMonitor = new DirectoryMonitor(Constants.DirectoryPath, logger);
                //InitializeRefreshTimer(); 
            }
            catch (Exception ex)
            {
                logger.Error("Failed in Form Load");
                throw ex;
            }
        }

        private void StartProcess(object sender, EventArgs e)
        {
            while (true)
            {
                #region logTrigger
                List<string> files = helper.DirSearch(Constants.DirectoryPath);
                if (files.Any())
                {
                    files.ForEach(x =>
                    {
                        string[] splitPath = x.Replace("\\\\", "\\").Split('\\');

                        if (splitPath.Length >= 5)
                        {
                            switch (splitPath[4])
                            {
                                case "In":
                                    if (!helper.IsFileLocked(x))
                                    {
                                        WriteStatus(lblFileName, Path.GetFileName(x));
                                        WriteStatus(lblDB, splitPath[3]);
                                        WriteStatus(lblStatus, "Importing In Process");
                                        WriteUILog(Path.GetFileName(x) + " - Import Started");
                                        if (AppHelper.IsXmlFile(x))
                                        {
                                            if (Constants.DetailedLogging)
                                            {
                                                logger.Info("Processing: XML file found - Start - " + Path.GetFileName(x) + " - Database: " + splitPath[3]);
                                            }

                                            logger.Info("------------------------*********---------------------------");
                                            logger.Info("Importing File Start: " + DateTime.Now.ToString());
                                            logger.Info("Client: " + splitPath[3]);
                                            logger.Info("File Name: " + Path.GetFileName(x));

                                            importerEngine = new XmlImporter(logger);
                                        }
                                        else if (AppHelper.IsCsvFile(x))
                                        {
                                            if (Constants.DetailedLogging)
                                            {
                                                logger.Info("Processing: CSV file found - Start - " + Path.GetFileName(x) + " - Database: " + splitPath[3]);
                                            }

                                            logger.Info("------------------------*********---------------------------");
                                            logger.Info("Importing File Start: " + DateTime.Now.ToString());
                                            logger.Info("Client: " + splitPath[3]);
                                            logger.Info("File Name: " + Path.GetFileName(x));

                                            importerEngine = new CsvImporter(logger);
                                        }
                                        else
                                        {
                                            if (Constants.DetailedLogging)
                                            {
                                                logger.Info("Processing: status file found - Start - " + Path.GetFileName(x) + " - Database: " + splitPath[3]);
                                            }

                                            logger.Info("------------------------*********---------------------------");
                                            logger.Info("Importing File Start: " + DateTime.Now.ToString());
                                            logger.Info("Client: " + splitPath[3]);
                                            logger.Info("File Name: " + Path.GetFileName(x));

                                            importerEngine = new StatusImporter(logger);
                                        }

                                        importerEngine.ProcessFile(x);
                                        WriteStatus(lblStatus, "Importing Completed");
                                        WriteUILog(Path.GetFileName(x) + " - Import Completed");
                                        if (Constants.DetailedLogging)
                                        {
                                            logger.Info("Processing: file - End - " + Path.GetFileName(x) + " - Database: " + splitPath[3]);
                                        }

                                        logger.Info("------------------------*********---------------------------");
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        //LoadGridData();
                    });
                }

                #endregion
            }
        }

        public void WriteUILog(string message)
        {
            DateTime checkDate = currentDate.AddDays(1);
            if (checkDate <= DateTime.Now)
            {
                currentDate = DateTime.Now;
                txtOverAllStatus.Invoke((MethodInvoker)(() => txtOverAllStatus.Text = string.Empty));
            }

            txtOverAllStatus.Invoke((MethodInvoker)(() => txtOverAllStatus.Text += System.Environment.NewLine + message));
        }

        public void WriteStatus(Label lbl, string text)
        {
            lbl.Invoke((MethodInvoker)(() => lbl.Text = text));
        }

        public void InitializeRefreshTimer()
        {
            //Timer logTimer = new Timer();
            //logTimer.Interval = (20 * 1000); // 5 secs
            //logTimer.Tick += new EventHandler(TriggerLogRrefresh);
            //logTimer.Start();

            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += TriggerStartProcess;
            timer.Start();
        }

        private void TriggerLogRrefresh(object sender, EventArgs e)
        {
            int error = 0;
            int processed = 0;
            List<string> files = helper.DirSearch(Constants.DirectoryPath);
            if (files.Any())
            {
                files.ForEach(x =>
                {
                    string[] splitPath = x.Replace("\\\\", "\\").Split('\\');
                    if (splitPath.Length >= 5)
                    {
                        switch (splitPath[4])
                        {
                            case "Error":
                                error++;
                                break;
                            case "Out":
                                processed++;
                                break;
                            default:
                                break;
                        }
                    }
                });
            }
            //lblProcessedCount.Text = processed.ToString();
            //lblErrorCount.Text = error.ToString();
        }

        private void TriggerStartProcess(object sender, EventArgs e)
        {
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync();
            }
        }

        //public void LoadGridData()
        //{
        //    var gridModels = new List<GridModel>();
        //    GridModel gridModel = null;
        //    var today = DateTime.Now;
        //    var thisYear = today.Year;
        //    var thisMonth = today.ToString("MMMM");
        //    var todayDate = today.ToString("dd");
        //    var yesterdayDate = today.AddDays(-1).ToString("dd");
        //    var files = helper.DirSearch(ConfigurationManager.AppSettings["DirectoryPath"]);
        //    var prevCust = "";
        //    if (files.Any())
        //    {
        //        gridModel = new GridModel();
        //        files.OrderBy(x => x);
        //        files.ForEach(x =>
        //        {
        //            var splitPath = x.Replace("\\\\", "\\").Split('\\');
        //            if (prevCust != "" && prevCust != splitPath[3])
        //            {
        //                gridModels.Add(gridModel);
        //                gridModel = new GridModel();
        //            }
        //            if (splitPath.Length >= 7)
        //            {
        //                gridModel.Customer = splitPath[3];
        //                if (splitPath[5] == thisYear.ToString() && splitPath[6] == thisMonth)
        //                {
        //                    switch (splitPath[4])
        //                    {
        //                        case "Error":
        //                            gridModel.ErrorCount += 1;
        //                            break;
        //                        case "Out":
        //                            if (splitPath[7] == todayDate)
        //                            {
        //                                gridModel.ImportedToday += 1;
        //                            }
        //                            else if (splitPath[7] == yesterdayDate)
        //                            {
        //                                gridModel.ImportedYesterday += 1;
        //                            }
        //                            break;
        //                        case "Duplicate":
        //                            gridModel.DuplicateCount += 1;
        //                            break;
        //                        case "DateProblem":
        //                            gridModel.DateProblem += 1;
        //                            break;
        //                        default:
        //                            break;
        //                    }
        //                }
        //            }
        //        });

        //        if (gridModel != null) gridModels.Add(gridModel);
        //    }
        //    if (gridModels.Any()) gridModels.ForEach(x => x.LastUpdated = DateTime.Now.ToString("dd/mm/yyyy hhmmss"));
        //    // Initialize the DataGridView.
        //    grdReportView.AutoGenerateColumns = true;
        //    grdReportView.AutoSize = true;
        //    grdReportView.DataSource = null;
        //    grdReportView.DataSource = ToDataTable<GridModel>(gridModels);
        //    return;
        //}

        //public static DataTable ToDataTable<T>(List<T> items)
        //{
        //    DataTable dataTable = new DataTable(typeof(T).Name);

        //    //Get all the properties
        //    PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    foreach (PropertyInfo prop in Props)
        //    {
        //        //Defining type of data column gives proper data table 
        //        var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
        //        //Setting column names as Property names
        //        dataTable.Columns.Add(prop.Name, type);
        //    }
        //    foreach (T item in items)
        //    {
        //        var values = new object[Props.Length];
        //        for (int i = 0; i < Props.Length; i++)
        //        {
        //            //inserting property values to datatable rows
        //            values[i] = Props[i].GetValue(item, null);
        //        }
        //        dataTable.Rows.Add(values);
        //    }
        //    //put a breakpoint here and check datatable
        //    return dataTable;
        //}
    }
}
