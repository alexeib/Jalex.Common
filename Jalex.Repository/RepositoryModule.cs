﻿using System;
using Autofac;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.Cassandra;
using Jalex.Repository.IdProviders;
using Jalex.Repository.MongoDB;
using Jalex.Repository.Utils;

namespace Jalex.Repository
{
    public class RepositoryModule : Module
    {
        public RepositoryType RepositoryType { get; set; }

        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ReflectedTypeDescriptorProvider>()
                                       .As<IReflectedTypeDescriptorProvider>()
                                       .InstancePerLifetimeScope();

            switch (RepositoryType)
            {
                case RepositoryType.Cassandra:

                    builder.RegisterType<GuidIdProvider>()
                                       .As<IIdProvider>()
                                       .InstancePerLifetimeScope();

                    builder.RegisterGeneric(typeof(CassandraRepository<>))
                                        .As(typeof(IQueryableRepository<>))
                                        .InstancePerLifetimeScope();

                    break;
                case RepositoryType.Mongo:

                    builder.RegisterType<ObjectIdIdProvider>()
                                       .As<IIdProvider>()
                                       .InstancePerLifetimeScope();

                    builder.RegisterGeneric(typeof(MongoDBRepository<>))
                                        .As(typeof(IQueryableRepository<>))
                                        .InstancePerLifetimeScope();

                    builder.RegisterType<MongoDBFileRepository>()
                                        .As<IFileRepository>()
                                        .InstancePerLifetimeScope();

                    break;
                default:
                    throw new NotSupportedException("Repository of type " + RepositoryType + " is not supported");

            }
        }

        #endregion
    }
}
