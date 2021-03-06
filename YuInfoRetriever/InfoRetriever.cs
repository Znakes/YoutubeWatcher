﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            //Contract.Requires(authProvider != null);

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
        public async Task<IEnumerable<Subscription>>    GetSubscriptions()
        {
            Contract.Requires(IsAuthorized);

            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = "YoutubeWatcher"
            });

            List<Subscription> channels = new List<Subscription>();
            var nextPageToken = "";
            while (nextPageToken != null)
            {

                var channelsListRequest = _youtubeService.Subscriptions.List("snippet,contentDetails");
                channelsListRequest.MaxResults = 50;
                channelsListRequest.Mine = true;
                channelsListRequest.PageToken = nextPageToken;

                // Retrieve the list of videos uploaded to the authenticated user's channel.
                SubscriptionListResponse channelsListResponse = await channelsListRequest.ExecuteAsync();

                channels.AddRange(channelsListResponse.Items.ToArray());
                nextPageToken = channelsListResponse.NextPageToken;
            }

            return channels.ToArray();
        }


        public async Task<IEnumerable<Channel>> GetChannelsFromSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            Contract.Requires(IsAuthorized);

            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = "YoutubeWatcher"
            });


            //var channelsListRequest = _youtubeService.Channels.List("snippet,contentDetails");
            //channelsListRequest.Id = String.Join(",", subscriptions.Select(s=>s.Snippet.ResourceId.ChannelId));
            //channelsListRequest.MaxResults = 50;
            ////channelsListRequest.Mine = true;

            //// Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
            //var channelsListResponse = await channelsListRequest.ExecuteAsync();


            var channels = new List<Channel>();
            var nextPageToken = "";
            while (nextPageToken != null)
            {

                var channelsListRequest = _youtubeService.Channels.List("snippet,contentDetails");
                channelsListRequest.Id = String.Join(",", subscriptions.Select(s => s.Snippet.ResourceId.ChannelId));
                channelsListRequest.MaxResults = 50;
                channelsListRequest.PageToken = nextPageToken;

                // Retrieve the list of videos uploaded to the authenticated user's channel.
                var channelsListResponse = await channelsListRequest.ExecuteAsync();

                channels.AddRange(channelsListResponse.Items.ToArray());
                nextPageToken = channelsListResponse.NextPageToken;
            }


            return channels.ToArray();
        }


        /// <summary>
        /// Gets List of channel playlists
        /// </summary>
        /// <param name="playlistId">Id of playlist</param>
        /// <param name="cancellationToken"></param>
        /// <param name="reportProgress">returns amount of items recved</param>
        /// <returns></returns>
        public async Task<IEnumerable<PlaylistItem>>    GetPlayListItems(string playlistId, CancellationToken cancellationToken, Action<double> reportProgress = null)
        {
            Contract.Assert(_youtubeService != null);
            Contract.Assert(IsAuthorized);

            {
                var uploadsListId = playlistId;
                List<PlaylistItem> playlistItems = new List<PlaylistItem>();
                var nextPageToken = "";
                while (nextPageToken != null)
                {
                    if(cancellationToken.IsCancellationRequested)
                        break;

                    var playlistItemsListRequest = _youtubeService.PlaylistItems.List("snippet,contentDetails");
                    playlistItemsListRequest.PlaylistId = uploadsListId;
                    playlistItemsListRequest.MaxResults = 50;
                    playlistItemsListRequest.PageToken = nextPageToken;
                    

                    // Retrieve the list of videos uploaded to the authenticated user's channel.
                    var playlistItemsListResponse =  await playlistItemsListRequest.ExecuteAsync(cancellationToken);



                    playlistItems.AddRange(playlistItemsListResponse.Items.ToArray());

                    if (reportProgress != null)
                    {
                        Debug.Assert(playlistItemsListResponse.PageInfo.TotalResults != null, "playlistItemsListResponse.PageInfo.TotalResults != null");
                        reportProgress(playlistItems.Count*100 / (double)playlistItemsListResponse.PageInfo.TotalResults);
                    }

                    nextPageToken = playlistItemsListResponse.NextPageToken;
                }


                if (reportProgress != null) reportProgress(100.0);
                return playlistItems;
            }
        }

        /// <summary>
        /// Gets List of channel playlists
        /// </summary>
        /// <param name="channelId">Id of channel</param>
        /// <returns></returns>
        public async Task<IEnumerable<Playlist>>        GetPlayLists(string channelId)
        {
            Contract.Assert(_youtubeService != null);
            Contract.Assert(IsAuthorized);
            //var plOfUser = _youtubeService.Playlists.List("snippet,contentDetails");
            //plOfUser.MaxResults = 50;
            //plOfUser.ChannelId = channelId;

            //var playlists = await plOfUser.ExecuteAsync();

            var playlists = new List<Playlist>();
            var nextPageToken = "";
            while (nextPageToken != null)
            {

                var plOfUser = _youtubeService.Playlists.List("snippet,contentDetails");
                plOfUser.MaxResults = 50;
                plOfUser.ChannelId = channelId;
                plOfUser.PageToken = nextPageToken;

                // Retrieve the list of videos uploaded to the authenticated user's channel.
                var playlistItemsListResponse = await plOfUser.ExecuteAsync();

                playlists.AddRange(playlistItemsListResponse.Items.ToArray());
                nextPageToken = playlistItemsListResponse.NextPageToken;
            }


            return playlists.ToArray();
        }

        /// <summary>
        /// Gets List of channel playlists
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Playlist>> GetPlayList(string playlistId)
        {
            Contract.Assert(_youtubeService != null);
            Contract.Assert(IsAuthorized);

            var plOfUser = _youtubeService.Playlists.List("snippet,contentDetails");
            plOfUser.Id = playlistId;
            plOfUser.MaxResults = 1;

            var playlists = await plOfUser.ExecuteAsync();

            return playlists.Items.ToArray();
        }

        public async Task<Channel> GetOwnChannel()
        {
            Contract.Assert(_youtubeService != null);
            Contract.Assert(IsAuthorized);

            var channelsService = _youtubeService.Channels.List("snippet,contentDetails");
            channelsService.Mine = true;

            var channel = await channelsService.ExecuteAsync();

            return channel.Items.First();
        } 




    }
}
