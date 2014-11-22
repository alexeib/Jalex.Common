using System.Diagnostics;
using System.Security.Principal;

namespace Jalex.Authentication.Objects
{
    [DebuggerDisplay("{Token}")]
    public class JalexIdentity : GenericIdentity
    {
        public static JalexIdentity EmptyIdentity
        {
            get
            {
                return new JalexIdentity(null);
            }
        }

        public AuthenticationToken Token { get; private set; }

        public JalexIdentity(AuthenticationToken token)
            : base(token != null ? token.UserId : string.Empty, "Jalex")
        {
            Token = token;
        }        
    }
}
