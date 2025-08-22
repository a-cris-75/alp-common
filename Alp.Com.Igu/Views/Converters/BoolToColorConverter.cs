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
    class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool v = System.Convert.ToBoolean(value); // NB: Se value è null, System.Convert.ToBoolean restituisce false.

            Color ColoreDafault = (Color)ColorConverter.ConvertFromString(parameter?.ToString());

            return v ? Colors.Red : ColoreDafault;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
