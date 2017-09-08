using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Reports.Services.Models.RouteMaster
{
    public class SubRouteMaster
    {
        public SubRouteMaster()
        {
            Stages = new List<SelectListItem>();
            Contracts = new List<SelectListItem>();
        }

        public string MainRouteNumber { get; set; }
        public List<SelectListItem> Stages { get; set; }
        public List<SelectListItem> Contracts { get; set; }
        public string SubRouteName { get; set; }
        public string Direction { get; set; }
        public string ScheduledDistance { get; set; }
        public string DOTDistance { get; set; }
        public string DOTNumber { get; set; }
        public bool IsPosition { get; set; }
        public string Contract { get; set; }
        public string DestinationBlind { get; set; }
        public string SubRouteDetail { get; set; }
        public string StartStage { get; set; }
        public string EndStage { get; set; }
        public string SubRouteNumber { get; set; }
        public string ViaStage { get; set; }
    }
}