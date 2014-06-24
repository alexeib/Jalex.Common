using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace Jalex.TestUtils.Dataflow
{
    public static class DataflowUtils
    {
        public static IEnumerable<T> GetDataFromBlock<T>(ISourceBlock<T> source)
        {
            ConcurrentBag<T> results = new ConcurrentBag<T>();
            var pusherBlock = new ActionBlock<T>(d => results.Add(d), new ExecutionDataflowBlockOptions { SingleProducerConstrained = true });
            source.LinkTo(pusherBlock, new DataflowLinkOptions { PropagateCompletion = true });
            pusherBlock.Completion.Wait();
            return results;
        }
    }
}
