# Metadata utilities and tests

## Index API
- `TabularMetadataIndex` builds lightweight dictionaries over tables, columns, measures, hierarchies, roles and named expressions. Keys are prefixed (for example `Measure:Sales[Total Sales]`) to avoid collisions.
- Dependencies are captured per expression with `TabularMetadataExpression.Dependencies`, which lists referenced tables/columns/measures/roles as `TabularMetadataReference`.
- Use the helper methods on `TabularMetadataKey` to build stable keys.

### Example
```csharp
using var handler = new TabularModelHandler("Model.bim");
var index = new TabularMetadataIndex(handler.Model);

var measure = index.Measures[TabularMetadataKey.Measure("Sales", "Total Sales")];
var deps = measure.Expressions.Single().Dependencies.Select(d => d.Key);
```

Pass `rebuildDependencies: false` to `TabularMetadataIndex` if you manage dependency trees yourself.

## Test fixture helper
- `MetadataTestModelFactory.Create()` (in `TOMWrapperTest/Metadata`) builds a small in-memory model with tables, measures, hierarchies and role filters. It is used by the metadata index tests and can be reused for manual inspection.
- Manual sanity check: create the fixture, build an index and inspect dependencies:
```csharp
using var handler = MetadataTestModelFactory.Create();
var index = new TabularMetadataIndex(handler.Model);
var roleFilter = index.Roles[TabularMetadataKey.Role("SalesReaders")].Expressions;
```

## Running the metadata tests
- Run the focused test suite (requires restored NuGet packages):
  - `dotnet test TOMWrapperTest/TOMWrapperTest.csproj --filter Metadata`
- The tests validate index coverage, dependency extraction and role filter handling on the fixture model.
