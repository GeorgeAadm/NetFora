using System;
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
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(IPostService postService, ILogger<PostsController> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        // GET /api/posts
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<PostDto>>> GetPosts([FromQuery] PostQueryParameters parameters)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _postService.GetPostsAsync(parameters, currentUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts with parameters: {@Parameters}", parameters);
                return StatusCode(500, "An error occurred while retrieving posts");
            }
        }

        // GET /api/posts/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PostDetailDto>> GetPost(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var post = await _postService.GetPostByIdAsync(id, currentUserId);

            if (post == null)
                return NotFound($"Post with ID {id} not found");

            return Ok(post);
        }

        // POST /api/posts
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            try
            {
                var post = await _postService.CreatePostAsync(request, userId);
                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for user {UserId}", userId);
                return StatusCode(500, "An error occurred while creating the post");
            }
        }
    }

}