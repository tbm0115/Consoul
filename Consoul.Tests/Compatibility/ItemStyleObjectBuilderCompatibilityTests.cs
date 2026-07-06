using System;
using System.Collections.Generic;
using System.Reflection;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests item-level builder compatibility with reusable style objects.
    /// </summary>
    public class ItemStyleObjectBuilderCompatibilityTests
    {
        /// <summary>
        /// Composition item APIs accept reusable styles for model-bound POC content.
        /// </summary>
        [Fact]
        public void CompositionItemBuilders_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var rows = new Rows()
                    .Width(36)
                    .Spacing(0)
                    .AddRow("Summary", new Style(ConsoleColor.Green))
                    .AddRow(new Text("Captured row"), new Style(ConsoleColor.Cyan), captureWidth: 20);
                var columns = new Columns()
                    .Width(48)
                    .ColumnCount(2)
                    .MinimumColumnWidth(12)
                    .Gutter(2)
                    .HeaderColor(new Style(ConsoleColor.Yellow))
                    .AddColumn("API ready", "Service", new Style(ConsoleColor.White))
                    .AddColumn(new Text("Worker waiting"), "Worker", new Style(ConsoleColor.Magenta), captureWidth: 20);
                var grid = new Grid()
                    .Width(48)
                    .AddColumns(2)
                    .MinimumColumnWidth(10)
                    .Gutter(2)
                    .HeaderColor(new Style(ConsoleColor.Yellow))
                    .AddCell("Build", "Job", new Style(ConsoleColor.Blue))
                    .AddCell(new Text("Ready"), "State", new Style(ConsoleColor.Green), captureWidth: 12);
                var layout = new Layout("Dashboard")
                    .Width(52)
                    .ColumnCount(2)
                    .MinimumPaneWidth(14)
                    .Gutter(2)
                    .TitleColor(new Style(ConsoleColor.Yellow))
                    .AddPane("Running", "Service", new Style(ConsoleColor.White))
                    .AddPane(new Text("Waiting"), "Worker", new Style(ConsoleColor.DarkCyan), captureWidth: 18);

                layout["Service"].Update("Updated", new Style(ConsoleColor.DarkGreen));

                AnsiConsole.Write(rows);
                AnsiConsole.Write(columns);
                AnsiConsole.Write(grid);
                AnsiConsole.Write(layout);

                VisualAssert.ContainsText(scope.Driver, "Summary");
                VisualAssert.ContainsText(scope.Driver, "Captured row");
                VisualAssert.ContainsText(scope.Driver, "API ready");
                VisualAssert.ContainsText(scope.Driver, "Worker waiting");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Updated");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Summary") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Captured row") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API ready") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Worker waiting") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Updated") && write.Color == ConsoleColor.DarkGreen);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting") && write.Color == ConsoleColor.DarkCyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Property list item and model APIs accept reusable styles for generated summaries.
        /// </summary>
        [Fact]
        public void PropertyListItemBuilders_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var pairs = new[]
                {
                    new KeyValuePair<string, string>("Owner", "Platform"),
                    new KeyValuePair<string, string>("Region", "us-east")
                };
                var list = new PropertyListEntry(new PropertyListOptions { Width = 52, LabelWidth = 14 });

                list.Add("Service", "API", new Style(ConsoleColor.Green), new Style(ConsoleColor.Yellow));
                list.AddKeyValues(pairs, new Style(ConsoleColor.Cyan), new Style(ConsoleColor.DarkGray));
                list.AddModel(
                    new DeploymentSummary { State = "Ready", Retries = 2 },
                    BindingFlags.Public | BindingFlags.Instance,
                    new Style(ConsoleColor.White),
                    new Style(ConsoleColor.DarkYellow));

                AnsiConsole.Write(list);

                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "Platform");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Owner") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Platform") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("State") && write.Color == ConsoleColor.DarkYellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("2") && write.Color == ConsoleColor.White);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private sealed class DeploymentSummary
        {
            public string State { get; set; } = string.Empty;

            public int Retries { get; set; }
        }
    }
}
