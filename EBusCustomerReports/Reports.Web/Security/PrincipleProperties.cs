using System;
using System.Collections.Generic;
using System.Linq;

namespace Helpers.Security
{
    public class PrincipleProperties
    {
        public PrincipleProperties()
        {
        }

        public PrincipleProperties(string data)
        {
            string[] d = data.Split('|');
            if (d.Length > 0)
            {
                CompanyName = d[0];
            }
            if (d.Length > 1)
            {
                ConnKey = d[1];
            }
            if (d.Length > 2)
            {
                AccessCodes = d[2].Split(',').ToList();
            }
            if (d.Length > 3)
            {
                RoleID = Convert.ToInt32(d[3]);
            }
            if (d.Length > 4)
            {
                WarningDate = Convert.ToDateTime(d[4]);
            }
            if (d.Length > 5)
            {
                LastDate = Convert.ToDateTime(d[5]);
            }
        }

        public string CompanyName { get; set; }

        public string ConnKey { get; set; }

        public List<string> AccessCodes { get; set; }

        public Nullable<DateTime> WarningDate { get; set; }
        public Nullable<DateTime> LastDate { get; set; }

        public int RoleID { get; set; }

        public string Serialize()
        {
            return string.Concat(CompanyName, "|", ConnKey, "|", string.Join(",", AccessCodes), "|", RoleID.ToString(), "|", WarningDate.Value.ToShortDateString(), "|", LastDate.Value.ToShortDateString());
        }
    }
}
