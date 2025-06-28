using System;

namespace NetFora.Application.DTOs
{
    public class UserProfileDto
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
    }
}
