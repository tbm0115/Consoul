using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a paragraph of text where appended parts can have individual styles.
    /// </summary>
    public class Paragraph : IRenderable
    {
        private readonly List<ParagraphRun> _runs = new List<ParagraphRun>();
        private readonly TextBlockOptions _options = new TextBlockOptions();

        /// <summary>
        /// Initializes a new instance of the <see cref="Paragraph"/> class.
        /// </summary>
        public Paragraph()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Paragraph"/> class.
        /// </summary>
        /// <param name="text">The paragraph text.</param>
        /// <param name="style">Optional style applied to the text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        public Paragraph(string text, Style style = null, Link link = null)
        {
            Append(text, style, link);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Paragraph"/> class.
        /// </summary>
        /// <param name="text">The paragraph text.</param>
        /// <param name="color">Foreground color applied to the text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        public Paragraph(string text, ConsoleColor color, Link link = null)
        {
            Append(text, color, link);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Paragraph"/> class.
        /// </summary>
        /// <param name="text">The paragraph text.</param>
        /// <param name="color">Foreground color applied to the text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        public Paragraph(string text, SpectreColor color, Link link = null)
        {
            Append(text, color, link);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Paragraph"/> class.
        /// </summary>
        /// <param name="text">The paragraph text.</param>
        /// <param name="color">Foreground color applied to the text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        public Paragraph(string text, RgbColor color, Link link = null)
        {
            Append(text, color, link);
        }

        /// <summary>
        /// Gets the options used to render the paragraph.
        /// </summary>
        public TextBlockOptions Options => _options;

        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        public Justify? Justification
        {
            get => ToJustify(_options.Alignment);
            set => _options.Alignment = value.HasValue ? JustifyMap.ToTextBlockAlignment(value.Value) : TextBlockAlignment.Left;
        }

        /// <summary>
        /// Gets or sets how content wider than the available line width should be handled.
        /// </summary>
        public Overflow? Overflow
        {
            get => _options.Overflow;
            set => _options.Overflow = value ?? ConsoulLibrary.Overflow.Wrap;
        }

        /// <summary>
        /// Gets the character count of the paragraph.
        /// </summary>
        public int Length => _runs.Sum(run => run.Text.Length);

        /// <summary>
        /// Gets the number of logical lines in the paragraph.
        /// </summary>
        public int Lines => BuildLogicalLines().Count;

        /// <summary>
        /// Appends text to the paragraph.
        /// </summary>
        /// <param name="text">The text to append.</param>
        /// <param name="style">Optional style applied to the appended text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph Append(string text, Style style = null, Link link = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            AppendRun(text, (style ?? new Style()).ToColorScheme(RenderOptions.DefaultScheme), link);
            return this;
        }

        /// <summary>
        /// Appends text to the paragraph using the supplied foreground color.
        /// </summary>
        /// <param name="text">The text to append.</param>
        /// <param name="color">Foreground color applied to the appended text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph Append(string text, ConsoleColor color, Link link = null)
        {
            return Append(text, new Style(color), link);
        }

        /// <summary>
        /// Appends text to the paragraph using the supplied foreground color.
        /// </summary>
        /// <param name="text">The text to append.</param>
        /// <param name="color">Foreground color applied to the appended text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph Append(string text, SpectreColor color, Link link = null)
        {
            return Append(text, new Style(color), link);
        }

        /// <summary>
        /// Appends text to the paragraph using the supplied foreground color.
        /// </summary>
        /// <param name="text">The text to append.</param>
        /// <param name="color">Foreground color applied to the appended text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph Append(string text, RgbColor color, Link link = null)
        {
            return Append(text, new Style(color), link);
        }

        /// <summary>
        /// Appends text to the paragraph using Spectre-style style text such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c>.
        /// </summary>
        /// <param name="text">The text to append.</param>
        /// <param name="style">Optional style applied to the appended text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph AppendStyled(string text, string style = null, Link link = null)
        {
            return Append(text, ParseOptionalStyle(style), link);
        }

        /// <summary>
        /// Appends a line break to the paragraph.
        /// </summary>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph AppendLine()
        {
            return Append(Environment.NewLine);
        }

        /// <summary>
        /// Appends text and then a line break to the paragraph.
        /// </summary>
        /// <param name="text">The text to append before the line break.</param>
        /// <param name="style">Optional style applied to the appended text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph AppendLine(string text, Style style = null, Link link = null)
        {
            Append(text, style, link);
            return AppendLine();
        }

        /// <summary>
        /// Appends text using the supplied foreground color and then a line break to the paragraph.
        /// </summary>
        /// <param name="text">The text to append before the line break.</param>
        /// <param name="color">Foreground color applied to the appended text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph AppendLine(string text, ConsoleColor color, Link link = null)
        {
            Append(text, color, link);
            return AppendLine();
        }

        /// <summary>
        /// Appends text using the supplied foreground color and then a line break to the paragraph.
        /// </summary>
        /// <param name="text">The text to append before the line break.</param>
        /// <param name="color">Foreground color applied to the appended text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph AppendLine(string text, SpectreColor color, Link link = null)
        {
            Append(text, color, link);
            return AppendLine();
        }

        /// <summary>
        /// Appends text using the supplied foreground color and then a line break to the paragraph.
        /// </summary>
        /// <param name="text">The text to append before the line break.</param>
        /// <param name="color">Foreground color applied to the appended text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph AppendLine(string text, RgbColor color, Link link = null)
        {
            Append(text, color, link);
            return AppendLine();
        }

        /// <summary>
        /// Appends text using Spectre-style style text and then a line break to the paragraph.
        /// </summary>
        /// <param name="text">The text to append before the line break.</param>
        /// <param name="style">Optional style applied to the appended text.</param>
        /// <param name="link">Optional hyperlink metadata retained for migration-friendly snippets.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph AppendLineStyled(string text, string style = null, Link link = null)
        {
            AppendStyled(text, style, link);
            return AppendLine();
        }

        /// <summary>
        /// Appends Spectre-style markup to the paragraph while preserving the parsed segment styles.
        /// </summary>
        /// <param name="markup">The markup text to append. A <see langword="null"/> value is treated as empty markup.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph AppendMarkup(string markup)
        {
            foreach (MarkupSegment segment in MarkupEntry.Parse(markup ?? string.Empty, RenderOptions.DefaultScheme))
            {
                AppendRun(segment.Text, segment.Scheme, segment.Link);
            }

            return this;
        }

        /// <summary>
        /// Appends escaped interpolated Spectre-style markup to the paragraph while preserving template markup.
        /// </summary>
        /// <param name="markup">The interpolated markup value to append.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph AppendMarkupInterpolated(FormattableString markup)
        {
            return AppendMarkup(MarkupEntry.FormatInterpolated(markup));
        }

        /// <summary>
        /// Appends Spectre-style markup and then a line break to the paragraph.
        /// </summary>
        /// <param name="markup">The markup text to append before the line break. A <see langword="null"/> value is treated as empty markup.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph AppendMarkupLine(string markup)
        {
            AppendMarkup(markup);
            return AppendLine();
        }

        /// <summary>
        /// Appends escaped interpolated Spectre-style markup and then a line break to the paragraph.
        /// </summary>
        /// <param name="markup">The interpolated markup value to append before the line break.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph AppendMarkupLineInterpolated(FormattableString markup)
        {
            AppendMarkupInterpolated(markup);
            return AppendLine();
        }

        private static Style ParseOptionalStyle(string style)
        {
            return string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style);
        }

        private void AppendRun(string text, ColorScheme scheme, Link link)
        {
            _runs.Add(new ParagraphRun(text, scheme, link));
        }

        /// <summary>
        /// Sets the requested paragraph width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph Width(int? width)
        {
            _options.Width = width;
            return this;
        }

        /// <summary>
        /// Aligns paragraph text to the left.
        /// </summary>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph LeftJustified()
        {
            _options.Alignment = TextBlockAlignment.Left;
            return this;
        }

        /// <summary>
        /// Centers paragraph text.
        /// </summary>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph Centered()
        {
            _options.Alignment = TextBlockAlignment.Center;
            return this;
        }

        /// <summary>
        /// Aligns paragraph text to the right.
        /// </summary>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph RightJustified()
        {
            _options.Alignment = TextBlockAlignment.Right;
            return this;
        }

        /// <summary>
        /// Sets paragraph justification using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The justification to apply.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph Justify(Justify justify)
        {
            _options.Alignment = JustifyMap.ToTextBlockAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets how content wider than the available line width should be handled.
        /// </summary>
        /// <param name="overflow">The overflow behavior.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public Paragraph OverflowMode(Overflow overflow)
        {
            _options.Overflow = overflow;
            return this;
        }

        /// <summary>
        /// Renders the paragraph to the active console driver.
        /// </summary>
        public void Render()
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            foreach (List<StyledCharacter> logicalLine in BuildLogicalLines())
            {
                foreach (List<StyledCharacter> line in ApplyOverflow(logicalLine, width, _options.Overflow))
                {
                    WriteAligned(line, width);
                }
            }
        }

        private List<List<StyledCharacter>> BuildLogicalLines()
        {
            var lines = new List<List<StyledCharacter>> { new List<StyledCharacter>() };
            foreach (ParagraphRun run in _runs)
            {
                foreach (char character in NormalizeLineEndings(run.Text))
                {
                    if (character == '\n')
                    {
                        lines.Add(new List<StyledCharacter>());
                    }
                    else
                    {
                        lines[lines.Count - 1].Add(new StyledCharacter(character, run.Scheme, run.Link));
                    }
                }
            }

            return lines;
        }

        private static string NormalizeLineEndings(string text)
        {
            return (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
        }

        private static IEnumerable<List<StyledCharacter>> ApplyOverflow(List<StyledCharacter> logicalLine, int width, Overflow overflow)
        {
            width = Math.Max(1, width);
            if (overflow == ConsoulLibrary.Overflow.Wrap)
            {
                if (logicalLine.Count == 0)
                {
                    yield return new List<StyledCharacter>();
                    yield break;
                }

                int offset = 0;
                while (offset < logicalLine.Count)
                {
                    int count = Math.Min(width, logicalLine.Count - offset);
                    yield return logicalLine.GetRange(offset, count);
                    offset += count;
                }

                yield break;
            }

            if (logicalLine.Count <= width)
            {
                yield return logicalLine;
                yield break;
            }

            if (overflow == ConsoulLibrary.Overflow.Ellipsis)
            {
                yield return ApplyEllipsis(logicalLine, width);
                yield break;
            }

            yield return logicalLine.GetRange(0, width);
        }

        private static List<StyledCharacter> ApplyEllipsis(List<StyledCharacter> logicalLine, int width)
        {
            var result = new List<StyledCharacter>();
            ColorScheme scheme = logicalLine.Count > 0 ? logicalLine[0].Scheme : RenderOptions.DefaultScheme;
            Link link = logicalLine.Count > 0 ? logicalLine[0].Link : null;
            if (width <= 3)
            {
                for (int index = 0; index < width; index++)
                {
                    result.Add(new StyledCharacter('.', scheme, link));
                }

                return result;
            }

            result.AddRange(logicalLine.Take(width - 3));
            result.Add(new StyledCharacter('.', scheme, link));
            result.Add(new StyledCharacter('.', scheme, link));
            result.Add(new StyledCharacter('.', scheme, link));
            return result;
        }

        private void WriteAligned(List<StyledCharacter> line, int width)
        {
            int contentWidth = Math.Min(width, line.Count);
            int remaining = Math.Max(0, width - contentWidth);
            int leftPadding;
            switch (_options.Alignment)
            {
                case TextBlockAlignment.Center:
                    leftPadding = remaining / 2;
                    break;
                case TextBlockAlignment.Right:
                    leftPadding = remaining;
                    break;
                default:
                    leftPadding = 0;
                    break;
            }

            if (leftPadding > 0)
            {
                Consoul.Write(new string(' ', leftPadding), RenderOptions.DefaultScheme, false);
            }

            WriteStyledCharacters(line);
            Consoul.Write(string.Empty, RenderOptions.DefaultScheme, true);
        }

        private static void WriteStyledCharacters(IReadOnlyList<StyledCharacter> characters)
        {
            if (characters.Count == 0)
            {
                return;
            }

            var builder = new StringBuilder();
            ColorScheme scheme = characters[0].Scheme;
            Link link = characters[0].Link;
            for (int index = 0; index < characters.Count; index++)
            {
                StyledCharacter current = characters[index];
                if (current.Scheme.Color != scheme.Color ||
                    current.Scheme.BackgroundColor != scheme.BackgroundColor ||
                    !TerminalLinkWriter.SameTarget(current.Link, link))
                {
                    TerminalLinkWriter.WriteText(builder.ToString(), scheme, link);
                    builder.Clear();
                    scheme = current.Scheme;
                    link = current.Link;
                }

                builder.Append(current.Character);
            }

            if (builder.Length > 0)
            {
                TerminalLinkWriter.WriteText(builder.ToString(), scheme, link);
            }
        }

        private static Justify ToJustify(TextBlockAlignment alignment)
        {
            switch (alignment)
            {
                case TextBlockAlignment.Center:
                    return ConsoulLibrary.Justify.Center;
                case TextBlockAlignment.Right:
                    return ConsoulLibrary.Justify.Right;
                default:
                    return ConsoulLibrary.Justify.Left;
            }
        }

        private readonly struct ParagraphRun
        {
            public ParagraphRun(string text, ColorScheme scheme, Link link)
            {
                Text = text ?? string.Empty;
                Scheme = scheme;
                Link = link;
            }

            public string Text { get; }

            public ColorScheme Scheme { get; }

            public Link Link { get; }
        }

        private readonly struct StyledCharacter
        {
            public StyledCharacter(char character, ColorScheme scheme, Link link)
            {
                Character = character;
                Scheme = scheme;
                Link = link;
            }

            public char Character { get; }

            public ColorScheme Scheme { get; }

            public Link Link { get; }
        }
    }
}
