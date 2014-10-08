using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Jalex.Infrastructure.Dataflow;
using Jalex.Infrastructure.Extensions;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Infrastructure.Test.Dataflow
{
    public class FilterBlockTests
    {
        private readonly IFixture _fixture;

        public FilterBlockTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Passes_Through_Unfiltered_Messages()
        {
            var entity = _fixture.Create<Entity>();
            var filterBlock = new FilterBlock<Entity>(e => true);

            filterBlock.Post(entity);
            filterBlock.Complete();

            var data = filterBlock.GetDataFromBlock();

            data
                .Should()
                .HaveCount(1)
                .And
                .Contain(entity);
        }

        [Fact]
        public void Declines_Filtered_Messages_If_Swallow_Flag_Is_False()
        {
            var entity = _fixture.Create<Entity>();
            var filterBlock = new FilterBlock<Entity>(e => false);

            filterBlock.Post(entity);
            filterBlock.Complete();
            
            var data = filterBlock.GetDataFromBlock();

            data
                .Should()
                .BeEmpty();
        }
    }
}
