using System;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests MVC-style view help builder compatibility.
    /// </summary>
    public class ViewHelpBuilderCompatibilityTests
    {
        /// <summary>
        /// View help entries accept Spectre-style style strings for labels and separators.
        /// </summary>
        [Fact]
        public void ViewHelpEntry_StyledHelpers_RenderWithConfiguredColors()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var help = new ViewHelpEntry("Commands", new ViewHelpOptions
                {
                    Width = 42,
                    LabelWidth = 12,
                    ShortcutWidth = 6
                });

                help.AddStyled("Deploy", "green", "Publish the current release", "d");
                help.AddSeparatorStyled("Danger", "grey");
                help.Add(ViewHelpItem.Styled("Rollback", "yellow", "Revert the release", "r", "approval missing"));
                help.Add("Inspect", new Style(ConsoleColor.Cyan), "Open diagnostics", "i");
                help.AddSeparator("Support", new Style(ConsoleColor.DarkGray));
                help.Add(ViewHelpItem.Styled("Docs", new Style(ConsoleColor.Magenta), "Open the runbook", "docs"));
                help.Add(ViewHelpItem.Separator("End", new Style(ConsoleColor.DarkYellow)));

                AnsiConsole.Write(help);

                VisualAssert.ContainsText(scope.Driver, "Commands");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "Danger");
                VisualAssert.ContainsText(scope.Driver, "Rollback");
                VisualAssert.ContainsText(scope.Driver, "Inspect");
                VisualAssert.ContainsText(scope.Driver, "Support");
                VisualAssert.ContainsText(scope.Driver, "Docs");
                VisualAssert.ContainsText(scope.Driver, "End");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Danger") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rollback") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Inspect") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Support") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Docs") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("End") && write.Color == ConsoleColor.DarkYellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 42, "Help line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// View help labels, descriptions, disabled reasons, and separators can use raw markup without leaking tags.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(48)]
        public void ViewHelpEntry_RawMarkupText_DoesNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                var help = new ViewHelpEntry("[cyan]Commands[/]", new ViewHelpOptions
                {
                    Width = width,
                    LabelWidth = 14,
                    ShortcutWidth = 5
                });

                help.Add(
                    "[green]Deploy[/] [[prod]]",
                    "[white on navy]Publish release[/] after approval",
                    "d",
                    "[yellow]approval missing[/]");
                help.AddSeparator("[cyan]Danger[/]");

                AnsiConsole.Write(help);

                VisualAssert.ContainsText(scope.Driver, "Commands");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "[prod]");
                VisualAssert.ContainsText(scope.Driver, "Publish");
                VisualAssert.ContainsText(scope.Driver, "missing");
                VisualAssert.ContainsText(scope.Driver, "Danger");
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Commands") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Publish") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("missing") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Danger") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Help line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Shortcut cells are displayed literally even when the key text matches a markup color name.
        /// </summary>
        [Fact]
        public void ViewHelpEntry_ShortcutText_RemainsLiteral()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var help = new ViewHelpEntry("Commands", new ViewHelpOptions
                {
                    Width = 42,
                    LabelWidth = 12,
                    ShortcutWidth = 9
                });

                help.Add("Deploy", "Publish release", "green");

                AnsiConsole.Write(help);

                VisualAssert.ContainsText(scope.Driver, "[green]");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }
    }
}
