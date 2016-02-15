using System;
using System.Threading;
using Cassandra;

namespace Jalex.Repository.Cassandra
{
    internal class CassandraRetryPolicy : IRetryPolicy
    {
        private readonly int _numRetries;
        private readonly TimeSpan _delay;

        public CassandraRetryPolicy(int numRetries, TimeSpan delay)
        {
            _numRetries = numRetries;
            _delay = delay;
        }

        public RetryDecision OnReadTimeout(IStatement query, ConsistencyLevel cl, int requiredResponses, int receivedResponses, bool dataRetrieved, int nbRetry)
        {
            return retryIfPossible(cl, nbRetry);
        }

        public RetryDecision OnWriteTimeout(IStatement query, ConsistencyLevel cl, string writeType, int requiredAcks, int receivedAcks, int nbRetry)
        {
            return retryIfPossible(cl, nbRetry);
        }

        public RetryDecision OnUnavailable(IStatement query, ConsistencyLevel cl, int requiredReplica, int aliveReplica, int nbRetry)
        {
            return retryIfPossible(cl, nbRetry);
        }

        private RetryDecision retryIfPossible(ConsistencyLevel cl, int nbRetry)
        {
            if (nbRetry < _numRetries)
            {
                if (_delay != TimeSpan.Zero)
                {
                    Thread.Sleep(_delay);
                }
                return RetryDecision.Retry(cl);
            }
            return RetryDecision.Rethrow();
        }
    }
}
