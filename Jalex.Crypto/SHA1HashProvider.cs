using System.Security.Cryptography;
using System.Text;

namespace Jalex.Crypto
{
    public class Sha1HashProvider : IHashProvider
    {
        private readonly SHA1 _sha1;
        public Sha1HashProvider()
        {
            _sha1 = new SHA1CryptoServiceProvider();
        }

        public string GetHash(string text)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] raw = encoding.GetBytes(text);
            byte[] result = _sha1.ComputeHash(raw);
            var hexString = getHexStringFromBytes(result);

            return hexString;
        }

        private static string getHexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        public void Dispose()
        {
            _sha1.Dispose();
        }
    }
}
