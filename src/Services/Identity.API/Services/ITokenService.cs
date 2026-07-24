using Identity.API.Entities;
using System.Security.Claims;

namespace Identity.API.Services
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(ApplicationUser user, IList<string>? roles = null);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}