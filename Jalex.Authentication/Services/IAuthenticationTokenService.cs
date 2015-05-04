using System;
using System.Threading.Tasks;
using Jalex.Authentication.Objects;
using Jalex.Infrastructure.Objects;

namespace Jalex.Authentication.Services
{
    /// <summary>
    /// This service provides a facade for dealing with authentication tokens
    /// </summary>
    public interface IAuthenticationTokenService
    {
        /// <summary>
        /// Represents the timespan during which token is valid after creation
        /// </summary>
        TimeSpan TokenTimeout { get; }

        /// <summary>
        /// Retrieves an existing device token, if any
        /// </summary>
        /// <param name="token">the token for which to retrieve the device token</param>
        /// <returns>the full token stored for the given token string, if any</returns>
        Task<OperationResult<AuthenticationToken>> GetExistingTokenAsync(Guid token);

        /// <summary>
        /// Retrieves an existing device token, if any
        /// </summary>
        /// <param name="userId">the username for which to retrieve a token</param>
        /// <param name="deviceId">the device id for which to retrieve a token</param>
        /// <returns>the token stored for that combination of username + device id, or null if none exist</returns>
        Task<OperationResult<AuthenticationToken>> GetExistingTokenForUserAndDeviceAsync(string userId, string deviceId);

        /// <summary>
        /// Creates a new device token for a given username and device id. Will return an existing token if it exists (will not create a new one)
        /// </summary>
        /// <param name="userId">the username for which to create a token</param>
        /// <param name="deviceId">the device for which to create a token</param>
        /// <returns>the newly created token</returns>
        Task<OperationResult<AuthenticationToken>> CreateTokenAsync(string userId, string deviceId);

        /// <summary>
        /// Deletes a token with the given id
        /// </summary>
        /// <param name="tokenId">The id of the token to delete</param>
        /// <returns>result of the operation</returns>
        Task<OperationResult> DeleteTokenAsync(Guid tokenId);
    }
}
