using System.Threading.Tasks;
using NetFora.Application.DTOs.Requests;
using NetFora.Application.DTOs.Responses;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Common;

namespace NetFora.Application.Interfaces.Services
{
    public interface ICommentService
    {
        Task<PagedResult<CommentDto>> GetCommentsForPostAsync(int postId, CommentQueryParameters parameters, string? currentUserId = null);
        Task<CommentDto?> GetCommentByIdAsync(int commentId, string? currentUserId = null);
        Task<CommentDto> CreateCommentAsync(CreateCommentRequest request, string authorId);
        Task<bool> CommentExistsAsync(int commentId);
        Task<bool> IsUserCommentAuthorAsync(int commentId, string userId);
    }
}
