# Task 05 — Scripting UX Upgrades

Goal: add a docked script console with syntax highlighting, saved snippets, and structured log/output. Provide script templates for common tasks.

Branching/merging
- Branch `feature/scripting-console`.
- Rebase on latest `main`; UI should avoid conflicts with panels from Tasks 02–04 (use separate dock pane).

Scope
- Script console with syntax highlighting and execution controls; non-blocking execution (avoid UI freezes).
- Saved snippet library (persisted) with tagging/categorization.
- Structured log output with timestamps and script result status; optional CSV/JSON export.
- Include starter scripts/templates (bulk role changes, deployment diffs, search helpers).

Deliverables
- New console UI + backend execution wiring.
- Snippet storage and retrieval.
- Documentation for using console and templates.

Tests (must run)
- Automated: basic script execution path unit tests; snippet persistence tests.
- Manual: run sample scripts, verify logs capture output; confirm UI remains responsive during long scripts.

Out-of-scope
- Deployment diff logic (Task 06) itself—only templates/hooks here.
