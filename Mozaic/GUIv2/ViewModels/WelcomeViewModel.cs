using GUIv2.Helpers;
using GUIv2.Interfaces;
using System.Windows.Input;

namespace GUIv2.ViewModels
{
    class WelcomeViewModel : BaseViewModel, IPageViewModel
    {

        private ICommand _goToCreateMozaicScreenCommand;
        private ICommand _goToPrepareImagesScreenCommand;


        #region Commands' implementations
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

        
        public ICommand GoToPrepareImagesScreenCommand
        {
            get
            {
                return _goToPrepareImagesScreenCommand ?? (_goToPrepareImagesScreenCommand = new RelayCommand(x =>
                {
                    Mediator.NotifyAction("GoToPrepareImagesScreen");
                }));
            }
        }
        #endregion
    }
}
