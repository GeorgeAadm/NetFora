using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetFora.Application.Interfaces.Repositories;
using NetFora.Domain.Entities;
using NetFora.Infrastructure.Data;

namespace NetFora.Infrastructure.Repositories
{
    public class PostStatsRepository : IPostStatsRepository
    {
        private readonly ApplicationDbContext _context;

        public PostStatsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<PostStats> CreateAsync(PostStats stats)
        {
            _context.PostStats.Add(stats);
            await _context.SaveChangesAsync();
            return stats;
        }

        public async Task UpdateAsync(PostStats stats)
        {
            _context.PostStats.Update(stats);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int postId)
        {
            return await _context.PostStats.AnyAsync(s => s.PostId == postId);
        }

        public async Task<PostStats?> GetByPostIdAsync(int postId)
        {
            return await _context.PostStats
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.PostId == postId);
        }

        public async Task UpdateCommentCountAsync(int postId, int commentCount)
        {
            var stats = await GetByPostIdAsync(postId);
            if (stats != null)
            {
                stats.CommentCount = commentCount;
                stats.LastUpdated = DateTime.UtcNow;
                stats.Version++;
                await UpdateAsync(stats);
            }
        }

        public async Task UpdateLikeCountAsync(int postId, int likeCount)
        {
            var stats = await GetByPostIdAsync(postId);
            if (stats != null)
            {
                stats.LikeCount = likeCount;
                stats.LastUpdated = DateTime.UtcNow;
                stats.Version++;
                await UpdateAsync(stats);
            }
        }

        public async Task UpsertAsync(PostStats stats)
        {
            var existing = await GetByPostIdAsync(stats.PostId);
            if (existing == null)
            {
                await CreateAsync(stats);
            }
            else
            {
                existing.LikeCount = stats.LikeCount;
                existing.CommentCount = stats.CommentCount;
                existing.LastUpdated = DateTime.UtcNow;
                existing.Version++;
                await UpdateAsync(existing);
            }
        }
    }
}
