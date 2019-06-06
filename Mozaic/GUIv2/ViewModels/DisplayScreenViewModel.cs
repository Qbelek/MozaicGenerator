using GUIv2.Interfaces;
using Lib;
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
            //_image = Processing.BitmapToBitmapSource(bmp);   
        }

    }
}
