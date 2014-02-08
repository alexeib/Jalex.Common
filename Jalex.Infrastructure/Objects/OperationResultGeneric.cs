
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

        public OperationResult(bool success, params Message[] messages) : base(success, messages)
        {
            
        }

        public OperationResult(bool success, T value, params Message[] messages)
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
