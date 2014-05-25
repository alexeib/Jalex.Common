using System.Threading;
using System.Web;
using Jalex.Authentication.Objects;

namespace Jalex.Authentication.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        public AuthenticationToken GetTokenForCurrentUser()
        {
            JalexPrincipal principal;

            if (HttpContext.Current != null)
            {
                principal = HttpContext.Current.User as JalexPrincipal;
            }
            else
            {
                principal = Thread.CurrentPrincipal as JalexPrincipal;
            }

            if (principal != null)
            {
                JalexIdentity identity = principal.Identity as JalexIdentity;
                if (identity != null)
                {
                    return identity.Token;
                }
            }

            return null;
        }

        public void SetTokenForCurrentUser(AuthenticationToken token)
        {
            JalexIdentity identity = new JalexIdentity(token);
            JalexPrincipal principal = new JalexPrincipal(identity);

            Thread.CurrentPrincipal = principal;

            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }
    }
}
