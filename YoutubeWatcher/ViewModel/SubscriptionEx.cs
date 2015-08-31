using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using Google.Apis.YouTube.v3.Data;
using Youtube;

namespace YoutubeWatcher.ViewModel
{
    public class SubscriptionEx
    {
        public SubscriptionEx()
        {
            Playlists = new ObservableCollection<PlaylistEx>();
        }

        public bool PlayListsAreLoaded { get; set; }
        
        public ObservableCollection<PlaylistEx> Playlists { get; set; }

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
                Playlists = new ObservableCollection<PlaylistEx>();

            if (Playlists.Any())
                Playlists.Clear();

            var pl = await GetPlaylists();

            foreach (var playlist in pl)
            {
                Playlists.Add(new PlaylistEx() {Playlist = playlist, Subscription = Subscription});
            }

            PlayListsAreLoaded = true;

        }
        
    }
}