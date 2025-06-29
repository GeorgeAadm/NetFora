using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetFora.Application.DTOs.Requests;
using NetFora.Application.DTOs.Responses;
using NetFora.Application.Interfaces.Services;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Common;

namespace NetFora.Api.Controllers
{
    [ApiController]
    [Route("api/posts/{postId}/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        // GET /api/posts/{postId}/comments
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<CommentDto>>> GetComments(int postId, [FromQuery] CommentQueryParameters parameters)
        {
            return Ok();
        }

        // GET /api/posts/{postId}/comments/{commentId}
        [HttpGet("{commentId}")]
        [AllowAnonymous]
        public async Task<ActionResult<CommentDto>> GetComment(int postId, int commentId)
        {
            return Ok();
        }

        // POST /api/posts/{postId}/comments
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CommentDto>> CreateComment(int postId, [FromBody] CreateCommentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            request.PostId = postId;
            var comment = await _commentService.CreateCommentAsync(request, userId);

            return CreatedAtAction(nameof(GetComment), new { postId, commentId = comment.Id }, comment);
        }
    }
}
