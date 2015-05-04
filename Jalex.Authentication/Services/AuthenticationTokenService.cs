using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<OperationResult<AuthenticationToken>> GetExistingTokenAsync(Guid token)
        {
            var deviceToken = await _repository.GetByIdAsync(token).ConfigureAwait(false);
            return createTokenOperationResult(deviceToken);
        }

        public async Task<OperationResult<AuthenticationToken>> GetExistingTokenForUserAndDeviceAsync(string userId, string deviceId)
        {
            var token = await _repository.FirstOrDefaultAsync(t => t.UserId == userId && t.DeviceId == deviceId).ConfigureAwait(false);
            return createTokenOperationResult(token);
        }

        public async Task<OperationResult<AuthenticationToken>> CreateTokenAsync(string userId, string deviceId)
        {
            var tokenResult = await GetExistingTokenForUserAndDeviceAsync(userId, deviceId).ConfigureAwait(false);
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

            var result = await _repository.SaveAsync(deviceToken, WriteMode.Insert).ConfigureAwait(false);
            deviceToken.Id = result.Value;

            return new OperationResult<AuthenticationToken>(result.Success, deviceToken, result.Messages.ToArray());
        }

        public async Task<OperationResult> DeleteTokenAsync(Guid tokenId)
        {
            return await _repository.DeleteAsync(tokenId).ConfigureAwait(false);
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
