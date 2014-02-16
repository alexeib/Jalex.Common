using Jalex.Authentication.Objects;

namespace Jalex.Authentication.Services
{
    /// <summary>
    /// This service is responsible for providing authentication services
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Retrieves the authentication token for the current user as determined by HttpContext.Current.User or Thrad.CurrentPrincipal. Returns null if no such token exists.
        /// </summary>
        /// <returns>The retrieved authentication token or null if no such token exists</returns>
        AuthenticationToken GetTokenForCurrentUser();

        /// <summary>
        /// Sets the authentication token for the current user (both HttpContext.Current.User and Thread.CurrentPrincipal)
        /// </summary>
        /// <param name="token">The token to set</param>
        void SetTokenForCurrentUser(AuthenticationToken token);
    }
}
