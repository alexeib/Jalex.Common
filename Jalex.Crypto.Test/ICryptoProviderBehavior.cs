using Machine.Specifications;

namespace Jalex.Crypto.Test
{
    [Behaviors]
    public class ICryptoProviderEncryptionBehavior
    {
        protected static string PlainText;
        protected static string EncryptedText;

        It should_create_non_empty_encrypted_text = () => EncryptedText.ShouldNotBeEmpty();
        It should_encrypt_the_plain_text = () => PlainText.ShouldNotEqual(EncryptedText);
    }

    [Behaviors]
    public class ICryptoProviderDecryptionBehavior
    {
        protected static string PlainText;
        protected static string DecryptedText;

        It should_decrypt_text_correctly = () => PlainText.ShouldEqual(DecryptedText);
    }
}
