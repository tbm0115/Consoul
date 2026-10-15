using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Event arguments when a <see cref="TableView"/> query yields no results.
    /// </summary>
    public class TableQueryYieldsNoResultsEventArgs : EventArgs
    {

        /// <summary>
        /// Gets or sets the message to display when no rows match the query.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the query text that produced no rows.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Initializes event arguments for a table query with no matching rows.
        /// </summary>
        /// <param name="message">Message to display for the empty result.</param>
        /// <param name="query">Query text that produced no rows.</param>
        public TableQueryYieldsNoResultsEventArgs(string message, string query)
        {
            Message = message;
            Query = query;
        }
    }
}
