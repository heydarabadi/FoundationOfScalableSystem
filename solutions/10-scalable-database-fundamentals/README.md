# 10. Scalable Database Fundamentals

![Chapter learning and experiment diagram](10-scalable-database-fundamentals-overview.svg)

> Chapter guide in the *Foundations of Scalable Systems* learning track. This repository guide paraphrases topics and proposes an independent practical exercise; it does not reproduce the book.

## Goal

Turn this chapter's design question into a small, repeatable experiment. The result is a comparison with stated assumptions, not a universal technology ranking.

## Chapter focus

Relational scale-up/out; read replicas; partitioning; NoSQL models; distribution; CAP trade-offs.

## Chapter synthesis

**Source reading: printed pages 185–204.** Original learning summary of the chapter's ideas; consult the book for full treatment.

The chapter explains why very large and diverse datasets drove database engines toward distributed architectures, and why there is no single database shape for every workload. Relational databases remain strong where transactions, constraints, joins, and flexible SQL matter. A first scale-up step is a larger machine: it avoids application changes and exploits shared memory/storage, but hardware cost rises, a node remains a failure boundary, and growth eventually reaches a ceiling. Read replicas provide a common scale-out step for read-heavy workloads: writes go to a primary and propagate asynchronously, so replicas can be stale. Applications must decide which reads tolerate that lag and how failover works.

Partitioning distributes a dataset across physical partitions or nodes. Horizontal partitioning assigns rows by key/value/hash; vertical partitioning separates columns, often to isolate hot or rarely used data. Relational joins across partitions can require substantial network movement and coordination. Reference data can be copied to each node, joins can align on partition keys/indexes, and selective filters can reduce data movement. Shared-everything clusters such as the Oracle RAC example add database compute around shared storage and cache coordination, but depend on specialized shared infrastructure that can become costly or a bottleneck. Shared-nothing systems give nodes their own compute and storage and distribute data, but require routing and replication protocols.

NoSQL grew from cheap commodity hardware, internet-scale availability needs, rapidly changing/unstructured data, and new analytics use cases. Its common trade-offs are simpler, evolvable models, product-specific query APIs, fewer general joins, and native horizontal distribution. Modeling shifts from normalizing the domain for flexible joins toward designing around known access patterns: denormalized/read-optimized records can make reads and sharding simpler, at the cost of duplicated data, extra write work, and integrity/convergence logic. The chapter distinguishes key-value (lookup by key), document (query/index document fields), wide-column (keyed rows with flexible columns), and graph (relationships as first-class data) models. “Schemaless” does not mean structure-free; applications must interpret versions and discover fields, while schema-on-write and schema-on-read place responsibility in different places.

Sharding needs a key that lets the database locate data and distribute load. Hash-based placement can balance well but makes range locality harder; value-based or range-based placement can preserve locality but risks skew and hot partitions. Graph relationships and relational joins make partitioning especially difficult when a query crosses nodes. Replication restores availability when a partition/node is lost and may scale reads, but brings consistency choices. Leader-follower designs send writes to one leader and can fan out reads; leaderless designs accept requests at multiple replicas and coordinate convergence. Strong consistency can require waiting for replicas; eventual consistency lets some replicas lag. CAP is about the choices during a network partition: a system may reject/limit operations to preserve a single consistent view (CP behavior), or accept operations on reachable sides and reconcile later (AP behavior). It is a failure-mode lens, not a blanket classification or a third freely selectable feature.

### Key takeaways

- Understand a relational baseline before adding replicas or partitions.
- Choose NoSQL by model and access pattern.
- Partition keys affect scale distribution and hot-key risk; CAP concerns partition-time guarantees.

### Coverage of chapter topics

- Drivers for distributed data and relational scale-up, primary/read replicas, horizontal/vertical partitioning, and distributed join costs.
- Shared-everything clustering (RAC example) versus shared-nothing architectures.
- NoSQL motivation, access-pattern-first denormalization, model families (key-value, document, wide-column, graph), schema/query trade-offs.
- Hash, value, and range sharding; locality, skew, hot partitions, and graph/join distribution challenges.
- Leader-follower and leaderless replication, strong/eventual consistency, and CAP behavior under network partitions.

## Proposed application

- **Application / solution name:** `ScalableDatabaseFundamentalsLab`.
- **Namespace convention:** FoundationsOfScalableSystems.Chapter10
- **Independent solution folder:** solutions/10-scalable-database-fundamentals/
- **Current state:** learning guide and experiment design; implementation is not yet present.

## Practical exercise

Build one transaction-oriented workload (for example, order creation plus order-history reads) against a PostgreSQL baseline. Add read replicas for read-heavy traffic, then compare a partitioned relational layout and a denormalized document/key-value model for the same named query patterns. Keep the product requirement and dataset constant; do not compare databases using unrelated schemas or query semantics.

1. List the read/write operations, transaction boundaries, indexes, data growth, consistency needed after a write, and acceptable stale-read window.
2. Establish a tuned single-primary baseline. Add a read replica only for queries that can tolerate replication delay; measure the delay and route read-after-write requests to the primary when needed.
3. Pick a candidate shard key from actual query predicates and tenant/aggregate access. Simulate skew and hot keys before choosing hash, range, or value placement.
4. For a NoSQL variant, build records around the same use cases and explicitly document duplicated fields, update fan-out, and repair/rebuild strategy.
5. Test primary/replica failure and a network partition. Record which operations block, fail, or return stale data and confirm that behavior matches the product contract.

### Implementable solution choices

| Solution | Choose it when | Concrete shape | Main costs / caveats |
|---|---|---|---|
| PostgreSQL scale-up + read replicas | Transactions and SQL are central; reads dominate and one primary still fits writes | ASP.NET API → PostgreSQL primary for writes → asynchronous read replicas for safe reads; connection pool and lag-aware routing | Primary remains write ceiling; replicas can return stale values; HA/failover needs operational runbooks |
| Partitioned relational database | Dataset/write volume exceeds one node and access has a stable partition key | Tenant/order-key partitioning → route queries by key → local indexes/joins; global reference tables replicated where small | Cross-partition joins/transactions are expensive; skew and rebalancing are complex |
| Document or key-value read model | Access patterns are known and bounded; low-latency reads justify denormalization | PostgreSQL/outbox or Kafka events → projection worker → MongoDB documents or Redis/Dynamo-style key records keyed by aggregate/user | Eventual consistency, duplicate data, rebuild and schema-version handling; keep system of record explicit |
| Relational + specialized stores (polyglot) | Different workload classes have distinct storage needs | PostgreSQL as transaction source → event stream → search/document/cache/analytics projections | More systems, pipelines, monitoring, backup and reconciliation; avoid making several stores co-equal sources of truth |

Promote a read model or partition only after measurements identify the bottleneck. Keep a migration/rebuild path and validate data correctness before moving production reads.

## What to measure

Per-query p50/p95/p99, write throughput, replication lag, cross-partition bytes/latency, hot-key skew, storage/index growth, failover behavior, and operational cost.

Report at least p50/p95/p99 latency, throughput, errors, CPU, memory, and behavior during one relevant failure. Mark any unmeasured claim as **Not measured**.

## Design review questions

- What is the bottleneck or quality attribute this design addresses?
- Which semantics must remain true under retry, restart, or partial failure?
- What new bottleneck or operational cost does the approach introduce?
- What result would make us reject this design?

## References

- Ian Gorton, *Foundations of Scalable Systems*, Chapter 10: “Scalable Database Fundamentals” — [O’Reilly chapter page](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch10.html).
- Supporting primary documentation: [official reference](https://www.postgresql.org/docs/current/ddl-partitioning.html).
- Repository-wide chapter map and implementation order: [Scalability roadmap](../../docs/SCALABILITY-ROADMAP-EN.md).

## Naming and independence

When implementation begins, keep this application's source, infrastructure, workload, and run instructions inside this folder. The namespace above is a proposed code namespace, not a Kubernetes namespace; choose a Kubernetes namespace from the same chapter slug only when this solution gets manifests. Do not share application code with another solution.
