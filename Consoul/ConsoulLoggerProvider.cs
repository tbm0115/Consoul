using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace ConsoulLibrary
{
    [ProviderAlias("Consoul")]
    public sealed class ConsoulLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ConsoulLogger> _loggers =
            new ConcurrentDictionary<string, ConsoulLogger>(StringComparer.OrdinalIgnoreCase);

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new ConsoulLogger());


        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
