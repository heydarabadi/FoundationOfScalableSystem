# Scalability and High-Load Solutions

This repository is a book-led learning and benchmarking catalog for scalability and high-performance programming. Its learning sequence follows Ian Gorton’s *Foundations of Scalable Systems*; each independent solution applies selected ideas and reports its own measurements and limits. The book provides the concepts, while this repository provides runnable experiments.

## Learning path

- [Scalability roadmap, chapter map, and proposed solution tracks](docs/SCALABILITY-ROADMAP-EN.md) · [نقشهٔ راه فارسی](docs/SCALABILITY-ROADMAP-FA.md)
- [Official book page at O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/)

## Shared architecture guide

- [System Design — English](docs/SYSTEM-DESIGN-MASTER-EN.md)
- [طراحی سیستم — فارسی](docs/SYSTEM-DESIGN-MASTER-FA.md)

## Solutions

| Approach | English guide | راهنمای فارسی |
|---|---|---|
| Event-driven processing with Kafka and Kubernetes | [Event-driven Kafka](solutions/14-scalable-event-driven-processing/README.md) | [Kafka و Kubernetes](solutions/14-scalable-event-driven-processing/README.fa.md) |

Each solution is self-contained. Start with its README for prerequisites, local run instructions, infrastructure, and performance profiles. New approaches will be added as sibling folders under `solutions/` when selected and implemented.
