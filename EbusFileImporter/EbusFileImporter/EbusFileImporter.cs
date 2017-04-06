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
        public EbusFileImporter()
        {
            InitializeComponent();
            logger = new FileLogService(ConfigurationManager.AppSettings["LogPath"]);
        }

        private void EbusFileImporter_Load(object sender, EventArgs e)
        {
            try
            {
                DirectoryMonitor dirMonitor = new DirectoryMonitor("C:\\eBusSuppliesTGX150AuditFiles\\Incoming",logger);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
