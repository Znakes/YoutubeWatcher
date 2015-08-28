using System.IO;

namespace YuInfoRetriever.Authorization
{
    public interface IAuthProvider
    {
        Stream GetAuthDataStream();
    }
}