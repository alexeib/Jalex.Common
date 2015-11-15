using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace Jalex.Concurrency.Dataflow
{
    public static class DataflowExtensions
    {
        public static IEnumerable<T> GetDataFromBlock<T>(this ISourceBlock<T> source)
        {
            var results = new ConcurrentBag<T>();
            var pusherBlock = new ActionBlock<T>(d => results.Add(d), new ExecutionDataflowBlockOptions { SingleProducerConstrained = true });
            source.LinkTo(pusherBlock, new DataflowLinkOptions { PropagateCompletion = true });
            pusherBlock.Completion.Wait();
            return results;
        }

        public static void PostMany<T>(this ITargetBlock<T> target, IEnumerable<T> messages)
        {
            foreach (var message in messages)
            {
                target.Post(message);
            }
        }

        public static void LinkWithCompletion<T>(this ISourceBlock<T> source, ITargetBlock<T> target)
        {
            source.LinkTo(target, new DataflowLinkOptions { PropagateCompletion = true });
        }
    }
}
