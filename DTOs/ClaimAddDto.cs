using System.ComponentModel.DataAnnotations;

namespace SecureVaultApp.DTOs;

public class ClaimAddDto
{
    [Required]
    public string ClaimType { get; set; }

    [Required]
    public string ClaimValue { get; set; }
}