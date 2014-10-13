using System;
using System.Configuration;
using System.Linq;
using Jalex.Authentication.Objects;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Repository;

namespace Jalex.Authentication.Services
{
    public class AuthenticationTokenService : IAuthenticationTokenService
    {
        private const string _tokenTimeoutMinutesSetting = "AuthenticationTokenTimeoutMinutes";
        private static readonly TimeSpan _defaultTokenTimeout = TimeSpan.FromHours(24);

        public TimeSpan TokenTimeout { get; private set; }

        private readonly IQueryableRepository<AuthenticationToken> _repository;
        public AuthenticationTokenService(IQueryableRepository<AuthenticationToken> repository)
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
            var deviceToken = _repository.GetByIdOrDefault(token);
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

            var result = _repository.Save(deviceToken, WriteMode.Insert);
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
