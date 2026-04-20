using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureVaultApp.DTOs;
using SecureVaultApp.Interfaces;

namespace SecureVaultApp.Controllers.API;

[ApiController]
[Route("api/security")]
[Authorize(Policy = "AnyRole")]
public class SecurityController : ControllerBase
{
    private readonly IEncryptionService _encryptionService;

    public SecurityController(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    // POST /api/security/encrypt
    [HttpPost("encrypt")]
    public IActionResult Encrypt([FromBody] EncryptRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var cipherText = _encryptionService.Encrypt(dto.PlainText);

        return Ok(new EncryptResponseDto
        {
            CipherText = cipherText
        });
    }

    // POST /api/security/decrypt
    [HttpPost("decrypt")]
    public IActionResult Decrypt([FromBody] EncryptResponseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var plainText = _encryptionService.Decrypt(dto.CipherText);
        if (plainText == null)
            return BadRequest("Invalid or corrupted cipher text.");

        return Ok(new EncryptRequestDto
        {
            PlainText = plainText
        });
    }

    // GET /api/security/token-info
    [HttpGet("token-info")]
    public IActionResult GetTokenInfo()
    {
        var claims = User.Claims.Select(c => new
        {
            c.Type,
            c.Value
        });

        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role),
            Department = User.FindFirstValue("department"),
            Claims = claims
        });
    }
}