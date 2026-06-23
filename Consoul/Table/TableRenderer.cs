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
            Consoul.Write(RenderOptions.LeftPad, writeLine: false);
            foreach (string cell in row)
            {
                if (isHeader)
                {
                    Consoul.Write(RenderOptions.Lines.VerticalCharacter.ToString(), RenderOptions.Lines.Color, writeLine: false);
                    Consoul.Center(cell, RenderOptions.ColumnSize.GetValueOrDefault(), RenderOptions.HeaderScheme, writeLine: false);
                }
                else
                {
                    Consoul.Write(RenderOptions.Lines.VerticalCharacter.ToString(), RenderOptions.Lines.Color, writeLine: false);
                    ColorScheme rowScheme = isSelected
                        ? RenderOptions.SelectionScheme
                        : isHighlighted
                            ? RenderOptions.HighlightedScheme
                            : (rowNumber % 2 == 0
                                ? RenderOptions.ContentScheme1
                                : RenderOptions.ContentScheme2);

                    Consoul.Center(cell, RenderOptions.ColumnSize.GetValueOrDefault(), rowScheme, writeLine: false);
                }
            }
            Consoul.Write(RenderOptions.Lines.VerticalCharacter.ToString(), RenderOptions.Lines.Color);
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
                Consoul.Write(RenderOptions.LeftPad + horizontalLine, RenderOptions.Lines.Color);
            }
        }
    }
}
