using System;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests Spectre-style visual builder compatibility for reusable renderables.
    /// </summary>
    public class VisualBuilderCompatibilityTests
    {
        /// <summary>
        /// Tree aliases render nested nodes through the existing tree renderer.
        /// </summary>
        [Fact]
        public void Tree_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var tree = new Tree("Deployment")
                    .AsciiGuide()
                    .Width(36)
                    .BranchColor(ConsoleColor.DarkGray)
                    .TextColor(ConsoleColor.White);

                TreeNode checks = tree.AddNode("Checks", "aqua");
                checks.AddNode("Build", "#22c55e");
                checks.AddNode("Tests");
                tree.AddNode("Release");

                AnsiConsole.Write(tree);

                VisualAssert.ContainsText(scope.Driver, "Deployment");
                VisualAssert.ContainsText(scope.Driver, "Checks");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Release");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("|-- ") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Checks") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.DarkCyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Tree line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Tree aliases accept renderable node labels for pasted Spectre-style snippets.
        /// </summary>
        [Fact]
        public void Tree_FluentBuilder_AcceptsRenderableNodeLabels()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                var tree = new Tree(new Markup("[cyan]Deployment[/]"))
                    .AsciiGuide()
                    .Width(36)
                    .BranchColor("grey")
                    .TextColor(ConsoleColor.White);

                TreeNode checks = tree.AddNode(new Markup("[green]Checks[/]"));
                checks.AddNode(new Panel("Build ready").AsciiBorder().Width(18).Padding(0), "yellow", captureWidth: 20);
                tree.AddNode(new Markup("[aqua]Release[/]"));

                AnsiConsole.Write(tree);

                VisualAssert.ContainsText(scope.Driver, "Deployment");
                VisualAssert.ContainsText(scope.Driver, "Checks");
                VisualAssert.ContainsText(scope.Driver, "Build ready");
                VisualAssert.ContainsText(scope.Driver, "Release");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployment") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Checks") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build ready") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Release") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Renderable tree line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Tree aliases treat raw markup labels as styled visible text for pasted Spectre-style snippets.
        /// </summary>
        [Fact]
        public void Tree_FluentBuilder_RendersRawMarkupLabels()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var tree = new Tree("[cyan]Deployment[/] [[literal]]")
                    .AsciiGuide()
                    .Width(40)
                    .BranchColor("grey")
                    .TextColor(ConsoleColor.White);

                TreeNode checks = tree.AddNode("[green]Checks[/]");
                checks.AddNode("[yellow]Build[/]");
                checks.AddNode("[white on navy]Approval[/]");
                tree.AddNode("[red]Blocked[/]");

                AnsiConsole.Write(tree);

                VisualAssert.ContainsText(scope.Driver, "Deployment [literal]");
                VisualAssert.ContainsText(scope.Driver, "Checks");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Approval");
                VisualAssert.ContainsText(scope.Driver, "Blocked");
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployment") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[literal]") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Checks") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Blocked") && write.Color == ConsoleColor.Red);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Raw markup tree line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Direct tree nodes can capture renderables without requiring the fluent tree wrapper.
        /// </summary>
        [Fact]
        public void TreeNode_RenderableConstructor_CapturesLabelForTreeEntry()
        {
            using (var scope = new ConsoleDriverScope(36))
            {
                var root = new TreeNode(new Markup("[green]Root[/]"));
                root.AddNode(new Text("Worker").Color(ConsoleColor.Cyan));

                Consoul.Tree(root, new TreeOptions
                {
                    Width = 28,
                    IncludeRoot = true
                });

                VisualAssert.ContainsText(scope.Driver, "Root");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Root") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Worker") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 28, "Renderable TreeNode line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Tree aliases accept mixed scalar, existing-node, and renderable labels for quick POC model trees.
        /// </summary>
        [Fact]
        public void Tree_FluentBuilder_AcceptsMixedObjectNodes()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var existing = new TreeNode("Existing branch");
                existing.AddNode("Existing child");

                var tree = new Tree(2026)
                    .AsciiGuide()
                    .Width(44)
                    .BranchColor("grey")
                    .TextColor(ConsoleColor.White)
                    .AddNodes(
                        "API",
                        2,
                        new Markup("[green]Ready[/]"),
                        existing,
                        new Panel("Panel node").AsciiBorder().Width(16).Padding(0));

                AnsiConsole.Write(tree);

                VisualAssert.ContainsText(scope.Driver, "2026");
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "2");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Existing branch");
                VisualAssert.ContainsText(scope.Driver, "Existing child");
                VisualAssert.ContainsText(scope.Driver, "Panel node");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Mixed object tree line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Tree builders can project model collections at both root and nested node levels.
        /// </summary>
        [Fact]
        public void Tree_FluentBuilder_AddNodesProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var services = new[]
                {
                    new TreeBranch(
                        "API",
                        ConsoleColor.Cyan,
                        "aqua",
                        new[]
                        {
                            new TreeBranch("Build", ConsoleColor.Green, "green"),
                            new TreeBranch("Smoke tests", ConsoleColor.Yellow, "yellow")
                        }),
                    new TreeBranch(
                        "Worker",
                        ConsoleColor.Magenta,
                        "white on navy",
                        new[]
                        {
                            new TreeBranch("Queue", ConsoleColor.Green, "green")
                        })
                };

                var tree = new Tree("Deployment")
                    .AsciiGuide()
                    .Width(44)
                    .BranchColor("grey")
                    .TextColor(ConsoleColor.White)
                    .AddNodes(
                        services,
                        service => service.Name,
                        service => (ConsoleColor?)service.Color)
                    .AddNodes(
                        services,
                        service => "Styled " + service.Name,
                        service => service.StyleText);

                tree.Root.Children[0].AddNodes(
                    services[0].Children,
                    child => child.Name,
                    child => (ConsoleColor?)child.Color);
                tree.Root.Children[1].AddNodes(
                    services[1].Children,
                    child => child.Name,
                    child => (ConsoleColor?)child.Color);
                tree.Root.Children[2].AddNodes(
                    services[0].Children,
                    child => "Styled " + child.Name,
                    child => child.StyleText);
                tree.Root.Children[3].AddNodes(
                    services[1].Children,
                    child => "Styled " + child.Name,
                    child => child.StyleText);

                AnsiConsole.Write(tree);

                VisualAssert.ContainsText(scope.Driver, "Deployment");
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Smoke tests");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "Styled API");
                VisualAssert.ContainsText(scope.Driver, "Styled Build");
                VisualAssert.ContainsText(scope.Driver, "Styled Worker");
                VisualAssert.ContainsText(scope.Driver, "Styled Queue");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Worker") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Smoke tests") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled API") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Worker") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Build") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Queue") && write.Color == ConsoleColor.Green);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Projected tree line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Tree aliases accept Spectre-style guide presets and sibling AddNodes helpers.
        /// </summary>
        [Fact]
        public void Tree_FluentBuilder_AcceptsTreeGuidePresetsAndAddNodes()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                var tree = new Tree("Services")
                    .Guide(TreeGuide.DoubleLine)
                    .Width(36)
                    .BranchColor(ConsoleColor.DarkGray)
                    .TextColor(ConsoleColor.White)
                    .AddNodes("API", "Worker", "Scheduler");

                AnsiConsole.Write(tree);

                VisualAssert.ContainsText(scope.Driver, "Services");
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "Scheduler");
                VisualAssert.ContainsText(scope.Driver, "\u2560\u2550");
                VisualAssert.ContainsText(scope.Driver, "\u255a\u2550");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\u2560\u2550") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "TreeGuide line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Tree aliases can collapse individual nodes or the whole tree without dropping the node label.
        /// </summary>
        [Fact]
        public void Tree_FluentBuilder_CollapsesNodesAndWholeTree()
        {
            var tree = new Tree("Project")
                .AsciiGuide()
                .Width(40);
            TreeNode expanded = tree.AddNode("Expanded");
            expanded.AddNode("Visible file");
            TreeNode collapsed = tree.AddNode("Collapsed");
            collapsed.AddNode("Hidden file");
            collapsed.Collapse();

            string partiallyCollapsed = AnsiConsole.Capture(tree, width: 44);

            Assert.Contains("Project", partiallyCollapsed);
            Assert.Contains("Expanded", partiallyCollapsed);
            Assert.Contains("Visible file", partiallyCollapsed);
            Assert.Contains("Collapsed", partiallyCollapsed);
            Assert.DoesNotContain("Hidden file", partiallyCollapsed);

            tree.Collapse();
            string fullyCollapsed = AnsiConsole.Capture(tree, width: 44);

            Assert.False(tree.Expanded);
            Assert.Contains("Project", fullyCollapsed);
            Assert.DoesNotContain("Expanded", fullyCollapsed);
            Assert.DoesNotContain("Collapsed", fullyCollapsed);
            Assert.All(SplitLines(fullyCollapsed), line => Assert.True(line.Length <= 40, "Collapsed tree line exceeded configured width: " + line));
        }

        /// <summary>
        /// Bar chart aliases render through the existing responsive bar chart renderer.
        /// </summary>
        [Fact]
        public void BarChart_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var chart = new BarChart()
                    .Width(36)
                    .LabelWidth(8)
                    .MaximumValue(10)
                    .BarCharacters('=')
                    .ValueFormatter(value => value.ToString("0") + " jobs")
                    .LabelColor("yellow")
                    .ValueColor("aqua")
                    .AddItem("Passing", 8, "green")
                    .AddItem("Failing", 2, "red");

                AnsiConsole.Write(chart);

                VisualAssert.ContainsText(scope.Driver, "Passing");
                VisualAssert.ContainsText(scope.Driver, "Failing");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Passing") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("8 jobs") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Bar chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bar charts can project model collections without hand-written item loops.
        /// </summary>
        [Fact]
        public void BarChart_FluentBuilder_AddItemsProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var jobs = new[]
                {
                    new ChartJob("Ready", 8, ConsoleColor.Green),
                    new ChartJob("Blocked", 2, ConsoleColor.Red)
                };

                var chart = new BarChart()
                    .Width(36)
                    .LabelWidth(8)
                    .MaximumValue(10)
                    .AddItems(jobs, job => job.Label, job => job.Value, job => job.Color);

                AnsiConsole.Write(chart);

                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Blocked");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Red);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Projected bar chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bar charts can project Spectre-style style strings directly from model data.
        /// </summary>
        [Fact]
        public void BarChart_FluentBuilder_AddItemsProjectsStyleStrings()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var jobs = new[]
                {
                    new ChartJob("Ready", 8, ConsoleColor.Green, "green"),
                    new ChartJob("Blocked", 2, ConsoleColor.Red, "red"),
                    new ChartJob("Queued", 4, ConsoleColor.Yellow, null!)
                };

                var chart = new BarChart()
                    .Width(36)
                    .LabelWidth(8)
                    .MaximumValue(10)
                    .BarColor("yellow")
                    .AddItems(jobs, job => job.Label, job => job.Value, job => job.StyleText);

                AnsiConsole.Write(chart);

                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Blocked");
                VisualAssert.ContainsText(scope.Driver, "Queued");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Yellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Style-string projected bar chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breakdown chart aliases render through the existing responsive breakdown chart renderer.
        /// </summary>
        [Fact]
        public void BreakdownChart_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var chart = new BreakdownChart()
                    .Width(36)
                    .SegmentCharacters('=')
                    .ValueFormatter(value => value.ToString("0") + " jobs")
                    .PercentageFormatter(value => value.ToString("0") + "%")
                    .LabelColor("white")
                    .ValueColor("aqua")
                    .AddItem("Queued", 4, "yellow")
                    .AddItem("Running", 2, "green")
                    .AddItem("Failed", 1, "red");

                AnsiConsole.Write(chart);

                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "Running");
                VisualAssert.ContainsText(scope.Driver, "Failed");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Queued") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("4 jobs") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Breakdown chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breakdown charts can project model collections with reusable style objects.
        /// </summary>
        [Fact]
        public void BreakdownChart_FluentBuilder_AddItemsProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var jobs = new[]
                {
                    new ChartJob("Queued", 4, ConsoleColor.Yellow),
                    new ChartJob("Running", 2, ConsoleColor.Green)
                };

                var chart = new BreakdownChart()
                    .Width(36)
                    .SegmentCharacters('=')
                    .AddItems(
                        jobs,
                        job => job.Label,
                        job => job.Value,
                        job => new Style(job.Color));

                AnsiConsole.Write(chart);

                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "Running");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Green);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Projected breakdown chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breakdown charts can project Spectre-style style strings directly from model data.
        /// </summary>
        [Fact]
        public void BreakdownChart_FluentBuilder_AddItemsProjectsStyleStrings()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var jobs = new[]
                {
                    new ChartJob("Queued", 4, ConsoleColor.Yellow, "yellow"),
                    new ChartJob("Running", 2, ConsoleColor.Green, "white on navy"),
                    new ChartJob("Unknown", 1, ConsoleColor.Red, null!)
                };

                var chart = new BreakdownChart()
                    .Width(36)
                    .SegmentCharacters('=')
                    .SegmentColor("red")
                    .AddItems(jobs, job => job.Label, job => job.Value, job => job.StyleText);

                AnsiConsole.Write(chart);

                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "Running");
                VisualAssert.ContainsText(scope.Driver, "Unknown");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Red);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Style-string projected breakdown chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private sealed class ChartJob
        {
            public ChartJob(string label, double value, ConsoleColor color, string styleText = "")
            {
                Label = label;
                Value = value;
                Color = color;
                StyleText = styleText;
            }

            public string Label { get; }

            public double Value { get; }

            public ConsoleColor Color { get; }

            public string StyleText { get; }
        }

        private sealed class TreeBranch
        {
            public TreeBranch(string name, ConsoleColor color)
                : this(name, color, "", new TreeBranch[0])
            {
            }

            public TreeBranch(string name, ConsoleColor color, string styleText)
                : this(name, color, styleText, new TreeBranch[0])
            {
            }

            public TreeBranch(string name, ConsoleColor color, TreeBranch[] children)
                : this(name, color, "", children)
            {
            }

            public TreeBranch(string name, ConsoleColor color, string styleText, TreeBranch[] children)
            {
                Name = name;
                Color = color;
                StyleText = styleText;
                Children = children ?? new TreeBranch[0];
            }

            public string Name { get; }

            public ConsoleColor Color { get; }

            public string StyleText { get; }

            public TreeBranch[] Children { get; }
        }

        private static string[] SplitLines(string value)
        {
            return (value ?? string.Empty).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }
    }
}
