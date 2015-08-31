using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Youtube;
using YuInfoRetriever.Authorization;

namespace YoutubeWatcher.ViewModel
{
    /// <summary>
    ///     This class contains properties that the main View can data bind to.
    ///     <para>
    ///         Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    ///     </para>
    ///     <para>
    ///         You can also use Blend to data bind with the tool's support.
    ///     </para>
    ///     <para>
    ///         See http://www.galasoft.ch/mvvm
    ///     </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly YInfoRetriever youRetriever;

        /// <summary>
        ///     Initializes a new instance of the MainViewModel class.
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

            youRetriever = SimpleIoc.Default.GetInstance<YInfoRetriever>();
            Subscriptions = new ObservableCollection<SubscriptionEx>();
            Connect().ConfigureAwait(false);
            Contract.ContractFailed += Contract_ContractFailed;
        }

        private void Contract_ContractFailed(object sender, ContractFailedEventArgs e)
        {
            //e.SetUnwind();
            ContractHelper.TriggerFailure(ContractFailureKind.Assert, "Fail!", "Message!", "Condition!", new Exception());
            //MessengerInstance.Send(new NotificationMessage(e.Message));
        }

        #region Properties

        private string _status = "Not Connected";

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisePropertyChanged();
            }
        }

        private bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged();
            }
        }


        public ObservableCollection<SubscriptionEx> Subscriptions { get; set; }

        #endregion

        #region Functions

        /// <summary>
        ///     Connects to Youtube Auth 2.0 service
        /// </summary>
        private async Task Connect()
        {
            IsConnected = false;
            if (youRetriever.IsAuthorized)
                youRetriever.Disconnect();

            var jsonReader = SimpleIoc.Default.GetInstance<IAuthProvider>();
            jsonReader.SetParams(@"D://client_secrets.json");

            var result = await youRetriever.Authorize(jsonReader);

            if (result)
            {
                Status = "Authorizated succesfully";
                IsConnected = true;
            }
            else
            {
                Status = "Authorizated failed";
                IsConnected = false;
            }
        }

        private async Task GetSubscriptions()
        {
            //Contract.Assert(youRetriever != null && youRetriever.IsAuthorized);

            if (youRetriever == null || !youRetriever.IsAuthorized)
            {
                Status = "Yor arent authorized! Please, press Connect first";
                return;
            }

            var subscriptions = await youRetriever.GetSubscriptions();
            Subscriptions.Clear();
            foreach (var subscription in subscriptions)
            {
                var ex = new SubscriptionEx {Subscription = subscription};
                Subscriptions.Add(ex);
            }
        }

        #endregion

        #region Commands

        private RelayCommand _connect;

        public ICommand ConnectCommand
        {
            get
            {
                return (_connect = _connect ?? new RelayCommand(async () => { await Connect(); }, () => !IsConnected));
            }
        }


        private RelayCommand _subscriptions;

        public ICommand SubscriptionsCommand
        {
            get
            {
                return
                    (_subscriptions =
                        _subscriptions ?? new RelayCommand(async () => { await GetSubscriptions(); }, () => IsConnected));
            }
        }

        #endregion
    }
}