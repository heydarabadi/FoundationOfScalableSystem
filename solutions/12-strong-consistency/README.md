# 12. Strong Consistency

![Chapter learning and experiment diagram](12-strong-consistency-overview.svg)

> Chapter guide in the *Foundations of Scalable Systems* learning track. This repository guide paraphrases topics and proposes an independent practical exercise; it does not reproduce the book.

## Goal

Turn this chapter's design question into a small, repeatable experiment. The result is a comparison with stated assumptions, not a universal technology ranking.

## Chapter focus

Consistency models; distributed transactions; two-phase commit; consensus; leader election; practical trade-offs.

## Chapter synthesis

**Source reading: printed pages 223–244.** Original learning summary of the chapter's listed ideas; consult the book for full treatment.

Strong consistency orders operations according to a stated model, simplifying some reasoning at coordination cost. Distributed transactions coordinate participants; two-phase commit has prepare and commit phases but can block or require recovery after failures. Consensus reaches agreement under explicit assumptions; Raft uses leaders and replicated logs, so leader election and log progress influence latency and availability. VoltDB and Spanner illustrate implementations with different mechanisms. Define invariants, failure assumptions, transaction scope, and latency budget before deciding whether strong guarantees justify coordination.

### Key takeaways

- Specify the consistency model and invariant precisely.
- Know two-phase commit recovery behavior and consensus assumptions.
- Use coordination where correctness requires it and account for availability and latency costs.

### Coverage of chapter topics

- Strong consistency models and distributed transactions.
- Two-phase commit and failure modes; consensus algorithms, Raft, and leader election.
- Practical strong-consistency examples: VoltDB and Google Cloud Spanner.

## Proposed application

- **Application / solution name:** `StrongConsistencyTransactionLab`.
- **Namespace convention:** FoundationsOfScalableSystems.Chapter12
- **Independent solution folder:** solutions/12-strong-consistency/
- **Current state:** learning guide and experiment design; implementation is not yet present.

## Practical exercise

Compare a single-database transaction with a distributed transaction scenario; force participant failure and inspect recovery.

1. Write down the operation, workload shape, data size, and correctness invariant.
2. Establish a simple baseline before adding the chapter's scaling technique.
3. Change one major design choice at a time and repeat the same workload.
4. Save raw results, environment details, versions, and the exact run command.

## What to measure

Commit latency, abort rate, lock duration, availability during failure, and invariant violations.

Report at least p50/p95/p99 latency, throughput, errors, CPU, memory, and behavior during one relevant failure. Mark any unmeasured claim as **Not measured**.

## Design review questions

- What is the bottleneck or quality attribute this design addresses?
- Which semantics must remain true under retry, restart, or partial failure?
- What new bottleneck or operational cost does the approach introduce?
- What result would make us reject this design?

## References

- Ian Gorton, *Foundations of Scalable Systems*, Chapter 12: “Strong Consistency” — [O’Reilly chapter page](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch12.html).
- Supporting primary documentation: [official reference](https://raft.github.io/raft.pdf).
- Repository-wide chapter map and implementation order: [Scalability roadmap](../../docs/SCALABILITY-ROADMAP-EN.md).

## Naming and independence

When implementation begins, keep this application's source, infrastructure, workload, and run instructions inside this folder. The namespace above is a proposed code namespace, not a Kubernetes namespace; choose a Kubernetes namespace from the same chapter slug only when this solution gets manifests. Do not share application code with another solution.
