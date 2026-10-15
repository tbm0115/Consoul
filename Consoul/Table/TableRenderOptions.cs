using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    internal struct TableRenderStyle
    {
        public TableRenderStyle(ColorScheme scheme, Decoration decoration = Decoration.None, RgbColor? foregroundRgb = null, RgbColor? backgroundRgb = null)
        {
            Scheme = scheme;
            Decoration = decoration;
            ForegroundRgb = foregroundRgb;
            BackgroundRgb = backgroundRgb;
        }

        public ColorScheme Scheme { get; }

        public Decoration Decoration { get; }

        public RgbColor? ForegroundRgb { get; }

        public RgbColor? BackgroundRgb { get; }

        public bool HasAnsiStyle => Decoration != Decoration.None || ForegroundRgb.HasValue || BackgroundRgb.HasValue;

        public static TableRenderStyle From(ColorScheme scheme)
        {
            return new TableRenderStyle(scheme);
        }

        public TableRenderStyle Apply(Style style)
        {
            if (style == null)
            {
                return this;
            }

            return new TableRenderStyle(
                style.ToColorScheme(Scheme),
                style.Decoration,
                style.ForegroundRgb,
                style.BackgroundRgb);
        }
    }

    internal static class TableRenderStyleWriter
    {
        public static void Write(string text, TableRenderStyle? style, bool writeLine = true)
        {
            if (!style.HasValue)
            {
                Consoul.Write(text, writeLine: writeLine);
                return;
            }

            TableRenderStyle resolved = style.Value;
            if (!resolved.HasAnsiStyle)
            {
                Consoul.Write(text, resolved.Scheme.Color, resolved.Scheme.BackgroundColor, writeLine);
                return;
            }

            TerminalLinkWriter.WriteText(
                text,
                resolved.Scheme,
                null,
                resolved.Decoration,
                resolved.ForegroundRgb,
                resolved.BackgroundRgb);

            if (writeLine)
            {
                Consoul.Write(string.Empty);
            }
        }
    }

    /// <summary>
    /// Options for how a <see cref="TableView"/> is rendered.
    /// </summary>
    public class TableRenderOptions {
        /// <summary>
        /// Gets or sets whether a leading choice column is included when normalizing table layout.
        /// </summary>
        public bool IncludeChoices { get; set; } = false;

        /// <summary>
        /// Render options for the row and column lines.
        /// </summary>
        public TableLineDisplayOptions Lines { get; set; } = new TableLineDisplayOptions();

        /// <summary>
        /// Color for the header.
        /// </summary>
        public ColorScheme HeaderScheme { get; set; } = RenderOptions.DefaultScheme;

        /// <summary>
        /// Gets or sets the text decoration used for header cells on ANSI-capable terminals.
        /// </summary>
        public Decoration HeaderDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for header cells on true-color terminals.
        /// </summary>
        public RgbColor? HeaderForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for header cells on true-color terminals.
        /// </summary>
        public RgbColor? HeaderBackgroundRgb { get; set; }

        /// <summary>
        /// Text color option 1.
        /// </summary>
        public ColorScheme ContentScheme1 { get; set; } = RenderOptions.DefaultScheme;

        /// <summary>
        /// Gets or sets the text decoration used for primary content cells on ANSI-capable terminals.
        /// </summary>
        public Decoration ContentDecoration1 { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for primary content cells on true-color terminals.
        /// </summary>
        public RgbColor? ContentForegroundRgb1 { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for primary content cells on true-color terminals.
        /// </summary>
        public RgbColor? ContentBackgroundRgb1 { get; set; }

        /// <summary>
        /// Text color option 2.
        /// </summary>
        public ColorScheme ContentScheme2 { get; set; } = RenderOptions.DefaultScheme;

        /// <summary>
        /// Gets or sets the text decoration used for alternate content cells on ANSI-capable terminals.
        /// </summary>
        public Decoration ContentDecoration2 { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for alternate content cells on true-color terminals.
        /// </summary>
        public RgbColor? ContentForegroundRgb2 { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for alternate content cells on true-color terminals.
        /// </summary>
        public RgbColor? ContentBackgroundRgb2 { get; set; }

        /// <summary>
        /// Text color for row(s) that are selected.
        /// </summary>
        public ColorScheme SelectionScheme { get; set; } = new ColorScheme() { Color = ConsoleColor.Green, BackgroundColor = ConsoleColor.DarkGreen };

        /// <summary>
        /// Gets or sets the color scheme for the row currently under the navigation cursor.
        /// </summary>
        public ColorScheme HighlightedScheme { get; set; } = RenderOptions.OptionScheme;

        /// <summary>
        /// Gets or sets a value indicating whether keyboard navigation help is rendered below the table.
        /// </summary>
        public bool ShowFooterHelp { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether configured column footer cells are rendered.
        /// </summary>
        public bool ShowFooters { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether header cells are rendered.
        /// </summary>
        public bool ShowHeaders { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether one-based row numbers are rendered as the first table column.
        /// </summary>
        public bool ShowRowNumbers { get; set; }

        /// <summary>
        /// Gets or sets the header text rendered above the row number column.
        /// </summary>
        public string RowNumberHeader { get; set; } = "#";

        /// <summary>
        /// Gets or sets the alignment used for the row number column.
        /// </summary>
        public TableCellAlignment RowNumberAlignment { get; set; } = TableCellAlignment.Right;

        /// <summary>
        /// Gets or sets the default alignment used for header cells.
        /// </summary>
        public TableCellAlignment HeaderAlignment { get; set; } = TableCellAlignment.Left;

        /// <summary>
        /// Gets or sets the default alignment used for content cells.
        /// </summary>
        public TableCellAlignment ContentAlignment { get; set; } = TableCellAlignment.Left;

        /// <summary>
        /// Gets or sets an optional title rendered above the table when no render message is supplied.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the alignment used for the table title.
        /// </summary>
        public TextBlockAlignment TitleAlignment { get; set; } = TextBlockAlignment.Center;

        /// <summary>
        /// Gets or sets the color scheme used for the table title.
        /// </summary>
        public ColorScheme TitleScheme { get; set; } = RenderOptions.PromptScheme;

        /// <summary>
        /// Gets or sets the text decoration used for the table title on ANSI-capable terminals.
        /// </summary>
        public Decoration TitleDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for the table title on true-color terminals.
        /// </summary>
        public RgbColor? TitleForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for the table title on true-color terminals.
        /// </summary>
        public RgbColor? TitleBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets an optional caption rendered below the table.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the alignment used for the table caption.
        /// </summary>
        public TextBlockAlignment CaptionAlignment { get; set; } = TextBlockAlignment.Left;

        /// <summary>
        /// Gets or sets the color scheme used for the table caption.
        /// </summary>
        public ColorScheme CaptionScheme { get; set; } = RenderOptions.SubnoteScheme;

        /// <summary>
        /// Gets or sets the text decoration used for the table caption on ANSI-capable terminals.
        /// </summary>
        public Decoration CaptionDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for the table caption on true-color terminals.
        /// </summary>
        public RgbColor? CaptionForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for the table caption on true-color terminals.
        /// </summary>
        public RgbColor? CaptionBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for column footer cells.
        /// </summary>
        public ColorScheme FooterScheme { get; set; } = RenderOptions.SubnoteScheme;

        /// <summary>
        /// Gets or sets the text decoration used for column footer cells on ANSI-capable terminals.
        /// </summary>
        public Decoration FooterDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for column footer cells on true-color terminals.
        /// </summary>
        public RgbColor? FooterForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for column footer cells on true-color terminals.
        /// </summary>
        public RgbColor? FooterBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets a message rendered inside the table body when no rows are available.
        /// </summary>
        public string EmptyMessage { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for <see cref="EmptyMessage"/>.
        /// </summary>
        public ColorScheme EmptyMessageScheme { get; set; } = RenderOptions.SubnoteScheme;

        /// <summary>
        /// Gets or sets the text decoration used for <see cref="EmptyMessage"/> on ANSI-capable terminals.
        /// </summary>
        public Decoration EmptyMessageDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for <see cref="EmptyMessage"/> on true-color terminals.
        /// </summary>
        public RgbColor? EmptyMessageForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for <see cref="EmptyMessage"/> on true-color terminals.
        /// </summary>
        public RgbColor? EmptyMessageBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets a summary rendered below the table.
        /// </summary>
        public string SummaryText { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for <see cref="SummaryText"/>.
        /// </summary>
        public ColorScheme SummaryScheme { get; set; } = RenderOptions.SubnoteScheme;

        /// <summary>
        /// Gets or sets the text decoration used for <see cref="SummaryText"/> on ANSI-capable terminals.
        /// </summary>
        public Decoration SummaryDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for <see cref="SummaryText"/> on true-color terminals.
        /// </summary>
        public RgbColor? SummaryForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for <see cref="SummaryText"/> on true-color terminals.
        /// </summary>
        public RgbColor? SummaryBackgroundRgb { get; set; }

        /// <summary>
        /// Left padding.
        /// </summary>
        public string LeftPad { get; private set; }

        /// <summary>
        /// Right padding.
        /// </summary>
        public string RightPad { get; private set; }

        /// <summary>
        /// Width of column.
        /// </summary>
        public int? ColumnSize { get; private set; }

        /// <summary>
        /// Character used for whitespace
        /// </summary>
        public char WhitespaceCharacater { get; set; } = (char)0x2588;

        /// <summary>
        /// Gets or sets the character used for whitespace between table cell content and cell boundaries.
        /// </summary>
        public char WhitespaceCharacter
        {
            get => WhitespaceCharacater;
            set => WhitespaceCharacater = value;
        }

        private int? _maximumTableWidth;
        private int? _normalizedMaximumTableWidth;

        /// <summary>
        /// Gets or sets the requested maximum width of the table.
        /// </summary>
        public int? MaximumTableWidth
        {
            get => _maximumTableWidth ?? _normalizedMaximumTableWidth;
            set
            {
                if (value.HasValue && value.Value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Maximum table width must be greater than zero.");
                }

                _maximumTableWidth = value;
                _normalizedMaximumTableWidth = null;
                IsNormalized = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether table columns should expand to consume unused available width.
        /// </summary>
        public bool Expand { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether <see cref="Normalize(IEnumerable{IEnumerable{string}})"/> has been called.
        /// </summary>
        public bool IsNormalized { get; private set; } = false;

        private decimal _tableWidthPercentage { get; set; } = 0.8m;
        /// <summary>
        /// Ratio of table width to the console width.
        /// </summary>
        public decimal TableWidthPercentage {
            get{
                return _tableWidthPercentage;
            }
            set{
                if (value > 1 || value < 0)
                    throw new ArgumentOutOfRangeException("Value must be between 0.0 and 1.0.");
                _tableWidthPercentage = value;
                _normalizedMaximumTableWidth = null;
                IsNormalized = false;
            }
        }

        /// <summary>
        /// Constructs a new instance of the render options.
        /// </summary>
        public TableRenderOptions() { }

        internal TableRenderStyle ResolveHeaderStyle()
        {
            return new TableRenderStyle(HeaderScheme, HeaderDecoration, HeaderForegroundRgb, HeaderBackgroundRgb);
        }

        internal TableRenderStyle ResolveContentStyle1()
        {
            return new TableRenderStyle(ContentScheme1, ContentDecoration1, ContentForegroundRgb1, ContentBackgroundRgb1);
        }

        internal TableRenderStyle ResolveContentStyle2()
        {
            return new TableRenderStyle(ContentScheme2, ContentDecoration2, ContentForegroundRgb2, ContentBackgroundRgb2);
        }

        internal TableRenderStyle ResolveFooterStyle()
        {
            return new TableRenderStyle(FooterScheme, FooterDecoration, FooterForegroundRgb, FooterBackgroundRgb);
        }

        internal int GetMaximumTableWidth()
        {
            int requestedWidth = _maximumTableWidth ?? _normalizedMaximumTableWidth ?? Consoul.ConsoleBufferWidth;
            return Math.Max(1, Math.Min(requestedWidth, Consoul.ConsoleBufferWidth));
        }

        /// <summary>
        /// Calculates table width, margins, and column sizing for the provided content.
        /// </summary>
        /// <param name="contents">Rows used to infer the normalized table layout.</param>
        public void Normalize(IEnumerable<IEnumerable<string>> contents)
        {
            int requestedWidth = _maximumTableWidth ?? (int)(Consoul.ConsoleBufferWidth * TableWidthPercentage);
            _normalizedMaximumTableWidth = Math.Max(1, Math.Min(requestedWidth, Consoul.ConsoleBufferWidth));
            int widthRemainder = Consoul.ConsoleBufferWidth - _normalizedMaximumTableWidth.Value;
            int marginLeft, marginRight;
            marginRight = widthRemainder / 2;
            marginLeft = marginRight;
            if (widthRemainder % 2 != 0)
                marginLeft = (widthRemainder + 1) / 2;

            int columnCount = 0;
            if (contents != null && contents.Any())
                columnCount = contents.Max(o => o.Count()) + (IncludeChoices ? 1 : 0);
            
            if (columnCount == 0)
                columnCount = 1;

            ColumnSize = _normalizedMaximumTableWidth.Value / columnCount;
            int maximumColumnSize = 0;
            if (contents != null && contents.Any())
                maximumColumnSize = contents.SelectMany(o => o).Max(o => o.Length);

            int minimumTableWidth = (maximumColumnSize + 3) * columnCount;

            LeftPad = new string(' ', marginLeft);
            RightPad = new string(' ', marginRight);

            IsNormalized = true;
        }

        /// <summary>
        /// Contains settings for how <see cref="TableView"/> lines are rendered.
        /// </summary>
        public class TableLineDisplayOptions {
            /// <summary>
            /// Character used to render horizontal lines.
            /// </summary>
            public char HorizontalCharacter { get; set; } = '─';

            /// <summary>
            /// Character used to render vertical lines.
            /// </summary>
            public char VerticalCharacter { get; set; } = '│';

            /// <summary>
            /// Gets or sets the table border preset. The default preserves the legacy repeated-line rendering.
            /// </summary>
            public TableBorderStyle BorderStyle { get; set; } = TableBorderStyle.Simple;

            /// <summary>
            /// Flags whether to render a horizontal line for the header of the table.
            /// </summary>
            public bool HeaderHorizontal { get; set; } = true;

            /// <summary>
            /// Flags whether to render vertical lines for the header of the table.
            /// </summary>
            public bool HeaderVertical { get; set; } = true;

            /// <summary>
            /// Flags whether to render horizontal lines for the contents of the table.
            /// </summary>
            public bool ContentHorizontal { get; set; } = true;

            /// <summary>
            /// Flags whether to render vertical lines for the contents of the table.
            /// </summary>
            public bool ContentVertical { get; set; } = true;

            /// <summary>
            /// Color of the lines.
            /// </summary>
            public ColorScheme Color { get; set; } = RenderOptions.SubnoteScheme;

            /// <summary>
            /// Gets or sets the text decoration used for table border and separator lines on ANSI-capable terminals.
            /// </summary>
            public Decoration Decoration { get; set; }

            /// <summary>
            /// Gets or sets the original RGB foreground used for table border and separator lines on true-color terminals.
            /// </summary>
            public RgbColor? ForegroundRgb { get; set; }

            /// <summary>
            /// Gets or sets the original RGB background used for table border and separator lines on true-color terminals.
            /// </summary>
            public RgbColor? BackgroundRgb { get; set; }

            /// <summary>
            /// Constructs a new instance of <see cref="TableLineDisplayOptions"/>
            /// </summary>
            public TableLineDisplayOptions() { }

            internal TableRenderStyle ResolveStyle()
            {
                return new TableRenderStyle(Color, Decoration, ForegroundRgb, BackgroundRgb);
            }

            internal bool UsesPresetBorder()
            {
                return BorderStyle != TableBorderStyle.Simple;
            }

            internal bool HasHorizontalLines()
            {
                return HeaderHorizontal || ContentHorizontal;
            }

            internal TableBorderCharacters GetBorderCharacters()
            {
                switch (BorderStyle)
                {
                    case TableBorderStyle.Square:
                        return new TableBorderCharacters('┌', '┬', '┐', '├', '┼', '┤', '└', '┴', '┘', '│', '─');
                    case TableBorderStyle.Rounded:
                        return new TableBorderCharacters('╭', '┬', '╮', '├', '┼', '┤', '╰', '┴', '╯', '│', '─');
                    case TableBorderStyle.Ascii:
                        return new TableBorderCharacters('+', '+', '+', '+', '+', '+', '+', '+', '+', '|', '-');
                    case TableBorderStyle.Heavy:
                        return new TableBorderCharacters('┏', '┳', '┓', '┣', '╋', '┫', '┗', '┻', '┛', '┃', '━');
                    case TableBorderStyle.Double:
                        return new TableBorderCharacters('╔', '╦', '╗', '╠', '╬', '╣', '╚', '╩', '╝', '║', '═');
                    default:
                        return new TableBorderCharacters(
                            HorizontalCharacter,
                            HorizontalCharacter,
                            HorizontalCharacter,
                            HorizontalCharacter,
                            HorizontalCharacter,
                            HorizontalCharacter,
                            HorizontalCharacter,
                            HorizontalCharacter,
                            HorizontalCharacter,
                            VerticalCharacter,
                            HorizontalCharacter);
                }
            }
        }

        internal struct TableBorderCharacters
        {
            public TableBorderCharacters(
                char topLeft,
                char topJoin,
                char topRight,
                char middleLeft,
                char middleJoin,
                char middleRight,
                char bottomLeft,
                char bottomJoin,
                char bottomRight,
                char vertical,
                char horizontal)
            {
                TopLeft = topLeft;
                TopJoin = topJoin;
                TopRight = topRight;
                MiddleLeft = middleLeft;
                MiddleJoin = middleJoin;
                MiddleRight = middleRight;
                BottomLeft = bottomLeft;
                BottomJoin = bottomJoin;
                BottomRight = bottomRight;
                Vertical = vertical;
                Horizontal = horizontal;
            }

            public char TopLeft { get; }

            public char TopJoin { get; }

            public char TopRight { get; }

            public char MiddleLeft { get; }

            public char MiddleJoin { get; }

            public char MiddleRight { get; }

            public char BottomLeft { get; }

            public char BottomJoin { get; }

            public char BottomRight { get; }

            public char Vertical { get; }

            public char Horizontal { get; }
        }
    }
}
