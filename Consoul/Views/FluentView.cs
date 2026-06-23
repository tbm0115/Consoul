using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Views;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a view that can be assembled fluently without creating a subclass.
    /// </summary>
    public class FluentView : IView, INavigationAwareView
    {
        private readonly List<FluentViewOption> _options = new List<FluentViewOption>();
        private string _goBackMessage;
        private bool _goBackRequested;
        private ViewNavigationContext _navigationContext = new ViewNavigationContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentView"/> class.
        /// </summary>
        /// <param name="title">The title rendered above the option prompt.</param>
        /// <param name="goBackMessage">Optional label for the generated option that exits the view.</param>
        public FluentView(string title, string goBackMessage = null)
        {
            Title = title;
            _goBackMessage = string.IsNullOrEmpty(goBackMessage)
                ? RenderOptions.DefaultGoBackMessage
                : goBackMessage;
        }

        /// <summary>
        /// Gets or sets the title of the view.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets a value indicating whether a request to navigate back has been made.
        /// </summary>
        public bool GoBackRequested => _goBackRequested;

        ViewNavigationContext INavigationAwareView.NavigationContext
        {
            get => _navigationContext;
            set => _navigationContext = value ?? new ViewNavigationContext();
        }

        /// <summary>
        /// Adds a synchronous option to the view.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="FluentView"/> instance.</returns>
        public FluentView Option(string label, Action action, ConsoleColor? color = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Option(label, cancellationToken =>
            {
                action();
                return Task.CompletedTask;
            }, color);
        }

        /// <summary>
        /// Adds an asynchronous option to the view.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="FluentView"/> instance.</returns>
        public FluentView Option(string label, Func<CancellationToken, Task> action, ConsoleColor? color = null)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Option label cannot be empty.", nameof(label));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _options.Add(new FluentViewOption(label, action, color ?? RenderOptions.OptionColor));
            return this;
        }

        /// <summary>
        /// Adds an option that navigates to another view type when selected.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <returns>The current <see cref="FluentView"/> instance.</returns>
        public FluentView Navigate<TView>(string label, ConsoleColor? color = null, bool replace = false) where TView : IView
        {
            return Option(label, cancellationToken =>
            {
                _navigationContext.RequestNavigation(replace
                    ? NavigationCommand.Replace(typeof(TView))
                    : NavigationCommand.Push(typeof(TView)));
                return Task.CompletedTask;
            }, color);
        }

        /// <summary>
        /// Requests to navigate back from the current view.
        /// </summary>
        public void GoBack()
        {
            _goBackRequested = true;
            _navigationContext.RequestNavigation(NavigationCommand.Pop());
        }

        /// <summary>
        /// Asynchronously renders the view and invokes the selected option.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <returns>A task representing the asynchronous render operation.</returns>
        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            int idx = -1;
            _goBackRequested = false;
            _navigationContext.Reset();

            do
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    GoBack();
                    break;
                }

                Consoul.ConsoleDriver.Clear();
                if (!string.IsNullOrEmpty(Title))
                {
                    BannerEntry.Render(Title, RenderOptions.PromptColor);
                }

                var prompt = new SelectionPrompt(string.Empty, false);
                foreach (var option in _options)
                {
                    prompt.Add(option.Label, option.Color);
                }

                prompt.Add(_goBackMessage, RenderOptions.SubnoteColor);

                try
                {
                    var result = prompt.Render(cancellationToken);
                    if (result.IsCanceled || cancellationToken.IsCancellationRequested)
                    {
                        GoBack();
                        break;
                    }

                    if (!result.HasSelection)
                    {
                        idx = -1;
                        continue;
                    }

                    idx = result.Index;
                    if (idx >= 0 && idx < _options.Count)
                    {
                        _navigationContext.Reset();
                        await _options[idx].Action(cancellationToken).ConfigureAwait(false);

                        if (_navigationContext.HasPendingCommand)
                        {
                            if (_navigationContext.PendingCommand.CommandType == NavigationCommandType.Pop)
                            {
                                _goBackRequested = true;
                            }

                            break;
                        }

                        if (_goBackRequested)
                        {
                            break;
                        }

                        idx = -1;
                    }
                    else if (idx == _options.Count)
                    {
                        GoBack();
                        break;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    GoBack();
                    break;
                }
                catch (Exception ex)
                {
                    Consoul.Write(ex, $"Failed to render '{Title}' view", true, RenderOptions.InvalidColor);
                    if (RenderOptions.WaitOnError)
                    {
                        Consoul.Wait(cancellationToken: cancellationToken);
                    }

                    if (RenderOptions.ViewErrorMode == RenderOptions.ViewErrorModes.Throw)
                    {
                        throw;
                    }

                    idx = -1;
                }
            }
            while (idx < 0 && !_navigationContext.HasPendingCommand && !GoBackRequested);
        }

        /// <summary>
        /// Renders the view synchronously and waits for user input.
        /// </summary>
        public void Render()
        {
            try
            {
                RenderAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex) when (RenderOptions.ViewErrorMode != RenderOptions.ViewErrorModes.Throw)
            {
                Consoul.Write(ex, $"Failed to render '{Title}' view", true, RenderOptions.InvalidColor);
                if (RenderOptions.WaitOnError)
                {
                    Consoul.Wait();
                }
            }
        }

        private sealed class FluentViewOption
        {
            public FluentViewOption(string label, Func<CancellationToken, Task> action, ConsoleColor color)
            {
                Label = label;
                Action = action;
                Color = color;
            }

            public string Label { get; }

            public Func<CancellationToken, Task> Action { get; }

            public ConsoleColor Color { get; }
        }
    }
}
