using System;
using System.Globalization;
using System.IO;
using ConsoulLibrary.Color;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for column view helpers.
    /// </summary>
    public class ColumnsResponsiveRenderingTests
    {
        /// <summary>
        /// Columns wrap long content and stay inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(20)]
        [InlineData(32)]
        [InlineData(80)]
        public void Columns_LongContent_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Columns(
                    new ColumnsOptions { Columns = 3, MinimumColumnWidth = 10 },
                    "Build queue contains a verylongtokenwithoutnaturalbreaks",
                    "Tests are passing",
                    "Deployment waiting for approval");

                VisualAssert.WroteText(scope.Driver, "Build");
                VisualAssert.WroteText(scope.Driver, "Tests");
                VisualAssert.WroteText(scope.Driver, "Deployment");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Columns honor configured width, headers, and content colors.
        /// </summary>
        [Fact]
        public void Columns_CustomOptions_RenderHeadersAndColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new ColumnsOptions
                {
                    Width = 40,
                    Columns = 2,
                    HeaderScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Yellow,
                        BackgroundColor = ConsoleColor.Black
                    },
                    ContentScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                };

                Consoul.Columns(columns =>
                {
                    columns.Add("Ready", "Build", ConsoleColor.Green);
                    columns.Add("Waiting", "Deploy");
                }, options);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Column line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Column headers and content can contain raw markup without leaking tags or overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(42)]
        public void Columns_RawMarkupHeadersAndContent_DoNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Columns(columns =>
                {
                    columns.Add("[green]Ready[/] for [[prod]]", "[cyan]Status[/]");
                    columns.Add("[white on navy]Approval required for deployment[/]", "[yellow]Gate[/]");
                }, new ColumnsOptions { Columns = 2, MinimumColumnWidth = 10 });

                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "[prod]");
                VisualAssert.WroteText(scope.Driver, "Approval");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Columns stack into fewer columns when the terminal cannot fit the preferred column count.
        /// </summary>
        [Fact]
        public void Columns_NarrowWidth_StacksColumns()
        {
            using (var scope = new ConsoleDriverScope(18))
            {
                Consoul.Columns(
                    new ColumnsOptions { Columns = 3, MinimumColumnWidth = 12 },
                    "Alpha",
                    "Beta",
                    "Gamma");

                Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("Alpha") && line.Contains("Beta"));
                Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("Beta") && line.Contains("Gamma"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Columns can capture existing Consoul render helpers as composable column content.
        /// </summary>
        [Fact]
        public void Columns_CapturesRenderableColumnContent()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                Consoul.Columns(columns =>
                {
                    columns.Add(() => Consoul.Panel("Build complete", title: "Summary", width: 28), "Panel", ConsoleColor.Cyan, captureWidth: 32);
                    columns.Add(() => Consoul.PropertyList(list =>
                    {
                        list.Add("Status", "Ready", ConsoleColor.Green);
                        list.Add("Owner", "Platform");
                    }, new PropertyListOptions { Width = 30, LabelWidth = 8 }), "Properties", ConsoleColor.Green, captureWidth: 32);
                }, new ColumnsOptions { Columns = 2, MinimumColumnWidth = 18 });

                VisualAssert.WroteText(scope.Driver, "Panel");
                VisualAssert.WroteText(scope.Driver, "Summary");
                VisualAssert.WroteText(scope.Driver, "Build complete");
                VisualAssert.WroteText(scope.Driver, "Properties");
                VisualAssert.WroteText(scope.Driver, "Status");
                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Columns can project model collections into headed dashboard sections.
        /// </summary>
        [Fact]
        public void Columns_AddColumns_ProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var statuses = new[]
                {
                    new DashboardStatus("API", "Ready", false),
                    new DashboardStatus("Worker", "Waiting", true)
                };

                new Columns()
                    .Width(56)
                    .ColumnCount(2)
                    .AddColumns(
                        statuses,
                        status => new Markup(status.Warning ? "[yellow]" + status.State + "[/]" : "[green]" + status.State + "[/]"),
                        status => status.Name,
                        status => status.Warning ? (ConsoleColor?)ConsoleColor.Yellow : ConsoleColor.Green)
                    .Render();

                VisualAssert.WroteText(scope.Driver, "API");
                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "Worker");
                VisualAssert.WroteText(scope.Driver, "Waiting");
                Assert.DoesNotContain("ConsoulLibrary.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-projected columns preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void Columns_ProjectionStyleSelectorsEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });
            var statuses = new[]
            {
                new StyledDashboardStatus("API", "Ready", RgbColor.FromHex("#112233"), SpectreColor.FromHex("#445566"))
            };

            var columns = new Columns()
                .Width(48)
                .ColumnCount(2)
                .MinimumColumnWidth(12)
                .Gutter(1)
                .AddColumns(statuses, status => "RGB column " + status.State, status => status.Name, status => status.RgbColor)
                .AddColumns(statuses, status => "Color column " + status.State, status => status.Name + " Color", status => status.SpectreColor)
                .AddColumns(statuses, status => "Style column " + status.State, status => status.Name + " Style", status => "bold #778899");

            console.Write(columns);

            string output = writer.ToString();
            Assert.Contains("\u001b[38;2;17;34;51mRGB column", output);
            Assert.Contains("\u001b[38;2;68;85;102mColor column", output);
            Assert.Contains("\u001b[1;38;2;119;136;153mStyle column", output);
            Assert.DoesNotContain("[#778899", output);
            Assert.DoesNotContain("[bold", output);
        }

        /// <summary>
        /// Column-level default header and content style helpers preserve RGB and decoration metadata.
        /// </summary>
        [Fact]
        public void Columns_DefaultStyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });
            var columns = new Columns()
                .Width(48)
                .ColumnCount(2)
                .MinimumColumnWidth(12)
                .Gutter(1)
                .HeaderColor("bold #112233")
                .ContentColor("italic #445566");
            columns.Add("Ready", "Status");
            columns.Add("Override", "Manual", "underline #778899");

            console.Write(columns);

            string output = writer.ToString();
            Assert.Contains("\u001b[1;38;2;17;34;51m", output);
            Assert.Contains("\u001b[3;38;2;68;85;102m", output);
            Assert.Contains("\u001b[4;38;2;119;136;153m", output);
            Assert.Contains("Status", output);
            Assert.Contains("Ready", output);
            Assert.Contains("Override", output);
            Assert.DoesNotContain("[#112233", output);
            Assert.DoesNotContain("[#445566", output);
            Assert.DoesNotContain("[#778899", output);
        }

        /// <summary>
        /// Model-backed dynamic views can render columns as dashboard sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderColumnsSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var model = new DashboardModel
                    {
                        Status = "Green",
                        Owner = "Platform",
                        NextStep = "Review deployment plan"
                    };

                    var view = Consoul.View("Dashboard", model)
                        .RenderModel(dashboard =>
                        {
                            Consoul.Columns(columns =>
                            {
                                columns.Add(dashboard.Status, "Status", ConsoleColor.Green);
                                columns.Add(dashboard.Owner, "Owner", ConsoleColor.Cyan);
                                columns.Add(dashboard.NextStep, "Next");
                            }, new ColumnsOptions { Columns = 3, MinimumColumnWidth = 12 });
                        })
                        .Option("Refresh", dashboard => dashboard.Status = "Refreshed");

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Status");
                    VisualAssert.WroteText(scope.Driver, "Green");
                    VisualAssert.WroteText(scope.Driver, "Owner");
                    VisualAssert.WroteText(scope.Driver, "Next");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private sealed class DashboardModel
        {
            public string Status { get; set; } = string.Empty;

            public string Owner { get; set; } = string.Empty;

            public string NextStep { get; set; } = string.Empty;
        }

        private sealed class DashboardStatus
        {
            public DashboardStatus(string name, string state, bool warning)
            {
                Name = name;
                State = state;
                Warning = warning;
            }

            public string Name { get; private set; }

            public string State { get; private set; }

            public bool Warning { get; private set; }
        }

        private sealed class StyledDashboardStatus
        {
            public StyledDashboardStatus(string name, string state, RgbColor rgbColor, SpectreColor spectreColor)
            {
                Name = name;
                State = state;
                RgbColor = rgbColor;
                SpectreColor = spectreColor;
            }

            public string Name { get; private set; }

            public string State { get; private set; }

            public RgbColor RgbColor { get; private set; }

            public SpectreColor SpectreColor { get; private set; }
        }

        private sealed class TestRoutine : Routine
        {
            public TestRoutine(string[] inputs)
            {
                foreach (string input in inputs)
                {
                    Enqueue(new RoutineInput { Value = input });
                }
            }
        }
    }
}
