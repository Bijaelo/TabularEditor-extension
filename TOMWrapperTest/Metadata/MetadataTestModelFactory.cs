using TabularEditor.TOMWrapper;
using TabularEditor.TOMWrapper.Utils;

namespace TOMWrapperTest.Metadata
{
    internal static class MetadataTestModelFactory
    {
        public static TabularModelHandler Create()
        {
            var handler = new TabularModelHandler(1601);
            var model = handler.Model;

            var dates = model.AddTable("Date");
            var date = dates.AddDataColumn("Date", "Date", null, DataType.DateTime);
            var year = dates.AddDataColumn("Year", "Year", null, DataType.Int64);
            var month = dates.AddDataColumn("Month", "Month", null, DataType.String);
            dates.AddHierarchy("Calendar", null, year, month);

            var sales = model.AddTable("Sales");
            var saleId = sales.AddDataColumn("SaleId", "SaleId", null, DataType.Int64);
            var amount = sales.AddDataColumn("Amount", "Amount", null, DataType.Decimal);
            var quantity = sales.AddDataColumn("Quantity", "Quantity", null, DataType.Int64);
            var saleDate = sales.AddDataColumn("SaleDate", "SaleDate", null, DataType.DateTime);
            sales.AddCalculatedColumn("ExtendedAmount", "[Amount] * [Quantity]");
            sales.AddMeasure("Total Sales", "SUM(Sales[Amount])");
            sales.AddMeasure("Total Extended", "[Total Sales] + SUM(Sales[ExtendedAmount])");
            sales.AddMeasure("Sales Per Unit", "DIVIDE([Total Extended], SUM(Sales[Quantity]))");

            var summary = model.AddCalculatedTable("Sales Summary", "SUMMARIZE(Sales, Sales[Quantity], \"Total Extended\", [Total Extended])");
            summary.AddCalculatedTableColumn("Quantity", "Quantity");
            summary.AddCalculatedTableColumn("Total Extended", "Total Extended");
            summary.AddMeasure("Max Extended", "MAX('Sales Summary'[Total Extended])");

            var expression = model.AddExpression("TopQuantity", "SUMMARIZE(Sales, Sales[Quantity])");
            expression.Expression = "SUMMARIZE(Sales, Sales[Quantity])";

            var rel = model.AddRelationship();
            rel.FromColumn = saleDate;
            rel.ToColumn = date;

            var role = model.AddRole("SalesReaders");
            role.RowLevelSecurity[sales] = "Sales[Quantity] > 0";
            role.RowLevelSecurity[summary] = "'Sales Summary'[Quantity] > 0";

            FormulaFixup.BuildDependencyTree();
            return handler;
        }
    }
}
