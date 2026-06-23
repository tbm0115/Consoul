using System;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for table visuals.
    /// </summary>
    public class TableResponsiveRenderingTests
    {
        /// <summary>
        /// Renders a complex table across terminal widths without overflowing the virtual screen.
        /// </summary>
        [Theory]
        [InlineData(20)]
        [InlineData(32)]
        [InlineData(80)]
        [InlineData(120)]
        public void Render_ComplexTable_DoesNotOverflowTerminalWidth(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                using (var table = CreateComplexTable())
                {

                    table.Render("Inventory Review", ConsoleColor.Yellow);

                    VisualAssert.NoOverflow(scope.Driver);
                    VisualAssert.ContainsText(scope.Driver, "Inventory Review");
                    Assert.True(scope.Driver.Writes.Count > 0);
                }
            }
        }

        /// <summary>
        /// Re-rendering after a terminal resize recalculates bounded table output.
        /// </summary>
        [Fact]
        public void Render_AfterTerminalResize_RemainsWithinNewWidth()
        {
            using (var scope = new ConsoleDriverScope(120))
            {
                using (var table = CreateComplexTable())
                {

                    table.Render("Wide View", ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);

                    scope.Driver.Resize(32);
                    scope.Driver.Clear();

                    table.Render("Narrow View", ConsoleColor.Cyan);

                    VisualAssert.NoOverflow(scope.Driver);
                    VisualAssert.ContainsText(scope.Driver, "Narrow View");
                }
            }
        }

        private static TableView CreateComplexTable()
        {
            var table = new TableView();
            table.AddHeaders("ID", "Name", "Description", "Owner", "Status");
            table.AddRow(new[] { "1", "Widget", "Supercalifragilisticexpialidocious component", "Operations", "Ready" });
            table.AddRow(new[] { "2", string.Empty, "Short", "QA" });
            table.AddRow(new[] { "3", "Many columns", "Uneven row", "Platform", "Blocked", "Extra" });
            table.AddRow(new[] { "4", "Telemetry", "Measures terminal rendering behavior under narrow widths", "DevEx", "Active" });
            return table;
        }
    }
}
