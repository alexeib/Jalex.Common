﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jalex.Repository.Cassandra;
using Jalex.Repository.IdProviders;
using Jalex.Repository.MongoDB;
using MongoDB.Bson;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class CassandraRepositoryTests : IQueryableRepositoryTests
    {
        public CassandraRepositoryTests()
            : base(createRepository(), createFixture())
        {
            
        }

        private static CassandraRepository<TestObject> createRepository()
        {
            var cassandraIdGenerator = new GuidIdProvider();
            return new CassandraRepository<TestObject>(cassandraIdGenerator);
        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();

            GuidIdProvider idProvider = new GuidIdProvider();

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<string>(idProvider.GenerateNewId);

            return fixture;
        }
    }
}
