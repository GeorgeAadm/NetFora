using NetFora.Application.DTOs;

namespace NetFora.Application.Interfaces
{
    public interface IModerationService
    {
        Task<bool> ModeratePostAsync(int postId, int flags, string moderatorId);
        Task<bool> ModerateCommentAsync(int commentId, int flags, string moderatorId);
        Task<IEnumerable<PostDto>> GetFlaggedPostsAsync();
        Task<IEnumerable<CommentDto>> GetFlaggedCommentsAsync();
    }
}
