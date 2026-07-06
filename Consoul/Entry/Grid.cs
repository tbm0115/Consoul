using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent grid composition over <see cref="GridEntry"/>.
    /// </summary>
    public class Grid : GridEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class.
        /// </summary>
        public Grid() : this((GridOptions)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the grid.</param>
        public Grid(GridOptions options) : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class with text cells.
        /// </summary>
        /// <param name="cells">The cell contents to render.</param>
        public Grid(params string[] cells) : base(cells)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class with mixed plain values and renderable cells.
        /// </summary>
        /// <param name="cells">The cell values to render.</param>
        public Grid(params object[] cells) : base(cells)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class with mixed plain values and renderable cells.
        /// </summary>
        /// <param name="cells">The cell values to render.</param>
        public Grid(IEnumerable<object> cells) : base(cells)
        {
        }

        /// <summary>
        /// Adds one column to the preferred grid column count.
        /// </summary>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddColumn()
        {
            return AddColumn(new GridColumn());
        }

        /// <summary>
        /// Adds a configured grid column.
        /// </summary>
        /// <param name="column">The column configuration.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddColumn(GridColumn column)
        {
            AddColumnDefinition(column);
            return this;
        }

        /// <summary>
        /// Adds multiple columns to the preferred grid column count.
        /// </summary>
        /// <param name="count">The number of columns to add.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddColumns(int count)
        {
            for (int index = 0; index < Math.Max(0, count); index++)
            {
                AddColumn();
            }

            return this;
        }

        /// <summary>
        /// Adds multiple configured grid columns.
        /// </summary>
        /// <param name="columns">The column configurations.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddColumns(params GridColumn[] columns)
        {
            if (columns == null)
            {
                return this;
            }

            foreach (GridColumn column in columns)
            {
                AddColumn(column);
            }

            return this;
        }

        /// <summary>
        /// Adds a text cell.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="color">Optional foreground color used for the content.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCell(string content, string header = null, ConsoleColor? color = null)
        {
            Add(content, header, color);
            return this;
        }

        /// <summary>
        /// Adds a cell, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="color">Optional foreground color used for the content.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCell(object content, string header = null, ConsoleColor? color = null)
        {
            Add(content, header, color);
            return this;
        }

        /// <summary>
        /// Adds a text cell using Spectre-style style text.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCell(string content, string header, string style)
        {
            Add(content, header, style);
            return this;
        }

        /// <summary>
        /// Adds a cell using Spectre-style style text, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCell(object content, string header, string style)
        {
            Add(content, header, style);
            return this;
        }

        /// <summary>
        /// Adds a text cell using a reusable style.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the content.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCell(string content, string header, Style style)
        {
            Add(content, header, style);
            return this;
        }

        /// <summary>
        /// Adds a cell using a reusable style, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the content.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCell(object content, string header, Style style)
        {
            Add(content, header, style);
            return this;
        }

        /// <summary>
        /// Adds a cell whose content is captured from a renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="color">Optional foreground color used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCell(IConsoulRenderable renderable, string header = null, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, header, color, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a cell whose content is captured from a renderable component using Spectre-style style text.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the captured content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCell(IConsoulRenderable renderable, string header, string style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, header, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a cell whose content is captured from a renderable component using a reusable style.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCell(IConsoulRenderable renderable, string header, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, header, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a row of text cells. Rows flow according to the configured preferred column count.
        /// </summary>
        /// <param name="cells">The row cell contents.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddRow(params string[] cells)
        {
            if (cells == null)
            {
                return this;
            }

            foreach (string cell in cells)
            {
                AddCell(cell);
            }

            return this;
        }

        /// <summary>
        /// Adds a row whose cells are captured from renderable components.
        /// </summary>
        /// <param name="cells">Renderable cell contents.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddRow(params IConsoulRenderable[] cells)
        {
            if (cells == null)
            {
                return this;
            }

            foreach (IConsoulRenderable cell in cells)
            {
                AddCell(cell);
            }

            return this;
        }

        /// <summary>
        /// Adds a row whose cells may mix plain values and reusable renderable components.
        /// </summary>
        /// <param name="cells">The row cell values.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddRow(params object[] cells)
        {
            if (cells == null)
            {
                return this;
            }

            foreach (object cell in cells)
            {
                AddCell(cell);
            }

            return this;
        }

        /// <summary>
        /// Adds cells projected from a model collection. Cells flow according to the configured preferred column count.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into cells.</param>
        /// <param name="contentSelector">Selects the cell content rendered for each model item.</param>
        /// <param name="headerSelector">Optionally selects the header rendered above each projected cell.</param>
        /// <param name="colorSelector">Optionally selects the foreground color used for each projected cell.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCells<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> headerSelector = null,
            Func<T, ConsoleColor?> colorSelector = null)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddCell(
                    contentSelector(item),
                    headerSelector != null ? headerSelector(item) : null,
                    colorSelector != null ? colorSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds cells projected from a model collection with Spectre-style per-cell colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into cells.</param>
        /// <param name="contentSelector">Selects the cell content rendered for each model item.</param>
        /// <param name="headerSelector">Optionally selects the header rendered above each projected cell.</param>
        /// <param name="colorSelector">Selects the foreground color used for each projected cell.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCells<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> headerSelector,
            Func<T, SpectreColor> colorSelector)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
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
                AddCell(
                    contentSelector(item),
                    headerSelector != null ? headerSelector(item) : null,
                    colorSelector(item).ToConsoleColor());
            }

            return this;
        }

        /// <summary>
        /// Adds cells projected from a model collection with RGB per-cell colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into cells.</param>
        /// <param name="contentSelector">Selects the cell content rendered for each model item.</param>
        /// <param name="headerSelector">Optionally selects the header rendered above each projected cell.</param>
        /// <param name="colorSelector">Selects the foreground color used for each projected cell.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCells<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> headerSelector,
            Func<T, RgbColor> colorSelector)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
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
                AddCell(
                    contentSelector(item),
                    headerSelector != null ? headerSelector(item) : null,
                    colorSelector(item).ToConsoleColor());
            }

            return this;
        }

        /// <summary>
        /// Adds cells projected from a model collection with per-cell colors parsed from Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into cells.</param>
        /// <param name="contentSelector">Selects the cell content rendered for each model item.</param>
        /// <param name="headerSelector">Optionally selects the header rendered above each projected cell.</param>
        /// <param name="styleSelector">Selects style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddCells<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> headerSelector,
            Func<T, string> styleSelector)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
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
                AddCell(
                    contentSelector(item),
                    headerSelector != null ? headerSelector(item) : null,
                    styleSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds an empty row using the configured column count.
        /// </summary>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddEmptyRow()
        {
            int columnCount = Columns.Count > 0
                ? Columns.Count
                : Math.Max(0, Options.Columns.GetValueOrDefault());
            if (columnCount <= 0)
            {
                return this;
            }

            return AddRow(new string[columnCount]);
        }

        /// <summary>
        /// Adds multiple rows of text cells.
        /// </summary>
        /// <param name="rows">The row cell contents.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid AddRows(IEnumerable<IEnumerable<string>> rows)
        {
            if (rows == null)
            {
                return this;
            }

            foreach (IEnumerable<string> row in rows)
            {
                AddRow(row == null ? null : new List<string>(row).ToArray());
            }

            return this;
        }

        /// <summary>
        /// Sets the requested grid width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the grid to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid Expand()
        {
            Options.Width = null;
            Options.Expand = true;
            return this;
        }

        /// <summary>
        /// Collapses the grid width to its measured content when no explicit width is set.
        /// </summary>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid Collapse()
        {
            Options.Width = null;
            Options.Expand = false;
            return this;
        }

        /// <summary>
        /// Sets the preferred number of grid columns.
        /// </summary>
        /// <param name="columns">The preferred number of columns.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid ColumnCount(int? columns)
        {
            Options.Columns = columns;
            return this;
        }

        /// <summary>
        /// Sets the minimum column width before the grid stacks.
        /// </summary>
        /// <param name="width">The minimum column width.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid MinimumColumnWidth(int width)
        {
            Options.MinimumColumnWidth = width;
            return this;
        }

        /// <summary>
        /// Sets the number of spaces between columns.
        /// </summary>
        /// <param name="gutter">The number of spaces between columns.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid Gutter(int gutter)
        {
            Options.Gutter = gutter;
            return this;
        }

        /// <summary>
        /// Sets the header foreground color.
        /// </summary>
        /// <param name="color">The header foreground color.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid HeaderColor(ConsoleColor color)
        {
            Options.HeaderScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the header color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid HeaderColor(Style style)
        {
            Options.HeaderScheme = Scheme(style, Options.ResolveHeaderScheme());
            return this;
        }

        /// <summary>
        /// Sets the header color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid HeaderColor(string style)
        {
            Options.HeaderScheme = Style.ParseScheme(style, Options.ResolveHeaderScheme());
            return this;
        }

        /// <summary>
        /// Sets the default content foreground color.
        /// </summary>
        /// <param name="color">The default content foreground color.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid ContentColor(ConsoleColor color)
        {
            Options.ContentScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the default content color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid ContentColor(Style style)
        {
            Options.ContentScheme = Scheme(style, Options.ResolveContentScheme());
            return this;
        }

        /// <summary>
        /// Sets the default content color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Grid"/> instance.</returns>
        public Grid ContentColor(string style)
        {
            Options.ContentScheme = Style.ParseScheme(style, Options.ResolveContentScheme());
            return this;
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
