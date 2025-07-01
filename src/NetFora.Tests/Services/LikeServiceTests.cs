using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetFora.Application.Interfaces.Repositories;
using NetFora.Application.Services;
using NetFora.Domain.Events;
using NetFora.Infrastructure.Interfaces;
using Xunit;

namespace NetFora.Tests.Services
{
    public class LikeServiceTests
{
    private readonly Mock<ILikeRepository> _likeRepositoryMock;
    private readonly Mock<IEventService> _eventServiceMock;
    private readonly Mock<ILogger<LikeService>> _loggerMock;
    private readonly LikeService _sut;

    public LikeServiceTests()
    {
        _likeRepositoryMock = new Mock<ILikeRepository>();
        _eventServiceMock = new Mock<IEventService>();
        _loggerMock = new Mock<ILogger<LikeService>>();
        // Note: IPostRepository is a dependency but not used in the tested methods. 
        // We can pass null or a mock if constructor requires it.
        _sut = new LikeService(
            _likeRepositoryMock.Object,
            new Mock<IPostRepository>().Object,
            _eventServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task LikePostAsync_CanLike_PublishesEventAndReturnsTrue()
    {
        // Arrange
        var postId = 1;
        var userId = "test-user";
        _likeRepositoryMock.Setup(r => r.CanUserLikePostAsync(postId, userId)).ReturnsAsync(true);

        // Act
        var result = await _sut.LikePostAsync(postId, userId);

        // Assert
        Assert.True(result);
        _eventServiceMock.Verify(e => e.PublishLikeEventAsync(
            It.Is<LikeEvent>(ev => ev.PostId == postId && ev.UserId == userId && ev.Action == "LIKE")),
            Times.Once);
    }

    [Fact]
    public async Task LikePostAsync_CannotLike_ReturnsFalseAndDoesNotPublishEvent()
    {
        // Arrange
        var postId = 1;
        var userId = "test-user";
        _likeRepositoryMock.Setup(r => r.CanUserLikePostAsync(postId, userId)).ReturnsAsync(false);

        // Act
        var result = await _sut.LikePostAsync(postId, userId);

        // Assert
        Assert.False(result);
        _eventServiceMock.Verify(e => e.PublishLikeEventAsync(It.IsAny<LikeEvent>()), Times.Never);
    }

    [Fact]
    public async Task UnlikePostAsync_PostIsLiked_PublishesEventAndReturnsTrue()
    {
        // Arrange
        var postId = 1;
        var userId = "test-user";
        _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId)).ReturnsAsync(true);

        // Act
        var result = await _sut.UnlikePostAsync(postId, userId);

        // Assert
        Assert.True(result);
        _eventServiceMock.Verify(e => e.PublishLikeEventAsync(
            It.Is<LikeEvent>(ev => ev.PostId == postId && ev.UserId == userId && ev.Action == "UNLIKE")),
            Times.Once);
    }

    [Fact]
    public async Task UnlikePostAsync_PostNotLiked_ReturnsFalseAndDoesNotPublishEvent()
    {
        // Arrange
        var postId = 1;
        var userId = "test-user";
        _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId)).ReturnsAsync(false);

        // Act
        var result = await _sut.UnlikePostAsync(postId, userId);

        // Assert
        Assert.False(result);
        _eventServiceMock.Verify(e => e.PublishLikeEventAsync(It.IsAny<LikeEvent>()), Times.Never);
    }

        [Fact]
        public async Task IsPostLikedByUserAsync_PostIsLiked_ReturnsTrue()
        {
            // Arrange
            var postId = 1;
            var userId = "user-1";
            _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId)).ReturnsAsync(true);

            // Act
            var result = await _sut.IsPostLikedByUserAsync(postId, userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsPostLikedByUserAsync_PostNotLiked_ReturnsFalse()
        {
            // Arrange
            var postId = 1;
            var userId = "user-1";
            _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId)).ReturnsAsync(false);

            // Act
            var result = await _sut.IsPostLikedByUserAsync(postId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetLikeCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var postId = 1;
            var expectedCount = 5;
            _likeRepositoryMock.Setup(r => r.GetLikeCountAsync(postId)).ReturnsAsync(expectedCount);

            // Act
            var result = await _sut.GetLikeCountAsync(postId);

            // Assert
            Assert.Equal(expectedCount, result);
        }

        [Fact]
        public async Task LikePostAsync_EventServiceThrows_ThrowsException()
        {
            // Arrange
            var postId = 1;
            var userId = "user-1";
            _likeRepositoryMock.Setup(r => r.CanUserLikePostAsync(postId, userId)).ReturnsAsync(true);
            _eventServiceMock.Setup(e => e.PublishLikeEventAsync(It.IsAny<LikeEvent>()))
                .ThrowsAsync(new InvalidOperationException("Event service error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.LikePostAsync(postId, userId));

            Assert.Equal("Event service error", exception.Message);
        }

        [Fact]
        public async Task UnlikePostAsync_EventServiceThrows_ThrowsException()
        {
            // Arrange
            var postId = 1;
            var userId = "user-1";
            _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId)).ReturnsAsync(true);
            _eventServiceMock.Setup(e => e.PublishLikeEventAsync(It.IsAny<LikeEvent>()))
                .ThrowsAsync(new InvalidOperationException("Event service error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.UnlikePostAsync(postId, userId));

            Assert.Equal("Event service error", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task LikePostAsync_WithInvalidUserId_ReturnsFalse(string userId)
        {
            // Arrange
            var postId = 1;
            _likeRepositoryMock.Setup(r => r.CanUserLikePostAsync(postId, userId)).ReturnsAsync(false);

            // Act
            var result = await _sut.LikePostAsync(postId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task LikePostAsync_UserTriesToLikeOwnPost_ReturnsFalse()
        {
            // Arrange
            var postId = 1;
            var userId = "post-author";
            // Simulate that CanUserLikePostAsync returns false because user is the post author
            _likeRepositoryMock.Setup(r => r.CanUserLikePostAsync(postId, userId)).ReturnsAsync(false);

            // Act
            var result = await _sut.LikePostAsync(postId, userId);

            // Assert
            Assert.False(result);
            _eventServiceMock.Verify(e => e.PublishLikeEventAsync(It.IsAny<LikeEvent>()), Times.Never);
        }

        [Fact]
        public async Task UnlikePostAsync_UserTriesToUnlikeNotLikedPost_ReturnsFalse()
        {
            // Arrange
            var postId = 1;
            var userId = "user-1";
            _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId)).ReturnsAsync(false);

            // Act
            var result = await _sut.UnlikePostAsync(postId, userId);

            // Assert
            Assert.False(result);
            _eventServiceMock.Verify(e => e.PublishLikeEventAsync(It.IsAny<LikeEvent>()), Times.Never);
        }
}
}