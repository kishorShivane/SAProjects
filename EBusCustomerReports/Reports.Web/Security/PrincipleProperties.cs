namespace Helpers.Security
{
    public class PrincipleProperties
    {
        public PrincipleProperties()
        {
        }

        public PrincipleProperties(string data)
        {
            var d = data.Split('|');
            if (d.Length > 0)
            {
                CompanyName = d[0];
            }
            if (d.Length > 1)
            {
                ConnKey = d[1];
            }
        }

        public string CompanyName { get; set; }

        public string ConnKey { get; set; }

        public string Serialize()
        {
            return string.Concat(CompanyName, "|", ConnKey);
        }
    }
}
