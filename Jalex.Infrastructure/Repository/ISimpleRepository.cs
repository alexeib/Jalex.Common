﻿using Jalex.Infrastructure.Logging;

namespace Jalex.Infrastructure.Repository
{
    public interface ISimpleRepository<T> : IReader<T>, IDeleter<T>, IUpdater<T>, IInserter<T>, IInjectableLogger
    {
    }
}