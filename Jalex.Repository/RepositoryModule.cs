using System;
using Autofac;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.Cassandra;
using Jalex.Repository.IdProviders;
using Jalex.Repository.MongoDB;

namespace Jalex.Repository
{
    public class RepositoryModule : Module
    {
        public RepositoryType RepositoryType { get; set; }

        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GuidIdProvider>()
                   .As<IIdProvider>()
                   .InstancePerLifetimeScope();

            switch (RepositoryType)
            {
                case RepositoryType.Cassandra:
                    builder.RegisterGeneric(typeof (CassandraRepository<>))
                           .Named("raw-repository", typeof (IQueryableRepository<>))
                           .AsImplementedInterfaces()
                           .Named("repository", typeof (IReader<>))
                           .Named("repository", typeof (IWriter<>))
                           .Named("repository", typeof (IDeleter<>))
                           .Named("repository", typeof (ISimpleRepository<>))
                           .Named("repository", typeof(IQueryableReader<>))
                           .Named("repository", typeof (IQueryableRepository<>))
                           .InstancePerLifetimeScope();

                    break;
                case RepositoryType.Mongo:
                    builder.RegisterGeneric(typeof (MongoDBRepository<>))
                           .Named("raw-repository", typeof (IQueryableRepository<>))
                           .AsImplementedInterfaces()
                           .Named("repository", typeof (IReader<>))
                           .Named("repository", typeof (IWriter<>))
                           .Named("repository", typeof (IDeleter<>))
                           .Named("repository", typeof (ISimpleRepository<>))
                           .Named("repository", typeof(IQueryableReader<>))
                           .Named("repository", typeof (IQueryableRepository<>))
                           .InstancePerLifetimeScope();

                    builder.RegisterType<MongoDBFileRepository>()
                           .As<IFileRepository>()
                           .InstancePerLifetimeScope();

                    break;
                default:
                    throw new ArgumentOutOfRangeException("Repository of type " + RepositoryType + " is not supported");

            }
        }

        #endregion
    }
}
