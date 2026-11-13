# 05. Application Services

![Chapter learning and experiment diagram](05-application-services-overview.svg)

> Chapter guide in the *Foundations of Scalable Systems* learning track. This repository guide paraphrases topics and proposes an independent practical exercise; it does not reproduce the book.

## Goal

Turn this chapter's design question into a small, repeatable experiment. The result is a comparison with stated assumptions, not a universal technology ranking.

## Chapter focus

API and service design; state management; application servers; horizontal scaling; load balancing; health; elasticity; session affinity.

## Chapter synthesis

**Source reading: printed pages 93–113.** Original learning summary of the chapter's listed ideas; consult the book for full treatment.

An application service exposes business behavior through an API contract: operations, resource identifiers, inputs, outputs, and error responses. The chapter uses HTTP CRUD as a common pattern, emphasizing that verb plus URI defines operation semantics and payloads commonly use JSON. OpenAPI can document this contract. Avoid chatty designs that require many network round trips for one business action; use complete resource reads, deliberate updates, or PATCH for partial changes. Compress large payloads when network savings exceed compression CPU cost.

Application servers host request handlers, route incoming API calls, manage lifecycle, and commonly execute concurrent requests using thread pools. Their configuration creates a pipeline of finite resources: listener/backlog, worker threads and task queues, then database connections and query queues. A request may spend significant time waiting in those queues; too many threads add memory and context-switching costs, and unbounded queues can turn overload into growing latency and eventual resource exhaustion. Tune thread and connection pools to the real workload, preserve headroom below saturation, and observe latency, active work, and queue sizes. Ease of framework abstraction can trade against generated plumbing and efficiency.

Service-level state is another key choice. HTTP requests can be self-contained, but application “conversational” session state is often accidentally kept in one server’s memory. That consumes memory proportional to active sessions, requires expiry, and makes subsequent requests depend on the same instance. Stateless APIs put the needed identity and data in each request or externalize state to a shared store. Stateless instances can then scale horizontally behind a load balancer and a failed request can be retried at another healthy replica; replicas also remove one server as a single point of failure.

Load balancers may route at Layer 4 using network connections and NAT, or at Layer 7 by inspecting HTTP methods, headers, and paths; the richer Layer 7 proxy costs some overhead. Policies include round robin, least connections, header/operation routing, and weights for differently sized instances. Health checks remove failed or unhealthy targets. Elasticity changes replica count using schedules or live metrics with minimum/maximum bounds and warm-up time. Sticky sessions can preserve in-memory state, but long-lived uneven sessions cause imbalance and make failure recovery harder. Finally, adding service replicas can simply overload a downstream database or dependency; measure the whole request path and scale its limiting tier too.

### Key takeaways

- Make API contracts, state ownership, and failure behavior explicit.
- Readiness, liveness, load balancing, and elasticity are distinct mechanisms.
- Scaling an API requires downstream capacity too.

### Coverage of chapter topics

- Service/API contracts, design boundaries, and ownership of application state.
- Application servers, horizontal replication, load balancers, and distribution policies.
- Health monitoring, elasticity, and the throughput/failover costs of session affinity.

## Proposed application

- **Application / solution name:** `ScalableApplicationServicesLab`.
- **Namespace convention:** FoundationsOfScalableSystems.Chapter05
- **Independent solution folder:** solutions/05-application-services/
- **Current state:** learning guide and experiment design; implementation is not yet present.

## Practical exercise

Serve an identical API from one and multiple stateless instances; compare balancing and instance-local state behavior.

1. Write down the operation, workload shape, data size, and correctness invariant.
2. Establish a simple baseline before adding the chapter's scaling technique.
3. Change one major design choice at a time and repeat the same workload.
4. Save raw results, environment details, versions, and the exact run command.

## What to measure

Request rate, latency distribution, error rate, instance skew, and recovery after instance loss.

Report at least p50/p95/p99 latency, throughput, errors, CPU, memory, and behavior during one relevant failure. Mark any unmeasured claim as **Not measured**.

## Design review questions

- What is the bottleneck or quality attribute this design addresses?
- Which semantics must remain true under retry, restart, or partial failure?
- What new bottleneck or operational cost does the approach introduce?
- What result would make us reject this design?

## References

- Ian Gorton, *Foundations of Scalable Systems*, Chapter 05: “Application Services” — [O’Reilly chapter page](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch05.html).
- Supporting primary documentation: [official reference](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/).
- Repository-wide chapter map and implementation order: [Scalability roadmap](../../docs/SCALABILITY-ROADMAP-EN.md).

## Naming and independence

When implementation begins, keep this application's source, infrastructure, workload, and run instructions inside this folder. The namespace above is a proposed code namespace, not a Kubernetes namespace; choose a Kubernetes namespace from the same chapter slug only when this solution gets manifests. Do not share application code with another solution.
