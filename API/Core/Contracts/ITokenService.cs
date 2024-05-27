using API.Entities;

namespace API.Core.Contracts
{
    public interface ITokenService : IDisposable
    {
        Task<string> CreateTokenAsync(AppUser appUser, TimeSpan? offsetTime = null, string type = "access_token");
        bool RenewRefreshToken(string token, TimeSpan offsetRenewRefreshToken);
    }
}
