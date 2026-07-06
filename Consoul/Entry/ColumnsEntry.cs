using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders responsive side-by-side text columns for dashboard-style console views.
    /// </summary>
    public class ColumnsEntry : IRenderable
    {
        private readonly List<GridCell> _columns = new List<GridCell>();
        private readonly ColumnsOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnsEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the columns.</param>
        public ColumnsEntry(ColumnsOptions options = null)
        {
            _options = options != null ? options.Clone() : new ColumnsOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnsEntry"/> class with text columns.
        /// </summary>
        /// <param name="columns">The column contents to render.</param>
        /// <param name="options">Optional rendering options for the columns.</param>
        public ColumnsEntry(IEnumerable<string> columns, ColumnsOptions options = null) : this(options)
        {
            if (columns == null)
            {
                return;
            }

            foreach (string column in columns)
            {
                Add(column);
            }
        }

        /// <summary>
        /// Gets the columns rendered by the layout.
        /// </summary>
        public IList<GridCell> Columns => _columns;

        /// <summary>
        /// Gets the options used to render the columns.
        /// </summary>
        public ColumnsOptions Options => _options;

        /// <summary>
        /// Adds a text column to the layout.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="color">Optional foreground color used for the column content.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(string content, string header = null, ConsoleColor? color = null)
        {
            var column = GridCell.Create(content, header, color);
            _columns.Add(column);
            return column;
        }

        /// <summary>
        /// Adds a column, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="color">Optional foreground color used for the column content.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(object content, string header = null, ConsoleColor? color = null)
        {
            var column = content as GridCell;
            if (column != null)
            {
                return Add(column);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, header, color);
            }

            return Add(RenderActionContent.FormatValue(content), header, color);
        }

        /// <summary>
        /// Adds a text column to the layout using Spectre-style style text.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the column content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(string content, string header, string style)
        {
            var column = GridCell.Create(content, header, style);
            _columns.Add(column);
            return column;
        }

        /// <summary>
        /// Adds a column using Spectre-style style text, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the column content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(object content, string header, string style)
        {
            var column = content as GridCell;
            if (column != null)
            {
                return Add(column);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, header, style);
            }

            return Add(RenderActionContent.FormatValue(content), header, style);
        }

        /// <summary>
        /// Adds a text column to the layout using a reusable style.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the column content.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(string content, string header, Style style)
        {
            var column = GridCell.Create(content, header, style);
            _columns.Add(column);
            return column;
        }

        /// <summary>
        /// Adds a column using a reusable style, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the column content.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(object content, string header, Style style)
        {
            var column = content as GridCell;
            if (column != null)
            {
                return Add(column);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, header, style);
            }

            return Add(RenderActionContent.FormatValue(content), header, style);
        }

        /// <summary>
        /// Adds a column whose content is captured from a Consoul render action.
        /// </summary>
        /// <param name="render">The render action used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="color">Optional foreground color used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(Action render, string header = null, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureColumn(render, captureWidth, captureHeight), header, color);
        }

        /// <summary>
        /// Adds a column whose content is captured from a Consoul render action using Spectre-style style text.
        /// </summary>
        /// <param name="render">The render action used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the captured content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(Action render, string header, string style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureColumn(render, captureWidth, captureHeight), header, style);
        }

        /// <summary>
        /// Adds a column whose content is captured from a Consoul render action using a reusable style.
        /// </summary>
        /// <param name="render">The render action used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(Action render, string header, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureColumn(render, captureWidth, captureHeight), header, style);
        }

        /// <summary>
        /// Adds a column whose content is captured from a Consoul renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="color">Optional foreground color used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(IConsoulRenderable renderable, string header = null, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureColumn(renderable, captureWidth, captureHeight), header, color);
        }

        /// <summary>
        /// Adds a column whose content is captured from a Consoul renderable component using Spectre-style style text.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the captured content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(IConsoulRenderable renderable, string header, string style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureColumn(renderable, captureWidth, captureHeight), header, style);
        }

        /// <summary>
        /// Adds a column whose content is captured from a Consoul renderable component using a reusable style.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(IConsoulRenderable renderable, string header, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureColumn(renderable, captureWidth, captureHeight), header, style);
        }

        /// <summary>
        /// Adds a configured column cell to the layout.
        /// </summary>
        /// <param name="column">The configured column cell to add.</param>
        /// <returns>The added column cell.</returns>
        public GridCell Add(GridCell column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            _columns.Add(column);
            return column;
        }

        /// <summary>
        /// Renders the columns to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_columns.Count == 0)
            {
                return;
            }

            var grid = new GridEntry(_options.ToGridOptions());
            foreach (GridCell column in _columns)
            {
                grid.Add(column);
            }

            grid.Render();
        }

        /// <summary>
        /// Renders temporary columns to the active console driver.
        /// </summary>
        /// <param name="columns">The column contents to render.</param>
        /// <param name="options">Optional rendering options for the columns.</param>
        public static void Render(IEnumerable<string> columns, ColumnsOptions options = null)
        {
            new ColumnsEntry(columns, options).Render();
        }

        private string CaptureColumn(Action render, int? captureWidth, int captureHeight)
        {
            return NormalizeCapturedColumn(RenderActionContent.Capture(render, captureWidth ?? _options.Width, captureHeight));
        }

        private string CaptureColumn(IConsoulRenderable renderable, int? captureWidth, int captureHeight)
        {
            return NormalizeCapturedColumn(RenderActionContent.Capture(renderable, captureWidth ?? _options.Width, captureHeight));
        }

        private static string NormalizeCapturedColumn(string content)
        {
            string normalized = (content ?? string.Empty).TrimEnd('\r', '\n').Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = normalized.Split('\n');
            for (int index = 0; index < lines.Length; index++)
            {
                lines[index] = lines[index].TrimEnd(' ');
            }

            return string.Join("\n", lines);
        }
    }
}
