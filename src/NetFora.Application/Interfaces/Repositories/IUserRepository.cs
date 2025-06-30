using System.Threading.Tasks;
using NetFora.Domain.Entities;

namespace NetFora.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<ApplicationUser?> GetByEmailAsync(string email);

        Task<string?> GetUserIdByUsernameAsync(string username);
        Task<string?> GetDisplayNameAsync(string userId);
        Task<bool> ExistsAsync(string id);
        Task<int> GetUserPostCountAsync(string userId);
        Task<int> GetUserCommentCountAsync(string userId);
    }
}
