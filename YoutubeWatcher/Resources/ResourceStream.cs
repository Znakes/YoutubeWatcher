using System;
using System.IO;
using System.Windows;
using YuInfoRetriever.Authorization;

namespace YoutubeWatcher
{
    internal class ResourceStream : IAuthProvider
    {
        public string ResourcePath { get; private set; }

        public void SetParams(object param)
        {
            if (param != null && !string.IsNullOrEmpty(param.ToString()))
            {
                ResourcePath = param.ToString();
                return;
            }

            throw new NullReferenceException();
        }

        public Stream GetAuthDataStream()
        {
            if (string.IsNullOrEmpty(ResourcePath))
                throw new NullReferenceException("ResourcePath");

            //@"Resources/client_secrets.json"
            var sri = Application.GetResourceStream(new Uri(ResourcePath, UriKind.Relative));
            if (sri != null)
            {
                return sri.Stream;
            }

            throw new NullReferenceException("sri");
        }
    }
}