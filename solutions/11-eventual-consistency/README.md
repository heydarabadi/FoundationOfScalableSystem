# 11. Eventual Consistency

![Chapter learning and experiment diagram](11-eventual-consistency-overview.svg)

> Chapter guide in the *Foundations of Scalable Systems* learning track. This repository guide paraphrases topics and proposes an independent practical exercise; it does not reproduce the book.

## Goal

Turn this chapter's design question into a small, repeatable experiment. The result is a comparison with stated assumptions, not a universal technology ranking.

## Chapter focus

Inconsistency windows; read-your-writes; tunable consistency; quorums; replica repair; conflict resolution.

## Chapter synthesis

**Source reading: printed pages 205–222.** Original learning summary of the chapter's ideas; consult the book for full treatment.

Replication turns a database into a distributed system: network delay, overload, failures, and geographic distance cause replicas to diverge temporarily. The inconsistency window is how long an update takes to reach all replicas; it has no guaranteed upper bound under arbitrary failures and grows with replica count, load, and distance. This is not an internal database detail: it determines what users can observe. Read-your-own-writes (RYOW) is a narrower guarantee: after a client changes an object, its later reads see that change, even if other clients may still see an older version. A system can implement this by routing a read to the leader or carrying a write bookmark/version into the next read.

Tunable consistency makes the latency/freshness choice explicit. For N replicas, W is the number that must confirm a write and R is the number read before returning. A larger W raises write latency and may reduce availability when nodes are unreachable; a larger R costs more on reads but gives the coordinator a better chance to find the latest version. Quorum values are a majority, (N/2)+1; when reads and writes use overlapping majorities (R + W > N), a read can intersect a successful write. This guarantee depends on the database's version ordering and quorum implementation. Sloppy quorum can accept writes on substitute nodes when home replicas are unavailable, raising availability but allowing stale reads until hinted handoff returns the update to its intended replica.

Replicas can drift through missed updates, node stalls, disk/network failures, or software bugs. Active/read repair compares replicas during reads and fixes stale copies; digest/hash reads can reduce data transfer. Passive anti-entropy periodically scans data, commonly using Merkle trees so matching subtrees need not be retransmitted. Repair consumes CPU, memory, network, and disk I/O and should be scheduled or throttled with production load in mind.

Leaderless writes can conflict when two clients update the same key concurrently. Last-writer-wins is operationally simple but timestamps do not prove real causal order: clock skew can make the later timestamp arbitrary and silently discard a valid update. Version checks reject stale writes; version vectors capture causality across replicas and can expose concurrent “siblings” for application-level merge or user resolution. CRDTs encode operations with merge rules that converge under concurrency for suitable data types (for example counters/sets), reducing manual conflict work but only when their semantics fit the domain. The practical contract must name which fields may be stale, whether the client needs RYOW, how concurrent edits are combined, and which data must never be overwritten silently.

### Key takeaways

- Eventual consistency is an application-visible contract.
- Quorums trade latency and freshness under failure.
- Conflict handling must preserve domain intent, not just compare timestamps.

### Coverage of chapter topics

- Replica propagation delay and inconsistency windows; read-your-writes via leader reads/bookmarks.
- Tunable N/R/W consistency, majority quorum overlap, availability/latency trade-offs, sloppy quorum, and hinted handoff.
- Active/read repair and passive anti-entropy with Merkle trees.
- Concurrent write conflicts, last-writer-wins limitations, version/version vectors, siblings, application merge, and CRDT convergence.

## Proposed application

- **Application / solution name:** `EventualConsistencyRecoveryLab`.
- **Namespace convention:** FoundationsOfScalableSystems.Chapter11
- **Independent solution folder:** solutions/11-eventual-consistency/
- **Current state:** learning guide and experiment design; implementation is not yet present.

## Practical exercise

Implement a small replicated profile or shopping-cart model with a primary/replicas simulation, then compare it with a database that exposes tunable consistency. Inject propagation delays and concurrent edits. The point is to make the user-visible consistency contract explicit and test it, not to build a new replication algorithm for production.

1. Define one record, write/read operations, acceptable stale-read duration (if any), RYOW requirements, and conflict behavior for every mutable field.
2. Test N=3 with W/R combinations such as (1,1), (2,2), (3,1), and (1,3); record latency and outcomes during normal operation and when one replica is unreachable.
3. Compare normal quorum with sloppy quorum/hinted handoff, and measure how long repair takes after the replica returns.
4. Issue two concurrent updates from the same old version. Verify whether last-writer-wins loses an edit, version-vector logic surfaces siblings, and a domain merge/CRDT gives the expected result.
5. Preserve a deterministic event trace so the same failure/interleaving can be replayed; do not infer causal order from wall-clock timestamps alone.

### Implementable solution choices

| Solution | Choose it when | Concrete shape | Main costs / caveats |
|---|---|---|---|
| Leader reads for RYOW + replica reads for tolerant data | Product needs immediate self-read after update but can show eventual updates to others | Write to leader → return version/bookmark → route that user's subsequent read to leader or wait until replica reaches version; other reads use followers | Leader gets more reads; session/bookmark state and fallback behavior must be designed |
| Quorum reads/writes | Need tunable balance of freshness, latency, and availability across replicated keys | N-replica store with request-level R/W settings; test majority overlap and observe failures when quorum is unavailable | Higher read/write latency and failed operations without quorum; sloppy quorum weakens freshness |
| Eventual read model / cache projection | UI or analytics can tolerate bounded-ish delay and writes should remain available | Source-of-truth transaction/outbox → event stream → per-view projection; expose updated-at/version and refresh/rebuild path | No hard convergence time guarantee during prolonged failure; duplicates, ordering and projection repair matter |
| Conflict-aware collaborative data | Independent concurrent edits are expected and should be merged | Optimistic version token + conflict response and merge UI, or a domain-appropriate CRDT for commutative operations | Version-vector/sibling or CRDT semantics add metadata and application complexity; LWW is unsafe for non-discardable edits |

For account balances, inventory reservations, or payments, do not select eventual semantics just to gain throughput. Keep strong invariants in a transactional authority and project less critical views asynchronously.

## What to measure

Observed staleness and convergence time, RYOW success, quorum failure rate, repair duration/bytes, conflict and lost-update count, and availability during partition.

Report at least p50/p95/p99 latency, throughput, errors, CPU, memory, and behavior during one relevant failure. Mark any unmeasured claim as **Not measured**.

## Design review questions

- What is the bottleneck or quality attribute this design addresses?
- Which semantics must remain true under retry, restart, or partial failure?
- What new bottleneck or operational cost does the approach introduce?
- What result would make us reject this design?

## References

- Ian Gorton, *Foundations of Scalable Systems*, Chapter 11: “Eventual Consistency” — [O’Reilly chapter page](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch11.html).
- Supporting primary documentation: [official reference](https://cassandra.apache.org/doc/latest/cassandra/architecture/dynamo.html).
- Repository-wide chapter map and implementation order: [Scalability roadmap](../../docs/SCALABILITY-ROADMAP-EN.md).

## Naming and independence

When implementation begins, keep this application's source, infrastructure, workload, and run instructions inside this folder. The namespace above is a proposed code namespace, not a Kubernetes namespace; choose a Kubernetes namespace from the same chapter slug only when this solution gets manifests. Do not share application code with another solution.
