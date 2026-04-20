
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureVaultApp.Data;
using SecureVaultApp.DTOs;
using SecureVaultApp.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace SecureVaultApp.Controllers.API;

[ApiController]
[Route("api/auth")]
public class ApiAuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _context;

    public ApiAuthController(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService,
        AppDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    // POST /api/auth/refresh-token
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto model)
    {
        if (model == null)
            return BadRequest("Invalid request.");

        // Get principal from expired token
        ClaimsPrincipal principal;
        try
        {
            principal = _tokenService.GetPrincipalFromExpiredToken(model.Token);
        }
        catch
        {
            return BadRequest("Invalid token.");
        }

        // Get user from principal
        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null)
            return Unauthorized("User not found.");

        // Validate refresh token
        var storedToken = _context.RefreshTokens
            .FirstOrDefault(t => t.Token == model.RefreshToken &&
                                 t.UserId == user.Id &&
                                 !t.IsRevoked &&
                                 t.ExpiresAt > DateTime.UtcNow);

        if (storedToken == null)
            return Unauthorized("Invalid or expired refresh token.");

        // Revoke old refresh token
        storedToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        // Generate new tokens
        var newToken = await _tokenService.GenerateJwtToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Save new refresh token
        _context.RefreshTokens.Add(new Models.RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        });
        await _context.SaveChangesAsync();

        return Ok(new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        });
    }
}