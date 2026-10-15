using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent table construction over <see cref="TableView"/>.
    /// </summary>
    /// <remarks>
    /// The table uses the same renderer and selection behavior as <see cref="TableView"/>. It exists as a migration-friendly
    /// builder for manual tables while model-backed MVC views can continue to use <see cref="DynamicTableView{T}"/>.
    /// </remarks>
    public class Table : TableView
    {
        private readonly List<TableColumn> _columns = new List<TableColumn>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        public Table() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class with explicit render options.
        /// </summary>
        /// <param name="options">Render options used by the table.</param>
        public Table(TableRenderOptions options) : base(options)
        {
            Rows = new TableRowCollection(this);
        }

        /// <summary>
        /// Gets the mutable row collection used by live dashboard snippets.
        /// </summary>
        public TableRowCollection Rows { get; }

        /// <summary>
        /// Gets the configured manual table columns.
        /// </summary>
        /// <remarks>
        /// Mutating a column returned by this collection updates the underlying <see cref="TableView"/> column metadata.
        /// Use <see cref="AddColumn(string, TableCellAlignment)"/> or <see cref="AddColumn(TableColumn)"/> to add columns.
        /// </remarks>
        public IReadOnlyList<TableColumn> Columns => _columns;

        /// <summary>
        /// Sets the table title rendered above the table.
        /// </summary>
        /// <param name="title">The table title.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(string title)
        {
            return ApplyTitle(title, null, null);
        }

        /// <summary>
        /// Sets the table title from a renderable component.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the table title.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(IConsoulRenderable title)
        {
            return ApplyTitle(TableColumn.Capture(title), null, null);
        }

        /// <summary>
        /// Sets the table title and alignment.
        /// </summary>
        /// <param name="title">The table title.</param>
        /// <param name="justification">The title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(string title, Justify justification)
        {
            return ApplyTitle(title, justification, null);
        }

        /// <summary>
        /// Sets the table title from a renderable component and alignment.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the table title.</param>
        /// <param name="justification">The title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(IConsoulRenderable title, Justify justification)
        {
            return ApplyTitle(TableColumn.Capture(title), justification, null);
        }

        /// <summary>
        /// Sets the table title and foreground color.
        /// </summary>
        /// <param name="title">The table title.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(string title, ConsoleColor color, Justify? justification = null)
        {
            return ApplyTitle(title, justification, new Style(color));
        }

        /// <summary>
        /// Sets the table title from a renderable component and foreground color.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the table title.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(IConsoulRenderable title, ConsoleColor color, Justify? justification = null)
        {
            return ApplyTitle(TableColumn.Capture(title), justification, new Style(color));
        }

        /// <summary>
        /// Sets the table title and foreground color.
        /// </summary>
        /// <param name="title">The table title.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(string title, SpectreColor color, Justify? justification = null)
        {
            return ApplyTitle(title, justification, new Style(color));
        }

        /// <summary>
        /// Sets the table title from a renderable component and foreground color.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the table title.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(IConsoulRenderable title, SpectreColor color, Justify? justification = null)
        {
            return ApplyTitle(TableColumn.Capture(title), justification, new Style(color));
        }

        /// <summary>
        /// Sets the table title and foreground color.
        /// </summary>
        /// <param name="title">The table title.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(string title, RgbColor color, Justify? justification = null)
        {
            return ApplyTitle(title, justification, new Style(color));
        }

        /// <summary>
        /// Sets the table title from a renderable component and foreground color.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the table title.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(IConsoulRenderable title, RgbColor color, Justify? justification = null)
        {
            return ApplyTitle(TableColumn.Capture(title), justification, new Style(color));
        }

        /// <summary>
        /// Sets the table title from a reusable style object.
        /// </summary>
        /// <param name="title">The table title.</param>
        /// <param name="style">The title style.</param>
        /// <param name="justification">Optional title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(string title, Style style, Justify? justification = null)
        {
            return ApplyTitle(title, justification, style);
        }

        /// <summary>
        /// Sets the table title from a renderable component and reusable style object.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the table title.</param>
        /// <param name="style">The title style.</param>
        /// <param name="justification">Optional title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(IConsoulRenderable title, Style style, Justify? justification = null)
        {
            return ApplyTitle(TableColumn.Capture(title), justification, style);
        }

        /// <summary>
        /// Sets the table title from a renderable component and Spectre-style style text.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the table title.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <param name="justification">Optional title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(IConsoulRenderable title, string style, Justify? justification = null)
        {
            return ApplyTitle(TableColumn.Capture(title), justification, Style.Parse(style));
        }

        /// <summary>
        /// Sets the table title from a reusable title object.
        /// </summary>
        /// <param name="title">The title metadata.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Title(TableTitle title)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            return ApplyTitle(title.Text, title.Justification, title.Style);
        }

        /// <summary>
        /// Sets the table caption rendered below the table.
        /// </summary>
        /// <param name="caption">The table caption.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(string caption)
        {
            return ApplyCaption(caption, null, null);
        }

        /// <summary>
        /// Sets the table caption from a renderable component.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(IConsoulRenderable caption)
        {
            return ApplyCaption(TableColumn.Capture(caption), null, null);
        }

        /// <summary>
        /// Sets the table caption and alignment.
        /// </summary>
        /// <param name="caption">The table caption.</param>
        /// <param name="justification">The caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(string caption, Justify justification)
        {
            return ApplyCaption(caption, justification, null);
        }

        /// <summary>
        /// Sets the table caption from a renderable component and alignment.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="justification">The caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(IConsoulRenderable caption, Justify justification)
        {
            return ApplyCaption(TableColumn.Capture(caption), justification, null);
        }

        /// <summary>
        /// Sets the table caption and foreground color.
        /// </summary>
        /// <param name="caption">The table caption.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(string caption, ConsoleColor color, Justify? justification = null)
        {
            return ApplyCaption(caption, justification, new Style(color));
        }

        /// <summary>
        /// Sets the table caption from a renderable component and foreground color.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(IConsoulRenderable caption, ConsoleColor color, Justify? justification = null)
        {
            return ApplyCaption(TableColumn.Capture(caption), justification, new Style(color));
        }

        /// <summary>
        /// Sets the table caption and foreground color.
        /// </summary>
        /// <param name="caption">The table caption.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(string caption, SpectreColor color, Justify? justification = null)
        {
            return ApplyCaption(caption, justification, new Style(color));
        }

        /// <summary>
        /// Sets the table caption from a renderable component and foreground color.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(IConsoulRenderable caption, SpectreColor color, Justify? justification = null)
        {
            return ApplyCaption(TableColumn.Capture(caption), justification, new Style(color));
        }

        /// <summary>
        /// Sets the table caption and foreground color.
        /// </summary>
        /// <param name="caption">The table caption.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(string caption, RgbColor color, Justify? justification = null)
        {
            return ApplyCaption(caption, justification, new Style(color));
        }

        /// <summary>
        /// Sets the table caption from a renderable component and foreground color.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(IConsoulRenderable caption, RgbColor color, Justify? justification = null)
        {
            return ApplyCaption(TableColumn.Capture(caption), justification, new Style(color));
        }

        /// <summary>
        /// Sets the table caption from a reusable style object.
        /// </summary>
        /// <param name="caption">The table caption.</param>
        /// <param name="style">The caption style.</param>
        /// <param name="justification">Optional caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(string caption, Style style, Justify? justification = null)
        {
            return ApplyCaption(caption, justification, style);
        }

        /// <summary>
        /// Sets the table caption from a renderable component and reusable style object.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="style">The caption style.</param>
        /// <param name="justification">Optional caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(IConsoulRenderable caption, Style style, Justify? justification = null)
        {
            return ApplyCaption(TableColumn.Capture(caption), justification, style);
        }

        /// <summary>
        /// Sets the table caption from a renderable component and Spectre-style style text.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <param name="justification">Optional caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(IConsoulRenderable caption, string style, Justify? justification = null)
        {
            return ApplyCaption(TableColumn.Capture(caption), justification, Style.Parse(style));
        }

        /// <summary>
        /// Sets the table caption from Spectre-style title metadata.
        /// </summary>
        /// <param name="caption">The caption metadata.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(TableTitle caption)
        {
            if (caption == null)
            {
                throw new ArgumentNullException(nameof(caption));
            }

            return ApplyCaption(caption.Text, caption.Justification, caption.Style);
        }

        /// <summary>
        /// Sets the table caption from a reusable caption object.
        /// </summary>
        /// <param name="caption">The caption metadata.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Caption(TableCaption caption)
        {
            if (caption == null)
            {
                throw new ArgumentNullException(nameof(caption));
            }

            return ApplyCaption(caption.Text, caption.Justification, caption.Style);
        }

        /// <summary>
        /// Sets the table border style.
        /// </summary>
        /// <param name="style">The border style to use.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Border(TableBorderStyle style)
        {
            TableRenderOptions.Lines.BorderStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the table border preset using Spectre-style border objects.
        /// </summary>
        /// <param name="border">The border preset to use.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Border(TableBorder border)
        {
            if (border == null)
            {
                throw new ArgumentNullException(nameof(border));
            }

            TableRenderOptions.Lines.BorderStyle = border.Style;
            if (border.HeaderHorizontal.HasValue)
            {
                TableRenderOptions.Lines.HeaderHorizontal = border.HeaderHorizontal.Value;
            }

            if (border.HeaderVertical.HasValue)
            {
                TableRenderOptions.Lines.HeaderVertical = border.HeaderVertical.Value;
            }

            if (border.ContentHorizontal.HasValue)
            {
                TableRenderOptions.Lines.ContentHorizontal = border.ContentHorizontal.Value;
            }

            if (border.ContentVertical.HasValue)
            {
                TableRenderOptions.Lines.ContentVertical = border.ContentVertical.Value;
            }

            return this;
        }

        /// <summary>
        /// Uses a rounded border preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table RoundedBorder()
        {
            return Border(TableBorderStyle.Rounded);
        }

        /// <summary>
        /// Uses a square border preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table SquareBorder()
        {
            return Border(TableBorderStyle.Square);
        }

        /// <summary>
        /// Uses an ASCII border preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AsciiBorder()
        {
            return Border(TableBorderStyle.Ascii);
        }

        /// <summary>
        /// Uses a heavy border preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HeavyBorder()
        {
            return Border(TableBorderStyle.Heavy);
        }

        /// <summary>
        /// Uses a double-line border preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table DoubleBorder()
        {
            return Border(TableBorderStyle.Double);
        }

        /// <summary>
        /// Hides all table border and separator lines.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table NoBorder()
        {
            return Border(TableBorder.None);
        }

        /// <summary>
        /// Uses the simple legacy line style.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table SimpleBorder()
        {
            return Border(TableBorderStyle.Simple);
        }

        /// <summary>
        /// Uses the simple heavy border preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table SimpleHeavyBorder()
        {
            return Border(TableBorder.SimpleHeavy);
        }

        /// <summary>
        /// Uses a minimal border preset that hides all border and separator lines.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table MinimalBorder()
        {
            return Border(TableBorder.Minimal);
        }

        /// <summary>
        /// Uses a minimal border preset with a heavy header separator.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table MinimalHeavyHeadBorder()
        {
            return Border(TableBorder.MinimalHeavyHead);
        }

        /// <summary>
        /// Uses a minimal border preset with a double header separator.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table MinimalDoubleHeadBorder()
        {
            return Border(TableBorder.MinimalDoubleHead);
        }

        /// <summary>
        /// Uses a horizontal-line-only border preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HorizontalBorder()
        {
            return Border(TableBorder.Horizontal);
        }

        /// <summary>
        /// Uses the alternate ASCII border preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Ascii2Border()
        {
            return Border(TableBorder.Ascii2);
        }

        /// <summary>
        /// Uses an ASCII border preset with an emphasized header separator.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AsciiDoubleHeadBorder()
        {
            return Border(TableBorder.AsciiDoubleHead);
        }

        /// <summary>
        /// Uses a markdown-friendly ASCII border preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table MarkdownBorder()
        {
            return Border(TableBorder.Markdown);
        }

        /// <summary>
        /// Uses the heavy border edge preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HeavyEdgeBorder()
        {
            return Border(TableBorder.HeavyEdge);
        }

        /// <summary>
        /// Uses the heavy border preset with an emphasized header separator.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HeavyHeadBorder()
        {
            return Border(TableBorder.HeavyHead);
        }

        /// <summary>
        /// Uses the double-line border edge preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table DoubleEdgeBorder()
        {
            return Border(TableBorder.DoubleEdge);
        }

        /// <summary>
        /// Uses a terminal-safe ASCII border preset.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table SafeBorder()
        {
            return Border(TableBorder.Safe);
        }

        /// <summary>
        /// Sets the requested maximum table width.
        /// </summary>
        /// <param name="width">The requested width. When null, width is controlled by <see cref="TableRenderOptions.TableWidthPercentage"/>.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Width(int? width)
        {
            TableRenderOptions.MaximumTableWidth = width.HasValue
                ? Math.Max(1, width.Value)
                : (int?)null;
            TableRenderOptions.Expand = true;
            return this;
        }

        /// <summary>
        /// Sets the table width as a percentage of the active terminal width.
        /// </summary>
        /// <param name="percentage">The table width percentage, from 0 to 1.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table WidthPercentage(decimal percentage)
        {
            TableRenderOptions.MaximumTableWidth = null;
            TableRenderOptions.TableWidthPercentage = percentage;
            TableRenderOptions.Expand = true;
            return this;
        }

        /// <summary>
        /// Uses the full available terminal width for the table.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Expand()
        {
            TableRenderOptions.MaximumTableWidth = null;
            TableRenderOptions.TableWidthPercentage = 1m;
            TableRenderOptions.Expand = true;
            return this;
        }

        /// <summary>
        /// Collapses the table to the width requested by its content when no explicit width is set.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Collapse()
        {
            TableRenderOptions.MaximumTableWidth = null;
            TableRenderOptions.Expand = false;
            return this;
        }

        /// <summary>
        /// Shows or hides header cells while keeping column metadata for layout.
        /// </summary>
        /// <param name="show">Whether headers should render.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ShowHeaders(bool show = true)
        {
            TableRenderOptions.ShowHeaders = show;
            return this;
        }

        /// <summary>
        /// Hides header cells while keeping column metadata for layout.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HideHeaders()
        {
            return ShowHeaders(false);
        }

        /// <summary>
        /// Shows or hides interactive table footer help.
        /// </summary>
        /// <param name="show">Whether footer help should render.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ShowFooter(bool show = true)
        {
            TableRenderOptions.ShowFooterHelp = show;
            return this;
        }

        /// <summary>
        /// Shows column footer rows using Spectre-style plural naming.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ShowFooters()
        {
            TableRenderOptions.ShowFooters = true;
            return this;
        }

        /// <summary>
        /// Hides interactive table footer help.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HideFooter()
        {
            return ShowFooter(false);
        }

        /// <summary>
        /// Hides column footer rows using Spectre-style plural naming.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HideFooters()
        {
            TableRenderOptions.ShowFooters = false;
            return this;
        }

        /// <summary>
        /// Shows one-based row numbers as the first table column.
        /// </summary>
        /// <param name="enabled">Whether row numbers should render.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ShowRowNumbers(bool enabled = true)
        {
            TableRenderOptions.ShowRowNumbers = enabled;
            return this;
        }

        /// <summary>
        /// Shows or hides horizontal separators between content rows.
        /// </summary>
        /// <param name="show">Whether row separators should render.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ShowRowSeparators(bool show = true)
        {
            TableRenderOptions.Lines.ContentHorizontal = show;
            return this;
        }

        /// <summary>
        /// Hides horizontal separators between content rows.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HideRowSeparators()
        {
            return ShowRowSeparators(false);
        }

        /// <summary>
        /// Sets the row number header text.
        /// </summary>
        /// <param name="header">The row number header text.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table RowNumberHeader(string header)
        {
            TableRenderOptions.RowNumberHeader = header;
            return this;
        }

        /// <summary>
        /// Sets the table-wide header cell alignment.
        /// </summary>
        /// <param name="alignment">The header cell alignment.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HeaderAlignment(TableCellAlignment alignment)
        {
            TableRenderOptions.HeaderAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the table-wide header cell alignment using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The header cell justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HeaderAlignment(Justify justify)
        {
            TableRenderOptions.HeaderAlignment = JustifyMap.ToTableCellAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets the table-wide content cell alignment.
        /// </summary>
        /// <param name="alignment">The content cell alignment.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ContentAlignment(TableCellAlignment alignment)
        {
            TableRenderOptions.ContentAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the table-wide content cell alignment using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The content cell justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ContentAlignment(Justify justify)
        {
            TableRenderOptions.ContentAlignment = JustifyMap.ToTableCellAlignment(justify);
            return this;
        }

        /// <summary>
        /// Left-aligns table header and content cells.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table LeftAligned()
        {
            TableRenderOptions.HeaderAlignment = TableCellAlignment.Left;
            TableRenderOptions.ContentAlignment = TableCellAlignment.Left;
            return this;
        }

        /// <summary>
        /// Left-aligns table header and content cells using text-builder justification vocabulary.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table LeftJustified()
        {
            return LeftAligned();
        }

        /// <summary>
        /// Centers table header and content cells.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Centered()
        {
            TableRenderOptions.HeaderAlignment = TableCellAlignment.Center;
            TableRenderOptions.ContentAlignment = TableCellAlignment.Center;
            return this;
        }

        /// <summary>
        /// Right-aligns table header and content cells.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table RightAligned()
        {
            TableRenderOptions.HeaderAlignment = TableCellAlignment.Right;
            TableRenderOptions.ContentAlignment = TableCellAlignment.Right;
            return this;
        }

        /// <summary>
        /// Right-aligns table header and content cells using text-builder justification vocabulary.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table RightJustified()
        {
            return RightAligned();
        }

        /// <summary>
        /// Sets the table title alignment.
        /// </summary>
        /// <param name="alignment">The title alignment.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table TitleAlignment(TextBlockAlignment alignment)
        {
            TableRenderOptions.TitleAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the table title alignment using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The title justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table TitleAlignment(Justify justify)
        {
            TableRenderOptions.TitleAlignment = JustifyMap.ToTextBlockAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets the table caption alignment.
        /// </summary>
        /// <param name="alignment">The caption alignment.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table CaptionAlignment(TextBlockAlignment alignment)
        {
            TableRenderOptions.CaptionAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the table caption alignment using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The caption justification.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table CaptionAlignment(Justify justify)
        {
            TableRenderOptions.CaptionAlignment = JustifyMap.ToTextBlockAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets the border and separator foreground color.
        /// </summary>
        /// <param name="color">The border foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table BorderColor(ConsoleColor color)
        {
            ApplyLineStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the border and separator foreground color.
        /// </summary>
        /// <param name="color">The border foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table BorderColor(SpectreColor color)
        {
            ApplyLineStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the border and separator foreground color.
        /// </summary>
        /// <param name="color">The border foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table BorderColor(RgbColor color)
        {
            ApplyLineStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the border and separator color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table BorderColor(string style)
        {
            ApplyLineStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the border and separator color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table BorderColor(Style style)
        {
            ApplyLineStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the header foreground color.
        /// </summary>
        /// <param name="color">The header foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HeaderColor(ConsoleColor color)
        {
            ApplyHeaderStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the header foreground color.
        /// </summary>
        /// <param name="color">The header foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HeaderColor(SpectreColor color)
        {
            ApplyHeaderStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the header foreground color.
        /// </summary>
        /// <param name="color">The header foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HeaderColor(RgbColor color)
        {
            ApplyHeaderStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the header color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HeaderColor(string style)
        {
            ApplyHeaderStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the header color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table HeaderColor(Style style)
        {
            ApplyHeaderStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the primary content foreground color.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ContentColor(ConsoleColor color)
        {
            ApplyContentStyles(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the primary content foreground color.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ContentColor(SpectreColor color)
        {
            ApplyContentStyles(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the primary content foreground color.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ContentColor(RgbColor color)
        {
            ApplyContentStyles(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the primary content color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ContentColor(string style)
        {
            ApplyContentStyles(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the primary content color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table ContentColor(Style style)
        {
            ApplyContentStyles(style);
            return this;
        }

        /// <summary>
        /// Sets the alternate-row content foreground color.
        /// </summary>
        /// <param name="color">The alternate-row foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AlternatingRowColor(ConsoleColor color)
        {
            ApplyContentStyle2(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the alternate-row content foreground color.
        /// </summary>
        /// <param name="color">The alternate-row foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AlternatingRowColor(SpectreColor color)
        {
            ApplyContentStyle2(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the alternate-row content foreground color.
        /// </summary>
        /// <param name="color">The alternate-row foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AlternatingRowColor(RgbColor color)
        {
            ApplyContentStyle2(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the alternate-row content color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AlternatingRowColor(string style)
        {
            ApplyContentStyle2(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the alternate-row content color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AlternatingRowColor(Style style)
        {
            ApplyContentStyle2(style);
            return this;
        }

        /// <summary>
        /// Sets the table title foreground color.
        /// </summary>
        /// <param name="color">The title foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table TitleColor(ConsoleColor color)
        {
            TableRenderOptions.TitleScheme = Scheme(color);
            TableRenderOptions.TitleForegroundRgb = null;
            TableRenderOptions.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the table title foreground color.
        /// </summary>
        /// <param name="color">The title foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table TitleColor(SpectreColor color)
        {
            TableRenderOptions.TitleScheme = Scheme(color);
            TableRenderOptions.TitleForegroundRgb = color.ToRgbColor();
            TableRenderOptions.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the table title foreground color.
        /// </summary>
        /// <param name="color">The title foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table TitleColor(RgbColor color)
        {
            TableRenderOptions.TitleScheme = Scheme(color);
            TableRenderOptions.TitleForegroundRgb = color;
            TableRenderOptions.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the table title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table TitleColor(string style)
        {
            Style parsed = Style.Parse(style);
            TableRenderOptions.TitleScheme = parsed.ToColorScheme(TableRenderOptions.TitleScheme);
            TableRenderOptions.TitleDecoration = parsed.Decoration;
            TableRenderOptions.TitleForegroundRgb = parsed.ForegroundRgb;
            TableRenderOptions.TitleBackgroundRgb = parsed.BackgroundRgb;
            return this;
        }

        /// <summary>
        /// Sets the table title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table TitleColor(Style style)
        {
            TableRenderOptions.TitleScheme = Scheme(style, TableRenderOptions.TitleScheme);
            TableRenderOptions.TitleDecoration = style?.Decoration ?? Decoration.None;
            TableRenderOptions.TitleForegroundRgb = style?.ForegroundRgb;
            TableRenderOptions.TitleBackgroundRgb = style?.BackgroundRgb;
            return this;
        }

        /// <summary>
        /// Sets the table caption foreground color.
        /// </summary>
        /// <param name="color">The caption foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table CaptionColor(ConsoleColor color)
        {
            TableRenderOptions.CaptionScheme = Scheme(color);
            TableRenderOptions.CaptionForegroundRgb = null;
            TableRenderOptions.CaptionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the table caption foreground color.
        /// </summary>
        /// <param name="color">The caption foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table CaptionColor(SpectreColor color)
        {
            TableRenderOptions.CaptionScheme = Scheme(color);
            TableRenderOptions.CaptionForegroundRgb = color.ToRgbColor();
            TableRenderOptions.CaptionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the table caption foreground color.
        /// </summary>
        /// <param name="color">The caption foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table CaptionColor(RgbColor color)
        {
            TableRenderOptions.CaptionScheme = Scheme(color);
            TableRenderOptions.CaptionForegroundRgb = color;
            TableRenderOptions.CaptionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the table caption color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table CaptionColor(string style)
        {
            Style parsed = Style.Parse(style);
            TableRenderOptions.CaptionScheme = parsed.ToColorScheme(TableRenderOptions.CaptionScheme);
            TableRenderOptions.CaptionDecoration = parsed.Decoration;
            TableRenderOptions.CaptionForegroundRgb = parsed.ForegroundRgb;
            TableRenderOptions.CaptionBackgroundRgb = parsed.BackgroundRgb;
            return this;
        }

        /// <summary>
        /// Sets the table caption color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table CaptionColor(Style style)
        {
            TableRenderOptions.CaptionScheme = Scheme(style, TableRenderOptions.CaptionScheme);
            TableRenderOptions.CaptionDecoration = style?.Decoration ?? Decoration.None;
            TableRenderOptions.CaptionForegroundRgb = style?.ForegroundRgb;
            TableRenderOptions.CaptionBackgroundRgb = style?.BackgroundRgb;
            return this;
        }

        /// <summary>
        /// Sets the column footer foreground color.
        /// </summary>
        /// <param name="color">The footer foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table FooterColor(ConsoleColor color)
        {
            ApplyFooterStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the column footer foreground color.
        /// </summary>
        /// <param name="color">The footer foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table FooterColor(SpectreColor color)
        {
            ApplyFooterStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the column footer foreground color.
        /// </summary>
        /// <param name="color">The footer foreground color.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table FooterColor(RgbColor color)
        {
            ApplyFooterStyle(new Style(color));
            return this;
        }

        /// <summary>
        /// Sets the column footer color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table FooterColor(string style)
        {
            ApplyFooterStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the column footer color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table FooterColor(Style style)
        {
            ApplyFooterStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the message rendered inside the table body when no rows are available.
        /// </summary>
        /// <param name="message">The empty table message.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table EmptyMessage(string message)
        {
            TableRenderOptions.EmptyMessage = message;
            return this;
        }

        /// <summary>
        /// Sets the summary text rendered below the table.
        /// </summary>
        /// <param name="summary">The summary text.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Summary(string summary)
        {
            TableRenderOptions.SummaryText = summary;
            return this;
        }

        /// <summary>
        /// Sets the whitespace filler character used inside cells.
        /// </summary>
        /// <param name="character">The whitespace filler character.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table Whitespace(char character)
        {
            TableRenderOptions.WhitespaceCharacter = character;
            return this;
        }

        /// <summary>
        /// Adds a table column.
        /// </summary>
        /// <param name="header">The header text.</param>
        /// <param name="alignment">Optional alignment for this column.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumn(string header, TableCellAlignment alignment = TableCellAlignment.Default)
        {
            var column = new TableColumn(header);
            if (alignment != TableCellAlignment.Default)
            {
                column.Alignment = alignment;
            }

            return AddColumn(column);
        }

        /// <summary>
        /// Adds a table column using Spectre-style alignment vocabulary.
        /// </summary>
        /// <param name="header">The header text.</param>
        /// <param name="alignment">Alignment for this column.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumn(string header, Justify alignment)
        {
            return AddColumn(header, JustifyMap.ToTableCellAlignment(alignment));
        }

        /// <summary>
        /// Adds and configures a table column.
        /// </summary>
        /// <param name="header">The header text.</param>
        /// <param name="configure">Column configuration callback.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumn(string header, Action<TableColumn> configure)
        {
            var column = new TableColumn(header);
            configure?.Invoke(column);
            return AddColumn(column);
        }

        /// <summary>
        /// Adds a table column from captured renderable header content.
        /// </summary>
        /// <param name="header">The renderable component used to produce the header text.</param>
        /// <param name="alignment">Optional alignment for this column.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumn(IConsoulRenderable header, TableCellAlignment alignment = TableCellAlignment.Default)
        {
            var column = new TableColumn(header);
            if (alignment != TableCellAlignment.Default)
            {
                column.Alignment = alignment;
            }

            return AddColumn(column);
        }

        /// <summary>
        /// Adds a table column from captured renderable header content using Spectre-style alignment vocabulary.
        /// </summary>
        /// <param name="header">The renderable component used to produce the header text.</param>
        /// <param name="alignment">Alignment for this column.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumn(IConsoulRenderable header, Justify alignment)
        {
            return AddColumn(header, JustifyMap.ToTableCellAlignment(alignment));
        }

        /// <summary>
        /// Adds and configures a table column from captured renderable header content.
        /// </summary>
        /// <param name="header">The renderable component used to produce the header text.</param>
        /// <param name="configure">Column configuration callback.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumn(IConsoulRenderable header, Action<TableColumn> configure)
        {
            var column = new TableColumn(header);
            configure?.Invoke(column);
            return AddColumn(column);
        }

        /// <summary>
        /// Adds a configured table column.
        /// </summary>
        /// <param name="column">The table column configuration.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumn(TableColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            int columnIndex = HeaderCount;
            _columns.Add(column);
            column.AttachChangeHandler(updated => ApplyColumnConfiguration(columnIndex, updated));
            AddHeader(column.ResolveHeader(), column.Alignment ?? TableCellAlignment.Default);
            ApplyColumnConfiguration(columnIndex, column);

            return this;
        }

        /// <summary>
        /// Adds a table column only when the supplied values contain visible cell content.
        /// </summary>
        /// <param name="header">The header text.</param>
        /// <param name="values">Candidate values used to decide whether the column should be shown.</param>
        /// <param name="alignment">Optional alignment for this column.</param>
        /// <returns><c>true</c> when the column was added; otherwise, <c>false</c>.</returns>
        public bool AddOptionalColumn(string header, IEnumerable values, TableCellAlignment alignment = TableCellAlignment.Default)
        {
            var column = new TableColumn(header);
            if (alignment != TableCellAlignment.Default)
            {
                column.Alignment = alignment;
            }

            return AddOptionalColumn(column, values);
        }

        /// <summary>
        /// Adds a table column using Spectre-style alignment vocabulary only when the supplied values contain visible cell content.
        /// </summary>
        /// <param name="header">The header text.</param>
        /// <param name="values">Candidate values used to decide whether the column should be shown.</param>
        /// <param name="alignment">Alignment for this column.</param>
        /// <returns><c>true</c> when the column was added; otherwise, <c>false</c>.</returns>
        public bool AddOptionalColumn(string header, IEnumerable values, Justify alignment)
        {
            return AddOptionalColumn(header, values, JustifyMap.ToTableCellAlignment(alignment));
        }

        /// <summary>
        /// Adds and configures a table column only when the supplied values contain visible cell content.
        /// </summary>
        /// <param name="header">The header text.</param>
        /// <param name="values">Candidate values used to decide whether the column should be shown.</param>
        /// <param name="configure">Column configuration callback.</param>
        /// <returns><c>true</c> when the column was added; otherwise, <c>false</c>.</returns>
        public bool AddOptionalColumn(string header, IEnumerable values, Action<TableColumn> configure)
        {
            var column = new TableColumn(header);
            configure?.Invoke(column);
            return AddOptionalColumn(column, values);
        }

        /// <summary>
        /// Adds a table column from captured renderable header content only when the supplied values contain visible cell content.
        /// </summary>
        /// <param name="header">The renderable component used to produce the header text.</param>
        /// <param name="values">Candidate values used to decide whether the column should be shown.</param>
        /// <param name="alignment">Optional alignment for this column.</param>
        /// <returns><c>true</c> when the column was added; otherwise, <c>false</c>.</returns>
        public bool AddOptionalColumn(IConsoulRenderable header, IEnumerable values, TableCellAlignment alignment = TableCellAlignment.Default)
        {
            var column = new TableColumn(header);
            if (alignment != TableCellAlignment.Default)
            {
                column.Alignment = alignment;
            }

            return AddOptionalColumn(column, values);
        }

        /// <summary>
        /// Adds a table column from captured renderable header content using Spectre-style alignment vocabulary only when values are visible.
        /// </summary>
        /// <param name="header">The renderable component used to produce the header text.</param>
        /// <param name="values">Candidate values used to decide whether the column should be shown.</param>
        /// <param name="alignment">Alignment for this column.</param>
        /// <returns><c>true</c> when the column was added; otherwise, <c>false</c>.</returns>
        public bool AddOptionalColumn(IConsoulRenderable header, IEnumerable values, Justify alignment)
        {
            return AddOptionalColumn(header, values, JustifyMap.ToTableCellAlignment(alignment));
        }

        /// <summary>
        /// Adds and configures a table column from captured renderable header content only when values are visible.
        /// </summary>
        /// <param name="header">The renderable component used to produce the header text.</param>
        /// <param name="values">Candidate values used to decide whether the column should be shown.</param>
        /// <param name="configure">Column configuration callback.</param>
        /// <returns><c>true</c> when the column was added; otherwise, <c>false</c>.</returns>
        public bool AddOptionalColumn(IConsoulRenderable header, IEnumerable values, Action<TableColumn> configure)
        {
            var column = new TableColumn(header);
            configure?.Invoke(column);
            return AddOptionalColumn(column, values);
        }

        /// <summary>
        /// Adds a configured table column only when the supplied values contain visible cell content.
        /// </summary>
        /// <param name="column">The table column configuration.</param>
        /// <param name="values">Candidate values used to decide whether the column should be shown.</param>
        /// <returns><c>true</c> when the column was added; otherwise, <c>false</c>.</returns>
        public bool AddOptionalColumn(TableColumn column, IEnumerable values)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            if (!HasVisibleValues(values))
            {
                return false;
            }

            AddColumn(column);
            return true;
        }

        /// <summary>
        /// Adds multiple table columns.
        /// </summary>
        /// <param name="headers">The header texts.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumns(params string[] headers)
        {
            foreach (string header in headers ?? new string[0])
            {
                AddColumn(header);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple table columns.
        /// </summary>
        /// <param name="headers">The header texts.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumns(IEnumerable<string> headers)
        {
            foreach (string header in headers ?? new string[0])
            {
                AddColumn(header);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple table columns from captured renderable header content.
        /// </summary>
        /// <param name="headers">The renderable components used to produce header text.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumns(params IConsoulRenderable[] headers)
        {
            return AddColumns((IEnumerable<IConsoulRenderable>)headers);
        }

        /// <summary>
        /// Adds multiple table columns from captured renderable header content.
        /// </summary>
        /// <param name="headers">The renderable components used to produce header text.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumns(IEnumerable<IConsoulRenderable> headers)
        {
            foreach (IConsoulRenderable header in headers ?? new IConsoulRenderable[0])
            {
                AddColumn(header);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple configured table columns.
        /// </summary>
        /// <param name="columns">The table column configurations.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumns(params TableColumn[] columns)
        {
            return AddColumns((IEnumerable<TableColumn>)columns);
        }

        /// <summary>
        /// Adds multiple configured table columns.
        /// </summary>
        /// <param name="columns">The table column configurations.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddColumns(IEnumerable<TableColumn> columns)
        {
            foreach (TableColumn column in columns ?? new TableColumn[0])
            {
                AddColumn(column);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple header cells to the table.
        /// </summary>
        /// <param name="labels">Header texts to add.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public new Table AddHeaders(params string[] labels)
        {
            foreach (string label in labels ?? new string[0])
            {
                AddColumn(label);
            }

            return this;
        }

        /// <summary>
        /// Adds a row to the table.
        /// </summary>
        /// <param name="cells">The row cell values.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddRow(params string[] cells)
        {
            base.AddRow(cells ?? new string[0]);
            return this;
        }

        /// <summary>
        /// Adds a row to the table, formatting cell values with the current culture.
        /// </summary>
        /// <param name="cells">The row cell values.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddRow(params object[] cells)
        {
            return AddRow(new List<string>(FormatCells(cells)).ToArray());
        }

        /// <summary>
        /// Adds multiple rows to the table from inline string row arrays.
        /// </summary>
        /// <param name="firstRow">The first row to add.</param>
        /// <param name="additionalRows">Additional rows to add.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddRows(string[] firstRow, params string[][] additionalRows)
        {
            AddRow(firstRow ?? new string[0]);

            foreach (string[] row in additionalRows ?? new string[0][])
            {
                AddRow(row);
            }

            return this;
        }

        /// <summary>
        /// Adds key/value pairs as two-column rows, creating default <c>Key</c> and <c>Value</c> columns when the table has no columns.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="values">The key/value pairs to append.</param>
        /// <param name="keyHeader">Header text used for the key column when columns are created automatically.</param>
        /// <param name="valueHeader">Header text used for the value column when columns are created automatically.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddKeyValueRows<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> values, string keyHeader = "Key", string valueHeader = "Value")
        {
            EnsureKeyValueColumns(keyHeader, valueHeader);

            foreach (KeyValuePair<TKey, TValue> pair in values ?? new KeyValuePair<TKey, TValue>[0])
            {
                AddRow(pair.Key, pair.Value);
            }

            return this;
        }

        /// <summary>
        /// Adds entries from a non-generic dictionary as two-column rows, creating default <c>Key</c> and <c>Value</c> columns when the table has no columns.
        /// </summary>
        /// <param name="values">The dictionary entries to append.</param>
        /// <param name="keyHeader">Header text used for the key column when columns are created automatically.</param>
        /// <param name="valueHeader">Header text used for the value column when columns are created automatically.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddDictionaryRows(IDictionary values, string keyHeader = "Key", string valueHeader = "Value")
        {
            EnsureKeyValueColumns(keyHeader, valueHeader);

            if (values == null)
            {
                return this;
            }

            foreach (DictionaryEntry entry in values)
            {
                AddRow(entry.Key, entry.Value);
            }

            return this;
        }

        /// <summary>
        /// Adds an empty row with the current number of configured columns.
        /// </summary>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddEmptyRow()
        {
            int cellCount = Math.Max(0, HeaderCount);
            return AddRow(new string[cellCount]);
        }

        /// <summary>
        /// Inserts a row into the table.
        /// </summary>
        /// <param name="rowIndex">Zero-based index where the row should be inserted. Use <see cref="TableView.RowCount"/> to append.</param>
        /// <param name="cells">The row cell values.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table InsertRow(int rowIndex, params string[] cells)
        {
            base.InsertRow(rowIndex, cells ?? new string[0]);
            return this;
        }

        /// <summary>
        /// Inserts a row into the table, formatting cell values with the current culture.
        /// </summary>
        /// <param name="rowIndex">Zero-based index where the row should be inserted. Use <see cref="TableView.RowCount"/> to append.</param>
        /// <param name="cells">The row cell values.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table InsertRow(int rowIndex, params object[] cells)
        {
            base.InsertRow(rowIndex, FormatCells(cells));
            return this;
        }

        /// <summary>
        /// Inserts a row into the table.
        /// </summary>
        /// <param name="rowIndex">Zero-based index where the row should be inserted. Use <see cref="TableView.RowCount"/> to append.</param>
        /// <param name="row">The row cell values.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table InsertRow(int rowIndex, IEnumerable<string> row)
        {
            base.InsertRow(rowIndex, row ?? new string[0]);
            return this;
        }

        /// <summary>
        /// Removes a row from the table.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table RemoveRow(int rowIndex)
        {
            base.RemoveRow(rowIndex);
            return this;
        }

        /// <summary>
        /// Updates a table cell.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="cellData">New cell text.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table UpdateCell(int rowIndex, int columnIndex, string cellData)
        {
            base.UpdateCell(rowIndex, columnIndex, cellData ?? string.Empty);
            return this;
        }

        /// <summary>
        /// Updates a table cell, formatting the value with the current culture.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="cellData">New cell value.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table UpdateCell(int rowIndex, int columnIndex, object cellData)
        {
            string value = FormatCellValue(cellData);
            return UpdateCell(rowIndex, columnIndex, value);
        }

        /// <summary>
        /// Updates a table cell from renderable content, preserving markup styling when supported.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="cellData">Renderable content used as the new cell value.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table UpdateCell(int rowIndex, int columnIndex, IConsoulRenderable cellData)
        {
            if (cellData == null)
            {
                throw new ArgumentNullException(nameof(cellData));
            }

            return UpdateCell(rowIndex, columnIndex, FormatCellValue(cellData));
        }

        /// <summary>
        /// Adds a row to the table.
        /// </summary>
        /// <param name="row">The row cell values.</param>
        /// <param name="renderAfterAdding">If <c>true</c>, renders the table after adding the row.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public new Table AddRow(IEnumerable<string> row, bool renderAfterAdding = false)
        {
            base.AddRow(row ?? new string[0], renderAfterAdding);
            return this;
        }

        /// <summary>
        /// Adds multiple rows to the table.
        /// </summary>
        /// <param name="rows">Rows to add.</param>
        /// <param name="renderAfterAdding">If <c>true</c>, renders the table after each row is added.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public new Table AddRows(IEnumerable<IEnumerable<string>> rows, bool renderAfterAdding = false)
        {
            base.AddRows(rows ?? new string[0][], renderAfterAdding);
            return this;
        }

        /// <summary>
        /// Adds model-backed rows by projecting each item into table cells.
        /// </summary>
        /// <typeparam name="T">The model type to project into table rows.</typeparam>
        /// <param name="items">The model items to append as rows.</param>
        /// <param name="cellSelector">The first selector that produces a cell value for each row.</param>
        /// <param name="additionalCellSelectors">Additional selectors that produce cell values for each row.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddRows<T>(IEnumerable<T> items, Func<T, object> cellSelector, params Func<T, object>[] additionalCellSelectors)
        {
            return AddRows(items, false, cellSelector, additionalCellSelectors);
        }

        /// <summary>
        /// Adds model-backed rows by projecting each item into table cells.
        /// </summary>
        /// <typeparam name="T">The model type to project into table rows.</typeparam>
        /// <param name="items">The model items to append as rows.</param>
        /// <param name="renderAfterAdding">If <c>true</c>, renders the table after each row is added.</param>
        /// <param name="cellSelector">The first selector that produces a cell value for each row.</param>
        /// <param name="additionalCellSelectors">Additional selectors that produce cell values for each row.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public Table AddRows<T>(IEnumerable<T> items, bool renderAfterAdding, Func<T, object> cellSelector, params Func<T, object>[] additionalCellSelectors)
        {
            if (cellSelector == null)
            {
                throw new ArgumentNullException(nameof(cellSelector));
            }

            if (additionalCellSelectors == null)
            {
                throw new ArgumentNullException(nameof(additionalCellSelectors));
            }

            var cellSelectors = new Func<T, object>[additionalCellSelectors.Length + 1];
            cellSelectors[0] = cellSelector;

            for (int index = 0; index < additionalCellSelectors.Length; index++)
            {
                Func<T, object> selector = additionalCellSelectors[index];
                if (selector == null)
                {
                    throw new ArgumentNullException(nameof(additionalCellSelectors), "Cell selectors cannot contain null values.");
                }

                cellSelectors[index + 1] = selector;
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                var row = new string[cellSelectors.Length];
                for (int index = 0; index < cellSelectors.Length; index++)
                {
                    row[index] = FormatCellValue(cellSelectors[index](item));
                }

                AddRow(row, renderAfterAdding);
            }

            return this;
        }

        /// <summary>
        /// Sets the alignment for a single table column.
        /// </summary>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="alignment">Alignment to apply to the column.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public new Table SetColumnAlignment(int columnIndex, TableCellAlignment alignment)
        {
            base.SetColumnAlignment(columnIndex, alignment);
            return this;
        }

        /// <summary>
        /// Sets alignments for multiple table columns.
        /// </summary>
        /// <param name="alignments">Column alignments indexed by cell position.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public new Table SetColumnAlignments(params TableCellAlignment[] alignments)
        {
            base.SetColumnAlignments(alignments);
            return this;
        }

        /// <summary>
        /// Sets the preferred width for a single table column.
        /// </summary>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="width">Preferred column width, or <see langword="null"/> to use content-based allocation.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public new Table SetColumnWidth(int columnIndex, int? width)
        {
            base.SetColumnWidth(columnIndex, width);
            return this;
        }

        /// <summary>
        /// Sets footer text for a single table column.
        /// </summary>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="footer">Footer text for the column.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public new Table SetColumnFooter(int columnIndex, string footer)
        {
            base.SetColumnFooter(columnIndex, footer);
            return this;
        }

        /// <summary>
        /// Sets padding for a single table column.
        /// </summary>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="padding">Padding to apply inside the column, or <see langword="null"/> to use the default cell padding.</param>
        /// <returns>The current <see cref="Table"/> instance.</returns>
        public new Table SetColumnPadding(int columnIndex, Padding padding)
        {
            base.SetColumnPadding(columnIndex, padding);
            return this;
        }

        private static IEnumerable<string> FormatCells(object[] cells)
        {
            if (cells == null)
            {
                return new string[0];
            }

            var values = new string[cells.Length];
            for (int index = 0; index < cells.Length; index++)
            {
                values[index] = FormatCellValue(cells[index]);
            }

            return values;
        }

        internal static string FormatCellValue(object cell)
        {
            if (cell == null)
            {
                return string.Empty;
            }

            var markup = cell as MarkupEntry;
            if (markup != null)
            {
                return markup.Markup ?? string.Empty;
            }

            var text = cell as TextBlockEntry;
            if (text != null)
            {
                return NormalizeCellText(text.Content);
            }

            var renderable = cell as IConsoulRenderable;
            if (renderable != null)
            {
                return CaptureCell(renderable);
            }

            return NormalizeCellText(Convert.ToString(cell, CultureInfo.CurrentCulture));
        }

        private static bool HasVisibleValues(IEnumerable values)
        {
            if (values == null)
            {
                return false;
            }

            var singleValue = values as string;
            if (singleValue != null)
            {
                return !string.IsNullOrWhiteSpace(FormatCellValue(singleValue));
            }

            foreach (object value in values)
            {
                if (!string.IsNullOrWhiteSpace(FormatCellValue(value)))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureKeyValueColumns(string keyHeader, string valueHeader)
        {
            if (HeaderCount <= 0)
            {
                AddColumn(keyHeader ?? "Key");
                AddColumn(valueHeader ?? "Value");
                return;
            }

            if (HeaderCount == 1)
            {
                AddColumn(valueHeader ?? "Value");
            }
        }

        private static string CaptureCell(IConsoulRenderable renderable)
        {
            string captured = Consoul.Capture(renderable, Consoul.TerminalWidth, 20);
            return NormalizeCellText(captured);
        }

        private static string NormalizeCellText(string value)
        {
            return (value ?? string.Empty).Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ').Trim();
        }

        private void ApplyColumnConfiguration(int columnIndex, TableColumn column)
        {
            SetHeader(columnIndex, column.ResolveHeader());
            SetColumnAlignment(columnIndex, column.Alignment ?? TableCellAlignment.Default);
            SetColumnWidth(columnIndex, column.Width);
            SetColumnPadding(columnIndex, column.ResolvePadding());
            SetColumnFooter(columnIndex, column.ResolveFooter());
            SetHeaderStyle(columnIndex, column.ResolveHeaderCellStyle());
            SetColumnFooterStyle(columnIndex, column.ResolveFooterCellStyle());
        }

        private Table ApplyTitle(string title, Justify? justification, Style style)
        {
            TableRenderOptions.Title = title;

            if (justification.HasValue)
            {
                TableRenderOptions.TitleAlignment = JustifyMap.ToTextBlockAlignment(justification.Value);
            }

            if (style != null)
            {
                TableRenderOptions.TitleScheme = Scheme(style, TableRenderOptions.TitleScheme);
                TableRenderOptions.TitleDecoration = style.Decoration;
                TableRenderOptions.TitleForegroundRgb = style.ForegroundRgb;
                TableRenderOptions.TitleBackgroundRgb = style.BackgroundRgb;
            }

            return this;
        }

        private Table ApplyCaption(string caption, Justify? justification, Style style)
        {
            TableRenderOptions.Caption = caption;

            if (justification.HasValue)
            {
                TableRenderOptions.CaptionAlignment = JustifyMap.ToTextBlockAlignment(justification.Value);
            }

            if (style != null)
            {
                TableRenderOptions.CaptionScheme = Scheme(style, TableRenderOptions.CaptionScheme);
                TableRenderOptions.CaptionDecoration = style.Decoration;
                TableRenderOptions.CaptionForegroundRgb = style.ForegroundRgb;
                TableRenderOptions.CaptionBackgroundRgb = style.BackgroundRgb;
            }

            return this;
        }

        private void ApplyLineStyle(Style style)
        {
            TableRenderOptions.Lines.Color = Scheme(style, TableRenderOptions.Lines.Color);
            TableRenderOptions.Lines.Decoration = style?.Decoration ?? Decoration.None;
            TableRenderOptions.Lines.ForegroundRgb = style?.ForegroundRgb;
            TableRenderOptions.Lines.BackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyHeaderStyle(Style style)
        {
            TableRenderOptions.HeaderScheme = Scheme(style, TableRenderOptions.HeaderScheme);
            TableRenderOptions.HeaderDecoration = style?.Decoration ?? Decoration.None;
            TableRenderOptions.HeaderForegroundRgb = style?.ForegroundRgb;
            TableRenderOptions.HeaderBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyContentStyles(Style style)
        {
            ApplyContentStyle1(style);
            ApplyContentStyle2(style);
        }

        private void ApplyContentStyle1(Style style)
        {
            TableRenderOptions.ContentScheme1 = Scheme(style, TableRenderOptions.ContentScheme1);
            TableRenderOptions.ContentDecoration1 = style?.Decoration ?? Decoration.None;
            TableRenderOptions.ContentForegroundRgb1 = style?.ForegroundRgb;
            TableRenderOptions.ContentBackgroundRgb1 = style?.BackgroundRgb;
        }

        private void ApplyContentStyle2(Style style)
        {
            TableRenderOptions.ContentScheme2 = Scheme(style, TableRenderOptions.ContentScheme2);
            TableRenderOptions.ContentDecoration2 = style?.Decoration ?? Decoration.None;
            TableRenderOptions.ContentForegroundRgb2 = style?.ForegroundRgb;
            TableRenderOptions.ContentBackgroundRgb2 = style?.BackgroundRgb;
        }

        private void ApplyFooterStyle(Style style)
        {
            TableRenderOptions.FooterScheme = Scheme(style, TableRenderOptions.FooterScheme);
            TableRenderOptions.FooterDecoration = style?.Decoration ?? Decoration.None;
            TableRenderOptions.FooterForegroundRgb = style?.ForegroundRgb;
            TableRenderOptions.FooterBackgroundRgb = style?.BackgroundRgb;
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme Scheme(SpectreColor color)
        {
            return Scheme(color.ToConsoleColor());
        }

        private static ColorScheme Scheme(RgbColor color)
        {
            return Scheme(color.ToConsoleColor());
        }

        private static ColorScheme Scheme(Style style, ColorScheme fallback)
        {
            return (style ?? Style.Plain).ToColorScheme(fallback);
        }
    }
}
