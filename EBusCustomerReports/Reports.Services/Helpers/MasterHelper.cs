using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Reports.Services.Helpers
{
    public class MasterHelper
    {
        public static string GeneratePIN(string PINSeed, string DriverNumber)
        {
            string result = "";
            int temp;

            if (DriverNumber.Length < 6)
            {
                DriverNumber = DriverNumber.PadLeft(6, '0'); //RIGHT HERE!!!
            }

            if (PINSeed.Length < 4)
            {
                PINSeed = PINSeed.PadLeft(4, '0'); //RIGHT HERE!!!
            }

            var charPINSeed = PINSeed.ToCharArray();
            var charDriverNumber = DriverNumber.ToCharArray();

            for (var i = 0; i < charDriverNumber.Length; i++)
            {
                if ((i + charPINSeed.Length) <= charDriverNumber.Length + 1)
                {
                    temp = 0;
                    if (i + 3 != charDriverNumber.Length)
                    {
                        temp = temp + (charPINSeed[0] ^ charDriverNumber[i]) + (charPINSeed[1] ^ charDriverNumber[i + 1]) + (charPINSeed[2] ^ charDriverNumber[i + 2]) + (charPINSeed[3] ^ charDriverNumber[i + 3]);
                    }
                    else
                    {
                        temp = temp + (charPINSeed[0] ^ charDriverNumber[i]) + (charPINSeed[1] ^ charDriverNumber[i + 1]) + (charPINSeed[2] ^ charDriverNumber[i + 2]) + (charPINSeed[3] ^ charDriverNumber[0]);
                    }
                    temp = temp & 15;
                    if (temp > 9) { temp = temp - 9; }
                    result = result + temp.ToString();
                }
            }

            return result;
        }

        public static DataSet FillDefaultValuesForEmptyDataSet(DataSet result)
        {
            if (result.Tables[0].Rows.Count == 0)
            {
                DataRow dr = result.Tables[0].NewRow();
                result.Tables[0].Rows.Add(dr);
            }
            return result;
        }
    }
}