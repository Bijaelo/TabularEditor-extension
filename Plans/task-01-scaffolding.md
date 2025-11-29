# Task 01 — Scaffolding and Shared Utilities

Another agent was given the task "Scaffoling and shared utilities". It did not succeed in completing the following 3 subtasks that is expected:
```text
1. Review existing TOMWrapper utilities/tests and design metadata index + fixture approach
2. Implement metadata index utilities and sample fixture assets
3. Add tests validating index accuracy/depeendencies and update docs/run checks.
```
Continue where it left off and try to complete its tasks.

It was given the following information regarding the tasks.

Goal: add reusable metadata utilities (objects, expressions, dependencies) plus a lightweight test harness for TOM-based helpers. Keep scope small and ready to merge cleanly into `main`.

Branching/merging
- Create branch `feature/scaffolding-metadata-utils`.
- Rebase on latest `main` before opening PR; resolve conflicts locally (no force-push over others).
- Keep changes isolated to utilities/tests; avoid UI changes in this task.

Scope
- Add unit/integration test harness (e.g., xUnit/NUnit or existing project conventions) runnable locally/CI.
- Implement metadata indexers for tables, columns, measures, hierarchies, roles; capture expressions and dependencies (tables/columns/measures/roles).
- Provide sample model fixtures for tests.
- Expose a simple API for downstream tasks (search, completion, diagrams, lint).

Deliverables
- New utility modules/classes with clear public surface.
- Tests covering index accuracy and dependency extraction using fixtures.
- Brief README/update (if applicable) describing how to run tests and consume utilities.

Tests (must run)
- All new unit/integration tests.
- Manual sanity: load fixture model through utilities and verify dependency output looks correct.

Out-of-scope
- UI wiring, editor changes, or linting rules.

Notes for next tasks
- Ensure APIs are stable and documented to minimize downstream changes.
- Keep test fixtures reusable by later tasks.
