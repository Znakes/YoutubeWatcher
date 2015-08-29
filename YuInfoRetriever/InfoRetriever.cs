using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Testing;
using YuInfoRetriever.Authorization;



namespace Youtube
{
    /// <summary>
    /// Retrieve information from Youtube by Avi.v3
    /// </summary>
    [VisibleForTestOnly]
    public class YInfoRetriever
    {
        #region Fields
        private UserCredential _credential;
        private YouTubeService _youtubeService;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets client's connection status
        /// </summary>
        public bool IsAuthorized { get; set; }

        #endregion
        
        /// <summary>
        /// Authorizes app in Youtube services
        /// </summary>
        /// <param name="authProvider">Instance of class with auth stream data</param>
        /// <returns></returns>
        public async Task<bool> Authorize(IAuthProvider authProvider)
        {
            Contract.Requires(authProvider != null);

            using (var stream = authProvider.GetAuthDataStream())
            {
                _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for read-only access to the authenticated 
                    // user's account, but not other types of account access.
                    new[] { YouTubeService.Scope.YoutubeReadonly}, "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            IsAuthorized = _credential != null;

            return IsAuthorized;
        }

        public void Disconnect()
        {
            IsAuthorized = false;
            _credential = null;
            _youtubeService.Dispose();
            _youtubeService = null;
        }


        


        /// <summary>
        /// Gets subsription list
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Subscription>> GetSubscriptions()
        {
            Contract.Requires(IsAuthorized);

            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = "YoutubeWatcher"
            });


            var channelsListRequest = _youtubeService.Subscriptions.List("snippet");
            channelsListRequest.Mine = true;

            // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
            var channelsListResponse = await channelsListRequest.ExecuteAsync();


            return channelsListResponse.Items.ToArray();

            foreach (Subscription channel in channelsListResponse.Items)
            {
                // From the API response, extract the playlist ID that identifies the list
                // of videos uploaded to the authenticated user's channel.
                
                var resourceListId = channel.Snippet.ResourceId.ChannelId;
                
                Console.WriteLine(@"Videos in list {0}", resourceListId);


                var plOfUser = _youtubeService.Playlists.List("snippet");
                plOfUser.ChannelId = resourceListId;

                var playlists = await plOfUser.ExecuteAsync();

                foreach (var playlist in playlists.Items)
                {

                    var uploadsListId = playlist.Id;

                    var nextPageToken = "";
                    while (nextPageToken != null)
                    {
                        var playlistItemsListRequest = _youtubeService.PlaylistItems.List("snippet");
                        playlistItemsListRequest.PlaylistId = uploadsListId;
                        playlistItemsListRequest.MaxResults = 50;
                        playlistItemsListRequest.PageToken = nextPageToken;

                        // Retrieve the list of videos uploaded to the authenticated user's channel.
                        PlaylistItemListResponse playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                        foreach (PlaylistItem playlistItem in playlistItemsListResponse.Items)
                        {
                            // Print information about each video.
                            Console.WriteLine("{0} ({1})", playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId);
                        }

                        nextPageToken = playlistItemsListResponse.NextPageToken;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets List of channel playlists
        /// </summary>
        /// <param name="playlistId">Id of playlist</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PlaylistItem>> GetPlayListItems(string playlistId, CancellationToken cancellationToken)
        {
            Contract.Assert(_youtubeService != null);
            Contract.Assert(IsAuthorized);

            return await Task.Run(() =>
            {
                var uploadsListId = playlistId;
                List<PlaylistItem> playlistItems = new List<PlaylistItem>();
                var nextPageToken = "";
                while (nextPageToken != null)
                {
                    if(cancellationToken.IsCancellationRequested)
                        break;

                    var playlistItemsListRequest = _youtubeService.PlaylistItems.List("snippet");
                    playlistItemsListRequest.PlaylistId = uploadsListId;
                    playlistItemsListRequest.MaxResults = 50;
                    playlistItemsListRequest.PageToken = nextPageToken;

                    // Retrieve the list of videos uploaded to the authenticated user's channel.
                    PlaylistItemListResponse playlistItemsListResponse = playlistItemsListRequest.Execute();

                    playlistItems.AddRange(playlistItemsListResponse.Items.ToArray());
                    nextPageToken = playlistItemsListResponse.NextPageToken;
                }

                return playlistItems;
            }, cancellationToken);
        }

        /// <summary>
        /// Gets List of channel playlists
        /// </summary>
        /// <param name="channelId">Id of channel</param>
        /// <returns></returns>
        public async Task<IEnumerable<Playlist>> GetPlayLists(string channelId)
        {
            Contract.Assert(_youtubeService != null);
            Contract.Assert(IsAuthorized);

            var plOfUser = _youtubeService.Playlists.List("snippet");
            plOfUser.ChannelId = channelId;

            var playlists = await plOfUser.ExecuteAsync();

            return playlists.Items.ToArray();
        }


        public async Task Create()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for full read/write access to the
                    // authenticated user's account.
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });

            // Create a new, private playlist in the authorized user's channel.
            var newPlaylist = new Playlist();
            newPlaylist.Snippet = new PlaylistSnippet();
            newPlaylist.Snippet.Title = "Test Playlist";
            newPlaylist.Snippet.Description = "A playlist created with the YouTube API v3";
            newPlaylist.Status = new PlaylistStatus();
            newPlaylist.Status.PrivacyStatus = "public";
            newPlaylist = await youtubeService.Playlists.Insert(newPlaylist, "snippet,status").ExecuteAsync();

            // Add a video to the newly created playlist.
            var newPlaylistItem = new PlaylistItem();
            newPlaylistItem.Snippet = new PlaylistItemSnippet();
            newPlaylistItem.Snippet.PlaylistId = newPlaylist.Id;
            newPlaylistItem.Snippet.ResourceId = new ResourceId();
            newPlaylistItem.Snippet.ResourceId.Kind = "youtube#video";
            newPlaylistItem.Snippet.ResourceId.VideoId = "GNRMeaz6QRI";
            newPlaylistItem = await youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet").ExecuteAsync();

            Console.WriteLine("Playlist item id {0} was added to playlist id {1}.", newPlaylistItem.Id, newPlaylist.Id);
        }
    }
}
