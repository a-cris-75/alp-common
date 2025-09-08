using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics;

namespace Alp.Com.Igu.Views.Converters
{
    class ElementSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double percentage = double.Parse(parameter.ToString(), culture);

            return double.Parse(value.ToString()) * percentage;
            //Debug.WriteLine("value = " + value + " - percentage = " + percentage);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
