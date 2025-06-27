using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFora.Domain.Events
{
    public class CommentEvent
    {
        public long Id { get; set; }
        public int PostId { get; set; }
        public int? CommentId { get; set; } // NULL for new comments
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "CREATE" or "DELETE"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Processed { get; set; } = false;
        public DateTime? ProcessedAt { get; set; }
    }
}
