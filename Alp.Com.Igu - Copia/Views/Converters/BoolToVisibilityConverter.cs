using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;

namespace Alp.Com.Igu.Views.Converters
{
    // Se il valore passato è true, ritona "Visible", altrimenti ritorna "Collapsed" (oppure Hidden se il ConverterParameter vale ShowOrHide)
    class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tipo_target = targetType.ToString();
            string tipo_ricevuto = value?.GetType()?.ToString();

            bool v = System.Convert.ToBoolean(value); // NB: Se value è null, System.Convert.ToBoolean restituisce false.

            if (parameter != null && parameter.ToString().Equals("ShowOrHide"))
            {
                return v ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            }
            else
            {
                return v ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
