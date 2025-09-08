using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace Alp.Com.Igu.Views.Converters
{
    class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool v = System.Convert.ToBoolean(value); // NB: Se value è null, System.Convert.ToBoolean restituisce false.

            Brush ColoreFalse = Brushes.Gray;
            Brush ColoreTrue = Brushes.White;

            try
            {
                if (parameter != null)
                {
                    string parameterString = parameter as string;
                    if (!string.IsNullOrEmpty(parameterString))
                    {
                        string[] parameters = parameterString.Split('|');
                        if (parameters.Length > 1)
                        {
                            ColoreFalse = (Brush)ColorConverter.ConvertFromString(parameters[0]);
                            ColoreTrue = (Brush)ColorConverter.ConvertFromString(parameters[1]);
                        }
                        else if (parameters.Length > 0)
                        {
                            ColoreFalse = (Brush)ColorConverter.ConvertFromString(parameters[0]);
                        }
                    }
                }
            }
            catch
            { }
                
            return v ? ColoreTrue : ColoreFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
