using System;
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
using NetFora.Domain.Events;
using NetFora.Infrastructure.Interfaces;

namespace NetFora.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IEventService _eventService;
        private readonly ILogger<CommentService> _logger;

        public CommentService(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            IEventService eventService,
            ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _eventService = eventService;
            _logger = logger;
        }

        public async Task<PagedResult<CommentDto>> GetCommentsForPostAsync(int postId, CommentQueryParameters parameters, string? currentUserId = null)
        {
            try
            {
                var comments = await _commentRepository.GetCommentsForPostAsync(postId, parameters);
                var totalCount = await _commentRepository.GetTotalCountForPostAsync(postId, parameters);

                var commentDtos = comments.Select(comment => new CommentDto
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    ModerationFlags = comment.ModerationFlags,
                    AuthorDisplayName = comment.Author.DisplayName,
                    AuthorUserName = comment.Author.UserName,
                    IsCurrentUserAuthor = currentUserId == comment.AuthorId
                });

                return new PagedResult<CommentDto>
                {
                    Items = commentDtos,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for post {PostId}", postId);
                throw;
            }
        }


        public async Task<CommentDto> CreateCommentAsync(CreateCommentRequest request, string authorId)
        {
            try
            {
                if (!await _postRepository.ExistsAsync(request.PostId))
                {
                    throw new InvalidOperationException($"Post with ID {request.PostId} does not exist");
                }

                var comment = new Comment
                {
                    Content = request.Content,
                    PostId = request.PostId,
                    AuthorId = authorId,
                    CreatedAt = DateTime.UtcNow
                };

                var createdComment = await _commentRepository.AddAsync(comment);

                // Publish comment event for background processing
                var commentEvent = new CommentEvent
                {
                    PostId = request.PostId,
                    CommentId = createdComment.Id,
                    UserId = authorId,
                    Action = "CREATE"
                };
                await _eventService.PublishCommentEventAsync(commentEvent);

                // Reload comment with author details
                var commentWithAuthor = await _commentRepository.GetByIdAsync(createdComment.Id);

                return new CommentDto
                {
                    Id = commentWithAuthor!.Id,
                    Content = commentWithAuthor.Content,
                    CreatedAt = commentWithAuthor.CreatedAt,
                    ModerationFlags = commentWithAuthor.ModerationFlags,
                    AuthorDisplayName = commentWithAuthor.Author.DisplayName,
                    AuthorUserName = commentWithAuthor.Author.UserName,
                    IsCurrentUserAuthor = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for post {PostId} by user {UserId}", request.PostId, authorId);
                throw;
            }
        }

        public async Task<CommentDto?> GetCommentByIdAsync(int commentId, string? currentUserId = null)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(commentId);
                if (comment == null)
                    return null;

                return new CommentDto
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    ModerationFlags = comment.ModerationFlags,
                    AuthorDisplayName = comment.Author.DisplayName,
                    AuthorUserName = comment.Author.UserName,
                    IsCurrentUserAuthor = currentUserId == comment.AuthorId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task<bool> CommentExistsAsync(int commentId)
        {
            return await _commentRepository.ExistsAsync(commentId);
        }
        public async Task<bool> IsUserCommentAuthorAsync(int commentId, string userId)
        {
            return await _commentRepository.IsUserAuthorAsync(commentId, userId);
        }
    }
}
