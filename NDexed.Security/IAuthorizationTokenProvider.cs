using System;

namespace NDexed.Security
{
    public interface IAuthorizationTokenProvider
    {
       string GenerateAuthorizationToken(Guid userId);
       Guid ValidateAuthorizationToken(string token);
    }
}
