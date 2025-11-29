# Task 07 — Roles and Permissions Ergonomics

Goal: improve role management with bulk-create/clone, filter permissions, and validation of row filters; small UI for batch editing.

Branching/merging
- Branch `feature/roles-permissions`.
- Rebase on latest `main`; coordinate with Task 03 (lint rules) to avoid duplicate validation logic.

Scope
- Bulk-create/clone roles; batch-apply filters to tables; clone role filters.
- Validation of role expressions with clear errors; optional lint integration hook.
- Compact UI for selecting tables/filters and applying changes safely.

Deliverables
- Role management UI + logic.
- Validation routines (reuse shared utilities where possible).
- Docs/help notes for workflows and safety/undo guidance.

Tests (must run)
- Automated: role cloning retains filters; validation catches malformed expressions; batch apply works on fixtures.
- Manual: create/clone roles on sample model; verify filters apply and save; confirm no data loss in other role settings.

Out-of-scope
- Deployment diff engine (Task 06) and performance metrics (Task 08).
