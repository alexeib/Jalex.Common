using System.IO;
using System.Net;

namespace Jalex.Infrastructure.IO
{
    public class FtpFileLoader : IFileLoader
    {
        #region Implementation of IFileLoader

        public Stream LoadStream(string path)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential("anonymous", "janeDoe@contoso.com");

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            if (responseStream == null)
            {
                response.Close();
                throw new FileLoadException("Failed to download file from " + path);
            }

            return responseStream;
        }

        #endregion
    }
}
