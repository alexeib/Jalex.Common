using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.Test.Objects;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Repository.Test
{
    public abstract class IQueryableRepositoryWithTtlTests<T> : IQueryableRepositoryTests<T>
        where T : class, IObjectWithIdAndName, new()
    {
        private readonly IQueryableRepositoryWithTtl<T> _queryableWithTtl;

        protected IQueryableRepositoryWithTtlTests(
            IFixture fixture)
            : base(fixture)
        {
            _queryableWithTtl = _fixture.Create<IQueryableRepositoryWithTtl<T>>();
        }


        [Fact]
        public virtual void Entity_Expires()
        {
            var sampleEntity = _sampleTestEntitys.First();

            var createResult = _queryableWithTtl.SaveAsync(sampleEntity, WriteMode.Upsert, TimeSpan.FromSeconds(1))
                                                    .Result;

            createResult.Success.Should().BeTrue();
            createResult.Value.Should()
                        .NotBeEmpty();
            sampleEntity.Id.Should().NotBeEmpty();
            createResult.Messages.Should().BeEmpty();

            Thread.Sleep(TimeSpan.FromSeconds(1));

            T retrieved = _queryableWithTtl.GetByIdAsync(sampleEntity.Id).Result;

            retrieved.Should()
                     .BeNull();
        }

        [Fact]
        public virtual void Updates_Ttl()
        {
            var sampleEntity = _sampleTestEntitys.First();

            var createResult = _queryableWithTtl.SaveAsync(sampleEntity, WriteMode.Upsert, TimeSpan.FromSeconds(2))
                                                    .Result;

            createResult.Success.Should().BeTrue();
            createResult.Value.Should()
                        .NotBeEmpty();
            sampleEntity.Id.Should().NotBeEmpty();
            createResult.Messages.Should().BeEmpty();

            _queryableWithTtl.UpdateTtlAsync(new[] {sampleEntity.Id}, TimeSpan.FromSeconds(25));

            Thread.Sleep(TimeSpan.FromSeconds(2));

            T retrieved = _queryableWithTtl.GetByIdAsync(sampleEntity.Id).Result;

            retrieved.Should()
                     .NotBeNull();
        }
    }
}
