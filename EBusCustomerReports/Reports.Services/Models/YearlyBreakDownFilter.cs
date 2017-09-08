using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Reports.Services.Models
{
    public class MonthyRevenueDataGraph
    {
        public List<MonthyRevenueData> SellerRevMonth { get; set; }
        public List<MonthyRevenueData> DriverRevMonth { get; set; }
        public decimal SellerRevMonthYear { get; set; }
        public decimal DriverRevMonthYear { get; set; }
    }

    public class MonthyRevenueData
    {
        public int MonthNum { get; set; }
        public string MonthName { get; set; }

        public int int4_DutyID { get; set; }

        public decimal int4_DutyRevenue { get; set; }
        public decimal int4_DutyNonRevenue { get; set; }
    }

    public class MonthyRevenueFilter
    {

        public MonthyRevenueFilter()
        {
            Months = new YearlyBreakDownFilter().GetMonths();
            Years = new YearlyBreakDownFilter().GetYears();
            Years.OrderBy(s => Convert.ToInt32(s.Value)).LastOrDefault().Selected = true;
        }

        public string[] MonthsSelected { get; set; }
        public List<SelectListItem> Months { get; set; }

        public string YearsSelected { get; set; }
        public List<SelectListItem> Years { get; set; }
    }

    public class YearlyBreakDownFilter
    {
        public YearlyBreakDownFilter()
        {
            Classes = new List<SelectListItem>();
            ExcelOrPDF = true; //PDF=true, EXCEL=false
            FromMonths = GetMonths();
            ToMonths = GetMonths();
            FromYears = GetYears();
            ToYears = GetYears();
            ToYears.Where(s => s.Value.Equals(DateTime.Now.Year.ToString())).First().Selected = true;
            RoutesList = new List<SelectListItem>();
        }

        public string[] RoutesSelected { get; set; }
        public List<SelectListItem> RoutesList { get; set; }

        public string[] ClassesSelected { get; set; }
        public List<SelectListItem> Classes { get; set; }

        public string[] FromMonthSelected { get; set; }
        public List<SelectListItem> FromMonths { get; set; }

        public string[] ToMonthSelected { get; set; }
        public List<SelectListItem> ToMonths { get; set; }

        public string FromYearSelected { get; set; }
        public List<SelectListItem> FromYears { get; set; }

        public string ToYearSelected { get; set; }
        public List<SelectListItem> ToYears { get; set; }

        public bool ExcelOrPDF { get; set; }

        public List<SelectListItem> GetMonths()
        {
            var months = new List<SelectListItem>();
            months.Add(new SelectListItem { Selected = false, Text = "Jan", Value = "1" });
            months.Add(new SelectListItem { Selected = false, Text = "Feb", Value = "2" });
            months.Add(new SelectListItem { Selected = false, Text = "Mar", Value = "3" });
            months.Add(new SelectListItem { Selected = false, Text = "Apr", Value = "4" });
            months.Add(new SelectListItem { Selected = false, Text = "May", Value = "5" });
            months.Add(new SelectListItem { Selected = false, Text = "Jun", Value = "6" });
            months.Add(new SelectListItem { Selected = false, Text = "Jul", Value = "7" });
            months.Add(new SelectListItem { Selected = false, Text = "Aug", Value = "8" });
            months.Add(new SelectListItem { Selected = false, Text = "Sep", Value = "9" });
            months.Add(new SelectListItem { Selected = false, Text = "Oct", Value = "10" });
            months.Add(new SelectListItem { Selected = false, Text = "Nov", Value = "11" });
            months.Add(new SelectListItem { Selected = false, Text = "Dec", Value = "12" });
            return months;
        }

        public List<SelectListItem> GetYears()
        {
            var years = new List<SelectListItem>();

            var fromYear = Convert.ToInt32(ConfigurationManager.AppSettings.Get("DataAvailableFrom"));

            for (int i = fromYear; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Selected = false, Text = i.ToString(), Value = i.ToString() });
            }
            return years;
        }
    }
}
