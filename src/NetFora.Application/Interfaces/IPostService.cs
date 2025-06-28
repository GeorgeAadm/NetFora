using System.Threading.Tasks;
using NetFora.Application.DTOs;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Common;

namespace NetFora.Application.Interfaces;

public interface IPostService
{
    Task<PagedResult<PostDto>> GetPostsAsync(PostQueryParameters parameters, string? currentUserId = null);
    Task<PostDetailDto?> GetPostByIdAsync(int id, string? currentUserId = null);
    Task<PostDto> CreatePostAsync(CreatePostRequest request, string authorId);
    Task<bool> PostExistsAsync(int postId);
    Task<bool> IsUserPostAuthorAsync(int postId, string userId);
    Task<PagedResult<PostDto>> GetUserPostsAsync(string userId, PostQueryParameters parameters);
}