using System.Collections.Generic;
using System.Threading.Tasks;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Entities;

namespace NetFora.Application.Interfaces.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(int id);
        Task<IEnumerable<Comment>> GetCommentsForPostAsync(int postId, CommentQueryParameters parameters);
        Task<IEnumerable<Comment>> GetFlaggedCommentsAsync(CommentQueryParameters parameters);
        Task<Comment> AddAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsUserAuthorAsync(int commentId, string userId);
        Task<int> GetCommentCountForPostAsync(int postId);
        Task<int> GetTotalCountForPostAsync(int postId, CommentQueryParameters parameters);
        Task<int> GetFlaggedCommentCountAsync(CommentQueryParameters parameters);
    }
}
