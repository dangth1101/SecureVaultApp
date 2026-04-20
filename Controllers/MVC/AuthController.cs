using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureVaultApp.Interfaces;
using SecureVaultApp.ViewModels;

namespace SecureVaultApp.Controllers.MVC;

public class AuthController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    // GET /Auth/Register
    [HttpGet]
    public IActionResult Register() => View();

    // POST /Auth/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "Email is already taken.");
            return View(model);
        }

        // Create user
        var user = new IdentityUser
        {
            UserName = model.UserName,
            Email = model.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, "User");

        return RedirectToAction("Login");
    }

    // GET /Auth/Login
    [HttpGet]
    public IActionResult Login() => View();

    // POST /Auth/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Find user by email
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // Check password
        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked. Try again in 15 minutes.");
            return View(model);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // Generate JWT and store in HTTP-only cookie
        var token = await _tokenService.GenerateJwtToken(user);
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(60)
        });

        return RedirectToAction("Index", "Dashboard");
    }

    // POST /Auth/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Clear JWT cookie
        Response.Cookies.Delete("jwt");

        // Sign out Identity session
        await _signInManager.SignOutAsync();

        return RedirectToAction("Login");
    }
}