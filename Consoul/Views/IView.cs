using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a delegate used to handle the callback action for when a choice is made in the view.
    /// Takes an integer parameter indicating the index of the choice selected.
    /// </summary>
    /// <param name="choiceIndex">The index of the chosen option.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public delegate Task ChoiceCallback(int choiceIndex);

    /// <summary>
    /// Defines the structure for a view that can be rendered in the console.
    /// A view may include a title, the ability to render content synchronously or asynchronously, and the ability to navigate back.
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Gets or sets the title of the view.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets a value indicating whether a request to navigate back has been made.
        /// </summary>
        bool GoBackRequested { get; }

        /// <summary>
        /// Renders the view asynchronously.
        /// The rendering can also include a callback to handle the choice made by the user.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <returns>A Task representing the asynchronous render operation.</returns>
        Task RenderAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders the view synchronously.
        /// The rendering can also include a callback to handle the choice made by the user.
        /// </summary>
        void Render();

        /// <summary>
        /// Signals a request to navigate back from the current view.
        /// This can be used to manage navigation within a multi-view console application.
        /// </summary>
        void GoBack();
    }
}
