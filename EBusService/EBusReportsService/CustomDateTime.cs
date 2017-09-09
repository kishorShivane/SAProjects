using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EBusReportsService
{
    public static class CustomDateTime
    {
        public static DateTime ConvertStringToDateSaFormat(string dateTime)
        {
            if (dateTime.Contains("/"))
            {
                dateTime = dateTime.Replace('/', '-');
            }
            const string pattern = "dd-MM-yyyy";
            DateTime parsedDate;
            DateTime.TryParseExact(dateTime, pattern, null,
                                   DateTimeStyles.None, out parsedDate);
            return parsedDate;
        }

        public static DateTime ConvertStringToDateDBFormat(string dateTime)
        {
            const string pattern = "MM-dd-yyyy";
            DateTime parsedDate;
            DateTime.TryParseExact(dateTime, pattern, null,
                                   DateTimeStyles.None, out parsedDate);
            return parsedDate;
        }
    }
}
