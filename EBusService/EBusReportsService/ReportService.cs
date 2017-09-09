using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace EBusReportsService
{
    public partial class ReportService : ServiceBase
    {
        ProcessJob process = new ProcessJob();

        public ReportService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Helper.WriteToFile("Service started");
            Helper.CreateServiceFileDirectory(Constants.EBusReportServiceFilesPath);
            this.ScheduleService();
        }


       

        protected override void OnStop()
        {
            Helper.WriteToFile("Simple Service stopped {0}");
            this.Schedular.Dispose();
        }

        private Timer Schedular;

        public void ScheduleService()
        {
            try
            {
                Schedular = new Timer(new TimerCallback(SchedularCallback));
                string mode = Constants.Mode;
                Helper.WriteToFile("Simple Service Mode: " + mode + " {0}");

                //Set the Default Time.
                DateTime scheduledTime = DateTime.MinValue;

                if (mode == "DAILY")
                {
                    Helper.WriteToFile("Service Info: {0} " + "Daily report processing mode");
                    //Get the Scheduled Time from AppSettings.
                    scheduledTime = DateTime.Parse(Constants.ScheduledTime);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next day.
                        scheduledTime = scheduledTime.AddDays(1);
                    }
                }

                if (mode.ToUpper() == "INTERVAL")
                {
                    Helper.WriteToFile("Service Info: {0} " + "Interval report processing mode");
                    //Get the Interval in Minutes from AppSettings.
                    Int64 intervalMinutes = Convert.ToInt64(Constants.IntervalMinutes);

                    //Set the Scheduled Time by adding the Interval to Current Time.
                    scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next Interval.
                        scheduledTime = scheduledTime.AddMinutes(intervalMinutes);
                    }
                }

                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                string schedule = string.Format("{0} day(s) {1} hour(s) {2} minute(s) {3} seconds(s)", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

                Helper.WriteToFile("Service scheduled to run after: " + schedule + " {0}");

                //Get the difference in Minutes between the Scheduled and Current Time.
                Int64 dueTime = Convert.ToInt64(timeSpan.TotalMilliseconds);

                //Change the Timer's Due Time.
                Schedular.Change(dueTime, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Helper.WriteToFile("Service Error on: {0} " + ex.Message + ex.StackTrace);

                //Stop the Windows Service.
                using (System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController("SimpleService"))
                {
                    serviceController.Stop();
                }
            }
        }

        private void SchedularCallback(object e)
        {
            try
            {
                Helper.WriteToFile("Service Info: {0} " + "SchedularCallback report processing Started");
                var customers = Constants.EnablesCustomersConStr;
                var allCustomers = customers.Split(',');
                var attachmentFilePaths = new List<string>();
                foreach (var cust in allCustomers)
                {
                    attachmentFilePaths = new List<string>();
                    Helper.WriteToFile("Service Info: {0} " + "Processing report for " + cust + " Started");
                    if (!String.IsNullOrEmpty(cust))
                    {
                        attachmentFilePaths.Add(process.AuditCommunicationReport(cust.Trim()));
                        if (cust.Trim() == "atamelang70")
                        {
                            attachmentFilePaths.Add(process.ScheduledVsNoOperated(cust.Trim(), true));
                        }
                        else
                        {
                            attachmentFilePaths.Add(process.ScheduledVsNoOperated(cust.Trim(), false));
                        }
                        EmailHelper.SendEmail(cust, attachmentFilePaths);
                    }
                    Helper.WriteToFile("Service Info: {0} " + "Processing report for " + cust + " Completed");
                }

                this.ScheduleService();
            }
            catch (Exception ex)
            {
                Helper.WriteToFile("Service Error on: {0} " + ex.Message + ex.StackTrace);

                //Stop the Windows Service.
                using (System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController("SimpleService"))
                {
                    serviceController.Stop();
                }
            }
        }
    }
}
