﻿using System.Collections.Generic;
using System.Linq;

namespace Jalex.Infrastructure.Objects
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public IEnumerable<Message> Messages { get; set; }

        public OperationResult()
        {
            Messages = new Message[0];
        }

        public OperationResult(bool success)
            : this()
        {
            Success = success;
        }

        public OperationResult(bool success, params Message[] messages)
            : this(success)
        {
            Messages = messages;
        }

        public OperationResult(bool success, Severity messageSeverity, params string[] messages)
            : this(success)
        {
            Messages = messages.Select(m => new Message { Severity = messageSeverity, Content = m });
        }
    }
}
