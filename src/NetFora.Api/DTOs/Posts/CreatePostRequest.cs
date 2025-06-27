using System.ComponentModel.DataAnnotations;

namespace NetFora.Api.DTOs.Posts
{
    public class CreatePostRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
