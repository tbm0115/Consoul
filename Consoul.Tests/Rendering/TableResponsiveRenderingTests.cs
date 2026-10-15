using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

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
        /// Table cells with raw markup strings use visible text for width allocation at common terminal widths.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(20)]
        [InlineData(32)]
        [InlineData(80)]
        [InlineData(120)]
        public void Render_RawMarkupCells_DoNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                var options = new TableRenderOptions
                {
                    ShowFooterHelp = false
                };
                options.Lines.BorderStyle = TableBorderStyle.Ascii;

                using (var table = new TableView(options))
                {
                    table.AddHeaders("[cyan]Service[/]", "[yellow]State[/]");
                    table.AddRow(new[] { "[green]API[/]", "[white on navy]Ready[/]" });
                    table.AddRow(new[] { "[[literal]]", "[red]Blocked[/]" });

                    table.RenderInline();

                    VisualAssert.ContainsText(scope.Driver, "API");
                    VisualAssert.ContainsText(scope.Driver, "Ready");
                    Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                    Assert.DoesNotContain("[green]", scope.Driver.Text);
                    Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                    Assert.DoesNotContain("[/]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
        }

        /// <summary>
        /// Opt-in table border presets render full table joins and remain bounded at common POC widths.
        /// </summary>
        [Theory]
        [InlineData(TableBorderStyle.Rounded, "╭", "┼", "╯")]
        [InlineData(TableBorderStyle.Square, "┌", "┼", "┘")]
        [InlineData(TableBorderStyle.Ascii, "+", "+", "+")]
        [InlineData(TableBorderStyle.Heavy, "┏", "╋", "┛")]
        public void Render_BorderPreset_RendersStyledSeparatorsWithoutOverflow(TableBorderStyle borderStyle, string topLeft, string middleJoin, string bottomRight)
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var options = new TableRenderOptions
                {
                    ShowFooterHelp = false
                };
                options.Lines.BorderStyle = borderStyle;

                using (var table = CreateComplexTable(options))
                {

                    table.Render("Styled Table", ConsoleColor.Yellow);

                    VisualAssert.ContainsText(scope.Driver, "Styled Table");
                    VisualAssert.ContainsText(scope.Driver, topLeft);
                    VisualAssert.ContainsText(scope.Driver, middleJoin);
                    VisualAssert.ContainsText(scope.Driver, bottomRight);
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
        }

        /// <summary>
        /// The default table border style preserves the legacy simple horizontal separator behavior.
        /// </summary>
        [Fact]
        public void Render_DefaultBorderStyle_PreservesSimpleSeparators()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var options = new TableRenderOptions
                {
                    ShowFooterHelp = false
                };

                using (var table = CreateComplexTable(options))
                {
                    table.Render("Simple Table", ConsoleColor.Yellow);

                    Assert.Equal(TableBorderStyle.Simple, options.Lines.BorderStyle);
                    Assert.DoesNotContain("┌", scope.Driver.Text);
                    Assert.DoesNotContain("╭", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
        }

        /// <summary>
        /// Common Spectre-style table border aliases map to bounded Consoul table rendering.
        /// </summary>
        [Fact]
        public void TableBorder_CommonSpectreAliases_RenderWithinBounds()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Assert.Same(TableBorder.None, TableBorder.Minimal);
                Assert.Same(TableBorder.Ascii, TableBorder.Ascii2);
                Assert.Same(TableBorder.Ascii, TableBorder.Safe);
                Assert.Same(TableBorder.Heavy, TableBorder.HeavyEdge);
                Assert.Same(TableBorder.Double, TableBorder.DoubleEdge);

                var minimal = new Table(new TableRenderOptions { ShowFooterHelp = false })
                    .Width(42)
                    .Border(TableBorder.Minimal)
                    .AddColumns("Service", "State")
                    .AddRow("API", "Ready");

                minimal.RenderInline();

                VisualAssert.ContainsText(scope.Driver, "API");
                Assert.DoesNotContain("│", scope.Driver.Text);
                Assert.DoesNotContain("|", scope.Driver.Text);

                var heavyHead = new Table(new TableRenderOptions { ShowFooterHelp = false })
                    .Width(42)
                    .Border(TableBorder.HeavyHead)
                    .AddColumns("Service", "State")
                    .AddRow("Worker", "Busy");

                heavyHead.RenderInline();

                VisualAssert.ContainsText(scope.Driver, "┏");
                VisualAssert.ContainsText(scope.Driver, "┣");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Table render options can carry a reusable title and caption for composed/captured tables.
        /// </summary>
        [Fact]
        public void Render_TitleAndCaptionOptions_RenderAlignedText()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var options = new TableRenderOptions
                {
                    ShowFooterHelp = false,
                    Title = "Service Overview",
                    Caption = "Data refreshed at build time.",
                    TitleAlignment = TextBlockAlignment.Center,
                    CaptionAlignment = TextBlockAlignment.Right,
                    TitleScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    },
                    CaptionScheme = new ColorScheme
                    {
                        Color = ConsoleColor.DarkGray,
                        BackgroundColor = ConsoleColor.Black
                    }
                };
                options.Lines.BorderStyle = TableBorderStyle.Rounded;

                using (var table = new TableView(options))
                {
                    table.AddHeaders("Service", "State");
                    table.AddRow(new[] { "API", "Ready" });

                    table.RenderInline();

                    FakeConsoleWrite titleWrite = scope.Driver.Writes.First(write => write.Text.Contains("Service Overview"));
                    FakeConsoleWrite captionWrite = scope.Driver.Writes.First(write => write.Text.Contains("Data refreshed"));
                    Assert.Equal(ConsoleColor.Cyan, titleWrite.Color);
                    Assert.Equal(ConsoleColor.DarkGray, captionWrite.Color);
                    Assert.True(titleWrite.Text.StartsWith(" "), "Expected title to be centered within the table width.");
                    Assert.True(captionWrite.Text.EndsWith("Data refreshed at build time."), "Expected caption to be right-aligned within the table width.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
        }

        /// <summary>
        /// Table builders and metadata objects accept shared palette colors directly.
        /// </summary>
        [Fact]
        public void Table_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var table = new Table(new TableRenderOptions { ShowFooterHelp = false })
                    .Width(54)
                    .RoundedBorder()
                    .Title("Service Palette", SpectreColor.Aqua, Justify.Center)
                    .Caption("Updated", RgbColor.DarkGray, Justify.Right)
                    .BorderColor(SpectreColor.Green)
                    .HeaderColor(RgbColor.Yellow)
                    .ContentColor(SpectreColor.White)
                    .AlternatingRowColor(RgbColor.Cyan)
                    .TitleColor(RgbColor.Aqua)
                    .CaptionColor(SpectreColor.DarkGray)
                    .FooterColor(RgbColor.Magenta)
                    .AddColumn(new TableColumn("Service").Footer("Total"))
                    .AddColumn(new TableColumn("State").Footer("2"))
                    .AddRow("API", "Ready")
                    .AddRow("Worker", "Busy");

                table.RenderInline();

                var metadataTable = new Table(new TableRenderOptions { ShowFooterHelp = false })
                    .Width(40)
                    .AsciiBorder()
                    .Title(new TableTitle("Metadata Title", SpectreColor.Yellow, Justify.Left).Color(RgbColor.Green))
                    .Caption(new TableCaption("Metadata Caption", RgbColor.Red, Justify.Right).Color(SpectreColor.Red))
                    .AddColumns("Name")
                    .AddRow("Value");

                metadataTable.RenderInline();

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service Palette") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Updated") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("╭") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Worker") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Total") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Metadata Title") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Metadata Caption") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Legacy render messages continue to behave as per-render table titles.
        /// </summary>
        [Fact]
        public void Render_MessageTitle_OverridesOptionTitleForCompatibility()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                var options = new TableRenderOptions
                {
                    ShowFooterHelp = false,
                    Title = "Configured title"
                };

                using (var table = new TableView(options))
                {
                    table.AddHeaders("Service", "State");
                    table.AddRow(new[] { "API", "Ready" });

                    table.RenderInline("Legacy title", ConsoleColor.Yellow);

                    FakeConsoleWrite titleWrite = scope.Driver.Writes.First(write => write.Text.Contains("Legacy title"));
                    Assert.Equal(ConsoleColor.Yellow, titleWrite.Color);
                    Assert.DoesNotContain("Configured title", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
        }

        /// <summary>
        /// Long table titles and captions wrap inside the resolved table width.
        /// </summary>
        [Fact]
        public void Render_LongTitleAndCaption_WrapWithinNarrowTerminal()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                var options = new TableRenderOptions
                {
                    ShowFooterHelp = false,
                    Title = "Deployment readiness table for a constrained terminal",
                    Caption = "Only visible cells should stay inside the fake screen width."
                };

                using (var table = new TableView(options))
                {
                    table.AddHeaders("Service", "State");
                    table.AddRow(new[] { "Worker", "Ready" });

                    table.RenderInline();

                    VisualAssert.WroteText(scope.Driver, "Deployment readiness");
                    VisualAssert.WroteText(scope.Driver, "Only visible cells");
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
        }

        /// <summary>
        /// Tables can hide header cells while preserving row rendering and bounds.
        /// </summary>
        [Fact]
        public void Render_ShowHeadersFalse_HidesManualHeaders()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var options = new TableRenderOptions
                {
                    ShowFooterHelp = false,
                    ShowHeaders = false
                };
                options.Lines.BorderStyle = TableBorderStyle.Rounded;

                using (var table = new TableView(options))
                {
                    table.AddHeaders("Service", "State");
                    table.AddRow(new[] { "API", "Ready" });

                    table.RenderInline();

                    Assert.DoesNotContain("Service", scope.Driver.Text);
                    Assert.DoesNotContain("State", scope.Driver.Text);
                    VisualAssert.ContainsText(scope.Driver, "API");
                    VisualAssert.ContainsText(scope.Driver, "Ready");
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
        }

        /// <summary>
        /// Tables can render one-based row numbers for selection-heavy views.
        /// </summary>
        [Fact]
        public void Render_ShowRowNumbers_RendersOneBasedIndexColumn()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var options = new TableRenderOptions
                {
                    ShowFooterHelp = false,
                    ShowRowNumbers = true
                };
                options.Lines.BorderStyle = TableBorderStyle.Rounded;

                using (var table = new TableView(options))
                {
                    table.AddHeaders("Service", "State");
                    table.AddRow(new[] { "API", "Ready" });
                    table.AddRow(new[] { "Worker", "Busy" });

                    table.RenderInline();

                    VisualAssert.ContainsText(scope.Driver, "#");
                    VisualAssert.ContainsText(scope.Driver, "API");
                    VisualAssert.ContainsText(scope.Driver, "Worker");
                    AssertRightAligned(
                        GetRenderedCells(scope.Driver, line => line.Contains("API") && line.Contains("Ready"))[0],
                        "1");
                    AssertRightAligned(
                        GetRenderedCells(scope.Driver, line => line.Contains("Worker") && line.Contains("Busy"))[0],
                        "2");
                    VisualAssert.NoOverflow(scope.Driver);
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

        /// <summary>
        /// The POC table helper renders selected model properties inline without footer prompt text.
        /// </summary>
        [Fact]
        public void ConsoulTable_SelectedProperties_RendersInlineWithoutFooter()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "Auth", Status = "OK", Retries = 1 },
                    new ServiceRow { Name = "Payments", Status = "Degraded", Retries = 3 }
                };

                Consoul.Write("Before table");
                DynamicTableView<ServiceRow> table = Consoul.Table(rows, row => row.Name, row => row.Status);

                Assert.Contains("Name", table.Headers);
                Assert.Contains("Status", table.Headers);
                VisualAssert.ContainsText(scope.Driver, "Before table");
                VisualAssert.ContainsText(scope.Driver, "Auth");
                VisualAssert.ContainsText(scope.Driver, "Degraded");
                Assert.DoesNotContain("Use Arrow Keys", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// The POC table helper can infer public scalar properties when no selectors are supplied.
        /// </summary>
        [Fact]
        public void ConsoulTable_NoSelectors_UsesPublicScalarProperties()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "Search", Status = "Ready", Retries = 0, Details = new object() }
                };

                DynamicTableView<ServiceRow> table = Consoul.Table(rows);

                Assert.Contains("Name", table.Headers);
                Assert.Contains("Status", table.Headers);
                Assert.Contains("Retries", table.Headers);
                Assert.DoesNotContain("Details", table.Headers);
                VisualAssert.ContainsText(scope.Driver, "Search");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Inferred POC table columns use model metadata and readable property names for headers.
        /// </summary>
        [Fact]
        public void ConsoulTable_NoSelectors_UsesModelMetadataLabels()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var rows = new[]
                {
                    new MetadataServiceRow { ServiceName = "Auth", Status = "Ready", OwnerName = "Ops", RetryCount = 2 }
                };

                DynamicTableView<MetadataServiceRow> table = Consoul.Table(rows);

                Assert.Contains("Service", table.Headers);
                Assert.Contains("Current state", table.Headers);
                Assert.Contains("Owner", table.Headers);
                Assert.Contains("Retry Count", table.Headers);
                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "Current state");
                VisualAssert.ContainsText(scope.Driver, "Owner");
                VisualAssert.ContainsText(scope.Driver, "Retry Count");
                VisualAssert.ContainsText(scope.Driver, "Auth");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Table-specific metadata controls inferred labels, order, and visibility for model-backed POC tables.
        /// </summary>
        [Fact]
        public void ConsoulTable_NoSelectors_UsesTableColumnMetadata()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var rows = new[]
                {
                    new TableColumnMetadataServiceRow
                    {
                        Name = "Deploy",
                        Status = "Ready",
                        Secret = "classified",
                        RetryCount = 2
                    }
                };

                DynamicTableView<TableColumnMetadataServiceRow> table = Consoul.Table(rows);
                string[] headers = table.Headers.ToArray();

                Assert.Equal(new[] { "State", "Service", "Retry Count" }, headers);
                Assert.DoesNotContain("Secret", headers);
                VisualAssert.ContainsText(scope.Driver, "State");
                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                AssertRightAligned(
                    GetRenderedCells(scope.Driver, line => line.Contains("Deploy") && line.Contains("2"))[2],
                    "2");
                Assert.DoesNotContain("classified", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Inferred model columns can format values through table metadata and display-format metadata.
        /// </summary>
        [Fact]
        public void ConsoulTable_NoSelectors_AppliesColumnFormatMetadata()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var rows = new[]
                {
                    new FormattedServiceRow
                    {
                        Started = new DateTime(2026, 6, 23, 9, 30, 0),
                        DurationMs = 42,
                        BuildNumber = 7
                    }
                };

                DynamicTableView<FormattedServiceRow> table = Consoul.Table(rows);

                Assert.Equal(new[] { "Started", "Duration", "Build" }, table.Headers.ToArray());
                VisualAssert.ContainsText(scope.Driver, "2026-06-23");
                VisualAssert.ContainsText(scope.Driver, "42 ms");
                VisualAssert.ContainsText(scope.Driver, "build-007");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Selector-based and computed dynamic columns can opt into explicit format strings.
        /// </summary>
        [Fact]
        public void DynamicTableView_FormattedColumns_RenderExpectedValues()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var rows = new[]
                {
                    new FormattedServiceRow
                    {
                        Started = new DateTime(2026, 6, 23, 9, 30, 0),
                        DurationMs = 42,
                        BuildNumber = 7
                    }
                };

                var table = new DynamicTableView<FormattedServiceRow>(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeader(row => row.Started, "Run date", format: "yyyy/MM/dd");
                table.AddColumn("Duration", row => row.DurationMs, "{0} milliseconds", TableCellAlignment.Right);
                table.AddRows(rows);

                table.RenderInline();

                VisualAssert.ContainsText(scope.Driver, "2026/06/23");
                VisualAssert.ContainsText(scope.Driver, "42 milliseconds");
                AssertRightAligned(
                    GetRenderedCells(scope.Driver, line => line.Contains("42 milliseconds"))[1],
                    "42 milliseconds");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Manual tables can align individual columns for scan-friendly dashboard values.
        /// </summary>
        [Fact]
        public void TableView_ColumnAlignment_AlignsCellsWithinBounds()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                using (var table = new TableView(new TableRenderOptions { ShowFooterHelp = false, TableWidthPercentage = 1m }))
                {
                    table.AddHeader("Service");
                    table.AddHeader("Retries", TableCellAlignment.Right);
                    table.AddHeader("State", TableCellAlignment.Center);
                    table.AddRow(new[] { "API", "7", "OK" });

                    table.RenderInline();

                    string[] cells = GetRenderedCells(scope.Driver, line => line.Contains("API") && line.Contains("7") && line.Contains("OK"));
                    Assert.Equal("API", GetContentArea(cells[0]).Trim());
                    AssertRightAligned(cells[1], "7");
                    AssertCentered(cells[2], "OK");
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
        }

        /// <summary>
        /// Dynamic table computed columns can opt into alignment without changing the model.
        /// </summary>
        [Fact]
        public void DynamicTableView_AddColumn_AppliesColumnAlignment()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "Worker", Status = "OK", Retries = 12 }
                };

                var table = new DynamicTableView<ServiceRow>(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeader(row => row.Name, "Service");
                table.AddColumn("Retries", row => row.Retries, TableCellAlignment.Right);
                table.AddRows(rows);
                table.RenderInline();

                string[] cells = GetRenderedCells(scope.Driver, line => line.Contains("Worker") && line.Contains("12"));
                AssertRightAligned(cells[1], "12");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic tables support labels and computed columns for quick model projections.
        /// </summary>
        [Fact]
        public void DynamicTableView_CustomLabelsAndComputedColumns_RenderExpectedValues()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "Worker", Status = "OK", Retries = 2 }
                };

                var table = new DynamicTableView<ServiceRow>(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeader(row => row.Name, "Service");
                table.AddColumn("Health", row => row.Status + "/" + row.Retries);
                table.AddRows(rows);
                table.RenderInline();

                Assert.Contains("Service", table.Headers);
                Assert.Contains("Health", table.Headers);
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "OK/2");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic model tables can opt into styled borders without leaving the MVC-style helper flow.
        /// </summary>
        [Fact]
        public void DynamicTableView_BorderPreset_RendersStyledModelTable()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "Worker", Status = "OK", Retries = 2 }
                };

                var options = new TableRenderOptions
                {
                    ShowFooterHelp = false
                };
                options.Lines.BorderStyle = TableBorderStyle.Rounded;

                DynamicTableView<ServiceRow> table = Consoul.Table(rows, options, row => row.Name, row => row.Status);

                VisualAssert.ContainsText(scope.Driver, "╭");
                VisualAssert.ContainsText(scope.Driver, "╯");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed POC tables can hide rendered headers while retaining header metadata.
        /// </summary>
        [Fact]
        public void DynamicTableView_ShowHeadersFalse_HidesRenderedHeadersButKeepsMetadata()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "API", Status = "Green", Retries = 0 }
                };
                var options = new TableRenderOptions
                {
                    ShowFooterHelp = false,
                    ShowHeaders = false
                };
                options.Lines.BorderStyle = TableBorderStyle.Rounded;

                DynamicTableView<ServiceRow> table = Consoul.Table(rows, options, row => row.Name, row => row.Status);

                Assert.Contains("Name", table.Headers);
                Assert.Contains("Status", table.Headers);
                Assert.DoesNotContain("Name", scope.Driver.Text);
                Assert.DoesNotContain("Status", scope.Driver.Text);
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Green");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed POC tables can show row numbers without changing header metadata.
        /// </summary>
        [Fact]
        public void DynamicTableView_ShowRowNumbers_RendersIndexesWithoutChangingHeaders()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "API", Status = "Green", Retries = 0 },
                    new ServiceRow { Name = "Worker", Status = "Busy", Retries = 1 }
                };
                var options = new TableRenderOptions
                {
                    ShowFooterHelp = false,
                    ShowRowNumbers = true
                };
                options.Lines.BorderStyle = TableBorderStyle.Rounded;

                DynamicTableView<ServiceRow> table = Consoul.Table(rows, options, row => row.Name, row => row.Status);

                Assert.Equal(new[] { "Name", "Status" }, table.Headers.ToArray());
                VisualAssert.ContainsText(scope.Driver, "#");
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                AssertRightAligned(
                    GetRenderedCells(scope.Driver, line => line.Contains("API") && line.Contains("Green"))[0],
                    "1");
                AssertRightAligned(
                    GetRenderedCells(scope.Driver, line => line.Contains("Worker") && line.Contains("Busy"))[0],
                    "2");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic model tables can filter, sort, and page rows before rendering.
        /// </summary>
        [Fact]
        public void DynamicTableView_FilterSortAndPage_RendersTransformedRows()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "Auth", Status = "OK", Retries = 0 },
                    new ServiceRow { Name = "Payments", Status = "Degraded", Retries = 3 },
                    new ServiceRow { Name = "Billing", Status = "Warning", Retries = 2 },
                    new ServiceRow { Name = "Search", Status = "OK", Retries = 1 }
                };

                var table = new DynamicTableView<ServiceRow>(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeader(row => row.Name);
                table.AddHeader(row => row.Retries);
                table.Filter(row => row.Retries > 0)
                    .SortBy(row => row.Retries, descending: true)
                    .Page(1, 2);
                table.AddRows(rows);

                table.RenderInline();

                ServiceRow[] visible = table.VisibleContents.ToArray();
                Assert.Equal(4, table.TotalRowCount);
                Assert.Equal(2, table.VisibleRowCount);
                Assert.Equal("Payments", visible[0].Name);
                Assert.Equal("Billing", visible[1].Name);
                VisualAssert.ContainsText(scope.Driver, "Payments");
                VisualAssert.ContainsText(scope.Driver, "Billing");
                Assert.DoesNotContain("Auth", scope.Driver.Text);
                Assert.DoesNotContain("Search", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic model tables can search across rendered cell values before rendering.
        /// </summary>
        [Fact]
        public void DynamicTableView_Search_FiltersVisibleCellValues()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "Auth", Status = "OK", Retries = 0 },
                    new ServiceRow { Name = "Payments", Status = "Degraded", Retries = 3 },
                    new ServiceRow { Name = "Billing", Status = "Warning", Retries = 2 }
                };

                var table = new DynamicTableView<ServiceRow>(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeader(row => row.Name);
                table.AddHeader(row => row.Status);
                table.AddRows(rows);
                table.Search("grad");

                table.RenderInline();

                ServiceRow[] visible = table.VisibleContents.ToArray();
                Assert.Equal("grad", table.ActiveSearchQuery);
                Assert.Single(visible);
                Assert.Equal("Payments", visible[0].Name);
                VisualAssert.ContainsText(scope.Driver, "Payments");
                VisualAssert.ContainsText(scope.Driver, "Degraded");
                Assert.DoesNotContain("Auth", scope.Driver.Text);
                Assert.DoesNotContain("Billing", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Search composes with sort and page transforms in a predictable order.
        /// </summary>
        [Fact]
        public void DynamicTableView_SearchSortAndPage_Compose()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "API", Status = "Needs review", Retries = 1 },
                    new ServiceRow { Name = "Worker", Status = "Needs review", Retries = 3 },
                    new ServiceRow { Name = "Billing", Status = "Needs review", Retries = 2 },
                    new ServiceRow { Name = "Search", Status = "Ready", Retries = 4 }
                };

                var table = new DynamicTableView<ServiceRow>(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeader(row => row.Name);
                table.AddHeader(row => row.Status);
                table.AddHeader(row => row.Retries);
                table.AddRows(rows);
                table.Search("needs")
                    .SortBy(row => row.Retries, descending: true)
                    .Page(1, 2);

                table.RenderInline();

                ServiceRow[] visible = table.VisibleContents.ToArray();
                Assert.Equal(2, table.VisibleRowCount);
                Assert.Equal("Worker", visible[0].Name);
                Assert.Equal("Billing", visible[1].Name);
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "Billing");
                Assert.DoesNotContain("API", scope.Driver.Text);
                Assert.DoesNotContain("Search", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic model tables can render a summary of the transformed row state.
        /// </summary>
        [Fact]
        public void DynamicTableView_ShowSummary_RendersTransformedCounts()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "API", Status = "Needs review", Retries = 1 },
                    new ServiceRow { Name = "Worker", Status = "Needs review", Retries = 3 },
                    new ServiceRow { Name = "Billing", Status = "Needs review", Retries = 2 },
                    new ServiceRow { Name = "Search", Status = "Ready", Retries = 4 }
                };

                var table = new DynamicTableView<ServiceRow>(new TableRenderOptions { ShowFooterHelp = false })
                {
                    ShowSummary = true
                };
                table.AddHeader(row => row.Name);
                table.AddHeader(row => row.Status);
                table.AddRows(rows);
                table.Search("needs")
                    .SortBy(row => row.Retries, descending: true)
                    .Page(1, 2);

                table.RenderInline();

                Assert.Equal(4, table.TotalRowCount);
                Assert.Equal(3, table.MatchedRowCount);
                Assert.Equal(2, table.VisibleRowCount);
                VisualAssert.ContainsText(scope.Driver, "Showing 2 rows of 3 matches (4 total) for \"needs\" on page 1.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic model tables can render a bounded empty state when transforms hide every row.
        /// </summary>
        [Fact]
        public void DynamicTableView_EmptyMessage_RendersWhenNoRowsMatch()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "API", Status = "Ready", Retries = 0 },
                    new ServiceRow { Name = "Worker", Status = "Ready", Retries = 1 }
                };

                var options = new TableRenderOptions { ShowFooterHelp = false };
                options.Lines.BorderStyle = TableBorderStyle.Rounded;
                var table = new DynamicTableView<ServiceRow>(options)
                {
                    ShowEmptyMessage = true,
                    ShowSummary = true,
                    EmptyMessage = "No services match the current search."
                };
                table.AddHeader(row => row.Name);
                table.AddHeader(row => row.Status);
                table.AddRows(rows);
                table.Search("blocked");

                table.RenderInline();

                Assert.Equal(0, table.VisibleRowCount);
                Assert.Equal(0, table.MatchedRowCount);
                VisualAssert.ContainsText(scope.Driver, "No services match the current search.");
                VisualAssert.ContainsText(scope.Driver, "Showing 0 rows of 0 matches");
                VisualAssert.ContainsText(scope.Driver, "\"blocked\".");
                VisualAssert.ContainsText(scope.Driver, "╭");
                VisualAssert.ContainsText(scope.Driver, "╯");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic table summaries can be customized by the calling view.
        /// </summary>
        [Fact]
        public void DynamicTableView_CustomSummaryFormatter_RendersCallerText()
        {
            using (var scope = new ConsoleDriverScope(60))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "API", Status = "Ready", Retries = 0 },
                    new ServiceRow { Name = "Worker", Status = "Warning", Retries = 1 }
                };

                var table = new DynamicTableView<ServiceRow>(new TableRenderOptions { ShowFooterHelp = false })
                {
                    ShowSummary = true,
                    SummaryFormatter = view => "Visible services: " + view.VisibleRowCount + "/" + view.TotalRowCount
                };
                table.AddHeader(row => row.Name);
                table.AddRows(rows);
                table.Search("worker");

                table.RenderInline();

                VisualAssert.ContainsText(scope.Driver, "Visible services: 1/2");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Clearing table transforms restores all model rows in source order.
        /// </summary>
        [Fact]
        public void DynamicTableView_ClearTransforms_RestoresSourceRows()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "Auth", Status = "OK", Retries = 0 },
                    new ServiceRow { Name = "Payments", Status = "Degraded", Retries = 3 },
                    new ServiceRow { Name = "Billing", Status = "Warning", Retries = 2 }
                };

                var table = new DynamicTableView<ServiceRow>(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeader(row => row.Name);
                table.Filter(row => row.Retries > 0)
                    .SortBy(row => row.Retries, descending: true)
                    .Search("billing")
                    .Page(1, 1);
                table.AddRows(rows);
                table.ClearFilter().ClearSearch().ClearSort().ClearPaging();

                table.RenderInline();

                ServiceRow[] visible = table.VisibleContents.ToArray();
                Assert.Null(table.ActiveSearchQuery);
                Assert.Equal(3, table.VisibleRowCount);
                Assert.Equal("Auth", visible[0].Name);
                Assert.Equal("Payments", visible[1].Name);
                Assert.Equal("Billing", visible[2].Name);
                VisualAssert.ContainsText(scope.Driver, "Auth");
                VisualAssert.ContainsText(scope.Driver, "Payments");
                VisualAssert.ContainsText(scope.Driver, "Billing");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Prompting a transformed dynamic table returns the selected visible model row.
        /// </summary>
        [Fact]
        public void DynamicTableView_Prompt_ReturnsVisibleRowAfterSortAndPage()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                scope.Driver.EnqueueKey(new ConsoleKeyInfo('1', ConsoleKey.D1, false, false, false));
                scope.Driver.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));

                var rows = new[]
                {
                    new ServiceRow { Name = "Auth", Status = "OK", Retries = 0 },
                    new ServiceRow { Name = "Payments", Status = "Degraded", Retries = 3 },
                    new ServiceRow { Name = "Billing", Status = "Warning", Retries = 2 }
                };

                var table = new DynamicTableView<ServiceRow>(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeader(row => row.Name);
                table.AddRows(rows);
                table.SortBy(row => row.Retries, descending: true)
                    .Page(1, 1);

                ServiceRow selected = table.Prompt("Choose service", allowEmpty: false);

                Assert.NotNull(selected);
                Assert.Equal("Payments", selected.Name);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Manual table selection returns toggled rows through an explicit result object.
        /// </summary>
        [Fact]
        public void TableView_Select_ReturnsToggledRows()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                scope.Driver.EnqueueKey(Key(ConsoleKey.DownArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Spacebar, ' '));
                scope.Driver.EnqueueKey(Key(ConsoleKey.DownArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Spacebar, ' '));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                using (var table = new TableView(new TableRenderOptions { ShowFooterHelp = false }))
                {
                    table.AddHeaders("Service", "State");
                    table.AddRow(new[] { "Auth", "Ready" });
                    table.AddRow(new[] { "Billing", "Pending" });
                    table.AddRow(new[] { "Search", "Ready" });

                    TableSelectionResult result = table.Select("Choose services", allowEmpty: false);

                    Assert.True(result.HasSelection);
                    Assert.False(result.IsCanceled);
                    Assert.Equal(0, result.Index);
                    Assert.Equal(new[] { 0, 1 }, result.Indexes.ToArray());
                }

                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic table selection maps selected visible indexes back to transformed model rows.
        /// </summary>
        [Fact]
        public void DynamicTableView_Select_ReturnsVisibleItemsAfterSearchAndSort()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                scope.Driver.EnqueueKey(Key(ConsoleKey.DownArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Spacebar, ' '));
                scope.Driver.EnqueueKey(Key(ConsoleKey.DownArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Spacebar, ' '));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                var rows = new[]
                {
                    new ServiceRow { Name = "Auth", Status = "Ready", Retries = 0 },
                    new ServiceRow { Name = "Payments", Status = "Needs attention", Retries = 3 },
                    new ServiceRow { Name = "Billing", Status = "Needs attention", Retries = 2 },
                    new ServiceRow { Name = "Search", Status = "Ready", Retries = 4 }
                };

                var table = new DynamicTableView<ServiceRow>(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeader(row => row.Name);
                table.AddHeader(row => row.Status);
                table.AddRows(rows);
                table.Search("needs")
                    .SortBy(row => row.Retries, descending: true);

                TableSelectionResult<ServiceRow> result = table.Select("Choose services", allowEmpty: false);

                Assert.True(result.HasSelection);
                Assert.Equal(new[] { 0, 1 }, result.Indexes.ToArray());
                Assert.Equal(new[] { "Payments", "Billing" }, result.SelectedItems.Select(row => row.Name).ToArray());
                Assert.Equal("Payments", result.SelectedItem.Name);
                Assert.False(result.IsCanceled);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Table selection reports cancellation distinctly from accepted empty selections.
        /// </summary>
        [Fact]
        public void TableView_Select_ReturnsCanceledWhenTokenIsCanceled()
        {
            using (new ConsoleDriverScope(64))
            using (var cancellation = new CancellationTokenSource())
            using (var table = new TableView(new TableRenderOptions { ShowFooterHelp = false }))
            {
                table.AddHeaders("Service", "State");
                table.AddRow(new[] { "Auth", "Ready" });
                cancellation.Cancel();

                TableSelectionResult result = table.Select("Choose services", allowEmpty: true, cancellationToken: cancellation.Token);

                Assert.True(result.IsCanceled);
                Assert.False(result.HasSelection);
                Assert.False(result.IsEmpty);
                Assert.Equal(-1, result.Index);
            }
        }

        /// <summary>
        /// Selector-based table columns use model metadata when no explicit label is supplied.
        /// </summary>
        [Fact]
        public void DynamicTableView_AddHeader_UsesModelMetadataLabel()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var rows = new[]
                {
                    new MetadataServiceRow { ServiceName = "Worker", Status = "Ready", RetryCount = 1 }
                };

                var table = new DynamicTableView<MetadataServiceRow>(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeader(row => row.ServiceName);
                table.AddHeader(row => row.Status);
                table.AddRows(rows);
                table.RenderInline();

                Assert.Contains("Service", table.Headers);
                Assert.Contains("Current state", table.Headers);
                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "Current state");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Selector-based table columns prefer table metadata labels before generic display metadata.
        /// </summary>
        [Fact]
        public void DynamicTableView_AddHeader_UsesTableColumnLabelBeforeDisplayMetadata()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var rows = new[]
                {
                    new TableColumnMetadataServiceRow { Name = "Worker", Status = "Ready", RetryCount = 1 }
                };

                var table = new DynamicTableView<TableColumnMetadataServiceRow>(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeader(row => row.Status);
                table.AddRows(rows);
                table.RenderInline();

                Assert.Contains("State", table.Headers);
                Assert.DoesNotContain("Displayed status", table.Headers);
                VisualAssert.ContainsText(scope.Driver, "State");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic tables render null cell values as empty text instead of throwing.
        /// </summary>
        [Fact]
        public void DynamicTableView_NullValues_RenderAsEmptyCells()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "Billing", Status = null, Retries = 0 }
                };

                DynamicTableView<ServiceRow> table = Consoul.Table(rows, row => row.Name, row => row.Status);

                Assert.Contains("Name", table.Headers);
                Assert.Contains("Status", table.Headers);
                VisualAssert.ContainsText(scope.Driver, "Billing");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic tables can add model-backed optional columns after rows are loaded.
        /// </summary>
        [Fact]
        public void DynamicTableView_AddOptionalColumn_AddsOnlyWhenVisibleValuesExist()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var rows = new[]
                {
                    new ServiceRow { Name = "Billing", Status = "Ready", Details = null },
                    new ServiceRow { Name = "Worker", Status = "Waiting", Details = new Markup("[yellow]retry[/]") }
                };
                var table = new DynamicTableView<ServiceRow>(new TableRenderOptions { ShowFooterHelp = false });

                table.AddHeader(row => row.Name);
                table.AddRows(rows);
                bool addedDetails = table.AddOptionalHeader(row => row.Details, "Details");
                bool addedOwner = table.AddOptionalColumn("Owner", row => string.Empty);

                table.Render();

                Assert.True(addedDetails);
                Assert.False(addedOwner);
                Assert.Contains("Name", table.Headers);
                Assert.Contains("Details", table.Headers);
                Assert.DoesNotContain("Owner", table.Headers);
                VisualAssert.ContainsText(scope.Driver, "Billing");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "retry");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("retry") && write.Color == ConsoleColor.Yellow);
                Assert.DoesNotContain("Owner", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style table column padding helpers affect manual table cells without overflowing.
        /// </summary>
        [Fact]
        public void TableColumn_PaddingHelpers_RenderPaddedManualCells()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var table = new Table(new TableRenderOptions { ShowFooterHelp = false })
                    .Width(48)
                    .RoundedBorder()
                    .HideFooter()
                    .AddColumn(new TableColumn("Service").Padding(2, 3))
                    .AddColumn(new TableColumn("State").PadLeft(0).PadRight(2))
                    .AddRow("API", "OK");

                table.RenderInline();

                string row = scope.Driver.Lines.First(line => line.Contains("API") && line.Contains("OK"));
                Assert.Contains("  API", row);
                Assert.Contains("OK  ", row);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style vertical table column padding renders bounded blank rows around cell content.
        /// </summary>
        [Fact]
        public void TableColumn_VerticalPadding_RendersBlankRowsAroundContent()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var table = new Table(new TableRenderOptions { ShowFooterHelp = false })
                    .Width(36)
                    .AsciiBorder()
                    .HideFooter()
                    .AddColumn(new TableColumn("Service").Padding(new Padding(1, 1, 1, 1)))
                    .AddColumn(new TableColumn("State"))
                    .AddRow("API", "OK");

                table.RenderInline();

                List<string> lines = scope.Driver.Lines.ToList();
                int contentRow = lines.FindIndex(line => line.Contains("API") && line.Contains("OK"));

                Assert.True(contentRow > 0, "The content row should be rendered after a padding row.");
                Assert.True(contentRow < lines.Count - 1, "The content row should be rendered before a padding row.");
                Assert.True(IsBlankAsciiTableRow(lines[contentRow - 1]), "Expected a blank top-padding row.");
                Assert.True(IsBlankAsciiTableRow(lines[contentRow + 1]), "Expected a blank bottom-padding row.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// TableView column padding also works with generated row-number columns.
        /// </summary>
        [Fact]
        public void TableView_SetColumnPadding_WithRowNumbers_KeepsCellsInBounds()
        {
            using (var scope = new ConsoleDriverScope(40))
            using (var table = new TableView(new TableRenderOptions
            {
                ShowFooterHelp = false,
                ShowRowNumbers = true
            }))
            {
                table.AddHeaders("Service", "State");
                table.SetColumnPadding(0, new Padding(0, 0, 2, 0));
                table.SetColumnPadding(1, new Padding(2, 0, 0, 0));
                table.AddRow(new[] { "Auth", "Ready" });

                table.RenderInline();

                string row = scope.Driver.Lines.First(line => line.Contains("Auth") && line.Contains("Ready"));
                Assert.Contains("Auth  ", row);
                Assert.Contains("  Ready", row);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// TableView header and footer cell style setters stay aligned when row numbers are rendered.
        /// </summary>
        [Fact]
        public void TableView_SetHeaderAndFooterStyles_WithRowNumbers_StylesDataCells()
        {
            using (var scope = new ConsoleDriverScope(48))
            using (var table = new TableView(new TableRenderOptions
            {
                ShowFooterHelp = false,
                ShowRowNumbers = true
            }))
            {
                table.AddHeaders("Service", "State");
                table.SetHeaderStyle(0, new Style(ConsoleColor.Cyan));
                table.SetHeaderStyle(1, new Style(ConsoleColor.Magenta));
                table.SetColumnFooter(0, "Total");
                table.SetColumnFooter(1, "2");
                table.SetColumnFooterStyle(0, new Style(ConsoleColor.Yellow));
                table.SetColumnFooterStyle(1, new Style(ConsoleColor.Green));
                table.AddRow(new[] { "Auth", "Ready" });

                table.RenderInline();

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("State") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Total") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("2") && write.Color == ConsoleColor.Green);
                VisualAssert.ContainsText(scope.Driver, "Auth");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private static ConsoleKeyInfo Key(ConsoleKey key, char keyChar = '\0')
        {
            return new ConsoleKeyInfo(keyChar, key, false, false, false);
        }

        private static bool IsBlankAsciiTableRow(string line)
        {
            return line.Contains("|") && string.IsNullOrWhiteSpace(line.Replace("|", string.Empty));
        }

        private static TableView CreateComplexTable(TableRenderOptions? options = null)
        {
            var table = new TableView(options);
            table.AddHeaders("ID", "Name", "Description", "Owner", "Status");
            table.AddRow(new[] { "1", "Widget", "Supercalifragilisticexpialidocious component", "Operations", "Ready" });
            table.AddRow(new[] { "2", string.Empty, "Short", "QA" });
            table.AddRow(new[] { "3", "Many columns", "Uneven row", "Platform", "Blocked", "Extra" });
            table.AddRow(new[] { "4", "Telemetry", "Measures terminal rendering behavior under narrow widths", "DevEx", "Active" });
            return table;
        }

        private static string[] GetRenderedCells(FakeConsoleDriver driver, Func<string, bool> predicate)
        {
            string line = driver.Lines.First(predicate);
            string[] parts = line.Split('│');
            return parts.Skip(1).Take(Math.Max(0, parts.Length - 2)).ToArray();
        }

        private static void AssertRightAligned(string cell, string value)
        {
            string contentArea = GetContentArea(cell);
            int valueStart = contentArea.IndexOf(value, StringComparison.Ordinal);

            Assert.Equal(value, contentArea.Trim());
            Assert.True(valueStart > 0, "Expected '" + value + "' to be right-aligned in cell '" + cell + "'.");
        }

        private static void AssertCentered(string cell, string value)
        {
            string contentArea = GetContentArea(cell);
            int valueStart = contentArea.IndexOf(value, StringComparison.Ordinal);
            int leftPadding = valueStart;
            int rightPadding = contentArea.Length - valueStart - value.Length;

            Assert.Equal(value, contentArea.Trim());
            Assert.InRange(Math.Abs(leftPadding - rightPadding), 0, 1);
        }

        private static string GetContentArea(string cell)
        {
            return string.IsNullOrEmpty(cell) ? string.Empty : cell.Substring(1);
        }

        private sealed class ServiceRow
        {
            public string Name { get; set; } = string.Empty;

            public string? Status { get; set; }

            public int Retries { get; set; }

            public object? Details { get; set; }
        }

        private sealed class MetadataServiceRow
        {
            [Display(Name = "Service")]
            public string ServiceName { get; set; } = string.Empty;

            [Description("Current state")]
            public string? Status { get; set; }

            [DisplayName("Owner")]
            public string OwnerName { get; set; } = string.Empty;

            public int RetryCount { get; set; }
        }

        private sealed class TableColumnMetadataServiceRow
        {
            [TableColumn("Service", Order = 2)]
            public string Name { get; set; } = string.Empty;

            [DisplayName("Displayed status")]
            [TableColumn("State", Order = 1)]
            public string? Status { get; set; }

            [TableColumn(Visible = false)]
            public string Secret { get; set; } = string.Empty;

            [TableColumn("Retry Count", Order = 3, Alignment = TableCellAlignment.Right)]
            public int RetryCount { get; set; }
        }

        private sealed class FormattedServiceRow
        {
            [TableColumn("Started", Order = 1, Format = "yyyy-MM-dd")]
            public DateTime Started { get; set; }

            [TableColumn("Duration", Order = 2, Format = "{0} ms", Alignment = TableCellAlignment.Right)]
            public int DurationMs { get; set; }

            [DisplayFormat(DataFormatString = "build-{0:000}")]
            [TableColumn("Build", Order = 3)]
            public int BuildNumber { get; set; }
        }
    }
}
