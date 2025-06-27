using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetFora.Api.Models;

namespace NetFora.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Like> PostLikes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Title).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Content).IsRequired();
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relationship - ApplicationUser

            entity.HasOne(p => p.Author)
                  .WithMany(u => u.Posts)
                  .HasForeignKey(p => p.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Like>(entity =>
        {
            entity.HasKey(pl => pl.Id);
            entity.Property(pl => pl.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relationships

            entity.HasOne(pl => pl.Post)
                  .WithMany(p => p.Likes)
                  .HasForeignKey(pl => pl.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pl => pl.User)
                  .WithMany(u => u.Likes)
                  .HasForeignKey(pl => pl.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}