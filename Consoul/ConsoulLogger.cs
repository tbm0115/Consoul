using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// An <see cref="ILogger"/> implementation that uses the <see cref="Consoul"/> methods.
    /// </summary>
    public sealed class ConsoulLogger : ILogger
    {
        public readonly Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap = new Dictionary<LogLevel, ConsoleColor>()
        {
            [LogLevel.Trace] = ConsoulLibrary.RenderOptions.OptionColor,
            [LogLevel.Debug] = ConsoulLibrary.RenderOptions.SubnoteColor,
            [LogLevel.Information] = ConsoulLibrary.RenderOptions.DefaultColor,
            [LogLevel.Warning] = ConsoulLibrary.RenderOptions.InvalidColor,
            [LogLevel.Error] = ConsoulLibrary.RenderOptions.InvalidColor,
            [LogLevel.Critical] = ConsoulLibrary.RenderOptions.InvalidColor,
            [LogLevel.None] = ConsoleColor.Black,
        };

        public IDisposable BeginScope<TState>(TState state) {
            return state as IDisposable;
        }

        public bool IsEnabled(LogLevel logLevel)
            => LogLevelToColorMap.ContainsKey(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            Consoul.Write(formatter(state, exception), LogLevelToColorMap[logLevel]);
        }
    }
}
