using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace EbusFileImporter.Service
{
    public partial class Service1 : ServiceBase
    {
        string importerPath = ConfigurationManager.AppSettings["ImporterPath"];
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerAsync();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(importerPath);
            p.Start();
            p.WaitForExit();
            base.Stop();
        }

        //protected override void OnStop()
        //{
        //    p.Kill();
        //}
    }
}
