using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SecureVaultApp.Interfaces;

namespace SecureVaultApp.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(IdentityUser user, IList<string> roles)
    {
        // Step 1 — Build claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add one claim per role
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Step 2 — Create signing key
        var secret = _configuration["JwtSettings:Secret"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Step 3 — Define the token
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"]!);
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        // Step 4 — Write and return token string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        // Generate 64 cryptographically random bytes and convert to Base64
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        // Validate token but ignore expiry
        var secret = _configuration["JwtSettings:Secret"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // ← ignore expiry on purpose
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidAudience = _configuration["JwtSettings:Audience"],
            IssuerSigningKey = key
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

        // Make sure it was signed with HmacSha256
        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}