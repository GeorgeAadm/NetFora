using System;

namespace NetFora.Domain.Entities
{
    public class PostStats
    {
        public int PostId { get; set; }
        public int LikeCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public long Version { get; set; } = 1;

        // Navigation property
        public virtual Post Post { get; set; } = null!;
    }
}
