using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureVaultApp.Data;

namespace SecureVaultApp.Controllers.API;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        AppDbContext context,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    // GET /api/admin/audit-logs
    [HttpGet("audit-logs")]
    public IActionResult GetAllAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalCount = _context.AuditLogs.Count();

        var logs = _context.AuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        if (!logs.Any())
            return NotFound("No audit logs found.");

        return Ok(new
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            Logs = logs
        });
    }

    // GET /api/admin/audit-logs/{userId}
    [HttpGet("audit-logs/{userId}")]
    public IActionResult GetAuditLogsByUser(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalCount = _context.AuditLogs.Count(l => l.UserId == userId);

        var logs = _context.AuditLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        if (!logs.Any())
            return NotFound($"No audit logs found for user {userId}.");

        return Ok(new
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            Logs = logs
        });
    }

    // POST /api/admin/roles
    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
            return BadRequest("Role name cannot be empty.");

        if (await _roleManager.RoleExistsAsync(roleName))
            return Conflict($"Role {roleName} already exists.");

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok($"Role {roleName} created successfully.");
    }

    // GET /api/admin/roles
    [HttpGet("roles")]
    public IActionResult GetAllRoles()
    {
        var roles = _roleManager.Roles.ToList();

        if (!roles.Any())
            return NotFound("No roles found.");

        return Ok(roles.Select(r => new { r.Id, r.Name }));
    }
}