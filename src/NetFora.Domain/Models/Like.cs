using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFora.Domain.Models
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
