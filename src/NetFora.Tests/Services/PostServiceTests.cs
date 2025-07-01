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

            _postRepositoryMock = new Mock<IPostRepository>(MockBehavior.Loose);
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
            var parameters = new PostQueryParameters();
            var posts = CreateTestPostList();
        var likeStatuses = new Dictionary<int, bool> { { 1, true }, { 2, false } };

            SetupGetPosts(posts);
            SetupGetTotalCount(2);
            SetupGetUserLikeStatus(likeStatuses, currentUserId);

        // Act
            var result = await _sut.GetPostsAsync(parameters, currentUserId);

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
            var parameters = new PostQueryParameters();
            var posts = CreateSingleTestPost();

            SetupGetPosts(posts);
            SetupGetTotalCount(1);

        // Act
            var result = await _sut.GetPostsAsync(parameters, null);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Items.First().IsLikedByCurrentUser);
        _likeRepositoryMock.Verify(r => r.GetUserLikeStatusForPostsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetPostByIdAsync_PostNotFound_ReturnsNull()
    {
        // Arrange
            SetupGetByIdWithStats(null);

        // Act
        var result = await _sut.GetPostByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
        public async Task GetPostByIdAsync_PostExists_ReturnsPostDetailDto()
        {
            // Arrange
            var postId = 1;
            var currentUserId = "user-1";
            var post = CreateTestPostWithStats(postId, "Test Post", "author-1");
            var comments = CreateTestComments();

            SetupGetByIdWithStats(post);
            SetupLikeExists(true, postId, currentUserId);
            SetupGetComments(comments);

            // Act
            var result = await _sut.GetPostByIdAsync(postId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(postId, result.Id);
            Assert.Equal("Test Post", result.Title);
            Assert.True(result.IsLikedByCurrentUser);
            Assert.Single(result.Comments);
        }

        [Fact]
    public async Task CreatePostAsync_ValidRequest_CreatesPostAndStats()
    {
        // Arrange
        var authorId = "author-1";
        var request = new CreatePostRequest { Title = "New Post", Content = "Some content" };
            var createdPost = CreateTestPost(1, request.Title, authorId);
            var postWithDetails = CreateTestPostWithStats(1, request.Title, authorId);

            SetupAddPost(createdPost);
            SetupGetByIdWithStats(postWithDetails);
            SetupCreateStats();

        // Act
        var result = await _sut.CreatePostAsync(request, authorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdPost.Id, result.Id);
        _postRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
            _statsRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<PostStats>()), Times.Once);
        }

        [Fact]
        public async Task PostExistsAsync_PostExists_ReturnsTrue()
        {
            // Arrange
            var postId = 1;
            SetupPostExists(true, postId);

            // Act
            var result = await _sut.PostExistsAsync(postId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsUserPostAuthorAsync_UserIsAuthor_ReturnsTrue()
        {
            // Arrange
            var postId = 1;
            var userId = "author-1";
            SetupIsUserAuthor(true, postId, userId);

            // Act
            var result = await _sut.IsUserPostAuthorAsync(postId, userId);

            // Assert
            Assert.True(result);
        }

        // Helper methods to avoid expression tree issues
        private void SetupGetPosts(List<Post> posts)
        {
            _postRepositoryMock.SetReturnsDefault<Task<IEnumerable<Post>>>(Task.FromResult<IEnumerable<Post>>(posts));
        }

        private void SetupGetTotalCount(int count)
        {
            _postRepositoryMock.SetReturnsDefault<Task<int>>(Task.FromResult(count));
        }

        private void SetupGetUserLikeStatus(Dictionary<int, bool> likeStatuses, string userId)
        {
            _likeRepositoryMock.Setup(r => r.GetUserLikeStatusForPostsAsync(It.IsAny<IEnumerable<int>>(), userId)).ReturnsAsync(likeStatuses);
        }

        private void SetupGetByIdWithStats(Post post)
        {
            _postRepositoryMock.Setup(r => r.GetByIdWithStatsAsync(It.IsAny<int>())).ReturnsAsync(post);
        }

        private void SetupLikeExists(bool exists, int postId, string userId)
        {
            _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId)).ReturnsAsync(exists);
        }

        private void SetupGetComments(List<Comment> comments)
        {
            _commentRepositoryMock.Setup(r => r.GetCommentsForPostAsync(It.IsAny<int>(), It.IsAny<CommentQueryParameters>())).ReturnsAsync(comments);
        }

        private void SetupAddPost(Post post)
        {
            _postRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Post>())).ReturnsAsync(post);
        }

        private void SetupCreateStats()
        {
            _statsRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<PostStats>())).ReturnsAsync(new PostStats());
        }

        private void SetupPostExists(bool exists, int postId)
        {
            _postRepositoryMock.Setup(r => r.ExistsAsync(postId)).ReturnsAsync(exists);
        }

        private void SetupIsUserAuthor(bool isAuthor, int postId, string userId)
        {
            _postRepositoryMock.Setup(r => r.IsUserAuthorAsync(postId, userId)).ReturnsAsync(isAuthor);
        }

        // Test data creation helpers
        private List<Post> CreateTestPostList()
        {
            return new List<Post> {
                CreateTestPost(1, "Post 1", "other-user"),
                CreateTestPost(2, "Post 2", "other-user")
            };
        }

        private List<Post> CreateSingleTestPost()
        {
            return new List<Post> { CreateTestPost(1, "Single Post", "author") };
        }

        private Post CreateTestPost(int id, string title, string authorId)
        {
            return new Post
            {
                Id = id,
                Title = title,
                Content = "Test content",
                AuthorId = authorId,
                Author = new ApplicationUser { DisplayName = "Test Author", UserName = "testauthor" }
            };
        }

        private Post CreateTestPostWithStats(int id, string title, string authorId)
        {
            var post = CreateTestPost(id, title, authorId);
            post.Stats = new PostStats { LikeCount = 5, CommentCount = 3 };
            return post;
        }

        private List<Comment> CreateTestComments()
        {
            return new List<Comment>
            {
                new Comment
                {
                    Id = 1,
                    Content = "Test comment",
                    Author = new ApplicationUser { DisplayName = "Commenter" },
                    CreatedAt = DateTime.UtcNow
                }
            };
    }
}
}