using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetFora.Application.DTOs.Requests;
using NetFora.Application.Interfaces.Repositories;
using NetFora.Application.QueryParameters;
using NetFora.Application.Services;
using NetFora.Domain.Entities;
using NetFora.Domain.Events;
using NetFora.Infrastructure.Interfaces;
using Xunit;

namespace NetFora.Tests.Services
{
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _commentRepositoryMock;
        private readonly Mock<IPostRepository> _postRepositoryMock;
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly Mock<ILogger<CommentService>> _loggerMock;
        private readonly CommentService _sut;

        public CommentServiceTests()
        {
            _commentRepositoryMock = new Mock<ICommentRepository>();
            _postRepositoryMock = new Mock<IPostRepository>();
            _eventServiceMock = new Mock<IEventService>();
            _loggerMock = new Mock<ILogger<CommentService>>();

            _sut = new CommentService(
                _commentRepositoryMock.Object,
                _postRepositoryMock.Object,
                _eventServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreateCommentAsync_PostExists_CreatesCommentAndPublishesEvent()
        {
            // Arrange
            var request = new CreateCommentRequest { PostId = 1, Content = "Great post!" };
            var authorId = "commenter-1";
            var createdComment = new Comment { Id = 101, PostId = request.PostId, Content = request.Content, AuthorId = authorId };
            var commentWithAuthor = new Comment { Id = 101, Content = "Great post!", Author = new ApplicationUser { DisplayName = "Commenter" } };

            _postRepositoryMock.Setup(r => r.ExistsAsync(request.PostId)).ReturnsAsync(true);
            _commentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Comment>())).ReturnsAsync(createdComment);
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(createdComment.Id)).ReturnsAsync(commentWithAuthor);

            // Act
            var result = await _sut.CreateCommentAsync(request, authorId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdComment.Id, result.Id);

            // Verify dependencies were called
            _commentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
            _eventServiceMock.Verify(e => e.PublishCommentEventAsync(
                It.Is<CommentEvent>(ev => ev.PostId == request.PostId && ev.CommentId == createdComment.Id && ev.Action == "CREATE")),
                Times.Once);
        }

        [Fact]
        public async Task CreateCommentAsync_PostDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new CreateCommentRequest { PostId = 99, Content = "This will fail" };
            var authorId = "commenter-1";
            _postRepositoryMock.Setup(r => r.ExistsAsync(request.PostId)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateCommentAsync(request, authorId));
            Assert.Equal($"Post with ID {request.PostId} does not exist", exception.Message);

            // Ensure no comment was created or event published
            _commentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Never);
            _eventServiceMock.Verify(e => e.PublishCommentEventAsync(It.IsAny<CommentEvent>()), Times.Never);
        }

        [Fact]
        public async Task GetCommentsForPostAsync_WithValidParameters_ReturnsPagedComments()
        {
            // Arrange
            var postId = 1;
            var parameters = new CommentQueryParameters { Page = 1, PageSize = 10 };
            var comments = new List<Comment>
    {
        new Comment { Id = 1, Content = "Comment 1", Author = new ApplicationUser { DisplayName = "User1" } },
        new Comment { Id = 2, Content = "Comment 2", Author = new ApplicationUser { DisplayName = "User2" } }
    };

            _commentRepositoryMock.Setup(r => r.GetCommentsForPostAsync(postId, parameters)).ReturnsAsync(comments);
            _commentRepositoryMock.Setup(r => r.GetTotalCountForPostAsync(postId, parameters)).ReturnsAsync(2);

            // Act
            var result = await _sut.GetCommentsForPostAsync(postId, parameters, "current-user");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetCommentsForPostAsync_WithModerationFlags_FiltersCorrectly()
        {
            // Arrange
            var postId = 1;
            var parameters = new CommentQueryParameters { ModerationFlags = 1 }; // Misleading flag
            var flaggedComments = new List<Comment>
    {
        new Comment { Id = 1, Content = "Flagged comment", ModerationFlags = 1, Author = new ApplicationUser() }
    };

            _commentRepositoryMock.Setup(r => r.GetCommentsForPostAsync(postId, parameters)).ReturnsAsync(flaggedComments);
            _commentRepositoryMock.Setup(r => r.GetTotalCountForPostAsync(postId, parameters)).ReturnsAsync(1);

            // Act
            var result = await _sut.GetCommentsForPostAsync(postId, parameters);

            // Assert
            Assert.Single(result.Items);
            Assert.True(result.Items.First().IsMisleading);
        }

        [Fact]
        public async Task GetCommentByIdAsync_CommentExists_ReturnsCommentDto()
        {
            // Arrange
            var commentId = 1;
            var currentUserId = "user-1";
            var comment = new Comment
            {
                Id = commentId,
                Content = "Test comment",
                AuthorId = currentUserId,
                Author = new ApplicationUser { DisplayName = "Test User", UserName = "testuser" }
            };

            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);

            // Act
            var result = await _sut.GetCommentByIdAsync(commentId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(commentId, result.Id);
            Assert.Equal("Test comment", result.Content);
            Assert.True(result.IsCurrentUserAuthor);
        }

        [Fact]
        public async Task GetCommentByIdAsync_CommentDoesNotExist_ReturnsNull()
        {
            // Arrange
            var commentId = 999;
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync((Comment?)null);

            // Act
            var result = await _sut.GetCommentByIdAsync(commentId, "user-1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateCommentAsync_RepositoryThrowsException_ThrowsAndLogsError()
        {
            // Arrange
            var request = new CreateCommentRequest { PostId = 1, Content = "Test comment" };
            var authorId = "user-1";

            _postRepositoryMock.Setup(r => r.ExistsAsync(request.PostId)).ReturnsAsync(true);
            _commentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Comment>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.CreateCommentAsync(request, authorId));

            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task CommentExistsAsync_CommentExists_ReturnsTrue()
        {
            // Arrange
            var commentId = 1;
            _commentRepositoryMock.Setup(r => r.ExistsAsync(commentId)).ReturnsAsync(true);

            // Act
            var result = await _sut.CommentExistsAsync(commentId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsUserCommentAuthorAsync_UserIsAuthor_ReturnsTrue()
        {
            // Arrange
            var commentId = 1;
            var userId = "user-1";
            _commentRepositoryMock.Setup(r => r.IsUserAuthorAsync(commentId, userId)).ReturnsAsync(true);

            // Act
            var result = await _sut.IsUserCommentAuthorAsync(commentId, userId);

            // Assert
            Assert.True(result);
        }

    }
}