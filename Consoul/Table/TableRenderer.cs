using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    public class TableRenderer
    {
        public TableRenderOptions RenderOptions { get; set; }

        public TableRenderer(TableRenderOptions renderOptions = default)
        {
            RenderOptions = renderOptions ?? throw new ArgumentNullException(nameof(renderOptions));
        }

        public void RenderHeaders(IEnumerable<string> headers, int rowNumber)
        {
            RenderHorizontalSeparator();
            RenderRow(headers, rowNumber, isHeader: true);
        }

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

        public void RenderHorizontalSeparator()
        {
            if (RenderOptions.Lines.ContentHorizontal || RenderOptions.Lines.HeaderHorizontal)
            {
                string horizontalLine = new string(RenderOptions.Lines.HorizontalCharacter, (int)RenderOptions.MaximumTableWidth);
                Consoul.Write(RenderOptions.LeftPad + horizontalLine, RenderOptions.Lines.Color);
            }
        }
    }
}
