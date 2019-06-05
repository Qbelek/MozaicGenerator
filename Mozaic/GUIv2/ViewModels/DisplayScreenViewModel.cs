using GUIv2.Interfaces;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GUIv2.ViewModels
{
    class DisplayScreenViewModel : BaseViewModel, IPageViewModel
    {
        private ImageSource _image;
        public ImageSource Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }


        public DisplayScreenViewModel(Bitmap bmp)
        {
            Image = CreateBitmapSourceFromGdiBitmap(bmp) as ImageSource;          
        }


        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
