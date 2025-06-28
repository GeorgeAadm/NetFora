using System.Threading.Tasks;

namespace NetFora.Application.Interfaces
{
    public interface ILikeService
    {
        Task<bool> LikePostAsync(int postId, string userId);
        Task<bool> UnlikePostAsync(int postId, string userId);
        Task<bool> IsPostLikedByUserAsync(int postId, string userId);
        Task<int> GetLikeCountAsync(int postId);
    }
}
