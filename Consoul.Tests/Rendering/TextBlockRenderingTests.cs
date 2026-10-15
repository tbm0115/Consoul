using System;
using System.Globalization;
using System.IO;
using ConsoulLibrary.Color;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for text block view helpers.
    /// </summary>
    public class TextBlockRenderingTests
    {
        /// <summary>
        /// Text blocks wrap long content and stay inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(80)]
        public void TextBlock_LongContent_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.TextBlock(
                    "This text block contains a verylongtokenwithoutnaturalbreaks and a sentence for wrapping.",
                    new TextBlockOptions { Width = width, Alignment = TextBlockAlignment.Left });

                VisualAssert.WroteText(scope.Driver, "This");
                VisualAssert.WroteText(scope.Driver, "verylongtoken");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Center and right alignment pad content within the configured width.
        /// </summary>
        [Fact]
        public void TextBlock_Alignment_PadsWithinWidth()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Consoul.TextBlock("Center", new TextBlockOptions { Width = 12, Alignment = TextBlockAlignment.Center });
                Consoul.TextBlock("Right", new TextBlockOptions { Width = 12, Alignment = TextBlockAlignment.Right });

                Assert.Contains(scope.Driver.Lines, line => line == "   Center   ");
                Assert.Contains(scope.Driver.Lines, line => line == "       Right");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text and paragraph builders accept aligned aliases alongside justified aliases.
        /// </summary>
        [Fact]
        public void TextAndParagraph_AlignedAliases_MapToExpectedJustification()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                AnsiConsole.Write(new Text("Left").Width(10).LeftAligned());
                AnsiConsole.Write(new Text("Right").Width(10).RightAligned());
                AnsiConsole.Write(new Paragraph("PLeft").Width(10).LeftAligned());
                AnsiConsole.Write(new Paragraph("PRight").Width(10).RightAligned());

                Assert.Contains(scope.Driver.Lines, line => line == "Left      ");
                Assert.Contains(scope.Driver.Lines, line => line == "     Right");
                Assert.Contains(scope.Driver.Lines, line => line == "PLeft");
                Assert.Contains(scope.Driver.Lines, line => line == "    PRight");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text blocks honor padding, configured width, and colors.
        /// </summary>
        [Fact]
        public void TextBlock_PaddingAndColors_StayWithinConfiguredWidth()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new TextBlockOptions
                {
                    Width = 18,
                    LeftPadding = 2,
                    RightPadding = 3,
                    TopPadding = 1,
                    BottomPadding = 1,
                    Scheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                };

                Consoul.TextBlock("Alpha Beta", options);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Alpha Beta") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("  Alpha Beta") && line.Length == 18);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 18, "Text block line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style paragraphs support appended text segments with individual styles.
        /// </summary>
        [Fact]
        public void Paragraph_Append_RendersStyledSegmentsAndTracksLength()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var paragraph = new Paragraph("Alpha", new Style(ConsoleColor.Green))
                    .Append(" Beta", new Style(ConsoleColor.Yellow))
                    .Append(Environment.NewLine + "Gamma", new Style(ConsoleColor.Cyan))
                    .Width(32)
                    .LeftJustified();

                Assert.Equal("Alpha Beta".Length + Environment.NewLine.Length + "Gamma".Length, paragraph.Length);
                Assert.Equal(2, paragraph.Lines);

                AnsiConsole.Write(paragraph);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Alpha") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" Beta") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Gamma") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Paragraph line and styled append helpers keep fluent POC prose compact without losing style metadata.
        /// </summary>
        [Fact]
        public void Paragraph_AppendLineAndStyledHelpers_RenderStyledSegments()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var paragraph = new Paragraph("Alpha", ConsoleColor.Green)
                    .Append(" Beta", ConsoleColor.Yellow)
                    .AppendLineStyled(" Ready", "white on navy")
                    .AppendStyled("Gamma", "#22c55e")
                    .Width(40);

                Assert.Equal("Alpha Beta Ready".Length + Environment.NewLine.Length + "Gamma".Length, paragraph.Length);
                Assert.Equal(2, paragraph.Lines);

                AnsiConsole.Write(paragraph);

                VisualAssert.ContainsText(scope.Driver, "Alpha Beta Ready");
                VisualAssert.ContainsText(scope.Driver, "Gamma");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Alpha") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" Beta") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" Ready") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Gamma") && write.Color == Style.ParseForeground("#22c55e", RenderOptions.DefaultColor));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text and paragraph builders accept shared palette color values directly.
        /// </summary>
        [Fact]
        public void TextAndParagraph_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                AnsiConsole.Write(new Text("Palette text", SpectreColor.Aqua).Width(24));
                AnsiConsole.Write(new Text("Rgb text").Color(RgbColor.Yellow).Width(24));

                var paragraph = new Paragraph("Alpha", SpectreColor.Green)
                    .Append(" Beta", RgbColor.Yellow)
                    .AppendLine(" Ready", SpectreColor.White)
                    .Append("Gamma", RgbColor.Cyan)
                    .Width(40);

                AnsiConsole.Write(paragraph);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Palette text") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rgb text") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Alpha") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" Beta") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" Ready") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Gamma") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Plain text color helpers clear previously configured RGB and decoration metadata.
        /// </summary>
        [Fact]
        public void Text_ColorHelperClearsPriorRichStyleMetadata()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 48, height: 8, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            console.Write(new Text("Plain text").Color("bold #112233").Color(ConsoleColor.Green).Width(24));

            string output = writer.ToString();
            Assert.Contains("Plain text", output);
            Assert.DoesNotContain("\u001b[1;", output);
            Assert.DoesNotContain("\u001b[38;2", output);
            Assert.DoesNotContain("[#112233", output);
            Assert.DoesNotContain("[bold", output);
        }

        /// <summary>
        /// Paragraph markup helpers preserve parsed segment styles and escape interpolated values for reusable view sections.
        /// </summary>
        [Fact]
        public void Paragraph_AppendMarkupHelpers_RenderParsedAndEscapedSegments()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                string owner = "Production [red]hot[/]";
                var paragraph = new Paragraph()
                    .AppendMarkupLine("[green]Ready[/] :check_mark:")
                    .AppendMarkupInterpolated($"Owner [cyan]{owner}[/]")
                    .Width(52);

                Assert.Equal(2, paragraph.Lines);

                AnsiConsole.Write(paragraph);

                VisualAssert.ContainsText(scope.Driver, "Ready " + Emoji.Known.CheckMark);
                VisualAssert.ContainsText(scope.Driver, "Owner Production [red]hot[/]");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production [red]hot[/]") && write.Color == ConsoleColor.Cyan);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("hot") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text and paragraph overflow helpers crop or ellipsize long content without requiring real console handles.
        /// </summary>
        [Fact]
        public void TextAndParagraph_OverflowHelpers_CropAndEllipsize()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                AnsiConsole.Write(new Text("abcdefghijklmnopqrstuvwxyz")
                    .Width(10)
                    .Overflow(Overflow.Ellipsis));

                var paragraph = new Paragraph("abcdefghijklmnopqrstuvwxyz")
                {
                    Justification = Justify.Right,
                    Overflow = Overflow.Crop
                }.Width(12);

                AnsiConsole.Write(paragraph);

                VisualAssert.ContainsText(scope.Driver, "abcdefg...");
                VisualAssert.ContainsText(scope.Driver, "abcdefghijkl");
                Assert.DoesNotContain("mnop", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text and paragraph overflow convenience aliases map to the existing overflow renderer.
        /// </summary>
        [Fact]
        public void TextAndParagraph_OverflowConvenienceAliases_CropAndEllipsize()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                AnsiConsole.Write(new Text("abcdefghijklmnopqrstuvwxyz")
                    .Width(9)
                    .Ellipsis());

                AnsiConsole.Write(new Paragraph("abcdefghijklmnopqrstuvwxyz")
                    .Width(8)
                    .Crop());

                VisualAssert.ContainsText(scope.Driver, "abcdef...");
                VisualAssert.ContainsText(scope.Driver, "abcdefgh");
                Assert.DoesNotContain("ijkl", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text object-initializer snippets can set style metadata without using the fluent color helper.
        /// </summary>
        [Fact]
        public void Text_ObjectInitializer_AcceptsStyleProperty()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var text = new Text("Ready")
                {
                    Style = new Style(ConsoleColor.Cyan, ConsoleColor.DarkBlue),
                    Justification = Justify.Center
                };
                text.Width(20);

                AnsiConsole.Write(text);

                Assert.NotNull(text.Style);
                Assert.Equal(ConsoleColor.Cyan, text.Style.Foreground);
                Assert.Equal(ConsoleColor.DarkBlue, text.Style.Background);
                Assert.Equal(Justify.Center, text.Justification);
                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Cyan && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 20, "Text line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text exposes Spectre-style empty/new-line instances and line metadata.
        /// </summary>
        [Fact]
        public void Text_Metadata_ReportsLengthAndLines()
        {
            Assert.Equal(0, Text.Empty.Length);
            Assert.Equal(1, Text.Empty.Lines);
            Assert.Equal(Environment.NewLine.Length, Text.NewLine.Length);
            Assert.True(Text.NewLine.Lines >= 2);

            string content = "One" + Environment.NewLine + "Two";
            var text = new Text(content);

            Assert.Equal(2, text.Lines);
            Assert.Equal(content.Length, text.Length);
        }

        /// <summary>
        /// Model-backed dynamic views can render text blocks as aligned explanatory sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderTextBlockSections()
        {
            using (var scope = new ConsoleDriverScope(36))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Dashboard", new DashboardModel { Status = "Green" })
                        .RenderModel(model =>
                        {
                            Consoul.TextBlock(
                                "Current status: " + model.Status,
                                new TextBlockOptions
                                {
                                    Width = 30,
                                    Alignment = TextBlockAlignment.Center,
                                    Scheme = new ColorScheme
                                    {
                                        Color = ConsoleColor.Green,
                                        BackgroundColor = ConsoleColor.Black
                                    }
                                });
                        })
                        .Option("Refresh", model => model.Status = "Refreshed");

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Current status: Green");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Current status: Green") && write.Color == ConsoleColor.Green);
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
