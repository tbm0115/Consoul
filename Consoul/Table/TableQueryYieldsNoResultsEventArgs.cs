using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Event arguments when a <see cref="TableView"/> query yields no results.
    /// </summary>
    public class TableQueryYieldsNoResultsEventArgs : EventArgs
    {

        public string Message { get; set; }

        public string Query { get; set; }

        public TableQueryYieldsNoResultsEventArgs(string message, string query)
        {
            Message = message;
            Query = query;
        }
    }
}
