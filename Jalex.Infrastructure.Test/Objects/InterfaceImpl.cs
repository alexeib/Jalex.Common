using System;

namespace Jalex.Infrastructure.Test.Objects
{
    public class InterfaceImpl : IInterface
    {
        public double NumberValue { get; set; }
        public string StringValue { get; set; }
        public DateTime DateVaTime { get; set; }
    }
}
