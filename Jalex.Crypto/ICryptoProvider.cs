using System;

namespace Jalex.Crypto
{
    public interface ICryptoProvider : IDisposable
    {
        /// <summary>
        /// Encrypts a plaintext using a given key
        /// </summary>
        /// <param name="text">The text to encrypt</param>
        /// <param name="key">The key used for encryption</param>
        /// <returns>Encrypted text</returns>
        string Encrypt(string text, string key);
        /// <summary>
        /// Decrypts cryptotext using a given key
        /// </summary>
        /// <param name="text">The crypto text to decrypt</param>
        /// <param name="key">The key to use for decryption</param>
        /// <returns>Decrypted text</returns>
        string Decrypt(string text, string key);
    }
}
