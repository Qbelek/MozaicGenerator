using GUIv2.Models;
using GUIv2.Helpers;
using GUIv2.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Lib;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using GUIv2.Handlers;

namespace GUIv2.ViewModels
{
    class PrepareImagesViewModel : BaseViewModel, IPageViewModel
    {
        private Thread _algorithmThread;
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG", ".BMP", ".PNG" };

        private ImagesDatabase DB;
        public int _progressBarValue;
        private int _sizeOfNewImagesNum;

        public string PathToDatabase { get; set; }
        public string OutputPath { get; set; }
        public string SizeOfNewImages { get; set; }
        public string ProgressBarValueString { get; private set; }

        private ICommand _createDatabaseCommand;
        private ICommand _cancelTaskCommand;
        private ICommand _goToWelcomeScreenCommand;
        private ICommand _selectDirectoryDialogCommand;



        public PrepareImagesViewModel()
        {
            DB = new ImagesDatabase();
            DB.ProgressChanged += DB_ProgressChanged;
            _algorithmThread = new Thread(new ThreadStart(AlgorithmThreadWork));
            _algorithmThread.IsBackground = true;
        }


        void DB_ProgressChanged(object sender, ProgressChangedEventHandler e)
        {
            _progressBarValue = e.Progress;
            UpdateProgressBar(_progressBarValue.ToString());           
        }


        void UpdateProgressBar(string progress)
        {
            ProgressBarValueString = progress;
            OnPropertyChanged("ProgressBarValueString");
        }


        private void AlgorithmThreadWork()
        {
            if (!Directory.Exists(OutputPath)) Directory.CreateDirectory(OutputPath);            
            DB.CreateDatabase(PathToDatabase, OutputPath, _sizeOfNewImagesNum);
            DB.ProgressChanged -= DB_ProgressChanged;
            var path = string.Concat(OutputPath, @"\data.bin");
            Utilities.SerializeItem(DB, path);
            UpdateProgressBar("100");
            System.Windows.MessageBox.Show("Operacja zakończona.");
        }


        #region Checking input methods
        private bool CanCreateDatabase()
        {
            if (!CheckDatabasePath() || !CheckSizeInput() || !CheckIfAlreadyRunning()) return false;
            return true;
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


        private bool CheckSizeInput()
        {
            try
            {
                _sizeOfNewImagesNum = Convert.ToInt32(SizeOfNewImages);
                if (_sizeOfNewImagesNum < 1) throw new ArgumentException();
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Rozmiar wyjściowych obrazów musi być dodatnią liczbą całkowitą");
                return false;
            }
            return true;
        }


        private bool CheckIfAlreadyRunning()
        {
            if (_algorithmThread.IsAlive)
            {
                System.Windows.MessageBox.Show("Program przygotowuje już bazę zdjęć. Aby stworzyć inną najpierw anuluj trwającą operację.");
                return false;
            }
            return true;
        }
        #endregion


        #region Commands' implementations
        public ICommand CreateDatabaseCommand
        {
            get
            {
                return _createDatabaseCommand ?? (_createDatabaseCommand = new RelayCommand(x =>
                {
                    if (!CanCreateDatabase()) return;
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
                    if (_algorithmThread.IsAlive)
                    {
                        _algorithmThread.Abort();
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
                    if (_algorithmThread.IsAlive)
                    {
                        _algorithmThread.Abort();
                    }
                    UpdateProgressBar("0");
                    Mediator.NotifyAction("GoToWelcomeScreen");
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
                    var input = x as string;
                    if (input.Equals("Database"))
                    {
                        PathToDatabase = dialog.SelectedPath;
                        OnPropertyChanged("PathToDatabase");
                    }
                    else if (input.Equals("Output"))
                    {
                        OutputPath = dialog.SelectedPath;
                        OnPropertyChanged("OutputPath");
                    }                   
                }));
            }
        }
        #endregion
    }
}
