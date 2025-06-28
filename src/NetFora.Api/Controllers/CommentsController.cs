using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetFora.Application.DTOs;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Common;

namespace NetFora.Api.Controllers
{
    [ApiController]
    [Route("api/posts/{postId}/comments")]
    public class CommentsController : ControllerBase
    {
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
            return Ok();
        }
    }
}
