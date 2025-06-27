using Microsoft.EntityFrameworkCore;
using NetFora.Domain.Events;
using NetFora.Infrastructure.Data;

namespace NetFora.Infrastructure.Services
{
    public interface IEventService
    {
        Task PublishLikeEventAsync(LikeEvent likeEvent);
        Task PublishCommentEventAsync(CommentEvent commentEvent);
        Task<List<LikeEvent>> GetUnprocessedLikeEventsAsync(int batchSize = 100);
        Task<List<CommentEvent>> GetUnprocessedCommentEventsAsync(int batchSize = 100);
        Task MarkEventsAsProcessedAsync(IEnumerable<long> likeEventIds, IEnumerable<long> commentEventIds);
    }
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;

        public EventService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task PublishLikeEventAsync(LikeEvent likeEvent)
        {
            _context.LikeEvents.Add(likeEvent);
            await _context.SaveChangesAsync();
        }

        public async Task PublishCommentEventAsync(CommentEvent commentEvent)
        {
            _context.CommentEvents.Add(commentEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LikeEvent>> GetUnprocessedLikeEventsAsync(int batchSize = 100)
        {
            return await _context.LikeEvents
                .Where(e => !e.Processed)
                .OrderBy(e => e.CreatedAt)
                .Take(batchSize)
                .ToListAsync();
        }

        public async Task<List<CommentEvent>> GetUnprocessedCommentEventsAsync(int batchSize = 100)
        {
            return await _context.CommentEvents
                .Where(e => !e.Processed)
                .OrderBy(e => e.CreatedAt)
                .Take(batchSize)
                .ToListAsync();
        }

        public async Task MarkEventsAsProcessedAsync(IEnumerable<long> likeEventIds, IEnumerable<long> commentEventIds)
        {
            var now = DateTime.UtcNow;

            // Mark like events as processed
            await _context.LikeEvents
                .Where(e => likeEventIds.Contains(e.Id))
                .ExecuteUpdateAsync(e => e
                    .SetProperty(x => x.Processed, true)
                    .SetProperty(x => x.ProcessedAt, now));

            // Mark comment events as processed
            await _context.CommentEvents
                .Where(e => commentEventIds.Contains(e.Id))
                .ExecuteUpdateAsync(e => e
                    .SetProperty(x => x.Processed, true)
                    .SetProperty(x => x.ProcessedAt, now));
        }
    }
}
