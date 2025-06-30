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
}
}