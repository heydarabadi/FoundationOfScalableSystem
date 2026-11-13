# 07. Asynchronous Messaging

![Chapter learning and experiment diagram](07-asynchronous-messaging-overview.svg)

> Chapter guide in the *Foundations of Scalable Systems* learning track. This repository guide paraphrases topics and proposes an independent practical exercise; it does not reproduce the book.

## Goal

Turn this chapter's design question into a small, repeatable experiment. The result is a comparison with stated assumptions, not a universal technology ranking.

## Chapter focus

Persistence; pub/sub; queues; replication; competing consumers; delivery safety; poison messages; idempotency.

## Chapter synthesis

**Source reading: printed pages 127–145.** Original learning summary of the chapter's ideas; consult the book for full treatment.

Asynchronous messaging lets a producer hand work to an intermediary and continue without waiting for the consumer's business operation to finish. A broker commonly accepts messages into named queues; a producer can wait for a broker confirmation, while consumers either poll (pull) or register for delivery (push). A queue usually gives each message to one competing consumer. A topic supports publish/subscribe, where each subscriber has its own interest and receives a copy of matching events. This decoupling smooths bursts, lets producers and consumers scale separately, and permits new subscribers to be added without changing publishers. It also moves complexity into backlog control, ordering, failure recovery, and message lifecycle.

The chapter uses RabbitMQ to make these concepts concrete. Producers publish to exchanges rather than directly to queues. Bindings route by a direct key, a topic pattern, or fanout broadcast. Connections are comparatively heavyweight and should be long-lived; channels are lighter logical streams multiplexed over a connection, but are not thread-safe. Reuse long-lived channels, assigning exclusive channel ownership per thread or using a bounded pool where appropriate. Push delivery avoids wasteful polling; manual acknowledgements allow the broker to redeliver work that was not acknowledged. Queue concurrency, consumer prefetch, and enough independent queues affect throughput; a single hot queue/thread can limit broker-side parallelism. Growing backlogs consume memory and can force producer throttling, so queue depth and drain rate need operational limits.

Safety and performance are a deliberate trade-off. In-memory queues are fast but can lose messages on broker failure. Durable queues and persistent messages, publisher confirms, and consumer acknowledgements reduce loss windows but add disk, network, and latency costs. Replication can make a queue available after a broker failure, but a leader still handles active traffic; replication is not automatically throughput load balancing. Quorum replication requires agreement by a majority and costs additional writes and resources. Select guarantees from business requirements rather than enabling every reliability option indiscriminately.

Retries can create duplicates: the broker may have accepted a publish even if the confirmation was lost, or a consumer may finish its side effect and fail before acknowledging. Therefore, “exactly once” must be scoped. Use a stable message/idempotency key, a durable deduplication record or naturally idempotent update, and an acknowledgement only after the intended effect commits. A poison message that always fails must not loop forever: cap deliveries, move it to a dead-letter queue, alert, preserve enough context to diagnose it, and define a safe replay process. These controls make at-least-once delivery practical without pretending that the network offers an end-to-end one-time effect.

### Key takeaways

- Separate delivery guarantees from business-effect guarantees.
- Choose persistence and replication for explicit failure goals.
- Make retry idempotent and poison-message recovery observable.

### Coverage of chapter topics

- Queue, producer, consumer, broker, push/pull delivery, and automatic/manual acknowledgement.
- Persistence, publish/subscribe, leader/follower replication, and RabbitMQ exchange types (direct, topic, fanout).
- Long-lived connections/channels, thread safety, channel reuse/pooling, broker queue concurrency, backlog pressure, and producer throttling.
- Publisher confirms, durable queues/messages, consumer acknowledgements, and the safety/latency trade-off.
- Competing consumers, scoped exactly-once effects through idempotency, bounded redelivery, dead-letter handling, monitoring, and replay.

## Proposed application

- **Application / solution name:** `ReliableAsyncMessagingLab`.
- **Namespace convention:** FoundationsOfScalableSystems.Chapter07
- **Independent solution folder:** solutions/07-asynchronous-messaging/
- **Current state:** learning guide and experiment design; implementation is not yet present.

## Practical exercise

Build a small order-processing service with an HTTP producer and independently scaled workers. Implement the same contract using (A) RabbitMQ work queues with competing consumers, and (B) Kafka topic/consumer groups when replay and multiple independent subscribers matter. Optionally add an SQS + DLQ deployment as a managed-queue comparison. Keep the business operation and load generator identical so the comparison isolates messaging semantics and operating costs.

1. Define the event schema, stable event ID, ordering key, idempotency rule, and the exact point at which an order is considered complete.
2. Publish with broker confirmation; persist the business effect and processed-event ID atomically where possible; acknowledge only after commit.
3. Add bounded retries with backoff, a dead-letter destination, alerting, and a documented replay command. Verify duplicate delivery does not duplicate the business effect.
4. Run a fixed workload with one worker and then several competing workers. Inject a worker crash before processing, after the effect but before ack, and a broker restart. Compare durable and lower-latency configurations.
5. Save raw results, environment details, versions, exact commands, and the failure-injection timeline.

### Implementable solution choices

| Solution | Choose it when | Concrete shape | Main costs / caveats |
|---|---|---|---|
| RabbitMQ durable work queue | Each task should be handled by one worker; routing and task acknowledgement are central | HTTP API → durable exchange/queue → competing .NET workers → PostgreSQL; publisher confirms, manual ack, bounded retry and DLQ | Broker queue/leader capacity can become the bottleneck; queue semantics differ from a replayable event log |
| Kafka event stream | Events must be retained/replayed and several consumer groups need independent copies | API + transactional outbox → Kafka topic keyed by aggregate ID → consumer groups → idempotent database projections | More operational components and partition planning; consumer offsets do not make an external DB effect exactly-once by themselves |
| Managed queue (e.g. SQS + DLQ) | Minimize broker operations and accept provider-specific semantics | API/outbox publisher → standard queue → worker fleet → DLQ after bounded receives | Cloud dependency, cost at volume, visibility-timeout and ordering constraints; validate provider limits |

For all three, separate “message accepted” from “business operation completed” in the API contract. Use an outbox when the database update and message publication must not diverge; keep consumers idempotent, cap in-flight work, and expose queue depth, oldest-message age, redelivery/DLQ count, processing latency, and drain time.

## What to measure

Publish/consume rate, queue depth or Kafka lag, oldest-message age, delivery failures, redelivery, DLQ count, drain time, and durable business completion. Also record broker disk/memory, network, and consumer concurrency.

Report at least p50/p95/p99 latency, throughput, errors, CPU, memory, and behavior during one relevant failure. Mark any unmeasured claim as **Not measured**.

## Design review questions

- What is the bottleneck or quality attribute this design addresses?
- Which semantics must remain true under retry, restart, or partial failure?
- What new bottleneck or operational cost does the approach introduce?
- What result would make us reject this design?

## References

- Ian Gorton, *Foundations of Scalable Systems*, Chapter 07: “Asynchronous Messaging” — [O’Reilly chapter page](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch07.html).
- Supporting primary documentation: [official reference](https://kafka.apache.org/documentation/).
- Repository-wide chapter map and implementation order: [Scalability roadmap](../../docs/SCALABILITY-ROADMAP-EN.md).

## Naming and independence

When implementation begins, keep this application's source, infrastructure, workload, and run instructions inside this folder. The namespace above is a proposed code namespace, not a Kubernetes namespace; choose a Kubernetes namespace from the same chapter slug only when this solution gets manifests. Do not share application code with another solution.
