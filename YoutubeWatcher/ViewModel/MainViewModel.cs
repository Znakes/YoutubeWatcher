using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Google.Apis.YouTube.v3.Data;
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
            
            Subscriptions = new ObservableCollection<Subscription>();
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

        public ObservableCollection<Subscription> Subscriptions { get; set; }


        

        #endregion


        #region Functions

        /// <summary>
        /// Connects to Youtube Auth 2.0 service
        /// </summary>
        private async Task Connect()
        {
            if (youRetriever.IsAuthorized)
                youRetriever.Disconnect();

            var jsonReader = SimpleIoc.Default.GetInstance<IAuthProvider>();
            jsonReader.SetParams(@"D://client_secrets.json");

            bool result = await youRetriever.Authorize(jsonReader);

            if (result)
                Status = "Authorizated succesfully";
            else
                Status = "Authorizated failed";
        }

        private async Task GetSubscriptions()
        {
            Contract.Assert(youRetriever != null && youRetriever.IsAuthorized);

            IEnumerable<Subscription> subscriptions =  await youRetriever.GetSubscriptions();

            foreach (var subscription in subscriptions)
            {
                Subscriptions.Add(subscription);
            }
        }

        #endregion


        #region Commands

        private RelayCommand _connect;

        public ICommand ConnectCommand
        {
            get { return (_connect = _connect ?? new RelayCommand(async () => { await Connect(); })); }
        }


        private RelayCommand _subscriptions;

        public ICommand SubscriptionsCommand
        {
            get
            {
                return (_subscriptions = _subscriptions ?? new RelayCommand(async () => { await GetSubscriptions(); }));
            }
        }

        #endregion
    }
}