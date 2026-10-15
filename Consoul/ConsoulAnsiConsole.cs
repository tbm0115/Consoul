using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides an injectable implementation of <see cref="IAnsiConsole"/> over a Consoul console driver.
    /// </summary>
    /// <remarks>
    /// Console operations use shared Consoul driver state. Serialize operations across instances, including awaited prompts.
    /// </remarks>
    public sealed class ConsoulAnsiConsole : IAnsiConsole
    {
        private static readonly IAnsiConsoleCursor DefaultCursor = new ConsoulAnsiConsoleCursor();
        private readonly IConsoleDriver _driver;
        private readonly IAnsiConsoleCursor _cursor;
        private RecordingConsoleDriver _recordingDriver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoulAnsiConsole"/> class.
        /// </summary>
        public ConsoulAnsiConsole()
        {
            _cursor = DefaultCursor;
        }

        internal ConsoulAnsiConsole(IConsoleDriver driver)
        {
            _driver = driver;
            _cursor = driver == null ? DefaultCursor : new ScopedAnsiConsoleCursor(() => ActiveDriver);
        }

        private bool HasScopedDriver => _driver != null;

        internal bool UsesScopedDriver => HasScopedDriver;

        private IConsoleDriver ActiveDriver => _recordingDriver ?? _driver;

        /// <summary>
        /// Gets a snapshot of the active terminal profile.
        /// </summary>
        public TerminalProfile Profile => HasScopedDriver ? TerminalProfile.FromDriver(ActiveDriver) : Consoul.Terminal;

        /// <summary>
        /// Gets cursor operations for the active console.
        /// </summary>
        public IAnsiConsoleCursor Cursor => _cursor;

        /// <summary>
        /// Starts recording output written through the active console.
        /// </summary>
        public void Record()
        {
            if (HasScopedDriver)
            {
                if (_recordingDriver == null)
                {
                    _recordingDriver = new RecordingConsoleDriver(_driver);
                }

                return;
            }

            Invoke(() => AnsiConsole.Record());
        }

        /// <summary>
        /// Exports recorded console output as plain text.
        /// </summary>
        /// <returns>The recorded console output.</returns>
        public string ExportText()
        {
            if (HasScopedDriver)
            {
                return GetRecordingDriver().GetText();
            }

            return Invoke(() => AnsiConsole.ExportText());
        }

        /// <summary>
        /// Exports recorded console output as escaped HTML.
        /// </summary>
        /// <returns>The recorded console output wrapped in a preformatted HTML block.</returns>
        public string ExportHtml()
        {
            if (HasScopedDriver)
            {
                return GetRecordingDriver().GetHtml();
            }

            return Invoke(() => AnsiConsole.ExportHtml());
        }

        private RecordingConsoleDriver GetRecordingDriver()
        {
            if (_recordingDriver == null)
            {
                throw new InvalidOperationException("Cannot export output since recording has not been started.");
            }

            return _recordingDriver;
        }

        private void Invoke(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!HasScopedDriver)
            {
                action();
                return;
            }

            using (Consoul.UseConsoleDriver(ActiveDriver))
            using (AnsiConsole.SuppressDispatch())
            {
                action();
            }
        }

        private T Invoke<T>(Func<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!HasScopedDriver)
            {
                return action();
            }

            using (Consoul.UseConsoleDriver(ActiveDriver))
            using (AnsiConsole.SuppressDispatch())
            {
                return action();
            }
        }

        private Task InvokeAsync(Func<Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!HasScopedDriver)
            {
                return action();
            }

            return InvokeAsyncCore(action);
        }

        private async Task InvokeAsyncCore(Func<Task> action)
        {
            using (Consoul.UseConsoleDriver(ActiveDriver))
            using (AnsiConsole.SuppressDispatch())
            {
                await action().ConfigureAwait(false);
            }
        }

        private Task<T> InvokeAsync<T>(Func<Task<T>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!HasScopedDriver)
            {
                return action();
            }

            return InvokeAsyncCore(action);
        }

        private async Task<T> InvokeAsyncCore<T>(Func<Task<T>> action)
        {
            using (Consoul.UseConsoleDriver(ActiveDriver))
            using (AnsiConsole.SuppressDispatch())
            {
                return await action().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Clears the active console driver.
        /// </summary>
        public void Clear()
        {
            Invoke(() => AnsiConsole.Clear());
        }

        /// <summary>
        /// Clears the active console driver and accepts Spectre-style cursor-home intent.
        /// </summary>
        /// <param name="home">Whether the cursor should be moved to the home position after clearing.</param>
        public void Clear(bool home)
        {
            Invoke(() => AnsiConsole.Clear(home));
        }

        /// <summary>
        /// Writes plain text without appending a line break.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(string text)
        {
            Invoke(() => AnsiConsole.Write(text));
        }

        /// <summary>
        /// Writes ANSI or VT escape sequences through a caller-provided action.
        /// </summary>
        /// <param name="action">The action that writes the escape sequence output.</param>
        public void WriteAnsi(Action action)
        {
            Invoke(() => AnsiConsole.WriteAnsi(action));
        }

        /// <summary>
        /// Renders a Consoul renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component to render.</param>
        public void Write(IConsoulRenderable renderable)
        {
            Invoke(() => AnsiConsole.Write(renderable));
        }

        /// <summary>
        /// Writes a blank line.
        /// </summary>
        public void WriteLine()
        {
            Invoke(() => AnsiConsole.WriteLine());
        }

        /// <summary>
        /// Writes plain text followed by a line break.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void WriteLine(string text)
        {
            Invoke(() => AnsiConsole.WriteLine(text));
        }

        /// <summary>
        /// Writes lightweight markup without appending a line break.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        public void Markup(string markup)
        {
            Invoke(() => AnsiConsole.Markup(markup));
        }

        /// <summary>
        /// Writes lightweight markup followed by a line break.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        public void MarkupLine(string markup)
        {
            Invoke(() => AnsiConsole.MarkupLine(markup));
        }

        /// <summary>
        /// Writes a blank markup line.
        /// </summary>
        public void MarkupLine()
        {
            Invoke(() => AnsiConsole.MarkupLine());
        }

        /// <summary>
        /// Writes interpolated markup without appending a line break, escaping interpolated values.
        /// </summary>
        /// <param name="markup">The interpolated markup text to render.</param>
        public void MarkupInterpolated(FormattableString markup)
        {
            Invoke(() => AnsiConsole.MarkupInterpolated(markup));
        }

        /// <summary>
        /// Writes interpolated markup followed by a line break, escaping interpolated values.
        /// </summary>
        /// <param name="markup">The interpolated markup text to render.</param>
        public void MarkupLineInterpolated(FormattableString markup)
        {
            Invoke(() => AnsiConsole.MarkupLineInterpolated(markup));
        }

        /// <summary>
        /// Escapes square brackets in text that will be inserted into markup.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>Markup-safe text.</returns>
        public string EscapeMarkup(string value)
        {
            return Invoke(() => AnsiConsole.EscapeMarkup(value));
        }

        /// <summary>
        /// Removes recognized markup tags from text.
        /// </summary>
        /// <param name="value">Markup text to strip.</param>
        /// <returns>Plain text with recognized markup tags removed.</returns>
        public string RemoveMarkup(string value)
        {
            return Invoke(() => AnsiConsole.RemoveMarkup(value));
        }

        /// <summary>
        /// Shows a prompt abstraction and returns its result.
        /// </summary>
        /// <typeparam name="TResult">The prompt result type.</typeparam>
        /// <param name="prompt">The prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The prompt result.</returns>
        public TResult Prompt<TResult>(IPrompt<TResult> prompt, CancellationToken cancellationToken = default)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            return Invoke(() => prompt.ShowAsync(this, cancellationToken).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Shows a prompt abstraction asynchronously and returns its result.
        /// </summary>
        /// <typeparam name="TResult">The prompt result type.</typeparam>
        /// <param name="prompt">The prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the prompt result.</returns>
        public Task<TResult> PromptAsync<TResult>(IPrompt<TResult> prompt, CancellationToken cancellationToken = default)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            return InvokeAsync(() => prompt.ShowAsync(this, cancellationToken));
        }

        /// <summary>
        /// Renders a selection prompt and returns its explicit selection result.
        /// </summary>
        /// <param name="prompt">The selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The selection result.</returns>
        public PromptResult Prompt(SelectionPrompt prompt, CancellationToken cancellationToken = default)
        {
            return Invoke(() => AnsiConsole.Prompt(prompt, cancellationToken));
        }

        /// <summary>
        /// Renders a typed selection prompt and returns the selected item result.
        /// </summary>
        /// <typeparam name="T">The option item type.</typeparam>
        /// <param name="prompt">The typed selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed selection result.</returns>
        public SelectionResult<T> Prompt<T>(SelectionPrompt<T> prompt, CancellationToken cancellationToken = default)
        {
            return Invoke(() => AnsiConsole.Prompt(prompt, cancellationToken));
        }

        /// <summary>
        /// Renders a typed text prompt and returns its parsed value result.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The typed text prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed text prompt result.</returns>
        public TextPromptResult<T> Prompt<T>(TextPrompt<T> prompt, CancellationToken cancellationToken = default)
        {
            return Invoke(() => AnsiConsole.Prompt(prompt, cancellationToken));
        }

        /// <summary>
        /// Renders a confirmation prompt and returns its explicit confirmation result.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The confirmation result.</returns>
        public ConfirmResult Prompt(ConfirmPrompt prompt, CancellationToken cancellationToken = default)
        {
            return Invoke(() => AnsiConsole.Prompt(prompt, cancellationToken));
        }

        /// <summary>
        /// Renders a secret prompt and returns its explicit secret result.
        /// </summary>
        /// <param name="prompt">The secret prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The secret prompt result.</returns>
        public SecretResult Prompt(SecretPrompt prompt, CancellationToken cancellationToken = default)
        {
            return Invoke(() => AnsiConsole.Prompt(prompt, cancellationToken));
        }

        /// <summary>
        /// Renders a multi-selection prompt and returns selected indexes.
        /// </summary>
        /// <param name="prompt">The multi-selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The multi-selection result.</returns>
        public MultiSelectionResult Prompt(MultiSelectionPrompt prompt, CancellationToken cancellationToken = default)
        {
            return Invoke(() => AnsiConsole.Prompt(prompt, cancellationToken));
        }

        /// <summary>
        /// Renders a typed multi-selection prompt and returns selected items.
        /// </summary>
        /// <typeparam name="T">The option item type.</typeparam>
        /// <param name="prompt">The typed multi-selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed multi-selection result.</returns>
        public MultiSelectionResult<T> Prompt<T>(MultiSelectionPrompt<T> prompt, CancellationToken cancellationToken = default)
        {
            return Invoke(() => AnsiConsole.Prompt(prompt, cancellationToken));
        }

        /// <summary>
        /// Creates and renders a typed text prompt.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed text prompt result.</returns>
        public TextPromptResult<T> Ask<T>(string message, CancellationToken cancellationToken = default)
        {
            return Invoke(() => AnsiConsole.Ask<T>(message, cancellationToken));
        }

        /// <summary>
        /// Creates and renders a typed text prompt with a default value.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="defaultValue">The default value used when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed text prompt result.</returns>
        public TextPromptResult<T> Ask<T>(string message, T defaultValue, CancellationToken cancellationToken = default)
        {
            return Invoke(() => AnsiConsole.Ask(message, defaultValue, cancellationToken));
        }

        /// <summary>
        /// Creates and renders a typed text prompt asynchronously.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the typed text prompt result.</returns>
        public Task<TextPromptResult<T>> AskAsync<T>(string message, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(() => AnsiConsole.AskAsync<T>(message, cancellationToken));
        }

        /// <summary>
        /// Creates and renders a typed text prompt with a default value asynchronously.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="defaultValue">The default value used when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the typed text prompt result.</returns>
        public Task<TextPromptResult<T>> AskAsync<T>(string message, T defaultValue, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(() => AnsiConsole.AskAsync(message, defaultValue, cancellationToken));
        }

        /// <summary>
        /// Renders a yes/no confirmation prompt.
        /// </summary>
        /// <param name="message">The confirmation message.</param>
        /// <param name="defaultValue">Optional default confirmation value.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The confirmation result.</returns>
        public ConfirmResult Confirm(string message, bool? defaultValue = null, CancellationToken cancellationToken = default)
        {
            return Invoke(() => AnsiConsole.Confirm(message, defaultValue, cancellationToken));
        }

        /// <summary>
        /// Renders a yes/no confirmation prompt asynchronously.
        /// </summary>
        /// <param name="message">The confirmation message.</param>
        /// <param name="defaultValue">Optional default confirmation value.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the confirmation result.</returns>
        public Task<ConfirmResult> ConfirmAsync(string message, bool? defaultValue = null, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(() => AnsiConsole.ConfirmAsync(message, defaultValue, cancellationToken));
        }

        /// <summary>
        /// Writes an exception using Consoul's exception renderer.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="message">Optional contextual message.</param>
        /// <param name="options">Optional exception rendering options.</param>
        public void WriteException(Exception exception, string message = null, ExceptionRenderOptions options = null)
        {
            Invoke(() => AnsiConsole.WriteException(exception, message, options));
        }

        /// <summary>
        /// Writes an exception using migration-friendly formatting flags.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="format">Formatting flags for the exception panel.</param>
        public void WriteException(Exception exception, ExceptionFormats format)
        {
            Invoke(() => AnsiConsole.WriteException(exception, format));
        }

        /// <summary>
        /// Writes an exception using Spectre-style settings.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="settings">Settings used to adapt the exception panel.</param>
        public void WriteException(Exception exception, ExceptionSettings settings)
        {
            Invoke(() => AnsiConsole.WriteException(exception, settings));
        }

        /// <summary>
        /// Renders a horizontal rule.
        /// </summary>
        /// <param name="title">Optional rule title.</param>
        /// <param name="options">Optional rule rendering options.</param>
        public void Rule(string title = null, RuleOptions options = null)
        {
            Invoke(() => AnsiConsole.Rule(title, options));
        }

        /// <summary>
        /// Renders content in a panel.
        /// </summary>
        /// <param name="content">The panel content.</param>
        /// <param name="options">Optional panel rendering options.</param>
        public void Panel(string content, PanelOptions options = null)
        {
            Invoke(() => AnsiConsole.Panel(content, options));
        }

        /// <summary>
        /// Captures and renders a component in a panel.
        /// </summary>
        /// <param name="renderable">The component to render inside the panel.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the component.</param>
        /// <param name="captureHeight">Optional virtual console height used while capturing the component.</param>
        public void Panel(IConsoulRenderable renderable, PanelOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            Invoke(() => AnsiConsole.Panel(renderable, options, captureWidth, captureHeight));
        }

        /// <summary>
        /// Captures and renders a delegate in a panel.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the action.</param>
        /// <param name="captureHeight">Optional virtual console height used while capturing the action.</param>
        public void Panel(Action render, PanelOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            Invoke(() => AnsiConsole.Panel(render, options, captureWidth, captureHeight));
        }

        /// <summary>
        /// Renders a model-backed table.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The table items.</param>
        /// <param name="selectors">Optional property selectors for visible columns.</param>
        /// <returns>The rendered dynamic table view.</returns>
        public DynamicTableView<T> Table<T>(IEnumerable<T> items, params Expression<Func<T, object>>[] selectors) where T : class
        {
            return Invoke(() => AnsiConsole.Table(items, selectors));
        }

        /// <summary>
        /// Renders a model-backed table with explicit rendering options.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The table items.</param>
        /// <param name="options">The table rendering options.</param>
        /// <param name="selectors">Optional property selectors for visible columns.</param>
        /// <returns>The rendered dynamic table view.</returns>
        public DynamicTableView<T> Table<T>(IEnumerable<T> items, TableRenderOptions options, params Expression<Func<T, object>>[] selectors) where T : class
        {
            return Invoke(() => AnsiConsole.Table(items, options, selectors));
        }

        /// <summary>
        /// Renders a tree.
        /// </summary>
        /// <param name="root">The root node to render.</param>
        /// <param name="options">Optional tree rendering options.</param>
        public void Tree(TreeNode root, TreeOptions options = null)
        {
            Invoke(() => AnsiConsole.Tree(root, options));
        }

        /// <summary>
        /// Renders a tree configured through a root-node callback.
        /// </summary>
        /// <param name="rootText">The root node text.</param>
        /// <param name="configure">Optional callback used to add child nodes.</param>
        /// <param name="options">Optional tree rendering options.</param>
        public void Tree(string rootText, Action<TreeNode> configure = null, TreeOptions options = null)
        {
            Invoke(() => AnsiConsole.Tree(rootText, configure, options));
        }

        /// <summary>
        /// Renders text columns.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        public void Columns(params string[] columns)
        {
            Invoke(() => AnsiConsole.Columns(columns));
        }

        /// <summary>
        /// Renders columns from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        public void Columns(params object[] columns)
        {
            Invoke(() => AnsiConsole.Columns(columns));
        }

        /// <summary>
        /// Renders text columns with explicit options.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        /// <param name="options">Optional column rendering options.</param>
        public void Columns(IEnumerable<string> columns, ColumnsOptions options = null)
        {
            Invoke(() => AnsiConsole.Columns(columns, options));
        }

        /// <summary>
        /// Renders columns from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        /// <param name="options">Optional column rendering options.</param>
        public void Columns(IEnumerable<object> columns, ColumnsOptions options = null)
        {
            Invoke(() => AnsiConsole.Columns(columns, options));
        }

        /// <summary>
        /// Renders responsive side-by-side text columns with explicit options.
        /// </summary>
        /// <param name="options">Optional column rendering options.</param>
        /// <param name="columns">The column values to render.</param>
        public void Columns(ColumnsOptions options, params string[] columns)
        {
            Invoke(() => AnsiConsole.Columns(options, columns));
        }

        /// <summary>
        /// Renders responsive side-by-side columns from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Optional column rendering options.</param>
        /// <param name="columns">The column values to render.</param>
        public void Columns(ColumnsOptions options, params object[] columns)
        {
            Invoke(() => AnsiConsole.Columns(options, columns));
        }

        /// <summary>
        /// Builds and renders responsive side-by-side text columns.
        /// </summary>
        /// <param name="configure">Action used to add columns before rendering.</param>
        /// <param name="options">Optional column rendering options.</param>
        public void Columns(Action<ColumnsEntry> configure, ColumnsOptions options = null)
        {
            Invoke(() => AnsiConsole.Columns(configure, options));
        }

        /// <summary>
        /// Writes a compact single-line badge.
        /// </summary>
        /// <param name="label">The badge label to render.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional badge rendering options.</param>
        /// <param name="writeLine">Whether to append a line break after the badge.</param>
        public void Badge(string label, BadgeKind kind = BadgeKind.Default, BadgeOptions options = null, bool writeLine = true)
        {
            Invoke(() => AnsiConsole.Badge(label, kind, options, writeLine));
        }

        /// <summary>
        /// Creates compact badge text without writing to the console.
        /// </summary>
        /// <param name="label">The badge label.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional badge rendering options.</param>
        /// <param name="maxWidth">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded badge string.</returns>
        public string BadgeText(string label, BadgeKind kind = BadgeKind.Default, BadgeOptions options = null, int? maxWidth = null)
        {
            return Invoke(() => AnsiConsole.BadgeText(label, kind, options, maxWidth));
        }

        /// <summary>
        /// Renders a responsive headline.
        /// </summary>
        /// <param name="text">The headline text to render.</param>
        /// <param name="options">Headline rendering options.</param>
        public void Headline(string text, HeadlineOptions options)
        {
            Invoke(() => AnsiConsole.Headline(text, options));
        }

        /// <summary>
        /// Renders a responsive headline.
        /// </summary>
        /// <param name="text">The headline text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="style">The headline rendering style.</param>
        public void Headline(string text, int? width = null, ConsoleColor? color = null, HeadlineStyle style = HeadlineStyle.Block)
        {
            Invoke(() => AnsiConsole.Headline(text, width, color, style));
        }

        /// <summary>
        /// Renders large ASCII text.
        /// </summary>
        /// <param name="text">The text to render as large glyphs.</param>
        /// <param name="options">Figlet rendering options.</param>
        public void FigletText(string text, FigletTextOptions options)
        {
            Invoke(() => AnsiConsole.FigletText(text, options));
        }

        /// <summary>
        /// Renders large ASCII text.
        /// </summary>
        /// <param name="text">The text to render as large glyphs.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="alignment">Alignment used for rendered lines.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <param name="character">The character used to fill glyphs.</param>
        public void FigletText(
            string text,
            int? width = null,
            TextBlockAlignment alignment = TextBlockAlignment.Center,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null,
            char character = '#')
        {
            Invoke(() => AnsiConsole.FigletText(text, width, alignment, color, backgroundColor, character));
        }

        /// <summary>
        /// Renders a responsive month calendar.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        /// <param name="options">Calendar rendering options.</param>
        public void Calendar(DateTime month, CalendarOptions options = null)
        {
            Invoke(() => AnsiConsole.Calendar(month, options));
        }

        /// <summary>
        /// Renders a responsive month calendar.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="options">Calendar rendering options.</param>
        public void Calendar(int year, int month, CalendarOptions options = null)
        {
            Invoke(() => AnsiConsole.Calendar(year, month, options));
        }

        /// <summary>
        /// Builds and renders a responsive month calendar.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        /// <param name="configure">Action used to mark days before rendering.</param>
        /// <param name="options">Calendar rendering options.</param>
        public void Calendar(DateTime month, Action<CalendarEntry> configure, CalendarOptions options = null)
        {
            Invoke(() => AnsiConsole.Calendar(month, configure, options));
        }

        /// <summary>
        /// Builds and renders a responsive month calendar.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="configure">Action used to mark days before rendering.</param>
        /// <param name="options">Calendar rendering options.</param>
        public void Calendar(int year, int month, Action<CalendarEntry> configure, CalendarOptions options = null)
        {
            Invoke(() => AnsiConsole.Calendar(year, month, configure, options));
        }

        /// <summary>
        /// Renders a responsive intent-colored callout.
        /// </summary>
        /// <param name="message">The callout message.</param>
        /// <param name="kind">The callout visual intent.</param>
        /// <param name="options">Optional callout rendering options.</param>
        public void Callout(string message, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null)
        {
            Invoke(() => AnsiConsole.Callout(message, kind, options));
        }

        /// <summary>
        /// Captures a render action and renders the captured output inside a callout.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="kind">The callout visual intent.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public void Callout(Action render, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            Invoke(() => AnsiConsole.Callout(render, kind, options, captureWidth, captureHeight));
        }

        /// <summary>
        /// Captures a renderable and renders the captured output inside a callout.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="kind">The callout visual intent.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public void Callout(IConsoulRenderable renderable, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            Invoke(() => AnsiConsole.Callout(renderable, kind, options, captureWidth, captureHeight));
        }

        /// <summary>
        /// Validates a model and renders a validation summary when issues are present.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <returns>The validation result for the model.</returns>
        public ValidationSummaryResult ValidationSummary(object model, ValidationSummaryOptions options = null)
        {
            return Invoke(() => AnsiConsole.ValidationSummary(model, options));
        }

        /// <summary>
        /// Validates a model without rendering output.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <returns>The validation result for the model.</returns>
        public ValidationSummaryResult ValidateModel(object model, ValidationSummaryOptions options = null)
        {
            return Invoke(() => AnsiConsole.ValidateModel(model, options));
        }

        /// <summary>
        /// Renders a responsive timeline.
        /// </summary>
        /// <param name="items">The timeline items to render.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        public void Timeline(IEnumerable<TimelineItem> items, TimelineOptions options = null)
        {
            Invoke(() => AnsiConsole.Timeline(items, options));
        }

        /// <summary>
        /// Renders a responsive timeline.
        /// </summary>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="items">The timeline items to render.</param>
        public void Timeline(TimelineOptions options, params TimelineItem[] items)
        {
            Invoke(() => AnsiConsole.Timeline(options, items));
        }

        /// <summary>
        /// Configures and renders a responsive timeline.
        /// </summary>
        /// <param name="configure">Action used to add timeline items.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        public void Timeline(Action<TimelineEntry> configure, TimelineOptions options = null)
        {
            Invoke(() => AnsiConsole.Timeline(configure, options));
        }

        /// <summary>
        /// Renders a responsive bullet or numbered list.
        /// </summary>
        /// <param name="items">The list items to render.</param>
        /// <param name="options">Optional list rendering options.</param>
        public void BulletList(IEnumerable<BulletListItem> items, BulletListOptions options = null)
        {
            Invoke(() => AnsiConsole.BulletList(items, options));
        }

        /// <summary>
        /// Renders a responsive bullet or numbered list.
        /// </summary>
        /// <param name="options">Optional list rendering options.</param>
        /// <param name="items">The list items to render.</param>
        public void BulletList(BulletListOptions options, params BulletListItem[] items)
        {
            Invoke(() => AnsiConsole.BulletList(options, items));
        }

        /// <summary>
        /// Configures and renders a responsive bullet or numbered list.
        /// </summary>
        /// <param name="configure">Action used to add list items.</param>
        /// <param name="options">Optional list rendering options.</param>
        public void BulletList(Action<BulletListEntry> configure, BulletListOptions options = null)
        {
            Invoke(() => AnsiConsole.BulletList(configure, options));
        }

        /// <summary>
        /// Renders responsive grid cells.
        /// </summary>
        /// <param name="cells">The cell contents to render.</param>
        public void Grid(params string[] cells)
        {
            Invoke(() => AnsiConsole.Grid(cells));
        }

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="cells">The cell values to render.</param>
        public void Grid(params object[] cells)
        {
            Invoke(() => AnsiConsole.Grid(cells));
        }

        /// <summary>
        /// Renders responsive grid cells.
        /// </summary>
        /// <param name="cells">The cell contents to render.</param>
        /// <param name="options">Optional grid rendering options.</param>
        public void Grid(IEnumerable<string> cells, GridOptions options = null)
        {
            Invoke(() => AnsiConsole.Grid(cells, options));
        }

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="cells">The cell values to render.</param>
        /// <param name="options">Optional grid rendering options.</param>
        public void Grid(IEnumerable<object> cells, GridOptions options = null)
        {
            Invoke(() => AnsiConsole.Grid(cells, options));
        }

        /// <summary>
        /// Renders responsive grid cells with explicit options.
        /// </summary>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="cells">The cell contents to render.</param>
        public void Grid(GridOptions options, params string[] cells)
        {
            Invoke(() => AnsiConsole.Grid(options, cells));
        }

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="cells">The cell values to render.</param>
        public void Grid(GridOptions options, params object[] cells)
        {
            Invoke(() => AnsiConsole.Grid(options, cells));
        }

        /// <summary>
        /// Configures and renders responsive grid cells.
        /// </summary>
        /// <param name="configure">Action used to add cells.</param>
        /// <param name="options">Optional grid rendering options.</param>
        public void Grid(Action<GridEntry> configure, GridOptions options = null)
        {
            Invoke(() => AnsiConsole.Grid(configure, options));
        }

        /// <summary>
        /// Builds and renders a small ASCII canvas.
        /// </summary>
        /// <param name="width">The logical canvas width.</param>
        /// <param name="height">The logical canvas height.</param>
        /// <param name="configure">Action used to draw on the canvas.</param>
        /// <param name="options">Optional canvas rendering options.</param>
        public void Canvas(int width, int height, Action<CanvasEntry> configure, CanvasOptions options = null)
        {
            Invoke(() => AnsiConsole.Canvas(width, height, configure, options));
        }

        /// <summary>
        /// Renders an existing ASCII canvas.
        /// </summary>
        /// <param name="canvas">The canvas to render.</param>
        public void Canvas(CanvasEntry canvas)
        {
            Invoke(() => AnsiConsole.Canvas(canvas));
        }

        /// <summary>
        /// Renders an existing dependency-free terminal canvas image.
        /// </summary>
        /// <param name="image">The image to render.</param>
        public void CanvasImage(CanvasImageEntry image)
        {
            Invoke(() => AnsiConsole.CanvasImage(image));
        }

        /// <summary>
        /// Renders a dependency-free terminal canvas image from a row-major pixel matrix.
        /// </summary>
        /// <param name="pixels">The source pixels, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional image rendering options.</param>
        public void CanvasImage(CanvasImagePixel[,] pixels, CanvasImageOptions options = null)
        {
            Invoke(() => AnsiConsole.CanvasImage(pixels, options));
        }

        /// <summary>
        /// Renders a dependency-free terminal canvas image from a row-major console color matrix.
        /// </summary>
        /// <param name="pixels">The source colors, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional image rendering options.</param>
        public void CanvasImage(ConsoleColor[,] pixels, CanvasImageOptions options = null)
        {
            Invoke(() => AnsiConsole.CanvasImage(pixels, options));
        }

        /// <summary>
        /// Renders vertically stacked rows.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        public void Rows(params string[] rows)
        {
            Invoke(() => AnsiConsole.Rows(rows));
        }

        /// <summary>
        /// Renders vertically stacked rows from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        public void Rows(params object[] rows)
        {
            Invoke(() => AnsiConsole.Rows(rows));
        }

        /// <summary>
        /// Renders vertically stacked rows.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        /// <param name="options">Optional row rendering options.</param>
        public void Rows(IEnumerable<string> rows, RowsOptions options = null)
        {
            Invoke(() => AnsiConsole.Rows(rows, options));
        }

        /// <summary>
        /// Renders vertically stacked rows from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        /// <param name="options">Optional row rendering options.</param>
        public void Rows(IEnumerable<object> rows, RowsOptions options = null)
        {
            Invoke(() => AnsiConsole.Rows(rows, options));
        }

        /// <summary>
        /// Renders vertically stacked rows with explicit options.
        /// </summary>
        /// <param name="options">Optional row rendering options.</param>
        /// <param name="rows">The row contents to render.</param>
        public void Rows(RowsOptions options, params string[] rows)
        {
            Invoke(() => AnsiConsole.Rows(options, rows));
        }

        /// <summary>
        /// Renders vertically stacked rows from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Optional row rendering options.</param>
        /// <param name="rows">The row values to render.</param>
        public void Rows(RowsOptions options, params object[] rows)
        {
            Invoke(() => AnsiConsole.Rows(options, rows));
        }

        /// <summary>
        /// Configures and renders vertically stacked rows.
        /// </summary>
        /// <param name="configure">Action used to add rows.</param>
        /// <param name="options">Optional row rendering options.</param>
        public void Rows(Action<RowsEntry> configure, RowsOptions options = null)
        {
            Invoke(() => AnsiConsole.Rows(configure, options));
        }

        /// <summary>
        /// Renders responsive layout panes.
        /// </summary>
        /// <param name="panes">The panes to render.</param>
        /// <param name="options">Optional layout rendering options.</param>
        public void Layout(IEnumerable<LayoutPane> panes, LayoutOptions options = null)
        {
            Invoke(() => AnsiConsole.Layout(panes, options));
        }

        /// <summary>
        /// Configures and renders responsive layout panes.
        /// </summary>
        /// <param name="configure">Action used to add panes.</param>
        /// <param name="options">Optional layout rendering options.</param>
        public void Layout(Action<LayoutEntry> configure, LayoutOptions options = null)
        {
            Invoke(() => AnsiConsole.Layout(configure, options));
        }

        /// <summary>
        /// Renders text with responsive padding.
        /// </summary>
        /// <param name="content">The text content to pad.</param>
        /// <param name="options">Padding options.</param>
        public void Pad(string content, PadOptions options)
        {
            Invoke(() => AnsiConsole.Pad(content, options));
        }

        /// <summary>
        /// Renders text with responsive padding.
        /// </summary>
        /// <param name="content">The text content to pad.</param>
        /// <param name="left">Spaces added to the left of each content line.</param>
        /// <param name="right">Spaces added to the right of each content line.</param>
        /// <param name="top">Blank lines added before the content.</param>
        /// <param name="bottom">Blank lines added after the content.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        public void Pad(
            string content,
            int left = 0,
            int right = 0,
            int top = 0,
            int bottom = 0,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null)
        {
            Invoke(() => AnsiConsole.Pad(content, left, right, top, bottom, width, color, backgroundColor));
        }

        /// <summary>
        /// Captures a render action and renders the captured text with responsive padding.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="options">Padding options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public void Pad(Action render, PadOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            Invoke(() => AnsiConsole.Pad(render, options, captureWidth, captureHeight));
        }

        /// <summary>
        /// Captures a renderable and renders the captured text with responsive padding.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="options">Padding options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public void Pad(IConsoulRenderable renderable, PadOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            Invoke(() => AnsiConsole.Pad(renderable, options, captureWidth, captureHeight));
        }

        /// <summary>
        /// Renders text aligned inside a responsive width.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="options">Alignment options.</param>
        public void Align(string content, AlignOptions options)
        {
            Invoke(() => AnsiConsole.Align(content, options));
        }

        /// <summary>
        /// Renders text aligned inside a responsive width.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="alignment">Alignment used for content lines.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        public void Align(
            string content,
            TextBlockAlignment alignment = TextBlockAlignment.Left,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null)
        {
            Invoke(() => AnsiConsole.Align(content, alignment, width, color, backgroundColor));
        }

        /// <summary>
        /// Captures a render action and renders the captured text aligned inside a responsive width.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="options">Alignment options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public void Align(Action render, AlignOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            Invoke(() => AnsiConsole.Align(render, options, captureWidth, captureHeight));
        }

        /// <summary>
        /// Captures a renderable and renders the captured text aligned inside a responsive width.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="options">Alignment options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public void Align(IConsoulRenderable renderable, AlignOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            Invoke(() => AnsiConsole.Align(renderable, options, captureWidth, captureHeight));
        }

        /// <summary>
        /// Renders wrapped, aligned, and padded text.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="options">Text block options.</param>
        public void TextBlock(string content, TextBlockOptions options)
        {
            Invoke(() => AnsiConsole.TextBlock(content, options));
        }

        /// <summary>
        /// Renders wrapped, aligned, and padded text.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="alignment">Alignment used for wrapped lines.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        public void TextBlock(
            string content,
            int? width = null,
            TextBlockAlignment alignment = TextBlockAlignment.Left,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null)
        {
            Invoke(() => AnsiConsole.TextBlock(content, width, alignment, color, backgroundColor));
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="options">Path rendering options.</param>
        public void TextPath(string path, TextPathOptions options)
        {
            Invoke(() => AnsiConsole.TextPath(path, options));
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="directoryColor">Optional directory segment color.</param>
        /// <param name="fileNameColor">Optional file-name color.</param>
        /// <param name="separatorColor">Optional separator color.</param>
        public void TextPath(
            string path,
            int? width = null,
            ConsoleColor? directoryColor = null,
            ConsoleColor? fileNameColor = null,
            ConsoleColor? separatorColor = null)
        {
            Invoke(() => AnsiConsole.TextPath(path, width, directoryColor, fileNameColor, separatorColor));
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path using Spectre-style style text.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="directoryStyle">Optional directory segment style.</param>
        /// <param name="fileNameStyle">Optional file-name style.</param>
        /// <param name="separatorStyle">Optional separator style.</param>
        public void TextPath(
            string path,
            int? width,
            string directoryStyle,
            string fileNameStyle = null,
            string separatorStyle = null)
        {
            Invoke(() => AnsiConsole.TextPath(path, width, directoryStyle, fileNameStyle, separatorStyle));
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path using Spectre-style style text.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="directoryStyle">Optional directory segment style.</param>
        /// <param name="fileNameStyle">Optional file-name style.</param>
        /// <param name="separatorStyle">Optional separator style.</param>
        public void TextPathStyled(
            string path,
            int? width = null,
            string directoryStyle = null,
            string fileNameStyle = null,
            string separatorStyle = null)
        {
            Invoke(() => AnsiConsole.TextPathStyled(path, width, directoryStyle, fileNameStyle, separatorStyle));
        }

        /// <summary>
        /// Renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="items">The breadcrumb items to render.</param>
        public void Breadcrumb(params string[] items)
        {
            Invoke(() => AnsiConsole.Breadcrumb(items));
        }

        /// <summary>
        /// Renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="items">The breadcrumb items to render.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        public void Breadcrumb(IEnumerable<string> items, BreadcrumbOptions options = null)
        {
            Invoke(() => AnsiConsole.Breadcrumb(items, options));
        }

        /// <summary>
        /// Renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="items">The breadcrumb items to render.</param>
        public void Breadcrumb(BreadcrumbOptions options, params string[] items)
        {
            Invoke(() => AnsiConsole.Breadcrumb(options, items));
        }

        /// <summary>
        /// Configures and renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="configure">Action used to add breadcrumb items.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        public void Breadcrumb(Action<BreadcrumbEntry> configure, BreadcrumbOptions options = null)
        {
            Invoke(() => AnsiConsole.Breadcrumb(configure, options));
        }

        /// <summary>
        /// Creates responsive breadcrumb text without writing to the console.
        /// </summary>
        /// <param name="items">The breadcrumb items.</param>
        /// <returns>A bounded breadcrumb string.</returns>
        public string BreadcrumbText(params string[] items)
        {
            return Invoke(() => AnsiConsole.BreadcrumbText(items));
        }

        /// <summary>
        /// Creates responsive breadcrumb text without writing to the console.
        /// </summary>
        /// <param name="items">The breadcrumb items.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded breadcrumb string.</returns>
        public string BreadcrumbText(IEnumerable<string> items, BreadcrumbOptions options = null, int? width = null)
        {
            return Invoke(() => AnsiConsole.BreadcrumbText(items, options, width));
        }

        /// <summary>
        /// Creates responsive breadcrumb text without writing to the console.
        /// </summary>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="items">The breadcrumb items.</param>
        /// <returns>A bounded breadcrumb string.</returns>
        public string BreadcrumbText(BreadcrumbOptions options, params string[] items)
        {
            return Invoke(() => AnsiConsole.BreadcrumbText(options, items));
        }

        /// <summary>
        /// Renders a responsive shortcut bar.
        /// </summary>
        /// <param name="items">The shortcut items to render.</param>
        public void ShortcutBar(params ShortcutItem[] items)
        {
            Invoke(() => AnsiConsole.ShortcutBar(items));
        }

        /// <summary>
        /// Renders a responsive shortcut bar.
        /// </summary>
        /// <param name="items">The shortcut items to render.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        public void ShortcutBar(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null)
        {
            Invoke(() => AnsiConsole.ShortcutBar(items, options));
        }

        /// <summary>
        /// Renders a responsive shortcut bar.
        /// </summary>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="items">The shortcut items to render.</param>
        public void ShortcutBar(ShortcutBarOptions options, params ShortcutItem[] items)
        {
            Invoke(() => AnsiConsole.ShortcutBar(options, items));
        }

        /// <summary>
        /// Configures and renders a responsive shortcut bar.
        /// </summary>
        /// <param name="configure">Action used to add shortcut items.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        public void ShortcutBar(Action<ShortcutBarEntry> configure, ShortcutBarOptions options = null)
        {
            Invoke(() => AnsiConsole.ShortcutBar(configure, options));
        }

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="items">The shortcut items.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        public string ShortcutBarText(params ShortcutItem[] items)
        {
            return Invoke(() => AnsiConsole.ShortcutBarText(items));
        }

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="items">The shortcut items.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        public string ShortcutBarText(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null, int? width = null)
        {
            return Invoke(() => AnsiConsole.ShortcutBarText(items, options, width));
        }

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="items">The shortcut items.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        public string ShortcutBarText(ShortcutBarOptions options, params ShortcutItem[] items)
        {
            return Invoke(() => AnsiConsole.ShortcutBarText(options, items));
        }

        /// <summary>
        /// Renders a responsive checklist.
        /// </summary>
        /// <param name="items">The checklist items to render.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        public void Checklist(IEnumerable<ChecklistItem> items, ChecklistOptions options = null)
        {
            Invoke(() => AnsiConsole.Checklist(items, options));
        }

        /// <summary>
        /// Renders a responsive checklist.
        /// </summary>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="items">The checklist items to render.</param>
        public void Checklist(ChecklistOptions options, params ChecklistItem[] items)
        {
            Invoke(() => AnsiConsole.Checklist(options, items));
        }

        /// <summary>
        /// Configures and renders a responsive checklist.
        /// </summary>
        /// <param name="configure">Action used to add checklist items.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        public void Checklist(Action<ChecklistEntry> configure, ChecklistOptions options = null)
        {
            Invoke(() => AnsiConsole.Checklist(configure, options));
        }

        /// <summary>
        /// Renders readable model properties as a responsive property list.
        /// </summary>
        /// <param name="model">The model object whose properties should be summarized.</param>
        /// <param name="options">Optional property list rendering options.</param>
        public void PropertyList(object model, PropertyListOptions options = null)
        {
            Invoke(() => AnsiConsole.PropertyList(model, options));
        }

        /// <summary>
        /// Renders a responsive list of labeled values.
        /// </summary>
        /// <param name="items">The property list items to render.</param>
        /// <param name="options">Optional property list rendering options.</param>
        public void PropertyList(IEnumerable<PropertyListItem> items, PropertyListOptions options = null)
        {
            Invoke(() => AnsiConsole.PropertyList(items, options));
        }

        /// <summary>
        /// Configures and renders a responsive property list.
        /// </summary>
        /// <param name="configure">Action used to add property list items.</param>
        /// <param name="options">Optional property list rendering options.</param>
        public void PropertyList(Action<PropertyListEntry> configure, PropertyListOptions options = null)
        {
            Invoke(() => AnsiConsole.PropertyList(configure, options));
        }

        /// <summary>
        /// Configures and renders compact responsive metrics.
        /// </summary>
        /// <param name="configure">Action used to add metric items.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        public void Metrics(Action<MetricsEntry> configure, MetricsOptions options = null)
        {
            Invoke(() => AnsiConsole.Metrics(configure, options));
        }

        /// <summary>
        /// Renders compact responsive metrics.
        /// </summary>
        /// <param name="items">The metric items to render.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        public void Metrics(IEnumerable<MetricItem> items, MetricsOptions options = null)
        {
            Invoke(() => AnsiConsole.Metrics(items, options));
        }

        /// <summary>
        /// Renders compact responsive metrics.
        /// </summary>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="items">The metric items to render.</param>
        public void Metrics(MetricsOptions options, params MetricItem[] items)
        {
            Invoke(() => AnsiConsole.Metrics(options, items));
        }

        /// <summary>
        /// Configures and renders a responsive horizontal bar chart.
        /// </summary>
        /// <param name="configure">Action used to add chart items.</param>
        /// <param name="options">Optional chart rendering options.</param>
        public void BarChart(Action<BarChartEntry> configure, BarChartOptions options = null)
        {
            Invoke(() => AnsiConsole.BarChart(configure, options));
        }

        /// <summary>
        /// Renders a responsive horizontal bar chart.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Optional chart rendering options.</param>
        public void BarChart(IEnumerable<BarChartItem> items, BarChartOptions options = null)
        {
            Invoke(() => AnsiConsole.BarChart(items, options));
        }

        /// <summary>
        /// Configures and renders a responsive breakdown chart.
        /// </summary>
        /// <param name="configure">Action used to add chart items.</param>
        /// <param name="options">Optional chart rendering options.</param>
        public void BreakdownChart(Action<BreakdownChartEntry> configure, BreakdownChartOptions options = null)
        {
            Invoke(() => AnsiConsole.BreakdownChart(configure, options));
        }

        /// <summary>
        /// Renders a responsive breakdown chart.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Optional chart rendering options.</param>
        public void BreakdownChart(IEnumerable<BreakdownChartItem> items, BreakdownChartOptions options = null)
        {
            Invoke(() => AnsiConsole.BreakdownChart(items, options));
        }

        /// <summary>
        /// Renders a compact responsive sparkline chart.
        /// </summary>
        /// <param name="values">The values rendered by the sparkline.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        public void Sparkline(IEnumerable<double> values, SparklineOptions options = null)
        {
            Invoke(() => AnsiConsole.Sparkline(values, options));
        }

        /// <summary>
        /// Renders a compact responsive sparkline chart.
        /// </summary>
        /// <param name="values">The values rendered by the sparkline.</param>
        /// <param name="label">Optional label rendered before the sparkline.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="color">Optional sparkline foreground color.</param>
        /// <param name="showRange">Whether the rendered value range should appear after the sparkline.</param>
        public void Sparkline(
            IEnumerable<double> values,
            string label = null,
            int? width = null,
            ConsoleColor? color = null,
            bool showRange = false)
        {
            Invoke(() => AnsiConsole.Sparkline(values, label, width, color, showRange));
        }

        /// <summary>
        /// Configures and renders a compact responsive sparkline chart.
        /// </summary>
        /// <param name="configure">Action used to add values.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        public void Sparkline(Action<SparklineEntry> configure, SparklineOptions options = null)
        {
            Invoke(() => AnsiConsole.Sparkline(configure, options));
        }

        /// <summary>
        /// Creates a status builder that can start status scopes with a Spectre-style workflow.
        /// </summary>
        /// <returns>A status builder.</returns>
        public StatusBuilder Status()
        {
            return Invoke(() => AnsiConsole.Status());
        }

        /// <summary>
        /// Creates an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <returns>The status scope.</returns>
        public StatusScope Status(string message, StatusOptions options = null)
        {
            return Invoke(() => AnsiConsole.Status(message, options));
        }

        /// <summary>
        /// Runs an action inside an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The scoped action.</param>
        /// <param name="options">Optional status rendering options.</param>
        public void Status(string message, Action<StatusScope> action, StatusOptions options = null)
        {
            Invoke(() => AnsiConsole.Status(message, action, options));
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public Task StatusAsync(string message, Func<StatusScope, CancellationToken, Task> action, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(() => AnsiConsole.StatusAsync(message, action, options, cancellationToken));
        }

        /// <summary>
        /// Creates a spinner builder that can start spinner scopes with a Spectre-style workflow.
        /// </summary>
        /// <returns>A spinner builder.</returns>
        public SpinnerBuilder Spinner()
        {
            return Invoke(() => AnsiConsole.Spinner());
        }

        /// <summary>
        /// Creates an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <returns>The spinner scope.</returns>
        public SpinnerScope Spinner(string message, SpinnerOptions options = null)
        {
            return Invoke(() => AnsiConsole.Spinner(message, options));
        }

        /// <summary>
        /// Runs an action inside an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The scoped action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        public void Spinner(string message, Action<SpinnerScope> action, SpinnerOptions options = null)
        {
            Invoke(() => AnsiConsole.Spinner(message, action, options));
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public Task SpinnerAsync(string message, Func<SpinnerScope, CancellationToken, Task> action, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(() => AnsiConsole.SpinnerAsync(message, action, options, cancellationToken));
        }

        /// <summary>
        /// Creates a multi-task progress context.
        /// </summary>
        /// <param name="options">Optional progress rendering options.</param>
        /// <returns>The progress context.</returns>
        public ProgressContext Progress(ProgressContextOptions options = null)
        {
            return Invoke(() => AnsiConsole.Progress(options));
        }

        /// <summary>
        /// Runs an action inside a multi-task progress context.
        /// </summary>
        /// <param name="action">The scoped progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        public void Progress(Action<ProgressContext> action, ProgressContextOptions options = null)
        {
            Invoke(() => AnsiConsole.Progress(action, options));
        }

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context.
        /// </summary>
        /// <param name="action">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public Task ProgressAsync(Func<ProgressContext, CancellationToken, Task> action, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(() => AnsiConsole.ProgressAsync(action, options, cancellationToken));
        }

        /// <summary>
        /// Creates a live display context.
        /// </summary>
        /// <param name="options">Optional live display options.</param>
        /// <returns>The live display context.</returns>
        public LiveDisplayContext Live(LiveDisplayOptions options = null)
        {
            return Invoke(() => AnsiConsole.Live(options));
        }

        /// <summary>
        /// Creates a live display context with an initial render target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="options">Optional live display options.</param>
        /// <returns>The live display context.</returns>
        public LiveDisplayContext Live(IConsoulRenderable target, LiveDisplayOptions options = null)
        {
            return Invoke(() => AnsiConsole.Live(target, options));
        }

        /// <summary>
        /// Runs an action inside a live display context.
        /// </summary>
        /// <param name="action">The scoped live display action.</param>
        /// <param name="options">Optional live display options.</param>
        public void Live(Action<LiveDisplayContext> action, LiveDisplayOptions options = null)
        {
            Invoke(() => AnsiConsole.Live(action, options));
        }

        /// <summary>
        /// Runs an action inside a live display context with an initial render target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The scoped live display action.</param>
        /// <param name="options">Optional live display options.</param>
        public void Live(IConsoulRenderable target, Action<LiveDisplayContext> action, LiveDisplayOptions options = null)
        {
            Invoke(() => AnsiConsole.Live(target, action, options));
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context.
        /// </summary>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public Task LiveAsync(Func<LiveDisplayContext, CancellationToken, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(() => AnsiConsole.LiveAsync(action, options, cancellationToken));
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public Task LiveAsync(IConsoulRenderable target, Func<LiveDisplayContext, CancellationToken, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(() => AnsiConsole.LiveAsync(target, action, options, cancellationToken));
        }

        /// <summary>
        /// Captures Consoul output from a render action into a string.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="width">The virtual console width.</param>
        /// <param name="height">The virtual console height.</param>
        /// <returns>The captured text.</returns>
        public string Capture(Action render, int width = 80, int height = 200)
        {
            return Invoke(() => AnsiConsole.Capture(render, width, height));
        }

        /// <summary>
        /// Captures Consoul output from a renderable component into a string.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="width">The virtual console width.</param>
        /// <param name="height">The virtual console height.</param>
        /// <returns>The captured text.</returns>
        public string Capture(IConsoulRenderable renderable, int width = 80, int height = 200)
        {
            return Invoke(() => AnsiConsole.Capture(renderable, width, height));
        }
    }
}
