using System;
using System.Threading;
using System.Web;
using Jalex.Authentication.Objects;
using Jalex.Authentication.Services;

namespace Jalex.Authentication.Web
{
    public class HeaderAuthenticationModule : IHttpModule
    {
        private readonly IDeviceTokenService _tokenService;

        public HeaderAuthenticationModule(IDeviceTokenService tokenService)
        {
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

            AuthenticateFromHeader(tokenAuthHeaderValue);
        }

        public void AuthenticateFromHeader(string tokenAuthHeaderValue)
        {
            if (!string.IsNullOrEmpty(tokenAuthHeaderValue))
            {
                var tokenResult = _tokenService.GetExistingToken(tokenAuthHeaderValue);

                if (tokenResult.Success)
                {
                    JalexIdentity identity = new JalexIdentity(tokenResult.Value);
                    JalexPrincipal principal = new JalexPrincipal(identity);

                    Thread.CurrentPrincipal = principal;

                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.User = principal;
                    }
                }
            }
        }

        public void Dispose()
        {
            // nothing to do
        }
    }
}
