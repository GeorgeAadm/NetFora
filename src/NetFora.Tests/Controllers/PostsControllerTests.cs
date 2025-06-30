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
using System.Linq; // Required for .ToList()

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
            // Ensure all four arguments are provided to the PagedResult constructor
            var expectedResult = new PagedResult<PostDto>(
                new List<PostDto> { new PostDto { Id = 1, Title = "Test Post" } },
                1, // totalCount
                1, // page
                10 // pageSize
            );
            _mockPostService.Setup(s => s.GetPostsAsync(parameters, It.IsAny<string>())).ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetPosts(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagedResult = Assert.IsType<PagedResult<PostDto>>(okResult.Value);
            Assert.Single(pagedResult.Items);
            // Convert to List before accessing by index
            Assert.Equal("Test Post", pagedResult.Items.ToList()[0].Title);
        }

        [Fact]
        public async Task GetPosts_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var parameters = new PostQueryParameters();
            _mockPostService.Setup(s => s.GetPostsAsync(parameters, It.IsAny<string>())).ThrowsAsync(new Exception("Test exception"));

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
            // Assuming PostDetailDto is a valid DTO with an Id and Title property
            var expectedPost = new PostDetailDto { Id = postId, Title = "Specific Post" };
            _mockPostService.Setup(s => s.GetPostByIdAsync(postId, It.IsAny<string>())).ReturnsAsync(expectedPost);

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
            _mockPostService.Setup(s => s.GetPostByIdAsync(postId, It.IsAny<string>())).ReturnsAsync((PostDetailDto)null);

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
            _mockPostService.Setup(s => s.CreatePostAsync(request, It.IsAny<string>())).ReturnsAsync(expectedPost);

            // Act
            var result = await _controller.CreatePost(request);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PostsController.GetPost), createdAtActionResult.ActionName);
            Assert.Equal(expectedPost.Id, ((PostDto)createdAtActionResult.Value).Id);
            Assert.Equal("New Post", ((PostDto)createdAtActionResult.Value).Title);
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
            _mockPostService.Setup(s => s.CreatePostAsync(request, It.IsAny<string>())).ThrowsAsync(new Exception("Test exception"));

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
    }
}
