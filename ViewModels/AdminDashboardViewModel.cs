namespace SecureVaultApp.ViewModels;

public class AdminDashboardViewModel
{
    public List<AdminUserViewModel> Users { get; set; } = new();
    public List<string> AvailableRoles { get; set; } = new() { "Admin", "User" };
    public List<string> AvailableDepartments { get; set; } = new() { "Finance", "Engineering", "HR", "Management" };
}