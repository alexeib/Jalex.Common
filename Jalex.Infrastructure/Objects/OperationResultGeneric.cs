using System.Collections.Generic;

namespace Jalex.Infrastructure.Objects
{
    public class OperationResult<T> : OperationResult
    {
        public T Value { get; set; }

        public OperationResult()
        {
            
        }

        public OperationResult(bool success) : base(success)
        {
            
        }

        public OperationResult(bool success, IEnumerable<Message> messages) : base(success, messages)
        {
            
        }

        public OperationResult(bool success, IEnumerable<Message> messages, T value)
            : base(success, messages)
        {
            Value = value;
        }

        public OperationResult(bool success, T value)
            : base(success, new Message[0])
        {
            Value = value;
        }
    }
}
