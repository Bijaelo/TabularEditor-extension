# PropertyGrid + OLS/RLS Discovery Summary

## PropertyGrid UI + binding
- The visual control is `TabularEditor.PropertyGridExtension.NavigatablePropertyGrid`, a thin subclass of WinForms `PropertyGrid`.
- Selection wiring happens in `TabularEditor/UI/UIController_PropertyGrid.cs` via `PropertyGrid_UpdateFromSelection()`, which sets:
  - Single-select: `UI.PropertyGrid.SelectedObject = ...`
  - Multi-select: `UI.PropertyGrid.SelectedObject = new MultiSelectProxy(...)` (proxy merge bypass)

## How properties are discovered and filtered
- Properties come from .NET TypeDescriptor/TypeConverter for the selected objects.
- `DynamicPropertyConverter` (`TOMWrapper/PropertyGridUI/DynamicPropertyObject.cs`) filters the property list:
  - Honors `[NoMultiselect]` in multi-select scenarios.
  - Uses `IDynamicPropertyObject` to decide `Browsable`/`Editable`.
- `TabularObject.Browsable()` and `IsBrowsable()` are the final gates, including:
  - Governance (`PowerBIGovernance.VisibleProperty`).
  - Compatibility level and role presence checks (e.g., OLS requires roles and compat >= 1400).

## OLS/RLS structure
- OLS/RLS are not scalar properties; they are expandable indexers:
  - Table OLS: `Table.ObjectLevelSecurity` -> `TableOLSIndexer`
  - Column OLS: `Column.ObjectLevelSecurity` -> `ColumnOLSIndexer`
  - Table RLS: `Table.RowLevelSecurity` -> `TableRLSIndexer`
- Expansion is handled by `IndexerConverter` in `TOMWrapper/PropertyGridUI/DictionaryProperty.cs`, which creates a sub-property per role.

## Multi-select failure (root cause, updated)
- The OLS/RLS properties exist and are returned by `DynamicPropertyConverter` for each selected object.
- The row is dropped by the **WinForms PropertyGrid merge logic**, not by governance or `[NoMultiselect]`.
- WinForms merge is stricter than “same name + same type” and compares descriptor metadata:
  - Attributes (e.g., browsable, read-only, type converter).
  - ComponentType and per-object descriptor identity.
  - Per-object `Browsable` / `Editable` differences (roles/compat gating).
- Because `DynamicPropertyDescriptor` is computed per object, OLS/RLS can become **descriptor‑incompatible** across the selection and are removed during merge.

## Observed side‑effects
- `IndexerConverter.ConvertTo(...)` does not run because the OLS/RLS row never makes it into the merged list, so the placeholder text (“Multiple objects selected.”) is never shown.
- Switching from multi‑select to single‑select may tear down internal editor threads; normal WinForms behavior (exit code 0) and not an error.

## Remediation history
- Added guards in `IndexerConverter.GetProperties(...)` to avoid NREs on multi‑select.
- Result: no exceptions, but OLS/RLS row still missing because WinForms merge drops it.

## Current remediation (implemented)
- **MultiSelectProxy is now an implemented feature** that replaces WinForms merge during multi‑select:
  - `TabularEditor/PropertyGrid/MultiSelectProxy.cs` implements `ICustomTypeDescriptor`.
  - `PropertyGrid_UpdateFromSelection()` uses `SelectedObject = new MultiSelectProxy(...)` for multi‑select.
  - The proxy explicitly preserves OLS/RLS in the merged list when type‑compatible.
- **Behavioral difference vs previous implementation**:
  - Previous: WinForms merge could drop OLS/RLS rows; placeholder text never appeared.
  - Now: OLS/RLS rows are reliably present in multi‑select and show the placeholder text.

## Files touched/inspected
- `TabularEditor/PropertyGrid/NavigatablePropertyGrid.cs`
- `TabularEditor/UI/UIController_PropertyGrid.cs`
- `TabularEditor/PropertyGrid/MultiSelectProxy.cs`
- `TOMWrapper/PropertyGridUI/DynamicPropertyObject.cs`
- `TOMWrapper/PropertyGridUI/DictionaryProperty.cs`
- `TOMWrapper/TOMWrapper/TabularObject.cs`
- `TOMWrapper/TOMWrapper/Table.cs`
- `TOMWrapper/TOMWrapper/Column.cs`

## Options to keep standard WinForms merge (mitigations)
1. **Descriptor normalization** (preferred if avoiding proxy):
   - Ensure OLS/RLS descriptors are identical across objects (consistent attributes/read‑only/browsable).
   - Add explicit `[MergableProperty(true)]` and consistent `[Browsable(true)]` on OLS/RLS.
   - Avoid per‑object `Editable`/`Browsable` variance when multi‑select is active.
2. **Conditional proxy only for known problem cases**:
   - Use proxy only when selection contains OLS/RLS (or other known problematic properties).
   - Keep standard `SelectedObjects` for all other selections.
3. **Custom TypeDescriptionProvider for OLS/RLS only**:
   - Replace only the OLS/RLS descriptors with merge‑stable descriptors.
   - Leave the rest to WinForms merge.

## Relationship to original bulk-edit goal
- Bulk-editing OLS/RLS depends on the PropertyGrid showing the OLS/RLS property row during multi-select.
- The proxy strategy restores the row and placeholder, unblocking the multi‑select UI surface.
- True bulk‑edit behavior still requires specific setter logic for OLS/RLS across multiple objects.
