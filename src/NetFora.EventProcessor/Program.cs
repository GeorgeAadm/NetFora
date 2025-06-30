using Microsoft.EntityFrameworkCore;
using NetFora.Infrastructure.Data;
using NetFora.Infrastructure.Services;
using NetFora.EventProcessor.Workers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NetFora.Infrastructure.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // Optimize for background processing
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});


builder.Services.AddScoped<IEventService, EventService>();


builder.Services.AddHostedService<LikeEventProcessor>();
builder.Services.AddHostedService<CommentEventProcessor>();

var host = builder.Build();

host.Run();