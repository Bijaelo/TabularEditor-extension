# Tabular Editor 3 Parity Work Plan (for Tabular Editor 2)

Plan the work as independent feature branches that can merge cleanly to `main`. Keep branches small, rebase on `main` before opening a PR, and run the listed tests before finishing each task.

## Branching and coordination
- Create one branch per task: `feature/<area>-<short-desc>` (e.g., `feature/dax-editor-intellisense`).
- Before finalizing, rebase onto the latest `main` and resolve conflicts locally; do not force-push over others’ work.
- Prefer shared utilities (metadata index, dependency extraction) to avoid rework across tasks.

## Task sequencing (ordered to minimize rework)
1) **Scaffolding and shared utilities**
   - Add a lightweight test harness (unit + integration) for TOM-based helpers.
   - Implement reusable metadata indexers: objects, expressions, dependencies (tables, columns, measures, roles).
   - Tests: unit tests for index accuracy; sample model fixtures.
2) **DAX editing improvements**
   - Swap the expression editor for a modern host (e.g., Monaco) with syntax highlighting, bracket matching, and basic completion using the metadata index + DAX parser.
   - Add hover/quick info from local metadata; optional signature hints for common functions.
   - Tests: manual smoke (completions, hovers), automated snapshot tests for the completion provider.
3) **Validation and linting**
   - Expand Best Practice Analyzer rule set (naming, unused columns/measures, expensive patterns, role expression checks).
   - Wire rule results into a docked panel and inline badges where feasible.
   - Tests: rule execution against sample models; ensure no crashes on large models.
4) **Search, bulk rename, and dependency/diagram view**
   - Build a “Model Search” panel using the shared index with scoped find/replace (names, expressions).
   - Generate dependency exports (Graphviz/DGML/HTML) for tables/relationships/DAX lineage; read-only visualizer in-app.
   - Tests: rename dry-runs, dependency export correctness on sample models.
5) **Scripting UX upgrades**
   - Add a docked script console with syntax highlighting, saved snippets, and structured log/output.
   - Provide script templates for common tasks (bulk role changes, deployment diffs).
   - Tests: scripts run without UI freeze; log persists across sessions.
6) **Deployment and comparison helpers**
   - Add TOM-based metadata diff (file-to-file/workspace-to-file) with a readable report; generate deployment plans.
   - Optional: CLI entry point to run diffs in CI.
   - Tests: diff on known fixtures; verify idempotent deploy plan generation.
7) **Roles and permissions ergonomics**
   - Bulk-create/clone roles, filter permissions, validate row filters; small UI to batch-edit.
   - Tests: role cloning retains filters; validation catches malformed expressions.
8) **Performance helpers**
   - Import VPAX/DAX Studio exports or DMV snapshots; surface table/column/store metrics and basic timing summaries.
   - Tests: parse sample VPAX/DMV exports; verify metrics render without blocking UI.
9) **Data source management utilities**
   - Templates for data sources, connection-string parameterization (dev/test/prod), reachability checks with standard .NET connectivity.
   - Tests: connection validation against mock/local endpoints; ensure no credentials are logged.

## Tasks that likely require user involvement
- **Runtime modernization (.NET 6/7, high-DPI/modern UI controls)**: full replatform outside scope of incremental scripting; would need coordinated migration and installer changes.
- **Proprietary/live diagnostics (integrated Profiler-like capture)**: relies on external tools or licensed components; user must decide acceptable external dependency path (e.g., continue to use DAX Studio/SQL Profiler).
- **Acceptance in target environments**: user should perform final validation against their production models/servers and sign off on any deployment automation.

## Test expectations (per branch)
- Run automated tests added in Task 1 and area-specific tests above.
- Manual acceptance: edit expressions, run lint rules, execute scripts, search/rename, view dependency graph, run diff/deploy dry-run, and import a VPAX sample.
- Verify no regressions in model save/open and existing relationship diagram.
