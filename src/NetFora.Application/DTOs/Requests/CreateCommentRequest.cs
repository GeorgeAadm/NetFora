using System.ComponentModel.DataAnnotations;

namespace NetFora.Application.DTOs.Requests
{
    public class CreateCommentRequest
    {
        public int PostId { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string Content { get; set; } = string.Empty;
    }
}
