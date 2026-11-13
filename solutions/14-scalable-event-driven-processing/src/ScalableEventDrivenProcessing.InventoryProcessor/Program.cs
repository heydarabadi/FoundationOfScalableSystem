using Npgsql;
using StackExchange.Redis;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:9464");
var connectionString = builder.Configuration.GetConnectionString("Inventory")
    ?? "Host=localhost;Port=5432;Database=hpt;Username=hpt;Password=scalable-event-driven-processing-dev-only";
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddHostedService<ScalableEventDrivenProcessing.InventoryProcessor.Worker>();
builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics
    .AddMeter("ScalableEventDrivenProcessing.InventoryProcessor")
    .AddPrometheusExporter());

var app = builder.Build();
app.MapPrometheusScrapingEndpoint();
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/ready", async (
    IConnectionMultiplexer redis,
    NpgsqlDataSource dataSource,
    CancellationToken cancellationToken) =>
{
    if (!ScalableEventDrivenProcessing.InventoryProcessor.WorkerMetrics.IsReady)
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

    try
    {
        await redis.GetDatabase().PingAsync();
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync(cancellationToken);
        return Results.Ok(new { status = "ready" });
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});
await app.RunAsync();
