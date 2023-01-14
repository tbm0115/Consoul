using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoulLibrary
{
    public static class ConsoulLoggerExtensions
    {
        public static ILoggingBuilder AddConsoulLogger(
            this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, ConsoulLoggerProvider>());

            return builder;
        }
    }
}
