using System;
using System.Collections.Generic;

namespace NetFora.Application.DTOs
{
    public class PostDetailDto : PostDto
    {
        public IEnumerable<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public bool IncludeComments { get; set; } = true;

        public DateTime? LastCommentDate { get; set; }
        public string? LastCommentAuthor { get; set; }

        public int CommentsPage { get; set; } = 1;
        public int CommentsPageSize { get; set; } = 20;
        public bool HasMoreComments { get; set; } = false;
    }
}
