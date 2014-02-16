using System;
using System.Configuration;
using System.Linq;
using Jalex.Authentication.Objects;
using Jalex.Infrastructure.Objects;
using Jalex.Repository;
using Jalex.Repository.Extensions;

namespace Jalex.Authentication.Services
{
    public class DefaultAuthenticationTokenService : IAuthenticationTokenService
    {
        private const string _tokenTimeoutMinutesSetting = "AuthenticationTokenTimeoutMinutes";
        private static readonly TimeSpan _defaultTokenTimeout = TimeSpan.FromMinutes(30);

        public TimeSpan TokenTimeout { get; private set; }

        protected readonly IRepository<AuthenticationToken> _repository;
        public DefaultAuthenticationTokenService(IRepository<AuthenticationToken> repository)
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

        public OperationResult<AuthenticationToken> GetExistingToken(string token)
        {
            var deviceToken = _repository.GetById(token);
            return createTokenOperationResult(deviceToken);
        }

        public OperationResult<AuthenticationToken> GetExistingTokenForUserAndDevice(string userId, string deviceId)
        {
            var token = _repository.Query(t => t.UserId == userId && t.DeviceId == deviceId).FirstOrDefault();
            return createTokenOperationResult(token);
        }

        public OperationResult<AuthenticationToken> CreateToken(string userId, string deviceId)
        {
            var tokenResult = GetExistingTokenForUserAndDevice(userId, deviceId);
            if (tokenResult.Success)
            {
                return tokenResult;
            }

            var date = DateTime.UtcNow;
            var deviceToken = new AuthenticationToken
            {
                UserId = userId,
                DeviceId = deviceId,
                Created = date
            };

            var result = _repository.Create(deviceToken);
            deviceToken.Id = result.Value;

            return new OperationResult<AuthenticationToken>(result.Success, deviceToken, result.Messages.ToArray());
        }

        public OperationResult DeleteToken(string tokenId)
        {
            return _repository.Delete(tokenId);
        }

        private OperationResult<AuthenticationToken> createTokenOperationResult(AuthenticationToken token)
        {
            if (checkIfTokenIsValid(token))
            {
                return new OperationResult<AuthenticationToken>(true, token);
            }

            return new OperationResult<AuthenticationToken>(false);
        }

        private bool checkIfTokenIsValid(AuthenticationToken token)
        {
            if (token == null)
            {
                return false;
            }

            return TokenTimeout == TimeSpan.Zero || (DateTime.UtcNow - token.Created.ToUniversalTime()) < TokenTimeout;
        }
    }
}
