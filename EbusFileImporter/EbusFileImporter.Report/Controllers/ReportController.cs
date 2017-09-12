using EbusFileImporter.Report.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EbusFileImporter.Report.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Report/

        public ActionResult Report()
        {
            var gridModels = new List<GridModel>();
            GridModel gridModel = null;
            var today = DateTime.Now;
            var thisYear = today.Year;
            var thisMonth = today.ToString("MMMM");
            var todayDay = today.ToString("dd");
            var yesterdayDate = today.AddDays(-1).ToString("dd");
            var files = DirSearch(ConfigurationManager.AppSettings["DirectoryPath"]);
            var prevCust = "";
            if (files.Any())
            {
                gridModel = new GridModel();
                files.OrderBy(x => x);
                files.ForEach(x =>
                {
                    var splitPath = x.Replace("\\\\", "\\").Split('\\');
                    if (prevCust != "" && prevCust != splitPath[3])
                    {
                        gridModels.Add(gridModel);
                        gridModel = new GridModel();
                    }
                    if (splitPath.Length >= 5)
                    {
                        gridModel.Customer = splitPath[3];
                        prevCust = gridModel.Customer;
                        gridModel.LastUpdated = today.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        if (splitPath[5] == thisYear.ToString() && splitPath[6] == thisMonth)
                        {
                            switch (splitPath[4])
                            {
                                case "Error":
                                    gridModel.ErrorCount += 1;
                                    break;
                                case "Out":
                                    if (splitPath[7] == todayDay)
                                    {
                                        gridModel.ImportedToday += 1;
                                    }
                                    else if (splitPath[7] == yesterdayDate)
                                    {
                                        gridModel.ImportedYesterday += 1;
                                    }
                                    break;
                                case "Duplicate":
                                    gridModel.DuplicateCount += 1;
                                    break;
                                case "DateProblem":
                                    gridModel.DateProblem += 1;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                });

                if (gridModel != null) gridModels.Add(gridModel);
            }
            return View(gridModels);
        }

        public List<string> DirSearch(string sDir)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d));
                }
            }
            catch (Exception)
            {
            }
            return files;
        }
    }
}
