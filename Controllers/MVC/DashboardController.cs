using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureVaultApp.ViewModels;

namespace SecureVaultApp.Controllers.MVC;

[Authorize(Policy = "AnyRole")]
public class DashboardController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    // GET /Dashboard
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Auth");

        // Check if Admin → redirect to Admin dashboard
        if (User.IsInRole("Admin"))
            return RedirectToAction("Admin");

        // Build User dashboard
        var department = User.FindFirstValue("department") ?? "Not assigned";
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";

        var model = new UserDashboardViewModel
        {
            UserName = user.UserName!,
            Email = user.Email!,
            Role = role,
            Department = department
        };

        return View(model);
    }

    // GET /Dashboard/Admin
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Admin()
    {
        var users = _userManager.Users.ToList();
        var adminUsers = new List<AdminUserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var department = claims.FirstOrDefault(c => c.Type == "department")?.Value ?? "Not assigned";

            adminUsers.Add(new AdminUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                Role = roles.FirstOrDefault() ?? "No role",
                Department = department
            });
        }

        var model = new AdminDashboardViewModel
        {
            Users = adminUsers
        };

        return View(model);
    }

    // POST /Dashboard/AssignRole
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        // Remove existing roles first
        var existingRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, existingRoles);

        // Assign new role
        await _userManager.AddToRoleAsync(user, role);

        return RedirectToAction("Admin");
    }

    // POST /Dashboard/AssignDepartment
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AssignDepartment(string userId, string department)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        // Remove existing department claim
        var existingClaims = await _userManager.GetClaimsAsync(user);
        var existingDepartment = existingClaims.FirstOrDefault(c => c.Type == "department");
        if (existingDepartment != null)
            await _userManager.RemoveClaimAsync(user, existingDepartment);

        // Assign new department claim
        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("department", department));

        return RedirectToAction("Admin");
    }
}