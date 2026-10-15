using System;
using System.Globalization;
using System.IO;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for tree view helpers.
    /// </summary>
    public class TreeResponsiveRenderingTests
    {
        /// <summary>
        /// Trees render nested and long content inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(18)]
        [InlineData(32)]
        [InlineData(80)]
        public void Tree_LongNestedContent_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                TreeNode root = CreateDeploymentTree();

                Consoul.Tree(root);

                VisualAssert.WroteText(scope.Driver, "Deployment");
                VisualAssert.WroteText(scope.Driver, "|-- ");
                VisualAssert.WroteText(scope.Driver, "`-- ");
                VisualAssert.WroteText(scope.Driver, "Product");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Trees honor configured width and text or branch colors.
        /// </summary>
        [Fact]
        public void Tree_CustomOptions_HonorWidthAndColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new TreeOptions
                {
                    Width = 24,
                    BranchScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Green,
                        BackgroundColor = ConsoleColor.Black
                    },
                    TextScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                };

                var root = new TreeNode("Root");
                root.Add(new TreeNode("Highlighted child", new ColorScheme
                {
                    Color = ConsoleColor.Yellow,
                    BackgroundColor = ConsoleColor.Black
                }));

                Consoul.Tree(root, options);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Root") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("`-- ") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Highlighted") && write.Color == ConsoleColor.Yellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 24, "Tree line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent tree color helpers accept Spectre-style style strings.
        /// </summary>
        [Fact]
        public void Tree_FluentBuilder_AcceptsStyleStrings()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var tree = new Tree("Root")
                    .AsciiGuide()
                    .Width(32)
                    .TextColor("white on navy")
                    .BranchColor("grey");
                tree.AddNode("Worker");

                AnsiConsole.Write(tree);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Root") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Worker") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("`-- ") && write.Color == ConsoleColor.Gray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Tree and node style aliases can style existing hierarchy nodes without rebuilding the tree.
        /// </summary>
        [Fact]
        public void Tree_FluentStyleAliases_CanStyleExistingNodes()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var tree = new Tree("Deployment")
                    .AsciiGuide()
                    .Width(42)
                    .Style("white on navy")
                    .BranchColor("grey");

                TreeNode queued = tree.AddNode("Queued").Style("bold yellow");
                queued.AddNode("Approval").Color(ConsoleColor.Cyan);
                tree.AddNode("Running").Color(SpectreColor.Green);
                tree.AddNode("Failed").Color(RgbColor.Red);
                tree.AddNode("Inherited");

                AnsiConsole.Write(tree);

                VisualAssert.ContainsText(scope.Driver, "Deployment");
                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "Approval");
                VisualAssert.ContainsText(scope.Driver, "Inherited");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployment") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Queued") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Running") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Failed") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Inherited") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Tree style helpers preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void Tree_StyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            var tree = new Tree("Root")
                .AsciiGuide()
                .Width(60)
                .Style("bold #22c55e")
                .BranchColor("#445566");
            tree.AddNode("API").Color("#336699");
            tree.AddNode("Worker").Style("underline #778899");
            tree.AddNode("Captured").Color(RgbColor.FromHex("#112233"));

            console.Write(tree);

            string output = writer.ToString();
            Assert.Contains("\u001b[1;38;2;34;197;94mRoot\u001b[22;39m", output);
            Assert.Contains("\u001b[38;2;68;85;102m|-- \u001b[39m", output);
            Assert.Contains("\u001b[38;2;51;102;153mAPI\u001b[39m", output);
            Assert.Contains("\u001b[4;38;2;119;136;153mWorker\u001b[24;39m", output);
            Assert.Contains("\u001b[1;38;2;17;34;51mCaptured\u001b[22;39m", output);
        }

        /// <summary>
        /// Tree style helpers use 256-color SGR when the terminal supports eight-bit color.
        /// </summary>
        [Fact]
        public void Tree_StyleHelpersEmitEightBitSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.EightBit
            });

            var items = new[]
            {
                new { Name = "API", Color = RgbColor.FromHex("#336699") }
            };

            var tree = new Tree(new Markup("[bold #22c55e]Root[/]"))
                .AsciiGuide()
                .Width(60)
                .BranchColor("#445566")
                .AddNodes(items, item => item.Name, item => item.Color);

            console.Write(tree);

            string output = writer.ToString();
            Assert.Contains("\u001b[1;38;5;41mRoot\u001b[22;39m", output);
            Assert.Contains("\u001b[38;5;239m`-- \u001b[39m", output);
            Assert.Contains("\u001b[38;5;60mAPI\u001b[39m", output);
            Assert.DoesNotContain("\u001b[38;2", output);
        }

        /// <summary>
        /// Tree labels wrap by visible markup text instead of counting style tags as display width.
        /// </summary>
        /// <param name="width">The configured tree width.</param>
        [Theory]
        [InlineData(18)]
        [InlineData(32)]
        public void Tree_RawMarkupLabels_WrapByVisibleTextAndDoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width + 4))
            {
                var root = new TreeNode("[cyan]Deployment[/]");
                root.Add("[green]ProductionClusterWithVeryLongNameWithoutNaturalBreaks[/]");
                root.Add("Escaped [[literal]] value");

                Consoul.Tree(root, new TreeOptions { Width = width });

                VisualAssert.ContainsText(scope.Driver, "Deployment");
                VisualAssert.ContainsText(scope.Driver, "Production");
                VisualAssert.ContainsText(scope.Driver, "literal");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployment") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[literal]"));
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Markup tree line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Trees can render only the root node children when the root is used as a logical grouping model.
        /// </summary>
        [Fact]
        public void Tree_ExcludeRoot_RendersChildrenOnly()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var root = new TreeNode("Hidden root");
                root.Add("Visible child");

                Consoul.Tree(root, new TreeOptions { IncludeRoot = false });

                Assert.DoesNotContain("Hidden root", scope.Driver.Text);
                VisualAssert.WroteText(scope.Driver, "Visible child");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render trees as part of their view content.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderTreeSections()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var model = new DeploymentModel
                    {
                        Environment = "Production",
                        Service = "Worker"
                    };

                    var view = Consoul.View("Deployment", model)
                        .RenderModel(deployment =>
                        {
                            Consoul.Tree("Deployment", root =>
                            {
                                root.Add("Environment: " + deployment.Environment);
                                root.Add("Service: " + deployment.Service);
                            });
                        })
                        .Option("Refresh", deployment => deployment.Service = "Api");

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Environment: Production");
                    VisualAssert.WroteText(scope.Driver, "Service: Worker");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private static TreeNode CreateDeploymentTree()
        {
            var root = new TreeNode("Deployment");
            TreeNode targets = root.Add("Targets");
            targets.Add("ProductionClusterWithVeryLongNameWithoutNaturalBreaks");
            targets.Add("Staging");

            TreeNode checks = root.Add("Checks", ConsoleColor.Yellow);
            checks.Add("Database migration status waiting on operator approval");
            checks.Add("Feature flags");

            return root;
        }

        private sealed class DeploymentModel
        {
            public string Environment { get; set; } = string.Empty;

            public string Service { get; set; } = string.Empty;
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
