using System;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Validates RGB color compatibility helpers for copied Spectre-style snippets.
    /// </summary>
    public class ColorCompatibilityTests
    {
        /// <summary>
        /// RGB named colors map to the nearest ConsoleColor used by Consoul renderers.
        /// </summary>
        [Fact]
        public void RgbColor_NamedColors_MapToConsoleColors()
        {
            Assert.Equal(ConsoleColor.Red, RgbColor.Red.ToConsoleColor());
            Assert.Equal(ConsoleColor.Cyan, RgbColor.Aqua.ToConsoleColor());
            Assert.Equal(ConsoleColor.DarkBlue, RgbColor.Navy.ToConsoleColor());

            ConsoleColor implicitColor = RgbColor.Yellow;

            Assert.Equal(ConsoleColor.Yellow, implicitColor);
        }

        /// <summary>
        /// RGB and hex colors retain their source channels while exposing the nearest console color.
        /// </summary>
        [Fact]
        public void RgbColor_RgbAndHex_MapToNearestConsoleColor()
        {
            RgbColor fromRgb = RgbColor.Rgb(255, 0, 0);
            RgbColor fromHex = RgbColor.FromHex("#0ff");

            Assert.Equal(255, fromRgb.R);
            Assert.Equal(0, fromRgb.G);
            Assert.Equal(0, fromRgb.B);
            Assert.Equal(ConsoleColor.Red, fromRgb.ConsoleColor);
            Assert.Equal(ConsoleColor.Cyan, fromHex.ConsoleColor);
            Assert.Equal("#00ffff", fromHex.ToString());
        }

        /// <summary>
        /// Named and hex color parsing is available for code that accepts user or configuration color tokens.
        /// </summary>
        [Fact]
        public void RgbColor_FromName_ParsesAliasesAndHexTokens()
        {
            Assert.Equal(ConsoleColor.Gray, RgbColor.FromName("grey").ConsoleColor);
            Assert.Equal(ConsoleColor.Green, RgbColor.FromName("#00ff00").ConsoleColor);
            Assert.True(RgbColor.TryFromName("aqua", out RgbColor aqua));
            Assert.Equal(ConsoleColor.Cyan, aqua.ConsoleColor);
            Assert.False(RgbColor.TryFromName("not-a-color", out _));
        }

        /// <summary>
        /// Spectre-style Color aliases map to Consoul's existing RGB and console color model.
        /// </summary>
        [Fact]
        public void Color_NamedRgbAndHexValues_MapToConsoleColors()
        {
            SpectreColor red = SpectreColor.Red;
            SpectreColor fromRgb = SpectreColor.Rgb(255, 0, 0);
            SpectreColor fromHex = SpectreColor.FromHex("#0ff");
            SpectreColor fromName = SpectreColor.FromName("grey");
            RgbColor rgbColor = (RgbColor)fromHex;
            ConsoleColor consoleColor = SpectreColor.Aqua;

            Assert.Equal(ConsoleColor.Red, red.ToConsoleColor());
            Assert.Equal(ConsoleColor.Red, fromRgb.ConsoleColor);
            Assert.Equal(ConsoleColor.Cyan, fromHex.ConsoleColor);
            Assert.Equal(ConsoleColor.Gray, fromName.ConsoleColor);
            Assert.Equal(0, fromHex.R);
            Assert.Equal(255, fromHex.G);
            Assert.Equal(255, fromHex.B);
            Assert.Equal("#00ffff", fromHex.ToString());
            Assert.Equal(RgbColor.Aqua, rgbColor);
            Assert.Equal(ConsoleColor.Cyan, consoleColor);
            Assert.True(SpectreColor.TryFromName("#00ff00", out SpectreColor parsed));
            Assert.Equal(ConsoleColor.Green, parsed.ConsoleColor);
            Assert.False(SpectreColor.TryFromHex("not-a-color", out _));
        }

        /// <summary>
        /// Style constructors accept RGB color values and preserve decoration metadata.
        /// </summary>
        [Fact]
        public void Style_AcceptsRgbColorValues()
        {
            var foregroundOnly = new Style(RgbColor.Red);
            var decorated = new Style(foreground: RgbColor.Yellow, decoration: Decoration.Bold);
            var foregroundAndBackground = new Style(RgbColor.White, RgbColor.Navy, Decoration.Underline);

            Assert.Equal(ConsoleColor.Red, foregroundOnly.Foreground);
            Assert.Equal(ConsoleColor.Yellow, decorated.Foreground);
            Assert.Equal(Decoration.Bold, decorated.Decoration);
            Assert.Equal(ConsoleColor.White, foregroundAndBackground.Foreground);
            Assert.Equal(ConsoleColor.DarkBlue, foregroundAndBackground.Background);
            Assert.Equal(Decoration.Underline, foregroundAndBackground.Decoration);
        }

        /// <summary>
        /// Style constructors accept Spectre-style Color values without ambiguous overload resolution.
        /// </summary>
        [Fact]
        public void Style_AcceptsColorValues()
        {
            var foregroundOnly = new Style(SpectreColor.Red);
            var decorated = new Style(SpectreColor.Yellow, Decoration.Bold);
            var foregroundAndBackground = new Style(SpectreColor.White, SpectreColor.Navy, Decoration.Underline);

            Assert.Equal(ConsoleColor.Red, foregroundOnly.Foreground);
            Assert.Equal(ConsoleColor.Yellow, decorated.Foreground);
            Assert.Equal(Decoration.Bold, decorated.Decoration);
            Assert.Equal(ConsoleColor.White, foregroundAndBackground.Foreground);
            Assert.Equal(ConsoleColor.DarkBlue, foregroundAndBackground.Background);
            Assert.Equal(Decoration.Underline, foregroundAndBackground.Decoration);
        }

        /// <summary>
        /// Style convenience helpers return immutable copies and preserve RGB color compatibility.
        /// </summary>
        [Fact]
        public void Style_ConvenienceHelpers_ReturnImmutableCopies()
        {
            Style plain = Style.Plain;
            Style styled = plain
                .WithForeground(RgbColor.Aqua)
                .WithBackground(RgbColor.Navy)
                .AddDecoration(Decoration.Bold)
                .AddDecoration(Decoration.Underline);
            Style replacedDecoration = styled.WithDecoration(Decoration.Italic);
            Style removedDecoration = styled.RemoveDecoration(Decoration.Bold);
            Style aliasRemovedDecoration = styled.WithoutDecoration(Decoration.Underline);

            Assert.Null(plain.Foreground);
            Assert.Null(plain.Background);
            Assert.Equal(Decoration.None, plain.Decoration);
            Assert.Equal(ConsoleColor.Cyan, styled.Foreground);
            Assert.Equal(ConsoleColor.DarkBlue, styled.Background);
            Assert.True(styled.Decoration.HasFlag(Decoration.Bold));
            Assert.True(styled.Decoration.HasFlag(Decoration.Underline));
            Assert.Equal(Decoration.Italic, replacedDecoration.Decoration);
            Assert.False(removedDecoration.Decoration.HasFlag(Decoration.Bold));
            Assert.True(removedDecoration.Decoration.HasFlag(Decoration.Underline));
            Assert.True(aliasRemovedDecoration.Decoration.HasFlag(Decoration.Bold));
            Assert.False(aliasRemovedDecoration.Decoration.HasFlag(Decoration.Underline));
            Assert.Equal(Decoration.Bold, Style.Bold.Decoration);
            Assert.Equal(Decoration.Dim, Style.Dim.Decoration);
            Assert.Equal(Decoration.Strikethrough, Style.Strikethrough.Decoration);
        }

        /// <summary>
        /// Style convenience helpers accept Spectre-style Color values.
        /// </summary>
        [Fact]
        public void Style_ConvenienceHelpers_AcceptColorValues()
        {
            Style style = Style.Plain
                .WithForeground(SpectreColor.Aqua)
                .WithBackground(SpectreColor.Navy)
                .AddDecoration(Decoration.Bold);

            Assert.Equal(ConsoleColor.Cyan, style.Foreground);
            Assert.Equal(ConsoleColor.DarkBlue, style.Background);
            Assert.Equal(Decoration.Bold, style.Decoration);
        }

        /// <summary>
        /// Parsed decoration-only style strings leave foreground and background unspecified for composition.
        /// </summary>
        [Fact]
        public void Style_ParseDecorationOnly_DoesNotForceDefaultColors()
        {
            Style italic = Style.Parse("italic");
            Style colored = Style.Parse("italic #336699 on #112233");

            Assert.Null(italic.Foreground);
            Assert.Null(italic.Background);
            Assert.Equal(Decoration.Italic, italic.Decoration);
            Assert.Null(italic.ForegroundRgb);
            Assert.Null(italic.BackgroundRgb);
            Assert.Equal(RgbColor.FromHex("#336699").ToConsoleColor(), colored.Foreground);
            Assert.Equal(RgbColor.FromHex("#112233").ToConsoleColor(), colored.Background);
            Assert.Equal(RgbColor.FromHex("#336699"), colored.ForegroundRgb);
            Assert.Equal(RgbColor.FromHex("#112233"), colored.BackgroundRgb);
        }

        /// <summary>
        /// Fluent renderable builders that accept ConsoleColor also accept RgbColor through implicit conversion.
        /// </summary>
        [Fact]
        public void RenderableBuilders_AcceptRgbColorValuesThroughImplicitConversion()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                AnsiConsole.Write(new Text("Ready").Color(RgbColor.Aqua));

                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent renderable builders that accept ConsoleColor also accept Spectre-style Color aliases through implicit conversion.
        /// </summary>
        [Fact]
        public void RenderableBuilders_AcceptColorValuesThroughImplicitConversion()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                AnsiConsole.Write(new Text("Ready").Color(SpectreColor.Aqua));

                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }
    }
}

namespace SpectreStyleSnippetCompatibility
{
    using System;
    using ConsoulLibrary;
    using ConsoulLibrary.Color;
    using ConsoulLibrary.Tests.Rendering;
    using Xunit;

    /// <summary>
    /// Verifies consumer snippets outside the ConsoulLibrary namespace can use the short Color.Red spelling.
    /// </summary>
    public class ColorSnippetCompatibilityTests
    {
        /// <summary>
        /// Spectre-style Color aliases compile in a typical consumer namespace.
        /// </summary>
        [Fact]
        public void ConsumerNamespace_CanUseShortColorAlias()
        {
            Style style = Style.Plain
                .WithForeground(Color.Red)
                .WithBackground(Color.Navy);

            using (var scope = new ConsoleDriverScope(40))
            {
                AnsiConsole.Write(new Text("Ready").Color(Color.Aqua));

                Assert.Equal(ConsoleColor.Red, style.Foreground);
                Assert.Equal(ConsoleColor.DarkBlue, style.Background);
                VisualAssert.ContainsText(scope.Driver, "Ready");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }
    }
}
