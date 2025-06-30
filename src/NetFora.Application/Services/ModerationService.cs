using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFora.Application.Interfaces.Repositories;
using NetFora.Application.Interfaces.Services;

namespace NetFora.Application.Services
{
    public class ModerationService : IModerationService
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger<ModerationService> _logger;

        public ModerationService(
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            ILogger<ModerationService> logger)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _logger = logger;
        }

        public async Task<bool> ModeratePostAsync(int postId, int flags, string moderatorId)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(postId);
                if (post == null)
                {
                    _logger.LogWarning("Post {PostId} not found for moderation", postId);
                    return false;
                }

                post.ModerationFlags = flags;
                await _postRepository.UpdateAsync(post);

                _logger.LogInformation("Post {PostId} moderated with flags {Flags} by moderator {ModeratorId}",
                    postId, flags, moderatorId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating post {PostId}", postId);
                throw;
            }
        }

        public async Task<bool> ModerateCommentAsync(int commentId, int flags, string moderatorId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(commentId);
                if (comment == null)
                {
                    _logger.LogWarning("Comment {CommentId} not found for moderation", commentId);
                    return false;
                }

                comment.ModerationFlags = flags;
                await _commentRepository.UpdateAsync(comment);

                _logger.LogInformation("Comment {CommentId} moderated with flags {Flags} by moderator {ModeratorId}",
                    commentId, flags, moderatorId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating comment {CommentId}", commentId);
                throw;
            }
        }

    }
}
