using System.Threading.Tasks;
using NetFora.Application.DTOs;

namespace NetFora.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<bool> UserExistsAsync(string userId);
        Task<string?> GetUserDisplayNameAsync(string userId);
    }
}
