using System;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests Spectre-style rule and markup builder compatibility.
    /// </summary>
    public class RuleMarkupBuilderCompatibilityTests
    {
        /// <summary>
        /// Markup aliases render through the existing markup renderer and default to inline output.
        /// </summary>
        [Fact]
        public void Markup_FluentBuilder_RendersInlineWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var markup = new Markup("Deploy [green]ready[/]")
                    .Append(" for [cyan]prod[/]");

                AnsiConsole.Write(markup);
                AnsiConsole.Write(" now");

                VisualAssert.ContainsText(scope.Driver, "Deploy ready for prod now");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("prod") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Lines, line => line.Contains("Deploy ready for prod now"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup aliases can opt into newline rendering when composed as reusable renderables.
        /// </summary>
        [Fact]
        public void Markup_FluentBuilder_CanAppendNewLine()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                AnsiConsole.Write(new Markup("[yellow]First[/]").NewLine());
                AnsiConsole.Write(new Markup("[green]Second[/]"));

                VisualAssert.ContainsText(scope.Driver, "First");
                VisualAssert.ContainsText(scope.Driver, "Second");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("First") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Second") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Lines, line => line.Contains("First"));
                Assert.Contains(scope.Driver.Lines, line => line.Contains("Second"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rule aliases render through the existing responsive rule renderer.
        /// </summary>
        [Fact]
        public void Rule_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var rule = new Rule("Checks")
                    .LeftJustified()
                    .Character('=')
                    .Width(32)
                    .RuleColor(ConsoleColor.DarkGray)
                    .TitleColor(ConsoleColor.Cyan);

                AnsiConsole.Write(rule);

                VisualAssert.ContainsText(scope.Driver, "Checks");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Checks") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 32, "Rule line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rule style aliases accept Consoul markup color syntax.
        /// </summary>
        [Fact]
        public void Rule_FluentBuilder_AcceptsMarkupStyleAliases()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var rule = new Rule("Deploy")
                    .RightJustified()
                    .RuleStyle("grey")
                    .TitleStyle("white on blue");

                AnsiConsole.Write(rule);

                VisualAssert.ContainsText(scope.Driver, "Deploy");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.Gray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Raw Spectre-style markup in rule titles renders as styled visible title text.
        /// </summary>
        [Fact]
        public void Rule_FluentBuilder_RendersMarkupTitle()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var rule = new Rule("[cyan]Checks[/] [green]Ready[/]")
                    .Width(32)
                    .RuleStyle("grey")
                    .TitleStyle("white");

                AnsiConsole.Write(rule);

                VisualAssert.ContainsText(scope.Driver, "Checks Ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Checks") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.Gray);
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 32, "Rule line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rule aliases accept reusable renderable titles without forcing MVC view fragments back to plain strings.
        /// </summary>
        [Fact]
        public void Rule_FluentBuilder_AcceptsRenderableTitles()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                IConsoulRenderable capturedTitle = Consoul.Renderable(() => AnsiConsole.Markup("[magenta]Snapshot[/]"));

                var markupRule = new Rule(new Markup("[cyan]Checks[/] [green]Ready[/]"), Justify.Left)
                    .Width(34)
                    .RuleStyle("grey")
                    .TitleStyle("white");

                var textRule = new Rule()
                    .RuleTitle(new Text("[yellow]Deploy[/]"))
                    .RightAligned()
                    .Width(34)
                    .RuleColor(ConsoleColor.DarkGray)
                    .TitleStyle("white");

                var capturedRule = new Rule(capturedTitle, ConsoleColor.Cyan, Justify.Center)
                    .Width(34)
                    .Character('=');

                AnsiConsole.Write(markupRule);
                AnsiConsole.Write(textRule);
                AnsiConsole.Write(capturedRule);

                Assert.Equal("[cyan]Checks[/] [green]Ready[/]", markupRule.Title);
                Assert.Equal("[yellow]Deploy[/]", textRule.Title);
                Assert.Equal("Snapshot", capturedRule.Title);
                VisualAssert.ContainsText(scope.Driver, "Checks Ready");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "Snapshot");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Checks") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Snapshot") && write.Color == ConsoleColor.Cyan);
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                Assert.DoesNotContain("[magenta]", scope.Driver.Text);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 34, "Renderable rule title line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rule aliases accept Spectre-style title and justification vocabulary.
        /// </summary>
        [Fact]
        public void Rule_FluentBuilder_AcceptsJustifyAliases()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var rule = new Rule()
                    .RuleTitle("Deploy")
                    .Justify(Justify.Left)
                    .Width(30);
                rule.Justification = Justify.Right;

                AnsiConsole.Write(rule);

                Assert.Equal(Justify.Right, rule.Justification);
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                Assert.Contains(scope.Driver.Lines, line => line.Contains("Deploy") && line.IndexOf("Deploy", StringComparison.Ordinal) > 12);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 30, "Rule line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rule aliases accept aligned naming used by text, table, and grid builders.
        /// </summary>
        [Fact]
        public void Rule_FluentBuilder_AcceptsAlignedAliases()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var left = new Rule("Left")
                    .LeftAligned()
                    .Width(30);
                var right = new Rule("Right")
                    .RightAligned()
                    .Width(30);

                AnsiConsole.Write(left);
                AnsiConsole.Write(right);

                Assert.Equal(Justify.Left, left.Justification);
                Assert.Equal(Justify.Right, right.Justification);
                VisualAssert.ContainsText(scope.Driver, "Left");
                VisualAssert.ContainsText(scope.Driver, "Right");
                Assert.Contains(scope.Driver.Lines, line => line.Contains("Left") && line.IndexOf("Left", StringComparison.Ordinal) < 8);
                Assert.Contains(scope.Driver.Lines, line => line.Contains("Right") && line.IndexOf("Right", StringComparison.Ordinal) > 12);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 30, "Rule line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rule constructors accept common one-expression title, color, and justification snippets.
        /// </summary>
        [Fact]
        public void Rule_Constructors_AcceptTitleColorAndJustification()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var left = new Rule("Left", Justify.Left)
                    .Width(30);
                var right = new Rule("Right", ConsoleColor.Yellow, Justify.Right)
                    .Width(30)
                    .RuleColor(ConsoleColor.DarkGray);

                AnsiConsole.Write(left);
                AnsiConsole.Write(right);

                Assert.Equal(Justify.Left, left.Justification);
                Assert.Equal(Justify.Right, right.Justification);
                VisualAssert.ContainsText(scope.Driver, "Left");
                VisualAssert.ContainsText(scope.Driver, "Right");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Right") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 30, "Rule line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rule object initializer snippets can set title, justification, and line style.
        /// </summary>
        [Fact]
        public void Rule_ObjectInitializer_AcceptsStyleProperty()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var rule = new Rule
                {
                    Title = "Deploy",
                    Justification = Justify.Left,
                    Style = new Style(ConsoleColor.DarkGray)
                };
                rule.Width(30).TitleStyle("aqua");

                AnsiConsole.Write(rule);

                Assert.NotNull(rule.Style);
                Assert.Equal(ConsoleColor.DarkGray, rule.Style.Foreground);
                Assert.Equal(Justify.Left, rule.Justification);
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 30, "Rule line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }
    }
}
