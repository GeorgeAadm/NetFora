using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetFora.Domain.Events;
using NetFora.Domain.Models;

namespace NetFora.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<PostStats> PostStats { get; set; }
        public DbSet<LikeEvent> LikeEvents { get; set; }
        public DbSet<CommentEvent> CommentEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<Post>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Title).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Content).IsRequired();
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Relationship with ApplicationUser
                entity.HasOne(p => p.Author)
                      .WithMany(u => u.Posts)
                      .HasForeignKey(p => p.AuthorId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationship with PostStats
                entity.HasOne(p => p.Stats)
                      .WithOne(s => s.Post)
                      .HasForeignKey<PostStats>(s => s.PostId);


                entity.HasIndex(p => p.CreatedAt);
                entity.HasIndex(p => p.AuthorId);
                entity.HasIndex(p => p.ModerationFlags);
            });


            builder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Content).IsRequired();
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasOne(c => c.Post)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(c => c.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Author)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.AuthorId)
                      .OnDelete(DeleteBehavior.Cascade);


                entity.HasIndex(c => c.PostId);
                entity.HasIndex(c => c.AuthorId);
                entity.HasIndex(c => c.CreatedAt);
                entity.HasIndex(c => c.ModerationFlags);
            });


            builder.Entity<Like>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Unique constraint
                entity.HasIndex(l => new { l.PostId, l.UserId }).IsUnique();

                // Relationships
                entity.HasOne(l => l.Post)
                      .WithMany(p => p.Likes)
                      .HasForeignKey(l => l.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.User)
                      .WithMany(u => u.Likes)
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Cascade);


                entity.HasIndex(l => l.UserId);
            });


            builder.Entity<PostStats>(entity =>
            {
                entity.HasKey(s => s.PostId);
                entity.Property(s => s.LastUpdated).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(s => s.Version).HasDefaultValue(1);


                entity.HasOne(s => s.Post)
                      .WithOne(p => p.Stats)
                      .HasForeignKey<PostStats>(s => s.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            builder.Entity<LikeEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(10);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Indexes for processing
                entity.HasIndex(e => new { e.Processed, e.CreatedAt });
                entity.HasIndex(e => e.PostId);
            });


            builder.Entity<CommentEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Indexes for processing
                entity.HasIndex(e => new { e.Processed, e.CreatedAt });
                entity.HasIndex(e => e.PostId);
            });
        }
    }
}
