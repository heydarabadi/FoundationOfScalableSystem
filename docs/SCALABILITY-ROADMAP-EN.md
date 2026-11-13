# Scalable Systems: Learning and Solution Roadmap

This repository follows Ian Gorton's *Foundations of Scalable Systems* as its learning spine. The book supplies the conceptual sequence; the solutions here are independent experiments that apply those concepts. This is a companion roadmap, not a reproduction of the book. Chapter topics below are brief paraphrases of the publisher's public contents.

**Primary source:** [O'Reilly — Foundations of Scalable Systems](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/)

## How to use this repository

1. Learn a design principle from the book.
2. Pick one independent solution that makes the principle concrete.
3. Run the same workload and compare throughput, tail latency, errors, resource use, durability, and operational cost.
4. Record what was measured, on which hardware and configuration. A technology name or a benchmark target is not evidence of capacity.

Each approach belongs in its own `solutions/<solution-name>/` directory with its own code, dependencies, infrastructure, workloads, and README. Do not create empty solution folders for ideas that have not been implemented. The chapter 14 solution keeps Lab and Performance as profiles of one implementation: [Scalable Event-Driven Processing](../solutions/14-scalable-event-driven-processing/README.md).

## Chapter solution guides

To compare complete architectures for the basket and order workload, start with the [English implementable blueprints](IMPLEMENTABLE-SOLUTION-BLUEPRINTS-EN.md) or [Persian version](IMPLEMENTABLE-SOLUTION-BLUEPRINTS-FA.md). These are designs; Chapter 14 is the only currently runnable solution.

Each chapter has a bilingual README and its own original SVG concept diagram. Chapters without runnable code yet are explicitly marked as guides/designs; their project and namespace names are proposals until implementation.

| Chapter | Guide |
|---|---|
| 01. Introduction to Scalable Systems | [README](../solutions/01-introduction-to-scalable-systems/README.md) · [فارسی](../solutions/01-introduction-to-scalable-systems/README.fa.md) |
| 02. Distributed Systems Architectures: An Introduction | [README](../solutions/02-distributed-systems-architectures/README.md) · [فارسی](../solutions/02-distributed-systems-architectures/README.fa.md) |
| 03. Distributed Systems Essentials | [README](../solutions/03-distributed-systems-essentials/README.md) · [فارسی](../solutions/03-distributed-systems-essentials/README.fa.md) |
| 04. An Overview of Concurrent Systems | [README](../solutions/04-overview-of-concurrent-systems/README.md) · [فارسی](../solutions/04-overview-of-concurrent-systems/README.fa.md) |
| 05. Application Services | [README](../solutions/05-application-services/README.md) · [فارسی](../solutions/05-application-services/README.fa.md) |
| 06. Distributed Caching | [README](../solutions/06-distributed-caching/README.md) · [فارسی](../solutions/06-distributed-caching/README.fa.md) |
| 07. Asynchronous Messaging | [README](../solutions/07-asynchronous-messaging/README.md) · [فارسی](../solutions/07-asynchronous-messaging/README.fa.md) |
| 08. Serverless Processing Systems | [README](../solutions/08-serverless-processing-systems/README.md) · [فارسی](../solutions/08-serverless-processing-systems/README.fa.md) |
| 09. Microservices | [README](../solutions/09-microservices/README.md) · [فارسی](../solutions/09-microservices/README.fa.md) |
| 10. Scalable Database Fundamentals | [README](../solutions/10-scalable-database-fundamentals/README.md) · [فارسی](../solutions/10-scalable-database-fundamentals/README.fa.md) |
| 11. Eventual Consistency | [README](../solutions/11-eventual-consistency/README.md) · [فارسی](../solutions/11-eventual-consistency/README.fa.md) |
| 12. Strong Consistency | [README](../solutions/12-strong-consistency/README.md) · [فارسی](../solutions/12-strong-consistency/README.fa.md) |
| 13. Distributed Database Implementations | [README](../solutions/13-distributed-database-implementations/README.md) · [فارسی](../solutions/13-distributed-database-implementations/README.fa.md) |
| 14. Scalable Event-Driven Processing | [README](../solutions/14-scalable-event-driven-processing/README.md) · [فارسی](../solutions/14-scalable-event-driven-processing/README.fa.md) |
| 15. Stream Processing Systems | [README](../solutions/15-stream-processing-systems/README.md) · [فارسی](../solutions/15-stream-processing-systems/README.fa.md) |
| 16. Final Tips for Success | [README](../solutions/16-final-tips-for-success/README.md) · [فارسی](../solutions/16-final-tips-for-success/README.fa.md) |
| Book chapter | Paraphrased subject | Repository application |
|---|---|---|
| 1. Introduction to Scalable Systems | Scalability, cost, and architecture trade-offs | Define workload and SLO before choosing a design; compare capacity with cost and operability. |
| 2. Distributed Systems Architectures | Scale-out, caching, data distribution, processing tiers | Use the current solution as a multi-tier baseline; add a simple synchronous baseline for comparison. |
| 3. Distributed Systems Essentials | Communication, partial failure, consensus, time | Add failure, retry, timeout, and duplicate-delivery experiments to solution runbooks. |
| 4. Concurrent Systems | Threads, races, deadlocks, coordination, pools | Compare concurrency limits and backpressure in each language implementation. |
| 5. Application Services | API design, state, horizontal scaling, load balancing, health | Keep a stateless API baseline; measure service scaling and load distribution. |
| 6. Distributed Caching | Application and web caches, HTTP caching | Compare no-cache and Redis-backed read projections; state invalidation and stale-read behavior. |
| 7. Asynchronous Messaging | Persistence, pub/sub, competing consumers, delivery and safety | Keep Kafka consumer scaling; add explicit retry, poison-message, idempotency, and outbox cases. |
| 8. Serverless Processing | Event-triggered compute, lifecycle, autoscaling, cost | Later optional solution: compare a managed function/queue path with always-on workers. |
| 9. Microservices | Decomposition, resilience, cascading failures, bulkheads | Treat the current API and processors as a service-boundary example; measure failure isolation. |
| 10. Scalable Database Fundamentals | Relational scale-up/out, replicas, partitioning, NoSQL, CAP | Compare PostgreSQL baseline with read replicas/partitioning and selected NoSQL models. |
| 11. Eventual Consistency | Quorums, repair, conflict handling, read-your-writes | Document consistency guarantees and test stale reads, recovery, and conflict behavior. |
| 12. Strong Consistency | Distributed transactions, 2PC, consensus, Raft | Compare transactional relational writes with a distributed-SQL option where justified. |
| 13. Distributed Database Implementations | Redis, MongoDB, DynamoDB models and trade-offs | Build equivalent data-access scenarios only when the data model fits; compare durability and query semantics. |
| 14. Scalable Event-Driven Processing | Kafka topics, producers, consumers, scale and availability | Existing event-driven solution; make partitioning, key choice, lag, replay, and consumer limits explicit. |
| 15. Stream Processing Systems | Stream platforms, Flink/DataStream, safety and scale | Later optional solution for windowed joins/aggregations; distinguish stream processing from simple consumers. |
| 16. Final Tips for Success | Automation, observability, deployment, data lakes | Require repeatable deployment, telemetry, load profiles, cleanup, and result records for every solution. |

## Candidate solution tracks

These are proposals, not claims that the technologies are already implemented or that one is universally fastest. Start with one workload and vary one major design choice at a time.

| Track | What it isolates | Candidate technologies | Suggested status |
|---|---|---|---|
| Event-driven baseline | Durable queue, partitioned work, consumer scaling | Existing .NET + Kafka + Redis + PostgreSQL + Kubernetes | Implemented; validate each scenario before treating results as capacity proof. |
| Synchronous relational baseline | Direct request/transaction path versus queued work | .NET or Go + PostgreSQL; optionally MySQL as a separate database variant | Next: create a small equivalent workload and keep semantics identical. |
| Database scaling variants | Read scaling, partitioning, document/key-value trade-offs | PostgreSQL replicas/partitioning; MongoDB; DynamoDB; Redis as cache/projection | Add as separate runnable variants only with explicit consistency and durability contracts. |
| CQRS and transactional outbox | Separate read/write models and reliable DB-to-broker publication | .NET/Go + PostgreSQL + Kafka | Add after the synchronous baseline; measure write amplification and projection lag. |
| Event sourcing | Durable domain-event history as source of truth, replay, snapshots | Kafka plus a durable event store/database; language chosen per implementation | Separate solution; Kafka retention alone should not be called a complete event store without explicit guarantees. |
| Stream processing | Stateful windows, joins, aggregations, recovery | Kafka + Flink (Java/Scala ecosystem); compare Kafka Streams where appropriate | Later: introduce only a workload that needs event-time/stateful processing. |
| Runtime/language comparison | Runtime, allocation, concurrency, and implementation overhead | C#/.NET, Go, Rust, Scala/JVM, Elixir/BEAM | Compare against the same protocol, data model, dependencies, and load; publish raw run context. |

## Database choice guide

- **PostgreSQL** is the current transactional reference for checkout and inventory. First tune queries, indexes, connection use, and transaction boundaries; then evaluate replicas or partitioning when measurements identify a read or data-volume bottleneck.
- **MySQL** can be a relational comparison, but use the same schema intent and transaction semantics. Keep it a distinct variant rather than silently swapping the database underneath one result.
- **Redis** is appropriate for fast cache/projection access in the current design; state whether values are disposable, rebuildable, or durable, and measure eviction and recovery.
- **MongoDB** fits document-shaped access patterns; compare query flexibility, index cost, partition key, and consistency behavior rather than treating it as a drop-in SQL replacement.
- **DynamoDB** is a managed key-value/document option for access patterns designed around partition keys and conditional operations. Keep cloud cost, limits, and regional behavior visible in the result.
- **Distributed SQL** (for example, CockroachDB or YugabyteDB) is a candidate when SQL transactions and horizontal distribution are both requirements; test transaction latency, contention, and operational overhead before adopting it.

No database comparison is fair unless it implements the same observable business operation and defines its consistency, durability, and failure behavior.

## Programming-language comparison rules

The repository should explore high-performance programming without declaring a language the winner in advance.

- **C#/.NET:** current reference implementation; measure async I/O, allocation, thread-pool pressure, and database/client bottlenecks.
- **Go:** candidate for a compact concurrent service/consumer; measure goroutine scheduling, allocation, and backpressure.
- **Rust:** candidate where CPU cost, memory control, or predictable tail latency is central; include implementation and operational complexity in the comparison.
- **Scala/JVM:** candidate for Kafka-native stream processing and typed data pipelines; measure JVM warm-up, GC, and steady-state behavior.
- **Elixir/BEAM:** candidate for large numbers of concurrent, failure-isolated processes and message-oriented workloads. Do not assume it wins CPU-heavy throughput tests; benchmark the workload it is suited to.

For valid comparisons, pin runtime and client versions, use equivalent message schemas and partition keys, match retry/commit semantics, warm up consistently, report p50/p95/p99, throughput, errors, CPU, memory, and recovery behavior, and state hardware and configuration. Keep language variants in separate solution folders if they are complete implementations; shared benchmark definitions may live in repository-level docs, but solutions must not share application code.

## Recommended order of implementation

1. Preserve the existing Kafka solution as the first worked example and capture a repeatable baseline.
2. Add a synchronous PostgreSQL solution as a control, using the same basket/order workload and clear transaction guarantees.
3. Add one database scaling variant (read replica or partitioning) based on a measured bottleneck, not as a technology showcase.
4. Add CQRS/outbox and event sourcing as separate designs with explicit consistency, replay, and duplicate-handling contracts.
5. Add a real stateful-streaming example when the workload requires windows, joins, or event-time processing.
6. Port one hot path to Go or Rust, then Scala or Elixir where their concurrency/data-processing model answers a specific question.

Every new solution should include a local README, setup/cleanup instructions, a workload definition, an architecture note, known limits, and measurements or an explicit “not measured” status. Keep the root README as the catalog and link each solution to this roadmap.
