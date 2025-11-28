using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TabularEditor.TOMWrapper.Metadata;

namespace TOMWrapperTest.Metadata
{
    [TestClass]
    public class MetadataIndexTests
    {
        [TestMethod]
        public void BuildsIndexesWithExpectedChildren()
        {
            using var handler = MetadataTestModelFactory.Create();
            var index = new TabularMetadataIndex(handler.Model);

            var salesKey = TabularMetadataKey.Table("Sales");
            Assert.IsTrue(index.Tables.ContainsKey(salesKey));

            var expectedSalesChildren = new[]
            {
                TabularMetadataKey.Column("Sales", "Amount"),
                TabularMetadataKey.Column("Sales", "Quantity"),
                TabularMetadataKey.Column("Sales", "ExtendedAmount"),
                TabularMetadataKey.Column("Sales", "SaleDate"),
                TabularMetadataKey.Measure("Sales", "Total Sales"),
                TabularMetadataKey.Measure("Sales", "Total Extended"),
                TabularMetadataKey.Measure("Sales", "Sales Per Unit")
            };
            CollectionAssert.IsSubsetOf(expectedSalesChildren, index.Tables[salesKey].Children.ToArray());

            var calendarKey = TabularMetadataKey.Hierarchy("Date", "Calendar");
            Assert.IsTrue(index.Hierarchies.ContainsKey(calendarKey));
            var hierarchyChildren = index.Hierarchies[calendarKey].Children.ToArray();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    TabularMetadataKey.Column("Date", "Year"),
                    TabularMetadataKey.Column("Date", "Month")
                },
                hierarchyChildren);
        }

        [TestMethod]
        public void CapturesExpressionDependencies()
        {
            using var handler = MetadataTestModelFactory.Create();
            var index = new TabularMetadataIndex(handler.Model);

            var calcColumn = index.Columns[TabularMetadataKey.Column("Sales", "ExtendedAmount")];
            var calcDependencies = calcColumn.Expressions
                .First(e => e.Property == "Expression")
                .Dependencies.Select(d => d.Key).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    TabularMetadataKey.Column("Sales", "Amount"),
                    TabularMetadataKey.Column("Sales", "Quantity")
                },
                calcDependencies);

            var extendedMeasure = index.Measures[TabularMetadataKey.Measure("Sales", "Total Extended")];
            var measureDependencies = extendedMeasure.Expressions
                .First(e => e.Property == "Expression")
                .Dependencies.Select(d => d.Key).ToList();
            CollectionAssert.IsSubsetOf(
                new[]
                {
                    TabularMetadataKey.Measure("Sales", "Total Sales"),
                    TabularMetadataKey.Column("Sales", "ExtendedAmount")
                },
                measureDependencies);
        }

        [TestMethod]
        public void CapturesRoleFiltersPerTable()
        {
            using var handler = MetadataTestModelFactory.Create();
            var index = new TabularMetadataIndex(handler.Model);

            var role = index.Roles[TabularMetadataKey.Role("SalesReaders")];
            var salesFilter = role.Expressions.Single(e => e.Property.Contains("Sales Summary") == false);
            var summaryFilter = role.Expressions.Single(e => e.Property.Contains("Sales Summary"));

            CollectionAssert.Contains(salesFilter.Dependencies.Select(d => d.Key).ToList(), TabularMetadataKey.Column("Sales", "Quantity"));

            CollectionAssert.Contains(summaryFilter.Dependencies.Select(d => d.Key).ToList(), TabularMetadataKey.Column("Sales Summary", "Quantity"));
        }

        [TestMethod]
        public void IndexesNamedExpressions()
        {
            using var handler = MetadataTestModelFactory.Create();
            var index = new TabularMetadataIndex(handler.Model);

            var expressionKey = TabularMetadataKey.Expression("TopQuantity");
            Assert.IsTrue(index.Expressions.ContainsKey(expressionKey));

            var expression = index.Expressions[expressionKey].Expressions.Single();
            Assert.AreEqual("SUMMARIZE(Sales, Sales[Quantity])", expression.Expression);
            Assert.AreEqual(0, expression.Dependencies.Count);
        }
    }
}
