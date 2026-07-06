using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a table column footer value that can be assigned from text or common renderable components.
    /// </summary>
    public sealed class TableColumnFooter
    {
        /// <summary>
        /// Gets an empty footer value.
        /// </summary>
        public static TableColumnFooter Empty { get; } = new TableColumnFooter(string.Empty);

        private TableColumnFooter(string text)
        {
            Text = text ?? string.Empty;
        }

        /// <summary>
        /// Gets the footer text that is rendered inside the table footer row.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Creates a footer value from a reusable renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <returns>A footer value containing the captured text.</returns>
        public static TableColumnFooter FromRenderable(IConsoulRenderable renderable)
        {
            if (renderable == null)
            {
                return Empty;
            }

            return new TableColumnFooter(Table.FormatCellValue(renderable));
        }

        /// <summary>
        /// Converts plain text into a table column footer value.
        /// </summary>
        /// <param name="text">The footer text.</param>
        public static implicit operator TableColumnFooter(string text)
        {
            return new TableColumnFooter(text);
        }

        /// <summary>
        /// Converts a text block renderable into a table column footer value.
        /// </summary>
        /// <param name="text">The text block renderable.</param>
        public static implicit operator TableColumnFooter(TextBlockEntry text)
        {
            return FromRenderable(text);
        }

        /// <summary>
        /// Converts a markup renderable into a table column footer value.
        /// </summary>
        /// <param name="markup">The markup renderable.</param>
        public static implicit operator TableColumnFooter(MarkupEntry markup)
        {
            return FromRenderable(markup);
        }

        /// <summary>
        /// Converts a panel renderable into a table column footer value.
        /// </summary>
        /// <param name="panel">The panel renderable.</param>
        public static implicit operator TableColumnFooter(PanelEntry panel)
        {
            return FromRenderable(panel);
        }

        /// <summary>
        /// Converts a table renderable into a table column footer value.
        /// </summary>
        /// <param name="table">The table renderable.</param>
        public static implicit operator TableColumnFooter(TableView table)
        {
            return FromRenderable(table);
        }

        /// <summary>
        /// Converts a table column footer value into plain text.
        /// </summary>
        /// <param name="footer">The footer value.</param>
        public static implicit operator string(TableColumnFooter footer)
        {
            return footer?.Text ?? string.Empty;
        }

        /// <summary>
        /// Returns the footer text.
        /// </summary>
        /// <returns>The footer text.</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
