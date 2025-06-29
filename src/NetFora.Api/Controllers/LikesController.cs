using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetFora.Application.Interfaces.Services;

namespace NetFora.Api.Controllers
{
    [ApiController]
    [Route("api/posts/{postId}/likes")]
    [Authorize]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly IPostService _postService;
        private readonly ILogger<LikesController> _logger;

        public LikesController(ILikeService likeService, IPostService postService, ILogger<LikesController> logger)
        {
            _likeService = likeService;
            _postService = postService;
            _logger = logger;
        }

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var success = await _likeService.LikePostAsync(postId, userId);

            return Ok(new { message = "Like request processed" });
        }

        // DELETE /api/posts/{postId}/likes
        [HttpDelete]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            return Ok();
        }
    }
}
