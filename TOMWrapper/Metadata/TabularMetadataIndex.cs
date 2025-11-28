using System;
using System.Collections.Generic;
using System.Linq;
using TabularEditor.TOMWrapper.Utils;

namespace TabularEditor.TOMWrapper.Metadata
{
    public enum TabularMetadataObjectType
    {
        Table,
        Column,
        Measure,
        Hierarchy,
        Role,
        Expression,
        Unknown
    }

    public record TabularMetadataReference(
        TabularMetadataObjectType Type,
        string Key,
        string Name,
        string Table)
    {
        public string Label => string.IsNullOrEmpty(Table) ? Name : Table + "." + Name;
    }

    public record TabularMetadataExpression(
        string Property,
        string Expression,
        IReadOnlyList<TabularMetadataReference> Dependencies);

    public record TabularMetadataObject(
        string Key,
        TabularMetadataObjectType Type,
        string Name,
        string Table,
        IReadOnlyList<string> Children,
        IReadOnlyList<TabularMetadataExpression> Expressions);

    public static class TabularMetadataKey
    {
        public static string Table(string tableName) => "Table:" + tableName;
        public static string Column(string tableName, string columnName) => "Column:" + tableName + "[" + columnName + "]";
        public static string Measure(string tableName, string measureName) => "Measure:" + tableName + "[" + measureName + "]";
        public static string Hierarchy(string tableName, string hierarchyName) => "Hierarchy:" + tableName + "::" + hierarchyName;
        public static string Role(string roleName) => "Role:" + roleName;
        public static string Expression(string expressionName) => "Expression:" + expressionName;
    }

    /// <summary>
    /// Builds lightweight indexes over a Tabular model for downstream tasks such as search, completion, diagrams or linting.
    /// Expressions are captured alongside their dependencies (tables, columns, measures, roles) so callers can reason about lineage without touching TOM directly.
    /// </summary>
    public sealed class TabularMetadataIndex
    {
        public IReadOnlyDictionary<string, TabularMetadataObject> Tables { get; }
        public IReadOnlyDictionary<string, TabularMetadataObject> Columns { get; }
        public IReadOnlyDictionary<string, TabularMetadataObject> Measures { get; }
        public IReadOnlyDictionary<string, TabularMetadataObject> Hierarchies { get; }
        public IReadOnlyDictionary<string, TabularMetadataObject> Roles { get; }
        public IReadOnlyDictionary<string, TabularMetadataObject> Expressions { get; }

        public TabularMetadataIndex(Model model, bool rebuildDependencies = true)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            if (rebuildDependencies && TabularModelHandler.Singleton != null)
            {
                // Ensure DependsOn collections are fresh before we read them.
                FormulaFixup.BuildDependencyTree();
            }

            Tables = model.Tables.ToDictionary(t => TabularMetadataKey.Table(t.Name), BuildTable);
            Columns = model.Tables
                .SelectMany(t => t.Columns.OfType<Column>())
                .ToDictionary(c => TabularMetadataKey.Column(c.Table.Name, c.Name), BuildColumn);
            Measures = model.Tables
                .SelectMany(t => t.Measures)
                .ToDictionary(m => TabularMetadataKey.Measure(m.Table.Name, m.Name), BuildMeasure);
            Hierarchies = model.Tables
                .SelectMany(t => t.Hierarchies)
                .ToDictionary(h => TabularMetadataKey.Hierarchy(h.Table.Name, h.Name), BuildHierarchy);
            Roles = model.Roles
                .ToDictionary(r => TabularMetadataKey.Role(r.Name), BuildRole);
            Expressions = model.Expressions
                .ToDictionary(e => TabularMetadataKey.Expression(e.Name), BuildExpression);
        }

        public static TabularMetadataIndex FromHandler(TabularModelHandler handler, bool rebuildDependencies = true)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return new TabularMetadataIndex(handler.Model, rebuildDependencies);
        }

        private static TabularMetadataObject BuildTable(Table table)
        {
            var children = new List<string>();
            children.AddRange(table.Columns.OfType<Column>().Select(c => TabularMetadataKey.Column(table.Name, c.Name)));
            children.AddRange(table.Measures.Select(m => TabularMetadataKey.Measure(table.Name, m.Name)));
            children.AddRange(table.Hierarchies.Select(h => TabularMetadataKey.Hierarchy(table.Name, h.Name)));

            return new TabularMetadataObject(
                TabularMetadataKey.Table(table.Name),
                TabularMetadataObjectType.Table,
                table.Name,
                table.Name,
                children,
                BuildDaxExpressions(table as IDaxDependantObject));
        }

        private static TabularMetadataObject BuildColumn(Column column)
        {
            return new TabularMetadataObject(
                TabularMetadataKey.Column(column.Table.Name, column.Name),
                TabularMetadataObjectType.Column,
                column.Name,
                column.Table.Name,
                Array.Empty<string>(),
                BuildDaxExpressions(column as IDaxDependantObject));
        }

        private static TabularMetadataObject BuildMeasure(Measure measure)
        {
            return new TabularMetadataObject(
                TabularMetadataKey.Measure(measure.Table.Name, measure.Name),
                TabularMetadataObjectType.Measure,
                measure.Name,
                measure.Table.Name,
                Array.Empty<string>(),
                BuildDaxExpressions(measure));
        }

        private static TabularMetadataObject BuildHierarchy(Hierarchy hierarchy)
        {
            var levels = hierarchy.Levels
                .Select(l => l.Column)
                .Where(c => c != null)
                .Select(c => TabularMetadataKey.Column(c.Table.Name, c.Name))
                .ToList();

            return new TabularMetadataObject(
                TabularMetadataKey.Hierarchy(hierarchy.Table.Name, hierarchy.Name),
                TabularMetadataObjectType.Hierarchy,
                hierarchy.Name,
                hierarchy.Table.Name,
                levels,
                Array.Empty<TabularMetadataExpression>());
        }

        private static TabularMetadataObject BuildRole(ModelRole role)
        {
            var expressions = new List<TabularMetadataExpression>();
            foreach (var permission in role.TablePermissions)
            {
                var property = "Filter (" + permission.Table.Name + ")";
                var permissionExpressions = BuildDaxExpressions(permission);
                if (permissionExpressions.Count > 0)
                {
                    var expression = permissionExpressions[0];
                    expressions.Add(new TabularMetadataExpression(property, expression.Expression, expression.Dependencies));
                }
            }

            return new TabularMetadataObject(
                TabularMetadataKey.Role(role.Name),
                TabularMetadataObjectType.Role,
                role.Name,
                null,
                Array.Empty<string>(),
                expressions);
        }

        private static TabularMetadataObject BuildExpression(NamedExpression expression)
        {
            var expressions = new List<TabularMetadataExpression>
            {
                new TabularMetadataExpression("Expression", expression.Expression ?? string.Empty, Array.Empty<TabularMetadataReference>())
            };

            return new TabularMetadataObject(
                TabularMetadataKey.Expression(expression.Name),
                TabularMetadataObjectType.Expression,
                expression.Name,
                null,
                Array.Empty<string>(),
                expressions);
        }

        private static IReadOnlyList<TabularMetadataExpression> BuildDaxExpressions(IDaxDependantObject daxObject)
        {
            if (daxObject == null) return Array.Empty<TabularMetadataExpression>();

            var expressions = new List<TabularMetadataExpression>();
            foreach (var property in daxObject.GetDAXProperties())
            {
                var dax = daxObject.GetDAX(property) ?? string.Empty;
                var dependencies = CollectDependencies(daxObject.DependsOn, property);
                expressions.Add(new TabularMetadataExpression(property.GetDescription(), dax, dependencies));
            }

            return expressions;
        }

        private static IReadOnlyList<TabularMetadataReference> CollectDependencies(DependsOnList dependsOn, DAXProperty property)
        {
            if (dependsOn == null || dependsOn.Count == 0) return Array.Empty<TabularMetadataReference>();

            var references = new List<TabularMetadataReference>();
            foreach (var kvp in dependsOn)
            {
                if (!kvp.Value.Any(r => r.property == property)) continue;
                if (TryCreateReference(kvp.Key, out var reference))
                {
                    references.Add(reference);
                }
            }

            return references;
        }

        private static bool TryCreateReference(IDaxObject daxObject, out TabularMetadataReference reference)
        {
            reference = null;
            if (daxObject is Table table)
            {
                reference = new TabularMetadataReference(
                    TabularMetadataObjectType.Table,
                    TabularMetadataKey.Table(table.Name),
                    table.Name,
                    table.Name);
                return true;
            }

            if (daxObject is Column column)
            {
                reference = new TabularMetadataReference(
                    TabularMetadataObjectType.Column,
                    TabularMetadataKey.Column(column.Table.Name, column.Name),
                    column.Name,
                    column.Table.Name);
                return true;
            }

            if (daxObject is Measure measure)
            {
                reference = new TabularMetadataReference(
                    TabularMetadataObjectType.Measure,
                    TabularMetadataKey.Measure(measure.Table.Name, measure.Name),
                    measure.Name,
                    measure.Table.Name);
                return true;
            }

            reference = new TabularMetadataReference(
                TabularMetadataObjectType.Unknown,
                daxObject.DaxObjectFullName,
                daxObject.DaxObjectName,
                daxObject.DaxTableName);
            return true;
        }
    }
}
