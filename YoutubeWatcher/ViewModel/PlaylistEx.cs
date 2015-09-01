using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using Google.Apis.YouTube.v3.Data;
using Youtube;

namespace YoutubeWatcher.ViewModel
{
    public class PlaylistEx
    {
        public PlaylistEx()
        {
            PlaylistItems = new ObservableCollection<PlaylistItem>();
        }


        public bool PlayListsItemsAreLoaded { get; set; }

        public Channel Subscription { get; set; }

        /// <summary>
        /// Gets or sets original <see cref="Playlist"/>
        /// </summary>
        public Playlist Playlist { get; set; }

        public ObservableCollection<PlaylistItem> PlaylistItems { get; set; }

        private async Task<IEnumerable<PlaylistItem>> GetPlaylistItems(CancellationToken cancellationToken)
        {

            var plId = Playlist.Id;

            var you = SimpleIoc.Default.GetInstance<YInfoRetriever>();

            if (you.IsAuthorized)
            {
                var res = await you.GetPlayListItems(plId, cancellationToken, ReportProgress);

                return res.OrderByDescending(r=>r.Snippet.PublishedAt.Value.Date);
            }
            return null;
        }

        private void ReportProgress(double progress)
        {
            var mvm = SimpleIoc.Default.GetInstance<MainViewModel>();
            Application.Current.Dispatcher.Invoke(() =>
            {
                mvm.Status = Math.Round(progress, 1).ToString() + "% progress.";
            });

        }


        public async Task RefreshPlayListItems(CancellationToken cancellationToken)
        {
            PlayListsItemsAreLoaded = false;

            if (PlaylistItems == null)
                PlaylistItems = new ObservableCollection<PlaylistItem>();

            if (PlaylistItems.Any())
                PlaylistItems.Clear();

            var pl = await GetPlaylistItems(cancellationToken);

            foreach (var item in pl)
            {
                PlaylistItems.Add(item);
            }

            PlayListsItemsAreLoaded = true;
        }
    }
}