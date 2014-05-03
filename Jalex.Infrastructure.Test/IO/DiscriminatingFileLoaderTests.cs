using FluentAssertions;
using Jalex.Infrastructure.IO;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Infrastructure.Test.IO
{
    public class DiscriminatingFileLoaderTests
    {
        protected IFixture _fixture;

        public DiscriminatingFileLoaderTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void HttpResourceReturnsHttpFileLoader()
        {
            var loader = _fixture.Create<DiscriminatingFileLoader>();

            const string httpUri = "http://google.com/somefile.txt";

            var proxy = loader.ChooseProxyBasedOnPath(httpUri);
            proxy.Should().BeOfType<HttpFileLoader>();
        }

        [Fact]
        public void HttpsResourceReturnsHttpFileLoader()
        {
            var loader = _fixture.Create<DiscriminatingFileLoader>();

            const string httpsUri = "https://google.com/somefile.txt";

            var proxy = loader.ChooseProxyBasedOnPath(httpsUri);
            proxy.Should().BeOfType<HttpFileLoader>();
        }

        [Fact]
        public void FtpResourceReturnsFtpFileLoader()
        {
            var loader = _fixture.Create<DiscriminatingFileLoader>();

            const string ftpUri = "ftp://google.com/somefile.txt";

            var proxy = loader.ChooseProxyBasedOnPath(ftpUri);
            proxy.Should().BeOfType<FtpFileLoader>();
        }

        [Fact]
        public void LocalFileResourceReturnsLocalFileLoader()
        {
            var loader = _fixture.Create<DiscriminatingFileLoader>();

            const string fileUri = "C:\\somefile.txt";

            var proxy = loader.ChooseProxyBasedOnPath(fileUri);
            proxy.Should().BeOfType<LocalFileLoader>();
        }
    }
}
