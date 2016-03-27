using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.Migration;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit;

// ReSharper disable ConsiderUsingAsyncSuffix

namespace Jalex.Repository.Test.Migration
{
    public class RepositoryDataMigratorTests
    {
        private readonly IFixture _fixture;
        private readonly string _tableName;
        private readonly Version _targetVersion;

        private readonly ITableDataMigrator _migratorSubstitute;
        private readonly IQueryableRepository<TableVersion> _versionRepoSubstitute;

        public RepositoryDataMigratorTests()
        {
            _fixture = new Fixture();

            _tableName = _fixture.Create<string>();
            _targetVersion = new Version(10, 10);

            _migratorSubstitute = Substitute.For<ITableDataMigrator>();
            _migratorSubstitute.TargetTable.Returns(_tableName);
            _migratorSubstitute.TargetVersion.Returns(_targetVersion);
            _migratorSubstitute.ExecuteAsync()
                              .Returns(Task.FromResult(0));

            _fixture.Inject<IEnumerable<ITableDataMigrator>>(new[] { _migratorSubstitute });

            _versionRepoSubstitute = Substitute.For<IQueryableRepository<TableVersion>>();
            _versionRepoSubstitute.SaveManyAsync(null, WriteMode.Upsert)
                                  .ReturnsForAnyArgs(Task.FromResult(new OperationResult<Guid>().ToEnumerable()));

            _fixture.Inject(_versionRepoSubstitute);
        }

        [Fact]
        public async Task MigratesWhenVersionIsMissing()
        {
            _versionRepoSubstitute.FirstOrDefaultAsync(null)
                                  .ReturnsForAnyArgs((TableVersion)null);

            var sut = _fixture.Create<RepositoryDataMigrator>();
            await sut.MigrateAsync();
#pragma warning disable 4014
            _versionRepoSubstitute.ReceivedWithAnyArgs()
                                  .FirstOrDefaultAsync(null);
            _migratorSubstitute.ReceivedWithAnyArgs()
                               .ExecuteAsync();
#pragma warning restore 4014
        }

        [Fact]
        public async Task MigratesWhenVersionIsLower()
        {
            _versionRepoSubstitute.FirstOrDefaultAsync(null)
                     .ReturnsForAnyArgs(new TableVersion { Id = Guid.NewGuid(), TableName = _tableName, Version = new Version(_targetVersion.Major, _targetVersion.Minor - 1) });

            var sut = _fixture.Create<RepositoryDataMigrator>();
            await sut.MigrateAsync();
#pragma warning disable 4014
            _migratorSubstitute.ReceivedWithAnyArgs()
                               .ExecuteAsync();
#pragma warning restore 4014
        }

        [Fact]
        public async Task DoesNotMigrateWhenVersionIsSame()
        {
            _versionRepoSubstitute.FirstOrDefaultAsync(null)
                     .ReturnsForAnyArgs(new TableVersion { Id = Guid.NewGuid(), TableName = _tableName, Version = _targetVersion });

            var sut = _fixture.Create<RepositoryDataMigrator>();
            await sut.MigrateAsync();
#pragma warning disable 4014
            _migratorSubstitute.DidNotReceiveWithAnyArgs()
                               .ExecuteAsync();
#pragma warning restore 4014
        }
    }
}
