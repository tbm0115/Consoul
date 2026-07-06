using System;
using ConsoulLibrary.Color;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for terminal path view helpers.
    /// </summary>
    public class TextPathRenderingTests
    {
        /// <summary>
        /// Long paths are elided and kept inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(12)]
        [InlineData(32)]
        [InlineData(80)]
        public void TextPath_LongPath_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.TextPath(@"C:\Users\LightWorks\Documents\GitHub\Consoul\Consoul.Tests\Rendering\TextPathRenderingTests.cs", width: width);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("..."));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Path rendering colorizes root, directory, file, extension, and separator segments independently.
        /// </summary>
        [Fact]
        public void TextPath_CustomOptions_ColorizeSegments()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.TextPath(@"C:\repo\src\Program.cs", new TextPathOptions
                {
                    Width = 80,
                    RootScheme = Scheme(ConsoleColor.DarkGray),
                    SeparatorScheme = Scheme(ConsoleColor.Yellow),
                    DirectoryScheme = Scheme(ConsoleColor.Blue),
                    FileNameScheme = Scheme(ConsoleColor.Cyan),
                    ExtensionScheme = Scheme(ConsoleColor.Green)
                });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("C:") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(@"\") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("repo") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Program") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(".cs") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text path helpers accept Spectre-style style strings through the facade.
        /// </summary>
        [Fact]
        public void TextPath_StyleStringHelpers_ColorizeSegments()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                AnsiConsole.TextPathStyled(
                    @"C:\repo\src\Program.cs",
                    width: 80,
                    directoryStyle: "grey",
                    fileNameStyle: "aqua",
                    separatorStyle: "yellow");

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("repo") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Program") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(@"\") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style TextPath aliases configure width, elision, separators, and segment colors.
        /// </summary>
        [Fact]
        public void TextPath_FluentBuilder_ColorizesSegmentsAndComposes()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var textPath = new TextPath(@"C:\repo\src\Program.cs")
                    .Width(80)
                    .Separator("/")
                    .Ellipsis("...")
                    .RootColor(Colors.DarkGrey)
                    .DirectoryStyle(new Style(Decoration.Dim, Colors.Blue))
                    .StemColor(Colors.Aqua)
                    .ExtensionColor("green")
                    .SeparatorColor("yellow");

                AnsiConsole.Write(textPath);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("C:") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("repo") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Program") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(".cs") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("/") && write.Color == ConsoleColor.Yellow);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains(@"\"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text path segment color helpers accept shared palette and RGB colors.
        /// </summary>
        [Fact]
        public void TextPath_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                AnsiConsole.Write(new TextPath(@"C:\repo\src\Program.cs")
                    .Width(80)
                    .Separator("/")
                    .RootColor(SpectreColor.DarkGray)
                    .DirectoryColor(RgbColor.Blue)
                    .StemColor(SpectreColor.Cyan)
                    .ExtensionColor(RgbColor.Green)
                    .SeparatorColor(SpectreColor.Yellow));

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("C:") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("repo") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Program") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(".cs") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("/") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Leaf aliases style both the final path stem and extension for quick pasted snippets.
        /// </summary>
        [Fact]
        public void TextPath_FluentBuilder_LeafColorStylesStemAndExtension()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                AnsiConsole.Write(new TextPath("/var/log/consoul.json")
                    .Width(64)
                    .LeafColor("aqua"));

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("consoul") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(".json") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Forward-slash paths preserve forward separators by default.
        /// </summary>
        [Fact]
        public void TextPath_ForwardSlashPath_PreservesSeparator()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Consoul.TextPath("/usr/local/bin/consoul", width: 40);

                VisualAssert.ContainsText(scope.Driver, "/usr/local/bin/consoul");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains(@"\"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Long final path segments are middle-elided while preserving bounded output.
        /// </summary>
        [Fact]
        public void TextPath_LongLeaf_ElidesLeaf()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                Consoul.TextPath("very-long-proof-of-concept-settings-file.json", width: 24);

                VisualAssert.ContainsText(scope.Driver, "...");
                VisualAssert.ContainsText(scope.Driver, "json");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render text paths as diagnostic content.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderTextPathSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Artifacts", new ArtifactModel { Path = @"C:\builds\consoul\artifacts\release.zip" })
                        .RenderModel(model => Consoul.TextPath(model.Path, width: 36, fileNameColor: ConsoleColor.Green))
                        .Option("Archive", model => model.Path = @"C:\builds\consoul\artifacts\archived.zip");

                    view.Render();

                    VisualAssert.ContainsText(scope.Driver, "release");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("release") && write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private sealed class ArtifactModel
        {
            public string Path { get; set; } = string.Empty;
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
