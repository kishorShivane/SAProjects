using Reports.Services.Helpers;
using Reports.Services.Models;
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
    public class StaffMasterService : BaseServices
    {
        public IEnumerable<SelectListItem> GetStaffType(string conKey)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "-Select Staff Type-", Value = "0",Selected=true });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusStaffMaster_GetStaffType", myConnection)
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

            return result.OrderBy(s => s.Text).ToList();
        }
        public IEnumerable<SelectListItem> GetLocations(string conKey)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "-Select Location-", Value = "0", Selected = true });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusStaffMaster_GetLocations", myConnection)
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

            return result.OrderBy(s => s.Text).ToList();
        }
        public IEnumerable<StaffMaster> GetStaffs(string conKey, string staffName, string staffNumber, string status, string pinSeed, string staffType, string location)
        {
            var result = new List<StaffMaster>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusStaffMaster_GetStaffDetails", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@StaffName", staffName == "" ? "" : staffName));
                cmd.Parameters.Add(new SqlParameter("@StaffNumber", staffNumber == "" ? "0" : staffNumber));
                cmd.Parameters.Add(new SqlParameter("@StaffType", staffType == "" ? "0" : staffType));
                cmd.Parameters.Add(new SqlParameter("@Location", location == "" ? "0" : location));
                cmd.Parameters.Add(new SqlParameter("@Status", status == "" ? "Active" : status));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new StaffMaster();
                    item.StaffNumber = dr["StaffNumber"].ToString();
                    item.StaffName = dr["StaffName"].ToString();
                    item.StaffTypeID = dr["StaffTypeID"].ToString();
                    item.StaffType = dr["StaffType"].ToString();
                    item.PIN = MasterHelper.GeneratePIN(pinSeed, item.StaffNumber);
                    item.IsSpecialStaff = dr["StaffSubTypeID"].ToString() == "1" ? true : false;
                    item.LocationCode = dr["LocationCode"].ToString();
                    item.Status = Convert.ToBoolean(dr["Status"]);
                    item.SerialNumber = dr["SerialNumber"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.ToList();
        }
        public int InsertOrUpdateStaff(StaffMaster StaffMaster, string conKey, bool isAdd = true)
        {
            var Status = 1;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusStaffMaster_InsertOrUpdateStaff", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@StaffNumber", StaffMaster.StaffNumber));
                cmd.Parameters.Add(new SqlParameter("@StaffName", StaffMaster.StaffName));
                cmd.Parameters.Add(new SqlParameter("@StaffTypeID", StaffMaster.StaffType));
                cmd.Parameters.Add(new SqlParameter("@LocationCode", StaffMaster.LocationCode));
                cmd.Parameters.Add(new SqlParameter("@Status", StaffMaster.Status));
                cmd.Parameters.Add(new SqlParameter("@SerialNumber", StaffMaster.SerialNumber));
                cmd.Parameters.Add(new SqlParameter("@SpecialStaff", StaffMaster.IsSpecialStaff ? 1 : 0));
                cmd.Parameters.Add(new SqlParameter("@IsAdd", isAdd));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    Status = Convert.ToInt32(dr["Status"]);
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

            return Status;
        }
        public bool DeleteStaffType(string StaffTypeID, string conKey)
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
        public int InsertOrUpdateStaffType(string staffType, int staffTypeID, string isAdd, string conKey)
        {
            var Status = 1;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusStaffTypeEditor_InsertOrUpdateStaffType", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@StaffTypeID", staffTypeID));
                cmd.Parameters.Add(new SqlParameter("@StaffType", staffType));
                cmd.Parameters.Add(new SqlParameter("@IsAdd", isAdd));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    Status = Convert.ToInt32(dr["Status"]);
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

            return Status;
        }
        public bool DeleteLocation(string LocationCode, string conKey)
        {
            var result = false;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusLocationEditor_DeleteLocation", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@LocationCode", LocationCode));

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
        public int InsertOrUpdateLocation(string locationCode, string locationName,string isAdd, string conKey)
        {
            var Status = 1;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusLocationEditor_InsertOrUpdateLocation", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@LocationCode", locationCode.PadLeft(4,'0')));
                cmd.Parameters.Add(new SqlParameter("@LocationName", locationName));
                cmd.Parameters.Add(new SqlParameter("@IsAdd", isAdd));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    Status = Convert.ToInt32(dr["Status"]);
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

            return Status;
        }
    }
}
