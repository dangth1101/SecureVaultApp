using Microsoft.AspNetCore.Authorization;

namespace SecureVaultApp.Extensions;

public static class AuthorizationPolicyExtensions
{
    public static IServiceCollection AddSecureVaultPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Level 1 — must be authenticated
            options.AddPolicy("Authenticated", p =>
                p.RequireAuthenticatedUser());

            // Level 2 — Admin only
            options.AddPolicy("AdminOnly", p =>
                p.RequireRole("Admin"));

            // Level 3 — Any authenticated role ← moved here
            options.AddPolicy("AnyRole", p =>
                p.RequireRole("Admin", "User"));

            // Level 4 — Department access
            options.AddPolicy("FinanceAccess", p =>
                p.RequireAssertion(IsAdminOrDepartment("Finance")));

            options.AddPolicy("EngineeringAccess", p =>
                p.RequireAssertion(IsAdminOrDepartment("Engineering")));

            options.AddPolicy("HRAccess", p =>
                p.RequireAssertion(IsAdminOrDepartment("HR")));
        });

        return services;
    }

    // Reusable helper — check Admin first, then department
    private static Func<AuthorizationHandlerContext, bool> IsAdminOrDepartment(string department)
    {
        return ctx =>
        {
            // Check Admin first
            if (ctx.User.IsInRole("Admin"))
                return true;

            // Then check department
            return ctx.User.IsInRole("User") &&
                   ctx.User.HasClaim("department", department);
        };
    }
}