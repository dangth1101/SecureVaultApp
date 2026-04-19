namespace SecureVaultApp.DTOs;

public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }