using System.Threading.Tasks;
using NetFora.Domain.Entities;

namespace NetFora.Infrastructure.Interfaces
{
    public interface IPostStatsRepository
    {
        Task<PostStats?> GetByPostIdAsync(int postId);
        Task<PostStats> CreateAsync(PostStats stats);
        Task UpdateAsync(PostStats stats);
        Task UpdateLikeCountAsync(int postId, int likeCount);
        Task UpdateCommentCountAsync(int postId, int commentCount);
        Task UpsertAsync(PostStats stats);
        Task<bool> ExistsAsync(int postId);
    }
}
