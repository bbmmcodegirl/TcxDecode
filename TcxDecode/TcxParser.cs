using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TcxDecode
{
    public class TcxParser
    {
        public Exception Exception { get; private set; }

        public List<Activity> Parse(string fileName)
        {
            Exception = null;
            var file = fileName;
            if (!File.Exists(file))
            {
                file = $"{file}.tcx";
            }

            if (!File.Exists(file))
            {
                return null;
            }

            XDocument xDoc;
            try
            {
                var text = File.ReadAllText(file);
                xDoc = XDocument.Parse(text);
            }
            catch(Exception ex)
            {
                Exception = ex;
                return null;
            }

            var activityElements = xDoc.Descendants().Where(d => d.Name.LocalName == "Activity").ToList();
            var activities = activityElements.Select(e => Activity.Parse(e)).Where(t => t != null).ToList();
            return activities;
        }

        private static int US_LCID = 1033;

        public static bool ParseFloat(string s, out float f)
        {
            f = 0F;
            if (!string.IsNullOrWhiteSpace(s) && Single.TryParse(s, NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo(US_LCID), out Single fValue))
            {
                f = fValue;
                return true;
            }
            return false;
        }

        public static bool ParseInt(string s, out int n)
        {
            n = 0;
            if (!string.IsNullOrWhiteSpace(s) && Int32.TryParse(s, NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo(US_LCID), out Int32 nValue))
            {
                n = nValue;
                return true;
            }
            return false;
        }

        public static bool ParseDouble(string s, out Double d)
        {
            d = 0.0;
            if (!string.IsNullOrWhiteSpace(s) && Double.TryParse(s, NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo(US_LCID), out Double dValue))
            {
                d = dValue;
                return true;
            }
            return false;
        }
    }
}
