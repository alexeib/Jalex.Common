using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;

namespace Jalex.Repository.Test
{
    public abstract class IQueryableRepositorySpec : ISimpleRepositorySpec
    {
    }

    [Behaviors]
    public class Repository_that_can_retrieve_entities_through_querying_by_name
    {
        protected static IEnumerable<TestEntity> _retrievedTestEntitys;
        protected static IEnumerable<TestEntity> _sampleTestEntitys;

        private It should_retrieve_right_number_of_TestEntitys = () => _retrievedTestEntitys.Count().ShouldEqual(_sampleTestEntitys.Count());
        private It should_retrieve_correct_TestEntitys = () => _retrievedTestEntitys.Select(r => r.Name).Intersect(_sampleTestEntitys.Select(r => r.Name)).Count().ShouldEqual(_sampleTestEntitys.Count());
    }
}
