using System;
using System.Threading.Tasks;
using FluentAssertions;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Repository.Migration;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Repository.Test.Migration
{
    public abstract class IDataMigratorTests
    {
        private readonly IFixture _fixture;
        private readonly string _pattern;
        private readonly string _replacement;

        private const string _notPattern = "123";

        protected IDataMigratorTests(IFixture fixture, string pattern, string replacement)
        {
            _fixture = fixture;
            _pattern = pattern;
            _replacement = replacement;

            if(_notPattern.Contains(pattern))
                throw new InvalidOperationException("pattern cannot be contained in " + _notPattern);
        }

        private async Task createTestObjectsAsync(ISimpleRepository<TestObject> repo, Guid testObject1Id, Guid testObject2Id)
        {
            var testObject1 = _fixture.Build<TestObject>()
                .With(o => o.Id, testObject1Id)
                .With(o => o.Name, _pattern)
                .Create();

            var testObject2 = _fixture.Build<TestObject>()
                .With(o => o.Id, testObject2Id)
                .With(o => o.Name, _notPattern)
                .Create();

            repo.SaveManyAsync(new[] {testObject1, testObject2}, WriteMode.Upsert)
                .Wait();
        }

        [Fact]
        public async Task MigratesCorrectly()
        {
            var repo = _fixture.Create<ISimpleRepository<TestObject>>();
            var testObject1Id = _fixture.Create<Guid>();
            var testObject2Id = _fixture.Create<Guid>();

            try
            {
                await createTestObjectsAsync(repo, testObject1Id, testObject2Id);

                var sut = _fixture.Create<IDataMigrator>();
                sut.Migrate();

                var obj1 = await repo.GetByIdAsync(testObject1Id);
                var obj2 = await repo.GetByIdAsync(testObject2Id);

                obj1.Name.Should()
                    .Be(_replacement);
                obj2.Name.Should()
                    .Be(_notPattern);
            }
            finally
            {
                await repo.DeleteManyAsync(new[] {testObject1Id, testObject2Id});
            }
        }
    }
}
