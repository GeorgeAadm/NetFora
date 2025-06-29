using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ILikeRepository _likeRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger<PostService> _logger;
        public PostService(
            IPostRepository postRepository,
            ILikeRepository likeRepository,
            IPostStatsRepository statsRepository,
            ICommentRepository commentRepository,
            ILogger<PostService> logger)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _statsRepository = statsRepository;
            _commentRepository = commentRepository;
            _logger = logger;
        }

        public async Task<PagedResult<PostDto>> GetPostsAsync(PostQueryParameters parameters, string? currentUserId = null)
        {
            try
            {
                var posts = await _postRepository.GetPostsAsync(parameters);
                var totalCount = await _postRepository.GetTotalCountAsync(parameters);

                var postDtos = new List<PostDto>();

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    // Check likes for posts NOT authored by current user
                    var postsNotByUser = posts.Where(p => p.AuthorId != currentUserId).Select(p => p.Id).ToList();

                    Dictionary<int, bool> likeStatuses = new();
                    if (postsNotByUser.Any())
                    {
                        likeStatuses = await _likeRepository.GetUserLikeStatusForPostsAsync(postsNotByUser, currentUserId);
                    }

                    foreach (var post in posts)
                    {
                        // User's own posts: cannot be liked by them
                        var isLiked = post.AuthorId != currentUserId && likeStatuses.GetValueOrDefault(post.Id, false);
                        postDtos.Add(MapToPostDto(post, currentUserId, isLiked));
                    }
                }
                else
                {
                    // Anonymous users - no likes to check
                    foreach (var post in posts)
                    {
                        postDtos.Add(MapToPostDto(post, null, false));
                    }
                }

                return new PagedResult<PostDto>
                {
                    Items = postDtos,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts with parameters: {@Parameters}", parameters);
                throw;
            }
        }

        public async Task<PostDetailDto?> GetPostByIdAsync(int id, string? currentUserId = null)
        {
            try
            {
                var post = await _postRepository.GetByIdWithStatsAsync(id);
                if (post == null)
                    return null;

                var isLiked = false;
                if (!string.IsNullOrEmpty(currentUserId) && post.AuthorId != currentUserId)
                {
                    isLiked = await _likeRepository.ExistsAsync(post.Id, currentUserId);
                }

                var postDto = MapToPostDto(post, currentUserId, isLiked);

                // Get comments for the post
                var commentParams = new CommentQueryParameters { Page = 1, PageSize = 20 };
                var comments = await _commentRepository.GetCommentsForPostAsync(id, commentParams);

                var commentDtos = new List<CommentDto>();
                foreach (var comment in comments)
                {
                    commentDtos.Add(MapToCommentDto(comment, currentUserId));
                }

                return new PostDetailDto
                {
                    Id = postDto.Id,
                    Title = postDto.Title,
                    Content = postDto.Content,
                    CreatedAt = postDto.CreatedAt,
                    ModerationFlags = postDto.ModerationFlags,
                    AuthorName = postDto.AuthorName,
                    IsCurrentUserAuthor = postDto.IsCurrentUserAuthor,
                    LikeCount = postDto.LikeCount,
                    CommentCount = postDto.CommentCount,
                    IsLikedByCurrentUser = postDto.IsLikedByCurrentUser,
                    Comments = commentDtos,
                    IncludeComments = true,
                    LastCommentDate = comments.Any() ? comments.Max(c => c.CreatedAt) : null,
                    LastCommentAuthor = comments.OrderByDescending(c => c.CreatedAt).FirstOrDefault()?.Author.DisplayName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post {PostId}", id);
                throw;
            }
        }

        public async Task<PagedResult<PostDto>> GetUserPostsAsync(string userId, PostQueryParameters parameters)
        {
            try
            {
                var posts = await _postRepository.GetUserPostsAsync(userId, parameters);
                var totalCount = await _postRepository.GetUserPostCountAsync(userId, parameters);

                var postDtos = posts.Select(post => MapToPostDto(post, userId, false)).ToList();

                return new PagedResult<PostDto>
                {
                    Items = postDtos,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for user {UserId}", userId);
                throw;
            }
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


        public async Task<bool> PostExistsAsync(int postId)
        {
            return await _postRepository.ExistsAsync(postId);
        }

        public async Task<bool> IsUserPostAuthorAsync(int postId, string userId)
        {
            return await _postRepository.IsUserAuthorAsync(postId, userId);
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

        private CommentDto MapToCommentDto(Comment comment, string? currentUserId)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                ModerationFlags = comment.ModerationFlags,
                AuthorName = comment.Author.DisplayName,
                IsCurrentUserAuthor = currentUserId == comment.AuthorId
            };
        }

    }
}
