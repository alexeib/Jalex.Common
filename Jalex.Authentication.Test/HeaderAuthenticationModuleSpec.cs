using System.Threading;
using Jalex.Authentication.Objects;
using Jalex.Authentication.Services;
using Jalex.Authentication.Web;
using Jalex.Infrastructure.Objects;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace Jalex.Authentication.Test
{
    public abstract class HeaderAuthenticationModuleSpec
    {        
        protected static IDeviceTokenService _tokenService;
        protected static HeaderAuthenticationModule _headerAuthenticationModule;

        protected static readonly string _validToken = "ValidToken";
        protected static readonly string _invalidToken = "InvalidToken";

        protected static readonly DeviceToken _validDeviceToken = new DeviceToken
        {
            UserId = "user",
            DeviceId = "device",
            Id = "id"
        };

        Establish context = () => 
        {
            var mockDeviceTokenSerivce = new Mock<IDeviceTokenService>();
            mockDeviceTokenSerivce.Setup(s => s.GetExistingToken(_validToken)).Returns(new OperationResult<DeviceToken>(true, _validDeviceToken));
            mockDeviceTokenSerivce.Setup(s => s.GetExistingToken(_invalidToken)).Returns(new OperationResult<DeviceToken>(false));

            _tokenService = mockDeviceTokenSerivce.Object;
            _headerAuthenticationModule = new HeaderAuthenticationModule(_tokenService);
        };
    }

    [Subject(typeof(HeaderAuthenticationModule))]
    public class When_Authenticating_With_Valid_Header : HeaderAuthenticationModuleSpec
    {
        Because of = () => _headerAuthenticationModule.AuthenticateFromHeader(_validToken);

        It should_set_valid_thread_principal = () => Thread.CurrentPrincipal.ShouldBeOfType<JalexPrincipal>();
        It should_set_valid_identity_on_principal = () => Thread.CurrentPrincipal.Identity.ShouldBeOfType<JalexIdentity>();
        It should_be_authenticated = () => Thread.CurrentPrincipal.Identity.IsAuthenticated.ShouldBeTrue();
        It should_have_valid_user_id = () => Thread.CurrentPrincipal.Identity.Name.ShouldEqual(_validDeviceToken.UserId);
        It should_have_device_token_in_identity = () => ((JalexIdentity)Thread.CurrentPrincipal.Identity).Token.ShouldBeLike(_validDeviceToken);
    }
}
