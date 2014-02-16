using Jalex.Authentication.Objects;
using Jalex.Infrastructure.Objects;
using Machine.Specifications;

namespace Jalex.Authentication.Test
{
    [Behaviors]
    public class IAuthenticationTokenService_ExpireTokenBehavior
    {
        protected static OperationResult<AuthenticationToken> GetExpiredTokenOperationResult;
        protected static OperationResult<AuthenticationToken> GetValidTokenOperationResult;

        It should_fail_to_retrieve_expired_token = () => GetExpiredTokenOperationResult.Success.ShouldBeFalse();
        It should_not_have_expired_token_as_part_of_response = () => GetExpiredTokenOperationResult.Value.ShouldBeNull();
        It should_succeed_in_retrieval_of_valid_token = () => GetValidTokenOperationResult.Success.ShouldBeTrue();
        It should_have_valid_token_as_part_of_response = () => GetValidTokenOperationResult.Value.ShouldNotBeNull();
    }
}
