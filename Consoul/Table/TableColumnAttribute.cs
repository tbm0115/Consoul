using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Configures how a model property is used when <see cref="DynamicTableView{T}"/> infers table columns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class TableColumnAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableColumnAttribute"/> class.
        /// </summary>
        public TableColumnAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableColumnAttribute"/> class with a display label.
        /// </summary>
        /// <param name="label">The label rendered for the inferred table column.</param>
        public TableColumnAttribute(string label)
        {
            Label = label;
        }

        /// <summary>
        /// Gets or sets the label rendered for the inferred table column.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the relative ordering for inferred table columns.
        /// </summary>
        public int Order { get; set; } = int.MaxValue;

        /// <summary>
        /// Gets or sets a value indicating whether the property is included when columns are inferred.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets the alignment used for the inferred column.
        /// </summary>
        public TableCellAlignment Alignment { get; set; } = TableCellAlignment.Default;

        /// <summary>
        /// Gets or sets the format used when rendering values in the inferred column.
        /// </summary>
        /// <remarks>
        /// Use either a standard/custom format string such as <c>yyyy-MM-dd</c> or a composite
        /// format string such as <c>{0} ms</c>.
        /// </remarks>
        public string Format { get; set; }
    }
}
