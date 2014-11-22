using Jalex.Infrastructure.Serialization;

namespace Jalex.Infrastructure.Test.Objects
{
    [CustomTypeName("my name")]
    public class CustomTypeNamedInterface : IInterface
    {
        #region Implementation of IInterface

        public string Id { get; set; }

        #endregion
    }
}
