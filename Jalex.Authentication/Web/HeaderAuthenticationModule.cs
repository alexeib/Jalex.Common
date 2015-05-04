using System;
using System.Web;
using Jalex.Authentication.Objects;
using Jalex.Authentication.Services;

namespace Jalex.Authentication.Web
{
    public class HeaderAuthenticationModule : IHttpModule
    {
        private readonly IAuthenticationTokenService _tokenService;
        private readonly IAuthenticationService _authenticationService;

        public HeaderAuthenticationModule(IAuthenticationService authenticationService, IAuthenticationTokenService tokenService)
        {
            _authenticationService = authenticationService;
            _tokenService = tokenService;
        }

        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += authenticateFromHeaderHandler;
        }

        private void authenticateFromHeaderHandler(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            string tokenAuthHeaderValue = application.Request.Headers[AuthenticationConstants.AuthTokenHeader];
            Guid tokenAsGuid;

            if (Guid.TryParse(tokenAuthHeaderValue, out tokenAsGuid))
            {
                var tokenResult = _tokenService.GetExistingTokenAsync(tokenAsGuid).Result;

                if (tokenResult.Success)
                {
                    _authenticationService.SetTokenForCurrentUser(tokenResult.Value);
                }
            }
        }

        public void Dispose()
        {
            // nothing to do
        }
    }
}
