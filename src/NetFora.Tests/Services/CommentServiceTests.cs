using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetFora.Application.DTOs.Requests;
using NetFora.Application.Interfaces.Repositories;
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
    }
}