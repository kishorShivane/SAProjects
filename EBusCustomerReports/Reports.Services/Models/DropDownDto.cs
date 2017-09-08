using System;
using System.Web.Mvc;

namespace Reports.Services.Models
{
    public class DropDownDto : SelectListItem
    {
        public int IntValue { set { Value = value.ToString(); } }
    }
}
