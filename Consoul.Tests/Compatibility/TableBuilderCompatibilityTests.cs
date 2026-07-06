using ConsoulLibrary.Tests.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConsoulLibrary.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests Spectre-style table builder compatibility.
    /// </summary>
    public class TableBuilderCompatibilityTests
    {
        /// <summary>
        /// Fluent manual tables render through the existing table renderer.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var table = new Table()
                    .Title("Jobs")
                    .Caption("Rendered by the Consoul table engine.")
                    .Border(TableBorderStyle.Rounded)
                    .HideFooter()
                    .AddColumn("Job")
                    .AddColumn("State")
                    .AddRow("Build", "Ready")
                    .AddRow("Deploy", "Waiting");

                AnsiConsole.Write(table);

                VisualAssert.ContainsText(scope.Driver, "Jobs");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "Rendered by the Consoul table engine.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Reusable title and caption objects project into the existing table render options.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_AcceptsReusableTitleAndCaptionObjects()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var title = new TableTitle("Deployments")
                    .SetAlignment(Justify.Left)
                    .Color(ConsoleColor.Green);
                var caption = new TableCaption("2 jobs", "white on navy", Justify.Right);

                var table = new Table()
                    .Width(42)
                    .AsciiBorder()
                    .Title(title)
                    .Caption(caption)
                    .HideFooter()
                    .AddColumns("Job", "State")
                    .AddRow("Build", "Ready")
                    .AddRow("Deploy", "Waiting");

                AnsiConsole.Write(table);

                Assert.Equal("Deployments", table.TableRenderOptions.Title);
                Assert.Equal(TextBlockAlignment.Left, table.TableRenderOptions.TitleAlignment);
                Assert.Equal("2 jobs", table.TableRenderOptions.Caption);
                Assert.Equal(TextBlockAlignment.Right, table.TableRenderOptions.CaptionAlignment);
                VisualAssert.ContainsText(scope.Driver, "Deployments");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "2 jobs");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployments") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("2 jobs") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 42, "Table line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-shaped title overloads also configure captions through the shared table render options.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_AcceptsInlineTitleStylesAndTitleObjectCaption()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var table = new Table()
                    .Width(44)
                    .AsciiBorder()
                    .Title("Runtime", new Style(ConsoleColor.Yellow), Justify.Right)
                    .Caption(new TableTitle("updated now", new Style(ConsoleColor.DarkGray), Justify.Center))
                    .HideFooter()
                    .AddColumns("Service", "State")
                    .AddRow("API", "Ready");

                AnsiConsole.Write(table);

                Assert.Equal("Runtime", table.TableRenderOptions.Title);
                Assert.Equal(TextBlockAlignment.Right, table.TableRenderOptions.TitleAlignment);
                Assert.Equal("updated now", table.TableRenderOptions.Caption);
                Assert.Equal(TextBlockAlignment.Center, table.TableRenderOptions.CaptionAlignment);
                VisualAssert.ContainsText(scope.Driver, "Runtime");
                VisualAssert.ContainsText(scope.Driver, "updated now");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Runtime") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("updated now") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Table line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Raw Spectre-style markup strings in manual table snippets render as visible styled cell text.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_TreatsRawMarkupStringsAsDisplayText()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                var table = new Table()
                    .Width(32)
                    .AsciiBorder()
                    .HideFooter()
                    .AddColumns("[cyan]Service[/]", "[yellow]State[/]")
                    .AddRow("[green]API[/]", "[white on navy]Ready[/]")
                    .AddRow("[[literal]]", "[red]Blocked[/]");

                AnsiConsole.Write(table);

                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "State");
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "[literal]");
                VisualAssert.ContainsText(scope.Driver, "Blocked");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("State") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Blocked") && write.Color == ConsoleColor.Red);
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.DoesNotContain("[/]", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent table methods can configure row numbers, width, and alignment while remaining capturable.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_CapturesResponsiveTable()
        {
            var table = new Table()
                .Expand()
                .ShowRowNumbers()
                .AddColumns("Service", "Retries")
                .SetColumnAlignment(1, TableCellAlignment.Right)
                .AddRows(new[]
                {
                    new[] { "API", "2" },
                    new[] { "Worker", "12" }
                });

            string captured = AnsiConsole.Capture(table, width: 36);

            Assert.Contains("Service", captured);
            Assert.Contains("Worker", captured);
            Assert.Contains("12", captured);
        }

        /// <summary>
        /// Fluent table polish helpers configure width, colors, alignment, and separators.
        /// </summary>
        [Fact]
        public void Table_FluentPolishHelpers_RenderConfiguredTable()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var table = new Table()
                    .Width(36)
                    .RoundedBorder()
                    .BorderColor(System.ConsoleColor.DarkGray)
                    .Title("Deployments")
                    .TitleAlignment(TextBlockAlignment.Left)
                    .TitleColor(System.ConsoleColor.Green)
                    .Caption("2 jobs")
                    .CaptionAlignment(TextBlockAlignment.Right)
                    .CaptionColor(System.ConsoleColor.DarkYellow)
                    .HeaderColor(System.ConsoleColor.Yellow)
                    .ContentColor(System.ConsoleColor.White)
                    .AlternatingRowColor(System.ConsoleColor.Cyan)
                    .ShowFooter(false)
                    .ShowHeaders()
                    .ShowRowNumbers()
                    .RowNumberHeader("No")
                    .ShowRowSeparators(false)
                    .AddColumns("Job", "Retries")
                    .SetColumnAlignment(1, TableCellAlignment.Right)
                    .AddRow("Build", 2)
                    .AddRow("Deploy", 12);

                AnsiConsole.Write(table);

                VisualAssert.ContainsText(scope.Driver, "Deployments");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "12");
                VisualAssert.ContainsText(scope.Driver, "2 jobs");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployments") && write.Color == System.ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Job") && write.Color == System.ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == System.ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == System.ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("2 jobs") && write.Color == System.ConsoleColor.DarkYellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("╭") && write.Color == System.ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Table line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent table color helpers accept Spectre-style style strings.
        /// </summary>
        [Fact]
        public void Table_FluentPolishHelpers_AcceptStyleStrings()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var table = new Table()
                    .Width(36)
                    .AsciiBorder()
                    .BorderColor("grey")
                    .Title("Deployments")
                    .TitleColor("#22c55e")
                    .Caption("2 jobs")
                    .CaptionColor("white on navy")
                    .HeaderColor("yellow")
                    .ContentColor("white")
                    .AlternatingRowColor("aqua")
                    .HideFooter()
                    .AddColumns("Job", "State")
                    .AddRow("Build", "Ready")
                    .AddRow("Deploy", "Waiting");

                AnsiConsole.Write(table);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployments") && write.Color == ConsoleColor.DarkCyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Job") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("2 jobs") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("+") && write.Color == ConsoleColor.Gray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Explicit table width remains bounded when the terminal is resized between captures.
        /// </summary>
        [Fact]
        public void Table_Width_StaysBoundedAcrossCaptureWidths()
        {
            var table = new Table()
                .Width(30)
                .AsciiBorder()
                .HideFooter()
                .AddColumns("Service", "State")
                .AddRow("API", "Ready")
                .AddRow("Worker", "Waiting");

            string wideCapture = AnsiConsole.Capture(table, width: 72);
            string narrowCapture = AnsiConsole.Capture(table, width: 20);

            Assert.All(SplitLines(wideCapture), line => Assert.True(line.Length <= 30, "Wide capture line exceeded requested width: " + line));
            Assert.All(SplitLines(narrowCapture), line => Assert.True(line.Length <= 20, "Narrow capture line exceeded terminal width: " + line));
        }

        /// <summary>
        /// Collapse keeps manual POC tables content-sized while Expand restores full-width layout.
        /// </summary>
        [Fact]
        public void Table_Collapse_UsesContentWidthAndExpandRestoresFullWidth()
        {
            var table = new Table()
                .AsciiBorder()
                .HideFooter()
                .AddColumns("Job", "State")
                .AddRow("Build", "OK");

            string collapsed = AnsiConsole.Capture(table.Collapse(), width: 60);
            string expanded = AnsiConsole.Capture(table.Expand(), width: 60);

            string[] collapsedLines = SplitLines(collapsed).Where(line => line.Length > 0).ToArray();
            string[] expandedLines = SplitLines(expanded).Where(line => line.Length > 0).ToArray();
            int collapsedWidth = collapsedLines.Max(line => line.Length);
            int expandedWidth = expandedLines.Max(line => line.Length);

            Assert.True(collapsedWidth < 60, "Collapsed table should stay content-sized.");
            Assert.Equal(60, expandedWidth);
            Assert.All(collapsedLines, line => Assert.True(line.Length <= 60, "Collapsed capture line exceeded terminal width: " + line));
            Assert.All(expandedLines, line => Assert.True(line.Length <= 60, "Expanded capture line exceeded terminal width: " + line));
        }

        /// <summary>
        /// Manual table rows can be mutated for live dashboard snippets.
        /// </summary>
        [Fact]
        public void Table_RowsCollection_UpdatesAndClearsRows()
        {
            var table = new Table()
                .Width(42)
                .HideFooter()
                .AddColumns("Service", "State")
                .AddRow("API", "Starting")
                .AddRow("Worker", "Queued");

            Assert.Equal(2, table.Rows.Count);
            table.Rows.Update(0, 1, new Markup("[green]Ready[/]"));
            table.Rows.Update(1, "Worker", "Running");

            string updated = AnsiConsole.Capture(table, width: 48);

            Assert.Contains("API", updated);
            Assert.Contains("Ready", updated);
            Assert.Contains("Running", updated);

            table.Rows.Clear();
            table.Rows.Add("Cache", "Online");

            string cleared = AnsiConsole.Capture(table, width: 48);

            Assert.Equal(1, table.Rows.Count);
            Assert.Contains("Cache", cleared);
            Assert.DoesNotContain("Worker", cleared);
        }

        /// <summary>
        /// Spectre-style table mutation aliases insert, update, remove, and add empty rows.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_MutatesRowsWithSpectreAliases()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var table = new Table()
                    .Width(42)
                    .AsciiBorder()
                    .HideFooter()
                    .AddColumns("Job", "State")
                    .AddRow("Build", "Queued")
                    .AddEmptyRow()
                    .InsertRow(1, "Deploy", "Waiting")
                    .UpdateCell(0, 1, new Markup("[green]Ready[/]"))
                    .RemoveRow(2);

                table.Rows.Insert(2, "Smoke", 1);
                table.Rows.Update(2, 1, "Passing");

                AnsiConsole.Write(table);

                Assert.Equal(3, table.Rows.Count);
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "Waiting");
                VisualAssert.ContainsText(scope.Driver, "Smoke");
                VisualAssert.ContainsText(scope.Driver, "Passing");
                Assert.DoesNotContain("Queued", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style plural footer aliases control configured column footer rows without changing footer help behavior.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_ShowAndHideFootersControlColumnFooters()
        {
            var table = new Table()
                .Width(36)
                .AsciiBorder()
                .HideFooter()
                .FooterColor("grey")
                .AddColumns(
                    new TableColumn("Job").Footer("Total"),
                    new TableColumn("Retries").RightAligned().Footer("14"))
                .AddRow("Build", 2)
                .AddRow("Deploy", 12);

            string visible = AnsiConsole.Capture(table, width: 48);

            table.HideFooters();
            string hidden = AnsiConsole.Capture(table, width: 48);

            table.ShowFooters();
            string restored = AnsiConsole.Capture(table, width: 48);

            Assert.Contains("Total", visible);
            Assert.Contains("14", visible);
            Assert.DoesNotContain("Total", hidden);
            Assert.DoesNotContain("14", hidden);
            Assert.Contains("Total", restored);
            Assert.Contains("14", restored);
        }

        /// <summary>
        /// Spectre-style table column objects configure header text, alignment, width, and column footers.
        /// </summary>
        [Fact]
        public void Table_TableColumnBuilder_ConfiguresAlignmentWidthAndFooters()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var table = new Table()
                    .Width(34)
                    .AsciiBorder()
                    .HideFooter()
                    .FooterColor("grey")
                    .AddColumn(new TableColumn("Job") { Width = 12 })
                    .AddColumn(new TableColumn("Retries").RightAligned().Width(9).Footer("14").NoWrap())
                    .AddRow("Build", 2)
                    .AddRow("Deploy", 12);

                AnsiConsole.Write(table);

                VisualAssert.ContainsText(scope.Driver, "Job");
                VisualAssert.ContainsText(scope.Driver, "Retries");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "14");
                AssertRightAlignedCell(scope.Driver.Lines.First(line => line.Contains("Build")), "2");
                AssertRightAlignedCell(scope.Driver.Lines.First(line => line.Contains("Deploy")), "12");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("14") && write.Color == ConsoleColor.Gray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 34, "Table line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Table column headers and footers can be captured from reusable renderables.
        /// </summary>
        [Fact]
        public void Table_TableColumnBuilder_CapturesRenderableHeaderAndFooter()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var table = new Table()
                    .Width(40)
                    .RoundedBorder()
                    .HideFooter()
                    .AddColumns(
                        new TableColumn(new Markup("[cyan]Service[/]")).Width(16),
                        new TableColumn("State").Centered().Footer(new Markup("[green]ready[/]")))
                    .AddRow("API", "Online");

                AnsiConsole.Write(table);

                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "State");
                VisualAssert.ContainsText(scope.Driver, "Online");
                VisualAssert.ContainsText(scope.Driver, "ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ready") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Table aliases accept Spectre-style justification vocabulary.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_AcceptsJustifyAliases()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var table = new Table()
                    .Width(40)
                    .AsciiBorder()
                    .HideFooter()
                    .Title("Jobs")
                    .TitleAlignment(Justify.Left)
                    .Caption("2 jobs")
                    .CaptionAlignment(Justify.Right)
                    .HeaderAlignment(Justify.Center)
                    .ContentAlignment(Justify.Left)
                    .AddColumn("Job")
                    .AddColumn(new TableColumn("Retries").Justify(Justify.Right))
                    .AddRow("Build", 2)
                    .AddRow("Deploy", 12);

                AnsiConsole.Write(table);

                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("Jobs", StringComparison.Ordinal));
                Assert.Contains(scope.Driver.Lines, line => line.EndsWith("2 jobs", StringComparison.Ordinal));
                AssertRightAlignedCell(scope.Driver.Lines.First(line => line.Contains("Build")), "2");
                AssertRightAlignedCell(scope.Driver.Lines.First(line => line.Contains("Deploy")), "12");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Manual table builders expose justified aliases that match text-like builder vocabulary.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_ProvidesJustifiedAlignmentAliases()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var retries = new TableColumn("Retries")
                    .RightJustified()
                    .Width(9);
                var table = new Table()
                    .Width(40)
                    .AsciiBorder()
                    .HideFooter()
                    .LeftJustified()
                    .AddColumn(new TableColumn("Job").LeftJustified().Width(12))
                    .AddColumn(retries)
                    .AddRow("Build", 2)
                    .RightJustified();

                AnsiConsole.Write(table);

                Assert.Equal(TableCellAlignment.Right, table.TableRenderOptions.HeaderAlignment);
                Assert.Equal(TableCellAlignment.Right, table.TableRenderOptions.ContentAlignment);
                Assert.Equal(TableCellAlignment.Left, table.Columns[0].Alignment);
                Assert.Equal(TableCellAlignment.Right, retries.Alignment);
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "2");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Table border objects, configure-column lambdas, and color aliases support pasted builder snippets.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_AcceptsBorderObjectsConfigureColumnsAndColorAliases()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var table = new Table()
                    .Width(44)
                    .Border(TableBorder.Double)
                    .BorderColor(Colors.Aqua)
                    .HideFooter()
                    .FooterColor(Colors.Grey)
                    .AddColumn("Job", column => column.Width(14).LeftAligned())
                    .AddColumn("Retries", column => column.Width(9).RightAligned().Footer("2"))
                    .AddRow("Build", 2)
                    .AddRow("Deploy", 0);

                AnsiConsole.Write(table);

                VisualAssert.ContainsText(scope.Driver, "Job");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "2");
                VisualAssert.ContainsText(scope.Driver, "╔");
                VisualAssert.ContainsText(scope.Driver, "╬");
                VisualAssert.ContainsText(scope.Driver, "╝");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("╔") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("2") && write.Color == ConsoleColor.Gray);
                AssertRightAlignedCell(scope.Driver.Lines.First(line => line.Contains("Build")), "2");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// The table border none preset renders content without legacy or preset separator lines.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_TableBorderNoneSuppressesSeparators()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var table = new Table()
                    .Width(30)
                    .Border(TableBorder.None)
                    .HideFooter()
                    .AddColumns("Job", "State")
                    .AddRow("Build", "Ready");

                AnsiConsole.Write(table);

                VisualAssert.ContainsText(scope.Driver, "Job");
                VisualAssert.ContainsText(scope.Driver, "Build");
                Assert.DoesNotContain("─", scope.Driver.Text);
                Assert.DoesNotContain("│", scope.Driver.Text);
                Assert.DoesNotContain("+", scope.Driver.Text);
                Assert.DoesNotContain("╔", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Public table columns stay synchronized with the underlying table metadata after they are added.
        /// </summary>
        [Fact]
        public void Table_ColumnsCollection_MutatesRenderedColumnMetadata()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var table = new Table()
                    .Width(40)
                    .AsciiBorder()
                    .HideFooter()
                    .AddColumns("Job", "Retries")
                    .AddRow("Build", 2)
                    .AddRow("Deploy", 0);

                table.Columns[0].Header = "Task";
                table.Columns[0].Width = 14;
                table.Columns[1].Alignment = TableCellAlignment.Right;
                table.Columns[1].Padding = new Padding(1, 0, 1, 0);
                table.Columns[1].Footer = new Text("2", new Style(foreground: Colors.Yellow, decoration: Decoration.Bold));
                string footerText = table.Columns[1].Footer;

                AnsiConsole.Write(table);

                Assert.Equal(2, table.Columns.Count);
                Assert.Equal("2", footerText);
                VisualAssert.ContainsText(scope.Driver, "Task");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "2");
                Assert.DoesNotContain("Job", scope.Driver.Text);
                AssertRightAlignedCell(scope.Driver.Lines.First(line => line.Contains("Build")), "2");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("2") && write.Color == ConsoleColor.Gray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Manual tables capture renderable row cells instead of rendering implementation type names.
        /// </summary>
        [Fact]
        public void Table_FluentBuilder_CapturesRenderableCellsForMixedContentRows()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var style = new Style(foreground: Colors.Aqua, decoration: Decoration.Italic);
                var innerTable = new Table()
                    .Width(18)
                    .Border(TableBorder.None)
                    .HideFooter()
                    .HideHeaders()
                    .AddColumns("Detail", "Value")
                    .AddRow("CPU", "95%");

                var table = new Table()
                    .Width(58)
                    .AsciiBorder()
                    .HideFooter()
                    .AddColumn("Type", column => column.Width(10))
                    .AddColumn("Example")
                    .AddRow(new Text("Markup"), new Markup("[green]Success[/] :check_mark:"))
                    .AddRow(new Text("Text", style), new Text("Styled Text", style))
                    .InsertRow(1, new Text("Panel"), new Panel("[yellow]Warning[/]").AsciiBorder().Width(18).Padding(0))
                    .InsertRow(3, new Text("Nested"), innerTable)
                    .UpdateCell(0, 1, new Markup("[green]Complete[/]"));

                AnsiConsole.Write(table);

                Assert.Equal(Decoration.Italic, style.Decoration);
                VisualAssert.ContainsText(scope.Driver, "Markup");
                VisualAssert.ContainsText(scope.Driver, "Complete");
                VisualAssert.ContainsText(scope.Driver, "Panel");
                VisualAssert.ContainsText(scope.Driver, "Warning");
                VisualAssert.ContainsText(scope.Driver, "Nested");
                VisualAssert.ContainsText(scope.Driver, "CPU");
                VisualAssert.ContainsText(scope.Driver, "Styled Text");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Complete") && write.Color == ConsoleColor.Green);
                Assert.DoesNotContain("ConsoulLibrary.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Live row collection object overloads capture renderable content consistently with table row builders.
        /// </summary>
        [Fact]
        public void Table_RowsCollection_CapturesRenderableObjects()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var table = new Table()
                    .Width(42)
                    .AsciiBorder()
                    .HideFooter()
                    .AddColumn("Service", column => column.Width(12))
                    .AddColumn("State");

                table.Rows
                    .Add(new Markup("[cyan]API[/]"), new Markup("[yellow]Starting[/]"))
                    .Insert(1, new Text("Worker", new Style(foreground: Colors.Green)), new Panel("Queued").AsciiBorder().Width(12).Padding(0))
                    .Update(0, 1, new Markup("[green]Ready[/]"));

                AnsiConsole.Write(table);

                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "Queued");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.DoesNotContain("ConsoulLibrary.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Manual tables can project model collections without losing renderable cell support.
        /// </summary>
        [Fact]
        public void Table_AddRows_ProjectsModelCollectionsWithRenderableCells()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var jobs = new[]
                {
                    new ManualTableJob("API", "Ready", 0, false),
                    new ManualTableJob("Worker", "Waiting", 2, true)
                };

                var table = new Table()
                    .Width(52)
                    .RoundedBorder()
                    .HideFooter()
                    .AddColumn("Service", column => column.Width(12))
                    .AddColumn("State")
                    .AddColumn("Retries", column => column.RightAligned())
                    .AddRows(
                        jobs,
                        job => new Text(job.Service, new Style(foreground: job.Warning ? Colors.Yellow : Colors.Green)),
                        job => new Markup(job.State == "Ready" ? "[green]Ready[/]" : "[yellow]Waiting[/]"),
                        job => job.Retries);

                AnsiConsole.Write(table);

                Assert.Equal(2, table.Rows.Count);
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Waiting");
                VisualAssert.ContainsText(scope.Driver, "2");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting") && write.Color == ConsoleColor.Yellow);
                Assert.DoesNotContain("ConsoulLibrary.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed manual rows fail early when selector delegates are missing.
        /// </summary>
        [Fact]
        public void Table_AddRows_RejectsNullModelSelectors()
        {
            var jobs = new[]
            {
                new ManualTableJob("API", "Ready", 0, false)
            };

            Assert.Throws<ArgumentNullException>(() => new Table().AddRows(jobs, null!));
            Assert.Throws<ArgumentNullException>(() => new Table().AddRows(jobs, job => job.Service, null!));
        }

        /// <summary>
        /// Key/value helpers make quick diagnostic tables without manual two-column row construction.
        /// </summary>
        [Fact]
        public void Table_AddKeyValueRows_AddsDefaultColumnsAndFormatsValues()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var settings = new Dictionary<string, object>
                {
                    { "Environment", "Development" },
                    { "Retries", 2 },
                    { "State", new Markup("[green]Ready[/]") }
                };

                var table = new Table()
                    .Width(48)
                    .AsciiBorder()
                    .HideFooter()
                    .AddKeyValueRows(settings);

                AnsiConsole.Write(table);

                Assert.Equal(2, table.Columns.Count);
                VisualAssert.ContainsText(scope.Driver, "Key");
                VisualAssert.ContainsText(scope.Driver, "Value");
                VisualAssert.ContainsText(scope.Driver, "Environment");
                VisualAssert.ContainsText(scope.Driver, "Development");
                VisualAssert.ContainsText(scope.Driver, "Retries");
                VisualAssert.ContainsText(scope.Driver, "2");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.DoesNotContain("ConsoulLibrary.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Key/value helpers respect columns that POC views already named explicitly.
        /// </summary>
        [Fact]
        public void Table_AddKeyValueRows_UsesExistingColumnsWhenConfigured()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var values = new[]
                {
                    new KeyValuePair<string, object>("Mode", "POC"),
                    new KeyValuePair<string, object>("Latency", 123.4m)
                };

                var table = new Table()
                    .Width(40)
                    .RoundedBorder()
                    .HideFooter()
                    .AddColumns("Setting", "Current")
                    .AddKeyValueRows(values);

                AnsiConsole.Write(table);

                Assert.Equal(2, table.Columns.Count);
                VisualAssert.ContainsText(scope.Driver, "Setting");
                VisualAssert.ContainsText(scope.Driver, "Current");
                VisualAssert.ContainsText(scope.Driver, "Mode");
                VisualAssert.ContainsText(scope.Driver, "POC");
                VisualAssert.ContainsText(scope.Driver, "Latency");
                Assert.DoesNotContain("Key", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Legacy dictionary helpers support quick settings bags without making generic dictionary overloads ambiguous.
        /// </summary>
        [Fact]
        public void Table_AddDictionaryRows_RendersNonGenericDictionaryEntries()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var values = new Hashtable
                {
                    { "Queue", "active" },
                    { "Workers", 4 }
                };

                var table = new Table()
                    .Width(40)
                    .AsciiBorder()
                    .HideFooter()
                    .AddDictionaryRows(values, "Name", "Current");

                AnsiConsole.Write(table);

                VisualAssert.ContainsText(scope.Driver, "Name");
                VisualAssert.ContainsText(scope.Driver, "Current");
                VisualAssert.ContainsText(scope.Driver, "Queue");
                VisualAssert.ContainsText(scope.Driver, "active");
                VisualAssert.ContainsText(scope.Driver, "Workers");
                VisualAssert.ContainsText(scope.Driver, "4");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private static string[] SplitLines(string value)
        {
            return (value ?? string.Empty).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        private sealed class ManualTableJob
        {
            public ManualTableJob(string service, string state, int retries, bool warning)
            {
                Service = service;
                State = state;
                Retries = retries;
                Warning = warning;
            }

            public string Service { get; private set; }

            public string State { get; private set; }

            public int Retries { get; private set; }

            public bool Warning { get; private set; }
        }

        private static void AssertRightAlignedCell(string line, string expected)
        {
            string cell = line
                .Split(new[] { '|', '│', '┃', '║' }, StringSplitOptions.RemoveEmptyEntries)
                .First(part => part.Contains(expected));

            Assert.Equal(expected, cell.Trim());
            Assert.True(cell.IndexOf(expected, StringComparison.Ordinal) > 0, "Expected right-aligned cell padding in line: " + line);
        }
    }
}
