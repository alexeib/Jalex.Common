using System;
using System.Configuration;
using System.Linq;
using Jalex.Authentication.Objects;
using Jalex.Infrastructure.Objects;
using Jalex.Repository;
using Jalex.Repository.Extensions;

namespace Jalex.Authentication.Services
{
    public class DefaultDeviceTokenService : IDeviceTokenService
    {
        private const string _tokenTimeoutMinutesSetting = "AuthenticationTokenTimeoutMinutes";
        private static readonly TimeSpan _defaultTokenTimeout = TimeSpan.FromMinutes(30);

        public TimeSpan TokenTimeout { get; private set; }

        protected readonly IRepository<DeviceToken> _repository;
        public DefaultDeviceTokenService(IRepository<DeviceToken> repository)
        {
            _repository = repository;

            var configuredTimeout = ConfigurationManager.AppSettings[_tokenTimeoutMinutesSetting];
            int minutes;

            if (!string.IsNullOrEmpty(configuredTimeout) && int.TryParse(configuredTimeout, out minutes))
            {
                TokenTimeout = TimeSpan.FromMinutes(minutes);
            }
            else
            {
                TokenTimeout = _defaultTokenTimeout;
            }
        }

        public OperationResult<DeviceToken> GetExistingToken(string token)
        {
            var deviceToken = _repository.GetById(token);
            return createTokenOperationResult(deviceToken);
        }

        public OperationResult<DeviceToken> GetExistingTokenForUserAndDevice(string userId, string deviceId)
        {
            var token = _repository.Query(t => t.UserId == userId && t.DeviceId == deviceId).FirstOrDefault();
            return createTokenOperationResult(token);
        }

        public OperationResult<DeviceToken> CreateToken(string userId, string deviceId)
        {
            var tokenResult = GetExistingTokenForUserAndDevice(userId, deviceId);
            if (tokenResult.Success)
            {
                return tokenResult;
            }

            var date = DateTime.UtcNow;
            var deviceToken = new DeviceToken
            {
                UserId = userId,
                DeviceId = deviceId,
                Created = date
            };

            var result = _repository.Create(deviceToken);
            deviceToken.Id = result.Value;

            return new OperationResult<DeviceToken>(result.Success, deviceToken, result.Messages.ToArray());
        }

        public OperationResult DeleteToken(string tokenId)
        {
            return _repository.Delete(tokenId);
        }

        private OperationResult<DeviceToken> createTokenOperationResult(DeviceToken token)
        {
            if (checkIfTokenIsValid(token))
            {
                return new OperationResult<DeviceToken>(true, token);
            }

            return new OperationResult<DeviceToken>(false);
        }

        private bool checkIfTokenIsValid(DeviceToken token)
        {
            if (token == null)
            {
                return false;
            }

            return TokenTimeout == TimeSpan.Zero || (DateTime.UtcNow - token.Created.ToUniversalTime()) < TokenTimeout;
        }
    }
}
