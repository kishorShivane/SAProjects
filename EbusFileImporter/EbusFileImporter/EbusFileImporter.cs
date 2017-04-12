using EbusFileImporter.App.Helpers;
using EbusFileImporter.Core;
using EbusFileImporter.Core.Helpers;
using EbusFileImporter.Core.Interfaces;
using EbusFileImporter.Helpers;
using EbusFileImporter.Logger;
using EbusFileImporter.Monitors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EbusFileImporter.App
{
    public partial class EbusFileImporter : Form
    {
        static ILogService logger = null;
        Helper helper = null;
        IImporter importerEngine;
        BackgroundWorker worker;
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
                worker.RunWorkerAsync();
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
                var files = helper.DirSearch(Constants.DirectoryPath);
                if (files.Any())
                {
                    files.ForEach(x =>
                    {
                        var splitPath = x.Replace("\\\\", "\\").Split('\\');

                        if (splitPath.Length >= 5)
                        {
                            switch (splitPath[4])
                            {
                                case "In":
                                    if (AppHelper.IsXmlFile(x))
                                    {
                                        logger.Info("Processing: XML file found - Start");
                                        importerEngine = new XmlImporter(logger);
                                        logger.Info("Processing: XML file found - End");
                                    }
                                    else
                                    {
                                        logger.Info("Processing: CSV file found - Start");
                                        importerEngine = new CsvImporter(logger);
                                        logger.Info("Processing: XML file found - End");
                                    }
                                    importerEngine.ProcessFile(x);
                                    break;
                                default:
                                    break;
                            }
                        }
                    });
                }

                #endregion
            }
        }

        public void InitializeRefreshTimer()
        {
            lblProcessedCount.Text = "0";
            lblErrorCount.Text = "0";
            Timer timer = new Timer();
            timer.Interval = (5 * 1000); // 5 secs
            timer.Tick += new EventHandler(TriggerRrefresh);
            timer.Start();
        }

        private void TriggerRrefresh(object sender, EventArgs e)
        {
            var error = 0;
            var processed = 0;
            var files = helper.DirSearch(Constants.DirectoryPath);
            if (files.Any())
            {
                files.ForEach(x =>
                {
                    var splitPath = x.Replace("\\\\", "\\").Split('\\');
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
            lblProcessedCount.Text = processed.ToString();
            lblErrorCount.Text = error.ToString();
        }
    }
}
