using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetFora.Application.DTOs;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Common;

namespace NetFora.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        // GET /api/posts
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<PostDto>>> GetPosts([FromQuery] PostQueryParameters parameters)
        {
            return Ok();
        }

        // GET /api/posts/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PostDetailDto>> GetPost(int id)
        {
            return Ok();
        }

        // POST /api/posts
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostRequest request)
        {
            return Ok();
        }
    }

}