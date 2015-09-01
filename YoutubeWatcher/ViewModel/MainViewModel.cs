using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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
            WatchedItems = new List<string>(1000);
            Connect().ConfigureAwait(false);
        }


        #region FIELDS

        private string _status = "Not Connected";
        private bool _isConnected;
        private RelayCommand _connect;
        private RelayCommand _subscriptions;
        private PlaylistEx _currentPlaylist;
        private PlaylistItem _currentPlaylistItem;
        private ChannelEx _currentChannel;
        private RelayCommand _getNext;
        private RelayCommand _getWatched;
        private ObservableCollection<ChannelEx> _channels;
        private bool _playRandomly;

        #endregion

        #region Properties

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisePropertyChanged();
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Use shuffling
        /// </summary>
        public bool PlayRandomly
        {
            get { return _playRandomly; }
            set
            {
                _playRandomly = value;
                RaisePropertyChanged();
            }
        }


        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged();
            }
        }


        /// <summary>
        ///     Gets or sets current selected PlayList
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


        /// <summary>
        ///     Gets or sets current selected playlist item (video)
        /// </summary>
        public PlaylistItem CurrentPlaylistItem
        {
            get { return _currentPlaylistItem; }
            set
            {
                _currentPlaylistItem = value;
                RaisePropertyChanged();
                RaisePropertyChanged();
            }
        }


        /// <summary>
        ///     Gets or sets current selected channel
        /// </summary>
        public ChannelEx CurrentChannel
        {
            get { return _currentChannel; }
            set
            {
                ;
                _currentChannel = value;
                RaisePropertyChanged();
            }
        }

        public List<string> WatchedItems { get; set; }

        public ObservableCollection<ChannelEx> Subscriptions
        {
            get { return _channels; }
            set
            {
                _channels = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Connects to Youtube Auth 2.0 service
        /// </summary>
        private async Task Connect()
        {
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

        private void GetNext()
        {
            //await Task.Run(() =>
            //{
            if (!WatchedItems.Any())
                return;


            if (IsConnected && CurrentPlaylist != null)
            {
                if (CurrentPlaylistItem == null)
                    CurrentPlaylistItem = CurrentPlaylist.PlaylistItems.First();

                if (!WatchedItems.Contains(CurrentPlaylistItem.Snippet.ResourceId.VideoId))
                    WatchedItems.Add(CurrentPlaylistItem.Snippet.ResourceId.VideoId);

                var startIndex = CurrentPlaylist.PlaylistItems.IndexOf(CurrentPlaylistItem);
                var foundUnwatched = false;


                if (PlayRandomly)
                {

                    List<int> arrayOfIds = new List<int>(CurrentPlaylist.PlaylistItems.Count);

                    for (int i = 0; i < CurrentPlaylist.PlaylistItems.Count; i++)
                    {
                        arrayOfIds.Add(i);   
                    }
                    var rand = new Random((int) DateTime.Now.ToFileTimeUtc());
                    
                    while (arrayOfIds.Any())
                    {
                        var id = rand.Next(0, arrayOfIds.Count);

                        if (!WatchedItems.Contains(CurrentPlaylist.PlaylistItems[id].Snippet.ResourceId.VideoId))
                        {
                            foundUnwatched = true;
                            CurrentPlaylistItem = CurrentPlaylist.PlaylistItems[id];
                            break;
                        }
                        else
                        {
                            arrayOfIds.Remove(id);
                        }
                    }
                }
                else
                {
                    for (var i = startIndex + 1; i < CurrentPlaylist.PlaylistItems.Count; i++)
                    {
                        if (!WatchedItems.Contains(CurrentPlaylist.PlaylistItems[i].Snippet.ResourceId.VideoId))
                        {
                            foundUnwatched = true;
                            CurrentPlaylistItem = CurrentPlaylist.PlaylistItems[i];
                            break;
                        }
                    }

                    if (!foundUnwatched)
                    {
                        for (var i = 0; i < startIndex - 1; i++)
                        {
                            if (!WatchedItems.Contains(CurrentPlaylist.PlaylistItems[i].Snippet.ResourceId.VideoId))
                            {
                                foundUnwatched = true;

                                CurrentPlaylistItem = CurrentPlaylist.PlaylistItems[i];
                                break;
                            }
                        }
                    }
                }

                Status = !foundUnwatched ? "Cannot find unwatched video" : "Enjoy!";
            }
            //});
        }


        private async Task GetSubscriptions()
        {
            Status = "Getting channels";
            //Contract.Assert(youRetriever != null && youRetriever.IsAuthorized);

            if (youRetriever == null || !youRetriever.IsAuthorized)
            {
                Status = "Yor arent authorized! Please, press Connect first";
                return;
            }

            Subscriptions.Clear();
            Subscriptions = new ObservableCollection<ChannelEx>();

            var subscriptions = await youRetriever.GetSubscriptions();
            var channels = await youRetriever.GetChannelsFromSubscriptions(subscriptions);
            await GetWatched();


            foreach (var channel in channels)
            {
                var ex = new ChannelEx {Channel = channel};
                Subscriptions.Add(ex);
            }

            var channelEx = Subscriptions.FirstOrDefault();
            if (channelEx != null) CurrentChannel = channelEx;
            Status = "Channels list updated";
        }

        private async Task GetWatched()
        {
            //Contract.Assert(youRetriever != null && youRetriever.IsAuthorized);
            Status = "Getting watched videos";

            if (youRetriever == null || !youRetriever.IsAuthorized)
            {
                Status = "Yor arent authorized! Please, press Connect first";
                return;
            }

            var yourChannel = await youRetriever.GetOwnChannel();
            var watched = await
                youRetriever.GetPlayListItems(yourChannel.ContentDetails.RelatedPlaylists.WatchHistory,
                    CancellationToken.None);
            WatchedItems.Clear();
            WatchedItems.AddRange(watched.Select(w => w.Snippet.ResourceId.VideoId));

            Status = "Watched history updated";
        }

        #endregion

        #region Commands

        public ICommand ConnectCommand
        {
            get
            {
                return (_connect = _connect ?? new RelayCommand(async () => { await Connect(); }, () => !IsConnected));
            }
        }


        public ICommand SubscriptionsCommand
        {
            get
            {
                return
                    (_subscriptions =
                        _subscriptions ?? new RelayCommand(async () => { await GetSubscriptions(); }, () => IsConnected));
            }
        }

        public ICommand GetNextCommand
        {
            get { return (_getNext = _getNext ?? new RelayCommand(() => { GetNext(); }, () => IsConnected)); }
        }

        public ICommand RefreshWatchedCommand
        {
            get
            {
                return
                    (_getWatched =
                        _getWatched ?? new RelayCommand(async () => { await GetWatched(); }, () => IsConnected));
            }
        }

        #endregion
    }
}