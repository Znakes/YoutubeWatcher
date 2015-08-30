using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
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

        private YInfoRetriever youRetriever;

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

            youRetriever = SimpleIoc.Default.GetInstance<YInfoRetriever>();
            Subscriptions = new ObservableCollection<SubscriptionEx>();
            
            Contract.ContractFailed += Contract_ContractFailed;
        }

        void Contract_ContractFailed(object sender, ContractFailedEventArgs e)
        {
            //e.SetUnwind();
            ContractHelper.TriggerFailure(ContractFailureKind.Assert, "Fail!", "Message!", "Condition!", new Exception());
            //MessengerInstance.Send(new NotificationMessage(e.Message));

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

        public ObservableCollection<SubscriptionEx> Subscriptions { get; set; }
      
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
            //Contract.Assert(youRetriever != null && youRetriever.IsAuthorized);

            if (youRetriever == null || !youRetriever.IsAuthorized)
            {
                Status = "Yor arent authorized! Please, press Connect first";
                return;
            }

            IEnumerable<Subscription> subscriptions =  await youRetriever.GetSubscriptions();

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

    public class SubscriptionEx
    {
        public SubscriptionEx()
        {
            Playlists = new ObservableCollection<Playlist>();
        }

        public bool PlayListsAreLoaded { get; set; }
        
        public ObservableCollection<Playlist> Playlists { get; set; }

        public Subscription Subscription { get; set; }

        private async Task<IEnumerable<Playlist>> GetPlaylists()
        {

            var you = SimpleIoc.Default.GetInstance<YInfoRetriever>();

            if (you.IsAuthorized)
            {
                var res = await you.GetPlayLists(Subscription.Snippet.ResourceId.ChannelId);

                return res;
            }

            return null;
        }
        
        public async Task RefreshPlayLists()
        {
            PlayListsAreLoaded = false;

            if (Playlists == null)
                Playlists = new ObservableCollection<Playlist>();

            if (Playlists.Any())
                Playlists.Clear();

            var pl = await GetPlaylists();

            foreach (var playlist in pl)
            {
                Playlists.Add(playlist);
            }

            PlayListsAreLoaded = true;

        }
        
    }
    
    public class PlaylistEx
    {
        public Subscription Subscription { get; set; }

        public Lazy<PlaylistItem> PlaylistItems { get; set; } 
    }
}


