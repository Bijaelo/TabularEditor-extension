# Task 09 — Data Source Management Utilities

Goal: add utilities to template/clone data sources, parameterize connection strings for environments, and validate reachability using standard .NET connectivity checks.

Branching/merging
- Branch `feature/data-source-utils`.
- Rebase on latest `main`; avoid UI overlap with other panels (use separate dialog/pane).

Scope
- Create/clone data source templates; support environment parameters (dev/test/prod) with safe storage.
- Validate connectivity (no credentials logged); lightweight reachability checks with timeout handling.
- Optional helpers to update model connections in bulk using templates/parameters.

Deliverables
- Data source utility functions + UI.
- Configuration guidance for environments and secrets handling.
- Docs/help notes describing usage and safety.

Tests (must run)
- Automated: parameter substitution/unit tests; connectivity checks against mock/local endpoints.
- Manual: clone/template a data source in a sample model; run reachability check; confirm no sensitive info is persisted in logs.

Out-of-scope
- Runtime modernization or installer changes.
