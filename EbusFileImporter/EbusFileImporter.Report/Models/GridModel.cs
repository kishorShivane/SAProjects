using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbusFileImporter.Report.Models
{
    public class GridModel
    {
        public string Customer { get; set; }
        public int ImportedToday { get; set; }
        public int ImportedYesterday { get; set; }
        public int ErrorCount { get; set; }
        public int DateProblem { get; set; }
        public int DuplicateCount { get; set; }
    }
}