﻿using System;

namespace NetFora.Application.DTOs.Responses
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ModerationFlags { get; set; }

        public string AuthorDisplayName { get; set; } = string.Empty;
        public string AuthorUserName { get; set; } = string.Empty;
        public bool IsCurrentUserAuthor { get; set; }

        public bool IsMisleading => (ModerationFlags & 1) > 0;
        public bool IsFalse => (ModerationFlags & 2) > 0;
        public bool IsFlagged => ModerationFlags > 0;
    }
}
