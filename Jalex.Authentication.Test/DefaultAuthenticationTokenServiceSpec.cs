using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Jalex.Authentication.Objects;
using Jalex.Authentication.Services;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Repository;
using Jalex.Repository;
using Machine.Specifications;
using Moq;

namespace Jalex.Authentication.Test
{
    public abstract class DefaultAuthenticationTokenServiceSpec
    {
        protected static IAuthenticationTokenService _authenticationTokenService;
        protected static AuthenticationToken _sampleValidToken;
        protected static AuthenticationToken _sampleExpiredToken;

        Establish context = () =>
        {
            var mockRepository = new Mock<IQueryableRepository<AuthenticationToken>>();
            mockRepository
                .Setup(r => r.GetByIds(Moq.It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids =>
                    {
                        HashSet<string> hashedIds = new HashSet<string>(ids);

                        List<AuthenticationToken> retList = new List<AuthenticationToken>();
                        if (_sampleValidToken != null && hashedIds.Contains(_sampleValidToken.Id))
                        {
                            retList.Add(_sampleValidToken);
                        }

                        if (_sampleExpiredToken != null && hashedIds.Contains(_sampleExpiredToken.Id))
                        {
                            retList.Add(_sampleExpiredToken);
                        }

                        return retList;
                    });

            mockRepository
                .Setup(r => r.Query(Moq.It.IsAny<Expression<Func<AuthenticationToken, bool>>>()))
                .Returns<Func<AuthenticationToken, bool>>(q =>
                    {
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

            _authenticationTokenService = new DefaultAuthenticationTokenService(mockRepository.Object);

            _sampleValidToken = new AuthenticationToken
            {
                Created = DateTime.Now,
                Id = "test1",
                UserId = "user1",
                DeviceId = "device1"
            };

            _sampleExpiredToken = new AuthenticationToken
            {
                Created = DateTime.Now.Subtract(_authenticationTokenService.TokenTimeout),
                Id = "test2",
                UserId = "user2",
                DeviceId = "device2"
            };
        };
    }

    [Subject(typeof(IAuthenticationTokenService))]
    public class When_retrieving_a_token_by_id : DefaultAuthenticationTokenServiceSpec
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
    public class When_Retrieving_A_Token_By_UserId_And_DeviceId : DefaultAuthenticationTokenServiceSpec
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
