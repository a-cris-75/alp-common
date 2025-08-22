using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;

namespace AlpTlc.Pre.Igu.Views.Converters
{
    class BoolToStringItaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            bool v = System.Convert.ToBoolean(value); // NB: Se value è null, System.Convert.ToBoolean restituisce false.

            string parameterString = parameter as string;
            if (!string.IsNullOrEmpty(parameterString))
            {
                string[] parameters = parameterString.Split('|');
                if (parameters.Length > 1)
                {
                    return v ? parameters[0] : parameters[1];
                }
                else if (parameters.Length > 0)
                {
                    return v ? parameters[0] : "";
                }
                else
                {
                    return v ? "Sì" : "No";
                }
            }
            else
            {
                return v ? "Sì" : "No";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
