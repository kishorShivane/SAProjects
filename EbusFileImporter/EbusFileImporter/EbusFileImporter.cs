using EbusFileImporter.Core.Helpers;
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
        public EbusFileImporter()
        {
            InitializeComponent();
            logger = new FileLogService(ConfigurationManager.AppSettings["LogPath"]);
            helper = new Helper(logger);
        }

        private void EbusFileImporter_Load(object sender, EventArgs e)
        {
            try
            {
                DirectoryMonitor dirMonitor = new DirectoryMonitor("C:\\eBusSuppliesTGX150AuditFiles\\Incoming", logger);
                InitializeRefreshTimer();
            }
            catch (Exception ex)
            {
                throw ex;
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
            var files = helper.DirSearch(ConfigurationManager.AppSettings["FilePath"]);
            if (files.Any())
            {
                files.ForEach(x =>
                {
                    var splitPath = x.Replace("\\\\", "\\").Split('\\');
                    if (Helpers.Helper.IsXmlFile(x))
                    {
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
                    }
                });
            }
            lblProcessedCount.Text = processed.ToString();
            lblErrorCount.Text = error.ToString();
        }
    }
}
