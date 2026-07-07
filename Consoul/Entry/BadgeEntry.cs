using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a compact single-line badge for statuses, tags, and model state.
    /// </summary>
    public class BadgeEntry : IRenderable
    {
        private readonly BadgeOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeEntry"/> class.
        /// </summary>
        /// <param name="label">The badge label to render.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional rendering options for the badge.</param>
        public BadgeEntry(string label, BadgeKind kind = BadgeKind.Default, BadgeOptions options = null)
        {
            Label = label ?? string.Empty;
            Kind = kind;
            _options = options != null ? options.Clone() : new BadgeOptions();
        }

        /// <summary>
        /// Gets or sets the badge label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the visual intent of the badge.
        /// </summary>
        public BadgeKind Kind { get; set; }

        /// <summary>
        /// Gets the options used to render the badge.
        /// </summary>
        public BadgeOptions Options => _options;

        /// <summary>
        /// Renders the badge to the active console driver.
        /// </summary>
        /// <param name="writeLine">Indicates whether to append a newline after writing the badge.</param>
        public void Render(bool writeLine = true)
        {
            ColorScheme scheme = _options.ResolveScheme(Kind);
            MarkupRenderText.WriteSegments(BuildSegments(ResolveWidth(null), scheme), scheme, writeLine);
        }

        void IConsoulRenderable.Render()
        {
            Render();
        }

        /// <summary>
        /// Creates the plain text representation of the badge.
        /// </summary>
        /// <param name="maxWidth">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded single-line badge string.</returns>
        public string ToText(int? maxWidth = null)
        {
            return MarkupRenderText.GetPlainText(BuildSegments(ResolveWidth(maxWidth), _options.ResolveScheme(Kind)));
        }

        private IList<FixedMessageSegment> BuildSegments(int width, ColorScheme scheme)
        {
            width = Math.Max(0, width);
            if (width <= 0)
            {
                return new List<FixedMessageSegment>();
            }

            IList<FixedMessageSegment> label = NormalizeLabelSegments(Label, _options.Uppercase, scheme);
            string leftEdge = _options.LeftEdge ?? string.Empty;
            string rightEdge = _options.RightEdge ?? string.Empty;
            int padding = Math.Max(0, _options.HorizontalPadding);
            string leftPadding = new string(' ', padding);
            string rightPadding = new string(' ', padding);

            int badgeWidth = leftEdge.Length + leftPadding.Length + MarkupRenderText.GetLength(label) + rightPadding.Length + rightEdge.Length;
            if (badgeWidth <= width)
            {
                return BuildBadgeSegments(leftEdge, leftPadding, label, rightPadding, rightEdge, scheme);
            }

            int availableLabelWidth = width - leftEdge.Length - rightEdge.Length - leftPadding.Length - rightPadding.Length;
            if (availableLabelWidth > 0)
            {
                IList<FixedMessageSegment> clippedLabel = TrimWithEllipsis(label, availableLabelWidth, _options.Ellipsis, scheme);
                IList<FixedMessageSegment> badge = BuildBadgeSegments(leftEdge, leftPadding, clippedLabel, rightPadding, rightEdge, scheme);
                if (MarkupRenderText.GetLength(badge) <= width)
                {
                    return badge;
                }
            }

            return MarkupRenderText.TruncateSegments(
                BuildBadgeSegments(leftEdge, leftPadding, label, rightPadding, rightEdge, scheme),
                width);
        }

        /// <summary>
        /// Renders a temporary badge to the active console driver.
        /// </summary>
        /// <param name="label">The badge label to render.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional rendering options for the badge.</param>
        /// <param name="writeLine">Indicates whether to append a newline after writing the badge.</param>
        public static void Render(string label, BadgeKind kind = BadgeKind.Default, BadgeOptions options = null, bool writeLine = true)
        {
            new BadgeEntry(label, kind, options).Render(writeLine);
        }

        /// <summary>
        /// Creates a plain text badge without writing to the console.
        /// </summary>
        /// <param name="label">The badge label.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional rendering options for the badge.</param>
        /// <param name="maxWidth">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded single-line badge string.</returns>
        public static string Text(string label, BadgeKind kind = BadgeKind.Default, BadgeOptions options = null, int? maxWidth = null)
        {
            return new BadgeEntry(label, kind, options).ToText(maxWidth);
        }

        private int ResolveWidth(int? maxWidth)
        {
            int terminalWidth = Math.Max(1, Consoul.ConsoleBufferWidth - 1);
            int requested = maxWidth ?? _options.MaxWidth ?? terminalWidth;
            return Math.Max(1, Math.Min(requested, terminalWidth));
        }

        private static IList<FixedMessageSegment> BuildBadgeSegments(
            string leftEdge,
            string leftPadding,
            IList<FixedMessageSegment> label,
            string rightPadding,
            string rightEdge,
            ColorScheme scheme)
        {
            var segments = new List<FixedMessageSegment>();
            AddPlainSegment(segments, leftEdge, scheme);
            AddPlainSegment(segments, leftPadding, scheme);
            AddSegments(segments, label);
            AddPlainSegment(segments, rightPadding, scheme);
            AddPlainSegment(segments, rightEdge, scheme);
            return segments;
        }

        private static IList<FixedMessageSegment> NormalizeLabelSegments(string label, bool uppercase, ColorScheme scheme)
        {
            label = (label ?? string.Empty)
                .Replace("\r\n", " ")
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Trim();

            List<FixedMessageSegment> segments = MarkupRenderText.ParseSegments(label, scheme);
            if (!uppercase)
            {
                return segments;
            }

            return segments
                .Select(segment => new FixedMessageSegment(
                    (segment.Text ?? string.Empty).ToUpper(CultureInfo.CurrentCulture),
                    segment.Scheme,
                    segment.Decoration,
                    segment.Link,
                    segment.ForegroundRgb,
                    segment.BackgroundRgb))
                .ToList();
        }

        private static IList<FixedMessageSegment> TrimWithEllipsis(
            IList<FixedMessageSegment> value,
            int width,
            string ellipsis,
            ColorScheme ellipsisScheme)
        {
            width = Math.Max(0, width);
            if (MarkupRenderText.GetLength(value) <= width)
            {
                return value;
            }

            ellipsis = ellipsis ?? string.Empty;
            if (ellipsis.Length == 0 || width <= ellipsis.Length)
            {
                return MarkupRenderText.TruncateSegments(value, width);
            }

            var segments = MarkupRenderText.TruncateSegments(value, width - ellipsis.Length);
            AddPlainSegment(segments, ellipsis, ellipsisScheme);
            return segments;
        }

        private static void AddSegments(IList<FixedMessageSegment> segments, IEnumerable<FixedMessageSegment> values)
        {
            foreach (FixedMessageSegment segment in values ?? Enumerable.Empty<FixedMessageSegment>())
            {
                AddSegment(
                    segments,
                    segment.Text,
                    segment.Scheme ?? RenderOptions.DefaultScheme,
                    segment.Decoration,
                    segment.Link,
                    segment.ForegroundRgb,
                    segment.BackgroundRgb);
            }
        }

        private static void AddPlainSegment(IList<FixedMessageSegment> segments, string text, ColorScheme scheme)
        {
            AddSegment(segments, text, scheme);
        }

        private static void AddSegment(IList<FixedMessageSegment> segments, string text, ColorScheme scheme)
        {
            AddSegment(segments, text, scheme, Decoration.None, null, null, null);
        }

        private static void AddSegment(
            IList<FixedMessageSegment> segments,
            string text,
            ColorScheme scheme,
            Decoration decoration,
            Link link,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            text = text ?? string.Empty;
            if (text.Length == 0)
            {
                return;
            }

            if (segments.Count > 0 &&
                SameScheme(segments[segments.Count - 1].Scheme, scheme) &&
                segments[segments.Count - 1].Decoration == decoration &&
                segments[segments.Count - 1].ForegroundRgb.Equals(foregroundRgb) &&
                segments[segments.Count - 1].BackgroundRgb.Equals(backgroundRgb) &&
                TerminalLinkWriter.SameTarget(segments[segments.Count - 1].Link, link))
            {
                FixedMessageSegment previous = segments[segments.Count - 1];
                segments[segments.Count - 1] = new FixedMessageSegment(
                    previous.Text + text,
                    scheme,
                    decoration,
                    link,
                    foregroundRgb,
                    backgroundRgb);
                return;
            }

            segments.Add(new FixedMessageSegment(text, scheme, decoration, link, foregroundRgb, backgroundRgb));
        }

        private static bool SameScheme(ColorScheme? left, ColorScheme right)
        {
            return left.HasValue &&
                   left.Value.Color == right.Color &&
                   left.Value.BackgroundColor == right.BackgroundColor;
        }
    }
}
