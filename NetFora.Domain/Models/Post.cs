using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFora.Domain.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int ModerationFlags { get; set; } = 0;

        // Foreign key
        public string AuthorId { get; set; } = string.Empty;

        // Navigation properties
        public virtual ApplicationUser Author { get; set; } = null!;
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual PostStats? Stats { get; set; }
    }
}
