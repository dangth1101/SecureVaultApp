using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace SecureVaultApp.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(IdentityUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}