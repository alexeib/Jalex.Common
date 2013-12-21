using Machine.Specifications;

namespace Jalex.Crypto.Test
{
    public abstract class TripleDesCryptoProviderSpec
    {
        protected static ICryptoProvider _cryptoProvider;
        
        protected static string Key;
        protected static string PlainText;
        protected static string EncryptedText;
        protected static string DecryptedText;

        Establish context = () =>
        {
            _cryptoProvider = new TripleDesCryptoProvider();
            Key = "My Super Secret Key";
            PlainText = "This Encryption is Gonna be Awesome!!!";
        };

        Cleanup after = () => _cryptoProvider.Dispose();
    }

    [Subject(typeof (ICryptoProvider))]
    public class When_encrypting_plaintext : TripleDesCryptoProviderSpec
    {
        Because of = () => EncryptedText = _cryptoProvider.Encrypt(PlainText, Key);
        
        Behaves_like<ICryptoProviderEncryptionBehavior> an_encryptor;
    }

    [Subject(typeof(ICryptoProvider))]
    public class When_decrypting_plaintext : TripleDesCryptoProviderSpec
    {
        Establish context = () => EncryptedText = _cryptoProvider.Encrypt(PlainText, Key);

        Because of = () => DecryptedText = _cryptoProvider.Decrypt(EncryptedText, Key);

        Behaves_like<ICryptoProviderDecryptionBehavior> a_decryptor;
    }
}
