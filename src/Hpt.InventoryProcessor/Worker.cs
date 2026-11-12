using System.Text.Json;
using System.Diagnostics;
using Confluent.Kafka;
using Hpt.Contracts;
using Npgsql;
using StackExchange.Redis;

namespace Hpt.InventoryProcessor;

public sealed class Worker(
    ILogger<Worker> logger,
    IConfiguration configuration,
    NpgsqlDataSource dataSource,
    IConnectionMultiplexer redis) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrap = configuration["Kafka:BootstrapServers"] ?? "localhost:29092";
        var basketTtlSeconds = Math.Clamp(configuration.GetValue("Basket:TtlSeconds", 86400), 60, 604800);
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrap,
            GroupId = configuration["Kafka:InventoryConsumerGroup"] ?? "inventory-reservation-v1",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            IsolationLevel = IsolationLevel.ReadCommitted
        };
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrap,
            Acks = Acks.All,
            EnableIdempotence = true,
            CompressionType = CompressionType.Zstd,
            LingerMs = 5,
            BatchSize = 131072
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
        consumer.Subscribe(configuration["Kafka:OrdersTopic"] ?? Topics.OrdersSubmitted);
        var redisDatabase = redis.GetDatabase();
        var processedSinceCommit = 0;
        var lastCommit = DateTimeOffset.UtcNow;

        void CommitPendingOffsets(bool force = false)
        {
            if (processedSinceCommit == 0 || (!force && processedSinceCommit < 250 && DateTimeOffset.UtcNow - lastCommit < TimeSpan.FromMilliseconds(250)))
                return;

            consumer.Commit();
            WorkerMetrics.Committed.Add(processedSinceCommit);
            processedSinceCommit = 0;
            lastCommit = DateTimeOffset.UtcNow;
        }

        logger.LogInformation("Inventory processor subscribed to {Topic}", configuration["Kafka:OrdersTopic"] ?? Topics.OrdersSubmitted);
        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? consumed = null;
            var processingStarted = Stopwatch.GetTimestamp();
            try
            {
                consumed = consumer.Consume(TimeSpan.FromMilliseconds(100));
                WorkerMetrics.SetReady(consumer.Assignment.Count > 0);
                if (consumed is null)
                {
                    CommitPendingOffsets();
                    continue;
                }
                var order = JsonSerializer.Deserialize<OrderSubmitted>(consumed.Message.Value)
                    ?? throw new InvalidDataException("Order event payload was empty.");
                var result = await ReserveAsync(order, stoppingToken);

                await producer.ProduceAsync(
                    configuration["Kafka:InventoryResultsTopic"] ?? Topics.InventoryResults,
                    new Message<string, string>
                    {
                        Key = order.OrderId.ToString("N"),
                        Value = JsonSerializer.Serialize(result)
                    }, stoppingToken);

                var basketStatus = result.Status == "inventory-reserved" ? "checkout-complete" : "checkout-rejected";
                await redisDatabase.HashSetAsync(
                    $"basket:{{{order.BasketId:N}}}:state",
                    [
                        new HashEntry("_status", basketStatus),
                        new HashEntry("_inventoryStatus", result.Status)
                    ]);
                await redisDatabase.KeyExpireAsync(
                    $"basket:{{{order.BasketId:N}}}:state",
                    TimeSpan.FromSeconds(basketTtlSeconds));

                WorkerMetrics.Processed.Add(1, new KeyValuePair<string, object?>("status", result.Status));
                WorkerMetrics.Duration.Record(Stopwatch.GetElapsedTime(processingStarted).TotalMilliseconds, new KeyValuePair<string, object?>("status", result.Status));
                consumer.StoreOffset(consumed);
                processedSinceCommit++;
                CommitPendingOffsets();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (ConsumeException exception)
            {
                WorkerMetrics.SetReady(false);
                logger.LogError(exception, "Kafka consume failed: {Reason}", exception.Error.Reason);
            }
            catch (Exception exception)
            {
                WorkerMetrics.Failed.Add(1);
                logger.LogError(exception, "Inventory event failed at {TopicPartitionOffset}; leaving offset uncommitted", consumed?.TopicPartitionOffset);
                if (consumed is not null)
                {
                    try { consumer.Seek(consumed.TopicPartitionOffset); }
                    catch (KafkaException seekException) { logger.LogWarning(seekException, "Could not seek back to the failed order event."); }
                }
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        try
        {
            WorkerMetrics.SetReady(false);
            CommitPendingOffsets(force: true);
        }
        catch (KafkaException exception)
        {
            logger.LogWarning(exception, "Could not commit the final inventory offsets during shutdown.");
        }
        consumer.Close();
    }

    private async Task<InventoryResult> ReserveAsync(OrderSubmitted order, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var eventLock = new NpgsqlCommand(
            "SELECT pg_advisory_xact_lock(hashtextextended(CAST(@eventId AS text), 0))",
            connection,
            transaction))
        {
            eventLock.Parameters.AddWithValue("eventId", order.EventId);
            await eventLock.ExecuteNonQueryAsync(cancellationToken);
        }

        const string duplicateSql = """
            SELECT o.status, o.reason
            FROM processed_events p
            JOIN orders o ON o.order_id = p.order_id
            WHERE p.event_id = @eventId
            """;
        await using (var duplicate = new NpgsqlCommand(duplicateSql, connection, transaction))
        {
            duplicate.Parameters.AddWithValue("eventId", order.EventId);
            await using var reader = await duplicate.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var existing = new InventoryResult(
                    order.EventId,
                    order.OrderId,
                    reader.GetString(0),
                    reader.IsDBNull(1) ? null : reader.GetString(1),
                    DateTimeOffset.UtcNow);
                await reader.CloseAsync();
                await transaction.CommitAsync(cancellationToken);
                return existing;
            }
        }

        var rejection = order.RejectionReason;
        if (order.Items.Count == 0 && rejection is null)
            rejection = "Basket is empty.";

        var reservedLines = new List<(BasketLine Line, decimal UnitPrice)>();
        if (rejection is null)
        {
            foreach (var line in order.Items.OrderBy(item => item.ProductId))
            {
                if (line.Quantity <= 0)
                {
                    rejection = "Order contains a non-positive quantity.";
                    break;
                }

                const string reserveSql = """
                    UPDATE inventory_products
                    SET available = available - @quantity,
                        reserved = reserved + @quantity
                    WHERE product_id = @productId
                      AND available >= @quantity
                    RETURNING unit_price
                    """;
                await using var reserve = new NpgsqlCommand(reserveSql, connection, transaction);
                reserve.Parameters.AddWithValue("productId", line.ProductId);
                reserve.Parameters.AddWithValue("quantity", line.Quantity);
                var price = await reserve.ExecuteScalarAsync(cancellationToken);
                if (price is null)
                {
                    rejection = $"Insufficient stock for product {line.ProductId}.";
                    break;
                }
                reservedLines.Add((line, (decimal)price));
            }
        }

        if (rejection is not null)
        {
            await transaction.RollbackAsync(cancellationToken);
            await using var rejectedTransaction = await connection.BeginTransactionAsync(cancellationToken);
            await InsertOrderAsync(connection, rejectedTransaction, order, "rejected", rejection, cancellationToken);
            await InsertProcessedEventAsync(connection, rejectedTransaction, order, cancellationToken);
            await rejectedTransaction.CommitAsync(cancellationToken);
            return new InventoryResult(order.EventId, order.OrderId, "rejected", rejection, DateTimeOffset.UtcNow);
        }

        await InsertOrderAsync(connection, transaction, order, "inventory-reserved", null, cancellationToken);
        foreach (var reserved in reservedLines)
        {
            const string itemSql = """
                INSERT INTO order_items(order_id, product_id, quantity, unit_price)
                VALUES (@orderId, @productId, @quantity, @unitPrice)
                ON CONFLICT (order_id, product_id) DO NOTHING
                """;
            await using var item = new NpgsqlCommand(itemSql, connection, transaction);
            item.Parameters.AddWithValue("orderId", order.OrderId);
            item.Parameters.AddWithValue("productId", reserved.Line.ProductId);
            item.Parameters.AddWithValue("quantity", reserved.Line.Quantity);
            item.Parameters.AddWithValue("unitPrice", reserved.UnitPrice);
            await item.ExecuteNonQueryAsync(cancellationToken);
        }
        await InsertProcessedEventAsync(connection, transaction, order, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new InventoryResult(order.EventId, order.OrderId, "inventory-reserved", null, DateTimeOffset.UtcNow);
    }

    private static async Task InsertOrderAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        OrderSubmitted order,
        string status,
        string? reason,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO orders(order_id, basket_id, status, reason, created_at)
            VALUES (@orderId, @basketId, @status, @reason, @createdAt)
            ON CONFLICT (order_id) DO UPDATE
            SET status = EXCLUDED.status, reason = EXCLUDED.reason
            """;
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("orderId", order.OrderId);
        command.Parameters.AddWithValue("basketId", order.BasketId);
        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue("reason", (object?)reason ?? DBNull.Value);
        command.Parameters.AddWithValue("createdAt", order.CreatedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertProcessedEventAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        OrderSubmitted order,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO processed_events(event_id, order_id, processed_at)
            VALUES (@eventId, @orderId, @processedAt)
            ON CONFLICT (event_id) DO NOTHING
            """;
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("eventId", order.EventId);
        command.Parameters.AddWithValue("orderId", order.OrderId);
        command.Parameters.AddWithValue("processedAt", DateTimeOffset.UtcNow);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
