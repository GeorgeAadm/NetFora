using System.Threading.Tasks;
using NetFora.Application.DTOs.Responses;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Common;

namespace NetFora.Application.Interfaces.Services
{
    public interface IModerationService
    {
        Task<bool> ModeratePostAsync(int postId, int flags, string moderatorId);
        Task<bool> ModerateCommentAsync(int commentId, int flags, string moderatorId);
        Task<PagedResult<PostDto>> GetFlaggedPostsAsync(PostQueryParameters parameters);
        Task<PagedResult<CommentDto>> GetFlaggedCommentsAsync(CommentQueryParameters parameters);
    }
}
