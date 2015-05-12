using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace Jalex.Infrastructure.Messaging
{
    public class MessagePipe<T> : IMessagePipe<T>
    {
        private readonly ILifetimeScope _lifetimeScope;

        public MessagePipe(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        #region Implementation of IMessagePipe<in T>

        public async Task SendAsync(T message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var consumers = scope.Resolve<IEnumerable<IMessageConsumer<T>>>();
                var consumingTasks = consumers.Select(c => c.ConsumeAsync(message));
                await Task.WhenAll(consumingTasks)
                          .ConfigureAwait(false);
            }
        }

        #endregion
    }
}
