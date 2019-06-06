using GUIv2.Helpers;
using GUIv2.Interfaces;
using GUIv2.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;

namespace GUIv2.ViewModels
{
    class CreateMozaicViewModel : BaseViewModel, IPageViewModel
    {
        private Thread _algorithmThread;
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG", ".BMP", ".PNG" };
        public event EventHandler<BitmapLoadedEventHandler> BitmapLoaded;

        private Mozaic _mozaic;
        private int TilesNum;
        public int ProgressBarValue;       

        public string PathToImage { get; set; }
        public string PathToDatabase { get; set; }
        public string Tiles { get; set; }
        public string ProgressBarValueString { get; private set; }

        private ICommand _createMozaicCommand;
        private ICommand _cancelTaskCommand;
        private ICommand _goToWelcomeScreenCommand;
        private ICommand _selectFileDialogCommand;
        private ICommand _selectDirectoryDialogCommand;


        public class BitmapLoadedEventHandler : EventArgs
        {
            public Bitmap BMP { get; private set; }

            public BitmapLoadedEventHandler(Bitmap bmp)
            {
                BMP = bmp;
            }
        }


        public CreateMozaicViewModel()
        {
            _mozaic = new Mozaic();
            _mozaic.ProgressChanged += Mozaic_ProgressChanged;
        }


        void Mozaic_ProgressChanged(object sender, Mozaic.ProgressChangedEventHandler e)
        {
            ProgressBarValue = e.Progress;
            UpdateProgressBar(ProgressBarValue.ToString());
        }


        void UpdateProgressBar(string progress)
        {
            ProgressBarValueString = progress;
            OnPropertyChanged("ProgressBarValueString");
        }


        private void AlgorithmThreadWork()
        {
            _mozaic.Process(PathToImage, PathToDatabase, TilesNum);
            UpdateProgressBar("100");

            BitmapLoaded?.Invoke(this, new BitmapLoadedEventHandler(_mozaic.OutputImg));
        }


        #region Checking input methods
        private bool CanCreateMozaic()
        {
            if (!CheckImagePath() || !CheckDatabasePath() || !CheckDatabaseDataFile() || !CheckTilesInput() || !CheckIfAlreadyRunning()) return false;
            return true;
        }


        private bool CheckImagePath()
        {
            if (File.Exists(PathToImage) && ImageExtensions.Contains(Path.GetExtension(PathToImage).ToUpperInvariant()))
            {
                return true;
            }
            System.Windows.MessageBox.Show("Podano nieprawidłową ścieżkę do obrazu wejściowego.");
            return false;
        }


        private bool CheckDatabasePath()
        {
            if (Directory.Exists(PathToDatabase))
            {
                var files = Directory.GetFiles(PathToDatabase);
                bool flag = false;
                foreach (string name in files)
                {
                    if (ImageExtensions.Contains(Path.GetExtension(name).ToUpperInvariant())) flag = true;
                }
                if (flag == true) return true;
            }
            System.Windows.MessageBox.Show("Podano nieprawidłową ścieżkę do bazy obrazów.");
            return false;
        }


        private bool CheckDatabaseDataFile()
        {
            if (File.Exists(PathToDatabase + @"\data.bin")) return true;
            System.Windows.MessageBox.Show("Najpierw przygotuj obrazy do utworzenia mozaiki.");
            return false;
        }


        private bool CheckTilesInput()
        {
            try
            {
                TilesNum = Convert.ToInt32(Tiles);
                if (TilesNum < 1) throw new ArgumentException();
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Ilość segmentów musi być dodatnią liczbą całkowitą.");
                return false;
            }
            return true;
        }


        private bool CheckIfAlreadyRunning()
        {
            if (_algorithmThread != null)
            {
                System.Windows.MessageBox.Show("Program tworzy już mozaikę. Aby stworzyć inną najpierw anuluj trwającą operację.");
                return false;
            }
            return true;
        }
        #endregion


        #region Commands' implementations
        public ICommand CreateMozaicCommand
        {
            get
            {
                return _createMozaicCommand ?? (_createMozaicCommand = new RelayCommand(x =>
                {
                    if (!CanCreateMozaic()) return;                    
                    _algorithmThread = new Thread(new ThreadStart(AlgorithmThreadWork));
                    _algorithmThread.IsBackground = true;
                    _algorithmThread.Start();
                }));
            }
        }

        
        public ICommand CancelTaskCommand
        {
            get
            {
                return _cancelTaskCommand ?? (_cancelTaskCommand = new RelayCommand(x =>
                {
                    if (_algorithmThread != null)
                    {
                        _algorithmThread.Abort();
                        _algorithmThread = null;
                    }
                    UpdateProgressBar("0");                   
                }));
            }
        }

        
        public ICommand GoToWelcomeScreenCommand
        {
            get
            {
                return _goToWelcomeScreenCommand ?? (_goToWelcomeScreenCommand = new RelayCommand(x =>
                {
                    if (_algorithmThread != null)
                    {
                        _algorithmThread.Abort();
                        _algorithmThread = null;
                    }
                    UpdateProgressBar("0");                    
                    Mediator.NotifyAction("GoToWelcomeScreen");
                }));
            }
        }

      
        public ICommand SelectFileDialogCommand
        {
            get
            {
                return _selectFileDialogCommand ?? (_selectFileDialogCommand = new RelayCommand(x =>
                {
                    Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                    if (openFileDialog.ShowDialog() == true)
                    {
                        PathToImage = openFileDialog.FileName;
                        OnPropertyChanged("PathToImage");
                    }
                }));
            }
        }


        public ICommand SelectDirectoryDialogCommand
        {
            get
            {
                return _selectDirectoryDialogCommand ?? (_selectDirectoryDialogCommand = new RelayCommand(x =>
                {
                    var dialog = new FolderBrowserDialog();
                    dialog.ShowDialog();
                    PathToDatabase = dialog.SelectedPath;
                    OnPropertyChanged("PathToDatabase");
                }));
            }
        }
        #endregion
    }
}
