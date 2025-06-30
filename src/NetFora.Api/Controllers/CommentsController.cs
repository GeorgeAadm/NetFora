using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetFora.Application.DTOs.Requests;
using NetFora.Application.DTOs.Responses;
using NetFora.Application.Interfaces.Services;
using NetFora.Application.QueryParameters;
using NetFora.Application.Services;
using NetFora.Domain.Common;

namespace NetFora.Api.Controllers
{
    [ApiController]
    [Route("api/posts/{postId}/comments")]
    public class CommentsController : ControllerBase
    {

        private readonly ICommentService _commentService;
        private readonly IPostService _postService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentService commentService, IPostService postService, ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _postService = postService;
            _logger = logger;
        }

        /// <summary>
        /// Get comments for a specific post
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="parameters">Query parameters</param>
        /// <returns>Paginated list of comments</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<CommentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagedResult<CommentDto>>> GetComments(int postId, [FromQuery] CommentQueryParameters parameters)
        {
            if (!await _postService.PostExistsAsync(postId))
                return NotFound($"Post with ID {postId} not found");

            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _commentService.GetCommentsForPostAsync(postId, parameters, currentUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for post {PostId}", postId);
                return StatusCode(500, "An error occurred while retrieving comments");
            }

        }

        /// <summary>
        /// Get a specific comment
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="commentId">Comment ID</param>
        /// <returns>Comment details</returns>
        [HttpGet("{commentId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDto>> GetComment(int postId, int commentId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var comment = await _commentService.GetCommentByIdAsync(commentId, currentUserId);
            if (comment == null)
                return NotFound($"Comment with ID {commentId} not found");

            return Ok(comment);
        }

        /// <summary>
        /// Add a comment to a post
        /// </summary>
        /// <param name="request">Comment creation request</param>
        /// <returns>Created comment</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _postService.PostExistsAsync(request.PostId))
                return NotFound($"Post with ID {request.PostId} not found");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            try
            {
                var comment = await _commentService.CreateCommentAsync(request, userId);
                return CreatedAtAction(nameof(GetComment), new { request.PostId, commentId = comment.Id }, comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for post {PostId}", request.PostId);
                return StatusCode(500, "An error occurred while creating the comment");
            }
        }
    }
}
