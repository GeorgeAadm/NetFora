using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetFora.Domain.Events;
using NetFora.Infrastructure.Data;
using NetFora.Infrastructure.Interfaces;

namespace NetFora.Infrastructure.Services
{
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

            if (likeEventIds.Any())
            {
                // Mark like events as processed
                await _context.LikeEvents
                    .Where(e => likeEventIds.Contains(e.Id))
                    .ExecuteUpdateAsync(e => e
                        .SetProperty(x => x.Processed, true)
                        .SetProperty(x => x.ProcessedAt, now));
            }

            if (commentEventIds.Any())
            {
                // Mark comment events as processed
                await _context.CommentEvents
                    .Where(e => commentEventIds.Contains(e.Id))
                    .ExecuteUpdateAsync(e => e
                        .SetProperty(x => x.Processed, true)
                        .SetProperty(x => x.ProcessedAt, now));
            }
        }
    }
}
