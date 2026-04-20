using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace SecureVaultApp.Interfaces;

public interface ITokenService
{
    Task<string> GenerateJwtToken(IdentityUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}