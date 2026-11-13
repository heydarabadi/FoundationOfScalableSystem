# Independent Solution Layout Design

> **Superseded naming amendment (2026-11-13):** The user expanded the approved structure to follow every chapter of *Foundations of Scalable Systems*. The implemented application is now `solutions/14-scalable-event-driven-processing/`, and the `solutions/01-...` through `solutions/16-...` directories each contain bilingual chapter guides and original SVG diagrams. Only Chapter 14 currently contains a runnable application; other chapter project/namespace names are proposals. The chapter 14 .NET projects and Kubernetes namespace use `ScalableEventDrivenProcessing` / `scalable-event-driven-processing`.

## Intent

Make this repository a catalog of independent, runnable approaches to scalability and high-load performance. The existing event-driven Kafka/Kubernetes implementation becomes one self-contained solution. Future approaches live beside it in their own solution folders.

The repository also keeps one central guide that explains the shared problem and compares the available approaches. Each solution has its own README and operational documentation, with links to the central guide.

## Current State

The repository currently has one application spread across the root: `src/`, `web/`, `compose.yaml`, `infra/`, `docs/`, and `14-scalable-event-driven-processing.slnx`. The root README contains the solution overview, setup instructions, architecture, API reference, and selected performance observations. The Kubernetes deploy script resolves the repository root relative to its own location and builds the application from root-relative paths. Compose build contexts are also rooted at the current repository root.

There are no other runnable solution folders today. The Lab and Performance Kubernetes profiles are two operating profiles of the existing Kafka-based solution, not separate architectural solutions.

## Goals

- Give every architectural solution its own source, web app if needed, dependencies, deployment files, performance workloads, and run instructions.
- Make each solution independently understandable and runnable from its own folder without importing code from another solution.
- Keep one central repository guide and system-design document that introduce the problem and compare the solutions.
- Keep root navigation concise: every listed solution links to its local README; every local README links back to the central system design.
- Preserve the behavior and learning materials of the existing Kafka/Kubernetes solution during relocation.
- Keep the existing English and Persian documentation pattern for the current solution and the root guide.

## Non-Goals

- Do not implement or scaffold unknown future architectures yet; add each when its workload and purpose are selected.
- Do not combine all implementations behind a shared runtime or shared application library.
- Do not turn the current Lab and Performance Kubernetes profiles into separate solutions; they remain profiles inside the event-driven solution.
- Do not claim that a solution reaches a particular throughput merely because its folder or performance profile exists.
- Do not change application behavior, schemas, dependencies, resource sizing, or benchmark claims as part of the move.

## Anti-Goals

- Avoid a root directory that accumulates multiple unrelated `src`, `compose`, and deployment trees.
- Avoid implicit cross-solution dependencies or commands that must be run from another solution's directory.
- Avoid duplicate copies of the central architecture comparison in each solution README; local READMEs link to the central guide and describe the local implementation.
- Avoid empty placeholder folders for solution types that have not been chosen.

## Approaches Considered

1. **A complete folder per solution (selected).** Move the current application and its deployment/docs into `solutions/14-scalable-event-driven-processing/`; add future solutions as siblings. Keep only the repository catalog and shared system-design material at the root. This provides clear boundaries and independent run paths, at the cost of a one-time path/documentation migration.
2. **Shared source with per-solution infrastructure.** Keep `src/` and `web/` at root and split only the deployment definitions. This reduces duplication but couples solutions to the same application, so it does not meet the request that they be fully separate.
3. **Keep the existing solution at root and put only future ones under `solutions/`.** This minimizes migration but gives the first solution a special layout and leaves the root crowded. It weakens the folder boundary the user requested.

The user selected complete independence. The exact folder name `14-scalable-event-driven-processing` is a proposed naming choice based on the current architecture and can be changed before implementation.

## Proposed Layout

```text
README.md                         # Main solution catalog and entry guide
README.fa.md                      # Persian catalog and entry guide
docs/
  SYSTEM-DESIGN-MASTER-EN.md       # Central problem framing and solution comparison
  SYSTEM-DESIGN-MASTER-FA.md
  superpowers/specs/               # Repository-level design documents
solutions/
  14-scalable-event-driven-processing/
    README.md                      # How to build/run this solution; links to central guide
    README.fa.md
    .dockerignore
    14-scalable-event-driven-processing.slnx
    compose.yaml
    src/
    web/
    infra/
      postgres/
      kubernetes/
      performance/
    docs/                          # Kafka/Kubernetes/performance implementation guides
      assets/
  <future-solution>/               # Added only when selected and implemented
    README.md
    ...                            # All code, infrastructure, workloads, and docs local
```

The root `.gitignore` remains repository-wide policy and must ignore generated output under any solution, including `**/artifacts/`, `**/bin/`, `**/obj/`, and `**/node_modules/`. The current `.dockerignore` moves with the event-driven solution because Compose and Docker build contexts belong to that solution.

## Documentation Boundaries

- Root `README.md` and `README.fa.md` become the main guide/catalog: explain the repository purpose, link the central system-design documents, and list each solution with its local README.
- The system-design master documents remain under root `docs/` and become the cross-solution guide. They describe common scalability concerns, trade-offs, and links to the implementations without duplicating setup instructions.
- Repository-level design specs remain under root `docs/superpowers/specs/`; they document the repository structure and are not tied to a single runnable solution.
- Current Kubernetes, performance, and solution-specific architecture documents move under `solutions/14-scalable-event-driven-processing/docs/` with their related assets.
- `solutions/14-scalable-event-driven-processing/README.md` and its Persian counterpart describe that solution's architecture, quick start, and local paths, then link to the central guide and its deeper local docs.
- Future solution README files follow the same pattern and are added to both root catalogs.

## Migration Scope

- Move the current solution's source, web app, Compose file, solution file, PostgreSQL initialization, Kubernetes manifests, k6 workloads, solution-specific docs, and solution-specific assets under `solutions/14-scalable-event-driven-processing/`.
- Rewrite the root README pair as catalogs; preserve current detailed setup and reference information in the solution README/docs rather than discarding it.
- Update path references in Compose, the Kubernetes deployment script, solution files, Docker instructions, all local documentation, image links, and both catalogs.
- Keep generated/untracked `artifacts/` output at the repository root untouched; adjust ignore rules so future generated artifacts inside a solution remain ignored.
- Preserve the existing Kubernetes Lab and Performance profiles and their dedicated-cluster guidance.
- Do not change remote Git configuration, deploy to a cluster, add credentials, or publish new applications.

## Constraints and Decisions Left to the Builder

- Keep paths within a solution internally relative so it can be copied or extracted without breaking its local build/deploy workflow.
- Commands in a solution's README must state that they run from that solution directory, or explicitly change into it first.
- Keep solution-specific namespace, Compose project name, and local ports documented so separate solutions can be run without accidentally colliding. Only change current values if required for this layout and describe the change.
- Update central design references and local documentation links in both languages.
- Use Git moves where content is relocated; preserve file history where Git can infer it.

## Acceptance Conditions

- The root contains repository-wide navigation, central system-design material and specs, repository policy/configuration, and `solutions/`; it no longer contains the existing solution's source, Compose stack, Kubernetes files, or solution-specific runbook.
- The current Kafka solution is independently buildable and deployable from its own folder using its own Compose and Kubernetes assets.
- The root catalogs link to the solution READMEs; each solution README links to the central system design and its own detailed docs.
- No solution requires source code or infra files from a sibling solution.
- Internal links and documented commands use the new locations; existing generated artifacts and unrelated workspace data remain untouched.

## Approval Gate

This document describes the proposed scope and layout only. No project files should be relocated or implementation paths changed until the user approves this design. After approval, prepare a concrete migration plan, then execute and review the moves and path updates.

## Builder Check Resolution

- Keep this and future repository-level design documents under `docs/superpowers/specs/`. The root `docs/` directory is the home for central cross-solution material and repository structure specs; solution-specific guides stay under the corresponding solution folder.
