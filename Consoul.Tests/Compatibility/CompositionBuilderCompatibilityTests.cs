using System;
using System.Collections.Generic;
using ConsoulLibrary.Tests.Rendering;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests Spectre-style composition builder compatibility.
    /// </summary>
    public class CompositionBuilderCompatibilityTests
    {
        /// <summary>
        /// Rows aliases render text and renderable rows through the existing rows renderer.
        /// </summary>
        [Fact]
        public void Rows_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var rows = new Rows()
                    .Width(40)
                    .Spacing(0)
                    .ContentColor("white")
                    .AddRow("Summary")
                    .AddRow(new Markup("[green]Ready[/]"));

                AnsiConsole.Write(rows);

                VisualAssert.ContainsText(scope.Driver, "Summary");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Summary") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.White);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Rows line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rows aliases accept enumerable renderable constructors and can collapse to content width.
        /// </summary>
        [Fact]
        public void Rows_FluentBuilder_AcceptsEnumerableRenderablesAndCollapse()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var items = new List<IConsoulRenderable>
                {
                    new Markup("[green]Ready[/]"),
                    new Text("Go").Color(ConsoleColor.Cyan)
                };
                var rows = new Rows(items)
                    .Spacing(0)
                    .Collapse()
                    .ContentColor("white");

                AnsiConsole.Write(rows);

                Assert.False(rows.Options.Expand);
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Go");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.White);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 5, "Collapsed enumerable rows line exceeded measured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rows aliases accept mixed plain values, renderables, and configured row items.
        /// </summary>
        [Fact]
        public void Rows_FluentBuilder_AcceptsMixedObjectRows()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var rows = new Rows(new object[]
                    {
                        "Summary",
                        new Markup("[green]Ready[/]"),
                        2
                    })
                    .Width(40)
                    .Spacing(0)
                    .ContentColor("white")
                    .AddRow(new RowsItem("Configured"))
                    .AddRow(3, ConsoleColor.Cyan);

                AnsiConsole.Write(rows);

                Assert.Equal(5, rows.Rows.Count);
                VisualAssert.ContainsText(scope.Driver, "Summary");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "2");
                VisualAssert.ContainsText(scope.Driver, "Configured");
                VisualAssert.ContainsText(scope.Driver, "3");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Summary") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("3") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Mixed rows line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rows aliases accept params overloads for text, renderable, and mixed object row batches.
        /// </summary>
        [Fact]
        public void Rows_FluentBuilder_AddRowsParamsAcceptTextRenderableAndObjectValues()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var rows = new Rows()
                    .Width(48)
                    .Spacing(0)
                    .ContentColor("white")
                    .AddRows("Summary", "Audit")
                    .AddRows(new Markup("[green]Ready[/]"), new Text("Queued").Color(ConsoleColor.Cyan))
                    .AddRows("Retries", 2, new Markup("[yellow]Waiting[/]"));

                AnsiConsole.Write(rows);

                Assert.Equal(7, rows.Rows.Count);
                VisualAssert.ContainsText(scope.Driver, "Summary");
                VisualAssert.ContainsText(scope.Driver, "Audit");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "Retries");
                VisualAssert.ContainsText(scope.Driver, "2");
                VisualAssert.ContainsText(scope.Driver, "Waiting");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Direct row options can request compact layout without using the fluent wrapper.
        /// </summary>
        [Fact]
        public void RowsOptions_ExpandFalse_CollapsesRowsEntry()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var rows = new RowsEntry(
                    new[] { "Alpha", "Beta" },
                    new RowsOptions
                    {
                        Expand = false,
                        Spacing = 0
                    });

                rows.Render();

                Assert.False(rows.Options.Expand);
                VisualAssert.ContainsText(scope.Driver, "Alpha");
                VisualAssert.ContainsText(scope.Driver, "Beta");
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 5, "Collapsed RowsEntry line exceeded measured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rows Expand restores terminal-width rendering after a compact builder chain.
        /// </summary>
        [Fact]
        public void Rows_FluentBuilder_ExpandRestoresTerminalWidth()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var rows = new Rows("One", "Two")
                    .Spacing(0)
                    .Collapse()
                    .Expand();

                AnsiConsole.Write(rows);

                Assert.True(rows.Options.Expand);
                Assert.Contains(scope.Driver.Lines, line => line.Length > 20 && line.Contains("One"));
                VisualAssert.ContainsText(scope.Driver, "Two");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Columns aliases render captured renderables side by side through the existing columns renderer.
        /// </summary>
        [Fact]
        public void Columns_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var columns = new Columns()
                    .Width(48)
                    .ColumnCount(2)
                    .MinimumColumnWidth(12)
                    .Gutter(2)
                    .HeaderColor("yellow")
                    .ContentColor("white")
                    .AddColumn(new Markup("[green]API[/] ready"), "Service")
                    .AddColumn("Worker waiting", "Worker", ConsoleColor.Cyan);

                AnsiConsole.Write(columns);

                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "API ready");
                VisualAssert.ContainsText(scope.Driver, "Worker waiting");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Worker") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Worker waiting") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 48, "Columns line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Columns aliases accept enumerable renderable constructors and can collapse to measured content.
        /// </summary>
        [Fact]
        public void Columns_FluentBuilder_AcceptsEnumerableRenderablesAndCollapse()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var items = new List<IConsoulRenderable>
                {
                    new Markup("[green]API[/] ready"),
                    new Text("Worker").Color(ConsoleColor.Cyan)
                };
                var columns = new Columns(items)
                    .ColumnCount(2)
                    .MinimumColumnWidth(1)
                    .Gutter(2)
                    .Collapse()
                    .ContentColor("white");

                AnsiConsole.Write(columns);

                Assert.False(columns.Options.Expand);
                VisualAssert.ContainsText(scope.Driver, "API ready");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API ready") && write.Color == ConsoleColor.White);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 18, "Collapsed enumerable columns line exceeded measured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Columns aliases accept mixed plain values, renderables, and configured cells.
        /// </summary>
        [Fact]
        public void Columns_FluentBuilder_AcceptsMixedObjectColumns()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var columns = new Columns(new object[]
                    {
                        "API",
                        new Markup("[green]Ready[/]"),
                        2
                    })
                    .Width(48)
                    .ColumnCount(3)
                    .MinimumColumnWidth(4)
                    .Gutter(1)
                    .HeaderColor("yellow")
                    .ContentColor("white")
                    .AddColumn(new GridCell("Configured", "Source"))
                    .AddColumn(3, "Retries", ConsoleColor.Cyan);

                AnsiConsole.Write(columns);

                Assert.Equal(5, columns.Columns.Count);
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "2");
                VisualAssert.ContainsText(scope.Driver, "Source");
                VisualAssert.ContainsText(scope.Driver, "Configured");
                VisualAssert.ContainsText(scope.Driver, "Retries");
                VisualAssert.ContainsText(scope.Driver, "3");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Retries") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("3") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 48, "Mixed columns line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Columns aliases accept params overloads for text, renderable, and mixed object column batches.
        /// </summary>
        [Fact]
        public void Columns_FluentBuilder_AddColumnsParamsAcceptTextRenderableAndObjectValues()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var columns = new Columns()
                    .Width(60)
                    .ColumnCount(3)
                    .MinimumColumnWidth(4)
                    .Gutter(1)
                    .ContentColor("white")
                    .AddColumns("API", "Worker")
                    .AddColumns(new Markup("[green]Ready[/]"), new Text("Queued").Color(ConsoleColor.Cyan))
                    .AddColumns("Retries", 2, new Markup("[yellow]Waiting[/]"));

                AnsiConsole.Write(columns);

                Assert.Equal(7, columns.Columns.Count);
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "Retries");
                VisualAssert.ContainsText(scope.Driver, "2");
                VisualAssert.ContainsText(scope.Driver, "Waiting");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Direct column options can request compact layout without using the fluent wrapper.
        /// </summary>
        [Fact]
        public void ColumnsOptions_ExpandFalse_CollapsesColumnsEntry()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var columns = new ColumnsEntry(
                    new[] { "Long", "1" },
                    new ColumnsOptions
                    {
                        Columns = 2,
                        Expand = false,
                        Gutter = 1,
                        MinimumColumnWidth = 1
                    });

                columns.Render();

                Assert.False(columns.Options.Expand);
                VisualAssert.ContainsText(scope.Driver, "Long");
                VisualAssert.ContainsText(scope.Driver, "1");
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 6, "Collapsed ColumnsEntry line exceeded measured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Columns Expand restores terminal-width rendering after a compact builder chain.
        /// </summary>
        [Fact]
        public void Columns_FluentBuilder_ExpandRestoresTerminalWidth()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var columns = new Columns("One", "Two")
                    .ColumnCount(2)
                    .MinimumColumnWidth(1)
                    .Collapse()
                    .Expand();

                AnsiConsole.Write(columns);

                Assert.True(columns.Options.Expand);
                Assert.Contains(scope.Driver.Lines, line => line.Length > 20 && line.Contains("One") && line.Contains("Two"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grid aliases support AddColumn/AddRow style migration snippets.
        /// </summary>
        [Fact]
        public void Grid_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var grid = new Grid()
                    .Width(48)
                    .AddColumns(2)
                    .MinimumColumnWidth(10)
                    .Gutter(2)
                    .HeaderColor("yellow")
                    .ContentColor("white")
                    .AddCell("Job", "Name")
                    .AddCell("State", "Status")
                    .AddRow("Build", "Ready")
                    .AddRow("Deploy", "Waiting");

                AnsiConsole.Write(grid);

                VisualAssert.ContainsText(scope.Driver, "Name");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Name") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.White);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 48, "Grid line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grid aliases accept configured Spectre-style columns and renderable row cells.
        /// </summary>
        [Fact]
        public void Grid_FluentBuilder_AcceptsConfiguredColumnsAndRenderableRows()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var grid = new Grid()
                    .Width(42)
                    .MinimumColumnWidth(4)
                    .Gutter(1)
                    .HeaderColor("yellow")
                    .ContentColor("white")
                    .AddColumns(
                        new GridColumn().Width(12).Padding(1, 1).NoWrap(),
                        new GridColumn().Width(10).Justify(Justify.Right))
                    .AddRow("LongWordValueThatShouldClip", "7")
                    .AddEmptyRow()
                    .AddRow(
                        new Markup("[green]API[/]"),
                        new Panel("OK").AsciiBorder().Width(8).Padding(0));

                AnsiConsole.Write(grid);

                Assert.Equal(2, grid.Columns.Count);
                Assert.Equal(6, grid.Cells.Count);
                VisualAssert.ContainsText(scope.Driver, "LongWordVa");
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "OK");
                Assert.DoesNotContain("LongWordValue", scope.Driver.Text);
                Assert.Contains(scope.Driver.Lines, line =>
                    line.Contains("LongWordVa") &&
                    line.Contains("7") &&
                    line.IndexOf("7", StringComparison.Ordinal) > line.IndexOf("LongWordVa", StringComparison.Ordinal) + "LongWordVa".Length);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 42, "Configured grid line exceeded width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grid columns can be configured inline or supplied from generated column metadata.
        /// </summary>
        [Fact]
        public void Grid_FluentBuilder_AcceptsConfiguredAndEnumerableColumns()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                IEnumerable<GridColumn> metricColumns = new[]
                {
                    new GridColumn().Width(8).Centered(),
                    new GridColumn().Width(10).RightAligned()
                };
                var grid = new Grid()
                    .Width(36)
                    .MinimumColumnWidth(4)
                    .Gutter(1)
                    .AddColumn(column => column.Width(12).Padding(1, 1).LeftAligned())
                    .AddColumns(metricColumns)
                    .AddRow("Service", "Load", "7")
                    .AddRow("Worker", "Queue", "12");

                AnsiConsole.Write(grid);

                Assert.Equal(3, grid.Columns.Count);
                Assert.Equal(3, grid.Options.Columns);
                Assert.Equal(12, grid.Columns[0].Width);
                Assert.Equal(Justify.Left, grid.Columns[0].Alignment);
                Assert.Equal(Justify.Center, grid.Columns[1].Alignment);
                Assert.Equal(Justify.Right, grid.Columns[2].Alignment);
                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "12");
                Assert.Contains(scope.Driver.Lines, line =>
                    line.Contains("Worker") &&
                    line.Contains("Queue") &&
                    line.Contains("12") &&
                    line.IndexOf("12", StringComparison.Ordinal) > line.IndexOf("Queue", StringComparison.Ordinal));
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Generated grid column line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grid columns expose table-like alignment helpers for quick dashboard snippets.
        /// </summary>
        [Fact]
        public void GridColumn_FluentAlignmentHelpers_SetJustification()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                GridColumn left = new GridColumn().Width(10).LeftJustified();
                GridColumn center = new GridColumn().Width(10).Centered();
                GridColumn right = new GridColumn().Width(10).RightAligned();
                var grid = new Grid()
                    .Width(36)
                    .MinimumColumnWidth(4)
                    .Gutter(1)
                    .AddColumns(left, center, right)
                    .AddRow("Name", "State", "7");

                AnsiConsole.Write(grid);

                Assert.Equal(Justify.Left, left.Alignment);
                Assert.Equal(Justify.Center, center.Alignment);
                Assert.Equal(Justify.Right, right.Alignment);
                VisualAssert.ContainsText(scope.Driver, "Name");
                VisualAssert.ContainsText(scope.Driver, "State");
                VisualAssert.ContainsText(scope.Driver, "7");
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Aligned grid line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grid rows accept mixed plain values, renderables, and existing cells for quick POC layout snippets.
        /// </summary>
        [Fact]
        public void Grid_FluentBuilder_AcceptsMixedObjectRows()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var grid = new Grid(new object[]
                    {
                        "Service",
                        new Markup("[green]Ready[/]"),
                        2
                    })
                    .Width(44)
                    .AddColumns(3)
                    .MinimumColumnWidth(4)
                    .Gutter(1)
                    .HeaderColor("yellow")
                    .ContentColor("white")
                    .AddRow(new GridCell("Worker"), new Panel("Queued").NoBorder().Width(10).Padding(0), null)
                    .AddCell(3, "Retries", ConsoleColor.Cyan);

                AnsiConsole.Write(grid);

                Assert.Equal(7, grid.Cells.Count);
                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "2");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "Retries");
                VisualAssert.ContainsText(scope.Driver, "3");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Retries") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("3") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Mixed grid line exceeded width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grid aliases can add several inline rows without collection wrappers.
        /// </summary>
        [Fact]
        public void Grid_FluentBuilder_AddRowsAcceptsInlineRowParams()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var grid = new Grid()
                    .Width(48)
                    .AddColumns(2)
                    .MinimumColumnWidth(8)
                    .Gutter(1)
                    .ContentColor("white")
                    .AddRows(
                        new[] { "API", "Ready" },
                        new[] { "Worker", "Queued" })
                    .AddRows(
                        new object[] { new Markup("[cyan]Scheduler[/]"), new Text("Running", ConsoleColor.Green) },
                        new object[] { "Retries", 2 });

                AnsiConsole.Write(grid);

                Assert.Equal(8, grid.Cells.Count);
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "Scheduler");
                VisualAssert.ContainsText(scope.Driver, "Running");
                VisualAssert.ContainsText(scope.Driver, "Retries");
                VisualAssert.ContainsText(scope.Driver, "2");
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("ConsoulLibrary.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grid aliases can collapse to measured content while preserving configured columns.
        /// </summary>
        [Fact]
        public void Grid_FluentBuilder_CollapseAutoSizesToContent()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var grid = new Grid()
                    .AddColumns(
                        new GridColumn().Padding(1, 1),
                        new GridColumn().Justify(Justify.Right))
                    .Gutter(2)
                    .Collapse()
                    .AddRow("Name", "API")
                    .AddRow("State", "Ready");

                AnsiConsole.Write(grid);

                Assert.False(grid.Options.Expand);
                Assert.Null(grid.Options.Width);
                VisualAssert.ContainsText(scope.Driver, "Name");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 26, "Collapsed grid line exceeded measured width: " + line));
                Assert.Contains(scope.Driver.Lines, line => line.Length == 26 && line.Contains("Name") && line.Contains("API"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Direct grid options can request collapsed layout without using the fluent wrapper.
        /// </summary>
        [Fact]
        public void GridOptions_ExpandFalse_CollapsesGridEntry()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var grid = new GridEntry(
                    new[] { "Long", "1", "A", "2" },
                    new GridOptions
                    {
                        Columns = 2,
                        Expand = false,
                        Gutter = 1,
                        MinimumColumnWidth = 1
                    });

                grid.Render();

                Assert.False(grid.Options.Expand);
                VisualAssert.ContainsText(scope.Driver, "Long");
                VisualAssert.ContainsText(scope.Driver, "2");
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 6, "Collapsed GridEntry line exceeded measured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grid Expand restores terminal-width rendering after a collapsed builder chain.
        /// </summary>
        [Fact]
        public void Grid_FluentBuilder_ExpandRestoresTerminalWidth()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var grid = new Grid()
                    .AddColumns(2)
                    .Collapse()
                    .Expand()
                    .AddRow("Left", "Right");

                AnsiConsole.Write(grid);

                Assert.True(grid.Options.Expand);
                Assert.Null(grid.Options.Width);
                VisualAssert.ContainsText(scope.Driver, "Left");
                VisualAssert.ContainsText(scope.Driver, "Right");
                Assert.Contains(scope.Driver.Lines, line => line.Length > 26 && line.Contains("Left") && line.Contains("Right"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rows, columns, and grids can project Spectre-style style strings directly from dashboard model data.
        /// </summary>
        [Fact]
        public void CompositionBuilders_ProjectStyleStringsFromModelCollections()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var statuses = new[]
                {
                    new DashboardTile("API", "Ready", "green"),
                    new DashboardTile("Worker", "Waiting", "white on navy"),
                    new DashboardTile("Search", "Queued", string.Empty)
                };

                new Rows()
                    .Width(48)
                    .Spacing(0)
                    .ContentColor("yellow")
                    .AddRow("Direct row", "aqua")
                    .AddRows(statuses, status => status.Name + ": " + status.State, status => status.StyleText)
                    .Render();

                new Columns()
                    .Width(60)
                    .ColumnCount(3)
                    .MinimumColumnWidth(8)
                    .Gutter(1)
                    .ContentColor("yellow")
                    .AddColumn("Direct column", "Direct", "aqua")
                    .AddColumns(statuses, status => status.State, status => status.Name, status => status.StyleText)
                    .Render();

                new Grid()
                    .Width(60)
                    .AddColumns(3)
                    .MinimumColumnWidth(8)
                    .Gutter(1)
                    .ContentColor("yellow")
                    .AddCell("Direct cell", "Direct", "#22c55e")
                    .AddCells(statuses, status => status.State, status => status.Name, status => status.StyleText)
                    .Render();

                new Layout()
                    .Width(60)
                    .ColumnCount(3)
                    .MinimumPaneWidth(8)
                    .Gutter(1)
                    .ContentColor("yellow")
                    .AddPane("Direct pane", "Direct", "aqua")
                    .AddPanes(statuses, status => "Layout " + status.State, status => status.Name, status => status.StyleText)
                    .Render();

                VisualAssert.ContainsText(scope.Driver, "Direct row");
                VisualAssert.ContainsText(scope.Driver, "Direct column");
                VisualAssert.ContainsText(scope.Driver, "Direct cell");
                VisualAssert.ContainsText(scope.Driver, "Direct pane");
                VisualAssert.ContainsText(scope.Driver, "API: Ready");
                VisualAssert.ContainsText(scope.Driver, "Worker: Waiting");
                VisualAssert.ContainsText(scope.Driver, "Search: Queued");
                VisualAssert.ContainsText(scope.Driver, "Layout Ready");
                VisualAssert.ContainsText(scope.Driver, "Layout Waiting");
                VisualAssert.ContainsText(scope.Driver, "Layout Queued");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Direct row") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Direct column") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Direct cell") && write.Color == ConsoleColor.DarkCyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Direct pane") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API: Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Worker: Waiting") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Search: Queued") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Queued") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Layout Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Layout Waiting") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Layout Queued") && write.Color == ConsoleColor.Yellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 60, "Style-string composition line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Align aliases capture renderables and center them through the existing alignment renderer.
        /// </summary>
        [Fact]
        public void Align_FluentBuilder_CapturesRenderableContent()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var align = new Align(new Panel("Ready").Header("Status").Width(18), HorizontalAlignment.Center)
                    .Width(36)
                    .Color("aqua");

                AnsiConsole.Write(align);

                Assert.Equal(TextBlockAlignment.Center, align.Options.Alignment);
                VisualAssert.ContainsText(scope.Driver, "Status");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("         ") && line.Contains("Status"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Align aliases accept text row collections for quick list-style snippets.
        /// </summary>
        [Fact]
        public void Align_FluentBuilder_AcceptsTextRows()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                var align = new Align(new[] { "Alpha", "Beta" }, HorizontalAlignment.Right)
                    .Width(12);

                AnsiConsole.Write(align);

                Assert.Equal(TextBlockAlignment.Right, align.Options.Alignment);
                VisualAssert.ContainsText(scope.Driver, "Alpha");
                VisualAssert.ContainsText(scope.Driver, "Beta");
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 12, "Aligned row exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Align exposes the same justified naming as text-like builders for pasted POC snippets.
        /// </summary>
        [Fact]
        public void Align_FluentBuilder_ProvidesJustifiedAliases()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                var left = new Align("Left", HorizontalAlignment.Center)
                    .Width(10)
                    .LeftJustified();
                var right = new Align("Right", HorizontalAlignment.Left)
                    .Width(10)
                    .RightJustified();

                AnsiConsole.Write(left);
                AnsiConsole.Write(right);

                Assert.Equal(TextBlockAlignment.Left, left.Options.Alignment);
                Assert.Equal(TextBlockAlignment.Right, right.Options.Alignment);
                VisualAssert.ContainsText(scope.Driver, "Left");
                VisualAssert.ContainsText(scope.Driver, "Right");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Align aliases support bounded vertical alignment without writing past the fake terminal.
        /// </summary>
        [Fact]
        public void Align_FluentBuilder_AppliesVerticalAlignmentHeight()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                var align = new Align("Ready", HorizontalAlignment.Right, VerticalAlignment.Middle)
                    .Width(12)
                    .Height(5);

                AnsiConsole.Write(align);

                Assert.Equal(TextBlockAlignment.Right, align.Options.Alignment);
                Assert.Equal(VerticalAlignment.Middle, align.Options.VerticalAlignment);
                Assert.Equal(5, align.Options.Height);
                int readyIndex = -1;
                for (int index = 0; index < scope.Driver.Lines.Count; index++)
                {
                    if (scope.Driver.Lines[index].Contains("Ready"))
                    {
                        readyIndex = index;
                        break;
                    }
                }

                Assert.Equal(2, readyIndex);
                Assert.Contains(scope.Driver.Lines, line => line.EndsWith("Ready", StringComparison.Ordinal) && line.Length <= 12);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Align and Padder color helpers accept Spectre-style color values without wrapper styles.
        /// </summary>
        [Fact]
        public void AlignAndPadder_ColorHelpers_AcceptColorAndRgbColorValues()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var colorAlign = new Align("Aligned color", HorizontalAlignment.Left)
                    .Width(24)
                    .Color(SpectreColor.Aqua);
                var rgbAlign = new Align("Aligned rgb", HorizontalAlignment.Left)
                    .Width(24)
                    .Color(RgbColor.Yellow);
                var colorPadder = new Padder("Padded color")
                    .Padding(1, 0)
                    .Color(SpectreColor.Red);
                var rgbPadder = new Padder("Padded rgb")
                    .Padding(1, 0)
                    .Color(RgbColor.Green);

                AnsiConsole.Write(colorAlign);
                AnsiConsole.Write(rgbAlign);
                AnsiConsole.Write(colorPadder);
                AnsiConsole.Write(rgbPadder);

                VisualAssert.ContainsText(scope.Driver, "Aligned color");
                VisualAssert.ContainsText(scope.Driver, "Aligned rgb");
                VisualAssert.ContainsText(scope.Driver, "Padded color");
                VisualAssert.ContainsText(scope.Driver, "Padded rgb");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Aligned color") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Aligned rgb") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Padded color") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Padded rgb") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Padder aliases render padded text through the existing padding renderer.
        /// </summary>
        [Fact]
        public void Padder_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var padder = new Padder("Ready", new Padding(3, 1, 2, 1))
                    .Color("aqua");

                AnsiConsole.Write(padder);

                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("   Ready") && line.Length <= 10);
                Assert.Contains(scope.Driver.Lines, line => line.Trim().Length == 0 && line.Length <= 10);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Padder aliases accept compact horizontal and vertical padding arguments.
        /// </summary>
        [Fact]
        public void Padder_FluentBuilder_AppliesHorizontalAndVerticalPadding()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var padder = new Padder("Ready")
                    .Padding(2, 1)
                    .Color(ConsoleColor.Green);

                AnsiConsole.Write(padder);

                Assert.Equal(2, padder.Padding.Left);
                Assert.Equal(2, padder.Padding.Right);
                Assert.Equal(1, padder.Padding.Top);
                Assert.Equal(1, padder.Padding.Bottom);
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("  Ready", StringComparison.Ordinal) && line.Length <= 9);
                Assert.Contains(scope.Driver.Lines, line => line.Trim().Length == 0 && line.Length <= 9);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Padder color helpers accept reusable style objects shared across POC widgets.
        /// </summary>
        [Fact]
        public void Padder_FluentBuilder_AcceptsReusableStyleObject()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var style = new Style(ConsoleColor.Magenta);
                var padder = new Padder("Ready")
                    .Padding(1, 0)
                    .Color(style);

                AnsiConsole.Write(padder);

                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Magenta);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Padding accepts Spectre-style horizontal and vertical constructor arguments.
        /// </summary>
        [Fact]
        public void Padding_TwoArgumentConstructor_AppliesHorizontalAndVerticalAmounts()
        {
            var padding = new Padding(2, 1);

            Assert.Equal(2, padding.Left);
            Assert.Equal(2, padding.Right);
            Assert.Equal(1, padding.Top);
            Assert.Equal(1, padding.Bottom);
        }

        /// <summary>
        /// Padder property initializers work for copied POC snippets without the fluent helpers.
        /// </summary>
        [Fact]
        public void Padder_PropertyInitializer_RendersWithPaddingAndExpandProperty()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                var padder = new Padder("Ready")
                {
                    Padding = new Padding(2, 1),
                    Expand = true
                }.Color(ConsoleColor.Green);

                AnsiConsole.Write(padder);

                Assert.True(padder.Expand);
                Assert.Equal(2, padder.Padding.Left);
                Assert.Equal(2, padder.Padding.Right);
                Assert.Equal(1, padder.Padding.Top);
                Assert.Equal(1, padder.Padding.Bottom);
                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("  Ready") && line.Length <= scope.Driver.BufferWidth);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Padder aliases capture renderables and can expand to a configured width.
        /// </summary>
        [Fact]
        public void Padder_RenderableBuilder_CapturesAndExpandsContent()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var padder = new Padder(new Panel("Build complete").Header("Summary").Width(24))
                    .PadLeft(2)
                    .PadRight(1)
                    .PadTop(1)
                    .PadBottom(1)
                    .Width(34)
                    .Expand();

                AnsiConsole.Write(padder);

                VisualAssert.ContainsText(scope.Driver, "Summary");
                VisualAssert.ContainsText(scope.Driver, "Build complete");
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("  ") && line.Contains("Summary"));
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 34, "Padder line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Layout aliases render named panes through the existing layout renderer.
        /// </summary>
        [Fact]
        public void Layout_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var layout = new Layout("Dashboard")
                    .Width(52)
                    .ColumnCount(2)
                    .MinimumPaneWidth(14)
                    .Gutter(2)
                    .TitleColor("yellow")
                    .ContentColor("white")
                    .AddPane(new Panel("Ready").Header("API").Width(20), "Service")
                    .AddPane("Waiting", "Worker", ConsoleColor.Cyan);

                layout["Worker"].Content = "Running";

                AnsiConsole.Write(layout);

                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Running");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Running") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 52, "Layout line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Layout aliases accept prepared pane collections for MVC-style dashboard sections.
        /// </summary>
        [Fact]
        public void Layout_FluentBuilder_AcceptsPreparedPaneCollections()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                LayoutPane[] constructorPanes =
                {
                    LayoutPane.Create("Constructor ready", "Constructed", "aqua")
                };
                IEnumerable<LayoutPane> panes = new[]
                {
                    LayoutPane.Create("Ready", "Service", "green").Size(10),
                    LayoutPane.Create("Waiting", "Worker", ConsoleColor.Cyan).Ratio(2),
                    LayoutPane.Create("Secret", "Hidden").Invisible()
                };
                var constructed = new Layout(constructorPanes)
                    .Width(30)
                    .ColumnCount(1);
                var layout = new Layout("Dashboard")
                    .Width(44)
                    .ColumnCount(3)
                    .MinimumPaneWidth(6)
                    .Gutter(1)
                    .AddPanes(panes);

                layout["Worker"].Update("Running", "yellow");

                AnsiConsole.Write(constructed);
                AnsiConsole.Write(layout);

                Assert.Single(constructed.Panes);
                Assert.Equal(3, layout.Panes.Count);
                Assert.Equal(10, layout.Panes[0].SizeValue);
                Assert.Equal(2, layout.Panes[1].RatioValue);
                VisualAssert.ContainsText(scope.Driver, "Constructed");
                VisualAssert.ContainsText(scope.Driver, "Constructor ready");
                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "Running");
                Assert.DoesNotContain("Hidden", scope.Driver.Text);
                Assert.DoesNotContain("Secret", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Constructor ready") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Running") && write.Color == ConsoleColor.Yellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Prepared layout pane line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Nested layouts expose mutable named panes for dashboard-style migration snippets.
        /// </summary>
        [Fact]
        public void Layout_SplitRowsAndColumns_UpdateNamedPanesRecursively()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var services = new Table()
                    .Width(30)
                    .HideFooter()
                    .AddColumns("Service", "State")
                    .AddRow("API", "Ready");

                var layout = new Layout("Root")
                    .Width(64)
                    .MinimumPaneWidth(18)
                    .SplitRows(
                        new Layout("Header"),
                        new Layout("Body").SplitColumns(
                            new Layout("Status"),
                            new Layout("Metrics")));

                layout["Header"].Update(new Panel("Real-time dashboard").Header("Operations").Width(34));
                layout["Status"].Update(services);
                layout["Metrics"].Update(new Text("Jobs: 4").Width(18).Color("cyan"));

                AnsiConsole.Write(layout);

                VisualAssert.ContainsText(scope.Driver, "Header");
                VisualAssert.ContainsText(scope.Driver, "Operations");
                VisualAssert.ContainsText(scope.Driver, "Real-time dashboard");
                VisualAssert.ContainsText(scope.Driver, "Status");
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Metrics");
                VisualAssert.ContainsText(scope.Driver, "Jobs: 4");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Split helpers accept reusable renderables directly for quick dashboard composition.
        /// </summary>
        [Fact]
        public void Layout_SplitRowsAndColumns_AcceptRenderablePanes()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var services = new Table()
                    .Width(30)
                    .HideFooter()
                    .AddColumns("Service", "State")
                    .AddRows(
                        new[] { "API", "Ready" },
                        new[] { "Worker", "Queued" });

                var layout = new Layout("Ops")
                    .Width(64)
                    .MinimumPaneWidth(16)
                    .SplitRows(
                        new Panel("Release train").Header("Overview").Width(28),
                        new Layout("Body").SplitColumns(
                            services,
                            new Rows()
                                .Width(24)
                                .Spacing(0)
                                .AddRows(new Markup("[green]Healthy[/]"), new Text("Jobs: 4").Color(ConsoleColor.Cyan))));

                AnsiConsole.Write(layout);

                Assert.Equal(LayoutDirection.Rows, layout.Options.Direction);
                Assert.Equal(2, layout.Panes.Count);
                VisualAssert.ContainsText(scope.Driver, "Overview");
                VisualAssert.ContainsText(scope.Driver, "Release train");
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "Healthy");
                VisualAssert.ContainsText(scope.Driver, "Jobs: 4");
                Assert.DoesNotContain("ConsoulLibrary.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Nested layout aliases honor fixed sizes and ratios when allocating column widths.
        /// </summary>
        [Fact]
        public void Layout_SplitColumns_HonorsSizeAndRatioHints()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var layout = new Layout("Root")
                    .Width(30)
                    .Gutter(1)
                    .MinimumPaneWidth(4)
                    .SplitColumns(
                        new Layout("Left").Size(8).Update("A"),
                        new Layout("Middle").Ratio(2).Update("B"),
                        new Layout("Right").Ratio(1).Update("C"));

                AnsiConsole.Write(layout);

                VisualAssert.ContainsText(scope.Driver, "Left");
                VisualAssert.ContainsText(scope.Driver, "Middle");
                VisualAssert.ContainsText(scope.Driver, "Right");
                Assert.Contains(scope.Driver.Lines, line =>
                    line.Contains("A") &&
                    line.Contains("B") &&
                    line.Contains("C") &&
                    line.IndexOf("B", StringComparison.Ordinal) == 9 &&
                    line.IndexOf("C", StringComparison.Ordinal) > line.IndexOf("B", StringComparison.Ordinal) + 10);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 30, "Sized layout line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Nested layout aliases can hide panes and use per-pane minimum sizes when deciding whether to stack.
        /// </summary>
        [Fact]
        public void Layout_SplitColumns_HonorsVisibilityAndMinimumSizeHints()
        {
            using (var scope = new ConsoleDriverScope(30))
            {
                var layout = new Layout("Root")
                    .Width(30)
                    .Gutter(2)
                    .MinimumPaneWidth(4)
                    .SplitColumns(
                        new Layout("Visible").MinimumSize(18).Update("Shown"),
                        new Layout("Hidden").Invisible().Update("Secret"),
                        new Layout("Other").MinimumSize(18).Update("Second"));

                AnsiConsole.Write(layout);

                VisualAssert.ContainsText(scope.Driver, "Visible");
                VisualAssert.ContainsText(scope.Driver, "Shown");
                VisualAssert.ContainsText(scope.Driver, "Other");
                VisualAssert.ContainsText(scope.Driver, "Second");
                Assert.DoesNotContain("Hidden", scope.Driver.Text);
                Assert.DoesNotContain("Secret", scope.Driver.Text);
                Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("Visible") && line.Contains("Other"));
                Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("Shown") && line.Contains("Second"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private sealed class DashboardTile
        {
            public DashboardTile(string name, string state, string styleText)
            {
                Name = name;
                State = state;
                StyleText = styleText;
            }

            public string Name { get; }

            public string State { get; }

            public string StyleText { get; }
        }
    }
}
