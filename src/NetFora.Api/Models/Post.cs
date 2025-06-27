using System.ComponentModel.DataAnnotations;

namespace NetFora.Api.Models;

public class Post
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key
    public string AuthorId { get; set; } = string.Empty;

    // Navigation properties
    public virtual ApplicationUser Author { get; set; } = null!;
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    // Computed property
    public int LikeCount => Likes?.Count ?? 0;
}
