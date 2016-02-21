using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Jalex.Infrastructure.Containers;
using Jalex.Infrastructure.Extensions;
using MoreLinq;
using Ploeh.AutoFixture;
using Xunit;
using Xunit.Abstractions;

namespace Jalex.Infrastructure.Test.Containers
{
    public class MemoryUsageTests
    {
        private readonly ITestOutputHelper _output;
        private readonly IFixture _fixture;

        public MemoryUsageTests(ITestOutputHelper output)
        {
            _output = output;
            _fixture = new Fixture();
        }

        class Keyed
        {
            public string Key { get; set; }
        }

        class A: Keyed { public double Value { get; set; } }

        class B : Keyed { public long Value { get; set; } }

        class C : Keyed { public string Value { get; set; } }


        [Fact(Skip = "Not valid atm, need another version of container to test against")]
        public void MemoryUsedShouldBeSmaller()
        {
            var aObjects = _fixture.CreateMany<A>(100).ToCollection();
            var bObjects = _fixture.CreateMany<B>(100).ToCollection();
            var cObjects = _fixture.CreateMany<C>(100).ToCollection();

            // dry run
            measureMemoryUsed(() => Tuple.Create(createV1(aObjects, 5), createV1(bObjects, 5), createV1(cObjects, 5)));
            measureMemoryUsed(() => Tuple.Create(createV2(aObjects, 5), createV2(bObjects, 5), createV2(cObjects, 5)));

            var memV1 = measureMemoryUsed(() => Tuple.Create(createV1(aObjects, 5), createV1(bObjects, 5), createV1(cObjects, 5)));
            var memV2 = measureMemoryUsed(() => Tuple.Create(createV2(aObjects, 5), createV2(bObjects, 5), createV2(cObjects, 5)));

            _output.WriteLine($"memV1: {memV1}");
            _output.WriteLine($"memV2: {memV2}");

            memV1.Should()
                 .BeGreaterThan(memV2);
        }

        private IReadOnlyCollection<TypedInstanceContainer<string, T>> createV1<T>(IEnumerable<T> objects, int numPerBucket) where T : Keyed
        {
            var defaultStr = _fixture.Create<string>();

            return objects.Batch(numPerBucket)
                          .Select(b =>
                                  {
                                      var typedInstanceContainer = new TypedInstanceContainer<string, T>(x => x.Key, defaultStr);
                                      b.ForEach(typedInstanceContainer.Set);
                                      return typedInstanceContainer;
                                  })
                          .ToCollection();

        }

        private IReadOnlyCollection<TypedInstanceContainer<string, T>> createV2<T>(IEnumerable<T> objects, int numPerBucket) where T : Keyed
        {
            var defaultStr = _fixture.Create<string>();

            return objects.Batch(numPerBucket)
                          .Select(b =>
                          {
                              var typedInstanceContainer = new TypedInstanceContainer<string, T>(x => x.Key, defaultStr);
                              b.ForEach(typedInstanceContainer.Set);
                              return typedInstanceContainer;
                          })
                          .ToCollection();

        }

        private static long measureMemoryUsed<T>(Func<T> creationFunc)
        {
            var initMemory = GC.GetTotalMemory(true);
            var x = creationFunc();
            var finalMemory = GC.GetTotalMemory(true);
            var str = x.ToString(); // keep x in memory
            return finalMemory - initMemory;
        }
    }
}
