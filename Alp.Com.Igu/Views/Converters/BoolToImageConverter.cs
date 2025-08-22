using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;

namespace Alp.Com.Igu.Views.Converters
{
    class BoolToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tipo_target = targetType.ToString();
            string tipo_ricevuto = value?.GetType()?.ToString();

            bool v = System.Convert.ToBoolean(value); // NB: Se value è null, System.Convert.ToBoolean restituisce false.

            string percorsoImmagine = "";

            if (parameter != null)
            {
                switch(parameter.ToString())
                {
                    case "Play":
                        percorsoImmagine = v ? @"/Images/PlayPauseStop1/play 1.png" : @"/Images/PlayPauseStop1/play 1 giallo.png";
                        break;

                    case "Pause":
                        percorsoImmagine = v ? @"/Images/PlayPauseStop1/pause 1.png" : @"/Images/PlayPauseStop1/pause 1 giallo.png";
                        break;

                    case "Stop":
                        percorsoImmagine = v ? @"/Images/PlayPauseStop1/stop 1.png" : @"/Images/PlayPauseStop1/stop 1 giallo.png";
                        break;

                    case "Timer":
                        percorsoImmagine = v ? @"/Images/PlayPauseStop/timer.png" : @"/Images/PlayPauseStop/timer giallo.png";
                        break;

                    case "TimerNo":
                        percorsoImmagine = v ? @"/Images/PlayPauseStop/timer_no.png" : @"/Images/PlayPauseStop/timer_no giallo.png";
                        break;
                }
            }

            return percorsoImmagine;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

    }
}
