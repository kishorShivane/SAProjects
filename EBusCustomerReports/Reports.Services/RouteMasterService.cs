using Reports.Services.Models;
using Reports.Services.Models.RouteMaster;
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
    public class RouteMasterService : BaseServices
    {
        public IEnumerable<SelectListItem> GetRoutes(string conKey)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "--", Value = "0" });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusRouteMaster_GetRoutes", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Text = dr["RouteName"].ToString();
                    item.Value = dr["RouteNumber"].ToString(); 
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.OrderBy(s => s.Text).ToList();
        }

        public IEnumerable<SelectListItem> GetStages(string conKey, string routeNumber)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "--", Value = "0" });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusRouteMaster_GetStages", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@RouteNumber", routeNumber == "0" ? "" : routeNumber));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Text = dr["StageName"].ToString();
                    item.Value = dr["StageNumber"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.ToList();
        }

        public IEnumerable<SelectListItem> GetContracts(string conKey)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "--", Value = "0" });
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusRouteMaster_GetContracts", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SelectListItem();
                    item.Text = dr["ContractRef"].ToString().Trim();
                    item.Value = dr["ContractID"].ToString().Trim();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.OrderBy(s => s.Text).ToList();
        }

        public IEnumerable<RouteMaster> GetMainRoutes(string conKey, string routeNumber)
        {
            var result = new List<RouteMaster>();
            var completeResult = new List<RouteMaster>();
            var stages = new List<RouteStage>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusRouteMaster_GetMainRoutes", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@RouteNumber", routeNumber == "0" ? "" : routeNumber));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new RouteMaster();
                    item.RouteDetail = dr["RouteDetail"].ToString();
                    item.RouteNumber = dr["RouteNumber"].ToString();
                    result.Add(item);
                }
                dr.NextResult();
                while (dr.Read())
                {
                    var item = new RouteStage();
                    item.RouteNumber = dr["RouteNumber"].ToString();
                    item.Order = dr["OrderNo"].ToString();
                    item.Stage = dr["StageNumber"].ToString();
                    item.StageName = dr["StageName"].ToString();
                    stages.Add(item);
                }

                foreach (var route in result)
                {
                    route.RouteStages.AddRange(stages.Where(x => x.RouteNumber.Equals(route.RouteNumber)).ToList());
                    completeResult.Add(route);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return completeResult.ToList();
        }

        public IEnumerable<SubRouteMaster> GetSubRoutesForRoutes(string conKey, string routeNumber)
        {
            var result = new List<SubRouteMaster>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusRouteMaster_GetSubRoutesForRoute", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@RouteNumber", routeNumber == "0" ? "" : routeNumber));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new SubRouteMaster();
                    item.MainRouteNumber = dr["MainRouteNumber"].ToString();
                    item.SubRouteNumber = dr["SubRouteNumber"].ToString();
                    item.StartStage = dr["StartStage"].ToString();
                    item.EndStage = dr["EndStage"].ToString();
                    item.SubRouteDetail = dr["SubRouteDetail"].ToString();
                    item.SubRouteName = dr["RouteName"].ToString();
                    item.Direction = dr["Direction"].ToString();
                    item.ScheduledDistance = dr["ScheduledDistance"].ToString();
                    item.DOTDistance = dr["DOTDistance"].ToString();
                    item.DOTNumber = dr["DOTSubRouteNumber"].ToString();
                    item.IsPosition = Convert.ToBoolean(dr["Position"]);
                    item.Contract = dr["Contract"].ToString();
                    item.DestinationBlind = dr["DestinationBlind"].ToString();
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.ToList();
        }

        public int InsertOrUpdateSubRoute(SubRouteMaster SubRouteMaster, string conKey)
        {
            var Status = 1;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusRouteMaster_InsertOrUpdateSubRoute", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@MainRouteNumber", SubRouteMaster.MainRouteNumber));
                cmd.Parameters.Add(new SqlParameter("@SubRouteNumber", SubRouteMaster.SubRouteNumber.PadLeft(4,'0')));
                cmd.Parameters.Add(new SqlParameter("@StartStage", SubRouteMaster.StartStage));
                cmd.Parameters.Add(new SqlParameter("@SubRouteName", SubRouteMaster.SubRouteName));
                cmd.Parameters.Add(new SqlParameter("@EndStage", SubRouteMaster.EndStage));
                cmd.Parameters.Add(new SqlParameter("@Direction", SubRouteMaster.Direction == "IN" ? true : false));
                cmd.Parameters.Add(new SqlParameter("@ScheduledDistance", SubRouteMaster.ScheduledDistance));
                cmd.Parameters.Add(new SqlParameter("@DOTDistance", SubRouteMaster.DOTDistance));
                cmd.Parameters.Add(new SqlParameter("@DOTNumber", SubRouteMaster.DOTNumber));
                cmd.Parameters.Add(new SqlParameter("@DestinationBlind", SubRouteMaster.DestinationBlind));
                cmd.Parameters.Add(new SqlParameter("@Contract", SubRouteMaster.Contract));
                cmd.Parameters.Add(new SqlParameter("@IsPosition", SubRouteMaster.IsPosition));

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

        public bool DeleteSubRoute(string subRouteNumber,string routeNumber, string conKey)
        {
            var result = false;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusRouteMaster_DeleteSubRoute", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@SubRouteNumber", subRouteNumber));
                cmd.Parameters.Add(new SqlParameter("@RouteNumber", routeNumber));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                if (cmd.ExecuteNonQuery() > 0)
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

        public int GetLastSubRouteNumber(string routeNumber, string conKey)
        {
            var result = 1;
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusRouteMaster_GetLastSubRouteNumber", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@RouteNumber", routeNumber));

                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    result = Convert.ToInt32(dr["result"]);
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
