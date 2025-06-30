using System.Collections.Generic;
using System.Threading.Tasks;
using NetFora.Domain.Entities;
using NetFora.Application.QueryParameters;

namespace NetFora.Application.Interfaces.Repositories
{
    public interface IPostRepository
    {
        Task<Post?> GetByIdAsync(int id);
        Task<Post?> GetByIdWithStatsAsync(int id);
        Task<IEnumerable<Post>> GetPostsAsync(PostQueryParameters parameters, string? authorId = null);
        Task<IEnumerable<Post>> GetUserPostsAsync(string userId, PostQueryParameters parameters);
        Task<IEnumerable<Post>> GetFlaggedPostsAsync(PostQueryParameters parameters);
        Task<Post> AddAsync(Post post);
        Task UpdateAsync(Post post);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsUserAuthorAsync(int postId, string userId);
        Task<int> GetTotalCountAsync(PostQueryParameters parameters, string? authorId = null);
        Task<int> GetUserPostCountAsync(string userId, PostQueryParameters parameters);
        Task<int> GetFlaggedPostCountAsync(PostQueryParameters parameters);
    }
}
