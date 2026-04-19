using System.ComponentModel.DataAnnotations;

namespace SecureVaultApp.DTOs;

public class DocumentCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }

    [Required]
    public string Classification { get; set; }
}