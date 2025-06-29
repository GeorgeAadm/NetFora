using System.Threading.Tasks;
using NetFora.Application.DTOs.Responses;

namespace NetFora.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<bool> UserExistsAsync(string userId);
        Task<string?> GetUserDisplayNameAsync(string userId);
    }
}
