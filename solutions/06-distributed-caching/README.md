# 06. Distributed Caching

![Chapter learning and experiment diagram](06-distributed-caching-overview.svg)

> Chapter guide in the *Foundations of Scalable Systems* learning track. This repository guide paraphrases topics and proposes an independent practical exercise; it does not reproduce the book.

## Goal

Turn this chapter's design question into a small, repeatable experiment. The result is a comparison with stated assumptions, not a universal technology ranking.

## Chapter focus

Application and web caching; HTTP validators; expiry; cache control; freshness and invalidation.

## Chapter synthesis

**Source reading: printed pages 115–124.** Original learning summary of the chapter's listed ideas; consult the book for full treatment.

Caching keeps reusable results near consumers to reduce computation, database reads, and network transfer. Application and web caches occupy different layers. HTTP caching uses freshness and validation: Cache-Control and Expires guide reuse; Last-Modified and ETag validate whether a representation changed. Caching lowers latency and read load but introduces stale data, invalidation errors, stampedes, and memory pressure. Define what may be cached, for how long, and how misses, eviction, and total cache loss behave.

### Key takeaways

- Cached data needs explicit freshness and invalidation policies.
- HTTP validators avoid transferring unchanged representations.
- Design cache misses, stampedes, eviction, and cache loss.

### Coverage of chapter topics

- Application cache versus web cache and where cached copies are reused.
- HTTP freshness and validation using Cache-Control, Expires, Last-Modified, and ETag.
- Correctness concerns: stale values, invalidation, and cache behavior on change.

## Proposed application

- **Application / solution name:** `DistributedCacheBehaviorLab`.
- **Namespace convention:** FoundationsOfScalableSystems.Chapter06
- **Independent solution folder:** solutions/06-distributed-caching/
- **Current state:** learning guide and experiment design; implementation is not yet present.

## Practical exercise

Compare cache disabled, cache-aside, and explicit invalidation for one read-heavy endpoint; define acceptable staleness.

1. Write down the operation, workload shape, data size, and correctness invariant.
2. Establish a simple baseline before adding the chapter's scaling technique.
3. Change one major design choice at a time and repeat the same workload.
4. Save raw results, environment details, versions, and the exact run command.

## What to measure

Hit ratio, backend load, latency, stale-read window, memory use, and recovery after cache loss.

Report at least p50/p95/p99 latency, throughput, errors, CPU, memory, and behavior during one relevant failure. Mark any unmeasured claim as **Not measured**.

## Design review questions

- What is the bottleneck or quality attribute this design addresses?
- Which semantics must remain true under retry, restart, or partial failure?
- What new bottleneck or operational cost does the approach introduce?
- What result would make us reject this design?

## References

- Ian Gorton, *Foundations of Scalable Systems*, Chapter 06: “Distributed Caching” — [O’Reilly chapter page](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch06.html).
- Supporting primary documentation: [official reference](https://redis.io/docs/latest/develop/use/patterns/).
- Repository-wide chapter map and implementation order: [Scalability roadmap](../../docs/SCALABILITY-ROADMAP-EN.md).

## Naming and independence

When implementation begins, keep this application's source, infrastructure, workload, and run instructions inside this folder. The namespace above is a proposed code namespace, not a Kubernetes namespace; choose a Kubernetes namespace from the same chapter slug only when this solution gets manifests. Do not share application code with another solution.
