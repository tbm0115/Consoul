using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Views;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// An abstract view that relies on an underlying model to dynamically change the labels and colors of choices whenever the view re-renders.
    /// </summary>
    /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
    public class DynamicView<T> : IView, INavigationAwareView
    {
        private bool _goBackRequested = false;
        private string _goBackMessage = RenderOptions.DefaultGoBackMessage;
        private ViewNavigationContext _navigationContext = new ViewNavigationContext();
        private ViewChromeOptions _chrome = new ViewChromeOptions();
        private Action<T> _modelRenderer;
        private readonly List<Action<T>> _modelSections = new List<Action<T>>();
        private Func<T, string> _titleBuilder;

        /// <summary>
        /// Gets or sets the label used for the generated navigation option that returns to the previous view.
        /// </summary>
        protected string GoBackMessage
        {
            get
            {
                return _goBackMessage;
            }
            set
            {
                _goBackMessage = value;
            }
        }

        internal string _title;
        /// <summary>
        /// Text displayed at the top of the console
        /// </summary>
        public string Title { get => _title; set { _title = value; } }

        internal List<DynamicOption<T>> _options = new List<DynamicOption<T>>();
        /// <summary>
        /// Collection of view options
        /// </summary>
        public IEnumerable<DynamicOption<T>> Options => _options;

        /// <summary>
        /// Flag indicating whether or not an underlying process has requested this view to go back.
        /// </summary>
        public bool GoBackRequested => _goBackRequested;

        ViewNavigationContext INavigationAwareView.NavigationContext
        {
            get => _navigationContext;
            set => _navigationContext = value ?? new ViewNavigationContext();
        }

        /// <summary>
        /// Reference to the source model of the dynamic view.
        /// </summary>
        public T Model { get; set; }

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
        /// Initializes a dynamic view and discovers attributed option methods.
        /// </summary>
        /// <param name="callback">Optional callback invoked after an option is selected.</param>
        public DynamicView(ChoiceCallback callback = null)
        {
            OnOptionSelected = callback;

            Type thisType = this.GetType();

            Type viewType = typeof(ViewAttribute);
            ViewAttribute viewAttr = thisType.GetCustomAttribute(viewType) as ViewAttribute;
            if (viewAttr != null)
            {
                Title = BannerEntry.Render(viewAttr.Title);
                _goBackMessage = viewAttr.GoBackMessage;
            }

            // Build the options from local methods decorated with ViewOption
            Type viewOptionType = typeof(DynamicViewOptionAttribute);
            IEnumerable<MethodInfo> allMethods = thisType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable<MethodInfo> viewOptionMethods = allMethods.Where(o => o.GetCustomAttribute(viewOptionType) != null);
            foreach (MethodInfo method in viewOptionMethods)
            {
                DynamicViewOptionAttribute attr = method.GetCustomAttribute(viewOptionType) as DynamicViewOptionAttribute;
                if (attr == null)
                    continue;

                MethodInfo messageBuilder = allMethods.FirstOrDefault(o => o.Name == attr.MessageMethod);
                MethodInfo colorBuilder = null;
                if (!string.IsNullOrEmpty(attr.ColorMethod))
                    colorBuilder = allMethods.FirstOrDefault(o => o.Name == attr.ColorMethod);
                MethodInfo descriptionBuilder = null;
                if (!string.IsNullOrEmpty(attr.DescriptionMethod))
                    descriptionBuilder = allMethods.FirstOrDefault(o => o.Name == attr.DescriptionMethod);
                MethodInfo shortcutBuilder = null;
                if (!string.IsNullOrEmpty(attr.ShortcutMethod))
                    shortcutBuilder = allMethods.FirstOrDefault(o => o.Name == attr.ShortcutMethod);
                MethodInfo searchTermsBuilder = null;
                if (!string.IsNullOrEmpty(attr.SearchTermsMethod))
                    searchTermsBuilder = allMethods.FirstOrDefault(o => o.Name == attr.SearchTermsMethod);

                if (messageBuilder == null)
                    continue;

                ParameterInfo[] methodParameters = method.GetParameters();
                List<object> implementedMethodParameters = new List<object>();
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

                DynamicOption<T> option = new DynamicOption<T>(
                    () =>
                    {
                        return thisType.InvokeMember(
                            attr.MessageMethod,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                            null,
                            this,
                            null
                        ).ToString();
                    },
                    () => thisType.InvokeMember(
                        method.Name,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                        null,
                        this,
                        useParameters ? implementedMethodParameters.ToArray() : null
                    ),
                    () =>
                    {
                        return colorBuilder != null
                        ? (ConsoleColor)thisType.InvokeMember(
                                attr.ColorMethod,
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                null,
                                this,
                                null
                            )
                        : RenderOptions.DefaultColor;
                    }
                );
                if (descriptionBuilder != null)
                {
                    option.Describe(() =>
                    {
                        object description = thisType.InvokeMember(
                            attr.DescriptionMethod,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                            null,
                            this,
                            null
                        );
                        return description != null ? description.ToString() : null;
                    });
                }
                if (shortcutBuilder != null)
                {
                    option.UseShortcut(() =>
                    {
                        object shortcut = thisType.InvokeMember(
                            attr.ShortcutMethod,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                            null,
                            this,
                            null
                        );
                        return shortcut != null ? shortcut.ToString() : null;
                    });
                }
                if (searchTermsBuilder != null)
                {
                    option.UseSearchTerms(() =>
                    {
                        object searchTerms = thisType.InvokeMember(
                            attr.SearchTermsMethod,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                            null,
                            this,
                            null
                        );
                        return NormalizeSearchTerms(searchTerms);
                    });
                }

                _options.Add(option);
            }
        }

        /// <summary>
        /// Initializes a dynamic view with a title, model, and optional go-back label.
        /// </summary>
        /// <param name="title">The title rendered above the view prompt.</param>
        /// <param name="model">The model rendered and acted on by the view.</param>
        /// <param name="goBackMessage">Optional label for the generated option that exits the view.</param>
        /// <param name="callback">Optional callback invoked after an option is selected.</param>
        public DynamicView(string title, T model, string goBackMessage = null, ChoiceCallback callback = null) : this(callback)
        {
            Title = title;
            Model = model;
            if (!string.IsNullOrEmpty(goBackMessage))
            {
                _goBackMessage = goBackMessage;
            }
        }

        /// <summary>
        /// Sets a dynamic title builder that is evaluated each time the view renders.
        /// </summary>
        /// <param name="titleBuilder">Function that creates a title from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> WithTitle(Func<T, string> titleBuilder)
        {
            _titleBuilder = titleBuilder ?? throw new ArgumentNullException(nameof(titleBuilder));
            return this;
        }

        /// <summary>
        /// Sets content that renders from the model before the option prompt.
        /// </summary>
        /// <param name="renderer">Action that writes model content to the console.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RenderModel(Action<T> renderer)
        {
            _modelRenderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            return this;
        }

        /// <summary>
        /// Sets reusable content that renders before the option prompt.
        /// </summary>
        /// <param name="renderable">Renderable component written each time the view renders.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RenderModel(IConsoulRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return RenderModel(model => Consoul.Render(renderable));
        }

        /// <summary>
        /// Sets reusable content that renders from the model before the option prompt.
        /// </summary>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RenderModel(Func<T, IConsoulRenderable> renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            return RenderModel(model =>
            {
                IConsoulRenderable renderable = renderer(model);
                if (renderable != null)
                {
                    Consoul.Render(renderable);
                }
            });
        }

        /// <summary>
        /// Adds a display-only separator that visually groups dynamic view options.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Optional separator foreground color.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Separator(string label = null, ConsoleColor? color = null)
        {
            _options.Add(DynamicOption<T>.Separator(label, color));
            return this;
        }

        /// <summary>
        /// Adds a display-only separator that visually groups dynamic view options using style text.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Separator(string label, string style)
        {
            _options.Add(DynamicOption<T>.Separator(label, style));
            return this;
        }

        /// <summary>
        /// Adds a display-only separator that visually groups dynamic view options using a reusable style.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Separator(string label, Style style)
        {
            _options.Add(DynamicOption<T>.Separator(label, style));
            return this;
        }

        /// <summary>
        /// Adds a display-only separator that visually groups dynamic view options using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the separator label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Separator(string label, SpectreColor color)
        {
            _options.Add(DynamicOption<T>.Separator(label, color));
            return this;
        }

        /// <summary>
        /// Adds a display-only separator that visually groups dynamic view options using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the separator label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Separator(string label, RgbColor color)
        {
            _options.Add(DynamicOption<T>.Separator(label, color));
            return this;
        }

        /// <summary>
        /// Adds a display-only separator that visually groups dynamic view options using style text.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SeparatorStyled(string label = null, string style = null)
        {
            _options.Add(DynamicOption<T>.SeparatorStyled(label, style));
            return this;
        }

        /// <summary>
        /// Configures optional navigation chrome for this view.
        /// </summary>
        /// <param name="configure">Action used to configure the chrome options.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> UseChrome(Action<ViewChromeOptions> configure)
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
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShowBreadcrumbs(BreadcrumbOptions options = null)
        {
            Chrome.ShowBreadcrumbs = true;
            Chrome.BreadcrumbOptions = options;
            return this;
        }

        /// <summary>
        /// Enables shortcut hint chrome above the view.
        /// </summary>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShowShortcuts(ShortcutBarOptions options = null)
        {
            Chrome.ShowShortcuts = true;
            Chrome.ShortcutBarOptions = options;
            return this;
        }

        /// <summary>
        /// Sets the error-handling mode for this view only.
        /// </summary>
        /// <param name="mode">The error-handling mode, or <see langword="null"/> to use <see cref="RenderOptions.ViewErrorMode"/>.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> UseErrorMode(RenderOptions.ViewErrorModes? mode)
        {
            ViewErrorMode = mode;
            return this;
        }

        /// <summary>
        /// Configures this view to rethrow rendering and option exceptions after writing the error details.
        /// </summary>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ThrowOnError()
        {
            return UseErrorMode(RenderOptions.ViewErrorModes.Throw);
        }

        /// <summary>
        /// Configures this view to write rendering and option exceptions, then continue view navigation where possible.
        /// </summary>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RenderErrorsAndContinue()
        {
            return UseErrorMode(RenderOptions.ViewErrorModes.RenderAndContinue);
        }

        /// <summary>
        /// Configures whether this view waits for input after writing an error.
        /// </summary>
        /// <param name="wait">Whether the view should wait after rendering an error.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> WaitOnRenderError(bool wait = true)
        {
            WaitOnError = wait;
            return this;
        }

        /// <summary>
        /// Clears view-local error handling so this view follows the global <see cref="RenderOptions"/> values.
        /// </summary>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> UseGlobalErrorHandling()
        {
            ViewErrorMode = null;
            WaitOnError = null;
            return this;
        }

        /// <summary>
        /// Creates a reusable help surface from the current dynamic view options.
        /// </summary>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        /// <param name="includeGoBack">Indicates whether the generated go-back option should be included.</param>
        /// <returns>A renderable help surface describing the current view options.</returns>
        public ViewHelpEntry CreateHelp(string title = null, ViewHelpOptions options = null, bool includeGoBack = true)
        {
            var help = new ViewHelpEntry(ResolveHelpTitle(title), options);
            foreach (DynamicOption<T> option in Options)
            {
                if (option.IsSeparator)
                {
                    help.Add(option.ToHelpItem(option.Entry.SetMessage()));
                    continue;
                }

                string disabledReason = option.IsEnabled ? null : option.DisabledReason;
                help.Add(option.ToHelpItem(option.Entry.SetMessage(), disabledReason));
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
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> HelpOption(
            string label = "Help",
            string title = null,
            ConsoleColor? color = null,
            string shortcut = "?",
            ViewHelpOptions options = null,
            bool waitForInput = true)
        {
            AddOption(
                model => label,
                (model, cancellationToken) =>
                {
                    ShowHelp(title, options, waitForInput, cancellationToken);
                    return Task.CompletedTask;
                },
                model => color ?? RenderOptions.SubnoteColor,
                null,
                null,
                model => "Show available actions for this view.");

            DynamicOption<T> helpOption = GetLastSelectableOption("assign a shortcut");
            helpOption.UseShortcut(shortcut);
            return this;
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
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> HelpOptionStyled(
            string label = "Help",
            string title = null,
            string style = null,
            string shortcut = "?",
            ViewHelpOptions options = null,
            bool waitForInput = true)
        {
            return HelpOption(label, title, ResolveStyle(style, RenderOptions.SubnoteColor), shortcut, options, waitForInput)
                .StyleLastOption(ParseStyle(style));
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
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> HelpOption(
            string label,
            string title,
            Style style,
            string shortcut = "?",
            ViewHelpOptions options = null,
            bool waitForInput = true)
        {
            return HelpOption(label, title, ResolveStyle(style, RenderOptions.SubnoteColor), shortcut, options, waitForInput)
                .StyleLastOption(style);
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
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> HelpOption(
            string label,
            string title,
            SpectreColor color,
            string shortcut = "?",
            ViewHelpOptions options = null,
            bool waitForInput = true)
        {
            return HelpOption(label, title, (ConsoleColor?)color.ToConsoleColor(), shortcut, options, waitForInput)
                .ColorLastOption(color.ToRgbColor());
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
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> HelpOption(
            string label,
            string title,
            RgbColor color,
            string shortcut = "?",
            ViewHelpOptions options = null,
            bool waitForInput = true)
        {
            return HelpOption(label, title, (ConsoleColor?)color.ToConsoleColor(), shortcut, options, waitForInput)
                .ColorLastOption(color);
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
            return ShowCommandPaletteAsync(message, clear, cancellationToken).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Shows a searchable command palette for the current view options and invokes the selected action.
        /// </summary>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The result returned by the palette prompt.</returns>
        public Task<PromptResult> ShowCommandPaletteAsync(string message = "Command palette", bool clear = true, CancellationToken cancellationToken = default)
        {
            return ShowCommandPaletteAsync(message, clear, null, cancellationToken);
        }

        /// <summary>
        /// Adds a selectable command palette option that searches and invokes the current view's actions.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="color">Optional foreground color for the palette option.</param>
        /// <param name="shortcut">Optional shortcut that selects the palette option.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CommandPaletteOption(
            string label = "Command palette",
            string message = "Command palette",
            ConsoleColor? color = null,
            string shortcut = "cmd",
            bool clear = true)
        {
            DynamicOption<T> paletteOption = null;
            paletteOption = new DynamicOption<T>(
                () => label,
                cancellationToken => ShowCommandPaletteAsync(message, clear, paletteOption, cancellationToken),
                () => color ?? RenderOptions.SubnoteColor)
                .Describe("Search and run an action from this view.")
                .UseShortcut(shortcut);

            _options.Add(paletteOption);
            return this;
        }

        /// <summary>
        /// Adds a selectable command palette option using style text for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="style">Optional foreground style for the palette option.</param>
        /// <param name="shortcut">Optional shortcut that selects the palette option.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CommandPaletteOptionStyled(
            string label = "Command palette",
            string message = "Command palette",
            string style = null,
            string shortcut = "cmd",
            bool clear = true)
        {
            return CommandPaletteOption(label, message, ResolveStyle(style, RenderOptions.SubnoteColor), shortcut, clear)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds a selectable command palette option using a reusable style for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="style">Optional foreground style for the palette option.</param>
        /// <param name="shortcut">Optional shortcut that selects the palette option.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CommandPaletteOption(
            string label,
            string message,
            Style style,
            string shortcut = "cmd",
            bool clear = true)
        {
            return CommandPaletteOption(label, message, ResolveStyle(style, RenderOptions.SubnoteColor), shortcut, clear)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a selectable command palette option using a Spectre-style color facade for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the palette option.</param>
        /// <param name="shortcut">Optional shortcut that selects the palette option.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CommandPaletteOption(
            string label,
            string message,
            SpectreColor color,
            string shortcut = "cmd",
            bool clear = true)
        {
            return CommandPaletteOption(label, message, (ConsoleColor?)color.ToConsoleColor(), shortcut, clear)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds a selectable command palette option using an RGB color mapped to Consoul's console palette for the option label.
        /// </summary>
        /// <param name="label">Label shown in the view prompt.</param>
        /// <param name="message">Prompt message rendered above the palette.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the palette option.</param>
        /// <param name="shortcut">Optional shortcut that selects the palette option.</param>
        /// <param name="clear">Indicates whether the console should be cleared while the palette is shown.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CommandPaletteOption(
            string label,
            string message,
            RgbColor color,
            string shortcut = "cmd",
            bool clear = true)
        {
            return CommandPaletteOption(label, message, (ConsoleColor?)color.ToConsoleColor(), shortcut, clear)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a named model section that renders after the primary model renderer and before the option prompt.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(string title, Action<T> renderer, ConsoleColor? titleColor = null, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return Section(model => title, renderer, model => titleColor, alignment);
        }

        /// <summary>
        /// Adds a named model section using style text for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(string title, Action<T> renderer, string titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionStyled(model => title, renderer, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section using a reusable style for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered in a rule above the section content.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(string title, Action<T> renderer, Style titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionStyled(model => title, renderer, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section with a title evaluated from the current model state.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleColor">Optional function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(Func<T, string> title, Action<T> renderer, Func<T, ConsoleColor?> titleColor = null, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            AddSection(title, renderer, null, titleColor, alignment);
            return this;
        }

        /// <summary>
        /// Adds a named model section using style text evaluated from the current model state.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleStyle">Optional function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(Func<T, string> title, Action<T> renderer, Func<T, string> titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionStyled(title, renderer, RuleTitleStyle(titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section with a dynamic title and reusable style evaluated from the current model.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleStyle">Optional function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(Func<T, string> title, Action<T> renderer, Func<T, Style> titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionStyled(title, renderer, RuleTitleStyle(titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section with a Spectre-style title color evaluated from the current model.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(Func<T, string> title, Action<T> renderer, Func<T, SpectreColor> titleColor, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionStyled(title, renderer, RuleTitleStyle(titleColor), alignment);
        }

        /// <summary>
        /// Adds a named model section with an RGB title color evaluated from the current model.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(Func<T, string> title, Action<T> renderer, Func<T, RgbColor> titleColor, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionStyled(title, renderer, RuleTitleStyle(titleColor), alignment);
        }

        /// <summary>
        /// Adds a named section that renders a reusable component after the primary model renderer and before the option prompt.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the view renders.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(string title, IConsoulRenderable renderable, ConsoleColor? titleColor = null, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return Section(title, model => Consoul.Render(renderable), titleColor, alignment);
        }

        /// <summary>
        /// Adds a named section that renders a reusable component using style text for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the view renders.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(string title, IConsoulRenderable renderable, string titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return SectionStyled(model => title, model => Consoul.Render(renderable), RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named section that renders a reusable component using a reusable style for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered in a rule above the component.</param>
        /// <param name="renderable">Renderable component written each time the view renders.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(string title, IConsoulRenderable renderable, Style titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return SectionStyled(model => title, model => Consoul.Render(renderable), RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component from the current model state.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(string title, Func<T, IConsoulRenderable> renderer, ConsoleColor? titleColor = null, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return Section(model => title, renderer, model => titleColor, alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component using style text for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(string title, Func<T, IConsoulRenderable> renderer, string titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableStyled(model => title, renderer, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component using a reusable style for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered in a rule above the component.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(string title, Func<T, IConsoulRenderable> renderer, Style titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableStyled(model => title, renderer, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component with a title evaluated from the current model state.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleColor">Optional function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, ConsoleColor?> titleColor = null, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            return Section(title, model =>
            {
                IConsoulRenderable renderable = renderer(model);
                if (renderable != null)
                {
                    Consoul.Render(renderable);
                }
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component using style text evaluated from the current model state.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleStyle">Optional function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, string> titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableStyled(title, renderer, RuleTitleStyle(titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component and evaluates a reusable title style from the current model.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleStyle">Optional function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, Style> titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableStyled(title, renderer, RuleTitleStyle(titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component and evaluates a Spectre-style title color from the current model.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, SpectreColor> titleColor, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableStyled(title, renderer, RuleTitleStyle(titleColor), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component and evaluates an RGB title color from the current model.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Section(Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, RgbColor> titleColor, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableStyled(title, renderer, RuleTitleStyle(titleColor), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(string title, Action<T> renderer, Func<T, bool> isVisible, ConsoleColor? titleColor = null, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionWhen(model => title, renderer, isVisible, model => titleColor, alignment);
        }

        /// <summary>
        /// Adds a named model section that renders only while visible using style text for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(string title, Action<T> renderer, Func<T, bool> isVisible, string titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionWhenStyled(model => title, renderer, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders only while visible using a reusable style for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered in a rule above the section content.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(string title, Action<T> renderer, Func<T, bool> isVisible, Style titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionWhenStyled(model => title, renderer, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section with a dynamic title that renders only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Optional function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(Func<T, string> title, Action<T> renderer, Func<T, bool> isVisible, Func<T, ConsoleColor?> titleColor = null, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            AddSection(title, renderer, isVisible, titleColor, alignment);
            return this;
        }

        /// <summary>
        /// Adds a named model section that renders only while visible using style text evaluated from the current model state.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(Func<T, string> title, Action<T> renderer, Func<T, bool> isVisible, Func<T, string> titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionWhenStyled(title, renderer, isVisible, RuleTitleStyle(titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section with a dynamic title that renders only while visible using a reusable style evaluated from the current model.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(Func<T, string> title, Action<T> renderer, Func<T, bool> isVisible, Func<T, Style> titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionWhenStyled(title, renderer, isVisible, RuleTitleStyle(titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section with a dynamic title that renders only while visible using a Spectre-style title color.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(Func<T, string> title, Action<T> renderer, Func<T, bool> isVisible, Func<T, SpectreColor> titleColor, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionWhenStyled(title, renderer, isVisible, RuleTitleStyle(titleColor), alignment);
        }

        /// <summary>
        /// Adds a named model section with a dynamic title that renders only while visible using an RGB title color.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(Func<T, string> title, Action<T> renderer, Func<T, bool> isVisible, Func<T, RgbColor> titleColor, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionWhenStyled(title, renderer, isVisible, RuleTitleStyle(titleColor), alignment);
        }

        /// <summary>
        /// Adds a named section that renders a reusable component only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the section is visible.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(string title, IConsoulRenderable renderable, Func<T, bool> isVisible, ConsoleColor? titleColor = null, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return SectionWhen(title, model => Consoul.Render(renderable), isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a named section that renders a reusable component only while visible using style text for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the section is visible.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(string title, IConsoulRenderable renderable, Func<T, bool> isVisible, string titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return SectionWhenStyled(model => title, model => Consoul.Render(renderable), isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named section that renders a reusable component only while visible using a reusable style for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered in a rule above the component.</param>
        /// <param name="renderable">Renderable component written each time the view renders.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(string title, IConsoulRenderable renderable, Func<T, bool> isVisible, Style titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return SectionWhenStyled(model => title, model => Consoul.Render(renderable), isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(string title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, ConsoleColor? titleColor = null, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionWhen(model => title, renderer, isVisible, model => titleColor, alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component only while visible using style text for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(string title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, string titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableWhenStyled(model => title, renderer, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component only while visible using a reusable style for the section rule title.
        /// </summary>
        /// <param name="title">Section title rendered in a rule above the component.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(string title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Style titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableWhenStyled(model => title, renderer, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component and dynamic title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Optional function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Func<T, ConsoleColor?> titleColor = null, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            return SectionWhen(title, model =>
            {
                IConsoulRenderable renderable = renderer(model);
                if (renderable != null)
                {
                    Consoul.Render(renderable);
                }
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component only while visible using style text evaluated from the current model state.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Func<T, string> titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableWhenStyled(title, renderer, isVisible, RuleTitleStyle(titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component and dynamic title only while visible using a reusable style evaluated from the current model.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Func<T, Style> titleStyle, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableWhenStyled(title, renderer, isVisible, RuleTitleStyle(titleStyle), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component only while visible using a Spectre-style title color.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Func<T, SpectreColor> titleColor, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableWhenStyled(title, renderer, isVisible, RuleTitleStyle(titleColor), alignment);
        }

        /// <summary>
        /// Adds a named model section that renders a reusable component only while visible using an RGB title color.
        /// </summary>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SectionWhen(Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Func<T, RgbColor> titleColor, RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionRenderableWhenStyled(title, renderer, isVisible, RuleTitleStyle(titleColor), alignment);
        }

        /// <summary>
        /// Adds a generated property-list section for the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="options">Optional property-list rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="bindingAttr">Binding flags describing the model properties to render.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PropertyListSection(
            string title = "Summary",
            PropertyListOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            return PropertyListSection(title, model => model, options, titleColor, alignment, bindingAttr);
        }

        /// <summary>
        /// Adds a generated property-list section for a model value selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="modelSelector">Function that selects the object whose public properties should be rendered.</param>
        /// <param name="options">Optional property-list rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="bindingAttr">Binding flags describing the selected model properties to render.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PropertyListSection(
            string title,
            Func<T, object> modelSelector,
            PropertyListOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            if (modelSelector == null)
            {
                throw new ArgumentNullException(nameof(modelSelector));
            }

            return Section(title, model =>
            {
                object selectedModel = modelSelector(model);
                if (selectedModel == null)
                {
                    return;
                }

                new PropertyListEntry(options)
                    .AddModel(selectedModel, bindingAttr)
                    .Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated property-list section for the current model using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="options">Optional property-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="bindingAttr">Binding flags describing the model properties to render.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PropertyListSectionStyled(
            string title = "Summary",
            PropertyListOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            return PropertyListSectionStyledCore(title, model => model, options, RuleTitleStyle(model => titleStyle), alignment, bindingAttr);
        }

        /// <summary>
        /// Adds a generated property-list section for a selected model value using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="modelSelector">Function that selects the object whose public properties should be rendered.</param>
        /// <param name="options">Optional property-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="bindingAttr">Binding flags describing the selected model properties to render.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PropertyListSectionStyled(
            string title,
            Func<T, object> modelSelector,
            PropertyListOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            return PropertyListSectionStyledCore(title, modelSelector, options, RuleTitleStyle(model => titleStyle), alignment, bindingAttr);
        }

        /// <summary>
        /// Adds a generated property-list section for the current model using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="options">Optional property-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="bindingAttr">Binding flags describing the model properties to render.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PropertyListSection(
            string title,
            PropertyListOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            return PropertyListSectionStyledCore(title, model => model, options, RuleTitleStyle(model => titleStyle), alignment, bindingAttr);
        }

        /// <summary>
        /// Adds a generated property-list section for a selected model value using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="modelSelector">Function that selects the object whose public properties should be rendered.</param>
        /// <param name="options">Optional property-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="bindingAttr">Binding flags describing the selected model properties to render.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PropertyListSection(
            string title,
            Func<T, object> modelSelector,
            PropertyListOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            return PropertyListSectionStyledCore(title, modelSelector, options, RuleTitleStyle(model => titleStyle), alignment, bindingAttr);
        }

        /// <summary>
        /// Adds a generated dynamic table section for a collection selected from the current model.
        /// </summary>
        /// <typeparam name="TItem">The row item type rendered by the table.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the table row items from the current model.</param>
        /// <param name="selectors">Optional property selectors rendered as columns. When omitted, public scalar properties are used.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TableSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            params Expression<Func<TItem, object>>[] selectors) where TItem : class
        {
            return TableSection(title, itemsSelector, null, (ConsoleColor?)null, RuleTitleAlignment.Left, selectors);
        }

        /// <summary>
        /// Adds a generated dynamic table section for a collection selected from the current model.
        /// </summary>
        /// <typeparam name="TItem">The row item type rendered by the table.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the table row items from the current model.</param>
        /// <param name="options">Optional table rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="selectors">Optional property selectors rendered as columns. When omitted, public scalar properties are used.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TableSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            TableRenderOptions options,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            params Expression<Func<TItem, object>>[] selectors) where TItem : class
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                DynamicTableView<TItem>.Create(items, options, selectors).RenderInline();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated dynamic table section using style text for the section title.
        /// </summary>
        /// <typeparam name="TItem">The row item type rendered by the table.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the table row items from the current model.</param>
        /// <param name="options">Optional table rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="selectors">Optional property selectors rendered as columns. When omitted, public scalar properties are used.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TableSectionStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            TableRenderOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            params Expression<Func<TItem, object>>[] selectors) where TItem : class
        {
            return TableSectionStyledCore(title, itemsSelector, options, RuleTitleStyle(model => titleStyle), alignment, selectors);
        }

        /// <summary>
        /// Adds a generated dynamic table section using a reusable section title style.
        /// </summary>
        /// <typeparam name="TItem">The row item type rendered by the table.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the table row items from the current model.</param>
        /// <param name="options">Optional table rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="selectors">Optional property selectors rendered as columns. When omitted, public scalar properties are used.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TableSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            TableRenderOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            params Expression<Func<TItem, object>>[] selectors) where TItem : class
        {
            return TableSectionStyledCore(title, itemsSelector, options, RuleTitleStyle(model => titleStyle), alignment, selectors);
        }

        /// <summary>
        /// Adds a generated metrics section for metric items selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the metric items from the current model.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSection(
            string title,
            Func<T, IEnumerable<MetricItem>> metricsSelector,
            MetricsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (metricsSelector == null)
            {
                throw new ArgumentNullException(nameof(metricsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<MetricItem> metrics = metricsSelector(model) ?? Enumerable.Empty<MetricItem>();
                new Metrics(metrics, options).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated metrics section for metric items selected from the current model using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the metric items from the current model.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSectionStyled(
            string title,
            Func<T, IEnumerable<MetricItem>> metricsSelector,
            MetricsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSectionStyledCore(title, metricsSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated metrics section for metric items selected from the current model using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the metric items from the current model.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSection(
            string title,
            Func<T, IEnumerable<MetricItem>> metricsSelector,
            MetricsOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSectionStyledCore(title, metricsSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated metrics section for a collection selected from the current model.
        /// </summary>
        /// <typeparam name="TMetric">The model item type projected into metric items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the source metric models from the current model.</param>
        /// <param name="labelSelector">Function that selects each metric label.</param>
        /// <param name="valueSelector">Function that selects each metric value.</param>
        /// <param name="unitSelector">Optional function that selects each metric unit.</param>
        /// <param name="noteSelector">Optional function that selects each metric note.</param>
        /// <param name="kindSelector">Optional function that selects each metric visual intent.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSection<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            Func<TMetric, string> unitSelector = null,
            Func<TMetric, string> noteSelector = null,
            Func<TMetric, MetricKind> kindSelector = null,
            MetricsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (metricsSelector == null)
            {
                throw new ArgumentNullException(nameof(metricsSelector));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TMetric> metrics = metricsSelector(model) ?? Enumerable.Empty<TMetric>();
                new Metrics(options)
                    .AddMetrics(metrics, labelSelector, valueSelector, unitSelector, noteSelector, kindSelector)
                    .Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated metrics section for a collection selected from the current model with explicit rendering options.
        /// </summary>
        /// <typeparam name="TMetric">The model item type projected into metric items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the source metric models from the current model.</param>
        /// <param name="labelSelector">Function that selects each metric label.</param>
        /// <param name="valueSelector">Function that selects each metric value.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSection<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            MetricsOptions options,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSection(title, metricsSelector, labelSelector, valueSelector, null, null, null, options, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated metrics section using style text for the section title.
        /// </summary>
        /// <typeparam name="TMetric">The model item type projected into metric items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the source metric models from the current model.</param>
        /// <param name="labelSelector">Function that selects each metric label.</param>
        /// <param name="valueSelector">Function that selects each metric value.</param>
        /// <param name="unitSelector">Optional function that selects each metric unit.</param>
        /// <param name="noteSelector">Optional function that selects each metric note.</param>
        /// <param name="kindSelector">Optional function that selects each metric visual intent.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSectionStyled<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            Func<TMetric, string> unitSelector = null,
            Func<TMetric, string> noteSelector = null,
            Func<TMetric, MetricKind> kindSelector = null,
            MetricsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSectionStyledCore(title, metricsSelector, labelSelector, valueSelector, unitSelector, noteSelector, kindSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated metrics section using a reusable section title style.
        /// </summary>
        /// <typeparam name="TMetric">The model item type projected into metric items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the source metric models from the current model.</param>
        /// <param name="labelSelector">Function that selects each metric label.</param>
        /// <param name="valueSelector">Function that selects each metric value.</param>
        /// <param name="unitSelector">Optional function that selects each metric unit.</param>
        /// <param name="noteSelector">Optional function that selects each metric note.</param>
        /// <param name="kindSelector">Optional function that selects each metric visual intent.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSection<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            Func<TMetric, string> unitSelector,
            Func<TMetric, string> noteSelector,
            Func<TMetric, MetricKind> kindSelector,
            MetricsOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSectionStyledCore(title, metricsSelector, labelSelector, valueSelector, unitSelector, noteSelector, kindSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated metrics section with explicit rendering options using a reusable section title style.
        /// </summary>
        /// <typeparam name="TMetric">The model item type projected into metric items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the source metric models from the current model.</param>
        /// <param name="labelSelector">Function that selects each metric label.</param>
        /// <param name="valueSelector">Function that selects each metric value.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSection<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            MetricsOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSection(title, metricsSelector, labelSelector, valueSelector, null, null, null, options, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated bar-chart section for chart items selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the chart items from the current model.</param>
        /// <param name="options">Optional bar-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BarChartSection(
            string title,
            Func<T, IEnumerable<BarChartItem>> itemsSelector,
            BarChartOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<BarChartItem> items = itemsSelector(model) ?? Enumerable.Empty<BarChartItem>();
                new BarChart(options).AddItems(items).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated bar-chart section for chart items using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the chart items from the current model.</param>
        /// <param name="options">Optional bar-chart rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BarChartSectionStyled(
            string title,
            Func<T, IEnumerable<BarChartItem>> itemsSelector,
            BarChartOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<BarChartItem> items = itemsSelector(model) ?? Enumerable.Empty<BarChartItem>();
                new BarChart(options).AddItems(items).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated bar-chart section for a collection selected from the current model.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="colorSelector">Optional function that selects each chart item bar color.</param>
        /// <param name="options">Optional bar-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BarChartSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            Func<TItem, ConsoleColor?> colorSelector = null,
            BarChartOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new BarChart(options)
                    .AddItems(items, labelSelector, valueSelector, colorSelector)
                    .Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated bar-chart section with explicit rendering options.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="options">Optional bar-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BarChartSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            BarChartOptions options,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return BarChartSection(title, itemsSelector, labelSelector, valueSelector, null, options, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated bar-chart section using style text for each bar and the section title.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="styleSelector">Optional function that selects each chart item bar style.</param>
        /// <param name="options">Optional bar-chart rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BarChartSectionStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            Func<TItem, string> styleSelector = null,
            BarChartOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var chart = new BarChart(options);
                if (styleSelector == null)
                {
                    chart.AddItems(items, labelSelector, valueSelector);
                }
                else
                {
                    chart.AddItems(items, labelSelector, valueSelector, styleSelector);
                }

                chart.Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated breakdown-chart section for chart items selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the chart items from the current model.</param>
        /// <param name="options">Optional breakdown-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreakdownChartSection(
            string title,
            Func<T, IEnumerable<BreakdownChartItem>> itemsSelector,
            BreakdownChartOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<BreakdownChartItem> items = itemsSelector(model) ?? Enumerable.Empty<BreakdownChartItem>();
                new BreakdownChart(options).AddItems(items).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated breakdown-chart section for chart items using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the chart items from the current model.</param>
        /// <param name="options">Optional breakdown-chart rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreakdownChartSectionStyled(
            string title,
            Func<T, IEnumerable<BreakdownChartItem>> itemsSelector,
            BreakdownChartOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<BreakdownChartItem> items = itemsSelector(model) ?? Enumerable.Empty<BreakdownChartItem>();
                new BreakdownChart(options).AddItems(items).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated breakdown-chart section for a collection selected from the current model.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="colorSelector">Optional function that selects each chart item segment color.</param>
        /// <param name="options">Optional breakdown-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreakdownChartSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            Func<TItem, ConsoleColor?> colorSelector = null,
            BreakdownChartOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new BreakdownChart(options)
                    .AddItems(items, labelSelector, valueSelector, colorSelector)
                    .Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated breakdown-chart section with explicit rendering options.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="options">Optional breakdown-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreakdownChartSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            BreakdownChartOptions options,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return BreakdownChartSection(title, itemsSelector, labelSelector, valueSelector, null, options, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated breakdown-chart section using style text for each segment and the section title.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="styleSelector">Optional function that selects each chart item segment style.</param>
        /// <param name="options">Optional breakdown-chart rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreakdownChartSectionStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            Func<TItem, string> styleSelector = null,
            BreakdownChartOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var chart = new BreakdownChart(options);
                if (styleSelector == null)
                {
                    chart.AddItems(items, labelSelector, valueSelector);
                }
                else
                {
                    chart.AddItems(items, labelSelector, valueSelector, styleSelector);
                }

                chart.Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated text-block section for model-projected explanatory text.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the text block content from the current model.</param>
        /// <param name="options">Optional text-block rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TextBlockSection(
            string title,
            Func<T, string> contentSelector,
            TextBlockOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return Section(title, model =>
            {
                new TextBlockEntry(contentSelector(model), options).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated text-block section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the text block content from the current model.</param>
        /// <param name="options">Optional text-block rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TextBlockSectionStyled(
            string title,
            Func<T, string> contentSelector,
            TextBlockOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return TextBlockSectionStyledCore(title, contentSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated text-block section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the text block content from the current model.</param>
        /// <param name="options">Optional text-block rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TextBlockSection(
            string title,
            Func<T, string> contentSelector,
            TextBlockOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return TextBlockSectionStyledCore(title, contentSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated panel section for model-projected text content.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the panel content from the current model.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PanelSection(
            string title,
            Func<T, string> contentSelector,
            PanelOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return Section(title, model =>
            {
                new PanelEntry(contentSelector(model), options).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated panel section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the panel content from the current model.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PanelSectionStyled(
            string title,
            Func<T, string> contentSelector,
            PanelOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return PanelSectionStyledCore(title, contentSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated panel section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the panel content from the current model.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PanelSection(
            string title,
            Func<T, string> contentSelector,
            PanelOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return PanelSectionStyledCore(title, contentSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated callout section for a model-projected status message.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="messageSelector">Function that selects the callout message from the current model.</param>
        /// <param name="kindSelector">Optional function that selects the callout visual intent from the current model.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalloutSection(
            string title,
            Func<T, string> messageSelector,
            Func<T, CalloutKind> kindSelector = null,
            CalloutOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (messageSelector == null)
            {
                throw new ArgumentNullException(nameof(messageSelector));
            }

            return Section(title, model =>
            {
                string message = messageSelector(model);
                CalloutKind kind = kindSelector != null ? kindSelector(model) : CalloutKind.Info;
                new CalloutEntry(message, kind, options).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated callout section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="messageSelector">Function that selects the callout message from the current model.</param>
        /// <param name="kindSelector">Optional function that selects the callout visual intent from the current model.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalloutSectionStyled(
            string title,
            Func<T, string> messageSelector,
            Func<T, CalloutKind> kindSelector = null,
            CalloutOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return CalloutSectionStyledCore(title, messageSelector, kindSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated callout section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="messageSelector">Function that selects the callout message from the current model.</param>
        /// <param name="kindSelector">Optional function that selects the callout visual intent from the current model.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalloutSection(
            string title,
            Func<T, string> messageSelector,
            Func<T, CalloutKind> kindSelector,
            CalloutOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return CalloutSectionStyledCore(title, messageSelector, kindSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated exception section that renders when the current model exposes an exception.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSection(
            string title,
            Func<T, Exception> exceptionSelector,
            ExceptionRenderOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ExceptionSection(title, exceptionSelector, (Func<T, string>)null, options, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated exception section with model-projected context text that renders when the current model exposes an exception.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="messageSelector">Optional function that selects context text from the current model.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSection(
            string title,
            Func<T, Exception> exceptionSelector,
            Func<T, string> messageSelector,
            ExceptionRenderOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (exceptionSelector == null)
            {
                throw new ArgumentNullException(nameof(exceptionSelector));
            }

            return SectionWhen(title, model =>
            {
                Exception exception = exceptionSelector(model);
                if (exception != null)
                {
                    string message = messageSelector != null ? messageSelector(model) : null;
                    new ExceptionEntry(exception, message, options).Render();
                }
            }, model => exceptionSelector(model) != null, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated exception section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSectionStyled(
            string title,
            Func<T, Exception> exceptionSelector,
            ExceptionRenderOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ExceptionSectionStyled(title, exceptionSelector, null, options, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated exception section with model-projected context text using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="messageSelector">Optional function that selects context text from the current model.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSectionStyled(
            string title,
            Func<T, Exception> exceptionSelector,
            Func<T, string> messageSelector,
            ExceptionRenderOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (exceptionSelector == null)
            {
                throw new ArgumentNullException(nameof(exceptionSelector));
            }

            return SectionWhen(title, model =>
            {
                Exception exception = exceptionSelector(model);
                if (exception != null)
                {
                    string message = messageSelector != null ? messageSelector(model) : null;
                    new ExceptionEntry(exception, message, options).Render();
                }
            }, model => exceptionSelector(model) != null, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated exception section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSection(
            string title,
            Func<T, Exception> exceptionSelector,
            ExceptionRenderOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ExceptionSection(title, exceptionSelector, null, options, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated exception section with model-projected context text using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="messageSelector">Optional function that selects context text from the current model.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSection(
            string title,
            Func<T, Exception> exceptionSelector,
            Func<T, string> messageSelector,
            ExceptionRenderOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (exceptionSelector == null)
            {
                throw new ArgumentNullException(nameof(exceptionSelector));
            }

            return SectionWhen(title, model =>
            {
                Exception exception = exceptionSelector(model);
                if (exception != null)
                {
                    string message = messageSelector != null ? messageSelector(model) : null;
                    new ExceptionEntry(exception, message, options).Render();
                }
            }, model => exceptionSelector(model) != null, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated breadcrumb section for a navigation trail selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects breadcrumb item labels from the current model.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreadcrumbSection(
            string title,
            Func<T, IEnumerable<string>> itemsSelector,
            BreadcrumbOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<string> items = itemsSelector(model) ?? Enumerable.Empty<string>();
                new BreadcrumbEntry(items, options).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated breadcrumb section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects breadcrumb item labels from the current model.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreadcrumbSectionStyled(
            string title,
            Func<T, IEnumerable<string>> itemsSelector,
            BreadcrumbOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<string> items = itemsSelector(model) ?? Enumerable.Empty<string>();
                new BreadcrumbEntry(items, options).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated breadcrumb section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects breadcrumb item labels from the current model.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreadcrumbSection(
            string title,
            Func<T, IEnumerable<string>> itemsSelector,
            BreadcrumbOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<string> items = itemsSelector(model) ?? Enumerable.Empty<string>();
                new BreadcrumbEntry(items, options).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated breadcrumb section for model-projected navigation segments.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into breadcrumb segments.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source breadcrumb segment models from the current model.</param>
        /// <param name="itemSelector">Function that selects each breadcrumb segment label.</param>
        /// <param name="colorSelector">Optional function that selects each breadcrumb segment foreground color.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreadcrumbSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> itemSelector,
            Func<TItem, ConsoleColor?> colorSelector = null,
            BreadcrumbOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (itemSelector == null)
            {
                throw new ArgumentNullException(nameof(itemSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var breadcrumb = new BreadcrumbEntry(options);
                foreach (TItem item in items)
                {
                    ConsoleColor? color = colorSelector != null ? colorSelector(item) : null;
                    if (color.HasValue)
                    {
                        breadcrumb.Add(itemSelector(item), color.Value);
                    }
                    else
                    {
                        breadcrumb.Add(itemSelector(item));
                    }
                }

                breadcrumb.Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated breadcrumb section for model-projected navigation segments using style text.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into breadcrumb segments.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source breadcrumb segment models from the current model.</param>
        /// <param name="itemSelector">Function that selects each breadcrumb segment label.</param>
        /// <param name="styleSelector">Optional function that selects style text for each breadcrumb segment.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreadcrumbSectionStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> itemSelector,
            Func<TItem, string> styleSelector = null,
            BreadcrumbOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (itemSelector == null)
            {
                throw new ArgumentNullException(nameof(itemSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var breadcrumb = new BreadcrumbEntry(options);
                if (styleSelector == null)
                {
                    breadcrumb.AddItems(items, itemSelector);
                }
                else
                {
                    breadcrumb.AddItems(items, itemSelector, styleSelector);
                }

                breadcrumb.Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section for action hints selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects shortcut items from the current model.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSection(
            string title,
            Func<T, IEnumerable<ShortcutItem>> itemsSelector,
            ShortcutBarOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<ShortcutItem> items = itemsSelector(model) ?? Enumerable.Empty<ShortcutItem>();
                new ShortcutBar(items, options).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects shortcut items from the current model.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSectionStyled(
            string title,
            Func<T, IEnumerable<ShortcutItem>> itemsSelector,
            ShortcutBarOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<ShortcutItem> items = itemsSelector(model) ?? Enumerable.Empty<ShortcutItem>();
                new ShortcutBar(items, options).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects shortcut items from the current model.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSection(
            string title,
            Func<T, IEnumerable<ShortcutItem>> itemsSelector,
            ShortcutBarOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<ShortcutItem> items = itemsSelector(model) ?? Enumerable.Empty<ShortcutItem>();
                new ShortcutBar(items, options).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section for model-projected action hints.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into shortcut hints.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source shortcut models from the current model.</param>
        /// <param name="keySelector">Function that selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Function that selects the shortcut action description.</param>
        /// <param name="disabledSelector">Optional function that selects whether each shortcut should render as disabled.</param>
        /// <param name="keyColorSelector">Optional function that selects each shortcut key foreground color.</param>
        /// <param name="descriptionColorSelector">Optional function that selects each shortcut description foreground color.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> keySelector,
            Func<TItem, string> descriptionSelector,
            Func<TItem, bool> disabledSelector = null,
            Func<TItem, ConsoleColor?> keyColorSelector = null,
            Func<TItem, ConsoleColor?> descriptionColorSelector = null,
            ShortcutBarOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (descriptionSelector == null)
            {
                throw new ArgumentNullException(nameof(descriptionSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new ShortcutBar(options)
                    .AddShortcuts(items, keySelector, descriptionSelector, disabledSelector, keyColorSelector, descriptionColorSelector)
                    .Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section for model-projected action hints using style text.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into shortcut hints.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source shortcut models from the current model.</param>
        /// <param name="keySelector">Function that selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Function that selects the shortcut action description.</param>
        /// <param name="keyStyleSelector">Optional function that selects style text for each shortcut key token.</param>
        /// <param name="descriptionStyleSelector">Optional function that selects style text for each shortcut description.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSectionStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> keySelector,
            Func<TItem, string> descriptionSelector,
            Func<TItem, string> keyStyleSelector = null,
            Func<TItem, string> descriptionStyleSelector = null,
            ShortcutBarOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ShortcutBarSectionStyled(
                title,
                itemsSelector,
                keySelector,
                descriptionSelector,
                null,
                keyStyleSelector,
                descriptionStyleSelector,
                options,
                titleStyle,
                alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section for model-projected action hints using style text and disabled state.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into shortcut hints.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source shortcut models from the current model.</param>
        /// <param name="keySelector">Function that selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Function that selects the shortcut action description.</param>
        /// <param name="disabledSelector">Optional function that selects whether each shortcut should render as disabled.</param>
        /// <param name="keyStyleSelector">Optional function that selects style text for each shortcut key token.</param>
        /// <param name="descriptionStyleSelector">Optional function that selects style text for each shortcut description.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSectionStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> keySelector,
            Func<TItem, string> descriptionSelector,
            Func<TItem, bool> disabledSelector,
            Func<TItem, string> keyStyleSelector,
            Func<TItem, string> descriptionStyleSelector = null,
            ShortcutBarOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (descriptionSelector == null)
            {
                throw new ArgumentNullException(nameof(descriptionSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new ShortcutBar(options)
                    .AddShortcuts(items, keySelector, descriptionSelector, disabledSelector, keyStyleSelector, descriptionStyleSelector)
                    .Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated calendar section for a month selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="monthSelector">Function that selects any date within the month to render.</param>
        /// <param name="options">Optional calendar rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalendarSection(
            string title,
            Func<T, DateTime> monthSelector,
            CalendarOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (monthSelector == null)
            {
                throw new ArgumentNullException(nameof(monthSelector));
            }

            return Section(title, model =>
            {
                new Calendar(monthSelector(model), options).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated calendar section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="monthSelector">Function that selects any date within the month to render.</param>
        /// <param name="options">Optional calendar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalendarSectionStyled(
            string title,
            Func<T, DateTime> monthSelector,
            CalendarOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (monthSelector == null)
            {
                throw new ArgumentNullException(nameof(monthSelector));
            }

            return Section(title, model =>
            {
                new Calendar(monthSelector(model), options).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated calendar section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="monthSelector">Function that selects any date within the month to render.</param>
        /// <param name="options">Optional calendar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalendarSection(
            string title,
            Func<T, DateTime> monthSelector,
            CalendarOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (monthSelector == null)
            {
                throw new ArgumentNullException(nameof(monthSelector));
            }

            return Section(title, model =>
            {
                new Calendar(monthSelector(model), options).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated calendar section with highlighted dates projected from the current model.
        /// </summary>
        /// <typeparam name="TEvent">The model item type projected into highlighted calendar dates.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="monthSelector">Function that selects any date within the month to render.</param>
        /// <param name="eventsSelector">Function that selects the source event models from the current model.</param>
        /// <param name="dateSelector">Function that selects the date to highlight for each event.</param>
        /// <param name="labelSelector">Optional function that selects the label describing each event.</param>
        /// <param name="colorSelector">Optional function that selects each highlighted date foreground color.</param>
        /// <param name="options">Optional calendar rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalendarSection<TEvent>(
            string title,
            Func<T, DateTime> monthSelector,
            Func<T, IEnumerable<TEvent>> eventsSelector,
            Func<TEvent, DateTime> dateSelector,
            Func<TEvent, string> labelSelector = null,
            Func<TEvent, ConsoleColor?> colorSelector = null,
            CalendarOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (monthSelector == null)
            {
                throw new ArgumentNullException(nameof(monthSelector));
            }

            if (eventsSelector == null)
            {
                throw new ArgumentNullException(nameof(eventsSelector));
            }

            if (dateSelector == null)
            {
                throw new ArgumentNullException(nameof(dateSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TEvent> events = eventsSelector(model) ?? Enumerable.Empty<TEvent>();
                new Calendar(monthSelector(model), options)
                    .AddCalendarEvents(events, dateSelector, labelSelector, colorSelector)
                    .Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated calendar section using style text for highlighted dates and the section title.
        /// </summary>
        /// <typeparam name="TEvent">The model item type projected into highlighted calendar dates.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="monthSelector">Function that selects any date within the month to render.</param>
        /// <param name="eventsSelector">Function that selects the source event models from the current model.</param>
        /// <param name="dateSelector">Function that selects the date to highlight for each event.</param>
        /// <param name="labelSelector">Optional function that selects the label describing each event.</param>
        /// <param name="styleSelector">Optional function that selects style text for each highlighted date.</param>
        /// <param name="options">Optional calendar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalendarSectionStyled<TEvent>(
            string title,
            Func<T, DateTime> monthSelector,
            Func<T, IEnumerable<TEvent>> eventsSelector,
            Func<TEvent, DateTime> dateSelector,
            Func<TEvent, string> labelSelector = null,
            Func<TEvent, string> styleSelector = null,
            CalendarOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (monthSelector == null)
            {
                throw new ArgumentNullException(nameof(monthSelector));
            }

            if (eventsSelector == null)
            {
                throw new ArgumentNullException(nameof(eventsSelector));
            }

            if (dateSelector == null)
            {
                throw new ArgumentNullException(nameof(dateSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TEvent> events = eventsSelector(model) ?? Enumerable.Empty<TEvent>();
                new Calendar(monthSelector(model), options)
                    .AddCalendarEventsStyled(events, dateSelector, labelSelector, styleSelector)
                    .Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated bullet-list section for list items selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the bullet-list items from the current model.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSection(
            string title,
            Func<T, IEnumerable<BulletListItem>> itemsSelector,
            BulletListOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<BulletListItem> items = itemsSelector(model) ?? Enumerable.Empty<BulletListItem>();
                new BulletList(options).AddItems(items).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated bullet-list section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the bullet-list items from the current model.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSectionStyled(
            string title,
            Func<T, IEnumerable<BulletListItem>> itemsSelector,
            BulletListOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<BulletListItem> items = itemsSelector(model) ?? Enumerable.Empty<BulletListItem>();
                new BulletList(options).AddItems(items).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated bullet-list section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the bullet-list items from the current model.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSection(
            string title,
            Func<T, IEnumerable<BulletListItem>> itemsSelector,
            BulletListOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<BulletListItem> items = itemsSelector(model) ?? Enumerable.Empty<BulletListItem>();
                new BulletList(options).AddItems(items).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated bullet-list section for items selected from the current model.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into bullet-list items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source item models from the current model.</param>
        /// <param name="textSelector">Function that selects each item text.</param>
        /// <param name="textColorSelector">Optional function that selects each item text foreground color.</param>
        /// <param name="markerColorSelector">Optional function that selects each item marker foreground color.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> textSelector,
            Func<TItem, ConsoleColor?> textColorSelector = null,
            Func<TItem, ConsoleColor?> markerColorSelector = null,
            BulletListOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new BulletList(options)
                    .AddItems(items, textSelector, textColorSelector, markerColorSelector)
                    .Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated bullet-list section by recursively projecting a model hierarchy into nested bullet-list items.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into bullet-list items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source top-level item models from the current model.</param>
        /// <param name="textSelector">Function that selects each item text.</param>
        /// <param name="childrenSelector">Function that selects child models for each item.</param>
        /// <param name="textColorSelector">Optional function that selects each item text foreground color.</param>
        /// <param name="markerColorSelector">Optional function that selects each item marker foreground color.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> textSelector,
            Func<TItem, IEnumerable<TItem>> childrenSelector,
            Func<TItem, ConsoleColor?> textColorSelector = null,
            Func<TItem, ConsoleColor?> markerColorSelector = null,
            BulletListOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            if (childrenSelector == null)
            {
                throw new ArgumentNullException(nameof(childrenSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var list = new BulletList(options);
                foreach (TItem item in items)
                {
                    BulletListItem listItem = BuildBulletListItem(item, textSelector, childrenSelector, textColorSelector, markerColorSelector);
                    if (listItem != null)
                    {
                        list.AddItem(listItem);
                    }
                }

                list.Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated bullet-list section using style text for list items and the section title.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into bullet-list items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source item models from the current model.</param>
        /// <param name="textSelector">Function that selects each item text.</param>
        /// <param name="textStyleSelector">Optional function that selects each item text style.</param>
        /// <param name="markerStyleSelector">Optional function that selects each item marker style.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSectionStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> textSelector,
            Func<TItem, string> textStyleSelector = null,
            Func<TItem, string> markerStyleSelector = null,
            BulletListOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var list = new BulletList(options);
                foreach (TItem item in items)
                {
                    list.AddItemStyled(
                        textSelector(item),
                        textStyleSelector != null ? textStyleSelector(item) : null,
                        markerStyleSelector != null ? markerStyleSelector(item) : null);
                }

                list.Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated nested bullet-list section using style text for list items and the section title.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into bullet-list items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source top-level item models from the current model.</param>
        /// <param name="textSelector">Function that selects each item text.</param>
        /// <param name="childrenSelector">Function that selects child models for each item.</param>
        /// <param name="textStyleSelector">Optional function that selects each item text style.</param>
        /// <param name="markerStyleSelector">Optional function that selects each item marker style.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSectionStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> textSelector,
            Func<TItem, IEnumerable<TItem>> childrenSelector,
            Func<TItem, string> textStyleSelector = null,
            Func<TItem, string> markerStyleSelector = null,
            BulletListOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            if (childrenSelector == null)
            {
                throw new ArgumentNullException(nameof(childrenSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var list = new BulletList(options);
                foreach (TItem item in items)
                {
                    BulletListItem listItem = BuildBulletListItemStyled(item, textSelector, childrenSelector, textStyleSelector, markerStyleSelector);
                    if (listItem != null)
                    {
                        list.AddItem(listItem);
                    }
                }

                list.Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated checklist section for checklist items selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the checklist items from the current model.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ChecklistSection(
            string title,
            Func<T, IEnumerable<ChecklistItem>> itemsSelector,
            ChecklistOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<ChecklistItem> items = itemsSelector(model) ?? Enumerable.Empty<ChecklistItem>();
                new Checklist(options).AddItems(items).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated checklist section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the checklist items from the current model.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ChecklistSectionStyled(
            string title,
            Func<T, IEnumerable<ChecklistItem>> itemsSelector,
            ChecklistOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<ChecklistItem> items = itemsSelector(model) ?? Enumerable.Empty<ChecklistItem>();
                new Checklist(options).AddItems(items).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated checklist section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the checklist items from the current model.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ChecklistSection(
            string title,
            Func<T, IEnumerable<ChecklistItem>> itemsSelector,
            ChecklistOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<ChecklistItem> items = itemsSelector(model) ?? Enumerable.Empty<ChecklistItem>();
                new Checklist(options).AddItems(items).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated checklist section for readiness or workflow items selected from the current model.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into checklist items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source item models from the current model.</param>
        /// <param name="labelSelector">Function that selects each checklist item label.</param>
        /// <param name="stateSelector">Optional function that selects each checklist item state.</param>
        /// <param name="descriptionSelector">Optional function that selects each checklist item description.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ChecklistSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> labelSelector,
            Func<TItem, ChecklistItemState> stateSelector = null,
            Func<TItem, string> descriptionSelector = null,
            ChecklistOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new Checklist(options)
                    .AddItems(items, labelSelector, stateSelector, descriptionSelector)
                    .Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated checklist section using style text for checklist items and the section title.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into checklist items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source item models from the current model.</param>
        /// <param name="labelSelector">Function that selects each checklist item label.</param>
        /// <param name="stateSelector">Optional function that selects each checklist item state.</param>
        /// <param name="descriptionSelector">Optional function that selects each checklist item description.</param>
        /// <param name="labelStyleSelector">Optional function that selects each checklist item label style.</param>
        /// <param name="markerStyleSelector">Optional function that selects each checklist item marker style.</param>
        /// <param name="descriptionStyleSelector">Optional function that selects each checklist item description style.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ChecklistSectionStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, string> labelSelector,
            Func<TItem, ChecklistItemState> stateSelector = null,
            Func<TItem, string> descriptionSelector = null,
            Func<TItem, string> labelStyleSelector = null,
            Func<TItem, string> markerStyleSelector = null,
            Func<TItem, string> descriptionStyleSelector = null,
            ChecklistOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var checklist = new Checklist(options);
                foreach (TItem item in items)
                {
                    checklist.AddItemStyled(
                        labelSelector(item),
                        stateSelector != null ? stateSelector(item) : ChecklistItemState.Pending,
                        descriptionSelector != null ? descriptionSelector(item) : null,
                        labelStyleSelector != null ? labelStyleSelector(item) : null,
                        markerStyleSelector != null ? markerStyleSelector(item) : null,
                        descriptionStyleSelector != null ? descriptionStyleSelector(item) : null);
                }

                checklist.Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated sparkline section for numeric trend values selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="valuesSelector">Function that selects the numeric trend values from the current model.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SparklineSection(
            string title,
            Func<T, IEnumerable<double>> valuesSelector,
            SparklineOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (valuesSelector == null)
            {
                throw new ArgumentNullException(nameof(valuesSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<double> values = valuesSelector(model) ?? Enumerable.Empty<double>();
                new Sparkline(values, options).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated sparkline section for numeric trend values using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="valuesSelector">Function that selects the numeric trend values from the current model.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SparklineSectionStyled(
            string title,
            Func<T, IEnumerable<double>> valuesSelector,
            SparklineOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (valuesSelector == null)
            {
                throw new ArgumentNullException(nameof(valuesSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<double> values = valuesSelector(model) ?? Enumerable.Empty<double>();
                new Sparkline(values, options).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated sparkline section for numeric trend values using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="valuesSelector">Function that selects the numeric trend values from the current model.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SparklineSection(
            string title,
            Func<T, IEnumerable<double>> valuesSelector,
            SparklineOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (valuesSelector == null)
            {
                throw new ArgumentNullException(nameof(valuesSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<double> values = valuesSelector(model) ?? Enumerable.Empty<double>();
                new Sparkline(values, options).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated sparkline section for a collection selected from the current model.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into sparkline values.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source trend models from the current model.</param>
        /// <param name="valueSelector">Function that selects each numeric sparkline value.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SparklineSection<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, double> valueSelector,
            SparklineOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new Sparkline(options).AddValues(items, valueSelector).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated sparkline section using style text for the section title.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into sparkline values.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source trend models from the current model.</param>
        /// <param name="valueSelector">Function that selects each numeric sparkline value.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SparklineSectionStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<TItem, double> valueSelector,
            SparklineOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new Sparkline(options).AddValues(items, valueSelector).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated tree section using an existing tree root selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rootSelector">Function that selects the tree root from the current model.</param>
        /// <param name="options">Optional tree rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TreeSection(
            string title,
            Func<T, TreeNode> rootSelector,
            TreeOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rootSelector == null)
            {
                throw new ArgumentNullException(nameof(rootSelector));
            }

            return Section(title, model =>
            {
                TreeNode root = rootSelector(model);
                if (root != null)
                {
                    new Tree(root, options).Render();
                }
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated tree section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rootSelector">Function that selects the tree root from the current model.</param>
        /// <param name="options">Optional tree rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TreeSectionStyled(
            string title,
            Func<T, TreeNode> rootSelector,
            TreeOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rootSelector == null)
            {
                throw new ArgumentNullException(nameof(rootSelector));
            }

            return Section(title, model =>
            {
                TreeNode root = rootSelector(model);
                if (root != null)
                {
                    new Tree(root, options).Render();
                }
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated tree section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rootSelector">Function that selects the tree root from the current model.</param>
        /// <param name="options">Optional tree rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TreeSection(
            string title,
            Func<T, TreeNode> rootSelector,
            TreeOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rootSelector == null)
            {
                throw new ArgumentNullException(nameof(rootSelector));
            }

            return Section(title, model =>
            {
                TreeNode root = rootSelector(model);
                if (root != null)
                {
                    new Tree(root, options).Render();
                }
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated tree section by recursively projecting a model hierarchy into tree nodes.
        /// </summary>
        /// <typeparam name="TNode">The model item type projected into tree nodes.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rootSelector">Function that selects the source root model from the current model.</param>
        /// <param name="contentSelector">Function that selects each tree node label. Values may be plain objects or reusable renderables.</param>
        /// <param name="childrenSelector">Function that selects child models for each tree node.</param>
        /// <param name="colorSelector">Optional function that selects each tree node foreground color.</param>
        /// <param name="options">Optional tree rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TreeSection<TNode>(
            string title,
            Func<T, TNode> rootSelector,
            Func<TNode, object> contentSelector,
            Func<TNode, IEnumerable<TNode>> childrenSelector,
            Func<TNode, ConsoleColor?> colorSelector = null,
            TreeOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rootSelector == null)
            {
                throw new ArgumentNullException(nameof(rootSelector));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (childrenSelector == null)
            {
                throw new ArgumentNullException(nameof(childrenSelector));
            }

            return Section(title, model =>
            {
                TreeNode root = BuildTreeNode(rootSelector(model), contentSelector, childrenSelector, colorSelector);
                if (root != null)
                {
                    new Tree(root, options).Render();
                }
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated tree section using style text for each tree node and the section title.
        /// </summary>
        /// <typeparam name="TNode">The model item type projected into tree nodes.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rootSelector">Function that selects the source root model from the current model.</param>
        /// <param name="contentSelector">Function that selects each tree node label. Values may be plain objects or reusable renderables.</param>
        /// <param name="childrenSelector">Function that selects child models for each tree node.</param>
        /// <param name="styleSelector">Optional function that selects style text for each tree node.</param>
        /// <param name="options">Optional tree rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TreeSectionStyled<TNode>(
            string title,
            Func<T, TNode> rootSelector,
            Func<TNode, object> contentSelector,
            Func<TNode, IEnumerable<TNode>> childrenSelector,
            Func<TNode, string> styleSelector = null,
            TreeOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rootSelector == null)
            {
                throw new ArgumentNullException(nameof(rootSelector));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (childrenSelector == null)
            {
                throw new ArgumentNullException(nameof(childrenSelector));
            }

            return SectionStyled(model => title, model =>
            {
                TreeNode root = BuildTreeNodeStyled(rootSelector(model), contentSelector, childrenSelector, styleSelector);
                if (root != null)
                {
                    new Tree(root, options).Render();
                }
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated timeline section for timeline items selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the timeline items from the current model.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TimelineSection(
            string title,
            Func<T, IEnumerable<TimelineItem>> itemsSelector,
            TimelineOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TimelineItem> items = itemsSelector(model) ?? Enumerable.Empty<TimelineItem>();
                new Timeline(options).AddSteps(items).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated timeline section for timeline items using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the timeline items from the current model.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TimelineSectionStyled(
            string title,
            Func<T, IEnumerable<TimelineItem>> itemsSelector,
            TimelineOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TimelineItem> items = itemsSelector(model) ?? Enumerable.Empty<TimelineItem>();
                new Timeline(options).AddSteps(items).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated timeline section for timeline items using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the timeline items from the current model.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TimelineSection(
            string title,
            Func<T, IEnumerable<TimelineItem>> itemsSelector,
            TimelineOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TimelineItem> items = itemsSelector(model) ?? Enumerable.Empty<TimelineItem>();
                new Timeline(options).AddSteps(items).Render();
            }, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated timeline section for a collection selected from the current model.
        /// </summary>
        /// <typeparam name="TStep">The model item type projected into timeline steps.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source step models from the current model.</param>
        /// <param name="titleSelector">Function that selects each step title.</param>
        /// <param name="stateSelector">Optional function that selects each step state.</param>
        /// <param name="descriptionSelector">Optional function that selects each step description.</param>
        /// <param name="timestampSelector">Optional function that selects each step timestamp.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TimelineSection<TStep>(
            string title,
            Func<T, IEnumerable<TStep>> itemsSelector,
            Func<TStep, string> titleSelector,
            Func<TStep, TimelineItemState> stateSelector = null,
            Func<TStep, string> descriptionSelector = null,
            Func<TStep, DateTimeOffset?> timestampSelector = null,
            TimelineOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (titleSelector == null)
            {
                throw new ArgumentNullException(nameof(titleSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TStep> items = itemsSelector(model) ?? Enumerable.Empty<TStep>();
                new Timeline(options)
                    .AddSteps(items, titleSelector, stateSelector, descriptionSelector, timestampSelector)
                    .Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated timeline section using style text for timeline steps and the section title.
        /// </summary>
        /// <typeparam name="TStep">The model item type projected into timeline steps.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source step models from the current model.</param>
        /// <param name="titleSelector">Function that selects each step title.</param>
        /// <param name="stateSelector">Optional function that selects each step state.</param>
        /// <param name="descriptionSelector">Optional function that selects each step description.</param>
        /// <param name="timestampSelector">Optional function that selects each step timestamp.</param>
        /// <param name="titleStyleSelector">Optional function that selects each step title style.</param>
        /// <param name="markerStyleSelector">Optional function that selects each step marker style.</param>
        /// <param name="descriptionStyleSelector">Optional function that selects each step description style.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TimelineSectionStyled<TStep>(
            string title,
            Func<T, IEnumerable<TStep>> itemsSelector,
            Func<TStep, string> titleSelector,
            Func<TStep, TimelineItemState> stateSelector = null,
            Func<TStep, string> descriptionSelector = null,
            Func<TStep, DateTimeOffset?> timestampSelector = null,
            Func<TStep, string> titleStyleSelector = null,
            Func<TStep, string> markerStyleSelector = null,
            Func<TStep, string> descriptionStyleSelector = null,
            TimelineOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (titleSelector == null)
            {
                throw new ArgumentNullException(nameof(titleSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TStep> items = itemsSelector(model) ?? Enumerable.Empty<TStep>();
                new Timeline(options)
                    .AddStepsStyled(items, titleSelector, stateSelector, descriptionSelector, timestampSelector, titleStyleSelector, markerStyleSelector, descriptionStyleSelector)
                    .Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated rows section for vertically stacked content selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rowsSelector">Function that selects row values from the current model.</param>
        /// <param name="options">Optional rows rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RowsSection(
            string title,
            Func<T, IEnumerable<object>> rowsSelector,
            RowsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rowsSelector == null)
            {
                throw new ArgumentNullException(nameof(rowsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<object> rows = rowsSelector(model) ?? Enumerable.Empty<object>();
                new Rows(options).AddRows(rows).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated rows section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rowsSelector">Function that selects row values from the current model.</param>
        /// <param name="options">Optional rows rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RowsSectionStyled(
            string title,
            Func<T, IEnumerable<object>> rowsSelector,
            RowsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return RowsSectionStyledCore(title, rowsSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated rows section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rowsSelector">Function that selects row values from the current model.</param>
        /// <param name="options">Optional rows rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RowsSection(
            string title,
            Func<T, IEnumerable<object>> rowsSelector,
            RowsOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return RowsSectionStyledCore(title, rowsSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated rows section for model-projected vertically stacked content.
        /// </summary>
        /// <typeparam name="TRow">The model item type projected into rows.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source row models from the current model.</param>
        /// <param name="rowSelector">Function that selects each rendered row value.</param>
        /// <param name="colorSelector">Optional function that selects each row foreground color.</param>
        /// <param name="options">Optional rows rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RowsSection<TRow>(
            string title,
            Func<T, IEnumerable<TRow>> itemsSelector,
            Func<TRow, object> rowSelector,
            Func<TRow, ConsoleColor?> colorSelector = null,
            RowsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (rowSelector == null)
            {
                throw new ArgumentNullException(nameof(rowSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TRow> items = itemsSelector(model) ?? Enumerable.Empty<TRow>();
                new Rows(options).AddRows(items, rowSelector, colorSelector).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated rows section for model-projected vertically stacked content using style text.
        /// </summary>
        /// <typeparam name="TRow">The model item type projected into rows.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source row models from the current model.</param>
        /// <param name="rowSelector">Function that selects each rendered row value.</param>
        /// <param name="styleSelector">Optional function that selects style text for each row.</param>
        /// <param name="options">Optional rows rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RowsSectionStyled<TRow>(
            string title,
            Func<T, IEnumerable<TRow>> itemsSelector,
            Func<TRow, object> rowSelector,
            Func<TRow, string> styleSelector = null,
            RowsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (rowSelector == null)
            {
                throw new ArgumentNullException(nameof(rowSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TRow> items = itemsSelector(model) ?? Enumerable.Empty<TRow>();
                var rows = new Rows(options);
                if (styleSelector == null)
                {
                    rows.AddRows(items, rowSelector);
                }
                else
                {
                    rows.AddRows(items, rowSelector, styleSelector);
                }

                rows.Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated columns section for side-by-side content selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="columnsSelector">Function that selects column values from the current model.</param>
        /// <param name="options">Optional columns rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ColumnsSection(
            string title,
            Func<T, IEnumerable<object>> columnsSelector,
            ColumnsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (columnsSelector == null)
            {
                throw new ArgumentNullException(nameof(columnsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<object> columns = columnsSelector(model) ?? Enumerable.Empty<object>();
                new Columns(options).AddColumns(columns).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated columns section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="columnsSelector">Function that selects column values from the current model.</param>
        /// <param name="options">Optional columns rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ColumnsSectionStyled(
            string title,
            Func<T, IEnumerable<object>> columnsSelector,
            ColumnsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ColumnsSectionStyledCore(title, columnsSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated columns section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="columnsSelector">Function that selects column values from the current model.</param>
        /// <param name="options">Optional columns rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ColumnsSection(
            string title,
            Func<T, IEnumerable<object>> columnsSelector,
            ColumnsOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ColumnsSectionStyledCore(title, columnsSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated columns section for model-projected side-by-side content.
        /// </summary>
        /// <typeparam name="TColumn">The model item type projected into columns.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source column models from the current model.</param>
        /// <param name="contentSelector">Function that selects each rendered column value.</param>
        /// <param name="headerSelector">Optional function that selects each column header.</param>
        /// <param name="colorSelector">Optional function that selects each column foreground color.</param>
        /// <param name="options">Optional columns rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ColumnsSection<TColumn>(
            string title,
            Func<T, IEnumerable<TColumn>> itemsSelector,
            Func<TColumn, object> contentSelector,
            Func<TColumn, string> headerSelector = null,
            Func<TColumn, ConsoleColor?> colorSelector = null,
            ColumnsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TColumn> items = itemsSelector(model) ?? Enumerable.Empty<TColumn>();
                new Columns(options).AddColumns(items, contentSelector, headerSelector, colorSelector).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated columns section for model-projected side-by-side content using style text.
        /// </summary>
        /// <typeparam name="TColumn">The model item type projected into columns.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source column models from the current model.</param>
        /// <param name="contentSelector">Function that selects each rendered column value.</param>
        /// <param name="headerSelector">Optional function that selects each column header.</param>
        /// <param name="styleSelector">Optional function that selects style text for each column.</param>
        /// <param name="options">Optional columns rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ColumnsSectionStyled<TColumn>(
            string title,
            Func<T, IEnumerable<TColumn>> itemsSelector,
            Func<TColumn, object> contentSelector,
            Func<TColumn, string> headerSelector = null,
            Func<TColumn, string> styleSelector = null,
            ColumnsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TColumn> items = itemsSelector(model) ?? Enumerable.Empty<TColumn>();
                var columns = new Columns(options);
                if (styleSelector == null)
                {
                    columns.AddColumns(items, contentSelector, headerSelector);
                }
                else
                {
                    columns.AddColumns(items, contentSelector, headerSelector, styleSelector);
                }

                columns.Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated grid section for responsive cells selected from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="cellsSelector">Function that selects cell values from the current model.</param>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> GridSection(
            string title,
            Func<T, IEnumerable<object>> cellsSelector,
            GridOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (cellsSelector == null)
            {
                throw new ArgumentNullException(nameof(cellsSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<object> cells = cellsSelector(model) ?? Enumerable.Empty<object>();
                var grid = new Grid(options);
                foreach (object cell in cells)
                {
                    grid.AddCell(cell);
                }

                grid.Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated grid section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="cellsSelector">Function that selects cell values from the current model.</param>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> GridSectionStyled(
            string title,
            Func<T, IEnumerable<object>> cellsSelector,
            GridOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return GridSectionStyledCore(title, cellsSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated grid section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="cellsSelector">Function that selects cell values from the current model.</param>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> GridSection(
            string title,
            Func<T, IEnumerable<object>> cellsSelector,
            GridOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return GridSectionStyledCore(title, cellsSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated grid section for model-projected responsive cells.
        /// </summary>
        /// <typeparam name="TCell">The model item type projected into grid cells.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source cell models from the current model.</param>
        /// <param name="contentSelector">Function that selects each rendered cell value.</param>
        /// <param name="headerSelector">Optional function that selects each cell header.</param>
        /// <param name="colorSelector">Optional function that selects each cell foreground color.</param>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> GridSection<TCell>(
            string title,
            Func<T, IEnumerable<TCell>> itemsSelector,
            Func<TCell, object> contentSelector,
            Func<TCell, string> headerSelector = null,
            Func<TCell, ConsoleColor?> colorSelector = null,
            GridOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TCell> items = itemsSelector(model) ?? Enumerable.Empty<TCell>();
                new Grid(options).AddCells(items, contentSelector, headerSelector, colorSelector).Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated grid section for model-projected responsive cells using style text.
        /// </summary>
        /// <typeparam name="TCell">The model item type projected into grid cells.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source cell models from the current model.</param>
        /// <param name="contentSelector">Function that selects each rendered cell value.</param>
        /// <param name="headerSelector">Optional function that selects each cell header.</param>
        /// <param name="styleSelector">Optional function that selects style text for each cell.</param>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> GridSectionStyled<TCell>(
            string title,
            Func<T, IEnumerable<TCell>> itemsSelector,
            Func<TCell, object> contentSelector,
            Func<TCell, string> headerSelector = null,
            Func<TCell, string> styleSelector = null,
            GridOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TCell> items = itemsSelector(model) ?? Enumerable.Empty<TCell>();
                var grid = new Grid(options);
                if (styleSelector == null)
                {
                    grid.AddCells(items, contentSelector, headerSelector);
                }
                else
                {
                    grid.AddCells(items, contentSelector, headerSelector, styleSelector);
                }

                grid.Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated layout section whose panes are built from the current model.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="layoutBuilder">Action that configures the layout from the current model.</param>
        /// <param name="options">Optional layout rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> LayoutSection(
            string title,
            Action<T, Layout> layoutBuilder,
            LayoutOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (layoutBuilder == null)
            {
                throw new ArgumentNullException(nameof(layoutBuilder));
            }

            return Section(title, model =>
            {
                var layout = new Layout(options)
                {
                    Name = title
                };

                layoutBuilder(model, layout);
                layout.Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated layout section using style text for the section title.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="layoutBuilder">Action that configures the layout from the current model.</param>
        /// <param name="options">Optional layout rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> LayoutSectionStyled(
            string title,
            Action<T, Layout> layoutBuilder,
            LayoutOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return LayoutSectionStyledCore(title, layoutBuilder, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated layout section using a reusable section title style.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="layoutBuilder">Action that configures the layout from the current model.</param>
        /// <param name="options">Optional layout rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> LayoutSection(
            string title,
            Action<T, Layout> layoutBuilder,
            LayoutOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return LayoutSectionStyledCore(title, layoutBuilder, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated layout section for model-projected dashboard panes.
        /// </summary>
        /// <typeparam name="TPane">The model item type projected into layout panes.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source pane models from the current model.</param>
        /// <param name="contentSelector">Function that selects each rendered pane content.</param>
        /// <param name="paneTitleSelector">Optional function that selects each pane title.</param>
        /// <param name="colorSelector">Optional function that selects each pane foreground color.</param>
        /// <param name="options">Optional layout rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> LayoutSection<TPane>(
            string title,
            Func<T, IEnumerable<TPane>> itemsSelector,
            Func<TPane, object> contentSelector,
            Func<TPane, string> paneTitleSelector = null,
            Func<TPane, ConsoleColor?> colorSelector = null,
            LayoutOptions options = null,
            int? captureWidth = null,
            int captureHeight = 200,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return Section(title, model =>
            {
                IEnumerable<TPane> items = itemsSelector(model) ?? Enumerable.Empty<TPane>();
                new Layout(options)
                    .AddPanes(items, contentSelector, paneTitleSelector, colorSelector, captureWidth, captureHeight)
                    .Render();
            }, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated layout section for model-projected dashboard panes using style text.
        /// </summary>
        /// <typeparam name="TPane">The model item type projected into layout panes.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source pane models from the current model.</param>
        /// <param name="contentSelector">Function that selects each rendered pane content.</param>
        /// <param name="paneTitleSelector">Optional function that selects each pane title.</param>
        /// <param name="styleSelector">Optional function that selects style text for each pane.</param>
        /// <param name="options">Optional layout rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> LayoutSectionStyled<TPane>(
            string title,
            Func<T, IEnumerable<TPane>> itemsSelector,
            Func<TPane, object> contentSelector,
            Func<TPane, string> paneTitleSelector = null,
            Func<TPane, string> styleSelector = null,
            LayoutOptions options = null,
            int? captureWidth = null,
            int captureHeight = 200,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TPane> items = itemsSelector(model) ?? Enumerable.Empty<TPane>();
                new Layout(options)
                    .AddPanes(items, contentSelector, paneTitleSelector, styleSelector, captureWidth, captureHeight)
                    .Render();
            }, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated property-list section for the current model only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional property-list rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="bindingAttr">Binding flags describing the model properties to render.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PropertyListSectionWhen(
            string title,
            Func<T, bool> isVisible,
            PropertyListOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            return PropertyListSectionWhen(title, model => model, isVisible, options, titleColor, alignment, bindingAttr);
        }

        /// <summary>
        /// Adds a generated property-list section for a selected model value only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="modelSelector">Function that selects the object whose public properties should be rendered.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional property-list rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="bindingAttr">Binding flags describing the selected model properties to render.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PropertyListSectionWhen(
            string title,
            Func<T, object> modelSelector,
            Func<T, bool> isVisible,
            PropertyListOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            if (modelSelector == null)
            {
                throw new ArgumentNullException(nameof(modelSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                object selectedModel = modelSelector(model);
                if (selectedModel == null)
                {
                    return;
                }

                new PropertyListEntry(options)
                    .AddModel(selectedModel, bindingAttr)
                    .Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated property-list section using style text only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional property-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="bindingAttr">Binding flags describing the model properties to render.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PropertyListSectionWhenStyled(
            string title,
            Func<T, bool> isVisible,
            PropertyListOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            return PropertyListSectionWhenStyledCore(title, model => model, isVisible, options, RuleTitleStyle(model => titleStyle), alignment, bindingAttr);
        }

        /// <summary>
        /// Adds a generated dynamic table section for a selected collection only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The row item type rendered by the table.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the table row items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="selectors">Optional property selectors rendered as columns. When omitted, public scalar properties are used.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TableSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            params Expression<Func<TItem, object>>[] selectors) where TItem : class
        {
            return TableSectionWhen(title, itemsSelector, isVisible, null, (ConsoleColor?)null, RuleTitleAlignment.Left, selectors);
        }

        /// <summary>
        /// Adds a generated dynamic table section for a selected collection only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The row item type rendered by the table.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the table row items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional table rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="selectors">Optional property selectors rendered as columns. When omitted, public scalar properties are used.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TableSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            TableRenderOptions options,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            params Expression<Func<TItem, object>>[] selectors) where TItem : class
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                DynamicTableView<TItem>.Create(items, options, selectors).RenderInline();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated dynamic table section using style text only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The row item type rendered by the table.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the table row items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional table rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <param name="selectors">Optional property selectors rendered as columns. When omitted, public scalar properties are used.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TableSectionWhenStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            TableRenderOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left,
            params Expression<Func<TItem, object>>[] selectors) where TItem : class
        {
            return TableSectionWhenStyledCore(title, itemsSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment, selectors);
        }

        /// <summary>
        /// Adds a generated metrics section for metric items only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the metric items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSectionWhen(
            string title,
            Func<T, IEnumerable<MetricItem>> metricsSelector,
            Func<T, bool> isVisible,
            MetricsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (metricsSelector == null)
            {
                throw new ArgumentNullException(nameof(metricsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<MetricItem> metrics = metricsSelector(model) ?? Enumerable.Empty<MetricItem>();
                new Metrics(metrics, options).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated metrics section for metric items using style text only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the metric items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSectionWhenStyled(
            string title,
            Func<T, IEnumerable<MetricItem>> metricsSelector,
            Func<T, bool> isVisible,
            MetricsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSectionWhenStyledCore(title, metricsSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated metrics section for metric items using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the metric items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSectionWhen(
            string title,
            Func<T, IEnumerable<MetricItem>> metricsSelector,
            Func<T, bool> isVisible,
            MetricsOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSectionWhenStyledCore(title, metricsSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated metrics section for a selected collection only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TMetric">The model item type projected into metric items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the source metric models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each metric label.</param>
        /// <param name="valueSelector">Function that selects each metric value.</param>
        /// <param name="unitSelector">Optional function that selects each metric unit.</param>
        /// <param name="noteSelector">Optional function that selects each metric note.</param>
        /// <param name="kindSelector">Optional function that selects each metric visual intent.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSectionWhen<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<T, bool> isVisible,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            Func<TMetric, string> unitSelector = null,
            Func<TMetric, string> noteSelector = null,
            Func<TMetric, MetricKind> kindSelector = null,
            MetricsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (metricsSelector == null)
            {
                throw new ArgumentNullException(nameof(metricsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TMetric> metrics = metricsSelector(model) ?? Enumerable.Empty<TMetric>();
                new Metrics(options)
                    .AddMetrics(metrics, labelSelector, valueSelector, unitSelector, noteSelector, kindSelector)
                    .Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated metrics section for a selected collection with explicit rendering options only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TMetric">The model item type projected into metric items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the source metric models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each metric label.</param>
        /// <param name="valueSelector">Function that selects each metric value.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSectionWhen<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<T, bool> isVisible,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            MetricsOptions options,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSectionWhen(title, metricsSelector, isVisible, labelSelector, valueSelector, null, null, null, options, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated metrics section using style text only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TMetric">The model item type projected into metric items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the source metric models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each metric label.</param>
        /// <param name="valueSelector">Function that selects each metric value.</param>
        /// <param name="unitSelector">Optional function that selects each metric unit.</param>
        /// <param name="noteSelector">Optional function that selects each metric note.</param>
        /// <param name="kindSelector">Optional function that selects each metric visual intent.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSectionWhenStyled<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<T, bool> isVisible,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            Func<TMetric, string> unitSelector = null,
            Func<TMetric, string> noteSelector = null,
            Func<TMetric, MetricKind> kindSelector = null,
            MetricsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSectionWhenStyledCore(title, metricsSelector, isVisible, labelSelector, valueSelector, unitSelector, noteSelector, kindSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated metrics section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TMetric">The model item type projected into metric items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the source metric models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each metric label.</param>
        /// <param name="valueSelector">Function that selects each metric value.</param>
        /// <param name="unitSelector">Optional function that selects each metric unit.</param>
        /// <param name="noteSelector">Optional function that selects each metric note.</param>
        /// <param name="kindSelector">Optional function that selects each metric visual intent.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSectionWhen<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<T, bool> isVisible,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            Func<TMetric, string> unitSelector,
            Func<TMetric, string> noteSelector,
            Func<TMetric, MetricKind> kindSelector,
            MetricsOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSectionWhenStyledCore(title, metricsSelector, isVisible, labelSelector, valueSelector, unitSelector, noteSelector, kindSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated metrics section with explicit rendering options using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TMetric">The model item type projected into metric items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="metricsSelector">Function that selects the source metric models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each metric label.</param>
        /// <param name="valueSelector">Function that selects each metric value.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> MetricsSectionWhen<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<T, bool> isVisible,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            MetricsOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return MetricsSectionWhen(title, metricsSelector, isVisible, labelSelector, valueSelector, null, null, null, options, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated bar-chart section for chart items only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the chart items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional bar-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BarChartSectionWhen(
            string title,
            Func<T, IEnumerable<BarChartItem>> itemsSelector,
            Func<T, bool> isVisible,
            BarChartOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<BarChartItem> items = itemsSelector(model) ?? Enumerable.Empty<BarChartItem>();
                new BarChart(options).AddItems(items).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated bar-chart section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the chart items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional bar-chart rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BarChartSectionWhenStyled(
            string title,
            Func<T, IEnumerable<BarChartItem>> itemsSelector,
            Func<T, bool> isVisible,
            BarChartOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<BarChartItem> items = itemsSelector(model) ?? Enumerable.Empty<BarChartItem>();
                new BarChart(options).AddItems(items).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated bar-chart section for a selected collection only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="colorSelector">Optional function that selects each chart item bar color.</param>
        /// <param name="options">Optional bar-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BarChartSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            Func<TItem, ConsoleColor?> colorSelector = null,
            BarChartOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new BarChart(options)
                    .AddItems(items, labelSelector, valueSelector, colorSelector)
                    .Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated bar-chart section with explicit rendering options only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="options">Optional bar-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BarChartSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            BarChartOptions options,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return BarChartSectionWhen(title, itemsSelector, isVisible, labelSelector, valueSelector, null, options, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated bar-chart section using style text for each bar and the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="styleSelector">Optional function that selects each chart item bar style.</param>
        /// <param name="options">Optional bar-chart rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BarChartSectionWhenStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            Func<TItem, string> styleSelector = null,
            BarChartOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var chart = new BarChart(options);
                if (styleSelector == null)
                {
                    chart.AddItems(items, labelSelector, valueSelector);
                }
                else
                {
                    chart.AddItems(items, labelSelector, valueSelector, styleSelector);
                }

                chart.Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated breakdown-chart section for chart items only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the chart items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional breakdown-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreakdownChartSectionWhen(
            string title,
            Func<T, IEnumerable<BreakdownChartItem>> itemsSelector,
            Func<T, bool> isVisible,
            BreakdownChartOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<BreakdownChartItem> items = itemsSelector(model) ?? Enumerable.Empty<BreakdownChartItem>();
                new BreakdownChart(options).AddItems(items).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated breakdown-chart section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the chart items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional breakdown-chart rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreakdownChartSectionWhenStyled(
            string title,
            Func<T, IEnumerable<BreakdownChartItem>> itemsSelector,
            Func<T, bool> isVisible,
            BreakdownChartOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<BreakdownChartItem> items = itemsSelector(model) ?? Enumerable.Empty<BreakdownChartItem>();
                new BreakdownChart(options).AddItems(items).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated breakdown-chart section for a selected collection only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="colorSelector">Optional function that selects each chart item segment color.</param>
        /// <param name="options">Optional breakdown-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreakdownChartSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            Func<TItem, ConsoleColor?> colorSelector = null,
            BreakdownChartOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new BreakdownChart(options)
                    .AddItems(items, labelSelector, valueSelector, colorSelector)
                    .Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated breakdown-chart section with explicit rendering options only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="options">Optional breakdown-chart rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreakdownChartSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            BreakdownChartOptions options,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return BreakdownChartSectionWhen(title, itemsSelector, isVisible, labelSelector, valueSelector, null, options, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated breakdown-chart section using style text for each segment and the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into chart items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source chart models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each chart item label.</param>
        /// <param name="valueSelector">Function that selects each chart item value.</param>
        /// <param name="styleSelector">Optional function that selects each chart item segment style.</param>
        /// <param name="options">Optional breakdown-chart rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreakdownChartSectionWhenStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> labelSelector,
            Func<TItem, double> valueSelector,
            Func<TItem, string> styleSelector = null,
            BreakdownChartOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var chart = new BreakdownChart(options);
                if (styleSelector == null)
                {
                    chart.AddItems(items, labelSelector, valueSelector);
                }
                else
                {
                    chart.AddItems(items, labelSelector, valueSelector, styleSelector);
                }

                chart.Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated text-block section for model-projected explanatory text only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the text block content from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional text-block rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TextBlockSectionWhen(
            string title,
            Func<T, string> contentSelector,
            Func<T, bool> isVisible,
            TextBlockOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                new TextBlockEntry(contentSelector(model), options).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated text-block section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the text block content from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional text-block rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TextBlockSectionWhenStyled(
            string title,
            Func<T, string> contentSelector,
            Func<T, bool> isVisible,
            TextBlockOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return TextBlockSectionWhenStyledCore(title, contentSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated text-block section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the text block content from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional text-block rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TextBlockSectionWhen(
            string title,
            Func<T, string> contentSelector,
            Func<T, bool> isVisible,
            TextBlockOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return TextBlockSectionWhenStyledCore(title, contentSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated panel section for model-projected text content only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the panel content from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PanelSectionWhen(
            string title,
            Func<T, string> contentSelector,
            Func<T, bool> isVisible,
            PanelOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                new PanelEntry(contentSelector(model), options).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated panel section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the panel content from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PanelSectionWhenStyled(
            string title,
            Func<T, string> contentSelector,
            Func<T, bool> isVisible,
            PanelOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return PanelSectionWhenStyledCore(title, contentSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated panel section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="contentSelector">Function that selects the panel content from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> PanelSectionWhen(
            string title,
            Func<T, string> contentSelector,
            Func<T, bool> isVisible,
            PanelOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return PanelSectionWhenStyledCore(title, contentSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated callout section for a model-projected status message only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="messageSelector">Function that selects the callout message from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="kindSelector">Optional function that selects the callout visual intent from the current model.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalloutSectionWhen(
            string title,
            Func<T, string> messageSelector,
            Func<T, bool> isVisible,
            Func<T, CalloutKind> kindSelector = null,
            CalloutOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (messageSelector == null)
            {
                throw new ArgumentNullException(nameof(messageSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                string message = messageSelector(model);
                CalloutKind kind = kindSelector != null ? kindSelector(model) : CalloutKind.Info;
                new CalloutEntry(message, kind, options).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated callout section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="messageSelector">Function that selects the callout message from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="kindSelector">Optional function that selects the callout visual intent from the current model.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalloutSectionWhenStyled(
            string title,
            Func<T, string> messageSelector,
            Func<T, bool> isVisible,
            Func<T, CalloutKind> kindSelector = null,
            CalloutOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return CalloutSectionWhenStyledCore(title, messageSelector, isVisible, kindSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated callout section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="messageSelector">Function that selects the callout message from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="kindSelector">Optional function that selects the callout visual intent from the current model.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalloutSectionWhen(
            string title,
            Func<T, string> messageSelector,
            Func<T, bool> isVisible,
            Func<T, CalloutKind> kindSelector,
            CalloutOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return CalloutSectionWhenStyledCore(title, messageSelector, isVisible, kindSelector, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated exception section that renders only while the current model satisfies a predicate and exposes an exception.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSectionWhen(
            string title,
            Func<T, Exception> exceptionSelector,
            Func<T, bool> isVisible,
            ExceptionRenderOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ExceptionSectionWhen(title, exceptionSelector, isVisible, (Func<T, string>)null, options, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated exception section with model-projected context text only while the current model satisfies a predicate and exposes an exception.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="messageSelector">Optional function that selects context text from the current model.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSectionWhen(
            string title,
            Func<T, Exception> exceptionSelector,
            Func<T, bool> isVisible,
            Func<T, string> messageSelector,
            ExceptionRenderOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (exceptionSelector == null)
            {
                throw new ArgumentNullException(nameof(exceptionSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                Exception exception = exceptionSelector(model);
                if (exception != null)
                {
                    string message = messageSelector != null ? messageSelector(model) : null;
                    new ExceptionEntry(exception, message, options).Render();
                }
            }, model => isVisible(model) && exceptionSelector(model) != null, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated exception section using style text for the section title only while the current model satisfies a predicate and exposes an exception.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSectionWhenStyled(
            string title,
            Func<T, Exception> exceptionSelector,
            Func<T, bool> isVisible,
            ExceptionRenderOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ExceptionSectionWhenStyled(title, exceptionSelector, isVisible, null, options, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated exception section with model-projected context text using style text for the section title only while the current model satisfies a predicate and exposes an exception.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="messageSelector">Optional function that selects context text from the current model.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSectionWhenStyled(
            string title,
            Func<T, Exception> exceptionSelector,
            Func<T, bool> isVisible,
            Func<T, string> messageSelector,
            ExceptionRenderOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (exceptionSelector == null)
            {
                throw new ArgumentNullException(nameof(exceptionSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                Exception exception = exceptionSelector(model);
                if (exception != null)
                {
                    string message = messageSelector != null ? messageSelector(model) : null;
                    new ExceptionEntry(exception, message, options).Render();
                }
            }, model => isVisible(model) && exceptionSelector(model) != null, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated exception section using a reusable section title style only while the current model satisfies a predicate and exposes an exception.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSectionWhen(
            string title,
            Func<T, Exception> exceptionSelector,
            Func<T, bool> isVisible,
            ExceptionRenderOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ExceptionSectionWhen(title, exceptionSelector, isVisible, null, options, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated exception section with model-projected context text using a reusable section title style only while the current model satisfies a predicate and exposes an exception.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="exceptionSelector">Function that selects the exception from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="messageSelector">Optional function that selects context text from the current model.</param>
        /// <param name="options">Optional exception rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ExceptionSectionWhen(
            string title,
            Func<T, Exception> exceptionSelector,
            Func<T, bool> isVisible,
            Func<T, string> messageSelector,
            ExceptionRenderOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (exceptionSelector == null)
            {
                throw new ArgumentNullException(nameof(exceptionSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                Exception exception = exceptionSelector(model);
                if (exception != null)
                {
                    string message = messageSelector != null ? messageSelector(model) : null;
                    new ExceptionEntry(exception, message, options).Render();
                }
            }, model => isVisible(model) && exceptionSelector(model) != null, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated breadcrumb section only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects breadcrumb item labels from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreadcrumbSectionWhen(
            string title,
            Func<T, IEnumerable<string>> itemsSelector,
            Func<T, bool> isVisible,
            BreadcrumbOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<string> items = itemsSelector(model) ?? Enumerable.Empty<string>();
                new BreadcrumbEntry(items, options).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated breadcrumb section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects breadcrumb item labels from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreadcrumbSectionWhenStyled(
            string title,
            Func<T, IEnumerable<string>> itemsSelector,
            Func<T, bool> isVisible,
            BreadcrumbOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<string> items = itemsSelector(model) ?? Enumerable.Empty<string>();
                new BreadcrumbEntry(items, options).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated breadcrumb section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects breadcrumb item labels from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreadcrumbSectionWhen(
            string title,
            Func<T, IEnumerable<string>> itemsSelector,
            Func<T, bool> isVisible,
            BreadcrumbOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<string> items = itemsSelector(model) ?? Enumerable.Empty<string>();
                new BreadcrumbEntry(items, options).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated breadcrumb section for model-projected navigation segments only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into breadcrumb segments.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source breadcrumb segment models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="itemSelector">Function that selects each breadcrumb segment label.</param>
        /// <param name="colorSelector">Optional function that selects each breadcrumb segment foreground color.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreadcrumbSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> itemSelector,
            Func<TItem, ConsoleColor?> colorSelector = null,
            BreadcrumbOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (itemSelector == null)
            {
                throw new ArgumentNullException(nameof(itemSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var breadcrumb = new BreadcrumbEntry(options);
                foreach (TItem item in items)
                {
                    ConsoleColor? color = colorSelector != null ? colorSelector(item) : null;
                    if (color.HasValue)
                    {
                        breadcrumb.Add(itemSelector(item), color.Value);
                    }
                    else
                    {
                        breadcrumb.Add(itemSelector(item));
                    }
                }

                breadcrumb.Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated breadcrumb section for model-projected navigation segments using style text only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into breadcrumb segments.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source breadcrumb segment models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="itemSelector">Function that selects each breadcrumb segment label.</param>
        /// <param name="styleSelector">Optional function that selects style text for each breadcrumb segment.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BreadcrumbSectionWhenStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> itemSelector,
            Func<TItem, string> styleSelector = null,
            BreadcrumbOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (itemSelector == null)
            {
                throw new ArgumentNullException(nameof(itemSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var breadcrumb = new BreadcrumbEntry(options);
                if (styleSelector == null)
                {
                    breadcrumb.AddItems(items, itemSelector);
                }
                else
                {
                    breadcrumb.AddItems(items, itemSelector, styleSelector);
                }

                breadcrumb.Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects shortcut items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSectionWhen(
            string title,
            Func<T, IEnumerable<ShortcutItem>> itemsSelector,
            Func<T, bool> isVisible,
            ShortcutBarOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<ShortcutItem> items = itemsSelector(model) ?? Enumerable.Empty<ShortcutItem>();
                new ShortcutBar(items, options).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects shortcut items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSectionWhenStyled(
            string title,
            Func<T, IEnumerable<ShortcutItem>> itemsSelector,
            Func<T, bool> isVisible,
            ShortcutBarOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<ShortcutItem> items = itemsSelector(model) ?? Enumerable.Empty<ShortcutItem>();
                new ShortcutBar(items, options).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects shortcut items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSectionWhen(
            string title,
            Func<T, IEnumerable<ShortcutItem>> itemsSelector,
            Func<T, bool> isVisible,
            ShortcutBarOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<ShortcutItem> items = itemsSelector(model) ?? Enumerable.Empty<ShortcutItem>();
                new ShortcutBar(items, options).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section for model-projected action hints only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into shortcut hints.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source shortcut models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="keySelector">Function that selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Function that selects the shortcut action description.</param>
        /// <param name="disabledSelector">Optional function that selects whether each shortcut should render as disabled.</param>
        /// <param name="keyColorSelector">Optional function that selects each shortcut key foreground color.</param>
        /// <param name="descriptionColorSelector">Optional function that selects each shortcut description foreground color.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> keySelector,
            Func<TItem, string> descriptionSelector,
            Func<TItem, bool> disabledSelector = null,
            Func<TItem, ConsoleColor?> keyColorSelector = null,
            Func<TItem, ConsoleColor?> descriptionColorSelector = null,
            ShortcutBarOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (descriptionSelector == null)
            {
                throw new ArgumentNullException(nameof(descriptionSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new ShortcutBar(options)
                    .AddShortcuts(items, keySelector, descriptionSelector, disabledSelector, keyColorSelector, descriptionColorSelector)
                    .Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section for model-projected action hints using style text only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into shortcut hints.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source shortcut models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="keySelector">Function that selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Function that selects the shortcut action description.</param>
        /// <param name="keyStyleSelector">Optional function that selects style text for each shortcut key token.</param>
        /// <param name="descriptionStyleSelector">Optional function that selects style text for each shortcut description.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSectionWhenStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> keySelector,
            Func<TItem, string> descriptionSelector,
            Func<TItem, string> keyStyleSelector = null,
            Func<TItem, string> descriptionStyleSelector = null,
            ShortcutBarOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ShortcutBarSectionWhenStyled(
                title,
                itemsSelector,
                isVisible,
                keySelector,
                descriptionSelector,
                null,
                keyStyleSelector,
                descriptionStyleSelector,
                options,
                titleStyle,
                alignment);
        }

        /// <summary>
        /// Adds a generated shortcut bar section for model-projected action hints using style text and disabled state only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into shortcut hints.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source shortcut models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="keySelector">Function that selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Function that selects the shortcut action description.</param>
        /// <param name="disabledSelector">Optional function that selects whether each shortcut should render as disabled.</param>
        /// <param name="keyStyleSelector">Optional function that selects style text for each shortcut key token.</param>
        /// <param name="descriptionStyleSelector">Optional function that selects style text for each shortcut description.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutBarSectionWhenStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> keySelector,
            Func<TItem, string> descriptionSelector,
            Func<TItem, bool> disabledSelector,
            Func<TItem, string> keyStyleSelector,
            Func<TItem, string> descriptionStyleSelector = null,
            ShortcutBarOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (descriptionSelector == null)
            {
                throw new ArgumentNullException(nameof(descriptionSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new ShortcutBar(options)
                    .AddShortcuts(items, keySelector, descriptionSelector, disabledSelector, keyStyleSelector, descriptionStyleSelector)
                    .Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated calendar section for a month only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="monthSelector">Function that selects any date within the month to render.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional calendar rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalendarSectionWhen(
            string title,
            Func<T, DateTime> monthSelector,
            Func<T, bool> isVisible,
            CalendarOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (monthSelector == null)
            {
                throw new ArgumentNullException(nameof(monthSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                new Calendar(monthSelector(model), options).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated calendar section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="monthSelector">Function that selects any date within the month to render.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional calendar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalendarSectionWhenStyled(
            string title,
            Func<T, DateTime> monthSelector,
            Func<T, bool> isVisible,
            CalendarOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (monthSelector == null)
            {
                throw new ArgumentNullException(nameof(monthSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                new Calendar(monthSelector(model), options).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated calendar section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="monthSelector">Function that selects any date within the month to render.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional calendar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalendarSectionWhen(
            string title,
            Func<T, DateTime> monthSelector,
            Func<T, bool> isVisible,
            CalendarOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (monthSelector == null)
            {
                throw new ArgumentNullException(nameof(monthSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                new Calendar(monthSelector(model), options).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated calendar section with highlighted dates only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TEvent">The model item type projected into highlighted calendar dates.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="monthSelector">Function that selects any date within the month to render.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="eventsSelector">Function that selects the source event models from the current model.</param>
        /// <param name="dateSelector">Function that selects the date to highlight for each event.</param>
        /// <param name="labelSelector">Optional function that selects the label describing each event.</param>
        /// <param name="colorSelector">Optional function that selects each highlighted date foreground color.</param>
        /// <param name="options">Optional calendar rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalendarSectionWhen<TEvent>(
            string title,
            Func<T, DateTime> monthSelector,
            Func<T, bool> isVisible,
            Func<T, IEnumerable<TEvent>> eventsSelector,
            Func<TEvent, DateTime> dateSelector,
            Func<TEvent, string> labelSelector = null,
            Func<TEvent, ConsoleColor?> colorSelector = null,
            CalendarOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (monthSelector == null)
            {
                throw new ArgumentNullException(nameof(monthSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (eventsSelector == null)
            {
                throw new ArgumentNullException(nameof(eventsSelector));
            }

            if (dateSelector == null)
            {
                throw new ArgumentNullException(nameof(dateSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TEvent> events = eventsSelector(model) ?? Enumerable.Empty<TEvent>();
                new Calendar(monthSelector(model), options)
                    .AddCalendarEvents(events, dateSelector, labelSelector, colorSelector)
                    .Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated calendar section using style text for highlighted dates and the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TEvent">The model item type projected into highlighted calendar dates.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="monthSelector">Function that selects any date within the month to render.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="eventsSelector">Function that selects the source event models from the current model.</param>
        /// <param name="dateSelector">Function that selects the date to highlight for each event.</param>
        /// <param name="labelSelector">Optional function that selects the label describing each event.</param>
        /// <param name="styleSelector">Optional function that selects style text for each highlighted date.</param>
        /// <param name="options">Optional calendar rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> CalendarSectionWhenStyled<TEvent>(
            string title,
            Func<T, DateTime> monthSelector,
            Func<T, bool> isVisible,
            Func<T, IEnumerable<TEvent>> eventsSelector,
            Func<TEvent, DateTime> dateSelector,
            Func<TEvent, string> labelSelector = null,
            Func<TEvent, string> styleSelector = null,
            CalendarOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (monthSelector == null)
            {
                throw new ArgumentNullException(nameof(monthSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (eventsSelector == null)
            {
                throw new ArgumentNullException(nameof(eventsSelector));
            }

            if (dateSelector == null)
            {
                throw new ArgumentNullException(nameof(dateSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TEvent> events = eventsSelector(model) ?? Enumerable.Empty<TEvent>();
                new Calendar(monthSelector(model), options)
                    .AddCalendarEventsStyled(events, dateSelector, labelSelector, styleSelector)
                    .Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated bullet-list section only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the bullet-list items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSectionWhen(
            string title,
            Func<T, IEnumerable<BulletListItem>> itemsSelector,
            Func<T, bool> isVisible,
            BulletListOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<BulletListItem> items = itemsSelector(model) ?? Enumerable.Empty<BulletListItem>();
                new BulletList(options).AddItems(items).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated bullet-list section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the bullet-list items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSectionWhenStyled(
            string title,
            Func<T, IEnumerable<BulletListItem>> itemsSelector,
            Func<T, bool> isVisible,
            BulletListOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<BulletListItem> items = itemsSelector(model) ?? Enumerable.Empty<BulletListItem>();
                new BulletList(options).AddItems(items).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated bullet-list section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the bullet-list items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSectionWhen(
            string title,
            Func<T, IEnumerable<BulletListItem>> itemsSelector,
            Func<T, bool> isVisible,
            BulletListOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<BulletListItem> items = itemsSelector(model) ?? Enumerable.Empty<BulletListItem>();
                new BulletList(options).AddItems(items).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated bullet-list section for model-projected items only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into bullet-list items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source item models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="textSelector">Function that selects each item text.</param>
        /// <param name="textColorSelector">Optional function that selects each item text foreground color.</param>
        /// <param name="markerColorSelector">Optional function that selects each item marker foreground color.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> textSelector,
            Func<TItem, ConsoleColor?> textColorSelector = null,
            Func<TItem, ConsoleColor?> markerColorSelector = null,
            BulletListOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new BulletList(options)
                    .AddItems(items, textSelector, textColorSelector, markerColorSelector)
                    .Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated nested bullet-list section only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into bullet-list items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source top-level item models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="textSelector">Function that selects each item text.</param>
        /// <param name="childrenSelector">Function that selects child models for each item.</param>
        /// <param name="textColorSelector">Optional function that selects each item text foreground color.</param>
        /// <param name="markerColorSelector">Optional function that selects each item marker foreground color.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> textSelector,
            Func<TItem, IEnumerable<TItem>> childrenSelector,
            Func<TItem, ConsoleColor?> textColorSelector = null,
            Func<TItem, ConsoleColor?> markerColorSelector = null,
            BulletListOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            if (childrenSelector == null)
            {
                throw new ArgumentNullException(nameof(childrenSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var list = new BulletList(options);
                foreach (TItem item in items)
                {
                    BulletListItem listItem = BuildBulletListItem(item, textSelector, childrenSelector, textColorSelector, markerColorSelector);
                    if (listItem != null)
                    {
                        list.AddItem(listItem);
                    }
                }

                list.Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated bullet-list section using style text for list items and the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into bullet-list items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source item models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="textSelector">Function that selects each item text.</param>
        /// <param name="textStyleSelector">Optional function that selects each item text style.</param>
        /// <param name="markerStyleSelector">Optional function that selects each item marker style.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSectionWhenStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> textSelector,
            Func<TItem, string> textStyleSelector = null,
            Func<TItem, string> markerStyleSelector = null,
            BulletListOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var list = new BulletList(options);
                foreach (TItem item in items)
                {
                    list.AddItemStyled(
                        textSelector(item),
                        textStyleSelector != null ? textStyleSelector(item) : null,
                        markerStyleSelector != null ? markerStyleSelector(item) : null);
                }

                list.Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated nested bullet-list section using style text for list items and the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into bullet-list items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source top-level item models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="textSelector">Function that selects each item text.</param>
        /// <param name="childrenSelector">Function that selects child models for each item.</param>
        /// <param name="textStyleSelector">Optional function that selects each item text style.</param>
        /// <param name="markerStyleSelector">Optional function that selects each item marker style.</param>
        /// <param name="options">Optional bullet-list rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> BulletListSectionWhenStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> textSelector,
            Func<TItem, IEnumerable<TItem>> childrenSelector,
            Func<TItem, string> textStyleSelector = null,
            Func<TItem, string> markerStyleSelector = null,
            BulletListOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            if (childrenSelector == null)
            {
                throw new ArgumentNullException(nameof(childrenSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var list = new BulletList(options);
                foreach (TItem item in items)
                {
                    BulletListItem listItem = BuildBulletListItemStyled(item, textSelector, childrenSelector, textStyleSelector, markerStyleSelector);
                    if (listItem != null)
                    {
                        list.AddItem(listItem);
                    }
                }

                list.Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated checklist section only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the checklist items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ChecklistSectionWhen(
            string title,
            Func<T, IEnumerable<ChecklistItem>> itemsSelector,
            Func<T, bool> isVisible,
            ChecklistOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<ChecklistItem> items = itemsSelector(model) ?? Enumerable.Empty<ChecklistItem>();
                new Checklist(options).AddItems(items).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated checklist section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the checklist items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ChecklistSectionWhenStyled(
            string title,
            Func<T, IEnumerable<ChecklistItem>> itemsSelector,
            Func<T, bool> isVisible,
            ChecklistOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<ChecklistItem> items = itemsSelector(model) ?? Enumerable.Empty<ChecklistItem>();
                new Checklist(options).AddItems(items).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated checklist section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the checklist items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ChecklistSectionWhen(
            string title,
            Func<T, IEnumerable<ChecklistItem>> itemsSelector,
            Func<T, bool> isVisible,
            ChecklistOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<ChecklistItem> items = itemsSelector(model) ?? Enumerable.Empty<ChecklistItem>();
                new Checklist(options).AddItems(items).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated checklist section for model-projected readiness or workflow items only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into checklist items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source item models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each checklist item label.</param>
        /// <param name="stateSelector">Optional function that selects each checklist item state.</param>
        /// <param name="descriptionSelector">Optional function that selects each checklist item description.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ChecklistSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> labelSelector,
            Func<TItem, ChecklistItemState> stateSelector = null,
            Func<TItem, string> descriptionSelector = null,
            ChecklistOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new Checklist(options)
                    .AddItems(items, labelSelector, stateSelector, descriptionSelector)
                    .Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated checklist section using style text for checklist items and the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into checklist items.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source item models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="labelSelector">Function that selects each checklist item label.</param>
        /// <param name="stateSelector">Optional function that selects each checklist item state.</param>
        /// <param name="descriptionSelector">Optional function that selects each checklist item description.</param>
        /// <param name="labelStyleSelector">Optional function that selects each checklist item label style.</param>
        /// <param name="markerStyleSelector">Optional function that selects each checklist item marker style.</param>
        /// <param name="descriptionStyleSelector">Optional function that selects each checklist item description style.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ChecklistSectionWhenStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, string> labelSelector,
            Func<TItem, ChecklistItemState> stateSelector = null,
            Func<TItem, string> descriptionSelector = null,
            Func<TItem, string> labelStyleSelector = null,
            Func<TItem, string> markerStyleSelector = null,
            Func<TItem, string> descriptionStyleSelector = null,
            ChecklistOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                var checklist = new Checklist(options);
                foreach (TItem item in items)
                {
                    checklist.AddItemStyled(
                        labelSelector(item),
                        stateSelector != null ? stateSelector(item) : ChecklistItemState.Pending,
                        descriptionSelector != null ? descriptionSelector(item) : null,
                        labelStyleSelector != null ? labelStyleSelector(item) : null,
                        markerStyleSelector != null ? markerStyleSelector(item) : null,
                        descriptionStyleSelector != null ? descriptionStyleSelector(item) : null);
                }

                checklist.Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated sparkline section for numeric trend values only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="valuesSelector">Function that selects the numeric trend values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SparklineSectionWhen(
            string title,
            Func<T, IEnumerable<double>> valuesSelector,
            Func<T, bool> isVisible,
            SparklineOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (valuesSelector == null)
            {
                throw new ArgumentNullException(nameof(valuesSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<double> values = valuesSelector(model) ?? Enumerable.Empty<double>();
                new Sparkline(values, options).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated sparkline section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="valuesSelector">Function that selects the numeric trend values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SparklineSectionWhenStyled(
            string title,
            Func<T, IEnumerable<double>> valuesSelector,
            Func<T, bool> isVisible,
            SparklineOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (valuesSelector == null)
            {
                throw new ArgumentNullException(nameof(valuesSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<double> values = valuesSelector(model) ?? Enumerable.Empty<double>();
                new Sparkline(values, options).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated sparkline section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="valuesSelector">Function that selects the numeric trend values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SparklineSectionWhen(
            string title,
            Func<T, IEnumerable<double>> valuesSelector,
            Func<T, bool> isVisible,
            SparklineOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (valuesSelector == null)
            {
                throw new ArgumentNullException(nameof(valuesSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<double> values = valuesSelector(model) ?? Enumerable.Empty<double>();
                new Sparkline(values, options).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated sparkline section for a selected collection only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into sparkline values.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source trend models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="valueSelector">Function that selects each numeric sparkline value.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SparklineSectionWhen<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, double> valueSelector,
            SparklineOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new Sparkline(options).AddValues(items, valueSelector).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated sparkline section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TItem">The model item type projected into sparkline values.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source trend models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="valueSelector">Function that selects each numeric sparkline value.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SparklineSectionWhenStyled<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TItem, double> valueSelector,
            SparklineOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                new Sparkline(options).AddValues(items, valueSelector).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated tree section from an existing tree root only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rootSelector">Function that selects the tree root from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional tree rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TreeSectionWhen(
            string title,
            Func<T, TreeNode> rootSelector,
            Func<T, bool> isVisible,
            TreeOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rootSelector == null)
            {
                throw new ArgumentNullException(nameof(rootSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                TreeNode root = rootSelector(model);
                if (root != null)
                {
                    new Tree(root, options).Render();
                }
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated tree section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rootSelector">Function that selects the tree root from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional tree rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TreeSectionWhenStyled(
            string title,
            Func<T, TreeNode> rootSelector,
            Func<T, bool> isVisible,
            TreeOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rootSelector == null)
            {
                throw new ArgumentNullException(nameof(rootSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                TreeNode root = rootSelector(model);
                if (root != null)
                {
                    new Tree(root, options).Render();
                }
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated tree section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rootSelector">Function that selects the tree root from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional tree rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TreeSectionWhen(
            string title,
            Func<T, TreeNode> rootSelector,
            Func<T, bool> isVisible,
            TreeOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rootSelector == null)
            {
                throw new ArgumentNullException(nameof(rootSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                TreeNode root = rootSelector(model);
                if (root != null)
                {
                    new Tree(root, options).Render();
                }
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated tree section by recursively projecting a model hierarchy only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TNode">The model item type projected into tree nodes.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rootSelector">Function that selects the source root model from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="contentSelector">Function that selects each tree node label. Values may be plain objects or reusable renderables.</param>
        /// <param name="childrenSelector">Function that selects child models for each tree node.</param>
        /// <param name="colorSelector">Optional function that selects each tree node foreground color.</param>
        /// <param name="options">Optional tree rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TreeSectionWhen<TNode>(
            string title,
            Func<T, TNode> rootSelector,
            Func<T, bool> isVisible,
            Func<TNode, object> contentSelector,
            Func<TNode, IEnumerable<TNode>> childrenSelector,
            Func<TNode, ConsoleColor?> colorSelector = null,
            TreeOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rootSelector == null)
            {
                throw new ArgumentNullException(nameof(rootSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (childrenSelector == null)
            {
                throw new ArgumentNullException(nameof(childrenSelector));
            }

            return SectionWhen(title, model =>
            {
                TreeNode root = BuildTreeNode(rootSelector(model), contentSelector, childrenSelector, colorSelector);
                if (root != null)
                {
                    new Tree(root, options).Render();
                }
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated tree section using style text for tree nodes and the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TNode">The model item type projected into tree nodes.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rootSelector">Function that selects the source root model from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="contentSelector">Function that selects each tree node label. Values may be plain objects or reusable renderables.</param>
        /// <param name="childrenSelector">Function that selects child models for each tree node.</param>
        /// <param name="styleSelector">Optional function that selects style text for each tree node.</param>
        /// <param name="options">Optional tree rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TreeSectionWhenStyled<TNode>(
            string title,
            Func<T, TNode> rootSelector,
            Func<T, bool> isVisible,
            Func<TNode, object> contentSelector,
            Func<TNode, IEnumerable<TNode>> childrenSelector,
            Func<TNode, string> styleSelector = null,
            TreeOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rootSelector == null)
            {
                throw new ArgumentNullException(nameof(rootSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (childrenSelector == null)
            {
                throw new ArgumentNullException(nameof(childrenSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                TreeNode root = BuildTreeNodeStyled(rootSelector(model), contentSelector, childrenSelector, styleSelector);
                if (root != null)
                {
                    new Tree(root, options).Render();
                }
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated timeline section for timeline items only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the timeline items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TimelineSectionWhen(
            string title,
            Func<T, IEnumerable<TimelineItem>> itemsSelector,
            Func<T, bool> isVisible,
            TimelineOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TimelineItem> items = itemsSelector(model) ?? Enumerable.Empty<TimelineItem>();
                new Timeline(options).AddSteps(items).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated timeline section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the timeline items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TimelineSectionWhenStyled(
            string title,
            Func<T, IEnumerable<TimelineItem>> itemsSelector,
            Func<T, bool> isVisible,
            TimelineOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TimelineItem> items = itemsSelector(model) ?? Enumerable.Empty<TimelineItem>();
                new Timeline(options).AddSteps(items).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated timeline section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the timeline items from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TimelineSectionWhen(
            string title,
            Func<T, IEnumerable<TimelineItem>> itemsSelector,
            Func<T, bool> isVisible,
            TimelineOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TimelineItem> items = itemsSelector(model) ?? Enumerable.Empty<TimelineItem>();
                new Timeline(options).AddSteps(items).Render();
            }, isVisible, titleStyle, alignment);
        }

        /// <summary>
        /// Adds a generated timeline section for a selected collection only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TStep">The model item type projected into timeline steps.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source step models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleSelector">Function that selects each step title.</param>
        /// <param name="stateSelector">Optional function that selects each step state.</param>
        /// <param name="descriptionSelector">Optional function that selects each step description.</param>
        /// <param name="timestampSelector">Optional function that selects each step timestamp.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TimelineSectionWhen<TStep>(
            string title,
            Func<T, IEnumerable<TStep>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TStep, string> titleSelector,
            Func<TStep, TimelineItemState> stateSelector = null,
            Func<TStep, string> descriptionSelector = null,
            Func<TStep, DateTimeOffset?> timestampSelector = null,
            TimelineOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (titleSelector == null)
            {
                throw new ArgumentNullException(nameof(titleSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TStep> items = itemsSelector(model) ?? Enumerable.Empty<TStep>();
                new Timeline(options)
                    .AddSteps(items, titleSelector, stateSelector, descriptionSelector, timestampSelector)
                    .Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated timeline section using style text for timeline steps and the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TStep">The model item type projected into timeline steps.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects the source step models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleSelector">Function that selects each step title.</param>
        /// <param name="stateSelector">Optional function that selects each step state.</param>
        /// <param name="descriptionSelector">Optional function that selects each step description.</param>
        /// <param name="timestampSelector">Optional function that selects each step timestamp.</param>
        /// <param name="titleStyleSelector">Optional function that selects each step title style.</param>
        /// <param name="markerStyleSelector">Optional function that selects each step marker style.</param>
        /// <param name="descriptionStyleSelector">Optional function that selects each step description style.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> TimelineSectionWhenStyled<TStep>(
            string title,
            Func<T, IEnumerable<TStep>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TStep, string> titleSelector,
            Func<TStep, TimelineItemState> stateSelector = null,
            Func<TStep, string> descriptionSelector = null,
            Func<TStep, DateTimeOffset?> timestampSelector = null,
            Func<TStep, string> titleStyleSelector = null,
            Func<TStep, string> markerStyleSelector = null,
            Func<TStep, string> descriptionStyleSelector = null,
            TimelineOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (titleSelector == null)
            {
                throw new ArgumentNullException(nameof(titleSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TStep> items = itemsSelector(model) ?? Enumerable.Empty<TStep>();
                new Timeline(options)
                    .AddStepsStyled(items, titleSelector, stateSelector, descriptionSelector, timestampSelector, titleStyleSelector, markerStyleSelector, descriptionStyleSelector)
                    .Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated rows section only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rowsSelector">Function that selects row values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional rows rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RowsSectionWhen(
            string title,
            Func<T, IEnumerable<object>> rowsSelector,
            Func<T, bool> isVisible,
            RowsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (rowsSelector == null)
            {
                throw new ArgumentNullException(nameof(rowsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<object> rows = rowsSelector(model) ?? Enumerable.Empty<object>();
                new Rows(options).AddRows(rows).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated rows section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rowsSelector">Function that selects row values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional rows rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RowsSectionWhenStyled(
            string title,
            Func<T, IEnumerable<object>> rowsSelector,
            Func<T, bool> isVisible,
            RowsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return RowsSectionWhenStyledCore(title, rowsSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated rows section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="rowsSelector">Function that selects row values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional rows rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RowsSectionWhen(
            string title,
            Func<T, IEnumerable<object>> rowsSelector,
            Func<T, bool> isVisible,
            RowsOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return RowsSectionWhenStyledCore(title, rowsSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated rows section for model-projected content only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TRow">The model item type projected into rows.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source row models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="rowSelector">Function that selects each rendered row value.</param>
        /// <param name="colorSelector">Optional function that selects each row foreground color.</param>
        /// <param name="options">Optional rows rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RowsSectionWhen<TRow>(
            string title,
            Func<T, IEnumerable<TRow>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TRow, object> rowSelector,
            Func<TRow, ConsoleColor?> colorSelector = null,
            RowsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (rowSelector == null)
            {
                throw new ArgumentNullException(nameof(rowSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TRow> items = itemsSelector(model) ?? Enumerable.Empty<TRow>();
                new Rows(options).AddRows(items, rowSelector, colorSelector).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated rows section for model-projected content using style text only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TRow">The model item type projected into rows.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source row models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="rowSelector">Function that selects each rendered row value.</param>
        /// <param name="styleSelector">Optional function that selects style text for each row.</param>
        /// <param name="options">Optional rows rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> RowsSectionWhenStyled<TRow>(
            string title,
            Func<T, IEnumerable<TRow>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TRow, object> rowSelector,
            Func<TRow, string> styleSelector = null,
            RowsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (rowSelector == null)
            {
                throw new ArgumentNullException(nameof(rowSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TRow> items = itemsSelector(model) ?? Enumerable.Empty<TRow>();
                var rows = new Rows(options);
                if (styleSelector == null)
                {
                    rows.AddRows(items, rowSelector);
                }
                else
                {
                    rows.AddRows(items, rowSelector, styleSelector);
                }

                rows.Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated columns section only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="columnsSelector">Function that selects column values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional columns rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ColumnsSectionWhen(
            string title,
            Func<T, IEnumerable<object>> columnsSelector,
            Func<T, bool> isVisible,
            ColumnsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (columnsSelector == null)
            {
                throw new ArgumentNullException(nameof(columnsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<object> columns = columnsSelector(model) ?? Enumerable.Empty<object>();
                new Columns(options).AddColumns(columns).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated columns section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="columnsSelector">Function that selects column values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional columns rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ColumnsSectionWhenStyled(
            string title,
            Func<T, IEnumerable<object>> columnsSelector,
            Func<T, bool> isVisible,
            ColumnsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ColumnsSectionWhenStyledCore(title, columnsSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated columns section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="columnsSelector">Function that selects column values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional columns rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ColumnsSectionWhen(
            string title,
            Func<T, IEnumerable<object>> columnsSelector,
            Func<T, bool> isVisible,
            ColumnsOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ColumnsSectionWhenStyledCore(title, columnsSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated columns section for model-projected content only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TColumn">The model item type projected into columns.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source column models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="contentSelector">Function that selects each rendered column value.</param>
        /// <param name="headerSelector">Optional function that selects each column header.</param>
        /// <param name="colorSelector">Optional function that selects each column foreground color.</param>
        /// <param name="options">Optional columns rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ColumnsSectionWhen<TColumn>(
            string title,
            Func<T, IEnumerable<TColumn>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TColumn, object> contentSelector,
            Func<TColumn, string> headerSelector = null,
            Func<TColumn, ConsoleColor?> colorSelector = null,
            ColumnsOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TColumn> items = itemsSelector(model) ?? Enumerable.Empty<TColumn>();
                new Columns(options).AddColumns(items, contentSelector, headerSelector, colorSelector).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated columns section for model-projected content using style text only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TColumn">The model item type projected into columns.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source column models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="contentSelector">Function that selects each rendered column value.</param>
        /// <param name="headerSelector">Optional function that selects each column header.</param>
        /// <param name="styleSelector">Optional function that selects style text for each column.</param>
        /// <param name="options">Optional columns rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ColumnsSectionWhenStyled<TColumn>(
            string title,
            Func<T, IEnumerable<TColumn>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TColumn, object> contentSelector,
            Func<TColumn, string> headerSelector = null,
            Func<TColumn, string> styleSelector = null,
            ColumnsOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TColumn> items = itemsSelector(model) ?? Enumerable.Empty<TColumn>();
                var columns = new Columns(options);
                if (styleSelector == null)
                {
                    columns.AddColumns(items, contentSelector, headerSelector);
                }
                else
                {
                    columns.AddColumns(items, contentSelector, headerSelector, styleSelector);
                }

                columns.Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated grid section only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="cellsSelector">Function that selects cell values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> GridSectionWhen(
            string title,
            Func<T, IEnumerable<object>> cellsSelector,
            Func<T, bool> isVisible,
            GridOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (cellsSelector == null)
            {
                throw new ArgumentNullException(nameof(cellsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<object> cells = cellsSelector(model) ?? Enumerable.Empty<object>();
                var grid = new Grid(options);
                foreach (object cell in cells)
                {
                    grid.AddCell(cell);
                }

                grid.Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated grid section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="cellsSelector">Function that selects cell values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> GridSectionWhenStyled(
            string title,
            Func<T, IEnumerable<object>> cellsSelector,
            Func<T, bool> isVisible,
            GridOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return GridSectionWhenStyledCore(title, cellsSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated grid section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="cellsSelector">Function that selects cell values from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> GridSectionWhen(
            string title,
            Func<T, IEnumerable<object>> cellsSelector,
            Func<T, bool> isVisible,
            GridOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return GridSectionWhenStyledCore(title, cellsSelector, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated grid section for model-projected cells only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TCell">The model item type projected into grid cells.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source cell models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="contentSelector">Function that selects each rendered cell value.</param>
        /// <param name="headerSelector">Optional function that selects each cell header.</param>
        /// <param name="colorSelector">Optional function that selects each cell foreground color.</param>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> GridSectionWhen<TCell>(
            string title,
            Func<T, IEnumerable<TCell>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TCell, object> contentSelector,
            Func<TCell, string> headerSelector = null,
            Func<TCell, ConsoleColor?> colorSelector = null,
            GridOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TCell> items = itemsSelector(model) ?? Enumerable.Empty<TCell>();
                new Grid(options).AddCells(items, contentSelector, headerSelector, colorSelector).Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated grid section for model-projected cells using style text only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TCell">The model item type projected into grid cells.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source cell models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="contentSelector">Function that selects each rendered cell value.</param>
        /// <param name="headerSelector">Optional function that selects each cell header.</param>
        /// <param name="styleSelector">Optional function that selects style text for each cell.</param>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> GridSectionWhenStyled<TCell>(
            string title,
            Func<T, IEnumerable<TCell>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TCell, object> contentSelector,
            Func<TCell, string> headerSelector = null,
            Func<TCell, string> styleSelector = null,
            GridOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TCell> items = itemsSelector(model) ?? Enumerable.Empty<TCell>();
                var grid = new Grid(options);
                if (styleSelector == null)
                {
                    grid.AddCells(items, contentSelector, headerSelector);
                }
                else
                {
                    grid.AddCells(items, contentSelector, headerSelector, styleSelector);
                }

                grid.Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated layout section only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="layoutBuilder">Action that configures the layout from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional layout rendering options.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> LayoutSectionWhen(
            string title,
            Action<T, Layout> layoutBuilder,
            Func<T, bool> isVisible,
            LayoutOptions options = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (layoutBuilder == null)
            {
                throw new ArgumentNullException(nameof(layoutBuilder));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhen(title, model =>
            {
                var layout = new Layout(options)
                {
                    Name = title
                };

                layoutBuilder(model, layout);
                layout.Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated layout section using style text for the section title only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="layoutBuilder">Action that configures the layout from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional layout rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> LayoutSectionWhenStyled(
            string title,
            Action<T, Layout> layoutBuilder,
            Func<T, bool> isVisible,
            LayoutOptions options = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return LayoutSectionWhenStyledCore(title, layoutBuilder, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated layout section using a reusable section title style only while the current model satisfies a predicate.
        /// </summary>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="layoutBuilder">Action that configures the layout from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="options">Optional layout rendering options.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> LayoutSectionWhen(
            string title,
            Action<T, Layout> layoutBuilder,
            Func<T, bool> isVisible,
            LayoutOptions options,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return LayoutSectionWhenStyledCore(title, layoutBuilder, isVisible, options, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a generated layout section for model-projected panes only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TPane">The model item type projected into layout panes.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source pane models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="contentSelector">Function that selects each rendered pane content.</param>
        /// <param name="paneTitleSelector">Optional function that selects each pane title.</param>
        /// <param name="colorSelector">Optional function that selects each pane foreground color.</param>
        /// <param name="options">Optional layout rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> LayoutSectionWhen<TPane>(
            string title,
            Func<T, IEnumerable<TPane>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TPane, object> contentSelector,
            Func<TPane, string> paneTitleSelector = null,
            Func<TPane, ConsoleColor?> colorSelector = null,
            LayoutOptions options = null,
            int? captureWidth = null,
            int captureHeight = 200,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionWhen(title, model =>
            {
                IEnumerable<TPane> items = itemsSelector(model) ?? Enumerable.Empty<TPane>();
                new Layout(options)
                    .AddPanes(items, contentSelector, paneTitleSelector, colorSelector, captureWidth, captureHeight)
                    .Render();
            }, isVisible, titleColor, alignment);
        }

        /// <summary>
        /// Adds a generated layout section for model-projected panes using style text only while the current model satisfies a predicate.
        /// </summary>
        /// <typeparam name="TPane">The model item type projected into layout panes.</typeparam>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="itemsSelector">Function that selects source pane models from the current model.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="contentSelector">Function that selects each rendered pane content.</param>
        /// <param name="paneTitleSelector">Optional function that selects each pane title.</param>
        /// <param name="styleSelector">Optional function that selects style text for each pane.</param>
        /// <param name="options">Optional layout rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> LayoutSectionWhenStyled<TPane>(
            string title,
            Func<T, IEnumerable<TPane>> itemsSelector,
            Func<T, bool> isVisible,
            Func<TPane, object> contentSelector,
            Func<TPane, string> paneTitleSelector = null,
            Func<TPane, string> styleSelector = null,
            LayoutOptions options = null,
            int? captureWidth = null,
            int captureHeight = 200,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TPane> items = itemsSelector(model) ?? Enumerable.Empty<TPane>();
                new Layout(options)
                    .AddPanes(items, contentSelector, paneTitleSelector, styleSelector, captureWidth, captureHeight)
                    .Render();
            }, isVisible, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a model validation summary section that re-validates on every render pass.
        /// </summary>
        /// <param name="options">Optional validation summary options.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySection(
            ValidationSummaryOptions options = null,
            string title = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySectionWhen(model => true, options, title, titleColor, alignment);
        }

        /// <summary>
        /// Adds a model validation summary section with a style-string title color.
        /// </summary>
        /// <param name="options">Optional validation summary options.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySectionStyled(
            ValidationSummaryOptions options = null,
            string title = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySectionWhenStyledCore(model => true, options, title, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a model validation summary section with a reusable title style.
        /// </summary>
        /// <param name="options">Optional validation summary options.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySection(
            ValidationSummaryOptions options,
            string title,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySectionWhenStyledCore(model => true, options, title, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a model validation summary section with a Spectre-style title color.
        /// </summary>
        /// <param name="options">Optional validation summary options.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleColor">Color facade used for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySection(
            ValidationSummaryOptions options,
            string title,
            SpectreColor titleColor,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySectionWhenStyledCore(model => true, options, title, RuleTitleStyle(model => titleColor), alignment);
        }

        /// <summary>
        /// Adds a model validation summary section with an RGB title color.
        /// </summary>
        /// <param name="options">Optional validation summary options.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleColor">RGB color used for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySection(
            ValidationSummaryOptions options,
            string title,
            RgbColor titleColor,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySectionWhenStyledCore(model => true, options, title, RuleTitleStyle(model => titleColor), alignment);
        }

        /// <summary>
        /// Adds a model validation summary section with a Spectre-style title color.
        /// </summary>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleColor">Color facade used for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySection(
            string title,
            SpectreColor titleColor,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySection(null, title, titleColor, alignment);
        }

        /// <summary>
        /// Adds a model validation summary section with an RGB title color.
        /// </summary>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleColor">RGB color used for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySection(
            string title,
            RgbColor titleColor,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySection(null, title, titleColor, alignment);
        }

        /// <summary>
        /// Adds a model validation summary section that renders only when the current model satisfies a predicate.
        /// </summary>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the validation summary should be rendered.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySectionWhen(
            Func<T, bool> isVisible,
            ValidationSummaryOptions options = null,
            string title = null,
            ConsoleColor? titleColor = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return SectionWhen(
                title,
                model => new ValidationSummaryEntry(model, options).Render(),
                isVisible ?? (model => true),
                titleColor,
                alignment);
        }

        /// <summary>
        /// Adds a conditional model validation summary section with a style-string title color.
        /// </summary>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the validation summary should be rendered.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySectionWhenStyled(
            Func<T, bool> isVisible,
            ValidationSummaryOptions options = null,
            string title = null,
            string titleStyle = null,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySectionWhenStyledCore(isVisible, options, title, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a conditional model validation summary section with a reusable title style.
        /// </summary>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the validation summary should be rendered.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySectionWhen(
            Func<T, bool> isVisible,
            ValidationSummaryOptions options,
            string title,
            Style titleStyle,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySectionWhenStyledCore(isVisible, options, title, RuleTitleStyle(model => titleStyle), alignment);
        }

        /// <summary>
        /// Adds a conditional model validation summary section with a Spectre-style title color.
        /// </summary>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the validation summary should be rendered.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleColor">Color facade used for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySectionWhen(
            Func<T, bool> isVisible,
            ValidationSummaryOptions options,
            string title,
            SpectreColor titleColor,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySectionWhenStyledCore(isVisible, options, title, RuleTitleStyle(model => titleColor), alignment);
        }

        /// <summary>
        /// Adds a conditional model validation summary section with an RGB title color.
        /// </summary>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the validation summary should be rendered.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleColor">RGB color used for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySectionWhen(
            Func<T, bool> isVisible,
            ValidationSummaryOptions options,
            string title,
            RgbColor titleColor,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySectionWhenStyledCore(isVisible, options, title, RuleTitleStyle(model => titleColor), alignment);
        }

        /// <summary>
        /// Adds a conditional model validation summary section with a Spectre-style title color.
        /// </summary>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the validation summary should be rendered.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleColor">Color facade used for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySectionWhen(
            Func<T, bool> isVisible,
            string title,
            SpectreColor titleColor,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySectionWhen(isVisible, null, title, titleColor, alignment);
        }

        /// <summary>
        /// Adds a conditional model validation summary section with an RGB title color.
        /// </summary>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the validation summary should be rendered.</param>
        /// <param name="title">Optional section title rendered above the validation summary.</param>
        /// <param name="titleColor">RGB color used for the section rule title.</param>
        /// <param name="alignment">Alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ValidationSummarySectionWhen(
            Func<T, bool> isVisible,
            string title,
            RgbColor titleColor,
            RuleTitleAlignment alignment = RuleTitleAlignment.Left)
        {
            return ValidationSummarySectionWhen(isVisible, null, title, titleColor, alignment);
        }

        /// <summary>
        /// Adds a synchronous option using StaticView-style naming.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Action action, ConsoleColor? color = null, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds a synchronous option using StaticView-style naming and style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Action action, string style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds a synchronous option using StaticView-style naming and a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Action action, Style style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<Task> action, ConsoleColor? color = null, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming and style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<Task> action, string style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming and a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<Task> action, Style style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<CancellationToken, Task> action, ConsoleColor? color = null, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming, style text, and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<CancellationToken, Task> action, string style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming, a reusable style, and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<CancellationToken, Task> action, Style style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds a synchronous model option using StaticView-style naming.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Action<T> action, ConsoleColor? color = null, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds a synchronous model option using StaticView-style naming and style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Action<T> action, string style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds a synchronous model option using StaticView-style naming and a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Action<T> action, Style style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<T, Task> action, ConsoleColor? color = null, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming and style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<T, Task> action, string style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming and a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<T, Task> action, Style style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<T, CancellationToken, Task> action, ConsoleColor? color = null, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming, style text, and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<T, CancellationToken, Task> action, string style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming, a reusable style, and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<T, CancellationToken, Task> action, Style style, string description = null)
        {
            return Option(label, action, style, description);
        }

        /// <summary>
        /// Adds a synchronous option using StaticView-style naming and a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Action action, SpectreColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds a synchronous option using StaticView-style naming and an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Action action, RgbColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming and a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<Task> action, SpectreColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming and an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<Task> action, RgbColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming, a Spectre-style color facade, and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<CancellationToken, Task> action, SpectreColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming, an RGB color mapped to Consoul's console palette, and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<CancellationToken, Task> action, RgbColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds a synchronous model option using StaticView-style naming and a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Action<T> action, SpectreColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds a synchronous model option using StaticView-style naming and an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Action<T> action, RgbColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming and a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<T, Task> action, SpectreColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming and an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<T, Task> action, RgbColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming, a Spectre-style color facade, and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<T, CancellationToken, Task> action, SpectreColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming, an RGB color mapped to Consoul's console palette, and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOption(string label, Func<T, CancellationToken, Task> action, RgbColor color, string description = null)
        {
            return Option(label, action, color, description);
        }

        /// <summary>
        /// Adds one synchronous model option for each item in a collection.
        /// </summary>
        /// <typeparam name="TItem">The item type projected into view options.</typeparam>
        /// <param name="items">Items to project into selectable options.</param>
        /// <param name="label">Function that creates each option label from an item.</param>
        /// <param name="action">Action invoked with the current model and selected item.</param>
        /// <param name="color">Optional function that creates each option foreground color from an item.</param>
        /// <param name="description">Optional function that creates helper text from an item.</param>
        /// <param name="configure">Optional callback used to configure each generated option with shortcuts, search terms, or styles.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOptions<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, string> label,
            Action<T, TItem> action,
            Func<TItem, ConsoleColor?> color = null,
            Func<TItem, string> description = null,
            Action<DynamicOption<T>, TItem> configure = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOptions(
                items,
                label,
                (model, item, cancellationToken) =>
                {
                    action(model, item);
                    return Task.CompletedTask;
                },
                color,
                description,
                configure);
        }

        /// <summary>
        /// Adds one model-gated synchronous option for each item in a collection.
        /// </summary>
        /// <typeparam name="TItem">The item type projected into view options.</typeparam>
        /// <param name="items">Items to project into selectable options.</param>
        /// <param name="label">Function that creates each option label from an item.</param>
        /// <param name="action">Action invoked with the current model and selected item.</param>
        /// <param name="isEnabled">Predicate that controls whether each generated option can be selected for the current model state.</param>
        /// <param name="disabledReason">Optional function that explains why a generated option is disabled.</param>
        /// <param name="color">Optional function that creates each option foreground color from an item.</param>
        /// <param name="description">Optional function that creates helper text from an item.</param>
        /// <param name="configure">Optional callback used to configure each generated option with shortcuts, search terms, or styles.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOptions<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, string> label,
            Action<T, TItem> action,
            Func<T, TItem, bool> isEnabled,
            Func<T, TItem, string> disabledReason = null,
            Func<TItem, ConsoleColor?> color = null,
            Func<TItem, string> description = null,
            Action<DynamicOption<T>, TItem> configure = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOptionsCore(
                items,
                label,
                (model, item, cancellationToken) =>
                {
                    action(model, item);
                    return Task.CompletedTask;
                },
                color,
                description,
                configure,
                isEnabled,
                disabledReason);
        }

        /// <summary>
        /// Adds one asynchronous model option for each item in a collection.
        /// </summary>
        /// <typeparam name="TItem">The item type projected into view options.</typeparam>
        /// <param name="items">Items to project into selectable options.</param>
        /// <param name="label">Function that creates each option label from an item.</param>
        /// <param name="action">Asynchronous action invoked with the current model and selected item.</param>
        /// <param name="color">Optional function that creates each option foreground color from an item.</param>
        /// <param name="description">Optional function that creates helper text from an item.</param>
        /// <param name="configure">Optional callback used to configure each generated option with shortcuts, search terms, or styles.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOptions<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, string> label,
            Func<T, TItem, Task> action,
            Func<TItem, ConsoleColor?> color = null,
            Func<TItem, string> description = null,
            Action<DynamicOption<T>, TItem> configure = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOptions(
                items,
                label,
                (model, item, cancellationToken) => action(model, item),
                color,
                description,
                configure);
        }

        /// <summary>
        /// Adds one model-gated asynchronous option for each item in a collection.
        /// </summary>
        /// <typeparam name="TItem">The item type projected into view options.</typeparam>
        /// <param name="items">Items to project into selectable options.</param>
        /// <param name="label">Function that creates each option label from an item.</param>
        /// <param name="action">Asynchronous action invoked with the current model and selected item.</param>
        /// <param name="isEnabled">Predicate that controls whether each generated option can be selected for the current model state.</param>
        /// <param name="disabledReason">Optional function that explains why a generated option is disabled.</param>
        /// <param name="color">Optional function that creates each option foreground color from an item.</param>
        /// <param name="description">Optional function that creates helper text from an item.</param>
        /// <param name="configure">Optional callback used to configure each generated option with shortcuts, search terms, or styles.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOptions<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, string> label,
            Func<T, TItem, Task> action,
            Func<T, TItem, bool> isEnabled,
            Func<T, TItem, string> disabledReason = null,
            Func<TItem, ConsoleColor?> color = null,
            Func<TItem, string> description = null,
            Action<DynamicOption<T>, TItem> configure = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AddOptionsCore(
                items,
                label,
                (model, item, cancellationToken) => action(model, item),
                color,
                description,
                configure,
                isEnabled,
                disabledReason);
        }

        /// <summary>
        /// Adds one cancellation-aware asynchronous model option for each item in a collection.
        /// </summary>
        /// <typeparam name="TItem">The item type projected into view options.</typeparam>
        /// <param name="items">Items to project into selectable options.</param>
        /// <param name="label">Function that creates each option label from an item.</param>
        /// <param name="action">Asynchronous action invoked with the current model, selected item, and active render token.</param>
        /// <param name="color">Optional function that creates each option foreground color from an item.</param>
        /// <param name="description">Optional function that creates helper text from an item.</param>
        /// <param name="configure">Optional callback used to configure each generated option with shortcuts, search terms, or styles.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOptions<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, string> label,
            Func<T, TItem, CancellationToken, Task> action,
            Func<TItem, ConsoleColor?> color = null,
            Func<TItem, string> description = null,
            Action<DynamicOption<T>, TItem> configure = null)
        {
            return AddOptionsCore(items, label, action, color, description, configure, null, null);
        }

        /// <summary>
        /// Adds one model-gated cancellation-aware asynchronous option for each item in a collection.
        /// </summary>
        /// <typeparam name="TItem">The item type projected into view options.</typeparam>
        /// <param name="items">Items to project into selectable options.</param>
        /// <param name="label">Function that creates each option label from an item.</param>
        /// <param name="action">Asynchronous action invoked with the current model, selected item, and active render token.</param>
        /// <param name="isEnabled">Predicate that controls whether each generated option can be selected for the current model state.</param>
        /// <param name="disabledReason">Optional function that explains why a generated option is disabled.</param>
        /// <param name="color">Optional function that creates each option foreground color from an item.</param>
        /// <param name="description">Optional function that creates helper text from an item.</param>
        /// <param name="configure">Optional callback used to configure each generated option with shortcuts, search terms, or styles.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddOptions<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, string> label,
            Func<T, TItem, CancellationToken, Task> action,
            Func<T, TItem, bool> isEnabled,
            Func<T, TItem, string> disabledReason = null,
            Func<TItem, ConsoleColor?> color = null,
            Func<TItem, string> description = null,
            Action<DynamicOption<T>, TItem> configure = null)
        {
            return AddOptionsCore(items, label, action, color, description, configure, isEnabled, disabledReason);
        }

        private DynamicView<T> AddOptionsCore<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, string> label,
            Func<T, TItem, CancellationToken, Task> action,
            Func<TItem, ConsoleColor?> color,
            Func<TItem, string> description,
            Action<DynamicOption<T>, TItem> configure,
            Func<T, TItem, bool> isEnabled,
            Func<T, TItem, string> disabledReason)
        {
            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (disabledReason != null && isEnabled == null)
            {
                throw new ArgumentNullException(nameof(isEnabled));
            }

            if (items == null)
            {
                return this;
            }

            foreach (TItem item in items)
            {
                TItem capturedItem = item;
                var option = new DynamicOption<T>(
                    () => label(capturedItem),
                    cancellationToken => action(Model, capturedItem, cancellationToken) ?? Task.CompletedTask,
                    () => RenderOptions.GetColorOrDefault(color != null ? color(capturedItem) : RenderOptions.OptionColor));

                if (description != null)
                {
                    option.Describe(() => description(capturedItem));
                }

                if (isEnabled != null)
                {
                    option.EnableWhen(
                        () => isEnabled(Model, capturedItem),
                        () => disabledReason != null ? disabledReason(Model, capturedItem) : null);
                }

                if (configure != null)
                {
                    configure(option, capturedItem);
                }

                _options.Add(option);
            }

            return this;
        }

        /// <summary>
        /// Adds a synchronous option to the dynamic view.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action action, ConsoleColor? color = null)
        {
            return Option(label, action, color, null);
        }

        /// <summary>
        /// Adds a synchronous option to the dynamic view using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action action, string style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds a synchronous option using a reusable style for the option label.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action action, Style style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous option with helper text to the dynamic view.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action action, ConsoleColor? color, string description)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Option(model => label, (model, cancellationToken) =>
            {
                action();
                return Task.CompletedTask;
            }, model => color, model => description);
        }

        /// <summary>
        /// Adds a synchronous option with helper text to the dynamic view using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action action, string style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds a synchronous option with helper text using a reusable style for the option label.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action action, Style style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous option to the dynamic view without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<Task> action, ConsoleColor? color = null)
        {
            return Option(label, action, color, null);
        }

        /// <summary>
        /// Adds an asynchronous option to the dynamic view using style text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<Task> action, string style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous option using a reusable style without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<Task> action, Style style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous option with helper text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<Task> action, ConsoleColor? color, string description)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Option(label, (Func<CancellationToken, Task>)(cancellationToken => action()), color, description);
        }

        /// <summary>
        /// Adds an asynchronous option with helper text using style text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<Task> action, string style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous option with helper text using a reusable style without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<Task> action, Style style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous option to the dynamic view.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<CancellationToken, Task> action, ConsoleColor? color = null)
        {
            return Option(label, action, color, null);
        }

        /// <summary>
        /// Adds an asynchronous option to the dynamic view using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<CancellationToken, Task> action, string style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous option using a reusable style for the option label.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<CancellationToken, Task> action, Style style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous option with helper text to the dynamic view.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<CancellationToken, Task> action, ConsoleColor? color, string description)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Option(model => label, (model, cancellationToken) => action(cancellationToken), model => color, model => description);
        }

        /// <summary>
        /// Adds an asynchronous option with helper text to the dynamic view using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<CancellationToken, Task> action, string style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous option with helper text using a reusable style for the option label.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<CancellationToken, Task> action, Style style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action to the dynamic view.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action<T> action, ConsoleColor? color = null)
        {
            return Option(label, action, color, null);
        }

        /// <summary>
        /// Adds a synchronous model action to the dynamic view using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action<T> action, string style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds a synchronous model action using a reusable style for the option label.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action<T> action, Style style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with helper text to the dynamic view.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action<T> action, ConsoleColor? color, string description)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Option(model => label, (model, cancellationToken) =>
            {
                action(model);
                return Task.CompletedTask;
            }, model => color, model => description);
        }

        /// <summary>
        /// Adds a synchronous model action with helper text to the dynamic view using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action<T> action, string style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds a synchronous model action with helper text using a reusable style for the option label.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action<T> action, Style style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action to the dynamic view without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, Task> action, ConsoleColor? color = null)
        {
            return Option(label, action, color, null);
        }

        /// <summary>
        /// Adds an asynchronous model action to the dynamic view using style text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, Task> action, string style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous model action using a reusable style without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, Task> action, Style style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with helper text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, Task> action, ConsoleColor? color, string description)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Option(label, (Func<T, CancellationToken, Task>)((model, cancellationToken) => action(model)), color, description);
        }

        /// <summary>
        /// Adds an asynchronous model action with helper text using style text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, Task> action, string style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous model action with helper text using a reusable style without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, Task> action, Style style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action to the dynamic view.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, CancellationToken, Task> action, ConsoleColor? color = null)
        {
            return Option(label, action, color, null);
        }

        /// <summary>
        /// Adds an asynchronous model action to the dynamic view using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, CancellationToken, Task> action, string style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous model action using a reusable style for the option label.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, CancellationToken, Task> action, Style style)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with helper text to the dynamic view.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, CancellationToken, Task> action, ConsoleColor? color, string description)
        {
            return Option(model => label, action, model => color, model => description);
        }

        /// <summary>
        /// Adds an asynchronous model action with helper text to the dynamic view using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, CancellationToken, Task> action, string style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous model action with helper text using a reusable style for the option label.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, CancellationToken, Task> action, Style style, string description)
        {
            return Option(label, action, ResolveStyle(style, RenderOptions.OptionColor), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous option to the dynamic view using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action action, SpectreColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds a synchronous option to the dynamic view using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action action, RgbColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous option to the dynamic view using a Spectre-style color facade without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<Task> action, SpectreColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds an asynchronous option to the dynamic view using an RGB color mapped to Consoul's console palette without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<Task> action, RgbColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous option to the dynamic view using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<CancellationToken, Task> action, SpectreColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds an asynchronous option to the dynamic view using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the active cancellation token when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<CancellationToken, Task> action, RgbColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous model action to the dynamic view using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action<T> action, SpectreColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds a synchronous model action to the dynamic view using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Action<T> action, RgbColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action to the dynamic view using a Spectre-style color facade without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, Task> action, SpectreColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds an asynchronous model action to the dynamic view using an RGB color mapped to Consoul's console palette without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, Task> action, RgbColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action to the dynamic view using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, CancellationToken, Task> action, SpectreColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds an asynchronous model action to the dynamic view using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(string label, Func<T, CancellationToken, Task> action, RgbColor color, string description = null)
        {
            return Option(label, action, (ConsoleColor?)color.ToConsoleColor(), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous model action with a label evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Action<T> action, Func<T, ConsoleColor?> color = null)
        {
            return Option(label, action, color, null);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label and style text evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Action<T> action, Func<T, string> style)
        {
            return Option(label, action, StyleColor(style), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label and reusable style evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Action<T> action, Func<T, Style> style)
        {
            return Option(label, action, StyleColor(style), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label and Spectre-style color evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Action<T> action, Func<T, SpectreColor> color)
        {
            return Option(label, action, StyleColor(color), null)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label and RGB color evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Action<T> action, Func<T, RgbColor> color)
        {
            return Option(label, action, StyleColor(color), null)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label and helper text evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Action<T> action, Func<T, ConsoleColor?> color, Func<T, string> description)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Option(label, (model, cancellationToken) =>
            {
                action(model);
                return Task.CompletedTask;
            }, color, description);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label, style text, and helper text evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Action<T> action, Func<T, string> style, Func<T, string> description)
        {
            return Option(label, action, StyleColor(style), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label, helper text, and reusable style evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Action<T> action, Func<T, Style> style, Func<T, string> description)
        {
            return Option(label, action, StyleColor(style), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label, helper text, and Spectre-style color evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Action<T> action, Func<T, SpectreColor> color, Func<T, string> description)
        {
            return Option(label, action, StyleColor(color), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label, helper text, and RGB color evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Action<T> action, Func<T, RgbColor> color, Func<T, string> description)
        {
            return Option(label, action, StyleColor(color), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with a label evaluated from the current model without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, Task> action, Func<T, ConsoleColor?> color = null)
        {
            return Option(label, action, color, null);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and style text evaluated from the current model without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, Task> action, Func<T, string> style)
        {
            return Option(label, action, StyleColor(style), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and reusable style evaluated from the current model without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, Task> action, Func<T, Style> style)
        {
            return Option(label, action, StyleColor(style), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and Spectre-style color evaluated from the current model without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, Task> action, Func<T, SpectreColor> color)
        {
            return Option(label, action, StyleColor(color), null)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and RGB color evaluated from the current model without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, Task> action, Func<T, RgbColor> color)
        {
            return Option(label, action, StyleColor(color), null)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and helper text evaluated from the current model without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, Task> action, Func<T, ConsoleColor?> color, Func<T, string> description)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Option(label, (Func<T, CancellationToken, Task>)((model, cancellationToken) => action(model)), color, description);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label, style text, and helper text evaluated from the current model without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, Task> action, Func<T, string> style, Func<T, string> description)
        {
            return Option(label, action, StyleColor(style), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label, helper text, and reusable style evaluated from the current model without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, Task> action, Func<T, Style> style, Func<T, string> description)
        {
            return Option(label, action, StyleColor(style), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label, helper text, and Spectre-style color evaluated from the current model without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, Task> action, Func<T, SpectreColor> color, Func<T, string> description)
        {
            return Option(label, action, StyleColor(color), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label, helper text, and RGB color evaluated from the current model without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, Task> action, Func<T, RgbColor> color, Func<T, string> description)
        {
            return Option(label, action, StyleColor(color), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with a label evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, ConsoleColor?> color = null)
        {
            return Option(label, action, color, null);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and style text evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, string> style)
        {
            return Option(label, action, StyleColor(style), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and reusable style evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, Style> style)
        {
            return Option(label, action, StyleColor(style), null)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and Spectre-style color evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, SpectreColor> color)
        {
            return Option(label, action, StyleColor(color), null)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and RGB color evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, RgbColor> color)
        {
            return Option(label, action, StyleColor(color), null)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and helper text evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, ConsoleColor?> color, Func<T, string> description)
        {
            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            AddOption(label, action, color, null, null, description);
            return this;
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label, style text, and helper text evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, string> style, Func<T, string> description)
        {
            return Option(label, action, StyleColor(style), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label, helper text, and reusable style evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, Style> style, Func<T, string> description)
        {
            return Option(label, action, StyleColor(style), description)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label, helper text, and Spectre-style color evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, SpectreColor> color, Func<T, string> description)
        {
            return Option(label, action, StyleColor(color), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label, helper text, and RGB color evaluated from the current model.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="description">Optional function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Option(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, RgbColor> color, Func<T, string> description)
        {
            return Option(label, action, StyleColor(color), description)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous model action that is selectable only while the model satisfies a predicate.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Action<T> action, Func<T, bool> isEnabled, string disabledReason = null, ConsoleColor? color = null)
        {
            return OptionWhen(
                model => label,
                action,
                isEnabled,
                model => disabledReason,
                model => color);
        }

        /// <summary>
        /// Adds a synchronous model action that is selectable only while the model satisfies a predicate using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Action<T> action, Func<T, bool> isEnabled, string disabledReason, string style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, ResolveStyle(style, RenderOptions.OptionColor))
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds a synchronous model action that is selectable only while the model satisfies a predicate using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Action<T> action, Func<T, bool> isEnabled, string disabledReason, Style style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, ResolveStyle(style, RenderOptions.OptionColor))
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous action that is selectable only while the model satisfies a predicate without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Func<Task> action, Func<T, bool> isEnabled, string disabledReason = null, ConsoleColor? color = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return OptionWhen(label, (Func<T, CancellationToken, Task>)((model, cancellationToken) => action()), isEnabled, disabledReason, color);
        }

        /// <summary>
        /// Adds an asynchronous action that is selectable only while the model satisfies a predicate using style text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Func<Task> action, Func<T, bool> isEnabled, string disabledReason, string style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, ResolveStyle(style, RenderOptions.OptionColor))
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous action that is selectable only while the model satisfies a predicate using a reusable style without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Func<Task> action, Func<T, bool> isEnabled, string disabledReason, Style style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, ResolveStyle(style, RenderOptions.OptionColor))
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only while the model satisfies a predicate without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Func<T, Task> action, Func<T, bool> isEnabled, string disabledReason = null, ConsoleColor? color = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return OptionWhen(label, (Func<T, CancellationToken, Task>)((model, cancellationToken) => action(model)), isEnabled, disabledReason, color);
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only while the model satisfies a predicate using style text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Func<T, Task> action, Func<T, bool> isEnabled, string disabledReason, string style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, ResolveStyle(style, RenderOptions.OptionColor))
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only while the model satisfies a predicate using a reusable style without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Func<T, Task> action, Func<T, bool> isEnabled, string disabledReason, Style style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, ResolveStyle(style, RenderOptions.OptionColor))
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only while the model satisfies a predicate.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Func<T, CancellationToken, Task> action, Func<T, bool> isEnabled, string disabledReason = null, ConsoleColor? color = null)
        {
            return OptionWhen(
                model => label,
                action,
                isEnabled,
                model => disabledReason,
                model => color);
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only while the model satisfies a predicate using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Func<T, CancellationToken, Task> action, Func<T, bool> isEnabled, string disabledReason, string style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, ResolveStyle(style, RenderOptions.OptionColor))
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only while the model satisfies a predicate using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(string label, Func<T, CancellationToken, Task> action, Func<T, bool> isEnabled, string disabledReason, Style style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, ResolveStyle(style, RenderOptions.OptionColor))
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with a dynamic label that is selectable only while the model satisfies a predicate.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Action<T> action, Func<T, bool> isEnabled, Func<T, string> disabledReason = null, Func<T, ConsoleColor?> color = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return OptionWhen(label, (model, cancellationToken) =>
            {
                action(model);
                return Task.CompletedTask;
            }, isEnabled, disabledReason, color);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label that is selectable only while the model satisfies a predicate using style text.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Action<T> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, string> style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(style))
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using a reusable style.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Action<T> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, Style> style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(style))
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using a Spectre-style color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Action<T> action, Func<T, bool> isEnabled, Func<T, SpectreColor> color)
        {
            return OptionWhen(label, action, isEnabled, null, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using an RGB color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Action<T> action, Func<T, bool> isEnabled, Func<T, RgbColor> color)
        {
            return OptionWhen(label, action, isEnabled, null, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using a Spectre-style color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Action<T> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, SpectreColor> color)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using an RGB color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Action<T> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, RgbColor> color)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, Task> action, Func<T, bool> isEnabled, Func<T, string> disabledReason = null, Func<T, ConsoleColor?> color = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return OptionWhen(label, (Func<T, CancellationToken, Task>)((model, cancellationToken) => action(model)), isEnabled, disabledReason, color);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using style text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, Task> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, string> style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(style))
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using a reusable style without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, Task> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, Style> style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(style))
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using a Spectre-style color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, Task> action, Func<T, bool> isEnabled, Func<T, SpectreColor> color)
        {
            return OptionWhen(label, action, isEnabled, null, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using an RGB color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, Task> action, Func<T, bool> isEnabled, Func<T, RgbColor> color)
        {
            return OptionWhen(label, action, isEnabled, null, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using a Spectre-style color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, Task> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, SpectreColor> color)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using an RGB color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, Task> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, RgbColor> color)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, bool> isEnabled, Func<T, string> disabledReason = null, Func<T, ConsoleColor?> color = null)
        {
            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (isEnabled == null)
            {
                throw new ArgumentNullException(nameof(isEnabled));
            }

            AddOption(label, action, color, isEnabled, disabledReason, null);
            return this;
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label that is selectable only while the model satisfies a predicate using style text.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, string> style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(style))
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using a reusable style.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, Style> style)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(style))
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using a Spectre-style color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, bool> isEnabled, Func<T, SpectreColor> color)
        {
            return OptionWhen(label, action, isEnabled, null, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using an RGB color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, bool> isEnabled, Func<T, RgbColor> color)
        {
            return OptionWhen(label, action, isEnabled, null, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using a Spectre-style color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, SpectreColor> color)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only while the model satisfies a predicate using an RGB color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhen(Func<T, string> label, Func<T, CancellationToken, Task> action, Func<T, bool> isEnabled, Func<T, string> disabledReason, Func<T, RgbColor> color)
        {
            return OptionWhen(label, action, isEnabled, disabledReason, StyleColor(color))
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous model action that is selectable only when the current model validates successfully.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Action<T> action,
            string disabledReason = "fix validation issues first",
            ConsoleColor? color = null,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhen(
                label,
                action,
                model => IsModelValid(model, validationOptions),
                disabledReason,
                color);
        }

        /// <summary>
        /// Adds a synchronous model action that is selectable only when the current model validates successfully using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Action<T> action,
            string disabledReason,
            string style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, ResolveStyle(style, RenderOptions.OptionColor), validationOptions)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds a synchronous model action that is selectable only when the current model validates successfully using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Action<T> action,
            string disabledReason,
            Style style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, ResolveStyle(style, RenderOptions.OptionColor), validationOptions)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous action that is selectable only when the current model validates successfully without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Func<Task> action,
            string disabledReason = "fix validation issues first",
            ConsoleColor? color = null,
            ValidationSummaryOptions validationOptions = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return OptionWhenValid(label, (Func<T, CancellationToken, Task>)((model, cancellationToken) => action()), disabledReason, color, validationOptions);
        }

        /// <summary>
        /// Adds an asynchronous action that is selectable only when the current model validates successfully using style text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Func<Task> action,
            string disabledReason,
            string style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, ResolveStyle(style, RenderOptions.OptionColor), validationOptions)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous action that is selectable only when the current model validates successfully using a reusable style without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Func<Task> action,
            string disabledReason,
            Style style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, ResolveStyle(style, RenderOptions.OptionColor), validationOptions)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only when the current model validates successfully without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Func<T, Task> action,
            string disabledReason = "fix validation issues first",
            ConsoleColor? color = null,
            ValidationSummaryOptions validationOptions = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return OptionWhenValid(label, (Func<T, CancellationToken, Task>)((model, cancellationToken) => action(model)), disabledReason, color, validationOptions);
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only when the current model validates successfully using style text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Func<T, Task> action,
            string disabledReason,
            string style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, ResolveStyle(style, RenderOptions.OptionColor), validationOptions)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only when the current model validates successfully using a reusable style without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Func<T, Task> action,
            string disabledReason,
            Style style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, ResolveStyle(style, RenderOptions.OptionColor), validationOptions)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only when the current model validates successfully.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Func<T, CancellationToken, Task> action,
            string disabledReason = "fix validation issues first",
            ConsoleColor? color = null,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhen(
                label,
                action,
                model => IsModelValid(model, validationOptions),
                disabledReason,
                color);
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only when the current model validates successfully using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Func<T, CancellationToken, Task> action,
            string disabledReason,
            string style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, ResolveStyle(style, RenderOptions.OptionColor), validationOptions)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous model action that is selectable only when the current model validates successfully using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Reason rendered when the current model has validation issues.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            string label,
            Func<T, CancellationToken, Task> action,
            string disabledReason,
            Style style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, ResolveStyle(style, RenderOptions.OptionColor), validationOptions)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with a dynamic label that is selectable only when the current model validates successfully.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Optional function that creates the disabled reason from the current model state.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            Func<T, string> label,
            Action<T> action,
            Func<T, string> disabledReason = null,
            Func<T, ConsoleColor?> color = null,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhen(
                label,
                action,
                model => IsModelValid(model, validationOptions),
                disabledReason ?? (model => "fix validation issues first"),
                color);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label that is selectable only when the current model validates successfully using style text.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Optional function that creates the disabled reason from the current model state.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            Func<T, string> label,
            Action<T> action,
            Func<T, string> disabledReason,
            Func<T, string> style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, StyleColor(style), validationOptions)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with a dynamic label that is selectable only when the current model validates successfully using a reusable style.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Optional function that creates the disabled reason from the current model state.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            Func<T, string> label,
            Action<T> action,
            Func<T, string> disabledReason,
            Func<T, Style> style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, StyleColor(style), validationOptions)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only when the current model validates successfully without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Optional function that creates the disabled reason from the current model state.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            Func<T, string> label,
            Func<T, Task> action,
            Func<T, string> disabledReason = null,
            Func<T, ConsoleColor?> color = null,
            ValidationSummaryOptions validationOptions = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return OptionWhenValid(label, (Func<T, CancellationToken, Task>)((model, cancellationToken) => action(model)), disabledReason, color, validationOptions);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label that is selectable only when the current model validates successfully using style text without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Optional function that creates the disabled reason from the current model state.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            Func<T, string> label,
            Func<T, Task> action,
            Func<T, string> disabledReason,
            Func<T, string> style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, StyleColor(style), validationOptions)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only when the current model validates successfully using a reusable style without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Optional function that creates the disabled reason from the current model state.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            Func<T, string> label,
            Func<T, Task> action,
            Func<T, string> disabledReason,
            Func<T, Style> style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, StyleColor(style), validationOptions)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only when the current model validates successfully.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Optional function that creates the disabled reason from the current model state.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            Func<T, string> label,
            Func<T, CancellationToken, Task> action,
            Func<T, string> disabledReason = null,
            Func<T, ConsoleColor?> color = null,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhen(
                label,
                action,
                model => IsModelValid(model, validationOptions),
                disabledReason ?? (model => "fix validation issues first"),
                color);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label that is selectable only when the current model validates successfully using style text.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Optional function that creates the disabled reason from the current model state.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            Func<T, string> label,
            Func<T, CancellationToken, Task> action,
            Func<T, string> disabledReason,
            Func<T, string> style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, StyleColor(style), validationOptions)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with a dynamic label that is selectable only when the current model validates successfully using a reusable style.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the option is selected.</param>
        /// <param name="disabledReason">Optional function that creates the disabled reason from the current model state.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="validationOptions">Optional validation options used when checking the model.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> OptionWhenValid(
            Func<T, string> label,
            Func<T, CancellationToken, Task> action,
            Func<T, string> disabledReason,
            Func<T, Style> style,
            ValidationSummaryOptions validationOptions = null)
        {
            return OptionWhenValid(label, action, disabledReason, StyleColor(style), validationOptions)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous option that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Action action, ConsoleColor? color = null, bool? defaultValue = false)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return ConfirmOption(
                model => label,
                model => confirmationMessage,
                (model, cancellationToken) =>
                {
                    action();
                    return Task.CompletedTask;
                },
                model => color,
                defaultValue);
        }

        /// <summary>
        /// Adds a synchronous option that asks for confirmation before running using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Action action, string style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds a synchronous option that asks for confirmation before running using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">Message shown in the confirmation prompt.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Action action, Style style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<Task> action, ConsoleColor? color = null, bool? defaultValue = false)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return ConfirmOption(label, confirmationMessage, (Func<T, CancellationToken, Task>)((model, cancellationToken) => action()), color, defaultValue);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<Task> action, string style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<Task> action, Style style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the active cancellation token when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, ConsoleColor? color = null, bool? defaultValue = false)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return ConfirmOption(label, confirmationMessage, (model, cancellationToken) => action(cancellationToken), color, defaultValue);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using style text and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the active cancellation token when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, string style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using a reusable style and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the active cancellation token when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, Style style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a model action that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Action<T> action, ConsoleColor? color = null, bool? defaultValue = false)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return ConfirmOption(
                model => label,
                model => confirmationMessage,
                (model, cancellationToken) =>
                {
                    action(model);
                    return Task.CompletedTask;
                },
                model => color,
                defaultValue);
        }

        /// <summary>
        /// Adds a model action that asks for confirmation before running using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Action<T> action, string style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds a synchronous model action that asks for confirmation before running using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">Message shown in the confirmation prompt.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Action<T> action, Style style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<T, Task> action, ConsoleColor? color = null, bool? defaultValue = false)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return ConfirmOption(label, confirmationMessage, (Func<T, CancellationToken, Task>)((model, cancellationToken) => action(model)), color, defaultValue);
        }

        /// <summary>
        /// Adds an asynchronous model action that asks for confirmation before running using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<T, Task> action, string style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous model action that asks for confirmation before running using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<T, Task> action, Style style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<T, CancellationToken, Task> action, ConsoleColor? color = null, bool? defaultValue = false)
        {
            return ConfirmOption(model => label, model => confirmationMessage, action, model => color, defaultValue);
        }

        /// <summary>
        /// Adds an asynchronous model action that asks for confirmation before running using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<T, CancellationToken, Task> action, string style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an asynchronous model action that asks for confirmation before running using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">Message shown in the confirmation prompt.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<T, CancellationToken, Task> action, Style style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, ResolveStyle(style, RenderOptions.OptionColor), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous option that asks for confirmation before running using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Action action, SpectreColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds a synchronous option that asks for confirmation before running using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Action action, RgbColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<Task> action, SpectreColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<Task> action, RgbColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using a Spectre-style color facade and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the active cancellation token when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, SpectreColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds an asynchronous option that asks for confirmation before running using an RGB color mapped to Consoul's console palette and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the active cancellation token when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, RgbColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a model action that asks for confirmation before running using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Action<T> action, SpectreColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds a model action that asks for confirmation before running using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Action<T> action, RgbColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action that asks for confirmation before running using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<T, Task> action, SpectreColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds an asynchronous model action that asks for confirmation before running using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<T, Task> action, RgbColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action that asks for confirmation before running using a Spectre-style color facade and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<T, CancellationToken, Task> action, SpectreColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds an asynchronous model action that asks for confirmation before running using an RGB color mapped to Consoul's console palette and the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(string label, string confirmationMessage, Func<T, CancellationToken, Task> action, RgbColor color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, (ConsoleColor?)color.ToConsoleColor(), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a model action with dynamic label and confirmation text that asks for confirmation before running.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Action<T> action, Func<T, ConsoleColor?> color = null, bool? defaultValue = false)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return ConfirmOption(label, confirmationMessage, (model, cancellationToken) =>
            {
                action(model);
                return Task.CompletedTask;
            }, color, defaultValue);
        }

        /// <summary>
        /// Adds a model action with dynamic label and confirmation text that asks for confirmation before running using style text.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Action<T> action, Func<T, string> style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(style), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label and confirmation text that asks for confirmation before running using a reusable style.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Action<T> action, Func<T, Style> style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(style), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label and confirmation text that asks for confirmation before running using a Spectre-style color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Action<T> action, Func<T, SpectreColor> color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(color), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous model action with dynamic label and confirmation text that asks for confirmation before running using an RGB color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Action<T> action, Func<T, RgbColor> color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(color), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and confirmation text that asks for confirmation before running.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Func<T, Task> action, Func<T, ConsoleColor?> color = null, bool? defaultValue = false)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return ConfirmOption(label, confirmationMessage, (Func<T, CancellationToken, Task>)((model, cancellationToken) => action(model)), color, defaultValue);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and confirmation text that asks for confirmation before running using style text.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Func<T, Task> action, Func<T, string> style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(style), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and confirmation text that asks for confirmation before running using a reusable style.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Func<T, Task> action, Func<T, Style> style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(style), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and confirmation text that asks for confirmation before running using a Spectre-style color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Func<T, Task> action, Func<T, SpectreColor> color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(color), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and confirmation text that asks for confirmation before running using an RGB color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Func<T, Task> action, Func<T, RgbColor> color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(color), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and confirmation text that asks for confirmation before running.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">Optional function that creates the option color from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Func<T, CancellationToken, Task> action, Func<T, ConsoleColor?> color = null, bool? defaultValue = false)
        {
            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            if (confirmationMessage == null)
            {
                throw new ArgumentNullException(nameof(confirmationMessage));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Option(label, async (model, cancellationToken) =>
            {
                ConfirmResult result = Consoul.Confirm(
                    confirmationMessage(model),
                    defaultValue: defaultValue,
                    cancellationToken: cancellationToken);
                if (result.HasValue && result.Value)
                {
                    await action(model, cancellationToken).ConfigureAwait(false);
                }
            }, color, null);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and confirmation text that asks for confirmation before running using style text.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Func<T, CancellationToken, Task> action, Func<T, string> style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(style), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and confirmation text that asks for confirmation before running using a reusable style.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional function that creates the option style from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Func<T, CancellationToken, Task> action, Func<T, Style> style, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(style), defaultValue)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and confirmation text that asks for confirmation before running using a Spectre-style color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Func<T, CancellationToken, Task> action, Func<T, SpectreColor> color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(color), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an asynchronous model action with dynamic label and confirmation text that asks for confirmation before running using an RGB color.
        /// </summary>
        /// <param name="label">Function that creates the option label from the current model state.</param>
        /// <param name="confirmationMessage">Function that creates the confirmation message from the current model state.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">Function that creates the option color from the current model state.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ConfirmOption(Func<T, string> label, Func<T, string> confirmationMessage, Func<T, CancellationToken, Task> action, Func<T, RgbColor> color, bool? defaultValue = false)
        {
            return ConfirmOption(label, confirmationMessage, action, StyleColor(color), defaultValue)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds a synchronous option using StaticView-style naming that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Action action, ConsoleColor? color = null, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds a synchronous option using StaticView-style naming and style text that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Action action, string style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds a synchronous option using StaticView-style naming and a reusable style that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Action action, Style style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<Task> action, ConsoleColor? color = null, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming and style text that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<Task> action, string style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming and a reusable style that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<Task> action, Style style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming that asks for confirmation before running and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the active cancellation token when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, ConsoleColor? color = null, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming and style text that asks for confirmation before running and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the active cancellation token when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, string style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming and a reusable style that asks for confirmation before running and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the active cancellation token when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, Style style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds a model option using StaticView-style naming that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Action<T> action, ConsoleColor? color = null, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds a model option using StaticView-style naming and style text that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Action<T> action, string style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds a model option using StaticView-style naming and a reusable style that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Action<T> action, Style style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<T, Task> action, ConsoleColor? color = null, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming and style text that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<T, Task> action, string style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming and a reusable style that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<T, Task> action, Style style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming that asks for confirmation before running and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the user confirms.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<T, CancellationToken, Task> action, ConsoleColor? color = null, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming and style text that asks for confirmation before running and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<T, CancellationToken, Task> action, string style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming and a reusable style that asks for confirmation before running and observes the active render cancellation token.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the user confirms.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<T, CancellationToken, Task> action, Style style, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, style, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds a synchronous option using StaticView-style naming and a Spectre-style color facade that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Action action, SpectreColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds a synchronous option using StaticView-style naming and an RGB color mapped to Consoul's console palette that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Action action, RgbColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming and a Spectre-style color facade that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<Task> action, SpectreColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming and an RGB color mapped to Consoul's console palette that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<Task> action, RgbColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming, a Spectre-style color facade, and the active render cancellation token that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the active cancellation token when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, SpectreColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous option using StaticView-style naming, an RGB color mapped to Consoul's console palette, and the active render cancellation token that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the active cancellation token when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<CancellationToken, Task> action, RgbColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds a synchronous model option using StaticView-style naming and a Spectre-style color facade that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Action<T> action, SpectreColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds a synchronous model option using StaticView-style naming and an RGB color mapped to Consoul's console palette that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Action<T> action, RgbColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming and a Spectre-style color facade that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<T, Task> action, SpectreColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming and an RGB color mapped to Consoul's console palette that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The asynchronous action invoked with the current model when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<T, Task> action, RgbColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming, a Spectre-style color facade, and the active render cancellation token that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the user confirms.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<T, CancellationToken, Task> action, SpectreColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an asynchronous model option using StaticView-style naming, an RGB color mapped to Consoul's console palette, and the active render cancellation token that asks for confirmation before running.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="confirmationMessage">The confirmation message shown before running the action.</param>
        /// <param name="action">The action invoked with the current model and active cancellation token when the user confirms.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="defaultValue">Optional default confirmation value used when the user presses Enter.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddConfirmOption(string label, string confirmationMessage, Func<T, CancellationToken, Task> action, RgbColor color, bool? defaultValue = false, string description = null)
        {
            return ConfirmOption(label, confirmationMessage, action, color, defaultValue)
                .DescribeLastOptionIfProvided(description);
        }

        private DynamicView<T> DescribeLastOptionIfProvided(string description)
        {
            if (description != null)
            {
                DescribeLastOption(description);
            }

            return this;
        }

        private DynamicView<T> StyleLastOption(Style style)
        {
            if (style != null)
            {
                GetLastSelectableOption("style the last option").UseStyle(style);
            }

            return this;
        }

        private DynamicView<T> StyleLastOption(Func<T, string> style)
        {
            if (style != null)
            {
                GetLastSelectableOption("style the last option").UseStyle(() => style(Model));
            }

            return this;
        }

        private DynamicView<T> StyleLastOption(Func<T, Style> style)
        {
            if (style != null)
            {
                GetLastSelectableOption("style the last option").UseStyle(() => style(Model));
            }

            return this;
        }

        private DynamicView<T> ColorLastOption(RgbColor color)
        {
            GetLastSelectableOption("color the last option").UseColor(color);
            return this;
        }

        private DynamicView<T> ColorLastOption(Func<T, SpectreColor> color)
        {
            if (color != null)
            {
                GetLastSelectableOption("color the last option").UseColor(() => color(Model));
            }

            return this;
        }

        private DynamicView<T> ColorLastOption(Func<T, RgbColor> color)
        {
            if (color != null)
            {
                GetLastSelectableOption("color the last option").UseColor(() => color(Model));
            }

            return this;
        }

        /// <summary>
        /// Adds helper text to the most recently added option.
        /// </summary>
        /// <param name="description">The helper text to render under the most recently added option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> DescribeLastOption(string description)
        {
            DynamicOption<T> option = GetLastSelectableOption("describe");
            option.Describe(description);
            return this;
        }

        /// <summary>
        /// Adds dynamic helper text to the most recently added option.
        /// </summary>
        /// <param name="description">Function that creates helper text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> DescribeLastOption(Func<T, string> description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            DynamicOption<T> option = GetLastSelectableOption("describe");
            option.Describe(() => description(Model));
            return this;
        }

        /// <summary>
        /// Adds shortcut text to the most recently added option.
        /// </summary>
        /// <param name="shortcut">Shortcut text for the most recently added option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutLastOption(string shortcut)
        {
            DynamicOption<T> option = GetLastSelectableOption("assign a shortcut");
            option.UseShortcut(shortcut);
            return this;
        }

        /// <summary>
        /// Adds dynamic shortcut text to the most recently added option.
        /// </summary>
        /// <param name="shortcut">Function that creates shortcut text from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> ShortcutLastOption(Func<T, string> shortcut)
        {
            if (shortcut == null)
            {
                throw new ArgumentNullException(nameof(shortcut));
            }

            DynamicOption<T> option = GetLastSelectableOption("assign a shortcut");
            option.UseShortcut(() => shortcut(Model));
            return this;
        }

        /// <summary>
        /// Adds hidden search terms to the most recently added option.
        /// </summary>
        /// <param name="searchTerms">Search terms or aliases for the most recently added option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SearchLastOption(params string[] searchTerms)
        {
            DynamicOption<T> option = GetLastSelectableOption("assign search terms");
            option.AddSearchTerms(searchTerms);
            return this;
        }

        /// <summary>
        /// Adds dynamic hidden search terms to the most recently added option.
        /// </summary>
        /// <param name="searchTerms">Function that creates search terms from the current model state.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> SearchLastOption(Func<T, IEnumerable<string>> searchTerms)
        {
            if (searchTerms == null)
            {
                throw new ArgumentNullException(nameof(searchTerms));
            }

            DynamicOption<T> option = GetLastSelectableOption("assign search terms");
            option.UseSearchTerms(() => searchTerms(Model));
            return this;
        }

        private DynamicOption<T> GetLastSelectableOption(string actionName)
        {
            DynamicOption<T> option = _options.LastOrDefault(item => !item.IsSeparator);
            if (option == null)
            {
                throw new InvalidOperationException("No options have been added to " + actionName + ".");
            }

            return option;
        }

        private void AddOption(
            Func<T, string> label,
            Func<T, CancellationToken, Task> action,
            Func<T, ConsoleColor?> color,
            Func<T, bool> isEnabled,
            Func<T, string> disabledReason,
            Func<T, string> description)
        {
            var option = new DynamicOption<T>(
                () => label(Model),
                cancellationToken => action(Model, cancellationToken),
                () => RenderOptions.GetColorOrDefault(color != null ? color(Model) : RenderOptions.OptionColor));

            if (isEnabled != null)
            {
                option.EnableWhen(
                    () => isEnabled(Model),
                    () => disabledReason != null ? disabledReason(Model) : null);
            }

            if (description != null)
            {
                option.Describe(() => description(Model));
            }

            _options.Add(option);
        }

        private static Func<T, ConsoleColor?> StyleColor(Func<T, string> style)
        {
            if (style == null)
            {
                return null;
            }

            return model => ResolveStyle(style(model), RenderOptions.OptionColor);
        }

        private static Func<T, ConsoleColor?> StyleColor(Func<T, Style> style)
        {
            if (style == null)
            {
                return null;
            }

            return model => ResolveStyle(style(model), RenderOptions.OptionColor);
        }

        private static Func<T, ConsoleColor?> StyleColor(Func<T, SpectreColor> color)
        {
            if (color == null)
            {
                return null;
            }

            return model => color(model).ToConsoleColor();
        }

        private static Func<T, ConsoleColor?> StyleColor(Func<T, RgbColor> color)
        {
            if (color == null)
            {
                return null;
            }

            return model => color(model).ToConsoleColor();
        }

        private static Func<T, PromptRenderHelpers.StyledText?> RuleTitleStyle(Func<T, ConsoleColor?> color)
        {
            if (color == null)
            {
                return null;
            }

            return model =>
            {
                ConsoleColor? resolvedColor = color(model);
                return resolvedColor.HasValue
                    ? (PromptRenderHelpers.StyledText?)PromptRenderHelpers.FromColor(resolvedColor.Value)
                    : null;
            };
        }

        private static Func<T, PromptRenderHelpers.StyledText?> RuleTitleStyle(Func<T, string> style)
        {
            return model => PromptRenderHelpers.ResolveStyle(
                ParseStyle(style != null ? style(model) : null),
                RenderOptions.DefaultColor);
        }

        private static Func<T, PromptRenderHelpers.StyledText?> RuleTitleStyle(Func<T, Style> style)
        {
            return model => PromptRenderHelpers.ResolveStyle(
                style != null ? style(model) : null,
                RenderOptions.DefaultColor);
        }

        private static Func<T, PromptRenderHelpers.StyledText?> RuleTitleStyle(Func<T, SpectreColor> color)
        {
            if (color == null)
            {
                return null;
            }

            return model => PromptRenderHelpers.ResolveStyle(new Style(color(model)), RenderOptions.DefaultColor);
        }

        private static Func<T, PromptRenderHelpers.StyledText?> RuleTitleStyle(Func<T, RgbColor> color)
        {
            if (color == null)
            {
                return null;
            }

            return model => PromptRenderHelpers.ResolveStyle(new Style(color(model)), RenderOptions.DefaultColor);
        }

        private static BulletListItem BuildBulletListItem<TItem>(
            TItem source,
            Func<TItem, string> textSelector,
            Func<TItem, IEnumerable<TItem>> childrenSelector,
            Func<TItem, ConsoleColor?> textColorSelector,
            Func<TItem, ConsoleColor?> markerColorSelector)
        {
            if (object.Equals(source, null))
            {
                return null;
            }

            BulletListItem item = BulletListItem.Create(
                textSelector(source),
                textColorSelector != null ? textColorSelector(source) : null,
                markerColorSelector != null ? markerColorSelector(source) : null);
            IEnumerable<TItem> children = childrenSelector(source);
            if (children == null)
            {
                return item;
            }

            foreach (TItem child in children)
            {
                BulletListItem childItem = BuildBulletListItem(child, textSelector, childrenSelector, textColorSelector, markerColorSelector);
                if (childItem != null)
                {
                    item.AddItem(childItem);
                }
            }

            return item;
        }

        private static BulletListItem BuildBulletListItemStyled<TItem>(
            TItem source,
            Func<TItem, string> textSelector,
            Func<TItem, IEnumerable<TItem>> childrenSelector,
            Func<TItem, string> textStyleSelector,
            Func<TItem, string> markerStyleSelector)
        {
            if (object.Equals(source, null))
            {
                return null;
            }

            BulletListItem item = BulletListItem.Create(
                textSelector(source),
                textStyleSelector != null ? textStyleSelector(source) : null,
                markerStyleSelector != null ? markerStyleSelector(source) : null);
            IEnumerable<TItem> children = childrenSelector(source);
            if (children == null)
            {
                return item;
            }

            foreach (TItem child in children)
            {
                BulletListItem childItem = BuildBulletListItemStyled(child, textSelector, childrenSelector, textStyleSelector, markerStyleSelector);
                if (childItem != null)
                {
                    item.AddItem(childItem);
                }
            }

            return item;
        }

        private static TreeNode BuildTreeNode<TNode>(
            TNode source,
            Func<TNode, object> contentSelector,
            Func<TNode, IEnumerable<TNode>> childrenSelector,
            Func<TNode, ConsoleColor?> colorSelector)
        {
            if (object.Equals(source, null))
            {
                return null;
            }

            var node = new TreeNode(
                contentSelector(source),
                CreateTreeNodeScheme(colorSelector != null ? colorSelector(source) : null));
            IEnumerable<TNode> children = childrenSelector(source);
            if (children == null)
            {
                return node;
            }

            foreach (TNode child in children)
            {
                TreeNode childNode = BuildTreeNode(child, contentSelector, childrenSelector, colorSelector);
                if (childNode != null)
                {
                    node.Add(childNode);
                }
            }

            return node;
        }

        private static TreeNode BuildTreeNodeStyled<TNode>(
            TNode source,
            Func<TNode, object> contentSelector,
            Func<TNode, IEnumerable<TNode>> childrenSelector,
            Func<TNode, string> styleSelector)
        {
            if (object.Equals(source, null))
            {
                return null;
            }

            var node = new TreeNode(
                contentSelector(source),
                CreateTreeNodeScheme(styleSelector != null ? styleSelector(source) : null));
            IEnumerable<TNode> children = childrenSelector(source);
            if (children == null)
            {
                return node;
            }

            foreach (TNode child in children)
            {
                TreeNode childNode = BuildTreeNodeStyled(child, contentSelector, childrenSelector, styleSelector);
                if (childNode != null)
                {
                    node.Add(childNode);
                }
            }

            return node;
        }

        private static ConsoulLibrary.Color.ColorScheme? CreateTreeNodeScheme(ConsoleColor? color)
        {
            if (!color.HasValue)
            {
                return null;
            }

            return new ConsoulLibrary.Color.ColorScheme
            {
                Color = color.Value,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ConsoulLibrary.Color.ColorScheme? CreateTreeNodeScheme(string style)
        {
            return string.IsNullOrWhiteSpace(style)
                ? (ConsoulLibrary.Color.ColorScheme?)null
                : Style.Parse(style).ToColorScheme(RenderOptions.DefaultScheme);
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

        private static Style ParseStyle(string style)
        {
            return string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style);
        }

        private static bool IsModelValid(T model, ValidationSummaryOptions validationOptions)
        {
            return ValidationSummaryEntry.Validate(model, validationOptions).IsValid;
        }

        private DynamicView<T> SectionStyled(
            Func<T, string> title,
            Action<T> renderer,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            AddSection(title, renderer, null, titleStyle, alignment);
            return this;
        }

        private DynamicView<T> SectionWhenStyled(
            Func<T, string> title,
            Action<T> renderer,
            Func<T, bool> isVisible,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            AddSection(title, renderer, isVisible, titleStyle, alignment);
            return this;
        }

        private DynamicView<T> SectionRenderableStyled(
            Func<T, string> title,
            Func<T, IConsoulRenderable> renderer,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            return SectionStyled(
                title,
                model =>
                {
                    IConsoulRenderable renderable = renderer(model);
                    if (renderable != null)
                    {
                        Consoul.Render(renderable);
                    }
                },
                titleStyle,
                alignment);
        }

        private DynamicView<T> SectionRenderableWhenStyled(
            Func<T, string> title,
            Func<T, IConsoulRenderable> renderer,
            Func<T, bool> isVisible,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            return SectionWhenStyled(
                title,
                model =>
                {
                    IConsoulRenderable renderable = renderer(model);
                    if (renderable != null)
                    {
                        Consoul.Render(renderable);
                    }
                },
                isVisible,
                titleStyle,
                alignment);
        }

        private DynamicView<T> PropertyListSectionStyledCore(
            string title,
            Func<T, object> modelSelector,
            PropertyListOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment,
            BindingFlags bindingAttr)
        {
            if (modelSelector == null)
            {
                throw new ArgumentNullException(nameof(modelSelector));
            }

            return SectionStyled(model => title, model =>
            {
                object selectedModel = modelSelector(model);
                if (selectedModel == null)
                {
                    return;
                }

                new PropertyListEntry(options)
                    .AddModel(selectedModel, bindingAttr)
                    .Render();
            }, titleStyle, alignment);
        }

        private DynamicView<T> PropertyListSectionWhenStyledCore(
            string title,
            Func<T, object> modelSelector,
            Func<T, bool> isVisible,
            PropertyListOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment,
            BindingFlags bindingAttr)
        {
            if (modelSelector == null)
            {
                throw new ArgumentNullException(nameof(modelSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                object selectedModel = modelSelector(model);
                if (selectedModel == null)
                {
                    return;
                }

                new PropertyListEntry(options)
                    .AddModel(selectedModel, bindingAttr)
                    .Render();
            }, isVisible, titleStyle, alignment);
        }

        private DynamicView<T> TableSectionStyledCore<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            TableRenderOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment,
            Expression<Func<TItem, object>>[] selectors) where TItem : class
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                DynamicTableView<TItem>.Create(items, options, selectors).RenderInline();
            }, titleStyle, alignment);
        }

        private DynamicView<T> TableSectionWhenStyledCore<TItem>(
            string title,
            Func<T, IEnumerable<TItem>> itemsSelector,
            Func<T, bool> isVisible,
            TableRenderOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment,
            Expression<Func<TItem, object>>[] selectors) where TItem : class
        {
            if (itemsSelector == null)
            {
                throw new ArgumentNullException(nameof(itemsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TItem> items = itemsSelector(model) ?? Enumerable.Empty<TItem>();
                DynamicTableView<TItem>.Create(items, options, selectors).RenderInline();
            }, isVisible, titleStyle, alignment);
        }

        private DynamicView<T> MetricsSectionStyledCore(
            string title,
            Func<T, IEnumerable<MetricItem>> metricsSelector,
            MetricsOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (metricsSelector == null)
            {
                throw new ArgumentNullException(nameof(metricsSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<MetricItem> metrics = metricsSelector(model) ?? Enumerable.Empty<MetricItem>();
                new Metrics(metrics, options).Render();
            }, titleStyle, alignment);
        }

        private DynamicView<T> MetricsSectionWhenStyledCore(
            string title,
            Func<T, IEnumerable<MetricItem>> metricsSelector,
            Func<T, bool> isVisible,
            MetricsOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (metricsSelector == null)
            {
                throw new ArgumentNullException(nameof(metricsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<MetricItem> metrics = metricsSelector(model) ?? Enumerable.Empty<MetricItem>();
                new Metrics(metrics, options).Render();
            }, isVisible, titleStyle, alignment);
        }

        private DynamicView<T> MetricsSectionStyledCore<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            Func<TMetric, string> unitSelector,
            Func<TMetric, string> noteSelector,
            Func<TMetric, MetricKind> kindSelector,
            MetricsOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (metricsSelector == null)
            {
                throw new ArgumentNullException(nameof(metricsSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<TMetric> metrics = metricsSelector(model) ?? Enumerable.Empty<TMetric>();
                new Metrics(options)
                    .AddMetrics(metrics, labelSelector, valueSelector, unitSelector, noteSelector, kindSelector)
                    .Render();
            }, titleStyle, alignment);
        }

        private DynamicView<T> MetricsSectionWhenStyledCore<TMetric>(
            string title,
            Func<T, IEnumerable<TMetric>> metricsSelector,
            Func<T, bool> isVisible,
            Func<TMetric, string> labelSelector,
            Func<TMetric, object> valueSelector,
            Func<TMetric, string> unitSelector,
            Func<TMetric, string> noteSelector,
            Func<TMetric, MetricKind> kindSelector,
            MetricsOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (metricsSelector == null)
            {
                throw new ArgumentNullException(nameof(metricsSelector));
            }

            if (isVisible == null)
            {
                throw new ArgumentNullException(nameof(isVisible));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<TMetric> metrics = metricsSelector(model) ?? Enumerable.Empty<TMetric>();
                new Metrics(options)
                    .AddMetrics(metrics, labelSelector, valueSelector, unitSelector, noteSelector, kindSelector)
                .Render();
            }, isVisible, titleStyle, alignment);
        }

        private DynamicView<T> TextBlockSectionStyledCore(
            string title,
            Func<T, string> contentSelector,
            TextBlockOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionStyled(model => title, model =>
            {
                new TextBlockEntry(contentSelector(model), options).Render();
            }, titleStyle, alignment);
        }

        private DynamicView<T> TextBlockSectionWhenStyledCore(
            string title,
            Func<T, string> contentSelector,
            Func<T, bool> isVisible,
            TextBlockOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                new TextBlockEntry(contentSelector(model), options).Render();
            }, isVisible, titleStyle, alignment);
        }

        private DynamicView<T> PanelSectionStyledCore(
            string title,
            Func<T, string> contentSelector,
            PanelOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionStyled(model => title, model =>
            {
                new PanelEntry(contentSelector(model), options).Render();
            }, titleStyle, alignment);
        }

        private DynamicView<T> PanelSectionWhenStyledCore(
            string title,
            Func<T, string> contentSelector,
            Func<T, bool> isVisible,
            PanelOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                new PanelEntry(contentSelector(model), options).Render();
            }, isVisible, titleStyle, alignment);
        }

        private DynamicView<T> CalloutSectionStyledCore(
            string title,
            Func<T, string> messageSelector,
            Func<T, CalloutKind> kindSelector,
            CalloutOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (messageSelector == null)
            {
                throw new ArgumentNullException(nameof(messageSelector));
            }

            return SectionStyled(model => title, model =>
            {
                string message = messageSelector(model);
                CalloutKind kind = kindSelector != null ? kindSelector(model) : CalloutKind.Info;
                new CalloutEntry(message, kind, options).Render();
            }, titleStyle, alignment);
        }

        private DynamicView<T> CalloutSectionWhenStyledCore(
            string title,
            Func<T, string> messageSelector,
            Func<T, bool> isVisible,
            Func<T, CalloutKind> kindSelector,
            CalloutOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (messageSelector == null)
            {
                throw new ArgumentNullException(nameof(messageSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                string message = messageSelector(model);
                CalloutKind kind = kindSelector != null ? kindSelector(model) : CalloutKind.Info;
                new CalloutEntry(message, kind, options).Render();
            }, isVisible, titleStyle, alignment);
        }

        private DynamicView<T> RowsSectionStyledCore(
            string title,
            Func<T, IEnumerable<object>> rowsSelector,
            RowsOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (rowsSelector == null)
            {
                throw new ArgumentNullException(nameof(rowsSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<object> rows = rowsSelector(model) ?? Enumerable.Empty<object>();
                new Rows(options).AddRows(rows).Render();
            }, titleStyle, alignment);
        }

        private DynamicView<T> RowsSectionWhenStyledCore(
            string title,
            Func<T, IEnumerable<object>> rowsSelector,
            Func<T, bool> isVisible,
            RowsOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (rowsSelector == null)
            {
                throw new ArgumentNullException(nameof(rowsSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<object> rows = rowsSelector(model) ?? Enumerable.Empty<object>();
                new Rows(options).AddRows(rows).Render();
            }, isVisible, titleStyle, alignment);
        }

        private DynamicView<T> ColumnsSectionStyledCore(
            string title,
            Func<T, IEnumerable<object>> columnsSelector,
            ColumnsOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (columnsSelector == null)
            {
                throw new ArgumentNullException(nameof(columnsSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<object> columns = columnsSelector(model) ?? Enumerable.Empty<object>();
                new Columns(options).AddColumns(columns).Render();
            }, titleStyle, alignment);
        }

        private DynamicView<T> ColumnsSectionWhenStyledCore(
            string title,
            Func<T, IEnumerable<object>> columnsSelector,
            Func<T, bool> isVisible,
            ColumnsOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (columnsSelector == null)
            {
                throw new ArgumentNullException(nameof(columnsSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<object> columns = columnsSelector(model) ?? Enumerable.Empty<object>();
                new Columns(options).AddColumns(columns).Render();
            }, isVisible, titleStyle, alignment);
        }

        private DynamicView<T> GridSectionStyledCore(
            string title,
            Func<T, IEnumerable<object>> cellsSelector,
            GridOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (cellsSelector == null)
            {
                throw new ArgumentNullException(nameof(cellsSelector));
            }

            return SectionStyled(model => title, model =>
            {
                IEnumerable<object> cells = cellsSelector(model) ?? Enumerable.Empty<object>();
                var grid = new Grid(options);
                foreach (object cell in cells)
                {
                    grid.AddCell(cell);
                }

                grid.Render();
            }, titleStyle, alignment);
        }

        private DynamicView<T> GridSectionWhenStyledCore(
            string title,
            Func<T, IEnumerable<object>> cellsSelector,
            Func<T, bool> isVisible,
            GridOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (cellsSelector == null)
            {
                throw new ArgumentNullException(nameof(cellsSelector));
            }

            return SectionWhenStyled(model => title, model =>
            {
                IEnumerable<object> cells = cellsSelector(model) ?? Enumerable.Empty<object>();
                var grid = new Grid(options);
                foreach (object cell in cells)
                {
                    grid.AddCell(cell);
                }

                grid.Render();
            }, isVisible, titleStyle, alignment);
        }

        private DynamicView<T> LayoutSectionStyledCore(
            string title,
            Action<T, Layout> layoutBuilder,
            LayoutOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (layoutBuilder == null)
            {
                throw new ArgumentNullException(nameof(layoutBuilder));
            }

            return SectionStyled(model => title, model =>
            {
                var layout = new Layout(options)
                {
                    Name = title
                };

                layoutBuilder(model, layout);
                layout.Render();
            }, titleStyle, alignment);
        }

        private DynamicView<T> LayoutSectionWhenStyledCore(
            string title,
            Action<T, Layout> layoutBuilder,
            Func<T, bool> isVisible,
            LayoutOptions options,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            if (layoutBuilder == null)
            {
                throw new ArgumentNullException(nameof(layoutBuilder));
            }

            return SectionWhenStyled(model => title, model =>
            {
                var layout = new Layout(options)
                {
                    Name = title
                };

                layoutBuilder(model, layout);
                layout.Render();
            }, isVisible, titleStyle, alignment);
        }

        private DynamicView<T> ValidationSummarySectionWhenStyledCore(
            Func<T, bool> isVisible,
            ValidationSummaryOptions options,
            string title,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            return SectionWhenStyled(
                model => title,
                model => new ValidationSummaryEntry(model, options).Render(),
                isVisible ?? (model => true),
                titleStyle,
                alignment);
        }

        private void AddSection(
            Func<T, string> title,
            Action<T> renderer,
            Func<T, bool> isVisible,
            Func<T, ConsoleColor?> titleColor,
            RuleTitleAlignment alignment)
        {
            AddSection(title, renderer, isVisible, RuleTitleStyle(titleColor), alignment);
        }

        private void AddSection(
            Func<T, string> title,
            Action<T> renderer,
            Func<T, bool> isVisible,
            Func<T, PromptRenderHelpers.StyledText?> titleStyle,
            RuleTitleAlignment alignment)
        {
            _modelSections.Add(model =>
            {
                if (isVisible != null && !isVisible(model))
                {
                    return;
                }

                string resolvedTitle = title(model);
                if (!string.IsNullOrWhiteSpace(resolvedTitle))
                {
                    RenderSectionRule(
                        resolvedTitle,
                        titleStyle != null ? titleStyle(model) : null,
                        alignment);
                }

                renderer(model);
            });
        }

        private static void RenderSectionRule(
            string title,
            PromptRenderHelpers.StyledText? titleStyle,
            RuleTitleAlignment alignment)
        {
            if (!titleStyle.HasValue)
            {
                Consoul.Rule(title, alignment: alignment);
                return;
            }

            PromptRenderHelpers.StyledText style = titleStyle.Value;
            Consoul.Rule(
                title,
                new RuleOptions
                {
                    Alignment = alignment,
                    RuleScheme = style.Scheme,
                    RuleDecoration = style.Decoration,
                    RuleForegroundRgb = style.ForegroundRgb,
                    RuleBackgroundRgb = style.BackgroundRgb,
                    TitleScheme = style.Scheme,
                    TitleDecoration = style.Decoration,
                    TitleForegroundRgb = style.ForegroundRgb,
                    TitleBackgroundRgb = style.BackgroundRgb
                });
        }

        private void RenderModelContent(T model)
        {
            if (_modelRenderer != null)
            {
                _modelRenderer(model);
            }

            foreach (Action<T> section in _modelSections)
            {
                section(model);
            }
        }

        /// <summary>
        /// Adds an option that navigates to another view type when selected.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Navigate<TView>(string label, ConsoleColor? color = null, bool replace = false) where TView : IView
        {
            return Option(label, (model, cancellationToken) =>
            {
                _navigationContext.RequestNavigation(replace
                    ? NavigationCommand.Replace(typeof(TView))
                    : NavigationCommand.Push(typeof(TView)));
                return Task.CompletedTask;
            }, color);
        }

        /// <summary>
        /// Adds an option that navigates to another view type when selected using style text.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Navigate<TView>(string label, string style, bool replace = false) where TView : IView
        {
            return Navigate<TView>(label, ResolveStyle(style, RenderOptions.OptionColor), replace)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an option that navigates to another view type when selected using a reusable style.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Navigate<TView>(string label, Style style, bool replace = false) where TView : IView
        {
            return Navigate<TView>(label, ResolveStyle(style, RenderOptions.OptionColor), replace)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an option that navigates to another view created by a factory when selected.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Navigate<TView>(string label, Func<TView> viewFactory, ConsoleColor? color = null, bool replace = false) where TView : IView
        {
            if (viewFactory == null)
            {
                throw new ArgumentNullException(nameof(viewFactory));
            }

            return Option(label, (model, cancellationToken) =>
            {
                _navigationContext.RequestNavigation(replace
                    ? NavigationCommand.Replace(() => (IView)viewFactory())
                    : NavigationCommand.Push(() => (IView)viewFactory()));
                return Task.CompletedTask;
            }, color);
        }

        /// <summary>
        /// Adds an option that navigates to another view created by a factory when selected using style text.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Navigate<TView>(string label, Func<TView> viewFactory, string style, bool replace = false) where TView : IView
        {
            return Navigate<TView>(label, viewFactory, ResolveStyle(style, RenderOptions.OptionColor), replace)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an option that navigates to another view created by a factory when selected using a reusable style.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Navigate<TView>(string label, Func<TView> viewFactory, Style style, bool replace = false) where TView : IView
        {
            return Navigate<TView>(label, viewFactory, ResolveStyle(style, RenderOptions.OptionColor), replace)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an option that navigates to another view type when selected using a Spectre-style color facade.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Navigate<TView>(string label, SpectreColor color, bool replace = false) where TView : IView
        {
            return Navigate<TView>(label, (ConsoleColor?)color.ToConsoleColor(), replace)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds an option that navigates to another view type when selected using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Navigate<TView>(string label, RgbColor color, bool replace = false) where TView : IView
        {
            return Navigate<TView>(label, (ConsoleColor?)color.ToConsoleColor(), replace)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an option that navigates to another view created by a factory when selected using a Spectre-style color facade.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Navigate<TView>(string label, Func<TView> viewFactory, SpectreColor color, bool replace = false) where TView : IView
        {
            return Navigate<TView>(label, viewFactory, (ConsoleColor?)color.ToConsoleColor(), replace)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds an option that navigates to another view created by a factory when selected using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> Navigate<TView>(string label, Func<TView> viewFactory, RgbColor color, bool replace = false) where TView : IView
        {
            return Navigate<TView>(label, viewFactory, (ConsoleColor?)color.ToConsoleColor(), replace)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that requests navigation to a typed view when selected.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddNavigationOption<TView>(string label, ConsoleColor? color = null, bool replace = false, string description = null) where TView : IView
        {
            return Navigate<TView>(label, color, replace)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that requests navigation to a typed view when selected using style text.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddNavigationOption<TView>(string label, string style, bool replace = false, string description = null) where TView : IView
        {
            return Navigate<TView>(label, style, replace)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that requests navigation to a typed view when selected using a reusable style.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddNavigationOption<TView>(string label, Style style, bool replace = false, string description = null) where TView : IView
        {
            return Navigate<TView>(label, style, replace)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that requests navigation to a view factory when selected.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddNavigationOption<TView>(
            string label,
            Func<TView> viewFactory,
            ConsoleColor? color = null,
            bool replace = false,
            string description = null)
            where TView : IView
        {
            return Navigate<TView>(label, viewFactory, color, replace)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that requests navigation to a view factory when selected using style text.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddNavigationOption<TView>(
            string label,
            Func<TView> viewFactory,
            string style,
            bool replace = false,
            string description = null)
            where TView : IView
        {
            return Navigate<TView>(label, viewFactory, style, replace)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that requests navigation to a view factory when selected using a reusable style.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddNavigationOption<TView>(
            string label,
            Func<TView> viewFactory,
            Style style,
            bool replace = false,
            string description = null)
            where TView : IView
        {
            return Navigate<TView>(label, viewFactory, style, replace)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that requests navigation to a typed view when selected using a Spectre-style color facade.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddNavigationOption<TView>(string label, SpectreColor color, bool replace = false, string description = null) where TView : IView
        {
            return Navigate<TView>(label, color, replace)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that requests navigation to a typed view when selected using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddNavigationOption<TView>(string label, RgbColor color, bool replace = false, string description = null) where TView : IView
        {
            return Navigate<TView>(label, color, replace)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that requests navigation to a view factory when selected using a Spectre-style color facade.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddNavigationOption<TView>(
            string label,
            Func<TView> viewFactory,
            SpectreColor color,
            bool replace = false,
            string description = null)
            where TView : IView
        {
            return Navigate<TView>(label, viewFactory, color, replace)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that requests navigation to a view factory when selected using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="viewFactory">Factory used to create the destination view.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="replace">When <c>true</c>, replaces this view instead of pushing the new view on the stack.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddNavigationOption<TView>(
            string label,
            Func<TView> viewFactory,
            RgbColor color,
            bool replace = false,
            string description = null)
            where TView : IView
        {
            return Navigate<TView>(label, viewFactory, color, replace)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option that opens the current reference-type model in the built-in object editor.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> EditModel(
            string label = "Edit model",
            ConsoleColor? color = null,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return Option(label, (model, cancellationToken) =>
            {
                if (model == null)
                {
                    throw new InvalidOperationException("DynamicView.EditModel requires a non-null model.");
                }

                if (model.GetType().IsValueType)
                {
                    throw new InvalidOperationException("DynamicView.EditModel requires a reference-type model so edits can be applied to the active view model.");
                }

                var editor = new EditObjectView(model, bindingAttr, enableDocumentEditor);
                return editor.RenderAsync(cancellationToken);
            }, color);
        }

        /// <summary>
        /// Adds an option that opens the current reference-type model in the built-in object editor using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> EditModel(
            string label,
            string style,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return EditModel(label, ResolveStyle(style, RenderOptions.OptionColor), bindingAttr, enableDocumentEditor)
                .StyleLastOption(ParseStyle(style));
        }

        /// <summary>
        /// Adds an option that opens the current reference-type model in the built-in object editor using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> EditModel(
            string label,
            Style style,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return EditModel(label, ResolveStyle(style, RenderOptions.OptionColor), bindingAttr, enableDocumentEditor)
                .StyleLastOption(style);
        }

        /// <summary>
        /// Adds an option that opens the current reference-type model in the built-in object editor using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> EditModel(
            string label,
            SpectreColor color,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return EditModel(label, (ConsoleColor?)color.ToConsoleColor(), bindingAttr, enableDocumentEditor)
                .ColorLastOption(color.ToRgbColor());
        }

        /// <summary>
        /// Adds an option that opens the current reference-type model in the built-in object editor using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> EditModel(
            string label,
            RgbColor color,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return EditModel(label, (ConsoleColor?)color.ToConsoleColor(), bindingAttr, enableDocumentEditor)
                .ColorLastOption(color);
        }

        /// <summary>
        /// Adds an option that opens the current reference-type model in the built-in form editor.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        /// <remarks>
        /// This MVC-style alias delegates to <see cref="EditModel(string, ConsoleColor?, BindingFlags, bool)"/> so form editing
        /// and object editing remain one implementation.
        /// </remarks>
        public DynamicView<T> EditForm(
            string label = "Edit form",
            ConsoleColor? color = null,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return EditModel(label, color, bindingAttr, enableDocumentEditor);
        }

        /// <summary>
        /// Adds an option that opens the current reference-type model in the built-in form editor using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> EditForm(
            string label,
            string style,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return EditModel(label, style, bindingAttr, enableDocumentEditor);
        }

        /// <summary>
        /// Adds an option that opens the current reference-type model in the built-in form editor using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> EditForm(
            string label,
            Style style,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return EditModel(label, style, bindingAttr, enableDocumentEditor);
        }

        /// <summary>
        /// Adds an option that opens the current reference-type model in the built-in form editor using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> EditForm(
            string label,
            SpectreColor color,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return EditModel(label, color, bindingAttr, enableDocumentEditor);
        }

        /// <summary>
        /// Adds an option that opens the current reference-type model in the built-in form editor using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> EditForm(
            string label,
            RgbColor color,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return EditModel(label, color, bindingAttr, enableDocumentEditor);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that opens the current reference-type model in the built-in form editor.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Optional foreground color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddEditFormOption(
            string label = "Edit form",
            ConsoleColor? color = null,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true,
            string description = null)
        {
            return EditForm(label, color, bindingAttr, enableDocumentEditor)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that opens the current reference-type model in the built-in form editor using style text.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddEditFormOption(
            string label,
            string style,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true,
            string description = null)
        {
            return EditForm(label, style, bindingAttr, enableDocumentEditor)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that opens the current reference-type model in the built-in form editor using a reusable style.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddEditFormOption(
            string label,
            Style style,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true,
            string description = null)
        {
            return EditForm(label, style, bindingAttr, enableDocumentEditor)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that opens the current reference-type model in the built-in form editor using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddEditFormOption(
            string label,
            SpectreColor color,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true,
            string description = null)
        {
            return EditForm(label, color, bindingAttr, enableDocumentEditor)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Adds an option using StaticView-style naming that opens the current reference-type model in the built-in form editor using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label shown in the selection prompt.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public DynamicView<T> AddEditFormOption(
            string label,
            RgbColor color,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true,
            string description = null)
        {
            return EditForm(label, color, bindingAttr, enableDocumentEditor)
                .DescribeLastOptionIfProvided(description);
        }

        /// <summary>
        /// Triggers the choice to go back to the previous view, or exits navigation if this is the top view.
        /// </summary>
        public void GoBack()
        {
            _goBackRequested = true;
            _navigationContext.RequestNavigation(NavigationCommand.Pop());
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
        /// Renders the current dynamic view
        /// </summary>
        /// <returns>awaitable task</returns>
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
                string title = _titleBuilder != null ? _titleBuilder(Model) : Title;
                RenderChrome(title);
                RenderModelContent(Model);

                SelectionPrompt prompt = new SelectionPrompt(title, false);
                ConfigurePrompt(prompt);
                List<DynamicOption<T>> selectableOptions = new List<DynamicOption<T>>();
                foreach (DynamicOption<T> option in Options)
                {
                    if (option.IsSeparator)
                    {
                        SelectOption separator = prompt.AddSeparator(option.Entry.SetMessage(), option.ResolveStyle().Scheme.Color);
                        option.ApplyStyleTo(separator);
                        continue;
                    }

                    selectableOptions.Add(option);
                    AddPromptOption(prompt, option);
                }

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
                        try
                        {
                            await selectableOptions[selectedIndex].InvokeAsync(cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception ex2)
                        {
                            RenderViewError(ex2, $"Failed to render '{title}[{selectedIndex}]' view", cancellationToken);
                            actionErrorHandled = true;

                            if (ShouldThrowViewError())
                                throw;
                        }
                        if (OnOptionSelected != null)
                            await OnOptionSelected(selectedIndex);
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
                        Consoul.Write(ex, $"Failed to render '{Title}[{idx}]' view", true, RenderOptions.InvalidColor);
                        if (EffectiveWaitOnError)
                            Consoul.Wait(cancellationToken: cancellationToken);
                    }

                    if (ShouldThrowViewError())
                        throw;
                }
            } while (idx < 0 && !_navigationContext.HasPendingCommand && !GoBackRequested);
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

        private void ShowHelp(string title, ViewHelpOptions options, bool waitForInput, CancellationToken cancellationToken)
        {
            Consoul.ConsoleDriver.Clear();
            CreateHelp(title, options).Render();
            if (waitForInput)
            {
                Consoul.Wait(cancellationToken: cancellationToken);
            }
        }

        private string ResolveHelpTitle(string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                return title;
            }

            string viewTitle = _titleBuilder != null ? _titleBuilder(Model) : Title;
            if (string.IsNullOrWhiteSpace(viewTitle) || viewTitle.IndexOfAny(new[] { '\r', '\n' }) >= 0)
            {
                return "View Help";
            }

            return viewTitle.Trim() + " Help";
        }

        private void RenderChrome(string currentTitle)
        {
            if (Chrome == null)
            {
                return;
            }

            if (Chrome.ShowBreadcrumbs)
            {
                var breadcrumbItems = ResolveBreadcrumbTrail(currentTitle);
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

        private IList<string> ResolveBreadcrumbTrail(string currentTitle)
        {
            if (_navigationContext.BreadcrumbTrail.Count > 0)
            {
                return _navigationContext.BreadcrumbTrail.ToList();
            }

            return string.IsNullOrWhiteSpace(currentTitle)
                ? new List<string>()
                : new List<string> { currentTitle };
        }

        private ShortcutBarEntry BuildShortcutBar()
        {
            var shortcutBar = new ShortcutBarEntry(Chrome.ShortcutBarOptions);
            foreach (DynamicOption<T> option in Options)
            {
                if (option.IsSeparator || string.IsNullOrWhiteSpace(option.Shortcut))
                {
                    continue;
                }

                ShortcutItem item = shortcutBar.Add(option.Shortcut, option.Entry.SetMessage(), !option.IsEnabled);
                option.ApplyStyleTo(item);
            }

            if (Chrome.ShowGoBackShortcut && !string.IsNullOrWhiteSpace(Chrome.GoBackShortcut))
            {
                shortcutBar.Add(Chrome.GoBackShortcut, string.IsNullOrWhiteSpace(Chrome.GoBackDescription) ? _goBackMessage : Chrome.GoBackDescription);
            }

            return shortcutBar;
        }

        private async Task<PromptResult> ShowCommandPaletteAsync(
            string message,
            bool clear,
            DynamicOption<T> excludedOption,
            CancellationToken cancellationToken)
        {
            SelectionPrompt prompt = new SelectionPrompt(message, clear)
            {
                FilterEnabled = true,
                KeyboardNavigationEnabled = PromptKeyboardNavigationEnabled,
                PageSize = PromptPageSize,
                ShowOptionDescriptions = true
            };

            List<DynamicOption<T>> selectableOptions = new List<DynamicOption<T>>();
            foreach (DynamicOption<T> option in Options)
            {
                if (ReferenceEquals(option, excludedOption))
                {
                    continue;
                }

                if (option.IsSeparator)
                {
                    SelectOption separator = prompt.AddSeparator(option.Entry.SetMessage(), option.ResolveStyle().Scheme.Color);
                    option.ApplyStyleTo(separator);
                    continue;
                }

                selectableOptions.Add(option);
                AddPromptOption(prompt, option);
            }

            PromptResult result = prompt.Render(cancellationToken);
            if (!result.HasSelection || result.IsCanceled || cancellationToken.IsCancellationRequested)
            {
                return result;
            }

            if (result.Index >= 0 && result.Index < selectableOptions.Count)
            {
                await selectableOptions[result.Index].InvokeAsync(cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        private static void AddPromptOption(
            SelectionPrompt prompt,
            DynamicOption<T> option)
        {
            string label = option.Entry.SetMessage();
            string description = option.Description;
            string shortcut = option.Shortcut;
            bool isEnabled = option.IsEnabled;
            string disabledReason = option.DisabledReason;
            ConsoleColor color = option.ResolveStyle().Scheme.Color;
            int index;
            if (!isEnabled)
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    if (string.IsNullOrWhiteSpace(shortcut))
                    {
                        index = prompt.AddDisabled(label, disabledReason, color);
                    }
                    else
                    {
                        index = prompt.AddDisabledShortcut(label, shortcut, disabledReason, color);
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(shortcut))
                    {
                        index = prompt.AddDisabledDescribed(label, description, disabledReason, color);
                    }
                    else
                    {
                        index = prompt.AddDisabledDescribedShortcut(label, description, shortcut, disabledReason, color);
                    }
                }

                option.ApplyStyleTo(FindPromptOption(prompt, index), disabled: true);
                prompt.AddSearchTerms(index, option.SearchTerms);
                return;
            }

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

                option.ApplyStyleTo(FindPromptOption(prompt, index));
                prompt.AddSearchTerms(index, option.SearchTerms);
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

            option.ApplyStyleTo(FindPromptOption(prompt, index));
            prompt.AddSearchTerms(index, option.SearchTerms);
        }

        private static SelectOption FindPromptOption(SelectionPrompt prompt, int index)
        {
            return prompt.Options.First(item => !item.IsSeparator && item.Index == index);
        }

        private static IEnumerable<string> NormalizeSearchTerms(object searchTerms)
        {
            if (searchTerms == null)
            {
                return Enumerable.Empty<string>();
            }

            if (searchTerms is string text)
            {
                return SplitSearchTerms(text);
            }

            if (searchTerms is IEnumerable<string> terms)
            {
                return terms
                    .Where(term => !string.IsNullOrWhiteSpace(term))
                    .Select(term => term.Trim());
            }

            string value = searchTerms.ToString();
            return string.IsNullOrWhiteSpace(value)
                ? Enumerable.Empty<string>()
                : new[] { value.Trim() };
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

        /// <summary>
        /// Renders the current dynamic view
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
    }
}
