using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetFora.Domain.Entities;
using NetFora.Infrastructure.Data;
using NetFora.Infrastructure.Interfaces;

namespace NetFora.EventProcessor.Workers
{
    public class CommentEventProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CommentEventProcessor> _logger;

        public CommentEventProcessor(IServiceProvider serviceProvider, ILogger<CommentEventProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Comment Event Processor started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Get unprocessed events
                    var events = await eventService.GetUnprocessedCommentEventsAsync(100);

                    if (events.Any())
                    {
                        _logger.LogInformation("Processing {Count} comment events", events.Count);

                        // Group by PostId for batch processing
                        var eventsByPost = events.GroupBy(e => e.PostId);

                        foreach (var postEvents in eventsByPost)
                        {
                            await UpdateCommentCount(context, postEvents.Key);
                        }

                        // Mark events as processed
                        var eventIds = events.Select(e => e.Id);
                        await eventService.MarkEventsAsProcessedAsync(Enumerable.Empty<long>(), eventIds);

                        _logger.LogInformation("Processed {Count} comment events", events.Count);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing comment events");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        private async Task UpdateCommentCount(ApplicationDbContext context, int postId)
        {
            var currentCommentCount = await context.Comments.CountAsync(c => c.PostId == postId);

            var stats = await context.PostStats.FirstOrDefaultAsync(s => s.PostId == postId);
            if (stats == null)
            {
                stats = new PostStats { PostId = postId };
                context.PostStats.Add(stats);
            }

            stats.CommentCount = currentCommentCount;
            stats.LastUpdated = DateTime.UtcNow;
            stats.Version++;

            await context.SaveChangesAsync();
            _logger.LogDebug("Updated comment count for post {PostId} to {Count}", postId, currentCommentCount);
        }
    }
}
