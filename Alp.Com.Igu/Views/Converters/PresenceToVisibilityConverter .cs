using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;

namespace Alp.Com.Igu.Views.Converters
{
    class PresenceToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tipo_target = targetType.ToString();
            string tipo_ricevuto = value?.GetType()?.ToString();

            bool assente = (value == null || value is string && string.IsNullOrEmpty(value as string));

            if (parameter != null && parameter.ToString().Equals("ShowOrHide"))
            {
                return (!assente) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            }
            else
            {
                return (!assente) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
