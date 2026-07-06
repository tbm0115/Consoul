using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders text or captured render output aligned inside a responsive width.
    /// </summary>
    public class AlignEntry : IRenderable
    {
        private readonly AlignOptions _options;
        private readonly bool _parseMarkup;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlignEntry"/> class.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="options">Optional rendering options for the aligned content.</param>
        public AlignEntry(string content, AlignOptions options = null) : this(content, options, true)
        {
        }

        private AlignEntry(string content, AlignOptions options, bool parseMarkup)
        {
            Content = content ?? string.Empty;
            _options = options != null ? options.Clone() : new AlignOptions();
            _parseMarkup = parseMarkup;
        }

        /// <summary>
        /// Gets or sets the text content rendered by the aligned block.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets the options used to render the aligned block.
        /// </summary>
        public AlignOptions Options => _options;

        /// <summary>
        /// Renders the aligned content to the active console driver.
        /// </summary>
        public void Render()
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            ColorScheme scheme = _options.ResolveScheme();
            List<List<FixedMessageSegment>> lines = GetLines(scheme);
            int height = ResolveHeight(lines.Count);
            int visibleCount = Math.Min(lines.Count, height);
            int blankCount = Math.Max(0, height - visibleCount);
            int topPadding = ResolveTopPadding(blankCount, _options.VerticalAlignment);

            WriteBlankLines(topPadding, width, scheme);

            for (int index = 0; index < visibleCount; index++)
            {
                MarkupRenderText.WriteSegments(
                    MarkupRenderText.AlignSegments(lines[index], width, _options.Alignment, scheme),
                    scheme,
                    true);
            }

            WriteBlankLines(blankCount - topPadding, width, scheme);
        }

        /// <summary>
        /// Renders temporary aligned content to the active console driver.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="options">Optional rendering options for the aligned content.</param>
        public static void Render(string content, AlignOptions options = null)
        {
            new AlignEntry(content, options).Render();
        }

        /// <summary>
        /// Captures a render action and renders the captured text aligned inside the requested width.
        /// </summary>
        /// <param name="render">The render action used to produce aligned content.</param>
        /// <param name="options">Optional rendering options for the aligned content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Render(Action render, AlignOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            AlignOptions resolvedOptions = options != null ? options.Clone() : new AlignOptions();
            string content = RenderActionContent.Capture(render, captureWidth ?? resolvedOptions.Width, captureHeight);
            new AlignEntry(content, resolvedOptions, false).Render();
        }

        /// <summary>
        /// Captures a renderable component and renders the captured text aligned inside the requested width.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce aligned content.</param>
        /// <param name="options">Optional rendering options for the aligned content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Render(IConsoulRenderable renderable, AlignOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            AlignOptions resolvedOptions = options != null ? options.Clone() : new AlignOptions();
            string content = RenderActionContent.Capture(renderable, captureWidth ?? resolvedOptions.Width, captureHeight);
            new AlignEntry(content, resolvedOptions, false).Render();
        }

        private List<List<FixedMessageSegment>> GetLines(ColorScheme scheme)
        {
            if (_parseMarkup)
            {
                return MarkupRenderText.GetLogicalLines(Content, scheme);
            }

            var rendered = new List<List<FixedMessageSegment>>();
            string content = Content;
            content = (content ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = content.Split('\n');
            if (lines.Length == 0)
            {
                rendered.Add(new List<FixedMessageSegment>());
                return rendered;
            }

            foreach (string line in lines)
            {
                rendered.Add(new List<FixedMessageSegment> { new FixedMessageSegment(line, scheme) });
            }

            return rendered;
        }

        private int ResolveHeight(int naturalHeight)
        {
            if (!_options.Height.HasValue)
            {
                return Math.Max(1, naturalHeight);
            }

            int requested = Math.Max(1, _options.Height.Value);
            return Math.Max(1, Math.Min(requested, Consoul.ConsoleBufferHeight));
        }

        private static int ResolveTopPadding(int blankCount, VerticalAlignment alignment)
        {
            switch (alignment)
            {
                case VerticalAlignment.Middle:
                    return blankCount / 2;
                case VerticalAlignment.Bottom:
                    return blankCount;
                default:
                    return 0;
            }
        }

        private static void WriteBlankLines(int count, int width, ColorScheme scheme)
        {
            for (int index = 0; index < count; index++)
            {
                Consoul.Write(new string(' ', width), scheme, true);
            }
        }

    }
}
