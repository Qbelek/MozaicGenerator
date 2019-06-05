using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Lib;

namespace GUIv2.Models
{
    [Serializable]
    class ImageItem
    {
        public int ThumbnailSize { get; private set; }
        public string Filename { get; private set; }
        public byte[,] Thumbnail { get; private set; }
        public float[] AverageHSB { get; private set; }


        public ImageItem(string path, string outputPath, int newSize)
        {
            ImageFormat extension;
            string outputName;

            Filename = Path.GetFileName(path);
            ThumbnailSize = 20;

            var originalImage = new Bitmap(path);

            extension = originalImage.RawFormat;
            originalImage = Processing.ChangePixelFormat(originalImage, PixelFormat.Format32bppRgb);

            Bitmap croppedImage = Processing.CropImage(originalImage, newSize);
            outputName = string.Concat(outputPath, "\\", Filename);
            croppedImage.Save(outputName, extension);

            Thumbnail = Processing.CreateThumbnail(croppedImage, ThumbnailSize);

            var AverageColor = Processing.CalculateAvgColor(originalImage);
            AverageHSB = new float[3] { AverageColor.GetHue(), AverageColor.GetSaturation(), AverageColor.GetBrightness() };

            originalImage.Dispose();
            croppedImage.Dispose();
        } 
    }
}
