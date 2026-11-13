# Independent Solution Layout Migration Implementation Plan

> **For agentic workers:** Execute the approved design task by task and track each completed step.

**Goal:** Move the existing Kafka/Kubernetes application into an independently runnable solution folder and leave a concise repository catalog and shared design guide at the root.

**Architecture:** Preserve the application’s internal relative layout under `solutions/14-scalable-event-driven-processing/`. Keep repository-level policies and cross-solution design material at the root; move implementation guides and assets beside the solution they describe.

**Tech Stack:** PowerShell, Git, Markdown, Docker Compose, .NET, React, Kubernetes YAML.

**Spec:** `docs/superpowers/specs/2026-11-13-independent-solution-layout-design.md`

## Global Constraints

- Do not change application behavior, schemas, dependencies, resource sizing, or benchmark claims.
- Keep generated root `artifacts/` untouched.
- Keep root `.gitignore`; move `.dockerignore` with the solution.
- Do not deploy to a cluster or push changes.
- Preserve English and Persian documentation.

## Review Focus

- Relative Docker Compose build contexts remain valid after moving as a unit.
- Kubernetes scripts resolve the solution root after relocation.
- Root and solution README links resolve in both languages.
- Generated outputs remain ignored from nested solution directories.
- Central design documents remain at root and do not retain broken local paths.

---

### Task 1: Relocate the existing runnable solution

**Files:** Move `src/`, `web/`, `infra/`, `compose.yaml`, `14-scalable-event-driven-processing.slnx`, `.dockerignore`, and solution-specific guide/assets into `solutions/14-scalable-event-driven-processing/`.

- [x] Create the destination and move only tracked solution files; leave root `artifacts/` untouched.
- [x] Keep Compose, Dockerfiles, SQL, Kubernetes manifests, and solution assets in their existing relative arrangement.

### Task 2: Establish repository and solution entry points

**Files:** Replace root `README.md` and `README.fa.md`; create solution-local README pair from the existing detailed guides.

- [x] Root READMEs explain the repository purpose, link the central system design, and list the event-driven solution.
- [x] Solution READMEs retain current setup and architecture material, link back to root system design, and use relocated guide paths.

### Task 3: Repair paths and ignore policy

**Files:** Root `.gitignore`, solution Kubernetes deploy script, moved guides, central system design docs.

- [x] Ensure nested build artifacts are ignored and deploy script resolves the solution root.
- [x] Update local links/path examples and central references in both languages.

### Task 4: Review the migrated tree

- [x] Inspect Git status and resulting tree for accidental moves or omissions.
- [x] Search for stale root-relative paths and validate local Markdown links and documented path anchors with targeted static checks; do not run application tests.

**Execution:** Native implementation in the current workspace; no commit or push included.
