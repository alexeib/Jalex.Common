using Machine.Specifications;

namespace Jalex.Crypto.Test
{
    public abstract class Sha1HashProviderSpec
    {
        protected static Sha1HashProvider _hashProvider;
        protected static string ExpectedHash = "603d7562d66f2c1f68e8aa6f4cf0f7ce77492ce7";
        protected static string Text = "This Hash is Gonna be Awesome!!!";

        Establish context = () =>
        {
            _hashProvider = new Sha1HashProvider();            
        };

        Cleanup after = () => _hashProvider.Dispose();
    }

    [Subject(typeof(Sha1HashProviderSpec))]
    public class When_hashing_text : Sha1HashProviderSpec
    {        
        protected static string Hash;

        Because of = () => Hash = _hashProvider.GetHash(Text);

        It should_produce_expected_hash = () => Hash.ShouldEqual(ExpectedHash);
    }
}
