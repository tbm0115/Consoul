using System;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests reusable <see cref="Style"/> objects in Spectre-style builder APIs.
    /// </summary>
    public class StyleObjectBuilderCompatibilityTests
    {
        /// <summary>
        /// Text builders accept reusable style objects.
        /// </summary>
        [Fact]
        public void Text_Color_AcceptsStyleObject()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Style style = Style.Plain
                    .WithForeground(RgbColor.Aqua)
                    .WithBackground(RgbColor.Navy)
                    .AddDecoration(Decoration.Bold);

                AnsiConsole.Write(new Text("Ready").Color(style));

                Assert.True(style.Decoration.HasFlag(Decoration.Bold));
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Ready") &&
                    write.Color == ConsoleColor.Cyan &&
                    write.BackgroundColor == ConsoleColor.DarkBlue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Panel builders accept reusable style objects for frame, title, footer, and content colors.
        /// </summary>
        [Fact]
        public void Panel_ColorHelpers_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel("Build ready")
                    .Header("Status")
                    .Footer("prod")
                    .AsciiBorder()
                    .Width(32)
                    .BorderColor(new Style(RgbColor.Grey))
                    .HeaderColor(new Style(RgbColor.Aqua))
                    .FooterColor(new Style(RgbColor.White, RgbColor.Navy))
                    .ContentColor(new Style(RgbColor.FromHex("#22c55e")));

                AnsiConsole.Write(panel);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("+") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("prod") &&
                    write.Color == ConsoleColor.White &&
                    write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build ready") && write.Color == ConsoleColor.DarkCyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Table builders accept reusable style objects for table-wide visual regions.
        /// </summary>
        [Fact]
        public void Table_ColorHelpers_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var table = new Table()
                    .Width(40)
                    .AsciiBorder()
                    .BorderColor(new Style(RgbColor.Grey))
                    .Title("Deployments")
                    .TitleColor(new Style(RgbColor.FromHex("#22c55e")))
                    .Caption("2 jobs")
                    .CaptionColor(new Style(RgbColor.White, RgbColor.Navy))
                    .HeaderColor(new Style(RgbColor.Yellow))
                    .ContentColor(new Style(RgbColor.White))
                    .AlternatingRowColor(new Style(RgbColor.Aqua))
                    .FooterColor(new Style(RgbColor.Grey))
                    .HideFooter()
                    .AddColumns("Job", "State")
                    .AddRow("Build", "Ready")
                    .AddRow("Deploy", "Waiting");

                AnsiConsole.Write(table);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("+") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployments") && write.Color == ConsoleColor.DarkCyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Job") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("2 jobs") &&
                    write.Color == ConsoleColor.White &&
                    write.BackgroundColor == ConsoleColor.DarkBlue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rule and tree builders accept reusable style objects without requiring real console handles.
        /// </summary>
        [Fact]
        public void RuleAndTree_ColorHelpers_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var rule = new Rule("Checks")
                    .Width(28)
                    .LeftJustified()
                    .RuleStyle(new Style(RgbColor.Grey))
                    .TitleStyle(new Style(RgbColor.Aqua));
                var tree = new Tree("Deployment")
                    .AsciiGuide()
                    .Width(32)
                    .BranchColor(new Style(RgbColor.Grey))
                    .TextColor(new Style(RgbColor.White));
                tree.AddNode("Build");

                AnsiConsole.Write(rule);
                AnsiConsole.Write(tree);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Checks") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployment") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("`--") && write.Color == ConsoleColor.Gray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }
    }
}
