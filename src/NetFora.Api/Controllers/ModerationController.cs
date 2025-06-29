using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetFora.Application.DTOs.Requests;
using NetFora.Application.DTOs.Responses;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Common;

namespace NetFora.Api.Controllers
{
    [ApiController]
    [Route("api/moderation")]
    [Authorize(Roles = "Moderator")]
    public class ModerationController : ControllerBase
    {
        // GET /api/moderation/posts/flagged
        [HttpGet("posts/flagged")]                  
        public async Task<ActionResult<PagedResult<PostDto>>> GetFlaggedPosts([FromQuery] PostQueryParameters parameters)
        {
            return Ok();
        }

        // GET /api/moderation/comments/flagged
        [HttpGet("comments/flagged")]
        public async Task<ActionResult<PagedResult<CommentDto>>> GetFlaggedComments([FromQuery] CommentQueryParameters parameters)
        {
            return Ok();
        }

        // PUT /api/moderation/posts/{postId}
        [HttpPut("posts/{postId}")]                 
        public async Task<IActionResult> ModeratePost(int postId, [FromBody] ModerationRequest request)
        {
            return Ok();
        }

        // PUT /api/moderation/comments/{commentId}
        [HttpPut("comments/{commentId}")]           
        public async Task<IActionResult> ModerateComment(int commentId, [FromBody] ModerationRequest request)
        {
            return Ok();
        }
}
}
