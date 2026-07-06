using System;
using System.Globalization;
using System.IO;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Tests lightweight markup rendering through the console driver.
    /// </summary>
    public class MarkupRenderingTests
    {
        /// <summary>
        /// Markup writes separate colored segments for foreground and background tags.
        /// </summary>
        [Fact]
        public void MarkupLine_WritesColoredSegments()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.MarkupLine("Deploy [green]ready[/] [white on blue]now[/].");

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy ") && write.Color == RenderOptions.DefaultColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("now") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.Blue);
                VisualAssert.ContainsText(scope.Driver, "Deploy ready now.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup supports Consoul render-option scheme aliases.
        /// </summary>
        [Fact]
        public void MarkupLine_WritesNamedSchemeSegments()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.MarkupLine("[prompt]Question[/] [subnote]hint[/] [invalid]error[/] [option]choice[/]");

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Question") && write.Color == RenderOptions.PromptColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("hint") && write.Color == RenderOptions.SubnoteColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("error") && write.Color == RenderOptions.InvalidColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("choice") && write.Color == RenderOptions.OptionColor);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup accepts common Spectre-style modifiers while preserving color rendering.
        /// </summary>
        [Fact]
        public void MarkupLine_AcceptsStyleModifiers()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.MarkupLine("[bold red]Hot[/] [dim]muted[/] [underline white on blue]linked[/] [bold prompt]Question[/]");

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Hot") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("muted") && write.Color == RenderOptions.DefaultColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("linked") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Question") && write.Color == RenderOptions.PromptColor);
                VisualAssert.ContainsText(scope.Driver, "Hot muted linked Question");
                Assert.DoesNotContain("[bold", scope.Driver.Text);
                Assert.DoesNotContain("[dim]", scope.Driver.Text);
                Assert.DoesNotContain("[underline", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup accepts hex colors by mapping them to the nearest console color.
        /// </summary>
        [Fact]
        public void MarkupLine_MapsHexColorsToNearestConsoleColor()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.MarkupLine("[#ff0000]failure[/] [#00ffff on #000080]runbook[/] [#0f0]ok[/]");

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("failure") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("runbook") && write.Color == ConsoleColor.Cyan && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ok") && write.Color == ConsoleColor.Green);
                VisualAssert.ContainsText(scope.Driver, "failure runbook ok");
                Assert.DoesNotContain("[#ff0000]", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup accepts common color aliases used by CSS and terminal style examples.
        /// </summary>
        [Fact]
        public void MarkupLine_AcceptsCommonColorAliases()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.MarkupLine("[aqua]api[/] [fuchsia]worker[/] [navy on silver]ops[/]");

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("api") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("worker") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ops") && write.Color == ConsoleColor.DarkBlue && write.BackgroundColor == ConsoleColor.Gray);
                VisualAssert.ContainsText(scope.Driver, "api worker ops");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Style parsing accepts common Spectre-style color aliases and hex colors.
        /// </summary>
        [Fact]
        public void Style_Parse_AcceptsCommonSpectreStyleTokens()
        {
            Style parsed = Style.Parse("bold #22c55e on navy");

            Assert.Equal(ConsoleColor.DarkCyan, parsed.Foreground);
            Assert.Equal(ConsoleColor.DarkBlue, parsed.Background);
            Assert.True(parsed.Decoration.HasFlag(Decoration.Bold));

            Assert.True(Style.TryParseColor("grey", out ConsoleColor grey));
            Assert.Equal(ConsoleColor.Gray, grey);

            Assert.True(Style.TryParseColor("aqua", out ConsoleColor aqua));
            Assert.Equal(ConsoleColor.Cyan, aqua);

            ColorScheme resolved = new Style(ConsoleColor.Yellow).ToColorScheme(null);
            Assert.Equal(ConsoleColor.Yellow, resolved.Color);
            Assert.Equal(RenderOptions.DefaultScheme.BackgroundColor, resolved.BackgroundColor);

            Assert.False(Style.TryParse("not-a-style", out _));
        }

        /// <summary>
        /// Style parsing preserves Spectre-style decoration metadata for APIs that carry reusable styles.
        /// </summary>
        [Fact]
        public void Style_Parse_PreservesDecorationFlags()
        {
            Style parsed = Style.Parse("bold faint italic underline strike white on blue");

            Assert.Equal(ConsoleColor.White, parsed.Foreground);
            Assert.Equal(ConsoleColor.Blue, parsed.Background);
            Assert.True(parsed.Decoration.HasFlag(Decoration.Bold));
            Assert.True(parsed.Decoration.HasFlag(Decoration.Dim));
            Assert.True(parsed.Decoration.HasFlag(Decoration.Italic));
            Assert.True(parsed.Decoration.HasFlag(Decoration.Underline));
            Assert.True(parsed.Decoration.HasFlag(Decoration.Strikethrough));
        }

        /// <summary>
        /// Named closing tags are accepted as compatibility aliases for the normal closing tag.
        /// </summary>
        [Fact]
        public void MarkupLine_ExplicitCloseTagsRestorePreviousStyle()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.MarkupLine("[green]outer [red]inner[/red] outer[/green] plain [bold cyan]cool[/bold cyan]");

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("outer ") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("inner") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" plain ") && write.Color == RenderOptions.DefaultColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("cool") && write.Color == ConsoleColor.Cyan);
                VisualAssert.ContainsText(scope.Driver, "outer inner outer plain cool");
                Assert.DoesNotContain("[/red]", scope.Driver.Text);
                Assert.DoesNotContain("[/green]", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Link markup renders its content without exposing link tags as visible text.
        /// </summary>
        [Fact]
        public void MarkupLine_AcceptsLinkTagsAsStyleOnly()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.MarkupLine("See [link=https://example.com/docs]docs[/] for [link]details[/].");

                VisualAssert.ContainsText(scope.Driver, "See docs for details.");
                Assert.DoesNotContain("[link", scope.Driver.Text);
                Assert.DoesNotContain("[/]", scope.Driver.Text);
                Assert.Contains(scope.Driver.ControlWrites, write => write == "\u001b]8;;https://example.com/docs\u001b\\");
                Assert.Contains(scope.Driver.ControlWrites, write => write == "\u001b]8;;\u001b\\");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Writer-backed ANSI consoles preserve link markup as OSC-8 hyperlinks when link capabilities are enabled.
        /// </summary>
        [Fact]
        public void MarkupLine_WriterConsoleEmitsOsc8WhenLinksAreSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes
            });

            console.MarkupLine("See [link=https://example.com/docs]docs[/].");

            string output = writer.ToString();
            Assert.Contains("See ", output);
            Assert.Contains("\u001b]8;;https://example.com/docs\u001b\\docs\u001b]8;;\u001b\\", output);
            Assert.DoesNotContain("[link", output);
        }

        /// <summary>
        /// Writer-backed consoles keep link markup as plain text when ANSI/link capabilities are disabled.
        /// </summary>
        [Fact]
        public void MarkupLine_WriterConsoleOmitsOsc8WhenLinksAreDisabled()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.No
            });

            console.MarkupLine("See [link=https://example.com/docs]docs[/].");

            string output = writer.ToString();
            Assert.Contains("See docs.", output);
            Assert.DoesNotContain("\u001b]8", output);
            Assert.DoesNotContain("[link", output);
        }

        /// <summary>
        /// Paragraph link metadata renders as OSC-8 hyperlinks on writer-backed ANSI consoles.
        /// </summary>
        [Fact]
        public void Paragraph_LinkMetadataEmitsOsc8WhenLinksAreSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes
            });

            console.Write(new Paragraph("docs", new Style(ConsoleColor.Cyan), new Link("https://example.com/docs")));

            string output = writer.ToString();
            Assert.Contains("\u001b]8;;https://example.com/docs\u001b\\docs\u001b]8;;\u001b\\", output);
        }

        /// <summary>
        /// Table cell markup preserves OSC-8 links through the shared segment rendering path.
        /// </summary>
        [Fact]
        public void TableCell_LinkMarkupEmitsOsc8WhenLinksAreSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes
            });
            var table = new Table(new TableRenderOptions { ShowFooterHelp = false })
                .AddColumn("Doc")
                .AddRow("[link=https://example.com/docs]Docs[/]");

            console.Write(table);

            string output = writer.ToString();
            Assert.Contains("\u001b]8;;https://example.com/docs\u001b\\Docs\u001b]8;;\u001b\\", output);
            Assert.DoesNotContain("[link", output);
        }

        /// <summary>
        /// Markup replaces known Spectre-style emoji shortcodes while preserving unknown aliases.
        /// </summary>
        [Fact]
        public void MarkupLine_ReplacesKnownEmojiShortcodes()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.MarkupLine("[green]:check_mark:[/] Ready [red]:x:[/] Failed :not_known:");

                VisualAssert.ContainsText(scope.Driver, Emoji.Known.CheckMark + " Ready " + Emoji.Known.CrossMark + " Failed :not_known:");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(Emoji.Known.CheckMark) && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(Emoji.Known.CrossMark) && write.Color == ConsoleColor.Red);
                Assert.DoesNotContain(":check_mark:", scope.Driver.Text);
                Assert.DoesNotContain(":x:", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Emoji helpers expose reusable shortcode lookup and replacement outside immediate markup rendering.
        /// </summary>
        [Fact]
        public void EmojiHelpers_ResolveAndReplaceKnownShortcodes()
        {
            Assert.Equal(Emoji.Known.CheckMark, Emoji.Get(":check-mark:"));
            Assert.True(Emoji.TryGet("rocket", out string rocket));
            Assert.Equal(Emoji.Known.Rocket, rocket);

            string replaced = Emoji.Replace("Deploy :rocket: :green_circle: :unknown:");

            Assert.Equal("Deploy " + Emoji.Known.Rocket + " " + Emoji.Known.GreenCircle + " :unknown:", replaced);
        }

        /// <summary>
        /// Interpolated markup escapes inserted values without escaping markup tags in the template.
        /// </summary>
        [Fact]
        public void MarkupLineInterpolated_EscapesInterpolatedValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                string target = "Production [red]hot[/]";
                int count = 7;

                Consoul.MarkupLineInterpolated($"Deploying [green]{target}[/] after {count:000} checks.");

                VisualAssert.ContainsText(scope.Driver, "Deploying Production [red]hot[/] after 007 checks.");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production [red]hot[/]") && write.Color == ConsoleColor.Green);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("hot") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Composite-format markup overloads escape formatted values without escaping template tags.
        /// </summary>
        [Fact]
        public void MarkupLineFormat_EscapesFormattedValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                string target = "Production [red]hot[/]";

                Consoul.MarkupLine(
                    CultureInfo.InvariantCulture,
                    "Deploying [green]{0}[/] after [cyan]{1:000.0}[/] checks.",
                    target,
                    7.5);

                VisualAssert.ContainsText(scope.Driver, "Deploying Production [red]hot[/] after 007.5 checks.");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production [red]hot[/]") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("007.5") && write.Color == ConsoleColor.Cyan);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("hot") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// EscapeMarkup returns text that can be inserted manually into markup strings.
        /// </summary>
        [Fact]
        public void EscapeMarkup_EscapesSquareBrackets()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                string escaped = Consoul.EscapeMarkup("[literal] [green]not colored[/]");

                Assert.Equal("[[literal]] [[green]]not colored[[/]]", escaped);

                Consoul.MarkupLine("[cyan]" + escaped + "[/]");

                VisualAssert.ContainsText(scope.Driver, "[literal] [green]not colored[/]");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[literal] [green]not colored[/]") && write.Color == ConsoleColor.Cyan);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("not colored") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup static helpers match common Spectre-style escaping and stripping snippets.
        /// </summary>
        [Fact]
        public void MarkupStaticHelpers_EscapeAndRemoveMarkup()
        {
            string value = "[literal] [green]not colored[/]";

            Assert.Equal("[[literal]] [[green]]not colored[[/]]", Markup.Escape(value));
            Assert.Equal(Markup.Escape(value), Consoul.EscapeMarkup(value));

            string stripped = Markup.Remove("See [green]ready[/] [[literal]] [link=https://example.com]docs[/] [not-a-style]kept[/]");

            Assert.Equal("See ready [literal] docs [not-a-style]kept[/]", stripped);
            Assert.Equal(stripped, Consoul.RemoveMarkup("See [green]ready[/] [[literal]] [link=https://example.com]docs[/] [not-a-style]kept[/]"));
            Assert.Equal(stripped, AnsiConsole.RemoveMarkup("See [green]ready[/] [[literal]] [link=https://example.com]docs[/] [not-a-style]kept[/]"));
            Assert.Throws<ArgumentNullException>(() => Markup.Escape(null));
            Assert.Throws<ArgumentNullException>(() => Markup.Remove(null));
        }

        /// <summary>
        /// Markup.FromInterpolated escapes inserted values while keeping template markup active.
        /// </summary>
        [Fact]
        public void MarkupFromInterpolated_EscapesValuesAndPreservesTemplateMarkup()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                string target = "Production [red]hot[/]";
                int count = 7;
                Markup markup = Markup.FromInterpolated($"Deploying [green]{target}[/] after {count:000} checks.");

                AnsiConsole.Write(markup);

                VisualAssert.ContainsText(scope.Driver, "Deploying Production [red]hot[/] after 007 checks.");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production [red]hot[/]") && write.Color == ConsoleColor.Green);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("hot") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup.FromInterpolated honors format providers and default style for untagged text.
        /// </summary>
        [Fact]
        public void MarkupFromInterpolated_UsesFormatProviderAndDefaultStyle()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Markup markup = Markup.FromInterpolated(
                    CultureInfo.InvariantCulture,
                    $"Total {12.5m:0.0} [green]ready[/]",
                    new Style(ConsoleColor.Yellow));

                AnsiConsole.Write(markup);

                VisualAssert.ContainsText(scope.Driver, "Total 12.5 ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Total 12.5 ") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ready") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup.FromFormat escapes formatted values while keeping template markup active.
        /// </summary>
        [Fact]
        public void MarkupFromFormat_EscapesValuesAndPreservesTemplateMarkup()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                string target = "Production [red]hot[/]";
                Markup markup = Markup.FromFormat(
                    CultureInfo.InvariantCulture,
                    "Deploying [green]{0}[/] after [cyan]{1:000.0}[/] checks.",
                    target,
                    7.5);

                AnsiConsole.Write(markup);

                VisualAssert.ContainsText(scope.Driver, "Deploying Production [red]hot[/] after 007.5 checks.");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production [red]hot[/]") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("007.5") && write.Color == ConsoleColor.Cyan);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("hot") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup.FromFormat honors format providers and remains a reusable renderable with a default style.
        /// </summary>
        [Fact]
        public void MarkupFromFormat_UsesFormatProviderAndDefaultStyle()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Markup markup = Markup.FromFormat(
                    CultureInfo.InvariantCulture,
                    "Total {0:0.0} [green]{1}[/]",
                    12.5m,
                    "ready");
                markup.DefaultStyle = new Style(ConsoleColor.Yellow);

                AnsiConsole.Write(markup);

                VisualAssert.ContainsText(scope.Driver, "Total 12.5 ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Total 12.5 ") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ready") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup escapes doubled brackets and keeps unknown tags literal.
        /// </summary>
        [Fact]
        public void MarkupLine_EscapesLiteralBracketsAndUnknownTags()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.MarkupLine("[[literal]] [not-a-color]tag[/]");

                VisualAssert.ContainsText(scope.Driver, "[literal] [not-a-color]tag[/]");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup applies unclosed tags until the end of the rendered message.
        /// </summary>
        [Fact]
        public void MarkupLine_UnclosedTagAppliesUntilEnd()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.MarkupLine("Status: [cyan]pending");

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("pending") && write.Color == ConsoleColor.Cyan);
                VisualAssert.ContainsText(scope.Driver, "Status: pending");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Markup entries can be reused as renderable components in captured layouts.
        /// </summary>
        [Fact]
        public void MarkupEntry_Renderable_CanBeCapturedAndComposed()
        {
            IConsoulRenderable markup = new MarkupEntry("Deploy [green]ready[/] for [cyan]prod[/].");

            string captured = Consoul.Capture(markup, width: 48);

            Assert.Contains("Deploy ready for prod.", captured);

            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Panel(markup, title: "Message", width: 40, captureWidth: 36);

                VisualAssert.ContainsText(scope.Driver, "Message");
                VisualAssert.ContainsText(scope.Driver, "Deploy ready for prod.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic views can use markup while rendering model-backed content.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderMarkupSections()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Dashboard", new DashboardModel { Status = "Ready" })
                        .RenderModel(model => new MarkupEntry("Status: [green]" + Consoul.EscapeMarkup(model.Status) + "[/]"))
                        .Option("Refresh", model => model.Status = "Refreshed");

                    view.Render();

                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                    VisualAssert.ContainsText(scope.Driver, "Status: Ready");
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
