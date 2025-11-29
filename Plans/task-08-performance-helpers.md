# Task 08 — Performance Helpers

Goal: import VPAX/DAX Studio exports or DMV snapshots and surface table/column/store metrics plus basic timing summaries; keep dependency on external tools minimal.

Branching/merging
- Branch `feature/performance-helpers`.
- Rebase on latest `main`; no UI conflicts expected, but use separate panel.

Scope
- Parser for VPAX or DMV snapshot exports (file-based).
- UI to display size/cardinality/store metrics and basic timing summaries; filtering/sorting for usability.
- Ensure non-blocking parsing and safe file handling (no credential capture).

Deliverables
- Parsing utilities and performance panel.
- Sample VPAX/DMV files for tests.
- Docs on supported inputs and how to import.

Tests (must run)
- Automated: parse sample exports and assert extracted metrics.
- Manual: import sample file, verify metrics display; check UI responsiveness on large files.

Out-of-scope
- Live Profiler-like capture (requires external tools/decisions by user).
