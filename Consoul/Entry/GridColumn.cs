using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Describes a configured column for Spectre-style <see cref="Grid"/> snippets.
    /// </summary>
    public sealed class GridColumn
    {
        /// <summary>
        /// Gets or sets the preferred column width. When <see langword="null"/>, the grid shares remaining width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether cell text should be clipped instead of wrapped.
        /// </summary>
        public bool NoWrap { get; set; }

        /// <summary>
        /// Gets or sets optional left/right padding for cells in this column.
        /// </summary>
        /// <remarks>
        /// Vertical padding is ignored by the responsive grid renderer.
        /// </remarks>
        public Padding Padding { get; set; }

        /// <summary>
        /// Gets or sets optional justification for cells in this column.
        /// </summary>
        public Justify? Alignment { get; set; }

        internal bool HasExplicitPadding => Padding != null;

        internal Padding ResolvePadding()
        {
            return Padding != null ? Padding.Clone() : new Padding(0);
        }
    }

    /// <summary>
    /// Provides fluent helpers for <see cref="GridColumn"/> instances.
    /// </summary>
    public static class GridColumnExtensions
    {
        /// <summary>
        /// Sets the preferred column width.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="width">The preferred width, or <see langword="null"/> to share remaining width.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn Width(this GridColumn column, int? width)
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
        /// Sets whether cell text should be clipped instead of wrapped.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="enabled">Whether no-wrap behavior should be enabled.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn NoWrap(this GridColumn column, bool enabled = true)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.NoWrap = enabled;
            return column;
        }

        /// <summary>
        /// Sets the left/right padding used by cells in this column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="padding">The padding to apply.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn Padding(this GridColumn column, Padding padding)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.Padding = padding != null ? padding.Clone() : null;
            return column;
        }

        /// <summary>
        /// Sets the left/right padding used by cells in this column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="left">Padding added to the left side.</param>
        /// <param name="right">Padding added to the right side.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn Padding(this GridColumn column, int left, int right)
        {
            return Padding(column, new Padding(left, 0, right, 0));
        }

        /// <summary>
        /// Sets the left padding used by cells in this column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="left">Padding added to the left side.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn PadLeft(this GridColumn column, int left)
        {
            return MutatePadding(column, padding => padding.Left = Math.Max(0, left));
        }

        /// <summary>
        /// Sets the right padding used by cells in this column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="right">Padding added to the right side.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn PadRight(this GridColumn column, int right)
        {
            return MutatePadding(column, padding => padding.Right = Math.Max(0, right));
        }

        /// <summary>
        /// Sets the cell justification for this column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="justify">The justification to apply.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn Justify(this GridColumn column, Justify justify)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.Alignment = justify;
            return column;
        }

        /// <summary>
        /// Left-aligns values in this grid column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn LeftAligned(this GridColumn column)
        {
            return Justify(column, ConsoulLibrary.Justify.Left);
        }

        /// <summary>
        /// Left-aligns values in this grid column using text-builder justification vocabulary.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn LeftJustified(this GridColumn column)
        {
            return LeftAligned(column);
        }

        /// <summary>
        /// Centers values in this grid column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn Centered(this GridColumn column)
        {
            return Justify(column, ConsoulLibrary.Justify.Center);
        }

        /// <summary>
        /// Right-aligns values in this grid column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn RightAligned(this GridColumn column)
        {
            return Justify(column, ConsoulLibrary.Justify.Right);
        }

        /// <summary>
        /// Right-aligns values in this grid column using text-builder justification vocabulary.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <returns>The configured <see cref="GridColumn"/> instance.</returns>
        public static GridColumn RightJustified(this GridColumn column)
        {
            return RightAligned(column);
        }

        private static GridColumn MutatePadding(GridColumn column, Action<Padding> mutate)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            Padding padding = column.Padding != null ? column.Padding.Clone() : new Padding(0);
            mutate(padding);
            column.Padding = padding;
            return column;
        }
    }
}
