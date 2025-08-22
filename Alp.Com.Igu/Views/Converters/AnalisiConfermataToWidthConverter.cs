using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;

namespace Alp.Com.Igu.Views.Converters
{
    class AnalisiConfermataToWidthConverter : IValueConverter
    {
        // Se l'analisi è confermata, la, celletta colorata è più larga
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tipo_target = targetType.ToString();
            string tipo_ricevuto = value.GetType().ToString();

            bool v = System.Convert.ToBoolean(value);
            return v ? 500 : 20; // 500 = infinito. TODO Sarebbe bene si usasse una percentuale della larghezza (vedi ElementSizeConverter, ma occorrerebbe aggiungere la condizione per ci non è al 100%)
        }

        public object ConvertBack(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
