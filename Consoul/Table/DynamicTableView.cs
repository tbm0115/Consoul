using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a dynamic table view that can display a list of items with customizable headers and rendering options.
    /// </summary>
    /// <typeparam name="T">The type of the source items displayed in the table.</typeparam>
    public class DynamicTableView<T> where T : class
    {
        /// <summary>
        /// Gets or sets the rendering options for the table.
        /// </summary>
        public TableRenderOptions RenderOptions
        {
            get
            {
                return _table?.TableRenderOptions;
            }
            set
            {
                _table.TableRenderOptions = value;
                // TODO: Should we re-render when this changes?
            }
        }

        private List<T> _contents { get; set; } = new List<T>();
        /// <summary>
        /// Gets or sets the list of items to be displayed in the table.
        /// </summary>
        public IEnumerable<T> Contents => _contents;

        private Dictionary<string, string> _headers { get; set; } = new Dictionary<string, string>();
        public IEnumerable<string> Headers => _headers.Keys;

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
        public void AddHeader(Expression<Func<T, object>> selector, string label = null)
        {
            PropertyInfo selectedProperty = null;
            if (selector.Body is MemberExpression memberExpression)
            {
                // Handle direct property access, e.g., (o => o.Age)
                selectedProperty = memberExpression.Member as PropertyInfo;
            }
            else if (selector.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            {
                // Handle conversions, e.g., (o => (object)o.Age)
                selectedProperty = operand.Member as PropertyInfo;
            }
            else
            {
                throw new ArgumentException("Selector must be a property access expression.");
            }
            if (!string.IsNullOrEmpty(label))
            {
                _headers.Add(selectedProperty.Name, label);
            } else
            {
                _headers.Add(selectedProperty.Name, selectedProperty.Name);
            }
            _table.AddHeader(selectedProperty.Name);
        }

        public void AddHeaders(params Expression<Func<T, object>>[] selectors)
        {
            foreach (var selector in selectors)
            {
                AddHeader(selector);
            }
        }

        public void AddRow(T item, bool renderAfterAdding = false)
        {
            var type = typeof(T);
            var properties = type.GetProperties().ToList();
            var row = new List<string>();
            foreach (var columnHeader in _headers)
            {
                string cellValue = string.Empty;
                var property = properties.FirstOrDefault(o => o.Name.Equals(columnHeader.Key, StringComparison.OrdinalIgnoreCase));
                if (property != null)
                {
                    cellValue = property.GetValue(item).ToString();
                }
                row.Add(cellValue);
            }
            _contents.Add(item);
            _table.AddRow(row);
            if (renderAfterAdding)
                Render();
        }
        public void AddRows(IEnumerable<T> items, bool renderAfterAdding = false)
        {
            foreach (var item in items)
            {
                AddRow(item, renderAfterAdding);
            }
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
        public void Render(string message = default, ConsoleColor? color = null, bool clearConsole = true)
        {
            _table?.Render(message, color, clearConsole);
        }

        /// <summary>
        /// Prompts the user to select an option from the table and returns the selected row index.
        /// </summary>
        /// <param name="message">Optional message to display before prompting.</param>
        /// <param name="color">Optional color for the message text.</param>
        /// <param name="allowEmpty">Specifies whether the user is allowed to skip the selection.</param>
        /// <returns>The selected row index, or -1 if selection was not valid.</returns>
        public T Prompt(string message = "", ConsoleColor? color = null, bool allowEmpty = false, bool clearConsole = true, CancellationToken cancellationToken = default)
        {
            var choice = _table.Prompt(message, color, allowEmpty, clearConsole, cancellationToken);
            if (choice >= 0 && choice <= _contents.Count)
                return _contents[choice.Value];
            return null;
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

        public static DynamicTableView<T> Create(IEnumerable<T> items, params Expression<Func<T, object>>[] selectors)
        {
            var table = new DynamicTableView<T>();
            table.AddHeaders(selectors);
            table.AddRows(items);
            return table;
        }
    }
}
