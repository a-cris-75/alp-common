using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Alp.Com.Igu.Views.Converters
{
    public static class ArrayToBitmapSource
    {

        // TODO vedere come fare a passare un logger generico anziché Serilog attraverso la dependency injection
        //Microsoft.Extensions.Logging.ILogger _logger;
        static Serilog.ILogger _logger = Serilog.Log.ForContext(typeof(ArrayToBitmapSource));

        public static BitmapSource ConvertGrayArrayToBitmapSource(byte[] buffer, int width, int height)
        {
            if (buffer.Length == 0)
            {
                _logger.Warning("ConvertGrayArrayToBitmapSource: la lunghezza del buffer è zero.");
                return null;
            }

            BitmapSource image = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Indexed8, //PixelFormats.Gray8,
                BitmapPalettes.Gray256, // null, 
                buffer,
                width);

            // Questo Freeze è importantante per la visualizzazione dell'immagine!!! ...:

            image.Freeze();

            return image;
        }

        public static BitmapSource ConvertColorBgrArrayToBitmapSource(byte[] buffer, int width, int height)
        {
            if (buffer.Length == 0)
            {
                _logger.Warning("ConvertArrayToBitmapSource: la lunghezza del buffer è zero.");
                return null;
            }

            BitmapSource image = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Bgr24,
                null,
                buffer,
                width * 3);

            // Questo Freeze è importantante per la visualizzazione dell'immagine!!! ...:

            image.Freeze();

            return image;
        }

        public static BitmapSource ConvertColorBgrArrayToBitmapSource2(byte[] buffer, int width, int height)
        {
            if (buffer.Length == 0)
            {
                _logger.Warning("ConvertArrayToBitmapSource: la lunghezza del buffer è zero.");
                return null;
            }

            BitmapSource image = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Bgr24,
                null,
                buffer,
                width);

            // Questo Freeze è importantante per la visualizzazione dell'immagine!!! ...:

            image.Freeze();

            return image;
        }

    }
}
