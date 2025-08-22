﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;


namespace Alp.Com.Igu.Views.Converters
{
    class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, 
                              object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            int.TryParse(value.ToString(), out int valore);
            return valore==0 ? parameter : valore;
        }

    }
}
