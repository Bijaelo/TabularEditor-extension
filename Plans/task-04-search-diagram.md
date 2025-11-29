# Task 04 — Search, Bulk Rename, and Dependency/Diagram View

Goal: build a “Model Search” panel for scoped find/replace plus dependency exports/visualizer for tables/relationships/DAX lineage, leveraging Task 01 utilities.

Branching/merging
- Branch `feature/search-dependencies`.
- Rebase on latest `main` and Task 01; coordinate with Task 02/03 to avoid UI conflicts (use separate panels/docks).

Scope
- Search panel indexing names and expressions; scoped find/replace (names, expressions, selected object types) with dry-run preview.
- Bulk rename with safety checks and undo/cancel paths where possible.
- Dependency export (Graphviz/DGML/HTML) showing relationships and DAX lineage; embed a read-only visualizer in-app.

Deliverables
- Search/rename UI + logic.
- Dependency export functions and embedded viewer.
- Docs/help notes for search scopes and export formats.

Tests (must run)
- Automated: rename dry-run tests on fixtures; dependency export correctness using known models.
- Manual: search/replace on sample model; view dependency graph; ensure no corruption of model save.

Out-of-scope
- Lint rules (Task 03) and editor changes (Task 02).
