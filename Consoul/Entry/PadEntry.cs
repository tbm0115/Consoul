using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders text or captured render output with responsive padding.
    /// </summary>
    public class PadEntry : IRenderable
    {
        private readonly PadOptions _options;
        private readonly bool _parseMarkup;

        /// <summary>
        /// Initializes a new instance of the <see cref="PadEntry"/> class.
        /// </summary>
        /// <param name="content">The text content to pad.</param>
        /// <param name="options">Optional rendering options for the padded content.</param>
        public PadEntry(string content, PadOptions options = null) : this(content, options, true)
        {
        }

        private PadEntry(string content, PadOptions options, bool parseMarkup)
        {
            Content = content ?? string.Empty;
            _options = options != null ? options.Clone() : new PadOptions();
            _parseMarkup = parseMarkup;
        }

        /// <summary>
        /// Gets or sets the text content rendered by the padded block.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets the options used to render the padded block.
        /// </summary>
        public PadOptions Options => _options;

        /// <summary>
        /// Renders the padded content to the active console driver.
        /// </summary>
        public void Render()
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            int left = _options.ResolveLeft();
            int right = _options.ResolveRight();

            if (left + right >= width)
            {
                left = Math.Min(left, Math.Max(0, width - 1));
                right = Math.Max(0, width - left - 1);
            }

            int innerWidth = Math.Max(1, width - left - right);
            ColorScheme scheme = _options.ResolveScheme();

            WriteBlankLines(_options.ResolveTop(), width, scheme);

            foreach (List<FixedMessageSegment> line in BuildContentLines(innerWidth, scheme))
            {
                List<FixedMessageSegment> inner = PadOrTrimSegments(line, innerWidth, scheme);
                List<FixedMessageSegment> padded = MarkupRenderText.PadSegments(inner, left, right, scheme);
                MarkupRenderText.WriteSegments(PadOrTrimSegments(padded, width, scheme), scheme, true);
            }

            WriteBlankLines(_options.ResolveBottom(), width, scheme);
        }

        /// <summary>
        /// Renders temporary padded content to the active console driver.
        /// </summary>
        /// <param name="content">The text content to pad.</param>
        /// <param name="options">Optional rendering options for the padded content.</param>
        public static void Render(string content, PadOptions options = null)
        {
            new PadEntry(content, options).Render();
        }

        /// <summary>
        /// Captures a render action and renders the captured text with padding.
        /// </summary>
        /// <param name="render">The render action used to produce padded content.</param>
        /// <param name="options">Optional rendering options for the padded content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Render(Action render, PadOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            PadOptions resolvedOptions = options != null ? options.Clone() : new PadOptions();
            string content = RenderActionContent.Capture(render, captureWidth ?? resolvedOptions.Width, captureHeight);
            new PadEntry(content, resolvedOptions, false).Render();
        }

        /// <summary>
        /// Captures a renderable component and renders the captured text with padding.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce padded content.</param>
        /// <param name="options">Optional rendering options for the padded content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Render(IConsoulRenderable renderable, PadOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            PadOptions resolvedOptions = options != null ? options.Clone() : new PadOptions();
            string content = RenderActionContent.Capture(renderable, captureWidth ?? resolvedOptions.Width, captureHeight);
            new PadEntry(content, resolvedOptions, false).Render();
        }

        private static void WriteBlankLines(int count, int width, ColorScheme scheme)
        {
            for (int index = 0; index < count; index++)
            {
                Consoul.Write(new string(' ', width), scheme, true);
            }
        }

        private IEnumerable<List<FixedMessageSegment>> BuildContentLines(int innerWidth, ColorScheme scheme)
        {
            if (_parseMarkup)
            {
                return MarkupRenderText.WrapSegments(Content, innerWidth, scheme);
            }

            var lines = new List<List<FixedMessageSegment>>();
            foreach (string line in PanelEntry.WrapLines(Content, innerWidth))
            {
                lines.Add(new List<FixedMessageSegment> { new FixedMessageSegment(line, scheme) });
            }

            if (lines.Count == 0)
            {
                lines.Add(new List<FixedMessageSegment>());
            }

            return lines;
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(IList<FixedMessageSegment> segments, int width, ColorScheme scheme)
        {
            List<FixedMessageSegment> result = MarkupRenderText.TruncateSegments(segments, Math.Max(0, width));
            MarkupRenderText.AddPadding(result, Math.Max(0, width - MarkupRenderText.GetLength(result)), scheme);
            return result;
        }
    }
}
