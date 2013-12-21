using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Jalex.Crypto
{
    public class TripleDesCryptoProvider : ICryptoProvider
    {
        private static byte[] _iv = new byte[] { 0x01, 0x02, 0x04, 0x05, 0x07, 0x08, 0xba, 0xad };

        public string Encrypt(string text, string key)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] raw = encoding.GetBytes(text);

            string strKey;
            var encProvider = createEncodingProvider(key, out strKey);

            using (MemoryStream mStream = new MemoryStream())
            {
                using (
                    CryptoStream cStream = new CryptoStream(mStream,
                        encProvider.CreateEncryptor(encoding.GetBytes(strKey), _iv), CryptoStreamMode.Write))
                {
                    cStream.Write(raw, 0, raw.Length);
                    cStream.FlushFinalBlock();

                    string enc = Convert.ToBase64String(mStream.GetBuffer(), 0, (int)mStream.Length);
                    return enc;
                }
            }
        }

        private static TripleDESCryptoServiceProvider createEncodingProvider(string key, out string strKey)
        {
            strKey = key;
            int lenKey = key.Length;
            int rem = key.Length & 0x00000007; // % 8 bytes (64 bits)
            if (rem != 0) //  Encryption key should be multiple of 64 bits
            {
                int pad = 8 - rem;
                if (lenKey + pad < 16) pad = 16 - lenKey;
                strKey = strKey.PadRight(lenKey + pad);
            }
            if (strKey.Length > 24) //  Maximum length 192 bit
                strKey = strKey.Substring(0, 24);

            TripleDESCryptoServiceProvider encProvider = new TripleDESCryptoServiceProvider
            {
                KeySize = strKey.Length*8,
                Padding = PaddingMode.ANSIX923
            };
            return encProvider;
        }

        public string Decrypt(string text, string key)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] raw = Convert.FromBase64String(text);

            string strKey;
            var encProvider = createEncodingProvider(key, out strKey);

            using (MemoryStream mStream = new MemoryStream())
            {
                using (
                    CryptoStream cStream = new CryptoStream(mStream,
                        encProvider.CreateDecryptor(encoding.GetBytes(strKey), _iv), CryptoStreamMode.Write))
                {

                    cStream.Write(raw, 0, raw.Length);
                    cStream.FlushFinalBlock();

                    string res = encoding.GetString(mStream.GetBuffer(), 0, (int)mStream.Length);
                    return res;

                }
            }
        }

        public void Dispose()
        {
            // no need to dispose
        }
    }
}
