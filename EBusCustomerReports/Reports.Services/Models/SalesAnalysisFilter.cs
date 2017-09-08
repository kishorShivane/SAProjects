using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Reports.Services.Models
{
    public class CashVsSmartCardUsageByRouteFilter
    {
        public CashVsSmartCardUsageByRouteFilter()
        {
            Routes = new List<SelectListItem>();
        }

        public string[] RoutesSelected { get; set; }
        public List<SelectListItem> Routes { get; set; }

        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }

        public bool ExcelOrPDF { get; set; } 
    }

    public class CashVsSmartCardUsageByRouteData
    { 
        public string str_RouteID { get; set; }
        public string str50_RouteName { get; set; }

        public string ColorCodeTickets { get; set; }
        public string ColorCodePasses { get; set; }

        public decimal int4_JourneyTickets { get; set; }
        public decimal int4_JourneyTicketsPercent { get; set; }

        public decimal int4_JourneyPasses { get; set; }
        public decimal int4_JourneyPassesPercent { get; set; }

        public decimal int4_JourneyTransfer { get; set; }
        public decimal int4_JourneyTransferPercent { get; set; }

        public decimal TotalPassengers { get; set; }
    }
}
