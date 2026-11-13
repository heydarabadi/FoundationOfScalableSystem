# 08. Serverless Processing Systems

![Chapter learning and experiment diagram](08-serverless-processing-systems-overview.svg)

> Chapter guide in the *Foundations of Scalable Systems* learning track. This repository guide paraphrases topics and proposes an independent practical exercise; it does not reproduce the book.

## Goal

Turn this chapter's design question into a small, repeatable experiment. The result is a comparison with stated assumptions, not a universal technology ranking.

## Chapter focus

Invocation lifecycle; autoscaling; cold starts; concurrency controls; throughput and cost parameters.

## Chapter synthesis

**Source reading: printed pages 147–161.** Original learning summary of the chapter's listed ideas; consult the book for full treatment.

Serverless offers managed execution that scales with demand, useful for intermittent or bursty workloads. The chapter examines Google App Engine and AWS Lambda, autoscaling, function life cycle, and execution considerations. Startup delay, limits, concurrency, externalized state, and platform constraints affect throughput and tail latency. Serverless reduces idle capacity and some operational work but does not remove design needs around dependencies, data, retries, or cost. Tune parameters through repeatable throughput and cost experiments.

### Key takeaways

- Match serverless to workload shape and idle periods.
- Include startup, execution limits, external state, and downstream concurrency.
- Tune against measured throughput, latency, and cost.

### Coverage of chapter topics

- Why serverless fits uneven demand; App Engine Standard and autoscaling.
- Lambda lifecycle, startup/execution behavior, and scalability limits.
- A throughput/cost case study and choosing parameter values from measurements.

## Proposed application

- **Application / solution name:** `ServerlessThroughputCostLab`.
- **Namespace convention:** FoundationsOfScalableSystems.Chapter08
- **Independent solution folder:** solutions/08-serverless-processing-systems/
- **Current state:** learning guide and experiment design; implementation is not yet present.

## Practical exercise

Compare a queue-triggered function with an always-on worker for the same idempotent task and arrival pattern.

1. Write down the operation, workload shape, data size, and correctness invariant.
2. Establish a simple baseline before adding the chapter's scaling technique.
3. Change one major design choice at a time and repeat the same workload.
4. Save raw results, environment details, versions, and the exact run command.

## What to measure

Cold-start latency, throughput, concurrency, retries, cost per completed task, and backlog recovery.

Report at least p50/p95/p99 latency, throughput, errors, CPU, memory, and behavior during one relevant failure. Mark any unmeasured claim as **Not measured**.

## Design review questions

- What is the bottleneck or quality attribute this design addresses?
- Which semantics must remain true under retry, restart, or partial failure?
- What new bottleneck or operational cost does the approach introduce?
- What result would make us reject this design?

## References

- Ian Gorton, *Foundations of Scalable Systems*, Chapter 08: “Serverless Processing Systems” — [O’Reilly chapter page](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch08.html).
- Supporting primary documentation: [official reference](https://docs.aws.amazon.com/lambda/latest/dg/welcome.html).
- Repository-wide chapter map and implementation order: [Scalability roadmap](../../docs/SCALABILITY-ROADMAP-EN.md).

## Naming and independence

When implementation begins, keep this application's source, infrastructure, workload, and run instructions inside this folder. The namespace above is a proposed code namespace, not a Kubernetes namespace; choose a Kubernetes namespace from the same chapter slug only when this solution gets manifests. Do not share application code with another solution.
