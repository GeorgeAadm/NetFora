using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFora.Domain.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int ModerationFlags { get; set; } = 0;

        // Foreign keys
        public int PostId { get; set; }
        public string AuthorId { get; set; } = string.Empty;

        // Navigation properties
        public virtual Post Post { get; set; } = null!;
        public virtual ApplicationUser Author { get; set; } = null!;
    }
}
