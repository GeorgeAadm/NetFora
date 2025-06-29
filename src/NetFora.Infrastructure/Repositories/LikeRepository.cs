using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetFora.Application.Interfaces.Repositories;
using NetFora.Domain.Entities;
using NetFora.Infrastructure.Data;

namespace NetFora.Infrastructure.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly ApplicationDbContext _context;

        public LikeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<int, bool>> GetUserLikeStatusForPostsAsync(IEnumerable<int> postIds, string userId)
        {
            var likedPostIds = await _context.Likes
                .Where(l => postIds.Contains(l.PostId) && l.UserId == userId)
                .Select(l => l.PostId)
                .ToListAsync();

            return postIds.ToDictionary(postId => postId, postId => likedPostIds.Contains(postId));
        }

        public async Task<Like?> GetLikeAsync(int postId, string userId)
        {
            return await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<Like> AddAsync(Like like)
        {
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return like;
        }

        
        public async Task RemoveAsync(Like like)
        {
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveByPostAndUserAsync(int postId, string userId)
        {
            var like = await GetLikeAsync(postId, userId);
            if (like != null)
            {
                await RemoveAsync(like);
            }
        }


        public async Task<bool> CanUserLikePostAsync(int postId, string userId)
        {
            // Check if post exists and user is not the author
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null || post.AuthorId == userId)
                return false;

            // Check if user hasn't already liked the post
            return !await ExistsAsync(postId, userId);
        }

        public async Task<bool> ExistsAsync(int postId, string userId)
        {
            return await _context.Likes.AnyAsync(l => l.PostId == postId && l.UserId == userId);
        }

        #region COUNTS

        public async Task<int> GetLikeCountAsync(int postId)
        {
            return await _context.Likes.CountAsync(l => l.PostId == postId);
        }

        public async Task<IEnumerable<Like>> GetLikesForPostAsync(int postId)
        {
            return await _context.Likes
                .Where(l => l.PostId == postId)
                .ToListAsync();
        }

        #endregion


    }
}
