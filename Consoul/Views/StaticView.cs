using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Views;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a static console view containing a set of predefined options.
    /// This view can display options to the user and respond to their selection, including supporting asynchronous rendering.
    /// </summary>
    public abstract class StaticView : IView, INavigationAwareView
    {
        private bool _goBackRequested = false;
        private string _goBackMessage = RenderOptions.DefaultGoBackMessage;
        private ViewNavigationContext _navigationContext = new ViewNavigationContext();
        private ViewChromeOptions _chrome = new ViewChromeOptions();
        private CancellationToken _activeRenderCancellationToken;

        /// <summary>
        /// Gets or sets the title of the view.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the list of view options available in this view.
        /// </summary>
        public List<ViewOption> Options { get; set; } = new List<ViewOption>();

        /// <summary>
        /// Gets a value indicating whether a "go back" request has been made.
        /// </summary>
        public bool GoBackRequested => _goBackRequested;

        ViewNavigationContext INavigationAwareView.NavigationContext
        {
            get => _navigationContext;
            set => _navigationContext = value ?? new ViewNavigationContext();
        }

        /// <summary>
        /// Gets or sets the callback invoked after an option is selected.
        /// </summary>
        public ChoiceCallback OnOptionSelected { get; set; }

        /// <summary>
        /// Gets or sets whether this view's option prompt uses arrow-key navigation when routine input is not queued.
        /// </summary>
        public bool PromptKeyboardNavigationEnabled { get; set; } = RenderOptions.ViewPromptKeyboardNavigationEnabled;

        /// <summary>
        /// Gets or sets whether this view's option prompt supports filtering.
        /// </summary>
        public bool PromptFilterEnabled { get; set; } = RenderOptions.ViewPromptFilterEnabled;

        /// <summary>
        /// Gets or sets the maximum number of view options rendered per prompt page. Set to <see langword="null"/> to render all options.
        /// </summary>
        public int? PromptPageSize { get; set; } = RenderOptions.ViewPromptPageSize;

        /// <summary>
        /// Gets or sets an optional error-handling mode for this view. When <see langword="null"/>, <see cref="RenderOptions.ViewErrorMode"/> is used.
        /// </summary>
        public RenderOptions.ViewErrorModes? ViewErrorMode { get; set; }

        /// <summary>
        /// Gets or sets whether this view waits for input after rendering an error. When <see langword="null"/>, <see cref="RenderOptions.WaitOnError"/> is used.
        /// </summary>
        public bool? WaitOnError { get; set; }

        /// <summary>
        /// Gets or sets optional navigation chrome rendered above the view.
        /// </summary>
        public ViewChromeOptions Chrome
        {
            get => _chrome;
            set => _chrome = value ?? new ViewChromeOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticView"/> class.
        /// The constructor initializes the view based on attributes and methods that define the options for the view.
        /// </summary>
        /// <param name="callback">Optional callback invoked after an option is selected.</param>
        public StaticView(ChoiceCallback callback = null)
        {
            OnOptionSelected = callback;

            Type thisType = this.GetType();

            // Set the view's title and "go back" message if the ViewAttribute is present.
            Type viewType = typeof(ViewAttribute);
            ViewAttribute viewAttr = thisType.GetCustomAttribute(viewType) as ViewAttribute;
            if (viewAttr != null)
            {
                Title = viewAttr.Title;
                _goBackMessage = viewAttr.GoBackMessage;
            }

            // Build the options from local methods decorated with the ViewOption attribute.
            Type viewOptionType = typeof(ViewOptionAttribute);
            IEnumerable<MethodInfo> allOptionMethods = thisType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable<MethodInfo> viewOptionMethods = allOptionMethods.Where(o => o.GetCustomAttribute(viewOptionType) != null);

            foreach (MethodInfo method in viewOptionMethods)
            {
                ViewOptionAttribute attr = method.GetCustomAttribute(viewOptionType) as ViewOptionAttribute;
                if (attr == null)
                    continue;

                ParameterInfo[] methodParameters = method.GetParameters();
                List<object> implementedMethodParameters = new List<object>();

                // Set default values for parameters where applicable.
                foreach (ParameterInfo methodParameter in methodParameters)
                {
                    if (methodParameter.HasDefaultValue)
                    {
                        implementedMethodParameters.Add(methodParameter.DefaultValue);
                    }
                    else if (Nullable.GetUnderlyingType(methodParameter.ParameterType) != null)
                    {
                        implementedMethodParameters.Add(null);
                    }
                }

                bool useParameters = methodParameters.Length > 0 && implementedMethodParameters.Count == methodParameters.Length;

                // Create a callback for the option's action.
                ViewOptionCallback p = delegate ()
                {
                    thisType.InvokeMember(
                        method.Name,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                        null,
                        this,
                        useParameters ? implementedMethodParameters.ToArray() : null
                    );
                };

                // Add the option to the list of available options in the view.
                var option = new ViewOption(
                    attr.Message,
                    p,
                    attr.Color,
                    attr.Description)
                {
                    Shortcut = attr.Shortcut
                };
                option.AddSearchTerms(SplitSearchTerms(attr.SearchTerms));
                Options.Add(option);
            }
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
        /// Adds an existing option to the view and returns it for additional metadata configuration.
        /// </summary>
        /// <param name="option">The option to add.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(ViewOption option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            Options.Add(option);
            return option;
        }

        /// <summary>
        /// Adds a synchronous option to the view.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Action action, ConsoleColor? color = null, string description = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOption(new ViewOption(label, () => action(), color, description));
        }

        /// <summary>
        /// Adds a synchronous option to the view using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Action action, string style, string description = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOption(new ViewOption(label, () => action(), style, description));
        }

        /// <summary>
        /// Adds a synchronous option to the view using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Action action, Style style, string description = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOption(new ViewOption(label, () => action(), style, description));
        }

        /// <summary>
        /// Adds a synchronous option using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Action action, SpectreColor color, string description = null)
        {
            return AddOption(label, action, (ConsoleColor?)color.ToConsoleColor(), description);
        }

        /// <summary>
        /// Adds a synchronous option using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Action action, RgbColor color, string description = null)
        {
            return AddOption(label, action, (ConsoleColor?)color.ToConsoleColor(), description);
        }

        /// <summary>
        /// Adds an asynchronous option to the view without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Func<Task> action, ConsoleColor? color = null, string description = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOption(label, () => action().GetAwaiter().GetResult(), color, description);
        }

        /// <summary>
        /// Adds an asynchronous option to the view using style text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Func<Task> action, string style, string description = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOption(label, () => action().GetAwaiter().GetResult(), style, description);
        }

        /// <summary>
        /// Adds an asynchronous option to the view using a reusable style without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Func<Task> action, Style style, string description = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOption(label, () => action().GetAwaiter().GetResult(), style, description);
        }

        /// <summary>
        /// Adds an asynchronous option using a Spectre-style color facade without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Func<Task> action, SpectreColor color, string description = null)
        {
            return AddOption(label, action, (ConsoleColor?)color.ToConsoleColor(), description);
        }

        /// <summary>
        /// Adds an asynchronous option using an RGB color mapped to Consoul's console palette without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Func<Task> action, RgbColor color, string description = null)
        {
            return AddOption(label, action, (ConsoleColor?)color.ToConsoleColor(), description);
        }

        /// <summary>
        /// Adds an asynchronous option to the view that observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Func<CancellationToken, Task> action, ConsoleColor? color = null, string description = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOption(label, () => action(_activeRenderCancellationToken).GetAwaiter().GetResult(), color, description);
        }

        /// <summary>
        /// Adds an asynchronous option to the view using style text that observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Func<CancellationToken, Task> action, string style, string description = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOption(label, () => action(_activeRenderCancellationToken).GetAwaiter().GetResult(), style, description);
        }

        /// <summary>
        /// Adds an asynchronous option to the view using a reusable style that observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Func<CancellationToken, Task> action, Style style, string description = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOption(label, () => action(_activeRenderCancellationToken).GetAwaiter().GetResult(), style, description);
        }

        /// <summary>
        /// Adds an asynchronous option using a Spectre-style color facade that observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Func<CancellationToken, Task> action, SpectreColor color, string description = null)
        {
            return AddOption(label, action, (ConsoleColor?)color.ToConsoleColor(), description);
        }

        /// <summary>
        /// Adds an asynchronous option using an RGB color mapped to Consoul's console palette that observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddOption(string label, Func<CancellationToken, Task> action, RgbColor color, string description = null)
        {
            return AddOption(label, action, (ConsoleColor?)color.ToConsoleColor(), description);
        }

        /// <summary>
        /// Adds a synchronous option that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Action action, ConsoleColor? color = null, bool? defaultValue = false, string description = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddConfirmOption(
                label,
                confirmationMessage,
                cancellationToken =>
                {
                    action();
                    return Task.CompletedTask;
                },
                color,
                defaultValue,
                description);
        }

        /// <summary>
        /// Adds a synchronous option that asks for confirmation before running using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Action action, string style, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue, description);
        }

        /// <summary>
        /// Adds a synchronous option that asks for confirmation before running using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Action action, Style style, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue, description);
        }

        /// <summary>
        /// Adds a synchronous option that asks for confirmation before running using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Action action, SpectreColor color, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue, description);
        }

        /// <summary>
        /// Adds a synchronous option that asks for confirmation before running using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Action action, RgbColor color, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue, description);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Func<Task> action, ConsoleColor? color = null, bool? defaultValue = false, string description = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddConfirmOption(label, confirmationMessage, cancellationToken => action(), color, defaultValue, description);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Func<Task> action, string style, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue, description);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Func<Task> action, Style style, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue, description);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Func<Task> action, SpectreColor color, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue, description);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Func<Task> action, RgbColor color, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue, description);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, ConsoleColor? color = null, bool? defaultValue = false, string description = null)
        {
            if (confirmationMessage == null)
            {
                throw new ArgumentNullException(nameof(confirmationMessage));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOption(label, () =>
            {
                ConfirmResult result = Consoul.Confirm(
                    confirmationMessage,
                    defaultValue: defaultValue,
                    cancellationToken: _activeRenderCancellationToken);
                if (result.HasValue && result.Value)
                {
                    action(_activeRenderCancellationToken).GetAwaiter().GetResult();
                }
            }, color, description);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using style text and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, string style, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue, description);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using a reusable style and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, Style style, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue, description);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using a Spectre-style color facade and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, SpectreColor color, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue, description);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using an RGB color mapped to Consoul's console palette and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, RgbColor color, bool? defaultValue = false, string description = null)
        {
            return AddConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue, description);
        }

        /// <summary>
        /// Adds an option that opens a model object in the built-in form editor.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="model">The model object edited by the form editor.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        /// <remarks>
        /// This MVC-style helper uses <see cref="Consoul.Form(object, BindingFlags, bool)"/> so static views, dynamic views,
        /// object editing, and form editing remain on one editor stack.
        /// </remarks>
        public ViewOption AddEditFormOption(
            string label,
            object model,
            ConsoleColor? color = null,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true,
            string description = null)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return AddOption(
                label,
                cancellationToken => Consoul.Form(model, bindingAttr, enableDocumentEditor).RenderAsync(cancellationToken),
                color,
                description);
        }

        /// <summary>
        /// Adds an option that opens a model object in the built-in form editor using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="model">The model object edited by the form editor.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddEditFormOption(
            string label,
            object model,
            string style,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true,
            string description = null)
        {
            return AddEditFormOption(label, model, ResolveStyle(style, RenderOptions.OptionColor), bindingAttr, enableDocumentEditor, description);
        }

        /// <summary>
        /// Adds an option that opens a model object in the built-in form editor using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="model">The model object edited by the form editor.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddEditFormOption(
            string label,
            object model,
            Style style,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true,
            string description = null)
        {
            return AddEditFormOption(label, model, ResolveStyle(style, RenderOptions.OptionColor), bindingAttr, enableDocumentEditor, description);
        }

        /// <summary>
        /// Adds an option that opens a model object in the built-in form editor using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="model">The model object edited by the form editor.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddEditFormOption(
            string label,
            object model,
            SpectreColor color,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true,
            string description = null)
        {
            return AddEditFormOption(label, model, (ConsoleColor?)color.ToConsoleColor(), bindingAttr, enableDocumentEditor, description);
        }

        /// <summary>
        /// Adds an option that opens a model object in the built-in form editor using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="model">The model object edited by the form editor.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddEditFormOption(
            string label,
            object model,
            RgbColor color,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true,
            string description = null)
        {
            return AddEditFormOption(label, model, (ConsoleColor?)color.ToConsoleColor(), bindingAttr, enableDocumentEditor, description);
        }

        /// <summary>
        /// Adds an option that requests navigation to a typed view when selected.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="replace">When <see langword="true"/>, replaces the current view; otherwise pushes the target view.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddNavigationOption<TView>(string label, ConsoleColor? color = null, bool replace = false, string description = null) where TView : IView
        {
            return AddOption(label, () => NavigateTo<TView>(replace), color, description);
        }

        /// <summary>
        /// Adds an option that requests navigation to a typed view when selected using style text.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <see langword="true"/>, replaces the current view; otherwise pushes the target view.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddNavigationOption<TView>(string label, string style, bool replace = false, string description = null) where TView : IView
        {
            return AddOption(label, () => NavigateTo<TView>(replace), style, description);
        }

        /// <summary>
        /// Adds an option that requests navigation to a typed view when selected using a reusable style.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <see langword="true"/>, replaces the current view; otherwise pushes the target view.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddNavigationOption<TView>(string label, Style style, bool replace = false, string description = null) where TView : IView
        {
            return AddOption(label, () => NavigateTo<TView>(replace), style, description);
        }

        /// <summary>
        /// Adds an option that requests navigation to a typed view when selected using a Spectre-style color facade.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <see langword="true"/>, replaces the current view; otherwise pushes the target view.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddNavigationOption<TView>(string label, SpectreColor color, bool replace = false, string description = null) where TView : IView
        {
            return AddNavigationOption<TView>(label, (ConsoleColor?)color.ToConsoleColor(), replace, description);
        }

        /// <summary>
        /// Adds an option that requests navigation to a typed view when selected using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <see langword="true"/>, replaces the current view; otherwise pushes the target view.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddNavigationOption<TView>(string label, RgbColor color, bool replace = false, string description = null) where TView : IView
        {
            return AddNavigationOption<TView>(label, (ConsoleColor?)color.ToConsoleColor(), replace, description);
        }

        /// <summary>
        /// Adds an option that requests navigation to a view created by a factory when selected.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="replace">When <see langword="true"/>, replaces the current view; otherwise pushes the target view.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddNavigationOption<TView>(
            string label,
            Func<TView> viewFactory,
            ConsoleColor? color = null,
            bool replace = false,
            string description = null)
            where TView : IView
        {
            if (viewFactory == null)
            {
                throw new ArgumentNullException(nameof(viewFactory));
            }

            return AddOption(label, () => NavigateTo(viewFactory, replace), color, description);
        }

        /// <summary>
        /// Adds an option that requests navigation to a view created by a factory when selected using style text.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <see langword="true"/>, replaces the current view; otherwise pushes the target view.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddNavigationOption<TView>(
            string label,
            Func<TView> viewFactory,
            string style,
            bool replace = false,
            string description = null)
            where TView : IView
        {
            return AddNavigationOption(label, viewFactory, ResolveStyle(style, RenderOptions.OptionColor), replace, description);
        }

        /// <summary>
        /// Adds an option that requests navigation to a view created by a factory when selected using a reusable style.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <see langword="true"/>, replaces the current view; otherwise pushes the target view.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddNavigationOption<TView>(
            string label,
            Func<TView> viewFactory,
            Style style,
            bool replace = false,
            string description = null)
            where TView : IView
        {
            return AddNavigationOption(label, viewFactory, ResolveStyle(style, RenderOptions.OptionColor), replace, description);
        }

        /// <summary>
        /// Adds an option that requests navigation to a view created by a factory using a Spectre-style color facade.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <see langword="true"/>, replaces the current view; otherwise pushes the target view.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddNavigationOption<TView>(
            string label,
            Func<TView> viewFactory,
            SpectreColor color,
            bool replace = false,
            string description = null)
            where TView : IView
        {
            return AddNavigationOption(label, viewFactory, (ConsoleColor?)color.ToConsoleColor(), replace, description);
        }

        /// <summary>
        /// Adds an option that requests navigation to a view created by a factory using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <see langword="true"/>, replaces the current view; otherwise pushes the target view.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The added <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddNavigationOption<TView>(
            string label,
            Func<TView> viewFactory,
            RgbColor color,
            bool replace = false,
            string description = null)
            where TView : IView
        {
            return AddNavigationOption(label, viewFactory, (ConsoleColor?)color.ToConsoleColor(), replace, description);
        }

        /// <summary>
        /// Adds a display-only separator that visually groups view options.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Optional separator foreground color.</param>
        /// <returns>The added separator option.</returns>
        public ViewOption AddSeparator(string label = null, ConsoleColor? color = null)
        {
            ViewOption separator = ViewOption.Separator(label, color);
            Options.Add(separator);
            return separator;
        }

        /// <summary>
        /// Adds a display-only separator that visually groups view options using style text.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>The added separator option.</returns>
        public ViewOption AddSeparator(string label, string style)
        {
            ViewOption separator = ViewOption.Separator(label, style);
            Options.Add(separator);
            return separator;
        }

        /// <summary>
        /// Adds a display-only separator that visually groups view options using a reusable style.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>The added separator option.</returns>
        public ViewOption AddSeparator(string label, Style style)
        {
            ViewOption separator = ViewOption.Separator(label, style);
            Options.Add(separator);
            return separator;
        }

        /// <summary>
        /// Adds a display-only separator that visually groups view options using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the separator label.</param>
        /// <returns>The added separator option.</returns>
        public ViewOption AddSeparator(string label, SpectreColor color)
        {
            ViewOption separator = ViewOption.Separator(label, color);
            Options.Add(separator);
            return separator;
        }

        /// <summary>
        /// Adds a display-only separator that visually groups view options using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the separator label.</param>
        /// <returns>The added separator option.</returns>
        public ViewOption AddSeparator(string label, RgbColor color)
        {
            ViewOption separator = ViewOption.Separator(label, color);
            Options.Add(separator);
            return separator;
        }

        /// <summary>
        /// Adds a display-only separator that visually groups view options using style text.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>The added separator option.</returns>
        public ViewOption AddSeparatorStyled(string label = null, string style = null)
        {
            ViewOption separator = ViewOption.SeparatorStyled(label, style);
            Options.Add(separator);
            return separator;
        }

        /// <summary>
        /// Configures optional navigation chrome for this view.
        /// </summary>
        /// <param name="configure">Action used to configure the chrome options.</param>
        /// <returns>The current <see cref="StaticView"/> instance.</returns>
        public StaticView UseChrome(Action<ViewChromeOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            configure(Chrome);
            return this;
        }

        /// <summary>
        /// Enables breadcrumb chrome above the view.
        /// </summary>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <returns>The current <see cref="StaticView"/> instance.</returns>
        public StaticView ShowBreadcrumbs(BreadcrumbOptions options = null)
        {
            Chrome.ShowBreadcrumbs = true;
            Chrome.BreadcrumbOptions = options;
            return this;
        }

        /// <summary>
        /// Enables shortcut hint chrome above the view.
        /// </summary>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <returns>The current <see cref="StaticView"/> instance.</returns>
        public StaticView ShowShortcuts(ShortcutBarOptions options = null)
        {
            Chrome.ShowShortcuts = true;
            Chrome.ShortcutBarOptions = options;
            return this;
        }

        /// <summary>
        /// Sets the error-handling mode for this view only.
        /// </summary>
        /// <param name="mode">The error-handling mode, or <see langword="null"/> to use <see cref="RenderOptions.ViewErrorMode"/>.</param>
        /// <returns>The current <see cref="StaticView"/> instance.</returns>
        public StaticView UseErrorMode(RenderOptions.ViewErrorModes? mode)
        {
            ViewErrorMode = mode;
            return this;
        }

        /// <summary>
        /// Configures this view to rethrow rendering and option exceptions after writing the error details.
        /// </summary>
        /// <returns>The current <see cref="StaticView"/> instance.</returns>
        public StaticView ThrowOnError()
        {
            return UseErrorMode(RenderOptions.ViewErrorModes.Throw);
        }

        /// <summary>
        /// Configures this view to write rendering and option exceptions, then continue view navigation where possible.
        /// </summary>
        /// <returns>The current <see cref="StaticView"/> instance.</returns>
        public StaticView RenderErrorsAndContinue()
        {
            return UseErrorMode(RenderOptions.ViewErrorModes.RenderAndContinue);
        }

        /// <summary>
        /// Configures whether this view waits for input after writing an error.
        /// </summary>
        /// <param name="wait">Whether the view should wait after rendering an error.</param>
        /// <returns>The current <see cref="StaticView"/> instance.</returns>
        public StaticView WaitOnRenderError(bool wait = true)
        {
            WaitOnError = wait;
            return this;
        }

        /// <summary>
        /// Clears view-local error handling so this view follows the global <see cref="RenderOptions"/> values.
        /// </summary>
        /// <returns>The current <see cref="StaticView"/> instance.</returns>
        public StaticView UseGlobalErrorHandling()
        {
            ViewErrorMode = null;
            WaitOnError = null;
            return this;
        }

        /// <summary>
        /// Creates a reusable help surface from the current view options.
        /// </summary>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        /// <param name="includeGoBack">Indicates whether the generated go-back option should be included.</param>
        /// <returns>A renderable help surface describing the current view options.</returns>
        public ViewHelpEntry CreateHelp(string title = null, ViewHelpOptions options = null, bool includeGoBack = true)
        {
            var help = new ViewHelpEntry(ResolveHelpTitle(title), options);
            foreach (ViewOption option in Options)
            {
                if (option.IsSeparator)
                {
                    help.AddSeparator(option.Render(), option.Color);
                    continue;
                }

                help.Add(option.Render(), option.Description, option.Shortcut, null, option.Color);
            }

            if (includeGoBack)
            {
                help.Add(_goBackMessage, "Return to the previous view or exit the current menu.", null, null, RenderOptions.SubnoteColor);
            }

            return help;
        }

        /// <summary>
        /// Adds a selectable help option that renders the current view's options, descriptions, shortcuts and navigation item.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="color">Optional foreground color for the help option.</param>
        /// <param name="shortcut">Optional shortcut that selects the help option.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        /// <param name="waitForInput">Indicates whether the help screen should wait for Enter before returning to the view.</param>
        /// <returns>The added help option.</returns>
        public ViewOption AddHelpOption(
            string label = "Help",
            string title = null,
            ConsoleColor? color = null,
            string shortcut = "?",
            ViewHelpOptions options = null,
            bool waitForInput = true)
        {
            var helpOption = new ViewOption(
                label,
                () => ShowHelp(title, options, waitForInput),
                color ?? RenderOptions.SubnoteColor,
                "Show available actions for this view.")
            {
                Shortcut = shortcut
            };
            Options.Add(helpOption);
            return helpOption;
        }

        /// <summary>
        /// Adds a selectable help option using style text for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="style">Optional foreground style for the help option.</param>
        /// <param name="shortcut">Optional shortcut that selects the help option.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        /// <param name="waitForInput">Indicates whether the help screen should wait for Enter before returning to the view.</param>
        /// <returns>The added help option.</returns>
        public ViewOption AddHelpOptionStyled(
            string label = "Help",
            string title = null,
            string style = null,
            string shortcut = "?",
            ViewHelpOptions options = null,
            bool waitForInput = true)
        {
            return AddHelpOption(label, title, ResolveStyle(style, RenderOptions.SubnoteColor), shortcut, options, waitForInput);
        }

        /// <summary>
        /// Adds a selectable help option using a reusable style for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="style">Optional foreground style for the help option.</param>
        /// <param name="shortcut">Optional shortcut that selects the help option.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        /// <param name="waitForInput">Indicates whether the help screen should wait for Enter before returning to the view.</param>
        /// <returns>The added help option.</returns>
        public ViewOption AddHelpOption(
            string label,
            string title,
            Style style,
            string shortcut = "?",
            ViewHelpOptions options = null,
            bool waitForInput = true)
        {
            return AddHelpOption(label, title, ResolveStyle(style, RenderOptions.SubnoteColor), shortcut, options, waitForInput);
        }

        /// <summary>
        /// Adds a selectable help option using a Spectre-style color facade for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the help option.</param>
        /// <param name="shortcut">Optional shortcut that selects the help option.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        /// <param name="waitForInput">Indicates whether the help screen should wait for Enter before returning to the view.</param>
        /// <returns>The added help option.</returns>
        public ViewOption AddHelpOption(
            string label,
            string title,
            SpectreColor color,
            string shortcut = "?",
            ViewHelpOptions options = null,
            bool waitForInput = true)
        {
            return AddHelpOption(label, title, (ConsoleColor?)color.ToConsoleColor(), shortcut, options, waitForInput);
        }

        /// <summary>
        /// Adds a selectable help option using an RGB color mapped to Consoul's console palette for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the help option.</param>
        /// <param name="shortcut">Optional shortcut that selects the help option.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        /// <param name="waitForInput">Indicates whether the help screen should wait for Enter before returning to the view.</param>
        /// <returns>The added help option.</returns>
        public ViewOption AddHelpOption(
            string label,
            string title,
            RgbColor color,
            string shortcut = "?",
            ViewHelpOptions options = null,
            bool waitForInput = true)
        {
            return AddHelpOption(label, title, (ConsoleColor?)color.ToConsoleColor(), shortcut, options, waitForInput);
        }

        /// <summary>
        /// Shows a searchable command palette for the current view options and invokes the selected action.
        /// </summary>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The result returned by the palette prompt.</returns>
        public PromptResult ShowCommandPalette(string message = "Command palette", bool clear = true, CancellationToken cancellationToken = default)
        {
            return ShowCommandPalette(message, clear, null, cancellationToken);
        }

        /// <summary>
        /// Adds a selectable command palette option that searches and invokes the current view's actions.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="color">Optional foreground color for the palette option.</param>
        /// <param name="shortcut">Optional shortcut that selects the palette option.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <returns>The added palette option.</returns>
        public ViewOption AddCommandPaletteOption(
            string label = "Command palette",
            string message = "Command palette",
            ConsoleColor? color = null,
            string shortcut = "cmd",
            bool clear = true)
        {
            ViewOption paletteOption = null;
            paletteOption = new ViewOption(
                label,
                () => ShowCommandPalette(message, clear, paletteOption, CancellationToken.None),
                color ?? RenderOptions.SubnoteColor,
                "Search and run an action from this view.")
            {
                Shortcut = shortcut
            };
            Options.Add(paletteOption);
            return paletteOption;
        }

        /// <summary>
        /// Adds a selectable command palette option using style text for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="style">Optional foreground style for the palette option.</param>
        /// <param name="shortcut">Optional shortcut that selects the palette option.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <returns>The added palette option.</returns>
        public ViewOption AddCommandPaletteOptionStyled(
            string label = "Command palette",
            string message = "Command palette",
            string style = null,
            string shortcut = "cmd",
            bool clear = true)
        {
            return AddCommandPaletteOption(label, message, ResolveStyle(style, RenderOptions.SubnoteColor), shortcut, clear);
        }

        /// <summary>
        /// Adds a selectable command palette option using a reusable style for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="style">Optional foreground style for the palette option.</param>
        /// <param name="shortcut">Optional shortcut that selects the palette option.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <returns>The added palette option.</returns>
        public ViewOption AddCommandPaletteOption(
            string label,
            string message,
            Style style,
            string shortcut = "cmd",
            bool clear = true)
        {
            return AddCommandPaletteOption(label, message, ResolveStyle(style, RenderOptions.SubnoteColor), shortcut, clear);
        }

        /// <summary>
        /// Adds a selectable command palette option using a Spectre-style color facade for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the palette option.</param>
        /// <param name="shortcut">Optional shortcut that selects the palette option.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <returns>The added palette option.</returns>
        public ViewOption AddCommandPaletteOption(
            string label,
            string message,
            SpectreColor color,
            string shortcut = "cmd",
            bool clear = true)
        {
            return AddCommandPaletteOption(label, message, (ConsoleColor?)color.ToConsoleColor(), shortcut, clear);
        }

        /// <summary>
        /// Adds a selectable command palette option using an RGB color mapped to Consoul's console palette for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the palette option.</param>
        /// <param name="shortcut">Optional shortcut that selects the palette option.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <returns>The added palette option.</returns>
        public ViewOption AddCommandPaletteOption(
            string label,
            string message,
            RgbColor color,
            string shortcut = "cmd",
            bool clear = true)
        {
            return AddCommandPaletteOption(label, message, (ConsoleColor?)color.ToConsoleColor(), shortcut, clear);
        }

        /// <summary>
        /// Requests navigation to a new view instance of type <typeparamref name="TView"/>.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="replace">
        /// When <see langword="true"/> (default), the current view is replaced with the new view. When <see langword="false"/>,
        /// the new view is pushed on top of the navigation stack.
        /// </param>
        protected void NavigateTo<TView>(bool replace = true) where TView : IView
        {
            if (replace)
            {
                _navigationContext.RequestNavigation(NavigationCommand.Replace(typeof(TView)));
            }
            else
            {
                _navigationContext.RequestNavigation(NavigationCommand.Push(typeof(TView)));
            }
        }

        /// <summary>
        /// Requests navigation to a new view created via the provided factory.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="viewFactory">Factory used to create the view instance.</param>
        /// <param name="replace">
        /// When <see langword="true"/> (default), the current view is replaced with the new view. When <see langword="false"/>,
        /// the new view is pushed on top of the navigation stack.
        /// </param>
        protected void NavigateTo<TView>(Func<TView> viewFactory, bool replace = true) where TView : IView
        {
            if (viewFactory == null)
            {
                throw new ArgumentNullException(nameof(viewFactory));
            }

            if (replace)
            {
                _navigationContext.RequestNavigation(NavigationCommand.Replace(() => (IView)viewFactory()));
            }
            else
            {
                _navigationContext.RequestNavigation(NavigationCommand.Push(() => (IView)viewFactory()));
            }
        }

        /// <summary>
        /// Asynchronously renders the view and handles user selection of options.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <returns>A task representing the asynchronous render operation.</returns>
        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            int idx = -1;
            _goBackRequested = false;
            _navigationContext.Reset();
            _activeRenderCancellationToken = cancellationToken;
            do
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    GoBack();
                    break;
                }

                Consoul.ConsoleDriver.Clear();

                RenderChrome();

                BannerEntry.Render(Title, RenderOptions.PromptColor);

                // Create and configure a SelectionPrompt to display the view options.
                SelectionPrompt prompt = new SelectionPrompt(string.Empty, false);
                ConfigurePrompt(prompt);
                List<ViewOption> selectableOptions = new List<ViewOption>();
                foreach (ViewOption option in Options)
                {
                    if (option.IsSeparator)
                    {
                        prompt.AddSeparator(option.Render(), option.Color);
                        continue;
                    }

                    selectableOptions.Add(option);
                    AddPromptOption(prompt, option.Render(), option.Description, option.Shortcut, option.Color, option.SearchTerms);
                }

                // Add the "go back" option to the prompt.
                prompt.Add(_goBackMessage, RenderOptions.SubnoteColor);

                bool actionErrorHandled = false;
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
                    if (idx >= 0 && idx < selectableOptions.Count)
                    {
                        int selectedIndex = idx;
                        _navigationContext.Reset();
                        // Execute the selected option's action asynchronously.
                        await Task.Run(() =>
                        {
                            try
                            {
                                selectableOptions[selectedIndex].Action.Invoke();
                            }
                            catch (Exception ex2)
                            {
                                RenderViewError(ex2, $"Failed to render '{Title}[{selectedIndex}]' view", cancellationToken);
                                actionErrorHandled = true;

                                if (ShouldThrowViewError())
                                    throw;
                            }
                        }, cancellationToken);

                        // Invoke the callback if provided.
                        if (OnOptionSelected != null)
                        {
                            await OnOptionSelected(selectedIndex);
                        }

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
                    else if (idx == selectableOptions.Count)
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
                    if (!actionErrorHandled)
                    {
                        RenderViewError(ex, $"Failed to render '{Title}' view", cancellationToken);
                    }

                    if (ShouldThrowViewError())
                        throw;
                }
            } while (idx < 0 && !_navigationContext.HasPendingCommand && !GoBackRequested);

            _activeRenderCancellationToken = default;
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
            catch (Exception ex) when (!ShouldThrowViewError())
            {
                RenderViewError(ex, $"Failed to render '{Title}' view", CancellationToken.None);
            }
        }

        /// <summary>
        /// Configures the option prompt before the view options are added.
        /// </summary>
        /// <param name="prompt">The prompt used for this render pass.</param>
        protected virtual void ConfigurePrompt(SelectionPrompt prompt)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            prompt.KeyboardNavigationEnabled = PromptKeyboardNavigationEnabled;
            prompt.FilterEnabled = PromptFilterEnabled;
            prompt.PageSize = PromptPageSize;
        }

        private void ShowHelp(string title, ViewHelpOptions options, bool waitForInput)
        {
            Consoul.ConsoleDriver.Clear();
            CreateHelp(title, options).Render();
            if (waitForInput)
            {
                Consoul.Wait();
            }
        }

        private string ResolveHelpTitle(string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                return title;
            }

            if (string.IsNullOrWhiteSpace(Title) || Title.IndexOfAny(new[] { '\r', '\n' }) >= 0)
            {
                return "View Help";
            }

            return Title.Trim() + " Help";
        }

        private void RenderChrome()
        {
            if (Chrome == null)
            {
                return;
            }

            if (Chrome.ShowBreadcrumbs)
            {
                var breadcrumbItems = ResolveBreadcrumbTrail();
                if (breadcrumbItems.Count > 0)
                {
                    new BreadcrumbEntry(breadcrumbItems, Chrome.BreadcrumbOptions).Render();
                }
            }

            if (Chrome.ShowShortcuts)
            {
                var shortcutBar = BuildShortcutBar();
                if (shortcutBar.Items.Count > 0)
                {
                    shortcutBar.Render();
                }
            }
        }

        private IList<string> ResolveBreadcrumbTrail()
        {
            if (_navigationContext.BreadcrumbTrail.Count > 0)
            {
                return _navigationContext.BreadcrumbTrail.ToList();
            }

            return string.IsNullOrWhiteSpace(Title)
                ? new List<string>()
                : new List<string> { Title };
        }

        private ShortcutBarEntry BuildShortcutBar()
        {
            var shortcutBar = new ShortcutBarEntry(Chrome.ShortcutBarOptions);
            foreach (ViewOption option in Options)
            {
                if (option.IsSeparator || string.IsNullOrWhiteSpace(option.Shortcut))
                {
                    continue;
                }

                shortcutBar.Add(option.Shortcut, option.Render());
            }

            if (Chrome.ShowGoBackShortcut && !string.IsNullOrWhiteSpace(Chrome.GoBackShortcut))
            {
                shortcutBar.Add(Chrome.GoBackShortcut, string.IsNullOrWhiteSpace(Chrome.GoBackDescription) ? _goBackMessage : Chrome.GoBackDescription);
            }

            return shortcutBar;
        }

        private PromptResult ShowCommandPalette(string message, bool clear, ViewOption excludedOption, CancellationToken cancellationToken)
        {
            SelectionPrompt prompt = new SelectionPrompt(message, clear)
            {
                FilterEnabled = true,
                KeyboardNavigationEnabled = PromptKeyboardNavigationEnabled,
                PageSize = PromptPageSize,
                ShowOptionDescriptions = true
            };

            List<ViewOption> selectableOptions = new List<ViewOption>();
            foreach (ViewOption option in Options)
            {
                if (ReferenceEquals(option, excludedOption))
                {
                    continue;
                }

                if (option.IsSeparator)
                {
                    prompt.AddSeparator(option.Render(), option.Color);
                    continue;
                }

                selectableOptions.Add(option);
                AddPromptOption(prompt, option.Render(), option.Description, option.Shortcut, option.Color, option.SearchTerms);
            }

            PromptResult result = prompt.Render(cancellationToken);
            if (!result.HasSelection || result.IsCanceled || cancellationToken.IsCancellationRequested)
            {
                return result;
            }

            if (result.Index >= 0 && result.Index < selectableOptions.Count)
            {
                selectableOptions[result.Index].Action.Invoke();
            }

            return result;
        }

        private static void AddPromptOption(SelectionPrompt prompt, string label, string description, string shortcut, ConsoleColor color, IEnumerable<string> searchTerms)
        {
            int index;
            if (string.IsNullOrWhiteSpace(description))
            {
                if (string.IsNullOrWhiteSpace(shortcut))
                {
                    index = prompt.Add(label, color);
                }
                else
                {
                    index = prompt.AddShortcut(label, shortcut, color);
                }

                prompt.AddSearchTerms(index, searchTerms);
                return;
            }

            if (string.IsNullOrWhiteSpace(shortcut))
            {
                index = prompt.AddDescribed(label, description, color);
            }
            else
            {
                index = prompt.AddDescribedShortcut(label, description, shortcut, color);
            }

            prompt.AddSearchTerms(index, searchTerms);
        }

        private static ConsoleColor ResolveStyle(string style, ConsoleColor fallback)
        {
            return string.IsNullOrWhiteSpace(style)
                ? fallback
                : Style.ParseForeground(style, fallback);
        }

        private static ConsoleColor ResolveStyle(Style style, ConsoleColor fallback)
        {
            return style == null ? fallback : style.ToForegroundColor(fallback);
        }

        private RenderOptions.ViewErrorModes EffectiveViewErrorMode => ViewErrorMode ?? RenderOptions.ViewErrorMode;

        private bool EffectiveWaitOnError => WaitOnError ?? RenderOptions.WaitOnError;

        private bool ShouldThrowViewError()
        {
            return EffectiveViewErrorMode == RenderOptions.ViewErrorModes.Throw;
        }

        private void RenderViewError(Exception exception, string message, CancellationToken cancellationToken)
        {
            Consoul.Write(exception, message, true, RenderOptions.InvalidColor);
            if (EffectiveWaitOnError)
            {
                Consoul.Wait(cancellationToken: cancellationToken);
            }
        }

        private static IEnumerable<string> SplitSearchTerms(string searchTerms)
        {
            if (string.IsNullOrWhiteSpace(searchTerms))
            {
                return Enumerable.Empty<string>();
            }

            return searchTerms
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(term => term.Trim())
                .Where(term => term.Length > 0);
        }
    }
}
