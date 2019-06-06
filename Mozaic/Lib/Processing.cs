using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Lib
{
    public static class Processing
    {
        public static Bitmap ChangePixelFormat(Bitmap img, PixelFormat frmt)
        {
            var output = new Bitmap(img.Width, img.Height, frmt);
            var temp = new Bitmap(img);
            output = temp.Clone(new Rectangle(0, 0, temp.Width, temp.Height), frmt);
            temp.Dispose();
            return output;
        }


        public static Bitmap MakeSquare(Bitmap img)
        {
            Rectangle rect;

            if (img.Width == img.Height) return img;
            else if (img.Width > img.Height)
            {
                rect = new Rectangle((int)((img.Width - img.Height) / 2), 0, img.Height, img.Height);
            }
            else
            {
                rect = new Rectangle(0, (int)((img.Height - img.Width) / 2), img.Width, img.Width);
            }

            return img.Clone(rect, img.PixelFormat);
        }


        public static Bitmap Resize(Bitmap img, int width, int height)
        {
            var destImage = new Bitmap(width, height, img.PixelFormat);

            var destRect = new Rectangle(0, 0, width, height);
            destImage.SetResolution(img.HorizontalResolution, img.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(img, destRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }


        public static byte[,,] BitmapToRGBArray(Bitmap img)
        {
            var arr = new byte[img.Width, img.Height, 3];

            var imgData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
            var ptr = imgData.Scan0;
            int imgBytes = Math.Abs(imgData.Stride) * img.Height;
            byte[] RGB = new byte[imgBytes];
            Marshal.Copy(ptr, RGB, 0, imgBytes);

            int index;
            for (var j = 0; j < img.Height; j++)
            {
                for (var i = 0; i < img.Width; i++)
                {
                    index = j * img.Width * 4 + i * 4;
                    arr[i, j, 0] = RGB[index + 2];
                    arr[i, j, 1] = RGB[index + 1];
                    arr[i, j, 2] = RGB[index];
                }
            }
            img.UnlockBits(imgData);
            return arr;
        }


        public static Bitmap RGBArrayToBitmap(byte[,,] arr)
        {
            int width = arr.GetLength(0), height = arr.GetLength(1);
            var img = new Bitmap(width, height, PixelFormat.Format32bppRgb);

            var imgData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, img.PixelFormat);
            var ptr = imgData.Scan0;
            int imgBytes = Math.Abs(imgData.Stride) * img.Height;
            byte[] RGB = new byte[imgBytes];

            int index;
            for (var j = 0; j < img.Height; j++)
            {
                for (var i = 0; i < img.Width; i++)
                {
                    index = j * img.Width * 4 + i * 4;
                    RGB[index + 2] = arr[i, j, 0];
                    RGB[index + 1] = arr[i, j, 1];
                    RGB[index] = arr[i, j, 2];
                }
            }

            Marshal.Copy(RGB, 0, ptr, imgBytes);
            img.UnlockBits(imgData);
            return img;
        }


        public static byte[,] RGBArrayToGrayscale(byte[,,] arr)
        {
            var output = new byte[arr.GetLength(0), arr.GetLength(1)];

            for (var j = 0; j < arr.GetLength(1); j++)
            {
                for (var i = 0; i < arr.GetLength(0); i++)
                {
                    output[i, j] = (byte)(arr[i, j, 0] * 0.2989 + arr[i, j, 1] * 0.5870 + arr[i, j, 2] * 0.1140);
                }
            }
            return output;
        }


        public static Color CalculateAvgColor(Bitmap img)
        {
            var arr = BitmapToRGBArray(img);
            ulong r = 0, g = 0, b = 0;

            for (int j = 0; j < arr.GetLength(1); j++)
            {
                for (int i = 0; i < arr.GetLength(0); i++)
                {
                    r += arr[i, j, 0];
                    g += arr[i, j, 1];
                    b += arr[i, j, 2];
                }
            }
            r /= (ulong)(arr.GetLength(0) * arr.GetLength(1));
            g /= (ulong)(arr.GetLength(0) * arr.GetLength(1));
            b /= (ulong)(arr.GetLength(0) * arr.GetLength(1));

            return Color.FromArgb((int)r, (int)g, (int)b);
        }


        public static void PasteArray(ref byte[,,] dest, byte[,,] src, int x, int y)
        {
            int width = src.GetLength(0), height = src.GetLength(1);

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    dest[i + x, j + y, 0] = src[i, j, 0];
                    dest[i + x, j + y, 1] = src[i, j, 1];
                    dest[i + x, j + y, 2] = src[i, j, 2];
                }
            }
        }

        public static Bitmap CropImage(Bitmap img, int size)
        {
            img = MakeSquare(img);
            img = Resize(img, size, size);
            return img;
        }


        public static byte[,] CreateThumbnail(Bitmap img, int size)
        {
            img = Resize(img, size, size);
            var a = BitmapToRGBArray(img);
            return RGBArrayToGrayscale(a);
        }


        public static ulong CalculateError(byte[,] first, byte[,] second)
        {
            ulong sum = 0;
            for (int j = 0; j < first.GetLength(1); j++)
            {
                for (int i = 0; i < first.GetLength(0); i++)
                {
                    sum += (ulong)Math.Pow(first[i, j] - second[i, j], 2);
                }
            }
            return sum;
        }


        //public static BitmapSource BitmapToBitmapSource(Bitmap source)
        //{
        //    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //                  source.GetHbitmap(),
        //                  IntPtr.Zero,
        //                  Int32Rect.Empty,
        //                  BitmapSizeOptions.FromEmptyOptions());
        //}
    }
}
