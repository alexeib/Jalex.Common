using System;
using System.IO;

namespace Jalex.Infrastructure.IO
{
    public class DiscriminatingFileLoader : IFileLoader
    {
        #region Implementation of IFileLoader

        public Stream LoadStream(string path)
        {
            IFileLoader proxy = ChooseProxyBasedOnPath(path);
            var stream = proxy.LoadStream(path);
            return stream;
        }        

        #endregion

        public IFileLoader ChooseProxyBasedOnPath(string path)
        {
            Uri uri;
            if (Uri.TryCreate(path, UriKind.Absolute, out uri))
            {
                switch (uri.Scheme)
                {
                    case "ftp":
                        return new FtpFileLoader();
                    case "http":
                    case "https":
                        return new HttpFileLoader();
                    case "file":
                        return new LocalFileLoader();
                }
            }

            throw new FileLoadException("Could not determine file loader for path " + path);
        }
    }
}
