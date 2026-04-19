using System.ComponentModel.DataAnnotations;

namespace SecureVaultApp.DTOs;

public class RoleAssignDto
{
    [Required]
    public string Role { get; set; }
}