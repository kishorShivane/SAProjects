using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbusFileImporter.Helpers
{
    public class Helper
    {

        public static bool IsXmlFile(string strToCheck)
        {
            bool result = false;

            if (strToCheck.ToUpper().Contains(".XML")) result = true;
            return result;
        }

        public static bool IsCsvFile(string strToCheck)
        {
            bool result = false;

            if (strToCheck.ToUpper().Contains(".CSV")) result = true;
            return result;
        }
    }
}
