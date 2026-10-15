using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a prompt that can be shown through an <see cref="IAnsiConsole"/> facade.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    public interface IPrompt<T>
    {
        /// <summary>
        /// Shows the prompt through the specified console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the prompt.</param>
        /// <returns>The prompt result.</returns>
        T Show(IAnsiConsole console);

        /// <summary>
        /// Shows the prompt asynchronously through the specified console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the prompt.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the prompt result.</returns>
        Task<T> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken = default);
    }
}
