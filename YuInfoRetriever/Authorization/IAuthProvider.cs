using System.IO;

namespace YuInfoRetriever.Authorization
{
    public interface IAuthProvider
    {
        void SetParams(object param);
        Stream GetAuthDataStream();
    }
}