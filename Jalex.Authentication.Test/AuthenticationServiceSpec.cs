using System;
using System.Threading;
using Jalex.Authentication.Objects;
using Jalex.Authentication.Services;
using Machine.Specifications;

namespace Jalex.Authentication.Test
{
    public abstract class AuthenticationServiceSpec
    {
        protected static IAuthenticationService _authenticationService;

        Establish context = () => _authenticationService = new AuthenticationService();
    }

    [Subject(typeof(IAuthenticationService))]
    public class When_Getting_Token_Of_Currently_Logged_In_User : AuthenticationServiceSpec
    {
        protected static AuthenticationToken _setToken;
        protected static AuthenticationToken _retrievedToken;

        Establish context = () =>
        {
            _setToken = new AuthenticationToken { Created = DateTime.UtcNow, DeviceId = "d", UserId = "u", Id = Guid.NewGuid() };
            JalexIdentity identity = new JalexIdentity(_setToken);
            JalexPrincipal principal = new JalexPrincipal(identity);
            Thread.CurrentPrincipal = principal;
        };

        Because of = () => _retrievedToken = _authenticationService.GetTokenForCurrentUser();

        It should_not_retrieve_non_null_token = () => _retrievedToken.ShouldNotBeNull();
        It should_retrieve_the_same_token_as_was_set = () => _retrievedToken.ShouldBeTheSameAs(_setToken);
    }

    [Subject(typeof(IAuthenticationService))]
    public class When_Authenticating_With_Valid_Header : AuthenticationServiceSpec
    {
        protected static AuthenticationToken _token;

        Establish context = () =>
        {
            _token = new AuthenticationToken { Created = DateTime.UtcNow, DeviceId = "d", UserId = "u", Id = Guid.NewGuid() };
            JalexIdentity identity = new JalexIdentity(_token);
            JalexPrincipal principal = new JalexPrincipal(identity);
            Thread.CurrentPrincipal = principal;
        };

        Because of = () => _authenticationService.SetTokenForCurrentUser(_token);

        It should_set_valid_thread_principal = () => Thread.CurrentPrincipal.ShouldBeAssignableTo<JalexPrincipal>();
        It should_set_valid_identity_on_principal = () => Thread.CurrentPrincipal.Identity.ShouldBeAssignableTo<JalexIdentity>();
        It should_be_authenticated = () => Thread.CurrentPrincipal.Identity.IsAuthenticated.ShouldBeTrue();
        It should_have_valid_user_id = () => Thread.CurrentPrincipal.Identity.Name.ShouldEqual(_token.UserId);
        It should_have_device_token_in_identity = () => ((JalexIdentity)Thread.CurrentPrincipal.Identity).Token.ShouldBeLike(_token);
    }
}
