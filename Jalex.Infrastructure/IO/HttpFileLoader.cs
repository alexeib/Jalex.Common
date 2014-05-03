using System.IO;
using System.Net;

namespace Jalex.Infrastructure.IO
{
    public class HttpFileLoader : IFileLoader
    {
        #region Implementation of IFileLoader

        public Stream LoadStream(string path)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();

            return stream;
        }

        #endregion
    }
}
