using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TcxDecode;

namespace TcxChart
{
    public class StringToPaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return convert(value, targetType);
        }

        private object convert(object value, Type targetType)
        { 
            if (targetType == typeof(string))
            {
                if (value is Pace pace)
                {
                    return pace.ToString();
                }
                return value?.ToString();
            }
            if (targetType == typeof(Pace))
            {
                if (value is string s)
                {
                    try
                    {
                        return Pace.Parse(s);
                    }
                    catch
                    {
                        return new Pace();
                    }
                }
                if (value is double d)
                {
                    return new Pace(d);
                }
                if (value is float f)
                {
                    return new Pace(f);
                }
                if (value is int i)
                {
                    return new Pace(i);
                }
                if (value is decimal dc)
                {
                    return new Pace((double)dc);
                }
                return new Pace();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return convert(value, targetType);
        }
    }
}
