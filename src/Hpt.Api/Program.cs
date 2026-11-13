using System.Text.Json;
using System.Globalization;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Threading.RateLimiting;
using Confluent.Kafka;
using Hpt.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.RateLimiting;
using Npgsql;
using StackExchange.Redis;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

var meter = new Meter("Hpt.Api", "1.0.0");
var basketCommands = meter.CreateCounter<long>("hpt.basket.commands", "{command}", "Basket commands accepted by the API");
var checkoutCommands = meter.CreateCounter<long>("hpt.checkout.commands", "{command}", "Checkout commands accepted by the API");
var kafkaPublishFailures = meter.CreateCounter<long>("hpt.kafka.publish.failures", "{message}", "Kafka record delivery failures");
var batchRejections = meter.CreateCounter<long>("hpt.basket.batch.rejections", "{request}", "Batch requests rejected by per-pod concurrency protection");
var kafkaPublishDuration = meter.CreateHistogram<double>("hpt.kafka.publish.duration", "ms", "Kafka producer publish duration");
var maxConcurrentBatches = Math.Clamp(builder.Configuration.GetValue("Ingestion:MaxConcurrentBatchesPerPod", 8), 1, 1024);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddConcurrencyLimiter("batch-publish", limiterOptions =>
    {
        limiterOptions.PermitLimit = maxConcurrentBatches;
        limiterOptions.QueueLimit = 0;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.OnRejected = (context, _) =>
    {
        batchRejections.Add(1);
        context.HttpContext.Response.Headers["Retry-After"] = "1";
        return ValueTask.CompletedTask;
    };
});
builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics
    .AddMeter("Hpt.Api")
    .AddAspNetCoreInstrumentation()
    .AddPrometheusExporter());

var kafkaBootstrap = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:29092";
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
var inventoryConnection = builder.Configuration.GetConnectionString("Inventory")
    ?? "Host=localhost;Port=5432;Database=hpt;Username=hpt;Password=hpt-dev-only";

builder.Services.AddSingleton<IProducer<string, string>>(_ =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = kafkaBootstrap,
        Acks = Acks.All,
        EnableIdempotence = true,
        CompressionType = CompressionType.Zstd,
        LingerMs = 5,
        BatchSize = 131072,
        QueueBufferingMaxMessages = 5000,
        QueueBufferingMaxKbytes = 65536,
        MessageTimeoutMs = 120000
    };
    return new ProducerBuilder<string, string>(config).Build();
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(inventoryConnection));
builder.Services.AddSingleton<Hpt.Api.CpuLoadSampler>();
builder.Services.AddHostedService(services => services.GetRequiredService<Hpt.Api.CpuLoadSampler>());
builder.Services.AddMemoryCache();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(builder.Configuration["Web:Origin"] ?? "http://localhost:5173")
        .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
var cpuLoadSampler = app.Services.GetRequiredService<Hpt.Api.CpuLoadSampler>();
meter.CreateObservableGauge(
    "hpt.api.cpu.utilization",
    () => cpuLoadSampler.Utilization,
    unit: "1",
    description: "Container CPU utilization fraction reported to the HTTP load balancer.");
// Emit a zero-valued series after the meter provider starts so Prometheus can detect the first failure.
app.Lifetime.ApplicationStarted.Register(() =>
{
    kafkaPublishFailures.Add(0, new KeyValuePair<string, object?>("mode", "batch"));
    batchRejections.Add(0);
});
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var utilization = cpuLoadSampler.Utilization.ToString("0.000", CultureInfo.InvariantCulture);
        context.Response.Headers["endpoint-load-metrics"] = $"JSON {{\"cpu_utilization\":{utilization}}}";
        return Task.CompletedTask;
    });
    await next();
});
app.UseRouting();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseCors();
app.UseRateLimiter();

app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/ready", async (IConnectionMultiplexer redis, NpgsqlDataSource dataSource) =>
{
    try
    {
        await redis.GetDatabase().PingAsync();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync();
        return Results.Ok(new { status = "ready" });
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapGet("/api/products", async (IMemoryCache cache, NpgsqlDataSource dataSource, CancellationToken cancellationToken) =>
{
    const string cacheKey = "catalog:products:v1";
    if (cache.TryGetValue(cacheKey, out List<ProductResponse>? cached))
        return Results.Ok(cached);

    const string sql = "SELECT product_id, name, unit_price, available FROM inventory_products ORDER BY name";
    await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
    await using var command = new NpgsqlCommand(sql, connection);
    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    var products = new List<ProductResponse>();
    while (await reader.ReadAsync(cancellationToken))
    {
        products.Add(new ProductResponse(reader.GetGuid(0), reader.GetString(1), reader.GetDecimal(2), reader.GetInt32(3)));
    }
    cache.Set(cacheKey, products, TimeSpan.FromSeconds(30));
    return Results.Ok(products);
});

app.MapPost("/api/baskets", () =>
{
    var id = Guid.NewGuid();
    return Results.Created($"/api/baskets/{id}", new { id });
});

app.MapGet("/api/baskets/{basketId:guid}", async (Guid basketId, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var entries = await db.HashGetAllAsync(BasketKey(basketId));
    var items = entries
        .Where(entry => !entry.Name.ToString().StartsWith('_'))
        .Select(entry => new { productId = Guid.Parse(entry.Name.ToString()), quantity = (int)entry.Value })
        .ToArray();
    var state = await db.HashGetAsync(BasketKey(basketId), "_status");
    return Results.Ok(new { id = basketId, status = (string?)state ?? "open", items });
});

app.MapPut("/api/baskets/{basketId:guid}/items/{productId:guid}", async (
    Guid basketId,
    Guid productId,
    SetQuantityRequest request,
    IProducer<string, string> producer,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    if (request.Quantity is < 0 or > 1000)
        return Results.Problem("Quantity must be between 0 and 1000.", statusCode: 400);

    var command = new BasketCommand(Guid.NewGuid(), basketId, "item-set", DateTimeOffset.UtcNow, productId, request.Quantity);
    var publishStarted = Stopwatch.GetTimestamp();
    var delivery = await producer.ProduceAsync(
        configuration["Kafka:BasketTopic"] ?? Topics.BasketCommands,
        new Message<string, string>
        {
            Key = basketId.ToString("N"),
            Value = JsonSerializer.Serialize(command)
        }, cancellationToken);
    kafkaPublishDuration.Record(Stopwatch.GetElapsedTime(publishStarted).TotalMilliseconds, new KeyValuePair<string, object?>("topic", configuration["Kafka:BasketTopic"] ?? Topics.BasketCommands));
    basketCommands.Add(1, new KeyValuePair<string, object?>("command", "item-set"));

    return Results.Accepted($"/api/baskets/{basketId}", new
    {
        eventId = command.EventId,
        partition = delivery.Partition.Value,
        offset = delivery.Offset.Value,
        message = "Basket update accepted; the read model is eventually consistent."
    });
});

app.MapPost("/api/baskets/commands/batch", async (
    BatchSetQuantityRequest request,
    IProducer<string, string> producer,
    IConfiguration configuration) =>
{
    const int maxBatchSize = 500;
    if (request.Items is null || request.Items.Count is < 1 or > maxBatchSize)
        return Results.Problem($"Batch must contain between 1 and {maxBatchSize} commands.", statusCode: 400);

    if (request.Items.Any(item => item.BasketId == Guid.Empty || item.ProductId == Guid.Empty || item.Quantity is < 0 or > 1000))
        return Results.Problem("Each command needs valid basket/product IDs and quantity from 0 to 1000.", statusCode: 400);

    if (request.Items.GroupBy(item => (item.BasketId, item.ProductId)).Any(group => group.Count() > 1))
        return Results.Problem("A batch cannot contain duplicate basket/product pairs.", statusCode: 400);

    var topic = configuration["Kafka:BasketTopic"] ?? Topics.BasketCommands;
    var started = Stopwatch.GetTimestamp();
    var delivered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var remaining = request.Items.Count;
    var failed = 0;
    Exception? firstFailure = null;

    void CompletePublish(Exception? failure)
    {
        if (failure is not null)
        {
            Interlocked.Increment(ref failed);
            Interlocked.CompareExchange(ref firstFailure, failure, null);
            kafkaPublishFailures.Add(1, new KeyValuePair<string, object?>("mode", "batch"));
        }
        else
        {
            basketCommands.Add(1, new KeyValuePair<string, object?>("command", "item-set-batch"));
        }

        if (Interlocked.Decrement(ref remaining) == 0)
            delivered.TrySetResult();
    }

    foreach (var item in request.Items)
    {
        try
        {
            var eventId = item.EventId == Guid.Empty ? Guid.NewGuid() : item.EventId;
            var command = new BasketCommand(eventId, item.BasketId, "item-set", DateTimeOffset.UtcNow, item.ProductId, item.Quantity);
            producer.Produce(topic, new Message<string, string>
            {
                Key = item.BasketId.ToString("N"),
                Value = JsonSerializer.Serialize(command)
            }, report => CompletePublish(report.Error.IsError ? new KafkaException(report.Error) : null));
        }
        catch (Exception exception)
        {
            CompletePublish(exception);
        }
    }

    try
    {
        // Once records are enqueued they cannot be canceled reliably. Keep the admission permit
        // until every delivery callback completes so client disconnects cannot bypass backpressure.
        await delivered.Task;
    }
    finally
    {
        kafkaPublishDuration.Record(Stopwatch.GetElapsedTime(started).TotalMilliseconds,
            new KeyValuePair<string, object?>("topic", topic),
            new KeyValuePair<string, object?>("mode", "batch"));
    }

    if (failed > 0)
    {
        return Results.Problem(
            title: "Kafka did not acknowledge every command in the batch.",
            detail: $"{failed} delivery reports failed. Some other commands may already be persisted; retry only with the same event IDs and account for the consumer deduplication limitation.",
            statusCode: StatusCodes.Status503ServiceUnavailable,
            extensions: new Dictionary<string, object?>
            {
                ["acknowledgedByKafka"] = request.Items.Count - failed,
                ["failedDeliveries"] = failed,
                ["failure"] = firstFailure?.GetType().Name
            });
    }

    return Results.Accepted(value: new
    {
        accepted = request.Items.Count,
        message = "All basket commands were acknowledged by Kafka."
    });
}).RequireRateLimiting("batch-publish");

app.MapPost("/api/baskets/{basketId:guid}/checkout", async (
    Guid basketId,
    CheckoutRequest request,
    IProducer<string, string> producer,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    if (request.IdempotencyKey == Guid.Empty)
        return Results.Problem("A non-empty idempotencyKey is required.", statusCode: 400);

    var command = new BasketCommand(
        request.IdempotencyKey,
        basketId,
        "checkout-requested",
        DateTimeOffset.UtcNow,
        OrderId: request.IdempotencyKey);

    var publishStarted = Stopwatch.GetTimestamp();
    await producer.ProduceAsync(
        configuration["Kafka:BasketTopic"] ?? Topics.BasketCommands,
        new Message<string, string>
        {
            Key = basketId.ToString("N"),
            Value = JsonSerializer.Serialize(command)
        }, cancellationToken);
    kafkaPublishDuration.Record(Stopwatch.GetElapsedTime(publishStarted).TotalMilliseconds, new KeyValuePair<string, object?>("topic", configuration["Kafka:BasketTopic"] ?? Topics.BasketCommands));
    checkoutCommands.Add(1, new KeyValuePair<string, object?>("command", "checkout"));

    return Results.Accepted($"/api/orders/{request.IdempotencyKey}", new
    {
        orderId = request.IdempotencyKey,
        status = "pending",
        message = "Checkout accepted; inventory is being reserved asynchronously."
    });
});

app.MapGet("/api/orders/{orderId:guid}", async (Guid orderId, NpgsqlDataSource dataSource, CancellationToken cancellationToken) =>
{
    const string orderSql = "SELECT status, reason, created_at FROM orders WHERE order_id = @id";
    await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
    await using var command = new NpgsqlCommand(orderSql, connection);
    command.Parameters.AddWithValue("id", orderId);
    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    if (!await reader.ReadAsync(cancellationToken)) return Results.NotFound();
    var status = reader.GetString(0);
    var reason = reader.IsDBNull(1) ? null : reader.GetString(1);
    var createdAt = reader.GetFieldValue<DateTimeOffset>(2);
    await reader.CloseAsync();

    await using var itemsCommand = new NpgsqlCommand(
        "SELECT product_id, quantity, unit_price FROM order_items WHERE order_id = @id ORDER BY product_id", connection);
    itemsCommand.Parameters.AddWithValue("id", orderId);
    await using var itemsReader = await itemsCommand.ExecuteReaderAsync(cancellationToken);
    var items = new List<object>();
    while (await itemsReader.ReadAsync(cancellationToken))
        items.Add(new { productId = itemsReader.GetGuid(0), quantity = itemsReader.GetInt32(1), unitPrice = itemsReader.GetDecimal(2) });

    return Results.Ok(new { id = orderId, status, reason, createdAt, items });
});

app.Run();

static RedisKey BasketKey(Guid basketId) => $"basket:{{{basketId:N}}}:state";

record ProductResponse(Guid Id, string Name, decimal Price, int Available);
