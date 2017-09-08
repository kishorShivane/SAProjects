using Reports.Services.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Services
{
    public class AssetMasterService : BaseServices
    {

        public List<Asset> GetAssetDetails(string conKey, string etmType = "", string etmSerialNumber = "")
        {
            var result = new List<Asset>();
            var myConnection = new SqlConnection(GetConnectionString(conKey));

            try
            {
                var cmd = new SqlCommand("eBusAsset_GetAssetDetails", myConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@ETMType", etmType));
                cmd.Parameters.Add(new SqlParameter("@ETMSerialNumber", etmSerialNumber));
                cmd.CommandTimeout = 500000;
                myConnection.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    var item = new Asset();
                    item.ID = Convert.ToInt32(dr["ID"]);
                    item.ETMSerialNumber = dr["ETMSerialNumber"].ToString();
                    item.ETMType = dr["ETMType"].ToString();
                    item.LastCommunicatedDate = Convert.ToDateTime(dr["LastCommunicatedDate"]);
                    result.Add(item);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return result.OrderBy(s => s.ID).ToList();
        }
    }
}
