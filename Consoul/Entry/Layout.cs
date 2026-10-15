using System;
using System.Collections.Generic;
using System.Linq;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent pane layout construction over <see cref="LayoutEntry"/>.
    /// </summary>
    public class Layout : LayoutEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Layout"/> class.
        /// </summary>
        public Layout() : this((LayoutOptions)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout"/> class with a friendly name.
        /// </summary>
        /// <param name="name">Optional layout name used by migration-friendly snippets.</param>
        public Layout(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional layout rendering options.</param>
        public Layout(LayoutOptions options) : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout"/> class with configured panes.
        /// </summary>
        /// <param name="panes">The panes to render.</param>
        public Layout(params LayoutPane[] panes) : this()
        {
            AddPanes(panes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout"/> class with configured panes.
        /// </summary>
        /// <param name="panes">The panes to render.</param>
        public Layout(IEnumerable<LayoutPane> panes) : this()
        {
            AddPanes(panes);
        }

        /// <summary>
        /// Gets or sets the optional layout name.
        /// </summary>
        public string Name { get; set; }

        internal int? PaneSize { get; private set; }

        internal int PaneRatio { get; private set; } = 1;

        internal int? PaneMinimumSize { get; private set; }

        internal bool PaneVisible { get; private set; } = true;

        /// <summary>
        /// Gets the first pane with the requested title or child layout name.
        /// </summary>
        /// <param name="title">The pane title or layout name to find.</param>
        /// <returns>The matching pane.</returns>
        public LayoutPane this[string title]
        {
            get
            {
                LayoutPane pane = FindPane(title);
                if (pane == null)
                {
                    throw new KeyNotFoundException("No layout pane named '" + title + "' was found.");
                }

                return pane;
            }
        }

        /// <summary>
        /// Replaces this layout's content with one plain text pane.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="color">Optional foreground color used for pane content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Update(string content, ConsoleColor? color = null)
        {
            Panes.Clear();
            AddPane(content, color: color);
            return this;
        }

        /// <summary>
        /// Replaces this layout's content with one plain text pane using a Spectre-style RGB foreground color.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="color">The foreground color used for pane content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Update(string content, SpectreColor color)
        {
            Panes.Clear();
            Add(content, null, color);
            return this;
        }

        /// <summary>
        /// Replaces this layout's content with one plain text pane using an RGB foreground color.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="color">The foreground color used for pane content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Update(string content, RgbColor color)
        {
            Panes.Clear();
            Add(content, null, color);
            return this;
        }

        /// <summary>
        /// Replaces this layout's content with one plain text pane using Spectre-style style text.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="style">Optional style used for pane content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Update(string content, string style)
        {
            Panes.Clear();
            Add(content, null, style);
            return this;
        }

        /// <summary>
        /// Replaces this layout's content with one plain text pane using a reusable style.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="style">Optional style used for pane content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Update(string content, Style style)
        {
            Panes.Clear();
            Add(content, null, style);
            return this;
        }

        /// <summary>
        /// Replaces this layout's content with one deferred render action.
        /// </summary>
        /// <param name="render">The render action used to produce layout content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Update(Action render, int? captureWidth = null, int captureHeight = 200)
        {
            if (render == null)
            {
                throw new ArgumentNullException(nameof(render));
            }

            Panes.Clear();
            Add(LayoutPane.CreateLive(Consoul.Renderable(render), null, (ColorScheme?)null, captureWidth, captureHeight));
            return this;
        }

        /// <summary>
        /// Replaces this layout's content with one deferred renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce layout content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Update(IConsoulRenderable renderable, int? captureWidth = null, int captureHeight = 200)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            Panes.Clear();
            Add(LayoutPane.CreateLive(renderable, null, (ColorScheme?)null, captureWidth, captureHeight));
            return this;
        }

        /// <summary>
        /// Adds a pane to the layout.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="color">Optional foreground color used for pane content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPane(string content, string title = null, ConsoleColor? color = null)
        {
            Add(content, title, color);
            return this;
        }

        /// <summary>
        /// Adds a pane to the layout using a Spectre-style RGB foreground color.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="color">The foreground color used for pane content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPane(string content, string title, SpectreColor color)
        {
            Add(content, title, color);
            return this;
        }

        /// <summary>
        /// Adds a pane to the layout using an RGB foreground color.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="color">The foreground color used for pane content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPane(string content, string title, RgbColor color)
        {
            Add(content, title, color);
            return this;
        }

        /// <summary>
        /// Adds a pane to the layout using Spectre-style style text.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for pane content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPane(string content, string title, string style)
        {
            Add(content, title, style);
            return this;
        }

        /// <summary>
        /// Adds a pane to the layout using a reusable style.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for pane content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPane(string content, string title, Style style)
        {
            Add(content, title, style);
            return this;
        }

        /// <summary>
        /// Adds a pane whose content is captured from a renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="color">Optional foreground color used for the captured pane content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPane(IConsoulRenderable renderable, string title = null, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, title, color, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a pane whose content is captured from a renderable component using a Spectre-style RGB foreground color.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="color">The foreground color used for the captured pane content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPane(IConsoulRenderable renderable, string title, SpectreColor color, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, title, color, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a pane whose content is captured from a renderable component using an RGB foreground color.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="color">The foreground color used for the captured pane content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPane(IConsoulRenderable renderable, string title, RgbColor color, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, title, color, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a pane whose content is captured from a renderable component using Spectre-style style text.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for the captured pane content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPane(IConsoulRenderable renderable, string title, string style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, title, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a pane whose content is captured from a renderable component using a reusable style.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for the captured pane content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPane(IConsoulRenderable renderable, string title, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            Add(renderable, title, style, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds an existing pane to the layout.
        /// </summary>
        /// <param name="pane">The pane to add.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPane(LayoutPane pane)
        {
            Add(pane);
            return this;
        }

        /// <summary>
        /// Adds multiple configured panes to the layout.
        /// </summary>
        /// <param name="panes">The panes to add.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPanes(params LayoutPane[] panes)
        {
            return AddPanes((IEnumerable<LayoutPane>)panes);
        }

        /// <summary>
        /// Adds multiple configured panes to the layout.
        /// </summary>
        /// <param name="panes">The panes to add.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout AddPanes(IEnumerable<LayoutPane> panes)
        {
            if (panes == null)
            {
                return this;
            }

            foreach (LayoutPane pane in panes)
            {
                AddPane(pane);
            }

            return this;
        }

        /// <summary>
        /// Adds panes projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into panes.</param>
        /// <param name="contentSelector">Selects the pane content rendered for each model item.</param>
        /// <param name="titleSelector">Optionally selects the title rendered above each projected pane.</param>
        /// <param name="colorSelector">Optionally selects the foreground color used for each projected pane.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public new Layout AddPanes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> titleSelector = null,
            Func<T, ConsoleColor?> colorSelector = null,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            base.AddPanes(items, contentSelector, titleSelector, colorSelector, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds panes projected from a model collection with Spectre-style pane colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into panes.</param>
        /// <param name="contentSelector">Selects the pane content rendered for each model item.</param>
        /// <param name="titleSelector">Optionally selects the title rendered above each projected pane.</param>
        /// <param name="colorSelector">Selects the foreground color used for each projected pane.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public new Layout AddPanes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> titleSelector,
            Func<T, SpectreColor> colorSelector,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            base.AddPanes(items, contentSelector, titleSelector, colorSelector, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds panes projected from a model collection with RGB pane colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into panes.</param>
        /// <param name="contentSelector">Selects the pane content rendered for each model item.</param>
        /// <param name="titleSelector">Optionally selects the title rendered above each projected pane.</param>
        /// <param name="colorSelector">Selects the foreground color used for each projected pane.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public new Layout AddPanes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> titleSelector,
            Func<T, RgbColor> colorSelector,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            base.AddPanes(items, contentSelector, titleSelector, colorSelector, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds panes projected from a model collection using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into panes.</param>
        /// <param name="contentSelector">Selects the pane content rendered for each model item.</param>
        /// <param name="titleSelector">Optionally selects the title rendered above each projected pane.</param>
        /// <param name="styleSelector">Optionally selects style text such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c> for each projected pane.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public new Layout AddPanes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> titleSelector,
            Func<T, string> styleSelector,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            base.AddPanes(items, contentSelector, titleSelector, styleSelector, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Replaces current panes and renders the supplied pane contents side by side when possible.
        /// </summary>
        /// <param name="panes">The pane contents.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout SplitColumns(params string[] panes)
        {
            Panes.Clear();
            Options.Direction = LayoutDirection.Columns;
            foreach (string pane in panes ?? new string[0])
            {
                AddPane(pane);
            }

            return this;
        }

        /// <summary>
        /// Replaces current panes and renders the supplied renderable components side by side when possible.
        /// </summary>
        /// <param name="renderables">The renderable components to capture as panes.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout SplitColumns(params IConsoulRenderable[] renderables)
        {
            Panes.Clear();
            Options.Direction = LayoutDirection.Columns;
            foreach (IConsoulRenderable renderable in renderables ?? new IConsoulRenderable[0])
            {
                if (renderable != null)
                {
                    Add(renderable);
                }
            }

            return this;
        }

        /// <summary>
        /// Replaces current panes and renders the supplied layouts side by side when possible.
        /// </summary>
        /// <param name="layouts">The layouts to capture as panes.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout SplitColumns(params Layout[] layouts)
        {
            Panes.Clear();
            Options.Direction = LayoutDirection.Columns;
            foreach (Layout layout in layouts ?? new Layout[0])
            {
                if (layout != null)
                {
                    Add(layout.CreatePane());
                }
            }

            return this;
        }

        /// <summary>
        /// Replaces current panes and renders the supplied pane contents vertically.
        /// </summary>
        /// <param name="panes">The pane contents.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout SplitRows(params string[] panes)
        {
            Panes.Clear();
            Options.Direction = LayoutDirection.Rows;
            foreach (string pane in panes ?? new string[0])
            {
                AddPane(pane);
            }

            return this;
        }

        /// <summary>
        /// Replaces current panes and renders the supplied renderable components vertically.
        /// </summary>
        /// <param name="renderables">The renderable components to capture as panes.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout SplitRows(params IConsoulRenderable[] renderables)
        {
            Panes.Clear();
            Options.Direction = LayoutDirection.Rows;
            foreach (IConsoulRenderable renderable in renderables ?? new IConsoulRenderable[0])
            {
                if (renderable != null)
                {
                    Add(renderable);
                }
            }

            return this;
        }

        /// <summary>
        /// Replaces current panes and renders the supplied layouts vertically.
        /// </summary>
        /// <param name="layouts">The layouts to capture as panes.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout SplitRows(params Layout[] layouts)
        {
            Panes.Clear();
            Options.Direction = LayoutDirection.Rows;
            foreach (Layout layout in layouts ?? new Layout[0])
            {
                if (layout != null)
                {
                    Add(layout.CreatePane());
                }
            }

            return this;
        }

        /// <summary>
        /// Sets the requested layout width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the layout to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Expand()
        {
            Options.Width = null;
            return this;
        }

        /// <summary>
        /// Sets the preferred number of columns for column-oriented layouts.
        /// </summary>
        /// <param name="columns">The preferred number of columns.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout ColumnCount(int? columns)
        {
            Options.Columns = columns;
            return this;
        }

        /// <summary>
        /// Sets the minimum pane width before the layout stacks.
        /// </summary>
        /// <param name="width">The minimum pane width.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout MinimumPaneWidth(int width)
        {
            Options.MinimumPaneWidth = width;
            return this;
        }

        /// <summary>
        /// Sets the number of spaces between side-by-side panes.
        /// </summary>
        /// <param name="gutter">The number of spaces between panes.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Gutter(int gutter)
        {
            Options.Gutter = gutter;
            return this;
        }

        /// <summary>
        /// Sets the number of blank lines between row-oriented panes.
        /// </summary>
        /// <param name="spacing">The row spacing.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout RowSpacing(int spacing)
        {
            Options.RowSpacing = spacing;
            return this;
        }

        /// <summary>
        /// Sets a fixed pane size hint for this layout when it is nested inside another layout.
        /// </summary>
        /// <param name="size">The fixed size, or <see langword="null"/> to use flexible sizing.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Size(int? size)
        {
            if (size.HasValue && size.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero.");
            }

            PaneSize = size;
            return this;
        }

        /// <summary>
        /// Sets the relative pane size used when this layout is nested inside another layout.
        /// </summary>
        /// <param name="ratio">The relative ratio. Values less than one are treated as one.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Ratio(int ratio)
        {
            PaneRatio = Math.Max(1, ratio);
            return this;
        }

        /// <summary>
        /// Sets the minimum pane size needed before this layout is placed beside another nested layout.
        /// </summary>
        /// <param name="size">The minimum size, or <see langword="null"/> to use the parent layout default.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout MinimumSize(int? size)
        {
            if (size.HasValue && size.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Minimum size must be greater than zero.");
            }

            PaneMinimumSize = size;
            return this;
        }

        /// <summary>
        /// Includes or excludes this layout when it is nested inside another layout.
        /// </summary>
        /// <param name="visible">Whether the layout should be rendered.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Visible(bool visible = true)
        {
            PaneVisible = visible;
            return this;
        }

        /// <summary>
        /// Excludes this layout when it is nested inside another layout.
        /// </summary>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout Invisible()
        {
            return Visible(false);
        }

        /// <summary>
        /// Sets the pane title foreground color.
        /// </summary>
        /// <param name="color">The pane title foreground color.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout TitleColor(ConsoleColor color)
        {
            Options.TitleScheme = Scheme(color);
            Options.TitleDecoration = Decoration.None;
            Options.TitleForegroundRgb = null;
            Options.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the pane title foreground color from a Spectre-style RGB color.
        /// </summary>
        /// <param name="color">The pane title foreground color.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout TitleColor(SpectreColor color)
        {
            ApplyTitleStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the pane title foreground color from an RGB color.
        /// </summary>
        /// <param name="color">The pane title foreground color.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout TitleColor(RgbColor color)
        {
            ApplyTitleStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the pane title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout TitleColor(Style style)
        {
            ApplyTitleStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the pane title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout TitleColor(string style)
        {
            ApplyTitleStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the default pane content foreground color.
        /// </summary>
        /// <param name="color">The default pane content foreground color.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout ContentColor(ConsoleColor color)
        {
            Options.ContentScheme = Scheme(color);
            Options.ContentDecoration = Decoration.None;
            Options.ContentForegroundRgb = null;
            Options.ContentBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default pane content foreground color from a Spectre-style RGB color.
        /// </summary>
        /// <param name="color">The default pane content foreground color.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout ContentColor(SpectreColor color)
        {
            ApplyContentStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the default pane content foreground color from an RGB color.
        /// </summary>
        /// <param name="color">The default pane content foreground color.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout ContentColor(RgbColor color)
        {
            ApplyContentStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the default pane content color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout ContentColor(Style style)
        {
            ApplyContentStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the default pane content color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Layout"/> instance.</returns>
        public Layout ContentColor(string style)
        {
            ApplyContentStyle(Style.Parse(style));
            return this;
        }

        private void ApplyTitleStyle(Style style)
        {
            Options.TitleScheme = Scheme(style, Options.ResolveTitleScheme());
            Options.TitleDecoration = style?.Decoration ?? Decoration.None;
            Options.TitleForegroundRgb = style?.ForegroundRgb;
            Options.TitleBackgroundRgb = style?.BackgroundRgb;
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

        private LayoutPane CreatePane()
        {
            return LayoutPane.CreateLive(this, Name, (ColorScheme?)null)
                .Size(PaneSize)
                .Ratio(PaneRatio)
                .MinimumSize(PaneMinimumSize)
                .Visible(PaneVisible);
        }

        private LayoutPane FindPane(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            foreach (LayoutPane pane in Panes)
            {
                if (IsPaneMatch(pane, title))
                {
                    return pane;
                }

                if (pane.ChildLayout != null)
                {
                    LayoutPane childPane = pane.ChildLayout.FindPane(title);
                    if (childPane != null)
                    {
                        return childPane;
                    }
                }
            }

            return null;
        }

        private static bool IsPaneMatch(LayoutPane pane, string title)
        {
            return string.Equals(pane.Title, title, StringComparison.OrdinalIgnoreCase)
                || pane.ChildLayout != null && string.Equals(pane.ChildLayout.Name, title, StringComparison.OrdinalIgnoreCase);
        }
    }
}
