# Task 03 — Validation and Linting

Goal: expand Best Practice Analyzer/lint rules using shared metadata utilities; surface results in a docked panel and (where safe) inline indicators.

Branching/merging
- Branch `feature/validation-linting`.
- Rebase on latest `main` and depend on merged Task 01 utilities; avoid UI overlap with Task 02 editor changes.

Scope
- Author rule set covering naming conventions, unused columns/measures, expensive patterns, role expression checks, and other BPA-worthy heuristics.
- Add execution pipeline and docked results panel; optional inline badges where low-risk.
- Ensure rules are configurable/override-friendly (no hard-coded org standards).

Deliverables
- Rule definitions and engine wiring.
- UI for viewing rule results; clear messages and severities.
- Docs/update on how to run and extend rules.

Tests (must run)
- Automated: rule execution against sample models; verify expected violations detected; ensure no crashes on empty/large models.
- Manual: run lint on a real model; confirm navigation from result to object works.

Out-of-scope
- Editor-specific IntelliSense (Task 02) or search/rename (Task 04).
