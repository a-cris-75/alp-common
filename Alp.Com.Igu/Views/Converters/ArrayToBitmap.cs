using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media.Imaging;

// Questo viene dal Dis, precisamente:
// src\01\05\Dis\AlpTlc.Domain.Dis.Entity\Auxiliary\ArrayToBitmap.cs

namespace Alp.Com.Igu.Views.Converters
{
    public static class ArrayToBitmap
    {

        //public static Bitmap ConvertArrayToBitmap(byte[] buffer, int width, int height)
        //{
        //    Bitmap b = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

        //    ColorPalette ncp = b.Palette;
        //    for (int i = 0; i < 256; i++)
        //        ncp.Entries[i] = Color.FromArgb(255, i, i, i);
        //    b.Palette = ncp;

        //    Rectangle BoundsRect = new Rectangle(0, 0, width, height);
        //    BitmapData bmpData = b.LockBits(BoundsRect, ImageLockMode.WriteOnly, b.PixelFormat);

        //    IntPtr ptr = bmpData.Scan0;

        //    int bytes = bmpData.Stride * b.Height;
        //    var rgbValues = new byte[bytes];

        //    // fill in rgbValues, e.g. with a for loop over an input array

        //    Marshal.Copy(rgbValues, 0, ptr, bytes);
        //    b.UnlockBits(bmpData);

        //    return b;                                

        //}

        public static Bitmap ConvertGrayArrayToBitmap(byte[] buffer, int width, int height)
        {
            try
            {
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                                            ImageLockMode.WriteOnly, bitmap.PixelFormat);

                Marshal.Copy(buffer, 0, bitmapData.Scan0, buffer.Length);
                bitmap.UnlockBits(bitmapData);

                var pal = bitmap.Palette;
                for (int i = 0; i < 256; i++)
                    pal.Entries[i] = Color.FromArgb(255, i, i, i);
                bitmap.Palette = pal;

                return bitmap;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static Bitmap ConvertBgrArrayToBitmap(byte[] buffer, int width, int height)
        {
            try
            {
                Bitmap b = new Bitmap(width, height, PixelFormat.Format24bppRgb);

                Rectangle BoundsRect = new Rectangle(0, 0, width, height);
                BitmapData bmpData = b.LockBits(BoundsRect, ImageLockMode.WriteOnly, b.PixelFormat);

                IntPtr ptr = bmpData.Scan0;

                // add back dummy bytes between lines, make each line be a multiple of 4 bytes
                int skipByte = bmpData.Stride - width * 3;
                byte[] newBuff = new byte[buffer.Length + skipByte * height];
                for (int j = 0; j < height; j++)
                {
                    Buffer.BlockCopy(buffer, j * width * 3, newBuff, j * (width * 3 + skipByte), width * 3);
                }

                // fill in rgbValues
                Marshal.Copy(newBuff, 0, ptr, newBuff.Length);
                b.UnlockBits(bmpData);

                return b;

            }
            catch (Exception ex)
            {
                return null;
            }

        }

        //public static Bitmap Convert3ChannelsArrayToBitmap(byte[] buffer, int width, int height)
        //{
        //    try
        //    {
        //        unsafe
        //        {
        //            int unmapByes = Math.Abs(stride) - (width * 3);
        //            byte* _ptrR = (byte*)ptrR;
        //            byte* _ptrG = (byte*)ptrG;
        //            byte* _ptrB = (byte*)ptrB;
        //            BitmapSource bmpsrc = null;
        //            bmpsrc = BitmapSource.Create(width,
        //                                                  height,
        //                                                  96,
        //                                                  96,
        //                                                  PixelFormats.Bgr24,
        //                                                  null,
        //                                                  new byte[bytes],
        //                                                  stride);
        //            BitmapBuffer bitmapBuffer = new BitmapBuffer(bmpsrc);
        //            byte* buffer = (byte*)bitmapBuffer.BufferPointer;


        //            Parallel.For(0, width - 1, (offset) =>
        //                {
        //                    int i = offset * 3;
        //                    *(buffer + i) = *(_ptrB + offset);
        //                    *(buffer + i + 1) = *(_ptrG + offset);
        //                    *(buffer + i + 2) = *(_ptrR + offset);
        //                });
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }

        //}


    }
}
