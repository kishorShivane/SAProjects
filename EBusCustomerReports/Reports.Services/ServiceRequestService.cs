using Reports.Services.Helpers;
using Reports.Services.Models;
using Reports.Services.Models.ServiceRequest;
using Reports.Services.Models.StaffMaster;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace Reports.Services
{
    public class ServiceRequestService : BaseServices
    {
        public IEnumerable<SelectListItem> GetRequestType(string conKey)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "-Select Request Type-", Value = "0", Selected = true });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusServiceRequest_GetRequestType", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Text = dr["Description"].ToString();
                    item.Value = dr["ID"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.OrderBy(s => s.Value).ToList();
        }

        public IEnumerable<SelectListItem> GetRequestStatus(string conKey)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "-Select Request Status-", Value = "0", Selected = true });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusServiceRequest_GetRequestStatus", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Text = dr["Description"].ToString();
                    item.Value = dr["ID"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.OrderBy(s => s.Value).ToList();
        }

        public IEnumerable<ServiceRequest> GetServiceRequests(string conKey, string status, string userid)
        {
            var result = new List<ServiceRequest>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusServiceRequest_GetServiceRequests", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@RequestStatusID", status == "" ? "0" : status));
                cmd.Parameters.Add(new SqlParameter("@UserID", userid));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new ServiceRequest();
                    item.ID = Convert.ToInt32(dr["ID"]);
                    item.ServiceRequestID = dr["ServiceRequestID"].ToString();
                    item.RequestType = dr["RequestType"].ToString();
                    item.Priority = dr["Priority"].ToString();
                    item.Email = dr["Email"].ToString();
                    item.Comments = dr["Comments"].ToString();
                    item.AdminComments = dr["AdminComments"].ToString();
                    item.RequestStatus = dr["RequestStatus"].ToString();
                    item.LastModifiedBy = dr["LastModifiedBy"].ToString();
                    item.LastModifiedDate = Convert.ToDateTime(dr["LastModifiedDate"]);
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.ToList();
        }

        public IEnumerable<ServiceRequestAudit> GetServiceRequestAudit(string conKey, string serviceRequestID)
        {
            var result = new List<ServiceRequestAudit>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusServiceRequest_GetServiceRequestsAudit", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@ServiceRequestID", serviceRequestID));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new ServiceRequestAudit();
                    item.ServiceRequestID = dr["ServiceRequestID"].ToString();
                    item.Descrption = dr["Description"].ToString();
                    item.UpdatedDateTime = Convert.ToDateTime(dr["UpdatedDateTime"]);
                    item.UpdatedBy = dr["UpdatedBy"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.ToList();
        }
        public int InsertOrUpdateServiceRequest(ServiceRequest ServiceRequest, string conKey, string userid)
        {
            var Status = 1;
            var serviceRequestID = 0;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusServiceRequest_InsertOrUpdateServiceRequest", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@ID", ServiceRequest.ID));
                cmd.Parameters.Add(new SqlParameter("@ServiceRequestID", ServiceRequest.ServiceRequestID));
                cmd.Parameters.Add(new SqlParameter("@RequestTypeID", ServiceRequest.RequestTypeID));
                cmd.Parameters.Add(new SqlParameter("@Priority", ServiceRequest.Priority));
                cmd.Parameters.Add(new SqlParameter("@Email", ServiceRequest.Email));
                cmd.Parameters.Add(new SqlParameter("@Comments", ServiceRequest.Comments == null ? "" : ServiceRequest.Comments));
                cmd.Parameters.Add(new SqlParameter("@AdminComments", ServiceRequest.AdminComments == null ? "" : ServiceRequest.AdminComments));
                cmd.Parameters.Add(new SqlParameter("@RequestStatusID", ServiceRequest.RequestStatusID));
                cmd.Parameters.Add(new SqlParameter("@LastModifiedBy", userid));
                cmd.Parameters.Add(new SqlParameter("@LastModifiedDate", DateTime.Now));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    serviceRequestID = Convert.ToInt32(dr["serviceRequestID"]);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                myConnection.Close();
            }

            return serviceRequestID;
        }
        public bool DeleteServiceRequest(string StaffTypeID, string conKey)
        {
            var result = false;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusStaffTypeEditor_DeleteStaffType", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@StaffTypeID", StaffTypeID));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                if (cmd.ExecuteScalar().ToString() == "1")
                {
                    result = true;
                }
            }
            finally
            {
                myConnection.Close();
            }
            return result;
        }
    }
}
