using System;

namespace Jalex.Infrastructure.Repository.Migration
{
    public interface IDataMigrator
    {
        string TargetTable { get; }

        Version TargetVersion { get; }

        void Migrate();
    }
}