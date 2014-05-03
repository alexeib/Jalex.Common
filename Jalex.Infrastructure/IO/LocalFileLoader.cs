using System.IO;

namespace Jalex.Infrastructure.IO
{
    public class LocalFileLoader : IFileLoader
    {
        #region Implementation of IFileLoader

        public Stream LoadStream(string path)
        {
            FileStream stream = File.OpenRead(path);
            return stream;
        }

        #endregion
    }
}
