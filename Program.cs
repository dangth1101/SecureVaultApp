using SecureVaultApp.Data;
using SecureVaultApp.Extensions;
using SecureVaultApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ===== SERVICES =====
builder.Services.AddControllersWithViews();
builder.Services.AddSecureVaultServices();
builder.Services.AddSecureVaultIdentity();
builder.Services.AddSecureVaultAuthentication(builder.Configuration);
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

app.UseMiddleware<ErrorHandlingMiddleware>();
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