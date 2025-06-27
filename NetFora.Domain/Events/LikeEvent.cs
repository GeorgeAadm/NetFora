using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFora.Domain.Events
{
    public class LikeEvent
    {
        public long Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "LIKE" or "UNLIKE"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Processed { get; set; } = false;
        public DateTime? ProcessedAt { get; set; }
    }
}
