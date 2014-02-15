using System;
using System.Threading;

namespace Jalex.Infrastructure.Objects
{
    public class AsyncResultWrapper<TResult> : IAsyncResult
    {
        private readonly AsyncCallback _callback;
        private readonly object _state;
        private ManualResetEvent _event;
        private bool _completed;
        private TResult _result;
        private readonly object _lock = new object();
        private Exception _ex;

        public AsyncResultWrapper(AsyncCallback callback, object state)
        {
            _callback = callback;
            _state = state;
        }

        public Object AsyncState
        {
            get { return _state; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public bool IsCompleted
        {
            get { return _completed; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                lock (_lock)
                {
                    if (_event == null)
                        _event = new ManualResetEvent(IsCompleted);
                    return _event;
                }
            }
        }

        public void CompleteCall(TResult result, Exception exception)
        {
            // Passing null for exception means no error occurred
            _ex = exception;

            // Cache result
            _result = result;

            lock (_lock)
            {
                _completed = true;
                if (_event != null)
                    _event.Set();
            }

            if (_callback != null)
            {
                // Invoke callback...
                _callback(this);
            }
        }

        public TResult EndInvoke()
        {
            // Operation is done...if an exception occured, throw it
            if (_ex != null) throw _ex;

            return _result;
        }
    }
}
