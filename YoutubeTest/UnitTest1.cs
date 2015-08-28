using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Youtube;

namespace YoutubeTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async void YInfoRetriever_Authorize_GetNull_ThrowEx()
        {
            Youtube.YInfoRetriever retriever = new YInfoRetriever();

            Assert.IsFalse(await retriever.Authorize(null));

        }
    }
}
