using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EbusFileImporter.Helpers;
using EbusFileImporter.Enums;
using EbusFileImporter.Core;
using EbusFileImporter.Core.Interfaces;
using EbusFileImporter.Logger;

namespace EbusFileImporter.Monitors
{
    public class DirectoryMonitor
    {
        private FileSystemWatcher fileSystemWatcher;
        static ILogService logService = null;
        IImporter importerEngine;
        public DirectoryMonitor(string directoryToWatch, ILogService logger)
        {
            fileSystemWatcher = new FileSystemWatcher(directoryToWatch);
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.IncludeSubdirectories = true;

            // Instruct the file system watcher to call the FileCreated method
            // when there are files created at the folder.
            fileSystemWatcher.Created += new FileSystemEventHandler(FileCreated);
            logService = logger;

        } // end FileInputMonitor()

        private void FileCreated(Object sender, FileSystemEventArgs e)
        {
            if (Helper.IsXmlFile(e.Name))
            {
                logService.Info("Processing: XML file found");
                importerEngine = new XmlImporter(logService);
                importerEngine.ProcessFile(e.FullPath);
            }
            else
            {
                //ProcessFile(e.FullPath, FileType.CSV);
            }
        }
    }
}