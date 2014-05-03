using System.IO;

namespace Jalex.Infrastructure.Extensions
{
    public static class StreamExtensions
    {
        public static string ReadToEndAndClose(this Stream stream)
        {
            string contents;

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (StreamReader reader = new StreamReader(stream))
            {
                contents = reader.ReadToEnd();
            }

            return contents;
        }
    }
}
