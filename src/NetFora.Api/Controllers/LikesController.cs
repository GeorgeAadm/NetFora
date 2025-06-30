using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        /// <summary>
         /// Get like count for a post
         /// </summary>
         /// <param name="postId">Post ID</param>
         /// <returns>Like count</returns>
        [HttpGet("count")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> GetLikeCount(int postId)
        {
            if (!await _postService.PostExistsAsync(postId))
                return NotFound($"Post with ID {postId} not found");

            var count = await _likeService.GetLikeCountAsync(postId);
            return Ok(count);
        }

        /// <summary>
        /// Like a post
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <returns>Success message</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            if (!await _postService.PostExistsAsync(postId))
                return NotFound($"Post with ID {postId} not found");

            try
            {
                var success = await _likeService.LikePostAsync(postId, userId);
                if (!success)
                    return BadRequest("Unable to like post. Post may already be liked or you cannot like your own post.");

                return Ok(new { message = "Post like request processed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking post {PostId} for user {UserId}", postId, userId);
                return StatusCode(500, "An error occurred while processing the like");
            }
        }


        /// <summary>
        /// Unlike a post
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <returns>Success message</returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            if (!await _postService.PostExistsAsync(postId))
                return NotFound($"Post with ID {postId} not found");

            try
            {
                var success = await _likeService.UnlikePostAsync(postId, userId);
                if (!success)
                    return BadRequest("Unable to unlike post. Post may not be liked.");

                return Ok(new { message = "Post unlike request processed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking post {PostId} for user {UserId}", postId, userId);
                return StatusCode(500, "An error occurred while processing the unlike");
            }
        }

    }
}
