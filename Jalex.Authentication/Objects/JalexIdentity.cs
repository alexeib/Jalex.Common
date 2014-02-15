using System.Diagnostics;
using System.Security.Principal;

namespace Jalex.Authentication.Objects
{
    [DebuggerDisplay("{Token}")]
    public class JalexIdentity : GenericIdentity
    {
        public JalexIdentity EmptyIdentity
        {
            get
            {
                return new JalexIdentity(null);
            }
        }

        public DeviceToken Token { get; private set; }

        public JalexIdentity(DeviceToken token)
            : base(token != null ? token.UserId : string.Empty, "Jalex")
        {
            Token = token;
        }        
    }
}
