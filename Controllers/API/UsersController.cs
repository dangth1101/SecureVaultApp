using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureVaultApp.DTOs;
using SecureVaultApp.Helpers;
using SecureVaultApp.Interfaces;

namespace SecureVaultApp.Controllers.API;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IAuditService _auditService;

    public UsersController(
        UserManager<IdentityUser> userManager,
        IAuditService auditService)
    {
        _userManager = userManager;
        _auditService = auditService;
    }

    // GET /api/users
    [HttpGet]
    public IActionResult GetAll()
    {
        var users = _userManager.Users.ToList();
        var result = users.Select(u => new
        {
            u.Id,
            u.UserName,
            Email = MaskingHelper.MaskEmail(u.Email!)
        });

        return Ok(result);
    }

    // GET /api/users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User {id} not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var department = claims.FirstOrDefault(c => c.Type == "department")?.Value ?? "Not assigned";

        return Ok(new
        {
            user.Id,
            user.UserName,
            Email = MaskingHelper.MaskEmail(user.Email!),
            Role = roles.FirstOrDefault() ?? "No role",
            Department = department
        });
    }

    // POST /api/users/{id}/roles
    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AssignRole(string id, [FromBody] RoleAssignDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User {id} not found.");

        // Remove existing roles first
        var existingRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, existingRoles);

        // Assign new role
        var result = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _auditService.LogAsync(
            adminId!,
            "ROLE_ASSIGNED",
            user.Id,
            IpAddressHelper.GetIpAddress(HttpContext));

        return Ok($"Role {dto.Role} assigned to user {user.UserName}.");
    }

    // DELETE /api/users/{id}/roles/{role}
    [HttpDelete("{id}/roles/{role}")]
    public async Task<IActionResult> RemoveRole(string id, string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User {id} not found.");

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _auditService.LogAsync(
            adminId!,
            "ROLE_REMOVED",
            user.Id,
            IpAddressHelper.GetIpAddress(HttpContext));

        return Ok($"Role {role} removed from user {user.UserName}.");
    }

    // GET /api/users/{id}/claims
    [HttpGet("{id}/claims")]
    public async Task<IActionResult> GetClaims(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User {id} not found.");

        var claims = await _userManager.GetClaimsAsync(user);
        return Ok(claims.Select(c => new { c.Type, c.Value }));
    }

    // POST /api/users/{id}/claims
    [HttpPost("{id}/claims")]
    public async Task<IActionResult> AddClaim(string id, [FromBody] ClaimAddDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User {id} not found.");

        // Remove existing claim of same type first
        var existingClaims = await _userManager.GetClaimsAsync(user);
        var existing = existingClaims.FirstOrDefault(c => c.Type == dto.ClaimType);
        if (existing != null)
            await _userManager.RemoveClaimAsync(user, existing);

        var result = await _userManager.AddClaimAsync(user, new Claim(dto.ClaimType, dto.ClaimValue));
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _auditService.LogAsync(
            adminId!,
            "CLAIM_ADDED",
            user.Id,
            IpAddressHelper.GetIpAddress(HttpContext));

        return Ok($"Claim {dto.ClaimType}:{dto.ClaimValue} added to user {user.UserName}.");
    }
}