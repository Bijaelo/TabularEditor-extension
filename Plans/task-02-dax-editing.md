# Task 02 — DAX Editing Improvements

Goal: upgrade the expression editor with a modern host (e.g., Monaco) plus syntax highlighting, bracket matching, and metadata-backed completion/hover. Use utilities from Task 01.

Branching/merging
- Branch `feature/dax-editor-intellisense`.
- Rebase on latest `main` (and Task 01 once merged) before PR; resolve conflicts locally.
- Keep editor changes modular to avoid conflicts with other UI work.

Scope
- Integrate a modern text editor control for DAX editing (keep existing docking/layout conventions).
- Implement completion provider using metadata index; include table/column/measure symbols and common DAX functions.
- Add hover/quick info from local metadata; optional signature hints for common functions if feasible.
- Maintain theme/readability; ensure no regression in existing editor behaviors (save/apply).

Deliverables
- Updated editor component with completion + hover + bracket matching.
- Wiring to metadata utilities from Task 01.
- Short usage notes in README/help (if applicable).

Tests (must run)
- Automated: snapshot or unit tests for completion provider (expected suggestions for known contexts).
- Manual: open model, edit a measure, verify completions/hover, confirm expressions save correctly; smoke on large model for performance.

Out-of-scope
- Full proprietary TE3 IntelliSense parity; keep to non-proprietary features.
- Validation/lint rules (Task 03).
