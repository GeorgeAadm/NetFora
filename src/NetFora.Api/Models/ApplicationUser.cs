using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace NetFora.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
}