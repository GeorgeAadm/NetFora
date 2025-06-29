using System.Collections.Generic;
using System.Threading.Tasks;
using NetFora.Domain.Entities;

namespace NetFora.Application.Interfaces.Repositories
{
    public interface ILikeRepository
    {
        Task<Like?> GetLikeAsync(int postId, string userId);
        Task<IEnumerable<Like>> GetLikesForPostAsync(int postId);
        Task<Like> AddAsync(Like like);
        Task RemoveAsync(Like like);
        Task RemoveByPostAndUserAsync(int postId, string userId);
        Task<bool> ExistsAsync(int postId, string userId);
        Task<int> GetLikeCountAsync(int postId);
        Task<bool> CanUserLikePostAsync(int postId, string userId);
    }
}
