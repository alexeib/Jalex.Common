using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jalex.Authentication.Objects;
using Jalex.Authentication.Services;
using Jalex.Infrastructure.Objects;
using Jalex.Repository;
using Machine.Specifications;
using Moq;

namespace Jalex.Authentication.Test
{
    public abstract class DefaultDeviceTokenServiceSpec
    {
        protected static IDeviceTokenService _deviceTokenService;
        protected static DeviceToken _sampleValidToken;
        protected static DeviceToken _sampleExpiredToken;

        Establish context = () =>
        {
            var mockRepository = new Mock<IRepository<DeviceToken>>();
            mockRepository
                .Setup(r => r.GetByIds(Moq.It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids =>
                    {
                        HashSet<string> hashedIds = new HashSet<string>(ids);

                        List<DeviceToken> retList = new List<DeviceToken>();
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
                .Setup(r => r.Query(Moq.It.IsAny<Func<DeviceToken, bool>>()))
                .Returns<Func<DeviceToken, bool>>(q =>
                    {
                        List<DeviceToken> retList = new List<DeviceToken>();
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

            _deviceTokenService = new DefaultDeviceTokenService(mockRepository.Object);

            _sampleValidToken = new DeviceToken
            {
                Created = DateTime.Now,
                Id = "test1",
                UserId = "user1",
                DeviceId = "device1"
            };

            _sampleExpiredToken = new DeviceToken
            {
                Created = DateTime.Now.Subtract(_deviceTokenService.TokenTimeout),
                Id = "test2",
                UserId = "user2",
                DeviceId = "device2"
            };
        };
    }

    public class When_retrieving_a_token_by_id : DefaultDeviceTokenServiceSpec
    {
        protected static OperationResult<DeviceToken> GetExpiredTokenOperationResult;
        protected static OperationResult<DeviceToken> GetValidTokenOperationResult;

        private Because of = () =>
        {
            GetValidTokenOperationResult = _deviceTokenService.GetExistingToken(_sampleValidToken.Id);
            GetExpiredTokenOperationResult = _deviceTokenService.GetExistingToken(_sampleExpiredToken.Id);
        };

        Behaves_like<IDeviceTokenService_ExpireTokenBehavior> device_token_service_that_expires_tokens_properly;
    }

    public class When_retrieving_a_token_by_user_id_and_device_id : DefaultDeviceTokenServiceSpec
    {
        protected static OperationResult<DeviceToken> GetExpiredTokenOperationResult;
        protected static OperationResult<DeviceToken> GetValidTokenOperationResult;

        private Because of = () =>
        {
            GetValidTokenOperationResult = _deviceTokenService.GetExistingTokenForUserAndDevice(_sampleValidToken.UserId, _sampleValidToken.DeviceId);
            GetExpiredTokenOperationResult = _deviceTokenService.GetExistingTokenForUserAndDevice(_sampleExpiredToken.UserId, _sampleExpiredToken.DeviceId);
        };

        Behaves_like<IDeviceTokenService_ExpireTokenBehavior> device_token_service_that_expires_tokens_properly;
    }
}
