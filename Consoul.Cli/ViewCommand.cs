using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Views;

namespace ConsoulLibrary
{
    /// <summary>
    /// Runs a Consoul MVC-style view as a command.
    /// </summary>
    /// <typeparam name="TView">The view type to render.</typeparam>
    public class ViewCommand<TView> : IAsyncCommand where TView : IView
    {
        private readonly Func<TView> _viewFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewCommand{TView}"/> class.
        /// </summary>
        public ViewCommand() : this(() => (TView)Activator.CreateInstance(typeof(TView)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewCommand{TView}"/> class.
        /// </summary>
        /// <param name="viewFactory">The view factory.</param>
        public ViewCommand(Func<TView> viewFactory)
        {
            _viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
        }

        /// <summary>
        /// Renders the configured view.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to exit code <c>0</c>.</returns>
        public async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            await new ViewRenderer()
                .RenderAsync<TView>(cancellationToken, () => _viewFactory())
                .ConfigureAwait(false);
            return 0;
        }
    }
}
