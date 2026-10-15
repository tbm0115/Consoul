using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders table headers, rows, and separators using normalized table render options.
    /// </summary>
    public class TableRenderer
    {
        /// <summary>
        /// Gets or sets the render options used for table layout and colors.
        /// </summary>
        public TableRenderOptions RenderOptions { get; set; }

        /// <summary>
        /// Initializes a new table renderer.
        /// </summary>
        /// <param name="renderOptions">Options used to render the table.</param>
        public TableRenderer(TableRenderOptions renderOptions = default)
        {
            RenderOptions = renderOptions ?? throw new ArgumentNullException(nameof(renderOptions));
        }

        /// <summary>
        /// Renders a table header row with surrounding separators.
        /// </summary>
        /// <param name="headers">Header cell values.</param>
        /// <param name="rowNumber">Row number used for styling decisions.</param>
        public void RenderHeaders(IEnumerable<string> headers, int rowNumber)
        {
            RenderHorizontalSeparator();
            RenderRow(headers, rowNumber, isHeader: true);
        }

        /// <summary>
        /// Renders a single table row.
        /// </summary>
        /// <param name="row">Cell values to render.</param>
        /// <param name="rowNumber">Zero-based row number used for alternating row styles.</param>
        /// <param name="isHighlighted">Indicates whether the row should use the highlighted color scheme.</param>
        /// <param name="isSelected">Indicates whether the row should use the selected color scheme.</param>
        /// <param name="isHeader">Indicates whether the row is a header row.</param>
        public void RenderRow(IEnumerable<string> row, int rowNumber, bool isHighlighted = false, bool isSelected = false, bool isHeader = false)
        {
            TableRenderStyle lineStyle = RenderOptions.Lines.ResolveStyle();
            Consoul.Write(RenderOptions.LeftPad, writeLine: false);
            foreach (string cell in row)
            {
                if (isHeader)
                {
                    TableRenderStyleWriter.Write(RenderOptions.Lines.VerticalCharacter.ToString(), lineStyle, writeLine: false);
                    WriteCentered(cell, RenderOptions.ColumnSize.GetValueOrDefault(), RenderOptions.ResolveHeaderStyle(), writeLine: false);
                }
                else
                {
                    TableRenderStyleWriter.Write(RenderOptions.Lines.VerticalCharacter.ToString(), lineStyle, writeLine: false);
                    TableRenderStyle rowStyle = isSelected
                        ? TableRenderStyle.From(RenderOptions.SelectionScheme)
                        : isHighlighted
                            ? TableRenderStyle.From(RenderOptions.HighlightedScheme)
                            : (rowNumber % 2 == 0
                                ? RenderOptions.ResolveContentStyle1()
                                : RenderOptions.ResolveContentStyle2());

                    WriteCentered(cell, RenderOptions.ColumnSize.GetValueOrDefault(), rowStyle, writeLine: false);
                }
            }
            TableRenderStyleWriter.Write(RenderOptions.Lines.VerticalCharacter.ToString(), lineStyle);
            RenderHorizontalSeparator();
        }

        /// <summary>
        /// Renders a horizontal table separator when enabled by the render options.
        /// </summary>
        public void RenderHorizontalSeparator()
        {
            if (RenderOptions.Lines.ContentHorizontal || RenderOptions.Lines.HeaderHorizontal)
            {
                string horizontalLine = new string(RenderOptions.Lines.HorizontalCharacter, RenderOptions.GetMaximumTableWidth());
                TableRenderStyleWriter.Write(RenderOptions.LeftPad + horizontalLine, RenderOptions.Lines.ResolveStyle());
            }
        }

        private static void WriteCentered(string message, int maxWidth, TableRenderStyle style, bool writeLine)
        {
            message = message ?? string.Empty;
            if (maxWidth == 0)
            {
                maxWidth = Consoul.ConsoleBufferWidth;
            }

            string text = message.Length > maxWidth
                ? message.Substring(0, Math.Max(0, maxWidth - 1)) + "…"
                : message;

            int remainder = maxWidth - text.Length - 1;
            int right = remainder / 2;
            int left = remainder % 2 != 0 ? (remainder + 1) / 2 : right;
            text = new string(' ', Math.Max(0, left)) + text + new string(' ', Math.Max(0, right));

            TableRenderStyleWriter.Write(text, style, writeLine);
        }
    }
}
