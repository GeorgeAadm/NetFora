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
using NetFora.Domain.Events;
using NetFora.Infrastructure.Interfaces;

namespace NetFora.Application.Services
{
    internal class CommentService : ICommentService
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

        public Task<bool> CommentExistsAsync(int commentId)
        {
            throw new NotImplementedException();
        }

        public async Task<CommentDto> CreateCommentAsync(CreateCommentRequest request, string authorId)
        {
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

        public Task<CommentDto?> GetCommentByIdAsync(int commentId, string? currentUserId = null)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<CommentDto>> GetCommentsForPostAsync(int postId, CommentQueryParameters parameters, string? currentUserId = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserCommentAuthorAsync(int commentId, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
