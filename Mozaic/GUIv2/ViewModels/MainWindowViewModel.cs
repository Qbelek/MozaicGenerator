using GUIv2.Helpers;
using GUIv2.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GUIv2.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public enum State { WelcomeScreen, PrepareImagesScreen, CreateMozaicScreen, DisplayScreen }

        private IPageViewModel _currentPageViewModel;
        private Dictionary<State, IPageViewModel> _pageViewModels;


        public MainWindowViewModel()
        {
            var createMozaicVM = new CreateMozaicViewModel();
            //createMozaicVM.BitmapLoaded += GoToDisplayScreen;
            PageViewModels.Add(State.WelcomeScreen, new WelcomeViewModel());
            PageViewModels.Add(State.PrepareImagesScreen, new PrepareImagesViewModel());
            PageViewModels.Add(State.CreateMozaicScreen, createMozaicVM);
            PageViewModels.Add(State.DisplayScreen, null);

            CurrentPageViewModel = PageViewModels[State.WelcomeScreen];

            Mediator.SubscribeAction("GoToWelcomeScreen", OnGoWelcomeScreen);
            Mediator.SubscribeAction("GoToPrepareImagesScreen", OnGoPrepareImagesScreen);
            Mediator.SubscribeAction("GoToCreateMozaicScreen", OnGoCreateMozaicScreen);
            Mediator.SubscribeAction("GoToDisplayScreen", OnGoDisplayScreen);
        }


        //void GoToDisplayScreen(object sender, CreateMozaicViewModel.BitmapLoadedEventHandler e)
        //{
        //    ChangeViewModel(State.DisplayScreen, e.BMP);
        //}


        public Dictionary<State, IPageViewModel> PageViewModels
        {
            get
            {
                if (_pageViewModels == null)
                    _pageViewModels = new Dictionary<State, IPageViewModel>();

                return _pageViewModels;
            }
        }

        public IPageViewModel CurrentPageViewModel
        {
            get
            {
                return _currentPageViewModel;
            }
            set
            {
                _currentPageViewModel = value;
                OnPropertyChanged("CurrentPageViewModel");
            }
        }

        private void ChangeViewModel(State state, object obj = null)
        {
            if (!PageViewModels.Keys.Contains(state))
                throw new ArgumentException();

            if (state.Equals(State.DisplayScreen))
            {
                CurrentPageViewModel = new DisplayScreenViewModel(obj as Bitmap);
                return;
            }

            CurrentPageViewModel = PageViewModels[state];
        }

        private void OnGoWelcomeScreen(object obj)
        {
            ChangeViewModel(State.WelcomeScreen);
        }

        private void OnGoPrepareImagesScreen(object obj)
        {
            ChangeViewModel(State.PrepareImagesScreen);
        }

        private void OnGoCreateMozaicScreen(object obj)
        {
            ChangeViewModel(State.CreateMozaicScreen);
        } 
        
        private void OnGoDisplayScreen(object obj)
        {
            ChangeViewModel(State.DisplayScreen);
        }
    }   
}
