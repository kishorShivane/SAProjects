using System.Collections.Generic;
using System.Web.Mvc;

namespace Reports.Services.Models
{
    public class AuditStatusModel
    {
        public List<AuditStatus> AuditStatuses { get; set; } = new List<AuditStatus>();
        public List<SelectListItem> Reasons { get; set; } = new List<SelectListItem>();
    }
}
