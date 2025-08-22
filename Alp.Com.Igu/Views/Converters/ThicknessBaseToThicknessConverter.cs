using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Alp.Com.Igu.Views.Converters
{
    // Se il valore passato è true, ritona "Visible", altrimenti ritorna "Collapsed" (oppure Hidden se il ConverterParameter vale ShowOrHide)
    class ThicknessBaseToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tipo_target = targetType.ToString();
            string tipo_ricevuto = value?.GetType()?.ToString();

            Thickness v = (Thickness)value;

            if (parameter != null && parameter is Thickness)
            {
                Thickness p = (Thickness)parameter;

                return new Thickness(v.Left + p.Left, v.Top + p.Top, v.Right + p.Right, v.Bottom + p.Bottom);
            }
            else
            {
                return v;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
