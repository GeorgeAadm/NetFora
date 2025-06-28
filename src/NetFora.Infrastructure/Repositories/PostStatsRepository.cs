using System;
using System.Threading.Tasks;
using NetFora.Domain.Entities;
using NetFora.Infrastructure.Interfaces;

namespace NetFora.Infrastructure.Repositories
{
    public class PostStatsRepository : IPostStatsRepository
    {
        public Task<PostStats> CreateAsync(PostStats stats)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(int postId)
        {
            throw new NotImplementedException();
        }

        public Task<PostStats?> GetByPostIdAsync(int postId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(PostStats stats)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCommentCountAsync(int postId, int commentCount)
        {
            throw new NotImplementedException();
        }

        public Task UpdateLikeCountAsync(int postId, int likeCount)
        {
            throw new NotImplementedException();
        }

        public Task UpsertAsync(PostStats stats)
        {
            throw new NotImplementedException();
        }
    }
}
