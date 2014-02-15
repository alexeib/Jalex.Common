using System.Diagnostics;
using System.Security.Principal;

namespace Jalex.Authentication.Objects
{
    [DebuggerDisplay("Identity: {Identity.Token}")]
    public class JalexPrincipal : GenericPrincipal
    {
        public JalexPrincipal(JalexIdentity identity, params string[] roles)
            : base(identity, roles)
        {
        }
    }
}
