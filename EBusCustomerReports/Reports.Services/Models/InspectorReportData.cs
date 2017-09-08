using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class InspectorReportData
    {
        public DateTime stagetime { get; set; } //inspector  datetime
        public string journeyid {get;set;}

        public string routeId {get;set;}
        public string driverno {get;set;}
        public string drivername { get; set; }
        public string busid {get;set;}
        public Int32 inspectorno {get;set;}
        public string int4_dutyid {get;set;}
        public string int2_stageid {get;set;}
        public string StageName { get; set; }

        public string InsDate { get; set; }
        public string InsTime { get; set; }
    }
}
