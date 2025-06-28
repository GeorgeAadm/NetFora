using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetFora.Domain.Entities;
using NetFora.Infrastructure.Data;
using NetFora.Infrastructure.Interfaces;

namespace NetFora.Infrastructure.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly ApplicationDbContext _context;

        public LikeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Like> AddAsync(Like like)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CanUserLikePostAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Like?> GetLikeAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetLikeCountAsync(int postId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Like>> GetLikesForPostAsync(int postId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(Like like)
        {
            throw new NotImplementedException();
        }

        public Task RemoveByPostAndUserAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
