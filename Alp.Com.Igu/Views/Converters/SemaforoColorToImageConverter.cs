using Alp.Com.DataAccessLayer.DataTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using AlpTlc.Domain.StatoMacchina;

namespace Alp.Com.Igu.Views.Converters
{
    public class SemaforoColorToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null)
            {
                Trace.TraceWarning("Attempted to convert value instead of SemaforoColor object in SemaforoColorToImageSourceConverter", value);
                return null;
            }

            SemaforoColor semaforoColor = (SemaforoColor)value;
            //ImageSource imageSource;
            //BitmapImage bitmapImage = null;
            //Uri uri = null;
            string percorsoImmagine = "";

            try
            {
                switch (semaforoColor)
                {
                    case SemaforoColor.Verde:
                        //uri = new Uri(@"/Images/SemaforoVerde.png", UriKind.RelativeOrAbsolute);
                        percorsoImmagine = @"/Images/SemaforoVerde.png";
                        break;
                    case SemaforoColor.Blu:
                        //uri = new Uri(@"/Images/SemaforoBlu.png", UriKind.RelativeOrAbsolute);
                        percorsoImmagine = @"/Images/SemaforoBlu.png";
                        break;
                    case SemaforoColor.Giallo:
                        //uri = new Uri(@"/Images/SemaforoGiallo.png", UriKind.RelativeOrAbsolute);
                        percorsoImmagine = @"/Images/SemaforoGiallo.png";
                        break;
                    case SemaforoColor.Rosso:
                        //uri = new Uri(@"/Images/SemaforoRosso.png", UriKind.RelativeOrAbsolute);
                        percorsoImmagine = @"/Images/SemaforoRosso.png";
                        break;
                    case SemaforoColor.Grigio:
                        //uri = new Uri(@"/Images/SemaforoGrigio.png", UriKind.RelativeOrAbsolute);
                        percorsoImmagine = @"/Images/SemaforoGrigio.png";
                        break;
                    default:
                        //uri = new Uri(@"/Images/SemaforoGrigio.png", UriKind.RelativeOrAbsolute);
                        percorsoImmagine = @"/Images/SemaforoGrigio.png";
                        break;
                }
                //if (uri != null)
                //    bitmapImage = new BitmapImage(uri);
            }
            catch (Exception excCatch)
            {
                //bitmapImage = null;
            }
            //imageSource = bitmapImage;

            //return imageSource; // NB: funziona ugualmente anche quando restituisce una ImageSource! (de.commentando quanto serve)
            return percorsoImmagine;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
