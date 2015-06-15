using Jalex.Infrastructure.Messaging;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Repository.Messages;
using Jalex.Repository.IdProviders;
using Jalex.Repository.Memory;
using Jalex.Repository.Test;
using Jalex.Services.Repository;
using NSubstitute;
using Ploeh.AutoFixture;

namespace Jalex.Services.Test.Repository
{
    public class NotifyingResponsibilityTests : ISimpleRepositoryTests<TestObject>
    {
        private readonly IMessagePipe<EntityCreated<TestObject>> _entityCreatedPipe;
        private readonly IMessagePipe<EntityUpdated<TestObject>> _entityUpdatedPipe;
        private readonly IMessagePipe<EntityDeleted<TestObject>> _entityDeletedPipe;

        public NotifyingResponsibilityTests()
            : base(createFixture())
        {
            _entityCreatedPipe = _fixture.Create<IMessagePipe<EntityCreated<TestObject>>>();
            _entityUpdatedPipe = _fixture.Create<IMessagePipe<EntityUpdated<TestObject>>>();
            _entityDeletedPipe = _fixture.Create<IMessagePipe<EntityDeleted<TestObject>>>();
        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();

            fixture.Inject(Substitute.For<IMessagePipe<EntityCreated<TestObject>>>());
            fixture.Inject(Substitute.For<IMessagePipe<EntityUpdated<TestObject>>>());
            fixture.Inject(Substitute.For<IMessagePipe<EntityDeleted<TestObject>>>());

            fixture.Register<IIdProvider>(fixture.Create<GuidIdProvider>);
            fixture.Register<IReflectedTypeDescriptorProvider>(fixture.Create<ReflectedTypeDescriptorProvider>);
            fixture.Register<IQueryableRepository<TestObject>>(() =>
            {
                var entityRepository = fixture.Create<MemoryRepository<TestObject>>();

                var entityCreatedPipe = fixture.Create<IMessagePipe<EntityCreated<TestObject>>>();
                var entityUpdatedPipe = fixture.Create<IMessagePipe<EntityUpdated<TestObject>>>();
                var entityDeletedPipe = fixture.Create<IMessagePipe<EntityDeleted<TestObject>>>();

                return new NotifyingResponsibility<TestObject>(
                    entityRepository,                    
                    fixture.Create<IReflectedTypeDescriptorProvider>(),
                    entityCreatedPipe,
                    entityUpdatedPipe,
                    entityDeletedPipe);
            });
            fixture.Register<ISimpleRepository<TestObject>>(fixture.Create<IQueryableRepository<TestObject>>);

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register(() => fixture.Create<IIdProvider>().GenerateNewId());

            return fixture;
        }

        #region Overrides of ISimpleRepositoryTests<TestObject>

        public override void CreatesEntity()
        {
            base.CreatesEntity();
            _entityCreatedPipe.ReceivedWithAnyArgs()
                              .SendAsync(null);
            _entityUpdatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
        }

        public override void CreatesManyEntities()
        {
            base.CreatesManyEntities();
            _entityCreatedPipe.ReceivedWithAnyArgs()
                              .SendAsync(null);
            _entityUpdatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
        }

        public override void CreatesManyEntitiesWithUpsert()
        {
            base.CreatesManyEntitiesWithUpsert();
            _entityCreatedPipe.ReceivedWithAnyArgs()
                              .SendAsync(null);
            _entityUpdatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);

        }

        public override void CreatesNonExistingEntitiesAndUpdatesExistingWithUpsert()
        {
            base.CreatesNonExistingEntitiesAndUpdatesExistingWithUpsert();
            _entityCreatedPipe.ReceivedWithAnyArgs()
                              .SendAsync(null);
            _entityUpdatedPipe.ReceivedWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
        }

        public override void DeletesExistingTestEntities()
        {
            base.DeletesExistingTestEntities();
            _entityCreatedPipe.ReceivedWithAnyArgs(1)
                              .SendAsync(null);
            _entityUpdatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.ReceivedWithAnyArgs()
                              .SendAsync(null);
        }

        public override void DoesNotCreateEntitiesWithDuplicateIds()
        {
            base.DoesNotCreateEntitiesWithDuplicateIds();
            _entityCreatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityUpdatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
        }

        public override void DoesNotCreateExistingEntities()
        {
            base.DoesNotCreateExistingEntities();
            _entityCreatedPipe.ReceivedWithAnyArgs(1)
                              .SendAsync(null);
            _entityUpdatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
        }

        public override void FailsToDeleteNonExistingEntities()
        {
            base.FailsToDeleteNonExistingEntities();
            _entityCreatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityUpdatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
        }

        public override void FailsToUpdateEntityWithNullId()
        {
            base.FailsToUpdateEntityWithNullId();
            _entityCreatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityUpdatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
        }

        public override void FailsToUpdateNonExistingEntity()
        {
            base.FailsToUpdateNonExistingEntity();
            _entityCreatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityUpdatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
        }

        public override void InsertsNewEntityWithUpsert()
        {
            base.InsertsNewEntityWithUpsert();
            _entityCreatedPipe.ReceivedWithAnyArgs()
                              .SendAsync(null);
            _entityUpdatedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
        }

        public override void UpdatesExistingEntity()
        {
            base.UpdatesExistingEntity();
            _entityCreatedPipe.ReceivedWithAnyArgs(3)
                              .SendAsync(null);
            _entityUpdatedPipe.ReceivedWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
        }

        public override void UpdatesExistingEntityWithUpsert()
        {
            base.UpdatesExistingEntityWithUpsert();
            _entityCreatedPipe.ReceivedWithAnyArgs(3)
                              .SendAsync(null);
            _entityUpdatedPipe.ReceivedWithAnyArgs()
                              .SendAsync(null);
            _entityDeletedPipe.DidNotReceiveWithAnyArgs()
                              .SendAsync(null);
        }

        #endregion

        #region Overrides of IQueryableRepositoryTests<TestObject>

        //public override void DeletesEntitiesUsingQuery()
        //{
        //    base.DeletesEntitiesUsingQuery();
        //    _entityUpdatedPipe.DidNotReceiveWithAnyArgs()
        //                      .SendAsync(null);
        //    _entityDeletedPipe.ReceivedWithAnyArgs()
        //                      .SendAsync(null);
        //}

        #endregion
    }
}
