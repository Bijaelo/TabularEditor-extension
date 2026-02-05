# PropertyGrid Property/Type Locations (Detailed)

This file lists each property/type referenced in the discovery summary and where it is defined or surfaced.

## PropertyGrid control + binding
- NavigatablePropertyGrid
  - File: `TabularEditor/PropertyGrid/NavigatablePropertyGrid.cs`
  - Type: `TabularEditor.PropertyGridExtension.NavigatablePropertyGrid : System.Windows.Forms.PropertyGrid`

- Selection binding to PropertyGrid
  - File: `TabularEditor/UI/UIController_PropertyGrid.cs`
  - Method: `PropertyGrid_UpdateFromSelection()`
  - Behavior:
    - Single-select: sets `UI.PropertyGrid.SelectedObject`
    - Multi-select: sets `UI.PropertyGrid.SelectedObject = new MultiSelectProxy(...)`

- Multi-select bypass (proxy descriptor)
  - File: `TabularEditor/PropertyGrid/MultiSelectProxy.cs`
  - Type: `TabularEditor.PropertyGridExtension.MultiSelectProxy : ICustomTypeDescriptor`
  - Behavior: provides a merged property list that explicitly preserves OLS/RLS in multi-select.

## Property discovery + filtering
- DynamicPropertyConverter (TypeConverter)
  - File: `TOMWrapper/PropertyGridUI/DynamicPropertyObject.cs`
  - Type: `DynamicPropertyConverter : ExpandableObjectConverter`
  - Role: builds property list, applies multi-select filtering, uses `IDynamicPropertyObject` hooks

- NoMultiselectAttribute
  - File: `TOMWrapper/PropertyGridUI/NoMultiselectAttribute.cs`
  - Role: marks properties removed from multi-select property list

- DynamicTypeDescriptor (TypeDescriptionProvider)
  - File: `TOMWrapper/PropertyGridUI/DynamicTypeDescriptor.cs`
  - Role: filters properties based on NoMultiselect + HidePropertyAttribute

- IDynamicPropertyObject
  - File: `TOMWrapper/PropertyGridUI/DynamicPropertyObject.cs`
  - Methods: `Browsable(string)`, `Editable(string)`
  - Implemented by: `TabularObject` base class

- TabularObject.Browsable / IsBrowsable
  - File: `TOMWrapper/TOMWrapper/TabularObject.cs`
  - Method: `Browsable(string propertyName)` -> `PowerBIGovernance.VisibleProperty` -> `IsBrowsable`

- PowerBIGovernance.VisibleProperty
  - File: `TOMWrapper/TOMWrapper/PowerBI/PowerBIGovernance.cs`
  - Role: governance-level property visibility gate

## OLS/RLS properties and indexers
- Table.ObjectLevelSecurity (OLS)
  - File: `TOMWrapper/TOMWrapper/Table.cs`
  - Property: `public TableOLSIndexer ObjectLevelSecurity { get; ... }`
  - Attributes: DisplayName "Object Level Security", Category "Translations, Perspectives, Security"
  - IsBrowsable gate: `Table.IsBrowsable` checks compat >= 1400 and `Model.Roles.Any()`

- Column.ObjectLevelSecurity (OLS)
  - File: `TOMWrapper/TOMWrapper/Column.cs`
  - Property: `public ColumnOLSIndexer ObjectLevelSecurity { get; private set; }`
  - Attributes: DisplayName "Object Level Security", Category "Translations, Perspectives, Security"
  - IsBrowsable gate: `Column.IsBrowsable` checks compat >= 1400 and `Model.Roles.Any()`

- Table.RowLevelSecurity (RLS)
  - File: `TOMWrapper/TOMWrapper/Table.cs`
  - Property: `public TableRLSIndexer RowLevelSecurity { get; private set; }`
  - Attributes: DisplayName "Row Level Security", Category "Translations, Perspectives, Security"
  - IsBrowsable gate: `Table.IsBrowsable` checks `Model.Roles.Any()`

- TableOLSIndexer
  - File: `TOMWrapper/TOMWrapper/Indexers/TableOLSIndexer.cs`
  - Base: `GenericIndexer<ModelRole, MetadataPermission>`
  - Purpose: OLS across all roles for a single table

- ColumnOLSIndexer
  - File: `TOMWrapper/TOMWrapper/Indexers/ColumnOLSIndexer.cs`
  - Base: `GenericIndexer<ModelRole, MetadataPermission>`
  - Purpose: OLS across all roles for a single column

- TableRLSIndexer
  - File: `TOMWrapper/TOMWrapper/Indexers/TableRLSIndexer.cs`
  - Base: `GenericIndexer<ModelRole, string>`
  - Purpose: RLS across all roles for a single table

- RoleOLSIndexer
  - File: `TOMWrapper/TOMWrapper/Indexers/RoleOLSIndexer.cs`
  - Base: `GenericIndexer<Table, MetadataPermission>`
  - Purpose: OLS across all tables for a single role

- RoleRLSIndexer
  - File: `TOMWrapper/TOMWrapper/Indexers/RoleRLSIndexer.cs`
  - Base: `GenericIndexer<Table, string>`
  - Purpose: RLS across all tables for a single role

## Indexer visualization
- IndexerConverter (ExpandableObjectConverter)
  - File: `TOMWrapper/PropertyGridUI/DictionaryProperty.cs`
  - Role: renders expandable indexers (OLS/RLS)
  - Responsibilities:
    - Create per-role child properties
    - `ConvertTo` provides top-level display text
    - Multi-select guards added to avoid NREs

- DictionaryPropertyDescriptor
  - File: `TOMWrapper/PropertyGridUI/DictionaryProperty.cs`
  - Role: per-role leaf property in the PropertyGrid

## Category / UI grouping
- Category "Translations, Perspectives, Security"
  - Used by OLS/RLS properties (see Table.cs / Column.cs)
  - Display grouping for PropertyGrid categories

## Notes on multi-select merging
- PropertyGrid internal merge logic drops any property that is:
  - Not present on all objects
  - Not browsable on any object
  - Not merge-compatible (descriptor mismatch: attributes/read-only/browsable/component type)
- These rules are in WinForms internal classes, not in this repo.
- The proxy bypass replaces WinForms merge when enabled, so the property list is controlled by `MultiSelectProxy`.
