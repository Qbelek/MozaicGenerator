using GUIv2.Helpers;
using GUIv2.Interfaces;
using Lib;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GUIv2.ViewModels
{
    class DisplayScreenViewModel : BaseViewModel, IPageViewModel
    {
        private Bitmap _mozaic;
        public string MozaicSavingPath { get; set; }
        private BitmapSource _bitmapSource;
        public BitmapSource BitmapSource
        {
            get
            {
                return _bitmapSource;
            }
            set
            {
                _bitmapSource = value;
                OnPropertyChanged("BitmapSource");
            }
        }


        public DisplayScreenViewModel(Bitmap bmp)
        {
            _mozaic = bmp;
            _bitmapSource = _mozaic.ToBitmapSource();
        }


        private ICommand _selectDirectoryDialogCommand;
        public ICommand SelectDirectoryDialogCommand
        {
            get
            {
                return _selectDirectoryDialogCommand ?? (_selectDirectoryDialogCommand = new RelayCommand(x =>
                {
                    var dialog = new FolderBrowserDialog();
                    dialog.ShowDialog();
                    var input = x as string;
                    MozaicSavingPath = dialog.SelectedPath;
                    OnPropertyChanged("MozaicSavingPath");                   
                }));
            }
        }


        private ICommand _goToCreateMozaicScreenCommand;
        public ICommand GoToCreateMozaicScreenCommand
        {
            get
            {
                return _goToCreateMozaicScreenCommand ?? (_goToCreateMozaicScreenCommand = new RelayCommand(x =>
                {
                    Mediator.NotifyAction("GoToCreateMozaicScreen");
                }));
            }
        }


        private ICommand _saveMozaicCommand;
        public ICommand SaveMozaicCommand
        {
            get
            {
                return _saveMozaicCommand ?? (_saveMozaicCommand = new RelayCommand(MozaicSavingPath =>
                {
                    _mozaic.Save(Path.GetDirectoryName(MozaicSavingPath as string) + "\\mozaic.bmp", ImageFormat.Bmp);
                }));
            }
        }
    }
}
