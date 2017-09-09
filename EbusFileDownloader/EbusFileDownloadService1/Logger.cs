using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;

namespace EbusFileDownloadService1
{
    public static class Logger
    {
        private static readonly ILog logger;

        static Logger()
        {
            logger = LogManager.GetLogger(typeof(Logger));
            XmlConfigurator.Configure();
        }

        public static void Log(string message)
        {
            logger.Info(message);
        }
    }
}
