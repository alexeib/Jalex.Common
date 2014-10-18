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

            switch (RepositoryType)
            {
                case RepositoryType.Cassandra:

                    builder.RegisterType<GuidIdProvider>()
                                       .As<IIdProvider>()
                                       .InstancePerLifetimeScope();                    

                    builder.RegisterGeneric(typeof(CassandraRepository<>))
                                        .Named("repository", typeof(IQueryableRepository<>))
                                        .InstancePerLifetimeScope();

                    break;
                case RepositoryType.Mongo:

                    builder.RegisterType<ObjectIdIdProvider>()
                                       .As<IIdProvider>()
                                       .InstancePerLifetimeScope();

                    builder.RegisterGeneric(typeof(MongoDBRepository<>))
                                        .Named("repository", typeof(IQueryableRepository<>))
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
