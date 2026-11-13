# 15. Stream Processing Systems

![Chapter learning and experiment diagram](15-stream-processing-systems-overview.svg)

> Chapter guide in the *Foundations of Scalable Systems* learning track. This repository guide paraphrases topics and proposes an independent practical exercise; it does not reproduce the book.

## Goal

Turn this chapter's design question into a small, repeatable experiment. The result is a comparison with stated assumptions, not a universal technology ranking.

## Chapter focus

Stream platforms; event time; stateful windows and joins; scalability; checkpointing and data safety.

## Chapter synthesis

**Source reading: printed pages 287–301.** Original learning summary of the chapter's ideas; consult the book for full treatment.

Stream processing computes continuously over data-in-motion, reducing the delay of a batch ETL pipeline when results must reflect recent events. Batch remains useful for large historical recomputation and workloads that tolerate minutes or hours of freshness delay. The Lambda architecture combines a periodic batch layer and a low-latency speed layer, then serves both; the chapter notes that a durable event log plus replayable stream processing can simplify this into a Kappa-style design for many workloads. These are architectural choices, not one mandatory product stack.

A streaming application is a directed acyclic dataflow from sources through transformations to sinks. Stateless operators transform independent events; stateful operators retain keyed data across events, for example counts, model features, or the latest bus locations. Windows turn an unbounded stream into finite computations: tumbling windows use disjoint intervals, while sliding windows overlap. The chapter's Flink example groups lift events by lift ID and computes a ten-minute rolling total every five minutes. A production design must also choose the time basis and policy for out-of-order/late events; the book’s core demonstration emphasizes processing-time windows and keyed aggregation.

Flink compiles the logical dataflow into a physical DAG. Parallelism is set per operator or as a default, and keyed partitioning routes events for the same key to a state owner. Task managers execute operator subtasks in task slots; operator chaining can reduce communication overhead. Higher parallelism improves throughput only when source partitions, network, CPU, and sink capacity can keep up. A global/single operator can become the bottleneck.

Stateful processing needs consistent recovery. Flink persists operator state and periodically checkpoints the complete dataflow. Stream barriers travel with source records; each operator snapshots state at the same input position, and a completed checkpoint ties state to source offsets. After failure, the job restores state and resumes after the checkpointed position. This prevents arbitrary gaps in the stream computation, but checkpoint storage and frequency consume I/O and can affect throughput, especially with large state. End-to-end effects also depend on the sink: replay after recovery can duplicate external writes unless the sink is transactional or idempotent. Measure checkpoint duration, failure recovery, event lag, state size, and sink correctness together.

### Key takeaways

- Set event-time and late-event policies explicitly.
- Align checkpoints, source offsets, and sink effects for recovery.
- Stateful streams support temporal computation but add operational complexity.

### Coverage of chapter topics

- Batch ETL freshness limits, streaming use cases, and Lambda versus log/replay-oriented Kappa architecture.
- Dataflow DAGs, stateless/stateful operators, keyed state, tumbling/sliding windows, and time-sensitive results.
- Flink DataStream source/transform/sink model, keyed partitioning, operator parallelism, task slots, task managers, and operator chaining.
- State persistence, stream-barrier checkpoints aligned with source positions, restore/replay, and sink-side duplicate safety.

## Proposed application

- **Application / solution name:** `StatefulStreamProcessingLab`.
- **Namespace convention:** FoundationsOfScalableSystems.Chapter15
- **Independent solution folder:** solutions/15-stream-processing-systems/
- **Current state:** learning guide and experiment design; implementation is not yet present.

## Practical exercise

Build a Flink job reading purchase events from Kafka, keyed by `productId`, producing a rolling five-minute sales total every minute to a database or Kafka output. Add a late/out-of-order test stream and a restartable state checkpoint. Compare this stateful job with a simple Kafka consumer that only validates and routes independent records; this establishes whether the extra stream runtime is justified.

1. Define the result semantics: processing-time or event-time, window size/slide, allowed lateness, and how late events revise or do not revise prior output.
2. Key the stream by the entity whose operations must be serialized. Set operator parallelism no higher than useful source partitions unless repartitioning is intentional.
3. Configure durable checkpoint storage and interval. Fail a task manager after state changes and verify restart resumes from the matching source position without losing counts.
4. Make the sink idempotent using a deterministic window/key/version or transactional sink; inject a crash after output but before checkpoint completion and verify no incorrect duplicate aggregate.
5. Vary parallelism and checkpoint interval separately; report throughput, event lag, p99 processing delay, checkpoint time, state bytes, recovery duration, and sink writes.

### Implementable solution choices

| Solution | Choose it when | Concrete shape | Main costs / caveats |
|---|---|---|---|
| Kafka Streams / simple consumer | Transformations are modest and the app should stay close to Kafka deployment | Kafka topic → keyed .NET/Java consumer or Kafka Streams topology → compacted/result topic or database | Less rich distributed state/window management than a dedicated engine; application handles state/rebalance/recovery details |
| Apache Flink stateful stream | Large state, event-time windows, joins, or checkpointed recovery are core requirements | Kafka source → keyed/windowed operators → checkpointed Flink cluster → transactional/idempotent sink | Job manager/task manager ops, checkpoint storage, state sizing and tuning; sink semantics must be proven |
| Batch + speed (Lambda) | Existing batch truth and historical recompute must coexist with urgent recent results | Durable event archive → periodic batch recompute + streaming fast view → serving layer merges/reconciles | Two implementations can disagree; duplicate business logic and reconciliation are costly |
| Log replay (Kappa-style) | One durable event log can be replayed to rebuild all derived state | Kafka retention/archive → versioned stream job → projections; redeploy/replay to recompute | Replay duration/capacity and schema evolution need planning; external side effects must be guarded |

Use a stream engine for keyed state, temporal windows, joins, or coordinated checkpoint recovery. For independent message handling, keep a regular consumer and avoid adding a second runtime without evidence.

## What to measure

End-to-end event lag, late/out-of-order event count, throughput, operator skew, state size, checkpoint duration/failures, recovery time, sink idempotency, and replay correctness.

Report at least p50/p95/p99 latency, throughput, errors, CPU, memory, and behavior during one relevant failure. Mark any unmeasured claim as **Not measured**.

## Design review questions

- What is the bottleneck or quality attribute this design addresses?
- Which semantics must remain true under retry, restart, or partial failure?
- What new bottleneck or operational cost does the approach introduce?
- What result would make us reject this design?

## References

- Ian Gorton, *Foundations of Scalable Systems*, Chapter 15: “Stream Processing Systems” — [O’Reilly chapter page](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch15.html).
- Supporting primary documentation: [official reference](https://nightlies.apache.org/flink/flink-docs-stable/docs/).
- Repository-wide chapter map and implementation order: [Scalability roadmap](../../docs/SCALABILITY-ROADMAP-EN.md).

## Naming and independence

When implementation begins, keep this application's source, infrastructure, workload, and run instructions inside this folder. The namespace above is a proposed code namespace, not a Kubernetes namespace; choose a Kubernetes namespace from the same chapter slug only when this solution gets manifests. Do not share application code with another solution.
