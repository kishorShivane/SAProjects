using Reports.Services.Helpers;
using Reports.Services.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reports.Services
{
    public class UserService : BaseServices
    {
        //staff
        public StaffEditorViewModel GetAllStaffDetails(string connKey)
        {
            var data = new StaffEditorViewModel();

            var res = new List<StaffEditor>();

            var allLocs = GetAllLocationGroups(connKey);
            var allSts = GetAllStaffTypes(connKey);

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = @"select s.int4_StaffID,s.str50_StaffName,s.bit_InUse,s.int4_StaffTypeID,
                                    st.str25_Description,L.str50_LocationGroupName,
                                    s.str4_LocationCode,s.SerialNumber from staff s
                                    left join Stafftype st on s.int4_StaffTypeID = st.int4_StaffTypeID
                                    left join Locationgroup L on s.str4_LocationCode = L.str4_LocationCode";

                cmd.CommandTimeout = 500000;
                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new StaffEditor();
                    obj.Locations = allLocs;
                    obj.StaffTypes = allSts;

                    if (dr["int4_StaffID"] != null && dr["int4_StaffID"].ToString() != string.Empty)
                    {
                        obj.int4_StaffID = Convert.ToInt32(dr["int4_StaffID"].ToString());
                    }

                    if (dr["int4_StaffTypeID"] != null && dr["int4_StaffTypeID"].ToString() != string.Empty)
                    {
                        obj.int4_StaffTypeID = Convert.ToInt32(dr["int4_StaffTypeID"].ToString());
                    }

                    if (dr["str25_Description"] != null && dr["str25_Description"].ToString() != string.Empty)
                    {
                        obj.StaffTypeSelected = dr["str25_Description"].ToString();
                    }

                    if (dr["str50_StaffName"] != null && dr["str50_StaffName"].ToString() != string.Empty)
                    {
                        obj.str50_StaffName = dr["str50_StaffName"].ToString().Trim();
                    }

                    //if (dr["str4_LocationCode"] != null && dr["str4_LocationCode"].ToString() != string.Empty)
                    //{
                    //    obj.LocationSelected = dr["str4_LocationCode"].ToString().Trim();
                    //}

                    if (dr["str50_LocationGroupName"] != null && dr["str50_LocationGroupName"].ToString() != string.Empty)
                    {
                        obj.LocationSelected = dr["str50_LocationGroupName"].ToString().Trim();
                    }

                    if (dr["SerialNumber"] != null && dr["SerialNumber"].ToString() != string.Empty)
                    {
                        obj.SerialNumber = dr["SerialNumber"].ToString().Trim();
                    }

                    if (dr["bit_InUse"] != null && dr["bit_InUse"].ToString() != string.Empty)
                    {
                        obj.bit_InUse = Convert.ToBoolean(dr["bit_InUse"].ToString());
                    }

                    res.Add(obj);
                }
            }

            finally
            {
                myConnection.Close();
            }

            data.StaffList = res;
            data.Locations = allLocs;
            data.StaffTypes = allSts;

            return data;
        }

        public void InserNewStaffDetails(StaffEditor staff, string connKey)
        {

        }

        public bool DeleteStaffDetails(StaffEditor staff, string connKey)
        {
            return false;
        }

        //location
        public List<SelectListItem> GetAllLocationGroups(string connKey)
        {
            var res = new List<SelectListItem>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "select str4_LocationCode,str50_LocationGroupName from Locationgroup";
                cmd.CommandTimeout = 500000;
                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();

                    if (dr["str4_LocationCode"] != null && dr["str4_LocationCode"].ToString() != string.Empty)
                    {
                        obj.Value = dr["str4_LocationCode"].ToString().Trim();
                    }

                    if (dr["str50_LocationGroupName"] != null && dr["str50_LocationGroupName"].ToString() != string.Empty)
                    {
                        obj.Text = dr["str50_LocationGroupName"].ToString().Trim();
                    }
                    res.Add(obj);
                }
            }

            finally
            {
                myConnection.Close();
            }

            return res;
        }

        public void InserNewLocation(StaffEditor staff, string connKey)
        {

        }

        public bool DeleteLocation(StaffEditor staff, string connKey)
        {
            return false;
        }

        //staff type
        public List<SelectListItem> GetAllStaffTypes(string connKey)
        {
            var res = new List<SelectListItem>();

            var myConnection = new SqlConnection(GetConnectionString(connKey));

            try
            {
                var cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    Connection = myConnection
                };

                cmd.CommandText = "select int4_StaffTypeID,str25_Description from stafftype";
                cmd.CommandTimeout = 500000;
                myConnection.Open();

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var obj = new SelectListItem();

                    if (dr["int4_StaffTypeID"] != null && dr["int4_StaffTypeID"].ToString() != string.Empty)
                    {
                        obj.Value = dr["int4_StaffTypeID"].ToString().Trim();
                    }

                    if (dr["str25_Description"] != null && dr["str25_Description"].ToString() != string.Empty)
                    {
                        obj.Text = dr["str25_Description"].ToString().Trim();
                    }
                    res.Add(obj);
                }
            }

            finally
            {
                myConnection.Close();
            }

            return res;
        }

        public void InserNewStaffType(StaffEditor staff, string connKey)
        {

        }

        public bool DeleteStaffType(StaffEditor staff, string connKey)
        {
            return false;
        }
    }
}
