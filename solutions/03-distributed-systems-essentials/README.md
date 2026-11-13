# 03. Distributed Systems Essentials

![Chapter learning and experiment diagram](03-distributed-systems-essentials-overview.svg)

> Chapter guide in the *Foundations of Scalable Systems* learning track. This repository guide paraphrases topics and proposes an independent practical exercise; it does not reproduce the book.

## Goal

Turn this chapter's design question into a small, repeatable experiment. The result is a comparison with stated assumptions, not a universal technology ranking.

## Chapter focus

Communication and remote calls; partial failures; consensus; clocks and time; uncertainty across machines.

## Chapter synthesis

**Source reading: printed pages 35–58.** Original learning summary of the chapter's listed ideas; consult the book for full treatment.

Distributed components communicate through networks that can delay, reorder, or lose messages. A remote call is not a local function call: after a timeout, the caller may not know whether the operation failed, succeeded, or is still running. Partial failure is normal, so deadlines, retries, and idempotency must be designed together. Consensus concerns agreement among distributed participants under failure assumptions; time is uncertain because clocks differ and messages take variable time. Do not assume perfect reachability or a globally precise clock. State guarantees and recovery behavior explicitly.

### Key takeaways

- A timeout means uncertainty, not proof the remote operation did not happen.
- Retries need deadlines and idempotent effects to avoid amplification and duplicates.
- Clocks and consensus have limits; specify ordering and recovery guarantees.

## Proposed application

- **Application / solution name:** `DistributedFailureSemanticsLab`.
- **Namespace convention:** FoundationsOfScalableSystems.Chapter03
- **Independent solution folder:** solutions/03-distributed-systems-essentials/
- **Current state:** learning guide and experiment design; implementation is not yet present.

## Practical exercise

Inject timeouts and unavailable dependencies into a small request path; document retries, deadlines, and duplicate effects.

1. Write down the operation, workload shape, data size, and correctness invariant.
2. Establish a simple baseline before adding the chapter's scaling technique.
3. Change one major design choice at a time and repeat the same workload.
4. Save raw results, environment details, versions, and the exact run command.

## What to measure

Recovery time, retry amplification, duplicate operations, timeout rate, and consistency after recovery.

Report at least p50/p95/p99 latency, throughput, errors, CPU, memory, and behavior during one relevant failure. Mark any unmeasured claim as **Not measured**.

## Design review questions

- What is the bottleneck or quality attribute this design addresses?
- Which semantics must remain true under retry, restart, or partial failure?
- What new bottleneck or operational cost does the approach introduce?
- What result would make us reject this design?

## References

- Ian Gorton, *Foundations of Scalable Systems*, Chapter 03: “Distributed Systems Essentials” — [O’Reilly chapter page](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch03.html).
- Supporting primary documentation: [official reference](https://lamport.azurewebsites.net/pubs/time-clocks.pdf).
- Repository-wide chapter map and implementation order: [Scalability roadmap](../../docs/SCALABILITY-ROADMAP-EN.md).

## Naming and independence

When implementation begins, keep this application's source, infrastructure, workload, and run instructions inside this folder. The namespace above is a proposed code namespace, not a Kubernetes namespace; choose a Kubernetes namespace from the same chapter slug only when this solution gets manifests. Do not share application code with another solution.
