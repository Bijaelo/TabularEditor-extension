# Task 06 — Deployment and Comparison Helpers

Goal: add TOM-based metadata diff (file↔file, workspace↔file) with readable reports and optional deployment plan generation; optional CLI entry point for CI.

Branching/merging
- Branch `feature/deploy-diff`.
- Rebase on latest `main`; coordinate with Task 05 for any shared scripts/templates (but keep UI separate).

Scope
- Diff engine comparing metadata (tables, measures, roles, partitions, relationships, translations, perspectives).
- Human-readable report + machine-readable output (JSON) for CI/automation.
- Deployment plan generator (idempotent) for apply/dry-run; expose minimal UI and CLI entry (if feasible).

Deliverables
- Diff engine + report generation.
- UI hooks (panel/dialog) for running diffs; optional CLI command.
- Docs on running diffs, interpreting output, and using deployment plans.

Tests (must run)
- Automated: diff on known fixtures with expected outputs; idempotence checks on repeated plans.
- Manual: run diff against two sample models; verify report clarity and no unintended model changes on dry-run.

Out-of-scope
- Live server deployments beyond TOM apply/dry-run; keep to metadata diff/apply.
