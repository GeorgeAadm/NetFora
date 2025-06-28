using System;

namespace NetFora.Application.DTOs
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ModerationFlags { get; set; }

        public string AuthorName { get; set; } = string.Empty;
        public bool IsCurrentUserAuthor { get; set; }

        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        public bool IsMisleading => (ModerationFlags & 1) > 0;
        public bool IsFalse => (ModerationFlags & 2) > 0;
        public bool IsFlagged => ModerationFlags > 0;
    }
}
