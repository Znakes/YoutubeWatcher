using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeWatcher
{
    /// <summary>
    /// YouTube Data API v3 sample: retrieve my uploads.
    /// Relies on the Google APIs Client Library for .NET, v1.7.0 or higher.
    /// See https://code.google.com/p/google-api-dotnet-client/wiki/GettingStarted
    /// </summary>
    internal class RetrieveChannels
    {
        public async Task<IEnumerable<Channel>> GetPlaylist()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for read-only access to the authenticated 
                    // user's account, but not other types of account access.
                    new[] { YouTubeService.Scope.YoutubeReadonly },"steel1991@gmail.com",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "YoutubeWatcher"
            });


            SubscriptionsResource.ListRequest channelsListRequest = youtubeService.Subscriptions.List("snippet");

            //var channelsListRequest = youtubeService.Channels.List("contentDetails");
            channelsListRequest.Mine = true;

            // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
            var channelsListResponse = await channelsListRequest.ExecuteAsync();




            foreach (Subscription channel in channelsListResponse.Items)
            {
                // From the API response, extract the playlist ID that identifies the list
                // of videos uploaded to the authenticated user's channel.



                var resourceListId = channel.Snippet.ResourceId.ChannelId;


                Console.WriteLine(@"Videos in list {0}", resourceListId);


                var plOfUser = youtubeService.Playlists.List("snippet");
                plOfUser.ChannelId = resourceListId;

                var playlists = await plOfUser.ExecuteAsync();

                foreach (var playlist in playlists.Items)
                {

                    var uploadsListId = playlist.Id;

                    var nextPageToken = "";
                    while (nextPageToken != null)
                    {
                        var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
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
