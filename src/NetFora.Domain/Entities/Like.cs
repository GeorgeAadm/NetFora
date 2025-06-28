using System;

namespace NetFora.Domain.Entities
{
    public class Like
    {
        public int Id { get; set; }

        // Foreign keys
        public int PostId { get; set; }
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Post Post { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
