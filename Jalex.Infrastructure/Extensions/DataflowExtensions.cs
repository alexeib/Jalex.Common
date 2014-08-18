using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace Jalex.Infrastructure.Extensions
{
    public static class DataflowExtensions
    {
        public static IEnumerable<T> GetDataFromBlock<T>(this ISourceBlock<T> source)
        {
            ConcurrentBag<T> results = new ConcurrentBag<T>();
            var pusherBlock = new ActionBlock<T>(d => results.Add(d), new ExecutionDataflowBlockOptions { SingleProducerConstrained = true });
            source.LinkTo(pusherBlock, new DataflowLinkOptions { PropagateCompletion = true });
            pusherBlock.Completion.Wait();
            return results;
        }
    }
}
