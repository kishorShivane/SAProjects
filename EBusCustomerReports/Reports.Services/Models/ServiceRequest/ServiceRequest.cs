using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reports.Services.Models.ServiceRequest
{
    public class ServiceRequest
    {
        public ServiceRequest()
        {
            RequestTypes = new List<SelectListItem>();
            RequestStatuss = new List<SelectListItem>();
        }

        public int ID { get; set; }
        public string ServiceRequestID { get; set; }
        public string RequestStatus { get; set; }
        public string RequestType { get; set; }
        public int RequestTypeID { get; set; }
        public string Priority { get; set; }
        public string Email { get; set; }
        public string Comments { get; set; }
        public string AdminComments { get; set; }
        public int RequestStatusID { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public List<string> AttachmentPath { get; set; }
        public List<SelectListItem> RequestStatuss { get; set; }
        public List<SelectListItem> RequestTypes { get; set; }
        public bool IsSpecialUser { get; set; }
    }

    public class ServiceRequestAudit
    {
        public string ServiceRequestID { get; set; }
        public string Descrption { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
    }
}
