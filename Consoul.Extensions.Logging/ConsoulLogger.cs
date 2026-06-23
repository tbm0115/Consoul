using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// An <see cref="ILogger"/> implementation that uses the <see cref="Consoul"/> methods.
    /// </summary>
    public sealed class ConsoulLogger : ILogger
    {
        /// <summary>
        /// Default color map for log levels.
        /// </summary>
        public readonly Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap = new Dictionary<LogLevel, ConsoleColor>()
        {
            [LogLevel.Trace] = RenderOptions.OptionColor,
            [LogLevel.Debug] = RenderOptions.SubnoteColor,
            [LogLevel.Information] = RenderOptions.DefaultColor,
            [LogLevel.Warning] = RenderOptions.InvalidColor,
            [LogLevel.Error] = RenderOptions.InvalidColor,
            [LogLevel.Critical] = RenderOptions.InvalidColor,
            [LogLevel.None] = ConsoleColor.Black,
        };

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return state as IDisposable;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
            => LogLevelToColorMap.ContainsKey(logLevel);

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            Consoul.Write(formatter(state, exception), LogLevelToColorMap[logLevel]);
            WriteException(exception);
        }

        private void WriteException(Exception exception, int tabDepth = 0)
        {
            string tabs = new string('\t', tabDepth);
            if (exception != null)
            {
                Consoul.Write(tabs + "Exception: ", ConsoleColor.Red);
                Consoul.Write(tabs + "\tMessage: " + exception.Message, ConsoleColor.Red);
                Consoul.Write(tabs + "\tStackTrace: " + exception.StackTrace, ConsoleColor.Gray);

                if (exception.InnerException != null)
                {
                    Consoul.Write(tabs + "\tInnerException: ", ConsoleColor.Red);
                    WriteException(exception.InnerException, tabDepth + 1);
                }
            }
        }
    }
}
