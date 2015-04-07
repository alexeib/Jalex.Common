using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
            AuthenticationToken token;

            var mockRepository = Substitute.For<IQueryableRepository<AuthenticationToken>>();
            mockRepository
                .TryGetById(Arg.Any<Guid>(), out token)
                .Returns(ci =>
                         {
                             var id = ci.Arg<Guid>();


                             if (_sampleValidToken != null && id == _sampleValidToken.Id)
                             {
                                 ci[1] = _sampleValidToken;
                                 return true;
                             }

                             if (_sampleExpiredToken != null && id == _sampleExpiredToken.Id)
                             {
                                 ci[1] = _sampleExpiredToken;
                                 return true;
                             }

                             return false;
                         });

            mockRepository
                .Query(Arg.Any<Expression<Func<AuthenticationToken, bool>>>())
                .Returns(ci =>
                         {
                             var qExpr = ci.Arg<Expression<Func<AuthenticationToken, bool>>>();
                             var q = qExpr.Compile();

                             List<AuthenticationToken> retList = new List<AuthenticationToken>();
                             if (q(_sampleValidToken))
                             {
                                 retList.Add((_sampleValidToken));
                             }

                             if (q(_sampleExpiredToken))
                             {
                                 retList.Add((_sampleExpiredToken));
                             }

                             return retList;
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
            GetValidTokenOperationResult = _authenticationTokenService.GetExistingToken(_sampleValidToken.Id);
            GetExpiredTokenOperationResult = _authenticationTokenService.GetExistingToken(_sampleExpiredToken.Id);
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
            GetValidTokenOperationResult = _authenticationTokenService.GetExistingTokenForUserAndDevice(_sampleValidToken.UserId, _sampleValidToken.DeviceId);
            GetExpiredTokenOperationResult = _authenticationTokenService.GetExistingTokenForUserAndDevice(_sampleExpiredToken.UserId, _sampleExpiredToken.DeviceId);
        };

#pragma warning disable 169
        Behaves_like<IAuthenticationTokenService_ExpireTokenBehavior> device_token_service_that_expires_tokens_properly;
#pragma warning restore 169
    }
}
