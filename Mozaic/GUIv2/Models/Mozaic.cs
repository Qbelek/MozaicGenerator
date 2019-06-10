using GUIv2.Handlers;
using Lib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace GUIv2.Models
{
    class Mozaic
    {
        public event EventHandler<ProgressChangedEventHandler> ProgressChanged;
        private readonly object _lockResources = new object();
        private readonly object _lockResources2 = new object();
        public Bitmap Img { get; private set; }
        public Bitmap OutputImg { get; private set; }
        private ImagesDatabase Db { get; set; }


        public Mozaic() { }


        public void Process(string originalImagePath, string imagesDBPath, int tiles)
        {
            Img = (Bitmap)Image.FromFile(originalImagePath);
            Img = Processing.ChangePixelFormat(Img, PixelFormat.Format32bppRgb);

            Db = Utilities.DeserializeItem(imagesDBPath + "\\data.bin") as ImagesDatabase;

            OutputImg = CreateMozaic(tiles, imagesDBPath);
        }


        private Bitmap CreateMozaic(int tiles, string imagesDBPath)
        {
            int count = 0;
            int pixels = Img.Width / tiles;
            int cols = tiles;
            int rows = Img.Height / pixels;
            int mozaicWidth = cols * Db.ImagesSize;
            int mozaicHeight = rows * Db.ImagesSize;
            byte[,,] mozaic = new byte[mozaicWidth, mozaicHeight, 3];

            int numOfThreads = Environment.ProcessorCount - 2;
            numOfThreads = numOfThreads < 2 ? 2 : numOfThreads;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = numOfThreads};
            Parallel.For(0, rows, options, j =>
            {
                var rng = new Random();
                for (int i = 0; i < cols; i++)
                {
                    Bitmap fragment;
                    lock (_lockResources)
                    {
                        count++;
                        double dprogress = (count / (double)(cols * rows)) * 100.0;
                        ProgressChanged?.Invoke(this, new ProgressChangedEventHandler((int)dprogress));
                        fragment = Img.Clone(new Rectangle(i * pixels, j * pixels, pixels, pixels), PixelFormat.Format32bppRgb);
                    }

                    ImageItem bestImage = FindBest(fragment, rng);
                    var bestImagePath = Path.Combine(imagesDBPath, bestImage.Filename);
                    Bitmap bestImageBMP = (Bitmap)Image.FromFile(bestImagePath);
                    bestImageBMP = Processing.ChangePixelFormat(bestImageBMP, PixelFormat.Format32bppRgb);
                    //bestImageBMP = AdjustColor(bestImageBMP, fragment, bestImage);
                    byte[,,] bestImageArr = Processing.BitmapToRGBArray(bestImageBMP);

                    bestImageBMP.Dispose();
                    fragment.Dispose();

                    lock (_lockResources2)
                    {
                        Processing.PasteArray(ref mozaic, bestImageArr, i * Db.ImagesSize, j * Db.ImagesSize);
                    }
                }
            });

            return Processing.RGBArrayToBitmap(mozaic);
        }


        private ImageItem FindBest(Bitmap img, Random rng)
        {
            List<ImageItem> listOfBestByContent = FindBestByContent(img);
            List<ImageItem> listOfBestByColor = FindBestByHue(img, listOfBestByContent);
            int index = rng.Next(listOfBestByColor.Count);
            return listOfBestByColor[index];
        }


        private List<ImageItem> FindBestByContent(Bitmap img)
        {
            ulong minError = ulong.MaxValue;
            ulong offset = 100000;
            byte[,] thumbnail = Processing.CreateThumbnail(img, Db.Images[0].ThumbnailSize);
            var candidatesList = new List<ImageItem>();
            var errorList = new Dictionary<string, ulong>();

            foreach (ImageItem item in Db.Images)
            {
                ulong error = Processing.CalculateError(item.Thumbnail, thumbnail);
                errorList.Add(item.Filename, error);
                if (error < minError)
                {
                    minError = error;
                }
            }
            foreach (ImageItem item in Db.Images)
            {
                if (errorList[item.Filename] < minError + offset)
                {
                    candidatesList.Add(item);
                }
            }
            return candidatesList;
        }


        private List<ImageItem> FindBestByHue(Bitmap img, List<ImageItem> list)
        {
            float littleImageHue, originalFragmentHue;
            float minError = float.MaxValue;
            float error;
            float offset = 15;
            var candidatesList = new List<ImageItem>();
            var errorList = new Dictionary<string, float>();
            Color avgColor = Processing.CalculateAvgColor(img);
            originalFragmentHue = avgColor.GetHue();

            foreach (ImageItem item in list)
            {
                littleImageHue = item.AverageHSB[0];

                //if (Math.Abs(littleImageHue - originalFragmentHue) < 180)
                //{
                //    error = Math.Abs(littleImageHue - originalFragmentHue);
                //}
                //else if (littleImageHue > originalFragmentHue)
                //{
                //    error = 360 - littleImageHue + originalFragmentHue;
                //}
                //else
                //{
                //    error = 360 - originalFragmentHue + littleImageHue;
                //}

                error = Math.Abs(littleImageHue - originalFragmentHue);

                errorList.Add(item.Filename, error);

                if (error < minError)
                {
                    minError = error;
                }
            }
            foreach (ImageItem item in list)
            {
                if (errorList[item.Filename] < minError + offset)
                {
                    candidatesList.Add(item);
                }
            }

            return candidatesList;
        }


        private Bitmap AdjustColor(Bitmap loadedImage, Bitmap originalFragment, ImageItem loadedImageInfo)
        {
            float satDifference;
            float loadedImageSaturation, originalFragmentSaturation;
            Color originalFragmentAvgColor;

            originalFragmentAvgColor = Processing.CalculateAvgColor(originalFragment);
            originalFragmentSaturation = originalFragmentAvgColor.GetSaturation();
            loadedImageSaturation = loadedImageInfo.AverageHSB[1];
            satDifference = originalFragmentSaturation - loadedImageSaturation;

            //loadedImageSaturation + satDifference * (float)2
            loadedImage = TweakSaturation(loadedImage, (float)5 * satDifference);
            return loadedImage;
        }


        private Bitmap TweakSaturation(Bitmap img, float saturation)
        {
            const float rwgt = 0.3086f;
            const float gwgt = 0.6094f;
            const float bwgt = 0.0820f;

            ImageAttributes imageAttributes = new ImageAttributes();
            ColorMatrix colorMatrix = new ColorMatrix();

            float baseSat = 1.0f - saturation;

            colorMatrix[0, 0] = baseSat * rwgt + saturation;
            colorMatrix[0, 1] = baseSat * rwgt;
            colorMatrix[0, 2] = baseSat * rwgt;
            colorMatrix[1, 0] = baseSat * gwgt;
            colorMatrix[1, 1] = baseSat * gwgt + saturation;
            colorMatrix[1, 2] = baseSat * gwgt;
            colorMatrix[2, 0] = baseSat * bwgt;
            colorMatrix[2, 1] = baseSat * bwgt;
            colorMatrix[2, 2] = baseSat * bwgt + saturation;

            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            var outBitmap = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppRgb);

            using (var g = Graphics.FromImage(outBitmap))
            {
                g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imageAttributes);
            }

            return outBitmap;
        }


        private Bitmap TweakSaturation2(Bitmap img, float saturation)
        {
            return new Bitmap(1, 1);
        }
    }
}
