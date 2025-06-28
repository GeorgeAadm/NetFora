using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NetFora.Api.Controllers
{
    [ApiController]
    [Route("api/posts/{postId}/likes")]
    [Authorize]
    public class LikesController : ControllerBase
    {
        // GET /api/posts/{postId}/likes/count
        [HttpGet("count")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> GetLikeCount(int postId)
        {
            return Ok();
        }

        // POST /api/posts/{postId}/likes
        [HttpPost]
        public async Task<IActionResult> LikePost(int postId)
        {
            return Ok();
        }

        // DELETE /api/posts/{postId}/likes
        [HttpDelete]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            return Ok();
        }
    }
}
