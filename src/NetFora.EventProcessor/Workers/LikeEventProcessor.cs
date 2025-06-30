using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NetFora.Domain.Events;
using NetFora.Infrastructure.Data;
using NetFora.Domain.Entities;
using NetFora.Infrastructure.Interfaces;

namespace NetFora.EventProcessor.Workers
{
    public class LikeEventProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LikeEventProcessor> _logger;
        private readonly int _batchSize;
        private readonly TimeSpan _minDelay;
        private readonly TimeSpan _maxDelay;

        public LikeEventProcessor(IServiceProvider serviceProvider, ILogger<LikeEventProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _batchSize = 100;                       // Process max 100 records
            _minDelay = TimeSpan.FromSeconds(1);    // Minimum wait time
            _maxDelay = TimeSpan.FromSeconds(30);   // Maximum when idle
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Like Event Processor started - Batch size: {BatchSize}", _batchSize);

            var currentDelay = _minDelay;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var eventsProcessed = await ProcessBatch();

                    // Adaptive > based on activity
                    if (eventsProcessed > 0)
                    {
                        // Reset fast polling > when busy
                        currentDelay = _minDelay; 
                        _logger.LogDebug("Processed {Count} events, continuing with fast polling", eventsProcessed);
                    }
                    else
                    {
                        // Exponential backoff > when idle
                        currentDelay = TimeSpan.FromSeconds(
                            Math.Min(_maxDelay.TotalSeconds, currentDelay.TotalSeconds * 1.5));
                        _logger.LogTrace("No events to process, backing off to {Delay}s", currentDelay.TotalSeconds);
                    }

                    await Task.Delay(currentDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in event processing loop");

                    // Wait longer on errors / Reset delay after recovery
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    currentDelay = _minDelay;
                }
            }

            _logger.LogInformation("Like Event Processor stopped");
        }

        private async Task<int> ProcessBatch()
        {
            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get unprocessed events
            var events = await eventService.GetUnprocessedLikeEventsAsync(_batchSize);

            if (!events.Any())
                return 0;

            _logger.LogInformation("Processing batch of {Count} like events", events.Count);

            try
            {
                // Group by PostId for efficient processing
                var eventsByPost = events.GroupBy(e => e.PostId);

                foreach (var postEvents in eventsByPost)
                {
                    await ProcessPostLikeEvents(context, postEvents.Key, postEvents.ToList());
                }

                // Mark all events as processed
                var eventIds = events.Select(e => e.Id);
                await eventService.MarkEventsAsProcessedAsync(eventIds, Enumerable.Empty<long>());

                _logger.LogInformation("Successfully processed {Count} like events", events.Count);
                return events.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process event batch");
                throw; // Will be caught by outer exception handler
            }
        }

        private async Task ProcessPostLikeEvents(ApplicationDbContext context, int postId, List<LikeEvent> events)
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Track changes for this post
                var likesToAdd = new List<Like>();
                var userIdsToRemove = new List<string>();


                foreach (var evt in events.OrderBy(e => e.CreatedAt))
                {
                    if (evt.Action == "LIKE")
                    {
                        // Remove from remove list if user changed their mind
                        userIdsToRemove.Remove(evt.UserId);

                        // Add to add list if not already there
                        if (!likesToAdd.Any(l => l.UserId == evt.UserId))
                        {
                            likesToAdd.Add(new Like
                            {
                                PostId = postId,
                                UserId = evt.UserId,
                                CreatedAt = evt.CreatedAt
                            });
                        }
                    }
                    else if (evt.Action == "UNLIKE")
                    {
                        // Remove from add list if user changed their mind
                        likesToAdd.RemoveAll(l => l.UserId == evt.UserId);

                        // Add to remove list
                        if (!userIdsToRemove.Contains(evt.UserId))
                        {
                            userIdsToRemove.Add(evt.UserId);
                        }
                    }
                }

                // Apply changes to database

                // Remove unlikes
                if (userIdsToRemove.Any())
                {
                    var likesToRemove = await context.Likes
                        .Where(l => l.PostId == postId && userIdsToRemove.Contains(l.UserId))
                        .ToListAsync();

                    context.Likes.RemoveRange(likesToRemove);
                }

                // Add new likes (check for duplicates)
                foreach (var newLike in likesToAdd)
                {
                    var exists = await context.Likes
                        .AnyAsync(l => l.PostId == postId && l.UserId == newLike.UserId);

                    if (!exists)
                    {
                        context.Likes.Add(newLike);
                    }
                }

                await context.SaveChangesAsync();

                // Update denormalized counter > PostStats
                await UpdatePostStats(context, postId);

                await transaction.CommitAsync();

                _logger.LogDebug("Processed {EventCount} events for post {PostId}: +{Added} -{Removed}",
                    events.Count, postId, likesToAdd.Count, userIdsToRemove.Count);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing like events for post {PostId}", postId);
                throw;
            }
        }

        private async Task UpdatePostStats(ApplicationDbContext context, int postId)
        {
            var currentLikeCount = await context.Likes.CountAsync(l => l.PostId == postId);

            var stats = await context.PostStats.FirstOrDefaultAsync(s => s.PostId == postId);
            if (stats == null)
            {
                stats = new PostStats { PostId = postId };
                context.PostStats.Add(stats);
            }

            stats.LikeCount = currentLikeCount;
            stats.LastUpdated = DateTime.UtcNow;
            stats.Version++;

            await context.SaveChangesAsync();
        }
    }
}
