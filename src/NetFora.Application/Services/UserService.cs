using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFora.Application.DTOs.Responses;
using NetFora.Application.Interfaces.Repositories;
using NetFora.Application.Interfaces.Services;

namespace NetFora.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return null;

                var postCount = await _userRepository.GetUserPostCountAsync(userId);
                var commentCount = await _userRepository.GetUserCommentCountAsync(userId);

                return new UserProfileDto
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email!,
                    JoinedDate = user.CreatedAt,
                    PostCount = postCount,
                    CommentCount = commentCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile for {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            return await _userRepository.ExistsAsync(userId);
        }

        public async Task<string?> GetUserDisplayNameAsync(string userId)
        {
            return await _userRepository.GetDisplayNameAsync(userId);
        }
    }
}
