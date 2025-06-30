using System;
using System.Collections.Generic;
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
using NetFora.Domain.Common;

namespace NetFora.Api.Controllers
{
    [ApiController]
    [Route("api/moderation")]
    [Authorize(Roles = "Moderator")]
    public class ModerationController : ControllerBase
    {
        private readonly IModerationService _moderationService;
        private readonly ILogger<ModerationController> _logger;

        public ModerationController(IModerationService moderationService, ILogger<ModerationController> logger)
        {
            _moderationService = moderationService;
            _logger = logger;
        }

        /// <summary>
        /// Moderate a post
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="request">Moderation request</param>
        /// <returns>Success message</returns>
        [HttpPut("posts/{postId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ModeratePost(int postId, [FromBody] ModerationRequest request)
        {
            var moderatorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (moderatorId == null)
                return Unauthorized();

            try
            {
                var success = await _moderationService.ModeratePostAsync(postId, request.Flags, moderatorId);
                if (!success)
                    return NotFound($"Post with ID {postId} not found");

                return Ok(new { message = "Post moderation updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating post {PostId}", postId);
                return StatusCode(500, "An error occurred while updating moderation");
            }
        }


        /// <summary>
        /// Moderate a comment
        /// </summary>
        /// <param name="commentId">Comment ID</param>
        /// <param name="request">Moderation request</param>
        /// <returns>Success message</returns>
        [HttpPut("comments/{commentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ModerateComment(int commentId, [FromBody] ModerationRequest request)
        {
            var moderatorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (moderatorId == null)
                return Unauthorized();

            try
            {
                var success = await _moderationService.ModerateCommentAsync(commentId, request.Flags, moderatorId);
                if (!success)
                    return NotFound($"Comment with ID {commentId} not found");

                return Ok(new { message = "Comment moderation updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating comment {CommentId}", commentId);
                return StatusCode(500, "An error occurred while updating moderation");
            }
        }

    }
}
