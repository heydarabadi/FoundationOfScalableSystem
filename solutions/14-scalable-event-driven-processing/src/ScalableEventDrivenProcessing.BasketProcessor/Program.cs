using StackExchange.Redis;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:9464");
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddHostedService<ScalableEventDrivenProcessing.BasketProcessor.Worker>();
builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics
    .AddMeter("ScalableEventDrivenProcessing.BasketProcessor")
    .AddPrometheusExporter());

var app = builder.Build();
app.MapPrometheusScrapingEndpoint();
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/ready", async (IConnectionMultiplexer redis) =>
{
    if (!ScalableEventDrivenProcessing.BasketProcessor.WorkerMetrics.IsReady)
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

    try
    {
        await redis.GetDatabase().PingAsync();
        return Results.Ok(new { status = "ready" });
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});
await app.RunAsync();
