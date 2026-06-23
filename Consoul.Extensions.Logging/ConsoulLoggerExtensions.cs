using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ConsoulLibrary
{
    /// <summary>
    /// Extension methods for registering Consoul logging.
    /// </summary>
    public static class ConsoulLoggerExtensions
    {
        /// <summary>
        /// Adds the Consoul logger provider to a logging builder.
        /// </summary>
        /// <param name="builder">Logging builder.</param>
        /// <returns>The same logging builder.</returns>
        public static ILoggingBuilder AddConsoulLogger(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, ConsoulLoggerProvider>());

            return builder;
        }
    }
}
