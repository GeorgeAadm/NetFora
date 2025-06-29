using System;

namespace NetFora.Application.DTOs.Responses
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
