using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecureVaultApp.Data;
using SecureVaultApp.Extensions;
using SecureVaultApp.Interfaces;
using SecureVaultApp.Services;
using SecureVaultApp.Services.EncryptionServices;

var builder = WebApplication.CreateBuilder(args);

// ===== SERVICES =====

// Add MVC with Views
builder.Services.AddControllersWithViews();

// Add In-Memory Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("SecureVaultDb"));

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IEncryptionService, AesEncryptionService>();

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddHttpContextAccessor();

// Add ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddSecureVaultPolicies();

// ===== BUILD =====

var app = builder.Build();

// ===== SEED DATABASE =====
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// ===== MIDDLEWARE PIPELINE =====

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();