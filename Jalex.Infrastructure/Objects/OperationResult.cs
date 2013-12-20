using System.Collections.Generic;

namespace Jalex.Infrastructure.Objects
{
    public class OperationResult
    {
        public bool Success { get;set;}
        public IEnumerable<Message> Messages { get; set; }

        public OperationResult()
        {
            Messages = new Message[0];
        }

        public OperationResult(bool success) : this()
        {
            Success = success;
        }

        public OperationResult(bool success, IEnumerable<Message> messages) : this(success)
        {
            Messages = messages;
        }
    }
}
