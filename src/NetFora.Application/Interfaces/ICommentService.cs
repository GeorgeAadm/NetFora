using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetFora.Application.DTOs;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Common;

namespace NetFora.Application.Interfaces
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
