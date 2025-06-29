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


        public Task<int> GetLikeCountAsync(int postId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsPostLikedByUserAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> LikePostAsync(int postId, string userId)
        {
            // Business Rule Validation
            if (!await _likeRepository.CanUserLikePostAsync(postId, userId))
                return false;

            var likeEvent = new LikeEvent
            {
                PostId = postId,
                UserId = userId,
                Action = "LIKE"
            };

            await _eventService.PublishLikeEventAsync(likeEvent);

            return true;
        }

        public Task<bool> UnlikePostAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
