using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetFora.Application.DTOs.Requests;
using NetFora.Application.DTOs.Responses;
using NetFora.Application.Interfaces.Repositories;
using NetFora.Application.QueryParameters;
using NetFora.Application.Services;
using NetFora.Domain.Common;
using NetFora.Domain.Entities;
using Xunit;


namespace NetFora.Tests.Services
{
    public class PostServiceTests
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IPostStatsRepository> _statsRepositoryMock;
    private readonly Mock<ILikeRepository> _likeRepositoryMock;
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<PostService>> _loggerMock;
    private readonly PostService _sut;

    public PostServiceTests()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _statsRepositoryMock = new Mock<IPostStatsRepository>();
        _likeRepositoryMock = new Mock<ILikeRepository>();
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<PostService>>();

        _sut = new PostService(
            _postRepositoryMock.Object,
            _likeRepositoryMock.Object,
            _statsRepositoryMock.Object,
            _commentRepositoryMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetPostsAsync_WithAuthenticatedUser_CorrectlySetsLikeStatus()
    {
        // Arrange
        var currentUserId = "current-user";
        var posts = new List<Post> {
            new Post { Id = 1, Author = new ApplicationUser() },
            new Post { Id = 2, Author = new ApplicationUser() }
        };
        var likeStatuses = new Dictionary<int, bool> { { 1, true }, { 2, false } };

        _postRepositoryMock.Setup(r => r.GetPostsAsync(It.IsAny<PostQueryParameters>(), null)).ReturnsAsync(posts);
        _postRepositoryMock.Setup(r => r.GetTotalCountAsync(It.IsAny<PostQueryParameters>(), null)).ReturnsAsync(2);
        _likeRepositoryMock.Setup(r => r.GetUserLikeStatusForPostsAsync(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 1, 2 })), currentUserId))
            .ReturnsAsync(likeStatuses);

        // Act
        var result = await _sut.GetPostsAsync(new PostQueryParameters(), currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.True(result.Items.First(p => p.Id == 1).IsLikedByCurrentUser);
        Assert.False(result.Items.First(p => p.Id == 2).IsLikedByCurrentUser);
        _likeRepositoryMock.Verify(r => r.GetUserLikeStatusForPostsAsync(It.IsAny<IEnumerable<int>>(), currentUserId), Times.Once);
    }

    [Fact]
    public async Task GetPostsAsync_WithUnauthenticatedUser_DoesNotCheckLikeStatus()
    {
        // Arrange
        var posts = new List<Post> { new Post { Id = 1, Author = new ApplicationUser() } };
        _postRepositoryMock.Setup(r => r.GetPostsAsync(It.IsAny<PostQueryParameters>(), null)).ReturnsAsync(posts);
        _postRepositoryMock.Setup(r => r.GetTotalCountAsync(It.IsAny<PostQueryParameters>(), null)).ReturnsAsync(1);

        // Act
        var result = await _sut.GetPostsAsync(new PostQueryParameters(), null); // currentUserId is null

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Items.First().IsLikedByCurrentUser);
        _likeRepositoryMock.Verify(r => r.GetUserLikeStatusForPostsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetPostByIdAsync_PostNotFound_ReturnsNull()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetByIdWithStatsAsync(It.IsAny<int>())).ReturnsAsync((Post)null);

        // Act
        var result = await _sut.GetPostByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreatePostAsync_ValidRequest_CreatesPostAndStats()
    {
        // Arrange
        var authorId = "author-1";
        var request = new CreatePostRequest { Title = "New Post", Content = "Some content" };
        var createdPost = new Post { Id = 1, Title = request.Title, Content = request.Content, AuthorId = authorId, CreatedAt = DateTime.UtcNow };
        var postWithDetails = new Post { Id = 1, Title = request.Title, Content = request.Content, AuthorId = authorId, Author = new ApplicationUser { DisplayName = "Author" }, Stats = new PostStats() };

        _postRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Post>())).ReturnsAsync(createdPost);
        _postRepositoryMock.Setup(r => r.GetByIdWithStatsAsync(createdPost.Id)).ReturnsAsync(postWithDetails);

        // Act
        var result = await _sut.CreatePostAsync(request, authorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdPost.Id, result.Id);
        _postRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
        _statsRepositoryMock.Verify(r => r.CreateAsync(It.Is<PostStats>(s => s.PostId == createdPost.Id)), Times.Once);
    }
}
}