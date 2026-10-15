using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders responsive headline text for view titles, demos, and section openers.
    /// </summary>
    public class HeadlineEntry : IRenderable
    {
        private const int GlyphHeight = 5;
        private readonly HeadlineOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeadlineEntry"/> class.
        /// </summary>
        /// <param name="text">The headline text to render.</param>
        /// <param name="options">Optional rendering options for the headline.</param>
        public HeadlineEntry(string text, HeadlineOptions options = null)
        {
            Text = text ?? string.Empty;
            _options = options != null ? options.Clone() : new HeadlineOptions();
        }

        /// <summary>
        /// Gets or sets the headline text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets the options used to render the headline.
        /// </summary>
        public HeadlineOptions Options => _options;

        /// <summary>
        /// Renders the headline to the active console driver.
        /// </summary>
        public void Render()
        {
            switch (_options.Style)
            {
                case HeadlineStyle.Plain:
                    RenderPlain();
                    return;
                case HeadlineStyle.Box:
                    RenderBox();
                    return;
                case HeadlineStyle.Rule:
                    RenderRule();
                    return;
                default:
                    RenderBlock();
                    return;
            }
        }

        /// <summary>
        /// Renders a temporary headline to the active console driver.
        /// </summary>
        /// <param name="text">The headline text to render.</param>
        /// <param name="options">Optional rendering options for the headline.</param>
        public static void Render(string text, HeadlineOptions options = null)
        {
            new HeadlineEntry(text, options).Render();
        }

        private void RenderPlain()
        {
            TextBlockEntry.Render(Text, new TextBlockOptions
            {
                Width = _options.Width,
                Alignment = _options.Alignment,
                Scheme = _options.ResolveScheme(),
                Decoration = _options.Decoration,
                ForegroundRgb = _options.ForegroundRgb,
                BackgroundRgb = _options.BackgroundRgb
            });
        }

        private void RenderBox()
        {
            var options = new PanelOptions
            {
                Width = _options.Width,
                ContentScheme = _options.ResolveScheme(),
                BorderScheme = _options.ResolveBorderScheme(),
                TitleScheme = _options.ResolveScheme(),
                ContentDecoration = _options.Decoration,
                ContentForegroundRgb = _options.ForegroundRgb,
                ContentBackgroundRgb = _options.BackgroundRgb,
                TitleDecoration = _options.Decoration,
                TitleForegroundRgb = _options.ForegroundRgb,
                TitleBackgroundRgb = _options.BackgroundRgb
            };

            new PanelEntry(Text, options).Render();
        }

        private void RenderRule()
        {
            new RuleEntry(Text, new RuleOptions
            {
                Width = _options.Width,
                Alignment = ToRuleAlignment(_options.Alignment),
                RuleScheme = _options.ResolveBorderScheme(),
                TitleScheme = _options.ResolveScheme(),
                TitleDecoration = _options.Decoration,
                TitleForegroundRgb = _options.ForegroundRgb,
                TitleBackgroundRgb = _options.BackgroundRgb,
                Character = _options.RuleCharacter
            }).Render();
        }

        private void RenderBlock()
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            ColorScheme scheme = _options.ResolveScheme();
            foreach (string[] block in BuildBlocks(Text, width, _options.BlockCharacter, _options.BlockGap))
            {
                for (int row = 0; row < GlyphHeight; row++)
                {
                    MarkupRenderText.WriteSegments(
                        new[]
                        {
                            new FixedMessageSegment(
                                Align(block[row], width, _options.Alignment, _options.Pad),
                                scheme,
                                _options.Decoration,
                                null,
                                _options.ForegroundRgb,
                                _options.BackgroundRgb)
                        },
                        scheme,
                        true);
                }
            }
        }

        private static IEnumerable<string[]> BuildBlocks(string text, int width, char blockCharacter, int blockGap)
        {
            blockGap = Math.Max(0, blockGap);
            var current = EmptyBlock();
            bool hasContent = false;
            foreach (char character in (text ?? string.Empty).ToUpperInvariant())
            {
                string[] glyph = ConvertGlyph(GetGlyph(character), blockCharacter);
                string[] candidate = AppendGlyph(current, glyph, hasContent ? blockGap : 0);
                if (hasContent && MaxLength(candidate) > width)
                {
                    yield return TrimBlock(current, width);
                    current = glyph;
                }
                else
                {
                    current = candidate;
                }

                hasContent = true;
            }

            if (!hasContent)
            {
                current = ConvertGlyph(GetGlyph(' '), blockCharacter);
            }

            yield return TrimBlock(current, width);
        }

        private static string[] EmptyBlock()
        {
            return new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
        }

        private static string[] AppendGlyph(string[] current, string[] glyph, int gapWidth)
        {
            var result = new string[GlyphHeight];
            string gap = gapWidth > 0 ? new string(' ', gapWidth) : string.Empty;
            for (int row = 0; row < GlyphHeight; row++)
            {
                result[row] = current[row] + gap + glyph[row];
            }

            return result;
        }

        private static string[] ConvertGlyph(string[] glyph, char blockCharacter)
        {
            var result = new string[GlyphHeight];
            for (int row = 0; row < GlyphHeight; row++)
            {
                char[] chars = glyph[row].ToCharArray();
                for (int index = 0; index < chars.Length; index++)
                {
                    if (chars[index] == 'X')
                    {
                        chars[index] = blockCharacter;
                    }
                }

                result[row] = new string(chars);
            }

            return result;
        }

        private static string[] TrimBlock(string[] block, int width)
        {
            width = Math.Max(1, width);
            var result = new string[GlyphHeight];
            for (int row = 0; row < GlyphHeight; row++)
            {
                string line = block[row] ?? string.Empty;
                result[row] = line.Length > width ? line.Substring(0, width) : line.TrimEnd();
            }

            return result;
        }

        private static int MaxLength(string[] lines)
        {
            int max = 0;
            for (int index = 0; index < lines.Length; index++)
            {
                max = Math.Max(max, (lines[index] ?? string.Empty).Length);
            }

            return max;
        }

        private static string Align(string text, int width, TextBlockAlignment alignment, bool pad)
        {
            text = text ?? string.Empty;
            width = Math.Max(1, width);
            if (text.Length > width)
            {
                text = text.Substring(0, width);
            }

            int padding = width - text.Length;
            switch (alignment)
            {
                case TextBlockAlignment.Center:
                    int left = padding / 2;
                    string centered = new string(' ', left) + text;
                    return pad ? centered + new string(' ', padding - left) : centered;
                case TextBlockAlignment.Right:
                    return new string(' ', padding) + text;
                default:
                    return pad ? text.PadRight(width) : text;
            }
        }

        private static RuleTitleAlignment ToRuleAlignment(TextBlockAlignment alignment)
        {
            switch (alignment)
            {
                case TextBlockAlignment.Left:
                    return RuleTitleAlignment.Left;
                case TextBlockAlignment.Right:
                    return RuleTitleAlignment.Right;
                default:
                    return RuleTitleAlignment.Center;
            }
        }

        private static string[] GetGlyph(char character)
        {
            switch (character)
            {
                case 'A': return new[] { " XX ", "X  X", "XXXX", "X  X", "X  X" };
                case 'B': return new[] { "XXX ", "X  X", "XXX ", "X  X", "XXX " };
                case 'C': return new[] { " XXX", "X   ", "X   ", "X   ", " XXX" };
                case 'D': return new[] { "XXX ", "X  X", "X  X", "X  X", "XXX " };
                case 'E': return new[] { "XXXX", "X   ", "XXX ", "X   ", "XXXX" };
                case 'F': return new[] { "XXXX", "X   ", "XXX ", "X   ", "X   " };
                case 'G': return new[] { " XXX", "X   ", "X XX", "X  X", " XXX" };
                case 'H': return new[] { "X  X", "X  X", "XXXX", "X  X", "X  X" };
                case 'I': return new[] { "XXX", " X ", " X ", " X ", "XXX" };
                case 'J': return new[] { "  XX", "   X", "   X", "X  X", " XX " };
                case 'K': return new[] { "X  X", "X X ", "XX  ", "X X ", "X  X" };
                case 'L': return new[] { "X   ", "X   ", "X   ", "X   ", "XXXX" };
                case 'M': return new[] { "X   X", "XX XX", "X X X", "X   X", "X   X" };
                case 'N': return new[] { "X  X", "XX X", "X XX", "X  X", "X  X" };
                case 'O': return new[] { " XX ", "X  X", "X  X", "X  X", " XX " };
                case 'P': return new[] { "XXX ", "X  X", "XXX ", "X   ", "X   " };
                case 'Q': return new[] { " XX ", "X  X", "X  X", "X XX", " XXX" };
                case 'R': return new[] { "XXX ", "X  X", "XXX ", "X X ", "X  X" };
                case 'S': return new[] { " XXX", "X   ", " XX ", "   X", "XXX " };
                case 'T': return new[] { "XXXXX", "  X  ", "  X  ", "  X  ", "  X  " };
                case 'U': return new[] { "X  X", "X  X", "X  X", "X  X", " XX " };
                case 'V': return new[] { "X   X", "X   X", " X X ", " X X ", "  X  " };
                case 'W': return new[] { "X   X", "X   X", "X X X", "XX XX", "X   X" };
                case 'X': return new[] { "X   X", " X X ", "  X  ", " X X ", "X   X" };
                case 'Y': return new[] { "X   X", " X X ", "  X  ", "  X  ", "  X  " };
                case 'Z': return new[] { "XXXX", "   X", "  X ", " X  ", "XXXX" };
                case '0': return new[] { " XX ", "X  X", "X  X", "X  X", " XX " };
                case '1': return new[] { " X ", "XX ", " X ", " X ", "XXX" };
                case '2': return new[] { "XXX ", "   X", " XX ", "X   ", "XXXX" };
                case '3': return new[] { "XXX ", "   X", " XX ", "   X", "XXX " };
                case '4': return new[] { "X  X", "X  X", "XXXX", "   X", "   X" };
                case '5': return new[] { "XXXX", "X   ", "XXX ", "   X", "XXX " };
                case '6': return new[] { " XXX", "X   ", "XXX ", "X  X", " XX " };
                case '7': return new[] { "XXXX", "   X", "  X ", " X  ", "X   " };
                case '8': return new[] { " XX ", "X  X", " XX ", "X  X", " XX " };
                case '9': return new[] { " XX ", "X  X", " XXX", "   X", "XXX " };
                case '-': return new[] { "   ", "   ", "XXX", "   ", "   " };
                case '_': return new[] { "    ", "    ", "    ", "    ", "XXXX" };
                case '.': return new[] { "  ", "  ", "  ", "  ", "X " };
                case ':': return new[] { " ", "X", " ", "X", " " };
                case '!': return new[] { "X", "X", "X", " ", "X" };
                case '?': return new[] { "XXX ", "   X", " XX ", "    ", " X  " };
                case '/': return new[] { "   X", "  X ", " X  ", "X   ", "    " };
                case '\\': return new[] { "X   ", " X  ", "  X ", "   X", "    " };
                case ' ': return new[] { "   ", "   ", "   ", "   ", "   " };
                default: return new[] { "XXX ", "   X", " XX ", "    ", " X  " };
            }
        }
    }
}
