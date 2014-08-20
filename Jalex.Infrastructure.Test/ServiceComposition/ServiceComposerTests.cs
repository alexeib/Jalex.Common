using FluentAssertions;
using Jalex.Infrastructure.ServiceComposition;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Infrastructure.Test.ServiceComposition
{
    public class ServiceComposerTests
    {
        private readonly IFixture _fixture;

        public ServiceComposerTests()
        {
            _fixture = new Fixture();
            injectComposableServiceSubstitute(_fixture);
        }

        private void injectComposableServiceSubstitute(IFixture fixture)
        {
            var serviceSub = Substitute.For<IComposableService<string>>();
            serviceSub.CanProcess(null).Returns(false);
            serviceSub.CanProcess(Arg.Is<string>(s => !string.IsNullOrEmpty(s))).Returns(true);
            fixture.Inject(serviceSub);
        }

        [Fact]
        public void Cannot_Process_If_Underlying_Services_Cannot()
        {
            var sut = _fixture.Create<ServiceComposer<string>>();
            var canProcess = sut.CanProcess(null);
            canProcess.Should().BeFalse();
        }

        [Fact]
        public void Can_Process_If_Underlying_Service_Can()
        {
            var arg = _fixture.Create<string>();
            var sut = _fixture.Create<ServiceComposer<string>>();
            var canProcess = sut.CanProcess(arg);
            canProcess.Should().BeTrue();
        }

        [Fact]
        public void Throws_Exceptions_If_Asked_To_Process_What_It_Cannot()
        {
            var sut = _fixture.Create<ServiceComposer<string>>();
            sut.Invoking(s => s.Process(null)).ShouldThrow<ComposableServiceNotFoundException<string>>();
        }

        [Fact]
        public void Calls_Process_On_Underlying_Service()
        {
            var composableService = _fixture.Create<IComposableService<string>>();
            var arg = _fixture.Create<string>();
            var sut = _fixture.Create<ServiceComposer<string>>();
            sut.Process(arg);
            
            composableService.Received().Process(arg);
        }
    }
}
