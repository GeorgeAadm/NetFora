using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using NetFora.Api.Controllers;
using NetFora.Application.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;

namespace NetFora.Tests.Controllers
{
    public class LikesControllerTests
    {
        private readonly Mock<ILikeService> _mockLikeService;
        private readonly Mock<IPostService> _mockPostService;
        private readonly Mock<ILogger<LikesController>> _mockLogger;
        private readonly LikesController _controller;

        public LikesControllerTests()
        {
            _mockLikeService = new Mock<ILikeService>();
            _mockPostService = new Mock<IPostService>();
            _mockLogger = new Mock<ILogger<LikesController>>();
            _controller = new LikesController(_mockLikeService.Object, _mockPostService.Object, _mockLogger.Object);

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
        public async Task GetLikeCount_ReturnsOk_WithCount()
        {
            // Arrange
            int postId = 1;
            int expectedCount = 5;
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            _mockLikeService.Setup(s => s.GetLikeCountAsync(postId)).ReturnsAsync(expectedCount);

            // Act
            var result = await _controller.GetLikeCount(postId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedCount, Assert.IsType<int>(okResult.Value));
        }

        [Fact]
        public async Task GetLikeCount_ReturnsNotFound_WhenPostDoesNotExist()
        {
            // Arrange
            int postId = 1;
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(false);

            // Act
            var result = await _controller.GetLikeCount(postId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task LikePost_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            int postId = 1;
            string userId = "testUserId";
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            // Simulate a successful like where the user is not the author or hasn't liked it yet
            _mockLikeService.Setup(s => s.LikePostAsync(postId, userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.LikePost(postId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Equal("{ message = Post like request processed }", okResult.Value.ToString());
        }

        [Fact]
        public async Task LikePost_ReturnsBadRequest_WhenUserTriesToLikeOwnPostOrAlreadyLiked()
        {
            // Arrange
            int postId = 1;
            string userId = "testUserId";
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            // Simulate the service returning false, indicating the business rule was violated
            _mockLikeService.Setup(s => s.LikePostAsync(postId, userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.LikePost(postId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unable to like post. Post may already be liked or you cannot like your own post.", badRequestResult.Value);
        }

        [Fact]
        public async Task LikePost_ReturnsUnauthorized_WhenUserIsNull()
        {
            // Arrange
            int postId = 1;
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity()) } // No user claim
            };

            // Act
            var result = await _controller.LikePost(postId);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task LikePost_ReturnsNotFound_WhenPostDoesNotExist()
        {
            // Arrange
            int postId = 1;
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(false);

            // Act
            var result = await _controller.LikePost(postId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task LikePost_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            int postId = 1;
            string userId = "testUserId";
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            _mockLikeService.Setup(s => s.LikePostAsync(postId, userId)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.LikePost(postId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while processing the like", statusCodeResult.Value);
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
        public async Task UnlikePost_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            int postId = 1;
            string userId = "testUserId";
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            _mockLikeService.Setup(s => s.UnlikePostAsync(postId, userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.UnlikePost(postId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Equal("{ message = Post unlike request processed }", okResult.Value.ToString());
        }

        [Fact]
        public async Task UnlikePost_ReturnsUnauthorized_WhenUserIsNull()
        {
            // Arrange
            int postId = 1;
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity()) } // No user claim
            };

            // Act
            var result = await _controller.UnlikePost(postId);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UnlikePost_ReturnsNotFound_WhenPostDoesNotExist()
        {
            // Arrange
            int postId = 1;
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(false);

            // Act
            var result = await _controller.UnlikePost(postId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UnlikePost_ReturnsBadRequest_WhenUnlikeFails()
        {
            // Arrange
            int postId = 1;
            string userId = "testUserId";
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            _mockLikeService.Setup(s => s.UnlikePostAsync(postId, userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.UnlikePost(postId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unable to unlike post. Post may not be liked.", badRequestResult.Value);
        }

        [Fact]
        public async Task UnlikePost_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            int postId = 1;
            string userId = "testUserId";
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            _mockLikeService.Setup(s => s.UnlikePostAsync(postId, userId)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UnlikePost(postId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while processing the unlike", statusCodeResult.Value);
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
        public async Task LikePost_AlreadyLiked_ReturnsBadRequest()
        {
            // Arrange
            int postId = 1;
            string userId = "testUserId";
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            _mockLikeService.Setup(s => s.LikePostAsync(postId, userId)).ReturnsAsync(false); // Already liked

            // Act
            var result = await _controller.LikePost(postId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("already be liked", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task GetLikeCount_WithValidPost_ReturnsCount()
        {
            // Arrange
            int postId = 1;
            int expectedCount = 42;
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            _mockLikeService.Setup(s => s.GetLikeCountAsync(postId)).ReturnsAsync(expectedCount);

            // Act
            var result = await _controller.GetLikeCount(postId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedCount, okResult.Value);
        }

        [Fact]
        public async Task Controller_WithInvalidPostId_ReturnsNotFound()
        {
            // Arrange
            int invalidPostId = -1;
            _mockPostService.Setup(s => s.PostExistsAsync(invalidPostId)).ReturnsAsync(false);

            // Act
            var result = await _controller.GetLikeCount(invalidPostId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
