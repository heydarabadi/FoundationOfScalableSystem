using System.Text.Json;
using System.Diagnostics;
using Confluent.Kafka;
using Hpt.Contracts;
using StackExchange.Redis;

namespace Hpt.BasketProcessor;

public sealed class Worker(ILogger<Worker> logger, IConfiguration configuration, IConnectionMultiplexer redis) : BackgroundService
{
    private const string SetItemScript = """
        local lastPartition = redis.call('HGET', KEYS[1], '_lastPartition')
        local lastOffset = tonumber(redis.call('HGET', KEYS[1], '_lastOffset') or '-1')
        local partition = ARGV[1]
        local offset = tonumber(ARGV[2])
        if lastPartition == partition and lastOffset >= offset then return 0 end
        local status = redis.call('HGET', KEYS[1], '_status')
        if status == 'checkout-pending' or status == 'checkout-complete' then return -1 end
        local quantity = tonumber(ARGV[4])
        if quantity == 0 then
          redis.call('HDEL', KEYS[1], ARGV[3])
        else
          redis.call('HSET', KEYS[1], ARGV[3], quantity)
        end
        redis.call('HSET', KEYS[1], '_lastPartition', partition, '_lastOffset', offset, '_status', 'open')
        redis.call('EXPIRE', KEYS[1], ARGV[5])
        return 1
        """;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrap = configuration["Kafka:BootstrapServers"] ?? "localhost:29092";
        var basketTtlSeconds = Math.Clamp(configuration.GetValue("Basket:TtlSeconds", 86400), 60, 604800);
        var maxPollBatchSize = Math.Clamp(configuration.GetValue("Basket:MaxPollBatchSize", 128), 1, 1024);
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrap,
            GroupId = configuration["Kafka:BasketConsumerGroup"] ?? "basket-projector-v1",
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
        consumer.Subscribe(configuration["Kafka:BasketTopic"] ?? Topics.BasketCommands);
        var database = redis.GetDatabase();
        var processedSinceCommit = 0;
        var lastCommit = DateTimeOffset.UtcNow;

        void CommitPendingOffsets(bool force = false)
        {
            if (processedSinceCommit == 0 || (!force && processedSinceCommit < 500 && DateTimeOffset.UtcNow - lastCommit < TimeSpan.FromMilliseconds(250)))
                return;

            consumer.Commit();
            WorkerMetrics.Committed.Add(processedSinceCommit);
            processedSinceCommit = 0;
            lastCommit = DateTimeOffset.UtcNow;
        }

        logger.LogInformation("Basket processor subscribed to {Topic}", configuration["Kafka:BasketTopic"] ?? Topics.BasketCommands);
        while (!stoppingToken.IsCancellationRequested)
        {
            var batch = new List<ConsumeResult<string, string>>(maxPollBatchSize);
            try
            {
                var first = consumer.Consume(TimeSpan.FromMilliseconds(100));
                WorkerMetrics.SetReady(consumer.Assignment.Count > 0);
                if (first is null)
                {
                    CommitPendingOffsets();
                    continue;
                }
                batch.Add(first);
                while (batch.Count < maxPollBatchSize && !stoppingToken.IsCancellationRequested)
                {
                    var next = consumer.Consume(TimeSpan.Zero);
                    if (next is null) break;
                    batch.Add(next);
                }

                var completed = new bool[batch.Count];
                var failures = new Exception?[batch.Count];
                var indexedResults = batch.Select((result, index) => (result, index));
                await Task.WhenAll(indexedResults
                    .GroupBy(item => item.result.Message.Key)
                    .Select(async basketGroup =>
                    {
                        foreach (var item in basketGroup)
                        {
                            try
                            {
                                await ProcessMessageAsync(item.result, producer, database, basketTtlSeconds, stoppingToken);
                                completed[item.index] = true;
                            }
                            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                            {
                                throw;
                            }
                            catch (Exception exception)
                            {
                                failures[item.index] = exception;
                                break;
                            }
                        }
                    }));

                var processedInBatch = 0;
                var encounteredFailure = false;
                foreach (var partitionGroup in batch.Select((result, index) => (result, index))
                             .GroupBy(item => item.result.TopicPartition))
                {
                    foreach (var item in partitionGroup.OrderBy(item => item.result.Offset.Value))
                    {
                        if (failures[item.index] is { } failure)
                        {
                            WorkerMetrics.Failed.Add(1);
                            logger.LogError(failure, "Basket command failed at {TopicPartitionOffset}; leaving this and later offsets uncommitted", item.result.TopicPartitionOffset);
                            try { consumer.Seek(item.result.TopicPartitionOffset); }
                            catch (KafkaException seekException) { logger.LogWarning(seekException, "Could not seek back to the failed basket command."); }
                            encounteredFailure = true;
                            break;
                        }

                        if (completed[item.index])
                        {
                            consumer.StoreOffset(item.result);
                            processedInBatch++;
                        }
                    }
                }

                processedSinceCommit += processedInBatch;
                WorkerMetrics.PollBatchSize.Record(batch.Count);
                CommitPendingOffsets();
                if (encounteredFailure)
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
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
                logger.LogError(exception, "Basket processor batch failed; offsets remain uncommitted");
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
            logger.LogWarning(exception, "Could not commit the final basket offsets during shutdown.");
        }
        consumer.Close();
    }

    private async Task ProcessMessageAsync(
        ConsumeResult<string, string> result,
        IProducer<string, string> producer,
        IDatabase database,
        int basketTtlSeconds,
        CancellationToken cancellationToken)
    {
        var processingStarted = Stopwatch.GetTimestamp();
        var command = JsonSerializer.Deserialize<BasketCommand>(result.Message.Value)
            ?? throw new InvalidDataException("Basket command payload was empty.");

        if (command.BasketId.ToString("N") != result.Message.Key)
            throw new InvalidDataException("Kafka key must match the basket ID.");

        if (command.Type == "item-set")
        {
            var redisKey = BasketKey(command.BasketId);
            var value = await database.ScriptEvaluateAsync(
                SetItemScript,
                [redisKey],
                [
                    result.Partition.Value,
                    result.Offset.Value,
                    command.ProductId?.ToString("D") ?? throw new InvalidDataException("Product ID is required."),
                    command.Quantity ?? throw new InvalidDataException("Quantity is required."),
                    basketTtlSeconds
                ]);

            if ((long)value == -1)
                logger.LogWarning("Ignored basket update after checkout for basket {BasketId}", command.BasketId);
        }
        else if (command.Type == "checkout-requested")
        {
            await HandleCheckoutAsync(command, result, producer, database, basketTtlSeconds, cancellationToken);
        }
        else
        {
            throw new InvalidDataException("Unsupported basket command type: " + command.Type);
        }

        WorkerMetrics.Processed.Add(1, new KeyValuePair<string, object?>("command", command.Type));
        WorkerMetrics.Duration.Record(Stopwatch.GetElapsedTime(processingStarted).TotalMilliseconds, new KeyValuePair<string, object?>("command", command.Type));
    }

    private async Task HandleCheckoutAsync(
        BasketCommand command,
        ConsumeResult<string, string> consumed,
        IProducer<string, string> producer,
        IDatabase database,
        int basketTtlSeconds,
        CancellationToken cancellationToken)
    {
        if (command.OrderId is null || command.OrderId == Guid.Empty)
            throw new InvalidDataException("Checkout command requires a non-empty order ID.");

        var key = BasketKey(command.BasketId);
        var entries = await database.HashGetAllAsync(key);
        var status = (string?)await database.HashGetAsync(key, "_status") ?? "open";
        var existingOrderId = (string?)await database.HashGetAsync(key, "_checkoutOrderId");
        var isAlreadyCheckedOut = status is "checkout-pending" or "checkout-complete";
        var rejectionReason = isAlreadyCheckedOut && existingOrderId != command.OrderId.Value.ToString("D")
            ? "Basket has already been checked out."
            : null;

        var items = entries
            .Where(entry => !entry.Name.ToString().StartsWith('_'))
            .Select(entry => new BasketLine(Guid.Parse(entry.Name.ToString()), (int)entry.Value))
            .ToArray();
        if (items.Length == 0 && rejectionReason is null)
            rejectionReason = "Basket is empty.";

        var order = new OrderSubmitted(
            command.EventId,
            command.OrderId.Value,
            command.BasketId,
            command.OccurredAt,
            rejectionReason is null ? items : [],
            rejectionReason);

        await producer.ProduceAsync(
            configuration["Kafka:OrdersTopic"] ?? Topics.OrdersSubmitted,
            new Message<string, string>
            {
                Key = command.OrderId.Value.ToString("N"),
                Value = JsonSerializer.Serialize(order)
            }, cancellationToken);

        if (rejectionReason is null)
        {
            await database.HashSetAsync(key,
            [
                new HashEntry("_status", "checkout-pending"),
                new HashEntry("_checkoutOrderId", command.OrderId.Value.ToString("D")),
                new HashEntry("_lastPartition", consumed.Partition.Value),
                new HashEntry("_lastOffset", consumed.Offset.Value)
            ]);
            await database.KeyExpireAsync(key, TimeSpan.FromSeconds(basketTtlSeconds));
        }
    }

    private static RedisKey BasketKey(Guid basketId) => $"basket:{{{basketId:N}}}:state";
}
