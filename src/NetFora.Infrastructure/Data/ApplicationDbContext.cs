using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetFora.Domain.Entities;
using NetFora.Domain.Events;

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


            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.DisplayName)
                      .IsRequired();

                entity.Property(u => u.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(u => u.UserName)
                      .IsRequired()
                      .HasMaxLength(256);
            });

            builder.Entity<Post>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Title).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Content).IsRequired();
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.ModerationFlags).HasDefaultValue(0);

                entity.HasOne(p => p.Author)
                      .WithMany(u => u.Posts)
                      .HasForeignKey(p => p.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Stats)
                      .WithOne(s => s.Post)
                      .HasForeignKey<PostStats>(s => s.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => p.CreatedAt).HasDatabaseName("IX_Posts_CreatedAt");
                entity.HasIndex(p => p.AuthorId).HasDatabaseName("IX_Posts_AuthorId");
                entity.HasIndex(p => p.ModerationFlags).HasDatabaseName("IX_Posts_ModerationFlags");
            });


            builder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Content).IsRequired();
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(c => c.ModerationFlags).HasDefaultValue(0);

                entity.HasOne(c => c.Post)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(c => c.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Author)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => c.PostId).HasDatabaseName("IX_Comments_PostId");
                entity.HasIndex(c => c.AuthorId).HasDatabaseName("IX_Comments_AuthorId");
                entity.HasIndex(c => c.CreatedAt).HasDatabaseName("IX_Comments_CreatedAt");
                entity.HasIndex(c => c.ModerationFlags).HasDatabaseName("IX_Comments_ModerationFlags");
            });


            builder.Entity<Like>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(l => new { l.PostId, l.UserId })
                      .IsUnique()
                      .HasDatabaseName("UQ_Likes_PostId_UserId");

                entity.HasOne(l => l.Post)
                      .WithMany(p => p.Likes)
                      .HasForeignKey(l => l.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.User)
                      .WithMany(u => u.Likes)
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(l => l.UserId).HasDatabaseName("IX_Likes_UserId");
            });


            builder.Entity<PostStats>(entity =>
            {
                entity.HasKey(s => s.PostId);
                entity.Property(s => s.LikeCount).HasDefaultValue(0);
                entity.Property(s => s.CommentCount).HasDefaultValue(0);
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
                entity.Property(e => e.Action)
                      .IsRequired()
                      .HasMaxLength(10)
                      .HasColumnType("varchar(10)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Processed).HasDefaultValue(false);

                entity.HasIndex(e => new { e.Processed, e.CreatedAt })
                      .HasDatabaseName("IX_LikeEvents_Processed_CreatedAt");
                entity.HasIndex(e => e.PostId).HasDatabaseName("IX_LikeEvents_PostId");
            });


            builder.Entity<CommentEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasColumnType("varchar(20)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Processed).HasDefaultValue(false);

                entity.HasIndex(e => new { e.Processed, e.CreatedAt })
                      .HasDatabaseName("IX_CommentEvents_Processed_CreatedAt");
                entity.HasIndex(e => e.PostId).HasDatabaseName("IX_CommentEvents_PostId");
            });
        }
    }
}
