using System.ComponentModel.DataAnnotations;

namespace SecureVaultApp.DTOs;

public class EncryptRequestDto
    {
        [Required]
        public string PlainText { get; set; }
    }