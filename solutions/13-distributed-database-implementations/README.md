# 13. Distributed Database Implementations

![Chapter learning and experiment diagram](13-distributed-database-implementations-overview.svg)

> Chapter guide in the *Foundations of Scalable Systems* learning track. This repository guide paraphrases topics and proposes an independent practical exercise; it does not reproduce the book.

## Goal

Turn this chapter's design question into a small, repeatable experiment. The result is a comparison with stated assumptions, not a universal technology ranking.

## Chapter focus

Data models and APIs; distribution and replication; strengths and limits of Redis, MongoDB, and DynamoDB.

## Chapter synthesis

**Source reading: printed pages 247–267.** Original learning summary of the chapter's listed ideas; consult the book for full treatment.

The chapter compares Redis, MongoDB, and DynamoDB through their data models and APIs, distribution and replication, and resulting strengths and weaknesses. They are not interchangeable: each favors different access patterns and makes distinct choices about persistence, partitioning, consistency, and operations. Product features matter only relative to an application’s requirements. A sound comparison holds behavior constant, states durability and consistency, selects suitable keys and indexes, and includes failure and recovery—not only a throughput number.

### Key takeaways

- Map operations to each database’s native model before comparing.
- Include persistence, replication, recovery, and operational complexity.
- Equivalent workload and semantics are prerequisites for a useful benchmark.

## Proposed application

- **Application / solution name:** `DistributedDatabaseModelComparisonLab`.
- **Namespace convention:** FoundationsOfScalableSystems.Chapter13
- **Independent solution folder:** solutions/13-distributed-database-implementations/
- **Current state:** learning guide and experiment design; implementation is not yet present.

## Practical exercise

Implement the same use case only where each data model fits; state index, partition key, durability, query, and consistency requirements.

1. Write down the operation, workload shape, data size, and correctness invariant.
2. Establish a simple baseline before adding the chapter's scaling technique.
3. Change one major design choice at a time and repeat the same workload.
4. Save raw results, environment details, versions, and the exact run command.

## What to measure

Operation latency, throughput, index/write amplification, hot partitions, failover, and cost.

Report at least p50/p95/p99 latency, throughput, errors, CPU, memory, and behavior during one relevant failure. Mark any unmeasured claim as **Not measured**.

## Design review questions

- What is the bottleneck or quality attribute this design addresses?
- Which semantics must remain true under retry, restart, or partial failure?
- What new bottleneck or operational cost does the approach introduce?
- What result would make us reject this design?

## References

- Ian Gorton, *Foundations of Scalable Systems*, Chapter 13: “Distributed Database Implementations” — [O’Reilly chapter page](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch13.html).
- Supporting primary documentation: [official reference](https://www.mongodb.com/docs/manual/sharding/).
- Repository-wide chapter map and implementation order: [Scalability roadmap](../../docs/SCALABILITY-ROADMAP-EN.md).

## Naming and independence

When implementation begins, keep this application's source, infrastructure, workload, and run instructions inside this folder. The namespace above is a proposed code namespace, not a Kubernetes namespace; choose a Kubernetes namespace from the same chapter slug only when this solution gets manifests. Do not share application code with another solution.
