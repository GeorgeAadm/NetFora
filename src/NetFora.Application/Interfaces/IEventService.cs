using NetFora.Domain.Events;

namespace NetFora.Application.Interfaces
{
    public interface IEventService
    {
        Task PublishLikeEventAsync(LikeEvent likeEvent);
        Task PublishCommentEventAsync(CommentEvent commentEvent);
        Task<List<LikeEvent>> GetUnprocessedLikeEventsAsync(int batchSize = 100);
        Task<List<CommentEvent>> GetUnprocessedCommentEventsAsync(int batchSize = 100);
        Task MarkEventsAsProcessedAsync(IEnumerable<long> likeEventIds, IEnumerable<long> commentEventIds);
    }
}
