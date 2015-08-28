using System.Diagnostics.Contracts;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Ninject;
using Youtube;
using YuInfoRetriever.Authorization;

namespace YoutubeWatcher.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        YInfoRetriever youRetriever = new YInfoRetriever();

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            
        }


        #region Properties

        private string _status= "Not Connected";
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region Functions

        /// <summary>
        /// Connects to Youtube Auth 2.0 service
        /// </summary>
        private void Command()
        {
            if (youRetriever.IsAuthorized)
                youRetriever.Disconnect();

            var jsonReader = SimpleIoc.Default.GetInstance<IAuthProvider>();
            jsonReader.SetParams("client_secrets.json");
            youRetriever.Authorize(jsonReader).ContinueWith(t=> { Status = "Authorizated succesfully"; }).ConfigureAwait(false);
        }


        #endregion


        #region Commands

        public ICommand ConnectCommand => new RelayCommand(Command);

        #endregion
    }
}