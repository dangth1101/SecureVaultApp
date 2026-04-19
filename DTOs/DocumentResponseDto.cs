namespace SecureVaultApp.DTOs;

public class DocumentResponseDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Classification { get; set; }
        public string OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }