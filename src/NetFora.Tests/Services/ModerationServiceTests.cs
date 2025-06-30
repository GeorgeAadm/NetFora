using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetFora.Application.Interfaces.Repositories;
using NetFora.Application.Services;
using NetFora.Domain.Entities;
using Xunit;


namespace NetFora.Tests.Services
{
    public class ModerationServiceTests
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<ILogger<ModerationService>> _loggerMock;
    private readonly ModerationService _sut;

    public ModerationServiceTests()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _loggerMock = new Mock<ILogger<ModerationService>>();
        _sut = new ModerationService(_postRepositoryMock.Object, _commentRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ModeratePostAsync_PostExists_UpdatesFlagsAndReturnsTrue()
    {
        // Arrange
        var postId = 1;
        var newFlags = 2;
        var moderatorId = "mod-1";
        var post = new Post { Id = postId, ModerationFlags = 0 };
        _postRepositoryMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(post);

        // Act
        var result = await _sut.ModeratePostAsync(postId, newFlags, moderatorId);

        // Assert
        Assert.True(result);
        Assert.Equal(newFlags, post.ModerationFlags); // Check that the object's property was updated
        _postRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Post>(p => p.Id == postId && p.ModerationFlags == newFlags)), Times.Once);
    }

    [Fact]
    public async Task ModeratePostAsync_PostDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var postId = 99;
        _postRepositoryMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync((Post)null);

        // Act
        var result = await _sut.ModeratePostAsync(postId, 1, "mod-1");

        // Assert
        Assert.False(result);
        _postRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Post>()), Times.Never);
    }

    [Fact]
    public async Task ModerateCommentAsync_CommentExists_UpdatesFlagsAndReturnsTrue()
    {
        // Arrange
        var commentId = 1;
        var newFlags = 4;
        var moderatorId = "mod-1";
        var comment = new Comment { Id = commentId, ModerationFlags = 0 };
        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);

        // Act
        var result = await _sut.ModerateCommentAsync(commentId, newFlags, moderatorId);

        // Assert
        Assert.True(result);
        Assert.Equal(newFlags, comment.ModerationFlags);
        _commentRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Comment>(c => c.Id == commentId && c.ModerationFlags == newFlags)), Times.Once);
    }

    [Fact]
    public async Task ModerateCommentAsync_CommentDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var commentId = 99;
        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync((Comment)null);

        // Act
        var result = await _sut.ModerateCommentAsync(commentId, 1, "mod-1");

        // Assert
        Assert.False(result);
        _commentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Comment>()), Times.Never);
    }
}
}