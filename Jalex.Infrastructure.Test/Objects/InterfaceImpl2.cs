using System;

namespace Jalex.Infrastructure.Test.Objects
{
    public class InterfaceImpl2 : IInterface
    {
        public double NumberValue2 { get; set; }
        public string StringValue2 { get; set; }
        public DateTime DateVaTime2 { get; set; }

        #region Implementation of IInterface

        public string Id { get; set; }

        #endregion
    }
}
