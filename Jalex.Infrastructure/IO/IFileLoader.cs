using System.IO;

namespace Jalex.Infrastructure.IO
{
    public interface IFileLoader
    {
        Stream LoadStream(string path);
    }
}
