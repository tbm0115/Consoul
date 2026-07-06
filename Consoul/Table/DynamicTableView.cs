using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using ConsoulLibrary.Views.Editing;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a dynamic table view that can display a list of items with customizable headers and rendering options.
    /// </summary>
    /// <typeparam name="T">The type of the source items displayed in the table.</typeparam>
    public class DynamicTableView<T> : IRenderable where T : class
    {
        /// <summary>
        /// Gets or sets the rendering options for the table.
        /// </summary>
        public TableRenderOptions RenderOptions
        {
            get
            {
                return _renderOptions;
            }
            set
            {
                _renderOptions = value ?? new TableRenderOptions();
                _table.TableRenderOptions = _renderOptions;
                if (_columns.Count > 0 || _contents.Count > 0)
                {
                    RebuildTableRows();
                }
            }
        }

        private TableRenderOptions _renderOptions;

        private List<T> _contents { get; set; } = new List<T>();
        /// <summary>
        /// Gets or sets the list of items to be displayed in the table.
        /// </summary>
        public IEnumerable<T> Contents => _contents;

        private List<T> _visibleContents { get; set; } = new List<T>();
        /// <summary>
        /// Gets the source items currently visible after filtering, sorting, and paging have been applied.
        /// </summary>
        public IEnumerable<T> VisibleContents => _visibleContents;

        /// <summary>
        /// Gets the total number of source rows added to this table.
        /// </summary>
        public int TotalRowCount => _contents.Count;

        /// <summary>
        /// Gets the number of source rows currently visible after filtering, sorting, and paging have been applied.
        /// </summary>
        public int VisibleRowCount => _visibleContents.Count;

        /// <summary>
        /// Gets the number of source rows that match active filters and search before paging is applied.
        /// </summary>
        public int MatchedRowCount { get; private set; }

        private readonly List<DynamicTableColumn> _columns = new List<DynamicTableColumn>();
        private Func<T, bool> _filterPredicate;
        private string _searchQuery;
        private StringComparison _searchComparison = StringComparison.CurrentCultureIgnoreCase;
        private Func<T, object> _sortKeySelector;
        private bool _sortDescending;
        private int? _pageNumber;
        private int? _pageSize;

        /// <summary>
        /// Gets the header keys used to render columns.
        /// </summary>
        public IEnumerable<string> Headers => _columns.Select(column => column.Label);

        /// <summary>
        /// Gets the active cell search query, or <see langword="null"/> when no search is applied.
        /// </summary>
        public string ActiveSearchQuery => _searchQuery;

        private bool _showSummary;
        /// <summary>
        /// Gets or sets a value indicating whether a summary line is rendered below the table.
        /// </summary>
        public bool ShowSummary
        {
            get => _showSummary;
            set
            {
                _showSummary = value;
                RebuildTableRows();
            }
        }

        private bool _showEmptyMessage;
        /// <summary>
        /// Gets or sets a value indicating whether an empty-state row is rendered when no rows are visible.
        /// </summary>
        public bool ShowEmptyMessage
        {
            get => _showEmptyMessage;
            set
            {
                _showEmptyMessage = value;
                RebuildTableRows();
            }
        }

        private string _emptyMessage = "No rows match the current table view.";
        /// <summary>
        /// Gets or sets the message rendered when <see cref="ShowEmptyMessage"/> is enabled and no rows are visible.
        /// </summary>
        public string EmptyMessage
        {
            get => _emptyMessage;
            set
            {
                _emptyMessage = value;
                RebuildTableRows();
            }
        }

        private Func<DynamicTableView<T>, string> _summaryFormatter;
        /// <summary>
        /// Gets or sets a formatter used to render the summary line when <see cref="ShowSummary"/> is enabled.
        /// </summary>
        public Func<DynamicTableView<T>, string> SummaryFormatter
        {
            get => _summaryFormatter;
            set
            {
                _summaryFormatter = value;
                RebuildTableRows();
            }
        }

        // Represents the underlying TableView instance used for rendering.
        private TableView _table { get; set; } = new TableView();

        /// <summary>
        /// Event that is triggered when a query yields no results in the table.
        /// </summary>
        public event TableQueryYieldsNoResults QueryYieldsNoResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTableView{TSource}"/> class with optional render options.
        /// </summary>
        /// <param name="options">Optional rendering options for the table.</param>
        public DynamicTableView(TableRenderOptions options = default)
        {
            RenderOptions = options ?? new TableRenderOptions();
        }

        /// <summary>
        /// Adds a header for the table using a lambda expression to define the property.
        /// </summary>
        /// <param name="selector">Lambda expression pointing to the property.</param>
        /// <param name="label">Optional display label for the selected property. When omitted, the label is resolved from property metadata.</param>
        /// <param name="alignment">Optional alignment for the selected column.</param>
        /// <param name="format">Optional format used when rendering the selected property value. When omitted, property metadata is used.</param>
        public void AddHeader(Expression<Func<T, object>> selector, string label = null, TableCellAlignment alignment = TableCellAlignment.Default, string format = null)
        {
            PropertyInfo selectedProperty = GetSelectedProperty(selector);
            string header = !string.IsNullOrEmpty(label) ? label : ResolvePropertyLabel(selectedProperty);
            Func<T, object> accessor = item => selectedProperty.GetValue(item);
            TableCellAlignment resolvedAlignment = alignment == TableCellAlignment.Default
                ? ResolvePropertyAlignment(selectedProperty)
                : alignment;
            string resolvedFormat = !string.IsNullOrWhiteSpace(format)
                ? format
                : ResolvePropertyFormat(selectedProperty);
            AddColumnCore(header, accessor, resolvedAlignment, resolvedFormat);
        }

        /// <summary>
        /// Adds a computed column for the table.
        /// </summary>
        /// <param name="label">Header label rendered for the column.</param>
        /// <param name="valueSelector">Function used to read the cell value from each row item.</param>
        /// <param name="alignment">Optional alignment for the computed column.</param>
        public void AddColumn(string label, Func<T, object> valueSelector, TableCellAlignment alignment = TableCellAlignment.Default)
        {
            AddColumnCore(label, valueSelector, alignment, null);
        }

        /// <summary>
        /// Adds a formatted computed column for the table.
        /// </summary>
        /// <param name="label">Header label rendered for the column.</param>
        /// <param name="valueSelector">Function used to read the cell value from each row item.</param>
        /// <param name="format">Format used when rendering the selected value.</param>
        /// <param name="alignment">Optional alignment for the computed column.</param>
        public void AddColumn(string label, Func<T, object> valueSelector, string format, TableCellAlignment alignment = TableCellAlignment.Default)
        {
            AddColumnCore(label, valueSelector, alignment, format);
        }

        private void AddColumnCore(string label, Func<T, object> valueSelector, TableCellAlignment alignment, string format)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("Column label cannot be empty.", nameof(label));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            _columns.Add(new DynamicTableColumn(label, valueSelector, alignment, format));
            RebuildTableRows();
        }

        /// <summary>
        /// Adds multiple headers using property selector expressions.
        /// </summary>
        /// <param name="selectors">Property selectors to add as columns.</param>
        public void AddHeaders(params Expression<Func<T, object>>[] selectors)
        {
            foreach (var selector in selectors)
            {
                AddHeader(selector);
            }
        }

        /// <summary>
        /// Adds columns for all public readable scalar properties on the source type, using property metadata for display labels, order, and visibility.
        /// </summary>
        public void AddPublicPropertyColumns()
        {
            foreach (PropertyInfo property in GetDefaultProperties())
            {
                PropertyInfo capturedProperty = property;
                AddColumn(
                    ResolvePropertyLabel(capturedProperty),
                    item => capturedProperty.GetValue(item),
                    ResolvePropertyFormat(capturedProperty),
                    ResolvePropertyAlignment(capturedProperty));
            }
        }

        /// <summary>
        /// Adds a source item as a table row.
        /// </summary>
        /// <param name="item">Source item to add.</param>
        /// <param name="renderAfterAdding">If <c>true</c>, renders the table after adding the row.</param>
        public void AddRow(T item, bool renderAfterAdding = false)
        {
            _contents.Add(item);
            RebuildTableRows();
            if (renderAfterAdding)
                Render();
        }

        /// <summary>
        /// Adds multiple source items as table rows.
        /// </summary>
        /// <param name="items">Source items to add.</param>
        /// <param name="renderAfterAdding">If <c>true</c>, renders the table after the rows are added.</param>
        public void AddRows(IEnumerable<T> items, bool renderAfterAdding = false)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                _contents.Add(item);
            }

            RebuildTableRows();
            if (renderAfterAdding)
            {
                Render();
            }
        }

        /// <summary>
        /// Filters the model rows rendered by this table.
        /// </summary>
        /// <param name="predicate">Predicate that returns <see langword="true"/> for rows that should remain visible.</param>
        /// <returns>The current <see cref="DynamicTableView{T}"/> instance.</returns>
        public DynamicTableView<T> Filter(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            _filterPredicate = predicate;
            RebuildTableRows();
            return this;
        }

        /// <summary>
        /// Clears the active row filter.
        /// </summary>
        /// <returns>The current <see cref="DynamicTableView{T}"/> instance.</returns>
        public DynamicTableView<T> ClearFilter()
        {
            _filterPredicate = null;
            RebuildTableRows();
            return this;
        }

        /// <summary>
        /// Filters the rendered model rows to those whose visible cell text contains the provided query.
        /// </summary>
        /// <param name="query">Search text matched against visible cell values.</param>
        /// <param name="comparison">String comparison used for matching cell values.</param>
        /// <returns>The current <see cref="DynamicTableView{T}"/> instance.</returns>
        public DynamicTableView<T> Search(string query, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return ClearSearch();
            }

            _searchQuery = query;
            _searchComparison = comparison;
            RebuildTableRows();
            return this;
        }

        /// <summary>
        /// Clears the active cell search.
        /// </summary>
        /// <returns>The current <see cref="DynamicTableView{T}"/> instance.</returns>
        public DynamicTableView<T> ClearSearch()
        {
            _searchQuery = null;
            _searchComparison = StringComparison.CurrentCultureIgnoreCase;
            RebuildTableRows();
            return this;
        }

        /// <summary>
        /// Sorts the model rows rendered by this table.
        /// </summary>
        /// <param name="keySelector">Function used to read the sort key from each source row.</param>
        /// <param name="descending">Indicates whether rows should be sorted in descending order.</param>
        /// <returns>The current <see cref="DynamicTableView{T}"/> instance.</returns>
        public DynamicTableView<T> SortBy(Func<T, object> keySelector, bool descending = false)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            _sortKeySelector = keySelector;
            _sortDescending = descending;
            RebuildTableRows();
            return this;
        }

        /// <summary>
        /// Clears the active row sort.
        /// </summary>
        /// <returns>The current <see cref="DynamicTableView{T}"/> instance.</returns>
        public DynamicTableView<T> ClearSort()
        {
            _sortKeySelector = null;
            _sortDescending = false;
            RebuildTableRows();
            return this;
        }

        /// <summary>
        /// Shows one page of the filtered and sorted model rows.
        /// </summary>
        /// <param name="pageNumber">One-based page number to render.</param>
        /// <param name="pageSize">Number of source rows to render on the page.</param>
        /// <returns>The current <see cref="DynamicTableView{T}"/> instance.</returns>
        public DynamicTableView<T> Page(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            }

            _pageNumber = pageNumber;
            _pageSize = pageSize;
            RebuildTableRows();
            return this;
        }

        /// <summary>
        /// Clears the active row page and renders all filtered and sorted rows.
        /// </summary>
        /// <returns>The current <see cref="DynamicTableView{T}"/> instance.</returns>
        public DynamicTableView<T> ClearPaging()
        {
            _pageNumber = null;
            _pageSize = null;
            RebuildTableRows();
            return this;
        }

        /// <summary>
        /// Handles the event when a query yields no results in the table.
        /// </summary>
        private void _table_QueryYieldsNoResults(object sender, TableQueryYieldsNoResultsEventArgs e)
        {
            raiseQueryYieldsNoResults(e.Message, e.Query);
        }

        /// <summary>
        /// Renders the table to the console.
        /// </summary>
        /// <param name="message">Optional message rendered before the table.</param>
        /// <param name="color">Optional foreground color for the message.</param>
        /// <param name="clearConsole">Indicates whether to clear the console before rendering.</param>
        public void Render(string message = default, ConsoleColor? color = null, bool clearConsole = true)
        {
            _table?.Render(message, color, clearConsole);
        }

        /// <summary>
        /// Renders the table at the current cursor position without clearing the console or moving to the origin.
        /// </summary>
        /// <param name="message">Optional message rendered before the table.</param>
        /// <param name="color">Optional foreground color for the message.</param>
        public void RenderInline(string message = default, ConsoleColor? color = null)
        {
            _table?.RenderInline(message, color);
        }

        /// <summary>
        /// Renders the table at the current cursor position when used as a reusable visual component.
        /// </summary>
        void IConsoulRenderable.Render()
        {
            RenderInline();
        }

        /// <summary>
        /// Prompts the user to select one or more visible model rows from the table.
        /// </summary>
        /// <param name="message">Optional message to display before prompting.</param>
        /// <param name="color">Optional color for the message text.</param>
        /// <param name="allowEmpty">Specifies whether the user is allowed to accept an empty selection.</param>
        /// <param name="clearConsole">If <c>true</c>, clears the console before rendering.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result describing selected visible row indexes, selected model items, cancellation, or accepted empty selection.</returns>
        public TableSelectionResult<T> Select(string message = "", ConsoleColor? color = null, bool allowEmpty = false, bool clearConsole = true, CancellationToken cancellationToken = default)
        {
            TableSelectionResult result = _table.Select(message, color, allowEmpty, clearConsole, cancellationToken);
            T[] selectedItems = result.Indexes
                .Where(index => index >= 0 && index < _visibleContents.Count)
                .Select(index => _visibleContents[index])
                .ToArray();
            return new TableSelectionResult<T>(result, selectedItems);
        }

        /// <summary>
        /// Prompts the user to select an option from the table and returns the selected model row.
        /// </summary>
        /// <param name="message">Optional message to display before prompting.</param>
        /// <param name="color">Optional color for the message text.</param>
        /// <param name="allowEmpty">Specifies whether the user is allowed to skip the selection.</param>
        /// <param name="clearConsole">If <c>true</c>, clears the console before rendering.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The selected model row, or <see langword="null"/> if the user canceled or accepted an empty selection.</returns>
        public T Prompt(string message = "", ConsoleColor? color = null, bool allowEmpty = false, bool clearConsole = true, CancellationToken cancellationToken = default)
        {
            TableSelectionResult<T> result = Select(message, color, allowEmpty, clearConsole, cancellationToken);
            return result.HasSelection ? result.SelectedItem : null;
        }

        /// <summary>
        /// Raises the <see cref="QueryYieldsNoResults"/> event.
        /// </summary>
        /// <param name="message">Message associated with the event.</param>
        /// <param name="query">The query that yielded no results.</param>
        private void raiseQueryYieldsNoResults(string message, string query)
        {
            QueryYieldsNoResults?.Invoke(this, new TableQueryYieldsNoResultsEventArgs(message, query));
        }

        /// <summary>
        /// Creates a dynamic table from source items and selected columns.
        /// </summary>
        /// <param name="items">Source items to render as rows.</param>
        /// <param name="selectors">Property selectors to render as columns.</param>
        /// <returns>A dynamic table containing the provided rows and columns.</returns>
        public static DynamicTableView<T> Create(IEnumerable<T> items, params Expression<Func<T, object>>[] selectors)
        {
            return Create(items, null, selectors);
        }

        /// <summary>
        /// Creates a dynamic table from source items, render options, and selected columns.
        /// </summary>
        /// <param name="items">Source items to render as rows.</param>
        /// <param name="options">Optional rendering options for the table.</param>
        /// <param name="selectors">Property selectors to render as columns. When omitted, public readable scalar properties are used.</param>
        /// <returns>A dynamic table containing the provided rows and columns.</returns>
        public static DynamicTableView<T> Create(IEnumerable<T> items, TableRenderOptions options, params Expression<Func<T, object>>[] selectors)
        {
            var table = new DynamicTableView<T>(options);
            if (selectors != null && selectors.Length > 0)
            {
                table.AddHeaders(selectors);
            }
            else
            {
                table.AddPublicPropertyColumns();
            }

            table.AddRows(items ?? Enumerable.Empty<T>());
            return table;
        }

        private void RebuildTableRows()
        {
            TableRenderOptions options = _renderOptions ?? new TableRenderOptions();
            _table?.Dispose();
            _table = new TableView(options);
            _table.QueryYieldsNoResults += _table_QueryYieldsNoResults;

            foreach (var column in _columns)
            {
                _table.AddHeader(column.Label, column.Alignment);
            }

            var matchedRows = GetVisibleSourceRows(applyPaging: false).ToList();
            MatchedRowCount = matchedRows.Count;
            _visibleContents = ApplyPaging(matchedRows).ToList();
            foreach (var item in _visibleContents)
            {
                _table.AddRow(CreateRow(item));
            }

            ApplyTableMessages();
        }

        private IEnumerable<T> GetVisibleSourceRows(bool applyPaging)
        {
            IEnumerable<T> query = _contents;

            if (_filterPredicate != null)
            {
                query = query.Where(item => _filterPredicate(item));
            }

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                query = query.Where(MatchesSearch);
            }

            if (_sortKeySelector != null)
            {
                query = _sortDescending
                    ? query.OrderByDescending(item => _sortKeySelector(item), DynamicTableSortComparer.Instance)
                    : query.OrderBy(item => _sortKeySelector(item), DynamicTableSortComparer.Instance);
            }

            if (applyPaging)
            {
                query = ApplyPaging(query);
            }

            return query;
        }

        private IEnumerable<T> ApplyPaging(IEnumerable<T> query)
        {
            if (_pageNumber.HasValue && _pageSize.HasValue)
            {
                int skip = (_pageNumber.Value - 1) * _pageSize.Value;
                return query.Skip(skip).Take(_pageSize.Value);
            }

            return query;
        }

        private void ApplyTableMessages()
        {
            TableRenderOptions options = _table.TableRenderOptions;
            options.EmptyMessage = _showEmptyMessage ? _emptyMessage : null;
            options.SummaryText = _showSummary ? BuildSummaryText() : null;
        }

        private string BuildSummaryText()
        {
            if (_summaryFormatter != null)
            {
                return _summaryFormatter(this);
            }

            string visible = VisibleRowCount == 1 ? "1 row" : VisibleRowCount + " rows";
            string matched = MatchedRowCount == 1 ? "1 match" : MatchedRowCount + " matches";
            string total = TotalRowCount == 1 ? "1 total" : TotalRowCount + " total";
            string summary = "Showing " + visible + " of " + matched + " (" + total + ")";

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                summary += " for \"" + _searchQuery + "\"";
            }

            if (_pageNumber.HasValue && _pageSize.HasValue)
            {
                summary += " on page " + _pageNumber.Value;
            }

            return summary + ".";
        }

        private bool MatchesSearch(T item)
        {
            if (string.IsNullOrWhiteSpace(_searchQuery))
            {
                return true;
            }

            foreach (var column in _columns)
            {
                object value = item == null ? null : column.ValueSelector(item);
                string cell = FormatCellValue(value, column.Format);
                if (cell.IndexOf(_searchQuery, _searchComparison) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private List<string> CreateRow(T item)
        {
            var row = new List<string>();
            foreach (var column in _columns)
            {
                object value = item == null ? null : column.ValueSelector(item);
                row.Add(FormatCellValue(value, column.Format));
            }

            return row;
        }

        private static PropertyInfo GetSelectedProperty(Expression<Func<T, object>> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            PropertyInfo selectedProperty = null;
            if (selector.Body is MemberExpression memberExpression)
            {
                selectedProperty = memberExpression.Member as PropertyInfo;
            }
            else if (selector.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            {
                selectedProperty = operand.Member as PropertyInfo;
            }

            if (selectedProperty == null)
            {
                throw new ArgumentException("Selector must be a property access expression.", nameof(selector));
            }

            return selectedProperty;
        }

        private static IEnumerable<PropertyInfo> GetDefaultProperties()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select((property, index) => new
                {
                    Property = property,
                    Index = index,
                    Metadata = property.GetCustomAttribute<TableColumnAttribute>(true)
                })
                .Where(item =>
                    item.Property.CanRead &&
                    IsDefaultColumnType(item.Property.PropertyType) &&
                    (item.Metadata == null || item.Metadata.Visible))
                .OrderBy(item => item.Metadata != null ? item.Metadata.Order : int.MaxValue)
                .ThenBy(item => item.Index)
                .Select(item => item.Property);
        }

        private static string ResolvePropertyLabel(PropertyInfo property)
        {
            var tableColumn = property.GetCustomAttribute<TableColumnAttribute>(true);
            if (tableColumn != null && !string.IsNullOrWhiteSpace(tableColumn.Label))
            {
                return tableColumn.Label;
            }

            var documentation = new PropertyDocumentation(property);
            if (!string.IsNullOrWhiteSpace(documentation.DisplayName))
            {
                return documentation.DisplayName;
            }

            if (!string.IsNullOrWhiteSpace(documentation.DisplayDescription))
            {
                return documentation.DisplayDescription;
            }

            return SplitPascalCase(property.Name);
        }

        private static TableCellAlignment ResolvePropertyAlignment(PropertyInfo property)
        {
            var tableColumn = property.GetCustomAttribute<TableColumnAttribute>(true);
            return tableColumn?.Alignment ?? TableCellAlignment.Default;
        }

        private static string ResolvePropertyFormat(PropertyInfo property)
        {
            var tableColumn = property.GetCustomAttribute<TableColumnAttribute>(true);
            if (tableColumn != null && !string.IsNullOrWhiteSpace(tableColumn.Format))
            {
                return tableColumn.Format;
            }

            return ResolveDisplayFormat(property);
        }

        private static string ResolveDisplayFormat(PropertyInfo property)
        {
            object displayFormat = property
                .GetCustomAttributes(true)
                .FirstOrDefault(attribute => attribute.GetType().FullName == "System.ComponentModel.DataAnnotations.DisplayFormatAttribute");

            if (displayFormat == null)
            {
                return null;
            }

            var dataFormatStringProperty = displayFormat.GetType().GetProperty("DataFormatString");
            return dataFormatStringProperty?.GetValue(displayFormat) as string;
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var builder = new StringBuilder(value.Length + 8);
            for (int index = 0; index < value.Length; index++)
            {
                if (index > 0 && ShouldInsertWordBoundary(value, index))
                {
                    builder.Append(' ');
                }

                builder.Append(value[index]);
            }

            return builder.ToString();
        }

        private static bool ShouldInsertWordBoundary(string value, int index)
        {
            char current = value[index];
            char previous = value[index - 1];
            if (char.IsWhiteSpace(current) || char.IsWhiteSpace(previous))
            {
                return false;
            }

            if (char.IsUpper(current) && (char.IsLower(previous) || char.IsDigit(previous)))
            {
                return true;
            }

            if (char.IsUpper(current) &&
                char.IsUpper(previous) &&
                index + 1 < value.Length &&
                char.IsLower(value[index + 1]))
            {
                return true;
            }

            if (char.IsDigit(current) && char.IsLetter(previous))
            {
                return true;
            }

            return char.IsLetter(current) && char.IsDigit(previous);
        }

        private static bool IsDefaultColumnType(Type propertyType)
        {
            Type type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            return type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type == typeof(Guid);
        }

        private static string FormatCellValue(object value, string format)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(format))
            {
                return Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
            }

            try
            {
                if (format.IndexOf("{0", StringComparison.Ordinal) >= 0)
                {
                    return string.Format(CultureInfo.CurrentCulture, format, value);
                }

                var formattable = value as IFormattable;
                if (formattable != null)
                {
                    return formattable.ToString(format, CultureInfo.CurrentCulture);
                }

                return string.Format(CultureInfo.CurrentCulture, "{0:" + format + "}", value);
            }
            catch (FormatException)
            {
                return Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
            }
        }

        private sealed class DynamicTableColumn
        {
            public DynamicTableColumn(string label, Func<T, object> valueSelector, TableCellAlignment alignment, string format)
            {
                Label = label;
                ValueSelector = valueSelector;
                Alignment = alignment;
                Format = format;
            }

            public string Label { get; }

            public Func<T, object> ValueSelector { get; }

            public TableCellAlignment Alignment { get; }

            public string Format { get; }
        }

        private sealed class DynamicTableSortComparer : IComparer<object>
        {
            public static readonly DynamicTableSortComparer Instance = new DynamicTableSortComparer();

            public int Compare(object x, object y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                var comparable = x as IComparable;
                if (comparable != null && x.GetType().IsInstanceOfType(y))
                {
                    return comparable.CompareTo(y);
                }

                string left = Convert.ToString(x);
                string right = Convert.ToString(y);
                return StringComparer.CurrentCultureIgnoreCase.Compare(left, right);
            }
        }
    }
}
