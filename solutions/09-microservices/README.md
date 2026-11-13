# 09. Microservices

![Chapter learning and experiment diagram](09-microservices-overview.svg)

> Chapter guide in the *Foundations of Scalable Systems* learning track. This repository guide paraphrases topics and proposes an independent practical exercise; it does not reproduce the book.

## Goal

Turn this chapter's design question into a small, repeatable experiment. The result is a comparison with stated assumptions, not a universal technology ranking.

## Chapter focus

Service decomposition; monolith boundaries; deployment; resilience; cascading failures; bulkheads.

## Chapter synthesis

**Source reading: printed pages 163–181.** Original learning summary of the chapter's ideas; consult the book for full treatment.

The chapter defines a microservice by the business capability and ownership boundary it encapsulates, not simply by small code size. A monolith keeps modules and often a shared database in one deployable unit: it is straightforward to build, test, monitor, and release, and can scale up or be replicated behind a load balancer. As the codebase and traffic grow, however, teams may slow down and the entire application must be replicated even when only one capability is hot. Microservices allow a cohesive capability to own its API, data, deployment, and scaling independently, and teams can choose an appropriate internal stack. These gains require stable contracts, automation, observability, and organizational ownership.

Decomposition is a consequential design choice. Domain-driven design bounded contexts are a useful starting point, but a theoretically clean split can create too many network calls for one user operation. Calls add latency, partial failure, and coordination; sometimes merging two services or maintaining a read-optimized copy of selected data is better. Duplicated data buys locality and performance at the cost of storage and eventual-convergence logic. For a workflow spanning services, choreography lets peers coordinate through direct or asynchronous messages, while orchestration puts workflow state and coordination in a dedicated component. Orchestration simplifies progress visibility but can become a bottleneck or failure point; choreography avoids a central coordinator but spreads workflow knowledge and diagnosis across participants.

An API gateway offers a stable client-facing facade while backend services evolve. It can route/proxy calls, authenticate and authorize, rate-limit, cache, and emit usage/latency/error telemetry. It is itself a scaling and availability concern: deploy it redundantly and measure its capacity rather than assuming the gateway is free. The chapter's service principles include domain-oriented boundaries, high observability, hiding implementation behind contracts, independent deployment, automation, and fault isolation.

At load, a slow dependency can trigger a cascading failure. Waiting requests occupy threads and connections upstream; queues grow, deadlines expire, and immediate retries amplify demand. Use timeouts aligned with measured tail latency, fail fast or throttle when capacity is exhausted, and use exponential backoff with jitter only where retry is safe and bounded. A circuit breaker stops calls while a dependency is unhealthy, probes recovery in half-open state, and resumes when success returns. Bulkheads reserve separate concurrency/thread/connection budgets so a traffic surge on one operation cannot starve unrelated operations. When business semantics allow it, a cached or default response can preserve degraded read service. These controls need metrics and logs so rejected, timed-out, and short-circuited work is visible.

### Key takeaways

- Decompose by cohesive ownership, not fashion.
- Network boundaries require timeout, failure handling, and observability.
- Bulkheads reduce blast radius; each service adds operational work.

### Coverage of chapter topics

- Monolith strengths and scaling/coordination limits; business-capability decomposition, independent scaling, and bounded contexts.
- Data ownership, service coupling, duplicated/read-model data trade-offs, and workflow orchestration versus choreography.
- API gateway as a client-facing facade, including routing, security, rate limits, caching, telemetry, and gateway bottlenecks.
- Service design principles: cohesive domain scope, observability, hidden implementation, fault isolation, independent deployment, and automation.
- Cascading failures, long-tail latency, timeouts, fail-fast/throttling, safe retry/backoff, circuit breakers, fallbacks, and bulkheads.

## Proposed application

- **Application / solution name:** `ServiceBoundaryResilienceLab`.
- **Namespace convention:** FoundationsOfScalableSystems.Chapter09
- **Independent solution folder:** solutions/09-microservices/
- **Current state:** learning guide and experiment design; implementation is not yet present.

## Practical exercise

Implement a checkout flow in two deployable shapes: a modular monolith and a three-service design (Checkout, Inventory, Payment) with owned data stores and an API gateway. First serve the workflow synchronously; then implement an asynchronous orchestrated workflow with durable state and compensating actions. Do not split services merely to maximize count; compare which boundary best matches the team's business ownership and the traffic hot spots.

1. Define API contracts, ownership of each record, workflow state transitions, timeout/deadline budget, and what partial completion means.
2. Establish the monolith baseline. Measure each endpoint and dependency separately, then repeat with one capability independently replicated.
3. Inject slow and failed Inventory/Payment responses. Add bounded timeouts, rate limits, a circuit breaker, and separate concurrency budgets; verify unrelated reads remain responsive.
4. Add safe retry only for idempotent operations. Use idempotency keys and explicit compensation for already committed steps; do not retry non-idempotent payment blindly.
5. Compare synchronous orchestration with message-based workflow coordination, measuring end-to-end completion and operational complexity as well as raw speed.

### Implementable solution choices

| Solution | Choose it when | Concrete shape | Main costs / caveats |
|---|---|---|---|
| Modular monolith with internal modules | Domain boundaries or team size are still changing; low operational overhead matters | ASP.NET Core modules with explicit interfaces → one deployment → one relational DB with module-owned schemas | Whole artifact scales/releases together; enforce module boundaries to prevent shared-table coupling |
| Independently scaled synchronous services | User requests need immediate responses and service boundaries are stable | API Gateway → Checkout → Inventory/Payment APIs; each service owns DB; deadlines, timeouts, breaker, bulkhead and fallback | Multi-hop tail latency; partial failure and data consistency are explicit design work |
| Asynchronous workflow with orchestration | Long-running checkout steps and recovery visibility matter | API + outbox → workflow orchestrator/state machine → Inventory/Payment workers → durable status API; idempotent steps and compensations | Eventual completion and workflow state storage; orchestrator requires HA and versioning |
| Event choreography | Services should react independently to domain events and coupling can be managed | Outbox events → Kafka topics → consumer groups maintain local projections; correlation IDs, idempotency and DLQ | Workflow logic is distributed; end-to-end tracing, replay and schema evolution need discipline |

Start with the simplest shape that meets the latency and ownership requirements. Add a service boundary when independent ownership, scaling, or release has measurable value; keep synchronous fan-out shallow and protect every remote call with an explicit resource/deadline budget.

## What to measure

End-to-end p50/p95/p99 latency, per-dependency latency, cross-service call count, saturation, timeout/rejection rate, breaker state, partial-failure behavior, deployment independence, and blast radius.

Report at least p50/p95/p99 latency, throughput, errors, CPU, memory, and behavior during one relevant failure. Mark any unmeasured claim as **Not measured**.

## Design review questions

- What is the bottleneck or quality attribute this design addresses?
- Which semantics must remain true under retry, restart, or partial failure?
- What new bottleneck or operational cost does the approach introduce?
- What result would make us reject this design?

## References

- Ian Gorton, *Foundations of Scalable Systems*, Chapter 09: “Microservices” — [O’Reilly chapter page](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch09.html).
- Supporting primary documentation: [official reference](https://learn.microsoft.com/en-us/azure/architecture/guide/architecture-styles/microservices).
- Repository-wide chapter map and implementation order: [Scalability roadmap](../../docs/SCALABILITY-ROADMAP-EN.md).

## Naming and independence

When implementation begins, keep this application's source, infrastructure, workload, and run instructions inside this folder. The namespace above is a proposed code namespace, not a Kubernetes namespace; choose a Kubernetes namespace from the same chapter slug only when this solution gets manifests. Do not share application code with another solution.
