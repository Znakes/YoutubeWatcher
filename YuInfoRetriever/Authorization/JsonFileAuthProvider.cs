using System.Diagnostics.Contracts;
using System.IO;

namespace YuInfoRetriever.Authorization
{
    public class JsonFileAuthProvider : IAuthProvider
    {
        public string JsonFileWithSecret { get; private set; }

        public Stream GetAuthDataStream()
        {
            Contract.Requires(!string.IsNullOrEmpty(JsonFileWithSecret));

            return new FileStream(JsonFileWithSecret, FileMode.Open, FileAccess.Read);
        }

        public void SetParams(object param)
        {
            Contract.Requires(param != null);
            Contract.Requires(File.Exists(param.ToString()));

            JsonFileWithSecret = param.ToString();
        }
    }
}