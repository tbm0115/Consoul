using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace ConsoulLibrary
{
    /// <summary>
    /// 
    /// </summary>
    [ProviderAlias("Consoul")]
    public sealed class ConsoulLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ConsoulLogger> _loggers =
            new ConcurrentDictionary<string, ConsoulLogger>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new ConsoulLogger());

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
