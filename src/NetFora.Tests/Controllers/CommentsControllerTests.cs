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
    public class CommentsControllerTests
    {
        private readonly Mock<ICommentService> _mockCommentService;
        private readonly Mock<IPostService> _mockPostService;
        private readonly Mock<ILogger<CommentsController>> _mockLogger;
        private readonly CommentsController _controller;

        public CommentsControllerTests()
        {
            _mockCommentService = new Mock<ICommentService>();
            _mockPostService = new Mock<IPostService>();
            _mockLogger = new Mock<ILogger<CommentsController>>();
            _controller = new CommentsController(_mockCommentService.Object, _mockPostService.Object, _mockLogger.Object);

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
        public async Task GetComments_ReturnsOk_WithPagedResult()
        {
            // Arrange
            int postId = 1;
            var parameters = new CommentQueryParameters();
            // Use the new PagedResult constructor with all parameters
            var expectedResult = new PagedResult<CommentDto>(
                new List<CommentDto> { new CommentDto { Id = 1, Content = "Test Comment" } },
                1, // totalCount
                1, // page
                10 // pageSize
            );
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            _mockCommentService.Setup(s => s.GetCommentsForPostAsync(postId, parameters, It.IsAny<string>())).ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetComments(postId, parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagedResult = Assert.IsType<PagedResult<CommentDto>>(okResult.Value);
            Assert.Single(pagedResult.Items);
            // Convert to List before accessing by index
            Assert.Equal("Test Comment", pagedResult.Items.ToList()[0].Content);
        }

        [Fact]
        public async Task GetComments_ReturnsNotFound_WhenPostDoesNotExist()
        {
            // Arrange
            int postId = 1;
            var parameters = new CommentQueryParameters();
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(false);

            // Act
            var result = await _controller.GetComments(postId, parameters);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetComments_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            int postId = 1;
            var parameters = new CommentQueryParameters();
            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            _mockCommentService.Setup(s => s.GetCommentsForPostAsync(postId, parameters, It.IsAny<string>())).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetComments(postId, parameters);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving comments", statusCodeResult.Value);
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
        public async Task GetComment_ReturnsOk_WithCommentDto()
        {
            // Arrange
            int postId = 1;
            int commentId = 1;
            var expectedComment = new CommentDto { Id = commentId, Content = "Specific Comment" };
            _mockCommentService.Setup(s => s.GetCommentByIdAsync(commentId, It.IsAny<string>())).ReturnsAsync(expectedComment);

            // Act
            var result = await _controller.GetComment(postId, commentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var comment = Assert.IsType<CommentDto>(okResult.Value);
            Assert.Equal(commentId, comment.Id);
            Assert.Equal("Specific Comment", comment.Content);
        }

        [Fact]
        public async Task GetComment_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            // Arrange
            int postId = 1;
            int commentId = 1;
            _mockCommentService.Setup(s => s.GetCommentByIdAsync(commentId, It.IsAny<string>())).ReturnsAsync((CommentDto)null);

            // Act
            var result = await _controller.GetComment(postId, commentId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateComment_ReturnsCreated_WithCommentDto()
        {
            // Arrange
            var request = new CreateCommentRequest { PostId = 1, Content = "New Comment" };
            // Removed PostId from CommentDto initialization as it's not a property of CommentDto
            var expectedComment = new CommentDto { Id = 10, Content = "New Comment" };
            _mockPostService.Setup(s => s.PostExistsAsync(request.PostId)).ReturnsAsync(true);
            _mockCommentService.Setup(s => s.CreateCommentAsync(request, It.IsAny<string>())).ReturnsAsync(expectedComment);

            // Act
            var result = await _controller.CreateComment(request);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(CommentsController.GetComment), createdAtActionResult.ActionName);
            Assert.Equal(expectedComment.Id, ((CommentDto)createdAtActionResult.Value).Id);
            Assert.Equal("New Comment", ((CommentDto)createdAtActionResult.Value).Content);
        }

        [Fact]
        public async Task CreateComment_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var request = new CreateCommentRequest { PostId = 1, Content = "" }; // Invalid content
            _controller.ModelState.AddModelError("Content", "Content is required.");

            // Act
            var result = await _controller.CreateComment(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateComment_ReturnsNotFound_WhenPostDoesNotExist()
        {
            // Arrange
            var request = new CreateCommentRequest { PostId = 1, Content = "New Comment" };
            _mockPostService.Setup(s => s.PostExistsAsync(request.PostId)).ReturnsAsync(false);

            // Act
            var result = await _controller.CreateComment(request);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateComment_ReturnsUnauthorized_WhenUserIsNull()
        {
            // Arrange
            var request = new CreateCommentRequest { PostId = 1, Content = "New Comment" };
            _mockPostService.Setup(s => s.PostExistsAsync(request.PostId)).ReturnsAsync(true);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity()) } // No user claim
            };

            // Act
            var result = await _controller.CreateComment(request);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task CreateComment_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new CreateCommentRequest { PostId = 1, Content = "New Comment" };
            _mockPostService.Setup(s => s.PostExistsAsync(request.PostId)).ReturnsAsync(true);
            _mockCommentService.Setup(s => s.CreateCommentAsync(request, It.IsAny<string>())).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.CreateComment(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while creating the comment", statusCodeResult.Value);
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
        [InlineData(CommentSortBy.CreatedDate, SortDirection.Ascending)]
        [InlineData(CommentSortBy.CreatedDate, SortDirection.Descending)]
        [InlineData(CommentSortBy.AuthorName, SortDirection.Ascending)]
        public async Task GetComments_WithDifferentSortingOptions_PassesCorrectParameters(CommentSortBy sortBy, SortDirection direction)
        {
            // Arrange
            int postId = 1;
            var parameters = new CommentQueryParameters { SortBy = sortBy, SortDirection = direction };
            var expectedResult = new PagedResult<CommentDto>(new List<CommentDto>(), 0, 1, 20);

            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            _mockCommentService.Setup(s => s.GetCommentsForPostAsync(postId, parameters, It.IsAny<string>())).ReturnsAsync(expectedResult);

            // Act
            await _controller.GetComments(postId, parameters);

            // Assert
            _mockCommentService.Verify(s => s.GetCommentsForPostAsync(postId,
                It.Is<CommentQueryParameters>(p => p.SortBy == sortBy && p.SortDirection == direction),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetComments_WithModerationFlagsFilter_FiltersCorrectly()
        {
            // Arrange
            int postId = 1;
            var parameters = new CommentQueryParameters { ModerationFlags = 1 }; // Misleading flag
            var expectedResult = new PagedResult<CommentDto>(
                new List<CommentDto> { new CommentDto { Id = 1, ModerationFlags = 1 } },
                1, 1, 20
            );

            _mockPostService.Setup(s => s.PostExistsAsync(postId)).ReturnsAsync(true);
            _mockCommentService.Setup(s => s.GetCommentsForPostAsync(postId, parameters, It.IsAny<string>())).ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetComments(postId, parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagedResult = Assert.IsType<PagedResult<CommentDto>>(okResult.Value);
            Assert.True(pagedResult.Items.First().IsMisleading);
        }

        [Theory]
        [InlineData(2001)] // Exceeds max length
        public async Task CreateComment_WithContentTooLong_ReturnsBadRequest(int contentLength)
        {
            // Arrange
            var longContent = new string('a', contentLength);
            var request = new CreateCommentRequest { PostId = 1, Content = longContent };
            _controller.ModelState.AddModelError("Content", "Comment cannot exceed 2000 characters");

            // Act
            var result = await _controller.CreateComment(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
