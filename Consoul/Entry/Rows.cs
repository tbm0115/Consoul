using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent row composition over <see cref="RowsEntry"/>.
    /// </summary>
    public class Rows : RowsEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rows"/> class.
        /// </summary>
        public Rows() : this((RowsOptions)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rows"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the rows.</param>
        public Rows(RowsOptions options) : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rows"/> class with text rows.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        public Rows(params string[] rows) : base(rows)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rows"/> class with text rows.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        public Rows(IEnumerable<string> rows) : base(rows)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rows"/> class with renderable rows.
        /// </summary>
        /// <param name="rows">The renderable components to capture as rows.</param>
        public Rows(params IConsoulRenderable[] rows) : this()
        {
            AddRows(rows);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rows"/> class with renderable rows.
        /// </summary>
        /// <param name="rows">The renderable components to capture as rows.</param>
        public Rows(IEnumerable<IConsoulRenderable> rows) : this()
        {
            AddRows(rows);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rows"/> class with mixed plain values and renderable rows.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        public Rows(params object[] rows) : this()
        {
            AddRows(rows);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rows"/> class with mixed plain values and renderable rows.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        public Rows(IEnumerable<object> rows) : this()
        {
            AddRows(rows);
        }

        /// <summary>
        /// Adds a text row.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="color">Optional foreground color used for the row content.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(string content, ConsoleColor? color = null)
        {
            Add(content, color);
            return this;
        }

        /// <summary>
        /// Adds a row, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="color">Optional foreground color used for the row content.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(object content, ConsoleColor? color = null)
        {
            Add(content, color);
            return this;
        }

        /// <summary>
        /// Adds a text row using Spectre-style style text.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="style">Optional style used for the row content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(string content, string style)
        {
            Add(content, style);
            return this;
        }

        /// <summary>
        /// Adds a row using Spectre-style style text, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="style">Optional style used for the row content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(object content, string style)
        {
            Add(content, style);
            return this;
        }

        /// <summary>
        /// Adds a text row using a reusable style.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="style">Optional style used for the row content.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(string content, Style style)
        {
            Add(content, style);
            return this;
        }

        /// <summary>
        /// Adds a row using a reusable style, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="style">Optional style used for the row content.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(object content, Style style)
        {
            Add(content, style);
            return this;
        }

        /// <summary>
        /// Adds a row whose content is captured from a render action.
        /// </summary>
        /// <param name="render">The render action used to produce row content.</param>
        /// <param name="color">Optional foreground color used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(Action render, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            Add(render, color, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a row whose content is captured from a render action using Spectre-style style text.
        /// </summary>
        /// <param name="render">The render action used to produce row content.</param>
        /// <param name="style">Optional style used for the captured content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(Action render, string style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(render, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a row whose content is captured from a render action using a reusable style.
        /// </summary>
        /// <param name="render">The render action used to produce row content.</param>
        /// <param name="style">Optional style used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(Action render, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(render, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a row whose content is captured from a renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce row content.</param>
        /// <param name="color">Optional foreground color used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(IConsoulRenderable renderable, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, color, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a row whose content is captured from a renderable component using Spectre-style style text.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce row content.</param>
        /// <param name="style">Optional style used for the captured content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(IConsoulRenderable renderable, string style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a row whose content is captured from a renderable component using a reusable style.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce row content.</param>
        /// <param name="style">Optional style used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRow(IConsoulRenderable renderable, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds multiple text rows.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRows(IEnumerable<string> rows)
        {
            if (rows == null)
            {
                return this;
            }

            foreach (string row in rows)
            {
                AddRow(row);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple text rows.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRows(params string[] rows)
        {
            return AddRows((IEnumerable<string>)rows);
        }

        /// <summary>
        /// Adds multiple renderable rows.
        /// </summary>
        /// <param name="rows">The renderable components to capture as rows.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRows(IEnumerable<IConsoulRenderable> rows)
        {
            if (rows == null)
            {
                return this;
            }

            foreach (IConsoulRenderable row in rows)
            {
                AddRow(row);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple renderable rows.
        /// </summary>
        /// <param name="rows">The renderable components to capture as rows.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRows(params IConsoulRenderable[] rows)
        {
            return AddRows((IEnumerable<IConsoulRenderable>)rows);
        }

        /// <summary>
        /// Adds multiple rows from mixed plain values and reusable renderable components.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRows(IEnumerable<object> rows)
        {
            if (rows == null)
            {
                return this;
            }

            foreach (object row in rows)
            {
                AddRow(row);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple rows from mixed plain values and reusable renderable components.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRows(params object[] rows)
        {
            return AddRows((IEnumerable<object>)rows);
        }

        /// <summary>
        /// Adds rows projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into rows.</param>
        /// <param name="rowSelector">Selects the row content rendered for each model item.</param>
        /// <param name="colorSelector">Optionally selects the foreground color used for each projected row.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRows<T>(
            IEnumerable<T> items,
            Func<T, object> rowSelector,
            Func<T, ConsoleColor?> colorSelector = null)
        {
            if (rowSelector == null)
            {
                throw new ArgumentNullException(nameof(rowSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddRow(rowSelector(item), colorSelector != null ? colorSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds rows projected from a model collection with Spectre-style per-row colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into rows.</param>
        /// <param name="rowSelector">Selects the row content rendered for each model item.</param>
        /// <param name="colorSelector">Selects the foreground color used for each projected row.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRows<T>(
            IEnumerable<T> items,
            Func<T, object> rowSelector,
            Func<T, SpectreColor> colorSelector)
        {
            if (rowSelector == null)
            {
                throw new ArgumentNullException(nameof(rowSelector));
            }

            if (colorSelector == null)
            {
                throw new ArgumentNullException(nameof(colorSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddRow(rowSelector(item), new Style(colorSelector(item)));
            }

            return this;
        }

        /// <summary>
        /// Adds rows projected from a model collection with RGB per-row colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into rows.</param>
        /// <param name="rowSelector">Selects the row content rendered for each model item.</param>
        /// <param name="colorSelector">Selects the foreground color used for each projected row.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRows<T>(
            IEnumerable<T> items,
            Func<T, object> rowSelector,
            Func<T, RgbColor> colorSelector)
        {
            if (rowSelector == null)
            {
                throw new ArgumentNullException(nameof(rowSelector));
            }

            if (colorSelector == null)
            {
                throw new ArgumentNullException(nameof(colorSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddRow(rowSelector(item), new Style(colorSelector(item)));
            }

            return this;
        }

        /// <summary>
        /// Adds rows projected from a model collection with per-row colors parsed from Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into rows.</param>
        /// <param name="rowSelector">Selects the row content rendered for each model item.</param>
        /// <param name="styleSelector">Selects style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows AddRows<T>(
            IEnumerable<T> items,
            Func<T, object> rowSelector,
            Func<T, string> styleSelector)
        {
            if (rowSelector == null)
            {
                throw new ArgumentNullException(nameof(rowSelector));
            }

            if (styleSelector == null)
            {
                throw new ArgumentNullException(nameof(styleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddRow(rowSelector(item), styleSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Sets the requested row width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands rows to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows Expand()
        {
            Options.Width = null;
            Options.Expand = true;
            return this;
        }

        /// <summary>
        /// Collapses rows to the widest rendered row when no explicit width is set.
        /// </summary>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows Collapse()
        {
            Options.Width = null;
            Options.Expand = false;
            return this;
        }

        /// <summary>
        /// Sets the number of blank lines between rows.
        /// </summary>
        /// <param name="spacing">The number of blank lines between rows.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows Spacing(int spacing)
        {
            Options.Spacing = spacing;
            return this;
        }

        /// <summary>
        /// Sets the default row foreground color.
        /// </summary>
        /// <param name="color">The default row foreground color.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows ContentColor(ConsoleColor color)
        {
            Options.ContentScheme = Scheme(color);
            Options.ContentDecoration = Decoration.None;
            Options.ContentForegroundRgb = null;
            Options.ContentBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default row color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows ContentColor(Style style)
        {
            ApplyContentStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the default row color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Rows"/> instance.</returns>
        public Rows ContentColor(string style)
        {
            ApplyContentStyle(Style.Parse(style));
            return this;
        }

        private void ApplyContentStyle(Style style)
        {
            Options.ContentScheme = Scheme(style, Options.ResolveContentScheme());
            Options.ContentDecoration = style?.Decoration ?? Decoration.None;
            Options.ContentForegroundRgb = style?.ForegroundRgb;
            Options.ContentBackgroundRgb = style?.BackgroundRgb;
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme Scheme(Style style, ColorScheme fallback)
        {
            return (style ?? Style.Plain).ToColorScheme(fallback);
        }
    }
}
