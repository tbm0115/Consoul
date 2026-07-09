using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent column composition over <see cref="ColumnsEntry"/>.
    /// </summary>
    public class Columns : ColumnsEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Columns"/> class.
        /// </summary>
        public Columns() : this((ColumnsOptions)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Columns"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the columns.</param>
        public Columns(ColumnsOptions options) : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Columns"/> class with text columns.
        /// </summary>
        /// <param name="columns">The column contents to render.</param>
        public Columns(params string[] columns) : base(columns)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Columns"/> class with text columns.
        /// </summary>
        /// <param name="columns">The column contents to render.</param>
        public Columns(IEnumerable<string> columns) : base(columns)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Columns"/> class with renderable columns.
        /// </summary>
        /// <param name="columns">The renderable components to capture as columns.</param>
        public Columns(params IConsoulRenderable[] columns) : this()
        {
            AddColumns(columns);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Columns"/> class with renderable columns.
        /// </summary>
        /// <param name="columns">The renderable components to capture as columns.</param>
        public Columns(IEnumerable<IConsoulRenderable> columns) : this()
        {
            AddColumns(columns);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Columns"/> class with mixed plain values and renderable columns.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        public Columns(params object[] columns) : this()
        {
            AddColumns(columns);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Columns"/> class with mixed plain values and renderable columns.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        public Columns(IEnumerable<object> columns) : this()
        {
            AddColumns(columns);
        }

        /// <summary>
        /// Adds a text column.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="color">Optional foreground color used for the column content.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(string content, string header = null, ConsoleColor? color = null)
        {
            Add(content, header, color);
            return this;
        }

        /// <summary>
        /// Adds a column, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="color">Optional foreground color used for the column content.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(object content, string header = null, ConsoleColor? color = null)
        {
            Add(content, header, color);
            return this;
        }

        /// <summary>
        /// Adds a text column using Spectre-style style text.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the column content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(string content, string header, string style)
        {
            Add(content, header, style);
            return this;
        }

        /// <summary>
        /// Adds a column using Spectre-style style text, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the column content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(object content, string header, string style)
        {
            Add(content, header, style);
            return this;
        }

        /// <summary>
        /// Adds a text column using a reusable style.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the column content.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(string content, string header, Style style)
        {
            Add(content, header, style);
            return this;
        }

        /// <summary>
        /// Adds a column using a reusable style, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the column content.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(object content, string header, Style style)
        {
            Add(content, header, style);
            return this;
        }

        /// <summary>
        /// Adds a column whose content is captured from a render action.
        /// </summary>
        /// <param name="render">The render action used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="color">Optional foreground color used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(Action render, string header = null, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            Add(render, header, color, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a column whose content is captured from a render action using Spectre-style style text.
        /// </summary>
        /// <param name="render">The render action used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the captured content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(Action render, string header, string style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(render, header, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a column whose content is captured from a render action using a reusable style.
        /// </summary>
        /// <param name="render">The render action used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(Action render, string header, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(render, header, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a column whose content is captured from a renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="color">Optional foreground color used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(IConsoulRenderable renderable, string header = null, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, header, color, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a column whose content is captured from a renderable component using Spectre-style style text.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the captured content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(IConsoulRenderable renderable, string header, string style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, header, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a column whose content is captured from a renderable component using a reusable style.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce column content.</param>
        /// <param name="header">Optional header rendered above the column content.</param>
        /// <param name="style">Optional style used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumn(IConsoulRenderable renderable, string header, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, header, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds multiple text columns.
        /// </summary>
        /// <param name="columns">The column contents to render.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumns(IEnumerable<string> columns)
        {
            if (columns == null)
            {
                return this;
            }

            foreach (string column in columns)
            {
                AddColumn(column);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple text columns.
        /// </summary>
        /// <param name="columns">The column contents to render.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumns(params string[] columns)
        {
            return AddColumns((IEnumerable<string>)columns);
        }

        /// <summary>
        /// Adds multiple renderable columns.
        /// </summary>
        /// <param name="columns">The renderable components to capture as columns.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumns(IEnumerable<IConsoulRenderable> columns)
        {
            if (columns == null)
            {
                return this;
            }

            foreach (IConsoulRenderable column in columns)
            {
                AddColumn(column);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple renderable columns.
        /// </summary>
        /// <param name="columns">The renderable components to capture as columns.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumns(params IConsoulRenderable[] columns)
        {
            return AddColumns((IEnumerable<IConsoulRenderable>)columns);
        }

        /// <summary>
        /// Adds multiple columns from mixed plain values and reusable renderable components.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumns(IEnumerable<object> columns)
        {
            if (columns == null)
            {
                return this;
            }

            foreach (object column in columns)
            {
                AddColumn(column);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple columns from mixed plain values and reusable renderable components.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumns(params object[] columns)
        {
            return AddColumns((IEnumerable<object>)columns);
        }

        /// <summary>
        /// Adds columns projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into columns.</param>
        /// <param name="contentSelector">Selects the column content rendered for each model item.</param>
        /// <param name="headerSelector">Optionally selects the header rendered above each projected column.</param>
        /// <param name="colorSelector">Optionally selects the foreground color used for each projected column.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumns<T>(
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
                AddColumn(
                    contentSelector(item),
                    headerSelector != null ? headerSelector(item) : null,
                    colorSelector != null ? colorSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds columns projected from a model collection with Spectre-style per-column colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into columns.</param>
        /// <param name="contentSelector">Selects the column content rendered for each model item.</param>
        /// <param name="headerSelector">Optionally selects the header rendered above each projected column.</param>
        /// <param name="colorSelector">Selects the foreground color used for each projected column.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumns<T>(
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
                AddColumn(
                    contentSelector(item),
                    headerSelector != null ? headerSelector(item) : null,
                    new Style(colorSelector(item)));
            }

            return this;
        }

        /// <summary>
        /// Adds columns projected from a model collection with RGB per-column colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into columns.</param>
        /// <param name="contentSelector">Selects the column content rendered for each model item.</param>
        /// <param name="headerSelector">Optionally selects the header rendered above each projected column.</param>
        /// <param name="colorSelector">Selects the foreground color used for each projected column.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumns<T>(
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
                AddColumn(
                    contentSelector(item),
                    headerSelector != null ? headerSelector(item) : null,
                    new Style(colorSelector(item)));
            }

            return this;
        }

        /// <summary>
        /// Adds columns projected from a model collection with per-column colors parsed from Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into columns.</param>
        /// <param name="contentSelector">Selects the column content rendered for each model item.</param>
        /// <param name="headerSelector">Optionally selects the header rendered above each projected column.</param>
        /// <param name="styleSelector">Selects style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns AddColumns<T>(
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
                AddColumn(
                    contentSelector(item),
                    headerSelector != null ? headerSelector(item) : null,
                    styleSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Sets the requested column layout width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands columns to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns Expand()
        {
            Options.Width = null;
            Options.Expand = true;
            return this;
        }

        /// <summary>
        /// Collapses columns to their measured content when no explicit width is set.
        /// </summary>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns Collapse()
        {
            Options.Width = null;
            Options.Expand = false;
            return this;
        }

        /// <summary>
        /// Sets the preferred number of columns.
        /// </summary>
        /// <param name="columns">The preferred number of columns.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns ColumnCount(int? columns)
        {
            Options.Columns = columns;
            return this;
        }

        /// <summary>
        /// Sets the minimum column width before the layout stacks.
        /// </summary>
        /// <param name="width">The minimum column width.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns MinimumColumnWidth(int width)
        {
            Options.MinimumColumnWidth = width;
            return this;
        }

        /// <summary>
        /// Sets the number of spaces between columns.
        /// </summary>
        /// <param name="gutter">The number of spaces between columns.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns Gutter(int gutter)
        {
            Options.Gutter = gutter;
            return this;
        }

        /// <summary>
        /// Sets the header foreground color.
        /// </summary>
        /// <param name="color">The header foreground color.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns HeaderColor(ConsoleColor color)
        {
            ApplyHeaderStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the header color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns HeaderColor(Style style)
        {
            ApplyHeaderStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the header color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns HeaderColor(string style)
        {
            ApplyHeaderStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the default content foreground color.
        /// </summary>
        /// <param name="color">The default content foreground color.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns ContentColor(ConsoleColor color)
        {
            ApplyContentStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the default content color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns ContentColor(Style style)
        {
            ApplyContentStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the default content color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Columns"/> instance.</returns>
        public Columns ContentColor(string style)
        {
            ApplyContentStyle(Style.Parse(style));
            return this;
        }

        private void ApplyHeaderStyle(Style style)
        {
            Options.HeaderScheme = Scheme(style, Options.HeaderScheme ?? RenderOptions.SubnoteScheme);
            Options.HeaderDecoration = style?.Decoration ?? Decoration.None;
            Options.HeaderForegroundRgb = style?.ForegroundRgb;
            Options.HeaderBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyContentStyle(Style style)
        {
            Options.ContentScheme = Scheme(style, Options.ContentScheme ?? RenderOptions.DefaultScheme);
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
