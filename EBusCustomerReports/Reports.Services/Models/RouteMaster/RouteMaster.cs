using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Reports.Services.Models.RouteMaster
{
    public class RouteMaster
    {
        public RouteMaster()
        {
            Routes = new List<SelectListItem>();
            RouteStages = new List<RouteStage>();
            SubRouteMaster = new SubRouteMaster();
        }
        public List<SelectListItem> Routes { get; set; }
        public string RouteDetail { get; set; }
        public string RouteNumber { get; set; }
        public List<RouteStage> RouteStages { get; set; }
        public SubRouteMaster SubRouteMaster { get; set; }
    }
}