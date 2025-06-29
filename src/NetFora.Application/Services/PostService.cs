using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFora.Application.DTOs.Requests;
using NetFora.Application.DTOs.Responses;
using NetFora.Application.Interfaces.Repositories;
using NetFora.Application.Interfaces.Services;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Common;
using NetFora.Domain.Entities;

namespace NetFora.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository; 
        private readonly IPostStatsRepository _statsRepository;
        private readonly ILogger<PostService> _logger;
        public PostService(
            IPostRepository postRepository,
            IPostStatsRepository statsRepository,
            ILogger<PostService> logger)
        {
            _postRepository = postRepository;
            _statsRepository = statsRepository;
            _logger = logger;
        }

        // Business logic implementation - maps from Domain entities to DTOs
        public async Task<PagedResult<PostDto>> GetPostsAsync(PostQueryParameters parameters, string? currentUserId = null)
        {
            throw new NotImplementedException();
        }

        public async Task<PostDto> CreatePostAsync(CreatePostRequest request, string authorId)
        {
            try
            {
                var post = new Post
                {
                    Title = request.Title,
                    Content = request.Content,
                    AuthorId = authorId,
                    CreatedAt = DateTime.UtcNow
                };

                var createdPost = await _postRepository.AddAsync(post);

                // Initialize post stats
                var stats = new PostStats
                {
                    PostId = createdPost.Id,
                    LikeCount = 0,
                    CommentCount = 0
                };
                await _statsRepository.CreateAsync(stats);

                // Reload post with author and stats
                var postWithDetails = await _postRepository.GetByIdWithStatsAsync(createdPost.Id);

                return MapToPostDto(postWithDetails!, authorId, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for user {UserId}", authorId);
                throw;
            }
        }

        public Task<PostDetailDto?> GetPostByIdAsync(int id, string? currentUserId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PostExistsAsync(int postId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserPostAuthorAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<PostDto>> GetUserPostsAsync(string userId, PostQueryParameters parameters)
        {
            throw new NotImplementedException();
        }

        private PostDto MapToPostDto(Post post, string? currentUserId, bool isLiked)
        {
            return new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                ModerationFlags = post.ModerationFlags,
                AuthorName = post.Author.DisplayName,
                IsCurrentUserAuthor = currentUserId == post.AuthorId,
                LikeCount = post.Stats?.LikeCount ?? 0,
                CommentCount = post.Stats?.CommentCount ?? 0,
                IsLikedByCurrentUser = isLiked
            };
        }

    }
}
