using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Describes a manually configured table column for Spectre-style table builder snippets.
    /// </summary>
    public sealed class TableColumn
    {
        private string _header;
        private TableColumnFooter _footer = TableColumnFooter.Empty;
        private int? _width;
        private Padding _padding;
        private bool _noWrap;
        private TableCellAlignment? _alignment;
        private Style _headerStyle;
        private Style _footerStyle;
        private Action<TableColumn> _changed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableColumn"/> class.
        /// </summary>
        /// <param name="header">The column header text.</param>
        public TableColumn(string header)
        {
            _header = header ?? string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableColumn"/> class with captured renderable header content.
        /// </summary>
        /// <param name="header">The renderable component used to produce the header text.</param>
        public TableColumn(IConsoulRenderable header)
        {
            _header = Table.FormatCellValue(header);
        }

        /// <summary>
        /// Gets or sets the column header text.
        /// </summary>
        public string Header
        {
            get => _header;
            set
            {
                _header = value ?? string.Empty;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets optional footer content rendered below the column values.
        /// </summary>
        public TableColumnFooter Footer
        {
            get => _footer;
            set
            {
                _footer = value ?? TableColumnFooter.Empty;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets an optional style applied to this column's header cell.
        /// </summary>
        public Style HeaderCellStyle
        {
            get => _headerStyle;
            set
            {
                _headerStyle = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets an optional style applied to this column's footer cell.
        /// </summary>
        public Style FooterCellStyle
        {
            get => _footerStyle;
            set
            {
                _footerStyle = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the preferred column width. When <see langword="null"/>, the table allocates width from content.
        /// </summary>
        public int? Width
        {
            get => _width;
            set
            {
                _width = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets optional padding applied inside this column.
        /// </summary>
        public Padding Padding
        {
            get => _padding;
            set
            {
                _padding = value?.Clone();
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether wrapping should be avoided for this column.
        /// </summary>
        /// <remarks>
        /// Consoul table cells already render as bounded single-line cells, so this flag is preserved for compatibility.
        /// </remarks>
        public bool NoWrap
        {
            get => _noWrap;
            set
            {
                _noWrap = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the column alignment. When <see langword="null"/>, the table default alignment is used.
        /// </summary>
        public TableCellAlignment? Alignment
        {
            get => _alignment;
            set
            {
                _alignment = value;
                OnChanged();
            }
        }

        internal void AttachChangeHandler(Action<TableColumn> changed)
        {
            _changed = changed;
        }

        internal Padding ResolvePadding()
        {
            return Padding?.Clone();
        }

        internal string ResolveHeader()
        {
            return Header ?? string.Empty;
        }

        internal string ResolveFooter()
        {
            return Footer?.Text ?? string.Empty;
        }

        internal Style ResolveHeaderCellStyle()
        {
            return HeaderCellStyle;
        }

        internal Style ResolveFooterCellStyle()
        {
            return FooterCellStyle;
        }

        private void OnChanged()
        {
            _changed?.Invoke(this);
        }

        internal static string Capture(IConsoulRenderable renderable, int width = 80, int height = 20)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            var markup = renderable as MarkupEntry;
            if (markup != null)
            {
                return Table.FormatCellValue(markup);
            }

            var textBlock = renderable as TextBlockEntry;
            if (textBlock != null)
            {
                return Table.FormatCellValue(textBlock);
            }

            return RenderActionContent.Capture(renderable, width, height).Trim();
        }
    }

    /// <summary>
    /// Provides fluent helpers for <see cref="TableColumn"/> instances.
    /// </summary>
    public static class TableColumnExtensions
    {
        /// <summary>
        /// Sets the column header text.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="header">The column header text.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn Header(this TableColumn column, string header)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.Header = header ?? string.Empty;
            return column;
        }

        /// <summary>
        /// Sets the column header from captured renderable content.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="header">The renderable component used to produce the header text.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn Header(this TableColumn column, IConsoulRenderable header)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.Header = TableColumn.Capture(header);
            return column;
        }

        /// <summary>
        /// Sets the column footer text.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="footer">The column footer text.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn Footer(this TableColumn column, string footer)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.Footer = footer ?? string.Empty;
            return column;
        }

        /// <summary>
        /// Sets the column footer from captured renderable content.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="footer">The renderable component used to produce the footer text.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn Footer(this TableColumn column, IConsoulRenderable footer)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.Footer = TableColumnFooter.FromRenderable(footer);
            return column;
        }

        /// <summary>
        /// Sets the header cell foreground color.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="color">The header foreground color.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn HeaderColor(this TableColumn column, ConsoleColor color)
        {
            return HeaderStyle(column, new Style(color));
        }

        /// <summary>
        /// Sets the header cell foreground color.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="color">The header foreground color.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn HeaderColor(this TableColumn column, SpectreColor color)
        {
            return HeaderStyle(column, new Style(color));
        }

        /// <summary>
        /// Sets the header cell foreground color.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="color">The header foreground color.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn HeaderColor(this TableColumn column, RgbColor color)
        {
            return HeaderStyle(column, new Style(color));
        }

        /// <summary>
        /// Sets the header cell color from Spectre-style style text.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn HeaderColor(this TableColumn column, string style)
        {
            return HeaderStyle(column, Style.Parse(style));
        }

        /// <summary>
        /// Sets the header cell color from a reusable style object.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn HeaderColor(this TableColumn column, Style style)
        {
            return HeaderStyle(column, style);
        }

        /// <summary>
        /// Sets the header cell style from Spectre-style style text.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="style">Style text such as <c>bold yellow</c> or <c>#22c55e on navy</c>.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn HeaderStyle(this TableColumn column, string style)
        {
            return HeaderStyle(column, Style.Parse(style));
        }

        /// <summary>
        /// Sets the header cell style.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn HeaderStyle(this TableColumn column, Style style)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.HeaderCellStyle = style;
            return column;
        }

        /// <summary>
        /// Sets the footer cell foreground color.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="color">The footer foreground color.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn FooterColor(this TableColumn column, ConsoleColor color)
        {
            return FooterStyle(column, new Style(color));
        }

        /// <summary>
        /// Sets the footer cell foreground color.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="color">The footer foreground color.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn FooterColor(this TableColumn column, SpectreColor color)
        {
            return FooterStyle(column, new Style(color));
        }

        /// <summary>
        /// Sets the footer cell foreground color.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="color">The footer foreground color.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn FooterColor(this TableColumn column, RgbColor color)
        {
            return FooterStyle(column, new Style(color));
        }

        /// <summary>
        /// Sets the footer cell color from Spectre-style style text.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn FooterColor(this TableColumn column, string style)
        {
            return FooterStyle(column, Style.Parse(style));
        }

        /// <summary>
        /// Sets the footer cell color from a reusable style object.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn FooterColor(this TableColumn column, Style style)
        {
            return FooterStyle(column, style);
        }

        /// <summary>
        /// Sets the footer cell style from Spectre-style style text.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="style">Style text such as <c>dim grey</c> or <c>#22c55e on navy</c>.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn FooterStyle(this TableColumn column, string style)
        {
            return FooterStyle(column, Style.Parse(style));
        }

        /// <summary>
        /// Sets the footer cell style.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn FooterStyle(this TableColumn column, Style style)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.FooterCellStyle = style;
            return column;
        }

        /// <summary>
        /// Sets the preferred column width.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="width">The preferred column width, or <see langword="null"/> to use content-based allocation.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn Width(this TableColumn column, int? width)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            if (width.HasValue && width.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Column width must be greater than zero.");
            }

            column.Width = width;
            return column;
        }

        /// <summary>
        /// Sets the column padding.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="padding">The padding to apply inside the column.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn Padding(this TableColumn column, Padding padding)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.Padding = padding?.Clone();
            return column;
        }

        /// <summary>
        /// Sets column padding.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="left">Padding added to the left side.</param>
        /// <param name="right">Padding added to the right side.</param>
        /// <param name="top">Padding accepted for compatibility.</param>
        /// <param name="bottom">Padding accepted for compatibility.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn Padding(this TableColumn column, int left, int right, int top = 0, int bottom = 0)
        {
            return Padding(column, new Padding(left, top, right, bottom));
        }

        /// <summary>
        /// Sets the left column padding.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="left">Padding added to the left side.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn PadLeft(this TableColumn column, int left)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            Padding padding = column.Padding?.Clone() ?? new Padding(1, 0, 0, 0);
            padding.Left = Math.Max(0, left);
            column.Padding = padding;
            return column;
        }

        /// <summary>
        /// Sets the right column padding.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="right">Padding added to the right side.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn PadRight(this TableColumn column, int right)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            Padding padding = column.Padding?.Clone() ?? new Padding(1, 0, 0, 0);
            padding.Right = Math.Max(0, right);
            column.Padding = padding;
            return column;
        }

        /// <summary>
        /// Sets whether wrapping should be avoided for this column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="enabled">Whether no-wrap behavior should be requested.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn NoWrap(this TableColumn column, bool enabled = true)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.NoWrap = enabled;
            return column;
        }

        /// <summary>
        /// Left-aligns values in this column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn LeftAligned(this TableColumn column)
        {
            return Justify(column, TableCellAlignment.Left);
        }

        /// <summary>
        /// Left-aligns values in this column using text-builder justification vocabulary.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn LeftJustified(this TableColumn column)
        {
            return LeftAligned(column);
        }

        /// <summary>
        /// Centers values in this column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn Centered(this TableColumn column)
        {
            return Justify(column, TableCellAlignment.Center);
        }

        /// <summary>
        /// Right-aligns values in this column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn RightAligned(this TableColumn column)
        {
            return Justify(column, TableCellAlignment.Right);
        }

        /// <summary>
        /// Right-aligns values in this column using text-builder justification vocabulary.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn RightJustified(this TableColumn column)
        {
            return RightAligned(column);
        }

        /// <summary>
        /// Sets the column alignment.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="alignment">The alignment to use for this column.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn Justify(this TableColumn column, TableCellAlignment alignment)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.Alignment = alignment;
            return column;
        }

        /// <summary>
        /// Sets the column alignment using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="justify">The justification to use for this column.</param>
        /// <returns>The configured <see cref="TableColumn"/> instance.</returns>
        public static TableColumn Justify(this TableColumn column, Justify justify)
        {
            return ApplyJustification(column, JustifyMap.ToTableCellAlignment(justify));
        }

        private static TableColumn ApplyJustification(TableColumn column, TableCellAlignment alignment)
        {
            return Justify(column, alignment);
        }
    }
}
