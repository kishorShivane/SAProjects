﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace EBusFileUploaderService
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //string[] args = { };
            //if (Environment.UserInteractive)
            //{
            //    EbusUploader service1 = new EbusUploader();
            //    service1.TestStartupAndStop(args);
            //}
            //else
            //{
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new EbusUploader ()
                };
                ServiceBase.Run(ServicesToRun);
            //}

        }
    }
}
