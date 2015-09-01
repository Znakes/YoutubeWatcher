using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
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
            Subscriptions = new ObservableCollection<ChannelEx>();
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

        private PlaylistEx _currentPlaylist;

        /// <summary>
        /// Gets or sets current selected PlayList
        /// </summary>
        public PlaylistEx CurrentPlaylist
        {
            get { return _currentPlaylist; }
            set
            {
                _currentPlaylist = value;
                RaisePropertyChanged();
            }
        }

        private PlaylistItem _currentPlaylistItem;

        /// <summary>
        /// Gets or sets current selected playlist item (video)
        /// </summary>
        public PlaylistItem CurrentPlaylistItem
        {
            get { return _currentPlaylistItem; }
            set
            {
                _currentPlaylistItem = value;
                RaisePropertyChanged();
            }
        }


        private PlaylistItem _currentChannel;

        /// <summary>
        /// Gets or sets current selected channel
        /// </summary>
        public PlaylistItem CurrentChannel
        {
            get { return _currentChannel; }
            set
            {
                _currentChannel = value;
                RaisePropertyChanged();
            }
        }



        public ObservableCollection<ChannelEx> Subscriptions { get; set; }

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

            var keyReader = SimpleIoc.Default.GetInstance<IAuthProvider>();
            keyReader.SetParams(@"Resources/client_secrets.json");

            var result = await youRetriever.Authorize(keyReader);

            if (result)
            {
                Status = "Authorizated succesfully";
                IsConnected = true;
                await GetSubscriptions();
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
            var channels = await youRetriever.GetChannelsFromSubscriptions(subscriptions);
            
            Subscriptions.Clear();

            foreach (var channel in channels)
            {
                var ex = new ChannelEx {Channel = channel};
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