using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using NetFora.Api.Controllers;
using NetFora.Application.Interfaces.Services;
using NetFora.Application.DTOs.Requests;
using NetFora.Application.DTOs.Responses;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFora.Tests.Controllers
{
    public class PostsControllerTests
    {
        private readonly Mock<IPostService> _mockPostService;
        private readonly Mock<ILogger<PostsController>> _mockLogger;
        private readonly PostsController _controller;

        public PostsControllerTests()
        {
            _mockPostService = new Mock<IPostService>();
            _mockLogger = new Mock<ILogger<PostsController>>();
            _controller = new PostsController(_mockPostService.Object, _mockLogger.Object);

            // Setup a default user for authenticated actions
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUserId")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetPosts_ReturnsOk_WithPagedResult()
        {
            // Arrange
            var parameters = new PostQueryParameters();
            var expectedResult = new PagedResult<PostDto>
            {
                Items = new List<PostDto> { new PostDto { Id = 1, Title = "Test Post" } },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };

            _mockPostService
                .Setup(s => s.GetPostsAsync(It.IsAny<PostQueryParameters>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetPosts(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagedResult = Assert.IsType<PagedResult<PostDto>>(okResult.Value);
            Assert.Single(pagedResult.Items);
            Assert.Equal("Test Post", pagedResult.Items.ToList()[0].Title);
        }

        [Fact]
        public async Task GetPosts_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var parameters = new PostQueryParameters();
            _mockPostService
                .Setup(s => s.GetPostsAsync(It.IsAny<PostQueryParameters>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetPosts(parameters);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving posts", statusCodeResult.Value);
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task GetPost_ReturnsOk_WithPostDetailDto()
        {
            // Arrange
            int postId = 1;
            var expectedPost = new PostDetailDto { Id = postId, Title = "Specific Post" };
            _mockPostService
                .Setup(s => s.GetPostByIdAsync(postId, It.IsAny<string>()))
                .ReturnsAsync(expectedPost);

            // Act
            var result = await _controller.GetPost(postId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var post = Assert.IsType<PostDetailDto>(okResult.Value);
            Assert.Equal(postId, post.Id);
            Assert.Equal("Specific Post", post.Title);
        }

        [Fact]
        public async Task GetPost_ReturnsNotFound_WhenPostDoesNotExist()
        {
            // Arrange
            int postId = 1;
            _mockPostService
                .Setup(s => s.GetPostByIdAsync(postId, It.IsAny<string>()))
                .ReturnsAsync((PostDetailDto?)null);

            // Act
            var result = await _controller.GetPost(postId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreatePost_ReturnsCreated_WithPostDto()
        {
            // Arrange
            var request = new CreatePostRequest { Title = "New Post", Content = "Post content" };
            var expectedPost = new PostDto { Id = 10, Title = "New Post", Content = "Post content" };
            _mockPostService
                .Setup(s => s.CreatePostAsync(It.IsAny<CreatePostRequest>(), It.IsAny<string>()))
                .ReturnsAsync(expectedPost);

            // Act
            var result = await _controller.CreatePost(request);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PostsController.GetPost), createdAtActionResult.ActionName);
            Assert.Equal(expectedPost.Id, ((PostDto)createdAtActionResult.Value!).Id);
            Assert.Equal("New Post", ((PostDto)createdAtActionResult.Value!).Title);
        }

        [Fact]
        public async Task CreatePost_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var request = new CreatePostRequest { Title = "", Content = "Post content" }; // Invalid title
            _controller.ModelState.AddModelError("Title", "Title is required.");

            // Act
            var result = await _controller.CreatePost(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreatePost_ReturnsUnauthorized_WhenUserIsNull()
        {
            // Arrange
            var request = new CreatePostRequest { Title = "New Post", Content = "Post content" };
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity()) } // No user claim
            };

            // Act
            var result = await _controller.CreatePost(request);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task CreatePost_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new CreatePostRequest { Title = "New Post", Content = "Post content" };
            _mockPostService
                .Setup(s => s.CreatePostAsync(It.IsAny<CreatePostRequest>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.CreatePost(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while creating the post", statusCodeResult.Value);
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(2, 5)]
        [InlineData(1, 50)]
        public async Task GetPosts_WithDifferentPaginationParameters_ReturnsCorrectPage(int page, int pageSize)
        {
            // Arrange
            var parameters = new PostQueryParameters { Page = page, PageSize = pageSize };
            var expectedResult = new PagedResult<PostDto>
            {
                Items = new List<PostDto> { new PostDto { Id = 1 } },
                TotalCount = 100,
                Page = page,
                PageSize = pageSize
            };

            _mockPostService
                .Setup(s => s.GetPostsAsync(It.IsAny<PostQueryParameters>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetPosts(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagedResult = Assert.IsType<PagedResult<PostDto>>(okResult.Value);
            Assert.Equal(page, pagedResult.Page);
            Assert.Equal(pageSize, pagedResult.PageSize);
        }

        [Fact]
        public async Task GetPosts_WithSearchAndFilterParameters_PassesCorrectParametersToService()
        {
            // Arrange
            var parameters = new PostQueryParameters
            {
                SearchTerm = "test",
                AuthorUserName = "author",
                MinLikes = 5,
                DateFrom = DateTime.Now.AddDays(-7)
            };
            var expectedResult = new PagedResult<PostDto>
            {
                Items = new List<PostDto>(),
                TotalCount = 0,
                Page = 1,
                PageSize = 10
            };

            _mockPostService
                .Setup(s => s.GetPostsAsync(It.IsAny<PostQueryParameters>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResult);

            // Act
            await _controller.GetPosts(parameters);

            // Assert
            _mockPostService.Verify(s => s.GetPostsAsync(
                It.Is<PostQueryParameters>(p =>
                    p.SearchTerm == "test" &&
                    p.AuthorUserName == "author" &&
                    p.MinLikes == 5),
                It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreatePost_WithInvalidTitle_ReturnsBadRequest(string title)
        {
            // Arrange
            var request = new CreatePostRequest { Title = title, Content = "Valid content" };
            _controller.ModelState.AddModelError("Title", "Title is required.");

            // Act
            var result = await _controller.CreatePost(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        public async Task GetPost_WithEdgeCaseIds_HandlesGracefully(int postId)
        {
            // Arrange
            _mockPostService
                .Setup(s => s.GetPostByIdAsync(postId, It.IsAny<string>()))
                .ReturnsAsync((PostDetailDto?)null);

            // Act
            var result = await _controller.GetPost(postId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetPosts_WithAnonymousUser_ReturnsPostsWithoutUserSpecificData()
        {
            // Arrange
            var parameters = new PostQueryParameters();
            var expectedResult = new PagedResult<PostDto>
            {
                Items = new List<PostDto> { new PostDto { Id = 1, Title = "Public Post" } },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };

            // Setup controller with no authenticated user
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            _mockPostService
                .Setup(s => s.GetPostsAsync(It.IsAny<PostQueryParameters>(), null))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetPosts(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagedResult = Assert.IsType<PagedResult<PostDto>>(okResult.Value);
            Assert.Single(pagedResult.Items);

            // Verify that service was called with null user ID
            _mockPostService.Verify(s => s.GetPostsAsync(It.IsAny<PostQueryParameters>(), null), Times.Once);
        }

        [Fact]
        public async Task CreatePost_WithMaxLengthTitle_CreatesSuccessfully()
        {
            // Arrange
            var longTitle = new string('a', 200); // Max length according to your Post entity
            var request = new CreatePostRequest { Title = longTitle, Content = "Content" };
            var expectedPost = new PostDto { Id = 1, Title = longTitle, Content = "Content" };

            _mockPostService
                .Setup(s => s.CreatePostAsync(It.IsAny<CreatePostRequest>(), It.IsAny<string>()))
                .ReturnsAsync(expectedPost);

            // Act
            var result = await _controller.CreatePost(request);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdPost = Assert.IsType<PostDto>(createdAtActionResult.Value);
            Assert.Equal(longTitle, createdPost.Title);
        }

        [Fact]
        public async Task GetPosts_WithComplexSortingAndFiltering_ReturnsCorrectResults()
        {
            // Arrange
            var parameters = new PostQueryParameters
            {
                SortBy = PostSortBy.LikeCount,
                SortDirection = SortDirection.Descending,
                HasComments = true,
                MinLikes = 10
            };
            var expectedResult = new PagedResult<PostDto>
            {
                Items = new List<PostDto>
                {
                    new PostDto { Id = 1, Title = "Popular Post", LikeCount = 15, CommentCount = 5 }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };

            _mockPostService
                .Setup(s => s.GetPostsAsync(It.IsAny<PostQueryParameters>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetPosts(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagedResult = Assert.IsType<PagedResult<PostDto>>(okResult.Value);
            Assert.Single(pagedResult.Items);
            Assert.True(pagedResult.Items.First().LikeCount >= 10);
            Assert.True(pagedResult.Items.First().CommentCount > 0);
        }
    }
}