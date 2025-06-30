using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFora.Application.Interfaces.Repositories;
using NetFora.Application.Interfaces.Services;
using NetFora.Domain.Events;
using NetFora.Infrastructure.Interfaces;

namespace NetFora.Application.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPostRepository _postRepository;
        private readonly IEventService _eventService;
        private readonly ILogger<LikeService> _logger;

        public LikeService(
            ILikeRepository likeRepository,
            IPostRepository postRepository,
            IEventService eventService,
            ILogger<LikeService> logger)
        {
            _likeRepository = likeRepository;
            _postRepository = postRepository;
            _eventService = eventService;
            _logger = logger;
        }

        // TODO: remove - possible duplication
        public async Task<bool> IsPostLikedByUserAsync(int postId, string userId)
        {
            return await _likeRepository.ExistsAsync(postId, userId);
        }
        // TODO: remove - handlesd by PostStats
        public async Task<int> GetLikeCountAsync(int postId)
        {
            return await _likeRepository.GetLikeCountAsync(postId);
        }


        public async Task<bool> LikePostAsync(int postId, string userId)
        {
            try
            {
                // TODO: remove Check if user can like the post (not author, hasn't already liked)
                if (!await _likeRepository.CanUserLikePostAsync(postId, userId))
                {
                    _logger.LogWarning("User {UserId} cannot like post {PostId}", userId, postId);
                    return false;
                }

                // Publish like event for background processing
                var likeEvent = new LikeEvent
                {
                    PostId = postId,
                    UserId = userId,
                    Action = "LIKE"
                };

                await _eventService.PublishLikeEventAsync(likeEvent);

                _logger.LogInformation("Like event published for post {PostId} by user {UserId}", postId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking post {PostId} for user {UserId}", postId, userId);
                // TODO: Handle err - check bool for success! - do NOT throw!
                throw;
            }
        }

        public async Task<bool> UnlikePostAsync(int postId, string userId)
        {
            try
            {
                // TODO: remove Check if user has liked the post
                if (!await _likeRepository.ExistsAsync(postId, userId))
                {
                    _logger.LogWarning("User {UserId} has not liked post {PostId}", userId, postId);
                    return false;
                }

                // Publish unlike event for background processing
                var likeEvent = new LikeEvent
                {
                    PostId = postId,
                    UserId = userId,
                    Action = "UNLIKE"
                };

                await _eventService.PublishLikeEventAsync(likeEvent);

                _logger.LogInformation("Unlike event published for post {PostId} by user {UserId}", postId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking post {PostId} for user {UserId}", postId, userId);
                // TODO: Handle err - check bool for success! - do NOT throw!
                throw;
            }
        }
    }
}
