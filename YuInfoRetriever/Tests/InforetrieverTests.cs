using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Youtube;
using YuInfoRetriever.Authorization;

namespace YuInfoRetriever.Tests
{
    [TestClass]
    public class InforetrieverTests
    {
        [TestMethod]
        [ExpectedException(typeof (NullReferenceException))]
        public async Task CheckAuthorization_Failed()
        {
            var manager = new YInfoRetriever();

            await manager.Authorize(null);
        }

        [TestMethod]
        public async Task CheckAuthorization_Succeed()
        {
            var manager = new YInfoRetriever();

            var fileProvaider = new JsonFileAuthProvider();
            fileProvaider.SetParams(@"D://client_secrets.json");

            await manager.Authorize(fileProvaider);

            Assert.IsTrue(manager.IsAuthorized);
        }

        [TestMethod]
        public async Task CheckGettingChannels()
        {
            var manager = new YInfoRetriever();

            var fileProvaider = new JsonFileAuthProvider();
            fileProvaider.SetParams(@"D://client_secrets.json");

            manager.Authorize(fileProvaider).Wait();

            var sub = await manager.GetSubscriptions();

            Assert.IsNotNull(sub);
        }


        [TestMethod]
        public async Task CheckGettingPlaylists()
        {
            var manager = new YInfoRetriever();

            var fileProvaider = new JsonFileAuthProvider();
            fileProvaider.SetParams(@"D://client_secrets.json");

            manager.Authorize(fileProvaider).Wait();

            var sub = await manager.GetSubscriptions();
            if (!sub.Any())
                return;

            var channel = sub.First();

            var plList = await manager.GetPlayLists(channel.Snippet.ChannelId);

            Assert.IsNotNull(plList);
        }

        [TestMethod]
        public async Task CheckGettingPlaylistItems()
        {
            var manager = new YInfoRetriever();

            var fileProvaider = new JsonFileAuthProvider();
            fileProvaider.SetParams(@"D://client_secrets.json");

            manager.Authorize(fileProvaider).Wait();

            var sub = await manager.GetSubscriptions();
            if (!sub.Any())
                return;

            var channel = sub.First();

            var plList = await manager.GetPlayLists(channel.Snippet.ChannelId);

            if (!plList.Any())
                return;

            var pl = plList.First();

            var items = await manager.GetPlayListItems(pl.Id, CancellationToken.None);

            Assert.IsNotNull(items);
        }
    }
}