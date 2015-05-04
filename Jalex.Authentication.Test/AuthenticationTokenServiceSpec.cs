using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Jalex.Authentication.Objects;
using Jalex.Authentication.Services;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Repository;
using Machine.Specifications;
using NSubstitute;

namespace Jalex.Authentication.Test
{
    public abstract class AuthenticationTokenServiceSpec
    {
        protected static IAuthenticationTokenService _authenticationTokenService;
        protected static AuthenticationToken _sampleValidToken;
        protected static AuthenticationToken _sampleExpiredToken;

        Establish context = () =>
        {
            var mockRepository = Substitute.For<IQueryableRepository<AuthenticationToken>>();
            mockRepository
                .GetByIdAsync(Arg.Any<Guid>())
                .Returns(ci =>
                         {
                             var id = ci.Arg<Guid>();


                             if (_sampleValidToken != null && id == _sampleValidToken.Id)
                             {
                                 return Task.FromResult(_sampleValidToken);
                             }

                             if (_sampleExpiredToken != null && id == _sampleExpiredToken.Id)
                             {
                                 return Task.FromResult(_sampleExpiredToken);
                             }

                             return Task.FromResult<AuthenticationToken>(null);
                         });

            mockRepository
                .FirstOrDefaultAsync(Arg.Any<Expression<Func<AuthenticationToken, bool>>>())
                .Returns(ci =>
                         {
                             var qExpr = ci.Arg<Expression<Func<AuthenticationToken, bool>>>();
                             var q = qExpr.Compile();

                             if (q(_sampleValidToken))
                             {
                                 return Task.FromResult(_sampleValidToken);
                             }

                             if (q(_sampleExpiredToken))
                             {
                                 return Task.FromResult(_sampleExpiredToken);
                             }

                             return Task.FromResult<AuthenticationToken>(null);
                         });

            _authenticationTokenService = new AuthenticationTokenService(mockRepository);

            _sampleValidToken = new AuthenticationToken
            {
                Created = DateTime.Now,
                Id = Guid.NewGuid(),
                UserId = "user1",
                DeviceId = "device1"
            };

            _sampleExpiredToken = new AuthenticationToken
            {
                Created = DateTime.Now.Subtract(_authenticationTokenService.TokenTimeout),
                Id = Guid.NewGuid(),
                UserId = "user2",
                DeviceId = "device2"
            };
        };
    }

    [Subject(typeof(IAuthenticationTokenService))]
    public class When_retrieving_a_token_by_id : AuthenticationTokenServiceSpec
    {
        protected static OperationResult<AuthenticationToken> GetExpiredTokenOperationResult;
        protected static OperationResult<AuthenticationToken> GetValidTokenOperationResult;

        private Because of = () =>
        {
            GetValidTokenOperationResult = _authenticationTokenService.GetExistingTokenAsync(_sampleValidToken.Id).Result;
            GetExpiredTokenOperationResult = _authenticationTokenService.GetExistingTokenAsync(_sampleExpiredToken.Id).Result;
        };

#pragma warning disable 169
        Behaves_like<IAuthenticationTokenService_ExpireTokenBehavior> device_token_service_that_expires_tokens_properly;
#pragma warning restore 169
    }

    [Subject(typeof(IAuthenticationTokenService))]
    public class When_Retrieving_A_Token_By_UserId_And_DeviceId : AuthenticationTokenServiceSpec
    {
        protected static OperationResult<AuthenticationToken> GetExpiredTokenOperationResult;
        protected static OperationResult<AuthenticationToken> GetValidTokenOperationResult;

        private Because of = () =>
        {
            GetValidTokenOperationResult = _authenticationTokenService.GetExistingTokenForUserAndDeviceAsync(_sampleValidToken.UserId, _sampleValidToken.DeviceId).Result;
            GetExpiredTokenOperationResult = _authenticationTokenService.GetExistingTokenForUserAndDeviceAsync(_sampleExpiredToken.UserId, _sampleExpiredToken.DeviceId).Result;
        };

#pragma warning disable 169
        Behaves_like<IAuthenticationTokenService_ExpireTokenBehavior> device_token_service_that_expires_tokens_properly;
#pragma warning restore 169
    }
}
