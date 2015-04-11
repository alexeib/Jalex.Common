using FluentAssertions;
using Jalex.Scripting.Python;
using Jalex.TestUtils.xUnit;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Scripting.Test.Python
{
    public class PythonFactoryTests
    {
        private readonly IFixture _fixture;
        private readonly string _scriptLocation = XUnitUtils.GetDeployedFileLocation(@"Python\PythonTests.py");

        public PythonFactoryTests()
        {
            _fixture = new Fixture();
            _fixture.Register<IScriptExecutor>(() => _fixture.Create<PythonExecutor>());
        }

        [Fact]
        public void Can_Create_Class_With_Arguments()
        {
            var arg1 = _fixture.Create<string>();
            var arg2 = _fixture.Create<string>();

            var sut = _fixture.Create<IScriptExecutor>();
            var createdClass = sut.CreateClass<dynamic>(_scriptLocation, "SimpleTest", arg1, arg2);
            bool isNotNull = createdClass != null;
            isNotNull.Should()
                     .BeTrue();

            var createdArg0 = (string)createdClass.Arg0;
            var createdArg1 = (string)createdClass.Arg1;
            var createdArg2 = (string)createdClass.Arg2;

            createdArg0.Should()
                .Be("Hi");
            createdArg1.Should()
                .Be(arg1);
            createdArg2.Should()
                .Be(arg2);
        }

        [Fact]
        public void Can_Call_Method_With_Argument()
        {
            var arg = _fixture.Create<string>();

            var sut = _fixture.Create<IScriptExecutor>();
            var result = sut.CallMethod<string>(_scriptLocation, "TestMethod", arg);

            result.Should()
                  .Be("got " + arg);
        }
    }
}
