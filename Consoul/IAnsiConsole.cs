using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines an injectable Spectre-style console facade over Consoul's output, prompt, and capture APIs.
    /// </summary>
    public interface IAnsiConsole
    {
        /// <summary>
        /// Gets a snapshot of the active terminal profile.
        /// </summary>
        TerminalProfile Profile { get; }

        /// <summary>
        /// Gets cursor operations for the active console.
        /// </summary>
        IAnsiConsoleCursor Cursor { get; }

        /// <summary>
        /// Starts recording output written through the active console.
        /// </summary>
        void Record();

        /// <summary>
        /// Exports recorded console output as plain text.
        /// </summary>
        /// <returns>The recorded console output.</returns>
        string ExportText();

        /// <summary>
        /// Exports recorded console output as escaped HTML.
        /// </summary>
        /// <returns>The recorded console output wrapped in a preformatted HTML block.</returns>
        string ExportHtml();

        /// <summary>
        /// Clears the active console driver.
        /// </summary>
        void Clear();

        /// <summary>
        /// Writes plain text without appending a line break.
        /// </summary>
        /// <param name="text">The text to write.</param>
        void Write(string text);

        /// <summary>
        /// Writes ANSI or VT escape sequences through a caller-provided action.
        /// </summary>
        /// <param name="action">The action that writes the escape sequence output.</param>
        void WriteAnsi(Action action);

        /// <summary>
        /// Renders a Consoul renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component to render.</param>
        void Write(IConsoulRenderable renderable);

        /// <summary>
        /// Writes a blank line.
        /// </summary>
        void WriteLine();

        /// <summary>
        /// Writes plain text followed by a line break.
        /// </summary>
        /// <param name="text">The text to write.</param>
        void WriteLine(string text);

        /// <summary>
        /// Writes lightweight markup without appending a line break.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        void Markup(string markup);

        /// <summary>
        /// Writes lightweight markup followed by a line break.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        void MarkupLine(string markup);

        /// <summary>
        /// Writes a blank markup line.
        /// </summary>
        void MarkupLine();

        /// <summary>
        /// Writes interpolated markup without appending a line break, escaping interpolated values.
        /// </summary>
        /// <param name="markup">The interpolated markup text to render.</param>
        void MarkupInterpolated(FormattableString markup);

        /// <summary>
        /// Writes interpolated markup followed by a line break, escaping interpolated values.
        /// </summary>
        /// <param name="markup">The interpolated markup text to render.</param>
        void MarkupLineInterpolated(FormattableString markup);

        /// <summary>
        /// Escapes square brackets in text that will be inserted into markup.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>Markup-safe text.</returns>
        string EscapeMarkup(string value);

        /// <summary>
        /// Removes recognized markup tags from text.
        /// </summary>
        /// <param name="value">Markup text to strip.</param>
        /// <returns>Plain text with recognized markup tags removed.</returns>
        string RemoveMarkup(string value);

        /// <summary>
        /// Shows a prompt through this console facade.
        /// </summary>
        /// <typeparam name="TResult">The prompt result type.</typeparam>
        /// <param name="prompt">The prompt to show.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The prompt result.</returns>
        TResult Prompt<TResult>(IPrompt<TResult> prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Shows a prompt asynchronously through this console facade.
        /// </summary>
        /// <typeparam name="TResult">The prompt result type.</typeparam>
        /// <param name="prompt">The prompt to show.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the prompt result.</returns>
        Task<TResult> PromptAsync<TResult>(IPrompt<TResult> prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders a selection prompt and returns its explicit selection result.
        /// </summary>
        /// <param name="prompt">The selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The selection result.</returns>
        PromptResult Prompt(SelectionPrompt prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders a typed selection prompt and returns the selected item result.
        /// </summary>
        /// <typeparam name="T">The option item type.</typeparam>
        /// <param name="prompt">The typed selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed selection result.</returns>
        SelectionResult<T> Prompt<T>(SelectionPrompt<T> prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders a typed text prompt and returns its parsed value result.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The typed text prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed text prompt result.</returns>
        TextPromptResult<T> Prompt<T>(TextPrompt<T> prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders a confirmation prompt and returns its explicit confirmation result.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The confirmation result.</returns>
        ConfirmResult Prompt(ConfirmPrompt prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders a secret prompt and returns its explicit secret result.
        /// </summary>
        /// <param name="prompt">The secret prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The secret prompt result.</returns>
        SecretResult Prompt(SecretPrompt prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders a multi-selection prompt and returns selected indexes.
        /// </summary>
        /// <param name="prompt">The multi-selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The multi-selection result.</returns>
        MultiSelectionResult Prompt(MultiSelectionPrompt prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders a typed multi-selection prompt and returns selected items.
        /// </summary>
        /// <typeparam name="T">The option item type.</typeparam>
        /// <param name="prompt">The typed multi-selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed multi-selection result.</returns>
        MultiSelectionResult<T> Prompt<T>(MultiSelectionPrompt<T> prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates and renders a typed text prompt.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed text prompt result.</returns>
        TextPromptResult<T> Ask<T>(string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates and renders a typed text prompt with a default value.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="defaultValue">The default value used when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed text prompt result.</returns>
        TextPromptResult<T> Ask<T>(string message, T defaultValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates and renders a typed text prompt asynchronously.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the typed text prompt result.</returns>
        Task<TextPromptResult<T>> AskAsync<T>(string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates and renders a typed text prompt with a default value asynchronously.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="defaultValue">The default value used when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the typed text prompt result.</returns>
        Task<TextPromptResult<T>> AskAsync<T>(string message, T defaultValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders a yes/no confirmation prompt.
        /// </summary>
        /// <param name="message">The confirmation message.</param>
        /// <param name="defaultValue">Optional default confirmation value.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The confirmation result.</returns>
        ConfirmResult Confirm(string message, bool? defaultValue = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders a yes/no confirmation prompt asynchronously.
        /// </summary>
        /// <param name="message">The confirmation message.</param>
        /// <param name="defaultValue">Optional default confirmation value.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the confirmation result.</returns>
        Task<ConfirmResult> ConfirmAsync(string message, bool? defaultValue = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes an exception using Consoul's exception renderer.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="message">Optional contextual message.</param>
        /// <param name="options">Optional exception rendering options.</param>
        void WriteException(Exception exception, string message = null, ExceptionRenderOptions options = null);

        /// <summary>
        /// Writes an exception using migration-friendly formatting flags.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="format">Formatting flags for the exception panel.</param>
        void WriteException(Exception exception, ExceptionFormats format);

        /// <summary>
        /// Writes an exception using Spectre-style settings.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="settings">Settings used to adapt the exception panel.</param>
        void WriteException(Exception exception, ExceptionSettings settings);

        /// <summary>
        /// Renders a horizontal rule.
        /// </summary>
        /// <param name="title">Optional rule title.</param>
        /// <param name="options">Optional rule rendering options.</param>
        void Rule(string title = null, RuleOptions options = null);

        /// <summary>
        /// Renders content in a panel.
        /// </summary>
        /// <param name="content">The panel content.</param>
        /// <param name="options">Optional panel rendering options.</param>
        void Panel(string content, PanelOptions options = null);

        /// <summary>
        /// Captures and renders a component in a panel.
        /// </summary>
        /// <param name="renderable">The component to render inside the panel.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the component.</param>
        /// <param name="captureHeight">Optional virtual console height used while capturing the component.</param>
        void Panel(IConsoulRenderable renderable, PanelOptions options = null, int? captureWidth = null, int captureHeight = 200);

        /// <summary>
        /// Captures and renders a delegate in a panel.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the action.</param>
        /// <param name="captureHeight">Optional virtual console height used while capturing the action.</param>
        void Panel(Action render, PanelOptions options = null, int? captureWidth = null, int captureHeight = 200);

        /// <summary>
        /// Renders a model-backed table.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The table items.</param>
        /// <param name="selectors">Optional property selectors for visible columns.</param>
        /// <returns>The rendered dynamic table view.</returns>
        DynamicTableView<T> Table<T>(IEnumerable<T> items, params Expression<Func<T, object>>[] selectors) where T : class;

        /// <summary>
        /// Renders a model-backed table with explicit rendering options.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The table items.</param>
        /// <param name="options">The table rendering options.</param>
        /// <param name="selectors">Optional property selectors for visible columns.</param>
        /// <returns>The rendered dynamic table view.</returns>
        DynamicTableView<T> Table<T>(IEnumerable<T> items, TableRenderOptions options, params Expression<Func<T, object>>[] selectors) where T : class;

        /// <summary>
        /// Renders a tree.
        /// </summary>
        /// <param name="root">The root node to render.</param>
        /// <param name="options">Optional tree rendering options.</param>
        void Tree(TreeNode root, TreeOptions options = null);

        /// <summary>
        /// Renders a tree configured through a root-node callback.
        /// </summary>
        /// <param name="rootText">The root node text.</param>
        /// <param name="configure">Optional callback used to add child nodes.</param>
        /// <param name="options">Optional tree rendering options.</param>
        void Tree(string rootText, Action<TreeNode> configure = null, TreeOptions options = null);

        /// <summary>
        /// Renders text columns.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        void Columns(params string[] columns);

        /// <summary>
        /// Renders columns from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        void Columns(params object[] columns);

        /// <summary>
        /// Renders text columns with explicit options.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        /// <param name="options">Optional column rendering options.</param>
        void Columns(IEnumerable<string> columns, ColumnsOptions options = null);

        /// <summary>
        /// Renders columns from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        /// <param name="options">Optional column rendering options.</param>
        void Columns(IEnumerable<object> columns, ColumnsOptions options = null);

        /// <summary>
        /// Renders responsive side-by-side text columns with explicit options.
        /// </summary>
        /// <param name="options">Optional column rendering options.</param>
        /// <param name="columns">The column values to render.</param>
        void Columns(ColumnsOptions options, params string[] columns);

        /// <summary>
        /// Renders responsive side-by-side columns from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Optional column rendering options.</param>
        /// <param name="columns">The column values to render.</param>
        void Columns(ColumnsOptions options, params object[] columns);

        /// <summary>
        /// Builds and renders responsive side-by-side text columns.
        /// </summary>
        /// <param name="configure">Action used to add columns before rendering.</param>
        /// <param name="options">Optional column rendering options.</param>
        void Columns(Action<ColumnsEntry> configure, ColumnsOptions options = null);

        /// <summary>
        /// Writes a compact single-line badge.
        /// </summary>
        /// <param name="label">The badge label to render.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional badge rendering options.</param>
        /// <param name="writeLine">Whether to append a line break after the badge.</param>
        void Badge(string label, BadgeKind kind = BadgeKind.Default, BadgeOptions options = null, bool writeLine = true);

        /// <summary>
        /// Creates compact badge text without writing to the console.
        /// </summary>
        /// <param name="label">The badge label.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional badge rendering options.</param>
        /// <param name="maxWidth">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded badge string.</returns>
        string BadgeText(string label, BadgeKind kind = BadgeKind.Default, BadgeOptions options = null, int? maxWidth = null);

        /// <summary>
        /// Renders a responsive headline.
        /// </summary>
        /// <param name="text">The headline text to render.</param>
        /// <param name="options">Headline rendering options.</param>
        void Headline(string text, HeadlineOptions options);

        /// <summary>
        /// Renders a responsive headline.
        /// </summary>
        /// <param name="text">The headline text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="style">The headline rendering style.</param>
        void Headline(string text, int? width = null, ConsoleColor? color = null, HeadlineStyle style = HeadlineStyle.Block);

        /// <summary>
        /// Renders large ASCII text.
        /// </summary>
        /// <param name="text">The text to render as large glyphs.</param>
        /// <param name="options">Figlet rendering options.</param>
        void FigletText(string text, FigletTextOptions options);

        /// <summary>
        /// Renders large ASCII text.
        /// </summary>
        /// <param name="text">The text to render as large glyphs.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="alignment">Alignment used for rendered lines.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <param name="character">The character used to fill glyphs.</param>
        void FigletText(
            string text,
            int? width = null,
            TextBlockAlignment alignment = TextBlockAlignment.Center,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null,
            char character = '#');

        /// <summary>
        /// Renders a responsive month calendar.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        /// <param name="options">Calendar rendering options.</param>
        void Calendar(DateTime month, CalendarOptions options = null);

        /// <summary>
        /// Renders a responsive month calendar.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="options">Calendar rendering options.</param>
        void Calendar(int year, int month, CalendarOptions options = null);

        /// <summary>
        /// Builds and renders a responsive month calendar.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        /// <param name="configure">Action used to mark days before rendering.</param>
        /// <param name="options">Calendar rendering options.</param>
        void Calendar(DateTime month, Action<CalendarEntry> configure, CalendarOptions options = null);

        /// <summary>
        /// Builds and renders a responsive month calendar.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="configure">Action used to mark days before rendering.</param>
        /// <param name="options">Calendar rendering options.</param>
        void Calendar(int year, int month, Action<CalendarEntry> configure, CalendarOptions options = null);

        /// <summary>
        /// Renders a responsive intent-colored callout.
        /// </summary>
        /// <param name="message">The callout message.</param>
        /// <param name="kind">The callout visual intent.</param>
        /// <param name="options">Optional callout rendering options.</param>
        void Callout(string message, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null);

        /// <summary>
        /// Captures a render action and renders the captured output inside a callout.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="kind">The callout visual intent.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        void Callout(Action render, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null, int? captureWidth = null, int captureHeight = 200);

        /// <summary>
        /// Captures a renderable and renders the captured output inside a callout.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="kind">The callout visual intent.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        void Callout(IConsoulRenderable renderable, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null, int? captureWidth = null, int captureHeight = 200);

        /// <summary>
        /// Validates a model and renders a validation summary when issues are present.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <returns>The validation result for the model.</returns>
        ValidationSummaryResult ValidationSummary(object model, ValidationSummaryOptions options = null);

        /// <summary>
        /// Validates a model without rendering output.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <returns>The validation result for the model.</returns>
        ValidationSummaryResult ValidateModel(object model, ValidationSummaryOptions options = null);

        /// <summary>
        /// Renders a responsive timeline.
        /// </summary>
        /// <param name="items">The timeline items to render.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        void Timeline(IEnumerable<TimelineItem> items, TimelineOptions options = null);

        /// <summary>
        /// Renders a responsive timeline.
        /// </summary>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="items">The timeline items to render.</param>
        void Timeline(TimelineOptions options, params TimelineItem[] items);

        /// <summary>
        /// Configures and renders a responsive timeline.
        /// </summary>
        /// <param name="configure">Action used to add timeline items.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        void Timeline(Action<TimelineEntry> configure, TimelineOptions options = null);

        /// <summary>
        /// Renders a responsive bullet or numbered list.
        /// </summary>
        /// <param name="items">The list items to render.</param>
        /// <param name="options">Optional list rendering options.</param>
        void BulletList(IEnumerable<BulletListItem> items, BulletListOptions options = null);

        /// <summary>
        /// Renders a responsive bullet or numbered list.
        /// </summary>
        /// <param name="options">Optional list rendering options.</param>
        /// <param name="items">The list items to render.</param>
        void BulletList(BulletListOptions options, params BulletListItem[] items);

        /// <summary>
        /// Configures and renders a responsive bullet or numbered list.
        /// </summary>
        /// <param name="configure">Action used to add list items.</param>
        /// <param name="options">Optional list rendering options.</param>
        void BulletList(Action<BulletListEntry> configure, BulletListOptions options = null);

        /// <summary>
        /// Renders responsive grid cells.
        /// </summary>
        /// <param name="cells">The cell contents to render.</param>
        void Grid(params string[] cells);

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="cells">The cell values to render.</param>
        void Grid(params object[] cells);

        /// <summary>
        /// Renders responsive grid cells.
        /// </summary>
        /// <param name="cells">The cell contents to render.</param>
        /// <param name="options">Optional grid rendering options.</param>
        void Grid(IEnumerable<string> cells, GridOptions options = null);

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="cells">The cell values to render.</param>
        /// <param name="options">Optional grid rendering options.</param>
        void Grid(IEnumerable<object> cells, GridOptions options = null);

        /// <summary>
        /// Renders responsive grid cells with explicit options.
        /// </summary>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="cells">The cell contents to render.</param>
        void Grid(GridOptions options, params string[] cells);

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="cells">The cell values to render.</param>
        void Grid(GridOptions options, params object[] cells);

        /// <summary>
        /// Configures and renders responsive grid cells.
        /// </summary>
        /// <param name="configure">Action used to add cells.</param>
        /// <param name="options">Optional grid rendering options.</param>
        void Grid(Action<GridEntry> configure, GridOptions options = null);

        /// <summary>
        /// Builds and renders a small ASCII canvas.
        /// </summary>
        /// <param name="width">The logical canvas width.</param>
        /// <param name="height">The logical canvas height.</param>
        /// <param name="configure">Action used to draw on the canvas.</param>
        /// <param name="options">Optional canvas rendering options.</param>
        void Canvas(int width, int height, Action<CanvasEntry> configure, CanvasOptions options = null);

        /// <summary>
        /// Renders an existing ASCII canvas.
        /// </summary>
        /// <param name="canvas">The canvas to render.</param>
        void Canvas(CanvasEntry canvas);

        /// <summary>
        /// Renders an existing dependency-free terminal canvas image.
        /// </summary>
        /// <param name="image">The image to render.</param>
        void CanvasImage(CanvasImageEntry image);

        /// <summary>
        /// Renders a dependency-free terminal canvas image from a row-major pixel matrix.
        /// </summary>
        /// <param name="pixels">The source pixels, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional image rendering options.</param>
        void CanvasImage(CanvasImagePixel[,] pixels, CanvasImageOptions options = null);

        /// <summary>
        /// Renders a dependency-free terminal canvas image from a row-major console color matrix.
        /// </summary>
        /// <param name="pixels">The source colors, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional image rendering options.</param>
        void CanvasImage(ConsoleColor[,] pixels, CanvasImageOptions options = null);

        /// <summary>
        /// Renders vertically stacked rows.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        void Rows(params string[] rows);

        /// <summary>
        /// Renders vertically stacked rows from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        void Rows(params object[] rows);

        /// <summary>
        /// Renders vertically stacked rows.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        /// <param name="options">Optional row rendering options.</param>
        void Rows(IEnumerable<string> rows, RowsOptions options = null);

        /// <summary>
        /// Renders vertically stacked rows from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        /// <param name="options">Optional row rendering options.</param>
        void Rows(IEnumerable<object> rows, RowsOptions options = null);

        /// <summary>
        /// Renders vertically stacked rows with explicit options.
        /// </summary>
        /// <param name="options">Optional row rendering options.</param>
        /// <param name="rows">The row contents to render.</param>
        void Rows(RowsOptions options, params string[] rows);

        /// <summary>
        /// Renders vertically stacked rows from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Optional row rendering options.</param>
        /// <param name="rows">The row values to render.</param>
        void Rows(RowsOptions options, params object[] rows);

        /// <summary>
        /// Configures and renders vertically stacked rows.
        /// </summary>
        /// <param name="configure">Action used to add rows.</param>
        /// <param name="options">Optional row rendering options.</param>
        void Rows(Action<RowsEntry> configure, RowsOptions options = null);

        /// <summary>
        /// Renders responsive layout panes.
        /// </summary>
        /// <param name="panes">The panes to render.</param>
        /// <param name="options">Optional layout rendering options.</param>
        void Layout(IEnumerable<LayoutPane> panes, LayoutOptions options = null);

        /// <summary>
        /// Configures and renders responsive layout panes.
        /// </summary>
        /// <param name="configure">Action used to add panes.</param>
        /// <param name="options">Optional layout rendering options.</param>
        void Layout(Action<LayoutEntry> configure, LayoutOptions options = null);

        /// <summary>
        /// Renders text with responsive padding.
        /// </summary>
        /// <param name="content">The text content to pad.</param>
        /// <param name="options">Padding options.</param>
        void Pad(string content, PadOptions options);

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
        void Pad(
            string content,
            int left = 0,
            int right = 0,
            int top = 0,
            int bottom = 0,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null);

        /// <summary>
        /// Captures a render action and renders the captured text with responsive padding.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="options">Padding options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        void Pad(Action render, PadOptions options = null, int? captureWidth = null, int captureHeight = 200);

        /// <summary>
        /// Captures a renderable and renders the captured text with responsive padding.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="options">Padding options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        void Pad(IConsoulRenderable renderable, PadOptions options = null, int? captureWidth = null, int captureHeight = 200);

        /// <summary>
        /// Renders text aligned inside a responsive width.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="options">Alignment options.</param>
        void Align(string content, AlignOptions options);

        /// <summary>
        /// Renders text aligned inside a responsive width.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="alignment">Alignment used for content lines.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        void Align(
            string content,
            TextBlockAlignment alignment = TextBlockAlignment.Left,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null);

        /// <summary>
        /// Captures a render action and renders the captured text aligned inside a responsive width.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="options">Alignment options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        void Align(Action render, AlignOptions options = null, int? captureWidth = null, int captureHeight = 200);

        /// <summary>
        /// Captures a renderable and renders the captured text aligned inside a responsive width.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="options">Alignment options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        void Align(IConsoulRenderable renderable, AlignOptions options = null, int? captureWidth = null, int captureHeight = 200);

        /// <summary>
        /// Renders wrapped, aligned, and padded text.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="options">Text block options.</param>
        void TextBlock(string content, TextBlockOptions options);

        /// <summary>
        /// Renders wrapped, aligned, and padded text.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="alignment">Alignment used for wrapped lines.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        void TextBlock(
            string content,
            int? width = null,
            TextBlockAlignment alignment = TextBlockAlignment.Left,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null);

        /// <summary>
        /// Renders a responsive, colorized terminal path.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="options">Path rendering options.</param>
        void TextPath(string path, TextPathOptions options);

        /// <summary>
        /// Renders a responsive, colorized terminal path.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="directoryColor">Optional directory segment color.</param>
        /// <param name="fileNameColor">Optional file-name color.</param>
        /// <param name="separatorColor">Optional separator color.</param>
        void TextPath(
            string path,
            int? width = null,
            ConsoleColor? directoryColor = null,
            ConsoleColor? fileNameColor = null,
            ConsoleColor? separatorColor = null);

        /// <summary>
        /// Renders a responsive, colorized terminal path using Spectre-style style text.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="directoryStyle">Optional directory segment style.</param>
        /// <param name="fileNameStyle">Optional file-name style.</param>
        /// <param name="separatorStyle">Optional separator style.</param>
        void TextPath(
            string path,
            int? width,
            string directoryStyle,
            string fileNameStyle = null,
            string separatorStyle = null);

        /// <summary>
        /// Renders a responsive, colorized terminal path using Spectre-style style text.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="directoryStyle">Optional directory segment style.</param>
        /// <param name="fileNameStyle">Optional file-name style.</param>
        /// <param name="separatorStyle">Optional separator style.</param>
        void TextPathStyled(
            string path,
            int? width = null,
            string directoryStyle = null,
            string fileNameStyle = null,
            string separatorStyle = null);

        /// <summary>
        /// Renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="items">The breadcrumb items to render.</param>
        void Breadcrumb(params string[] items);

        /// <summary>
        /// Renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="items">The breadcrumb items to render.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        void Breadcrumb(IEnumerable<string> items, BreadcrumbOptions options = null);

        /// <summary>
        /// Renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="items">The breadcrumb items to render.</param>
        void Breadcrumb(BreadcrumbOptions options, params string[] items);

        /// <summary>
        /// Configures and renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="configure">Action used to add breadcrumb items.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        void Breadcrumb(Action<BreadcrumbEntry> configure, BreadcrumbOptions options = null);

        /// <summary>
        /// Creates responsive breadcrumb text without writing to the console.
        /// </summary>
        /// <param name="items">The breadcrumb items.</param>
        /// <returns>A bounded breadcrumb string.</returns>
        string BreadcrumbText(params string[] items);

        /// <summary>
        /// Creates responsive breadcrumb text without writing to the console.
        /// </summary>
        /// <param name="items">The breadcrumb items.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded breadcrumb string.</returns>
        string BreadcrumbText(IEnumerable<string> items, BreadcrumbOptions options = null, int? width = null);

        /// <summary>
        /// Creates responsive breadcrumb text without writing to the console.
        /// </summary>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="items">The breadcrumb items.</param>
        /// <returns>A bounded breadcrumb string.</returns>
        string BreadcrumbText(BreadcrumbOptions options, params string[] items);

        /// <summary>
        /// Renders a responsive shortcut bar.
        /// </summary>
        /// <param name="items">The shortcut items to render.</param>
        void ShortcutBar(params ShortcutItem[] items);

        /// <summary>
        /// Renders a responsive shortcut bar.
        /// </summary>
        /// <param name="items">The shortcut items to render.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        void ShortcutBar(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null);

        /// <summary>
        /// Renders a responsive shortcut bar.
        /// </summary>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="items">The shortcut items to render.</param>
        void ShortcutBar(ShortcutBarOptions options, params ShortcutItem[] items);

        /// <summary>
        /// Configures and renders a responsive shortcut bar.
        /// </summary>
        /// <param name="configure">Action used to add shortcut items.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        void ShortcutBar(Action<ShortcutBarEntry> configure, ShortcutBarOptions options = null);

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="items">The shortcut items.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        string ShortcutBarText(params ShortcutItem[] items);

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="items">The shortcut items.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        string ShortcutBarText(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null, int? width = null);

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="items">The shortcut items.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        string ShortcutBarText(ShortcutBarOptions options, params ShortcutItem[] items);

        /// <summary>
        /// Renders a responsive checklist.
        /// </summary>
        /// <param name="items">The checklist items to render.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        void Checklist(IEnumerable<ChecklistItem> items, ChecklistOptions options = null);

        /// <summary>
        /// Renders a responsive checklist.
        /// </summary>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="items">The checklist items to render.</param>
        void Checklist(ChecklistOptions options, params ChecklistItem[] items);

        /// <summary>
        /// Configures and renders a responsive checklist.
        /// </summary>
        /// <param name="configure">Action used to add checklist items.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        void Checklist(Action<ChecklistEntry> configure, ChecklistOptions options = null);

        /// <summary>
        /// Renders a responsive list of labeled values.
        /// </summary>
        /// <param name="items">The property list items to render.</param>
        /// <param name="options">Optional property list rendering options.</param>
        void PropertyList(IEnumerable<PropertyListItem> items, PropertyListOptions options = null);

        /// <summary>
        /// Renders readable model properties as a responsive property list.
        /// </summary>
        /// <param name="model">The model object whose properties should be summarized.</param>
        /// <param name="options">Optional property list rendering options.</param>
        void PropertyList(object model, PropertyListOptions options = null);

        /// <summary>
        /// Configures and renders a responsive property list.
        /// </summary>
        /// <param name="configure">Action used to add property list items.</param>
        /// <param name="options">Optional property list rendering options.</param>
        void PropertyList(Action<PropertyListEntry> configure, PropertyListOptions options = null);

        /// <summary>
        /// Configures and renders compact responsive metrics.
        /// </summary>
        /// <param name="configure">Action used to add metric items.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        void Metrics(Action<MetricsEntry> configure, MetricsOptions options = null);

        /// <summary>
        /// Renders compact responsive metrics.
        /// </summary>
        /// <param name="items">The metric items to render.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        void Metrics(IEnumerable<MetricItem> items, MetricsOptions options = null);

        /// <summary>
        /// Renders compact responsive metrics.
        /// </summary>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="items">The metric items to render.</param>
        void Metrics(MetricsOptions options, params MetricItem[] items);

        /// <summary>
        /// Configures and renders a responsive horizontal bar chart.
        /// </summary>
        /// <param name="configure">Action used to add chart items.</param>
        /// <param name="options">Optional chart rendering options.</param>
        void BarChart(Action<BarChartEntry> configure, BarChartOptions options = null);

        /// <summary>
        /// Renders a responsive horizontal bar chart.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Optional chart rendering options.</param>
        void BarChart(IEnumerable<BarChartItem> items, BarChartOptions options = null);

        /// <summary>
        /// Configures and renders a responsive breakdown chart.
        /// </summary>
        /// <param name="configure">Action used to add chart items.</param>
        /// <param name="options">Optional chart rendering options.</param>
        void BreakdownChart(Action<BreakdownChartEntry> configure, BreakdownChartOptions options = null);

        /// <summary>
        /// Renders a responsive breakdown chart.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Optional chart rendering options.</param>
        void BreakdownChart(IEnumerable<BreakdownChartItem> items, BreakdownChartOptions options = null);

        /// <summary>
        /// Renders a compact responsive sparkline chart.
        /// </summary>
        /// <param name="values">The values rendered by the sparkline.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        void Sparkline(IEnumerable<double> values, SparklineOptions options = null);

        /// <summary>
        /// Renders a compact responsive sparkline chart.
        /// </summary>
        /// <param name="values">The values rendered by the sparkline.</param>
        /// <param name="label">Optional label rendered before the sparkline.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="color">Optional sparkline foreground color.</param>
        /// <param name="showRange">Whether the rendered value range should appear after the sparkline.</param>
        void Sparkline(
            IEnumerable<double> values,
            string label = null,
            int? width = null,
            ConsoleColor? color = null,
            bool showRange = false);

        /// <summary>
        /// Configures and renders a compact responsive sparkline chart.
        /// </summary>
        /// <param name="configure">Action used to add values.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        void Sparkline(Action<SparklineEntry> configure, SparklineOptions options = null);

        /// <summary>
        /// Creates a status builder that can start status scopes with a Spectre-style workflow.
        /// </summary>
        /// <returns>A status builder.</returns>
        StatusBuilder Status();

        /// <summary>
        /// Creates an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <returns>The status scope.</returns>
        StatusScope Status(string message, StatusOptions options = null);

        /// <summary>
        /// Runs an action inside an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The scoped action.</param>
        /// <param name="options">Optional status rendering options.</param>
        void Status(string message, Action<StatusScope> action, StatusOptions options = null);

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        Task StatusAsync(string message, Func<StatusScope, CancellationToken, Task> action, StatusOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a spinner builder that can start spinner scopes with a Spectre-style workflow.
        /// </summary>
        /// <returns>A spinner builder.</returns>
        SpinnerBuilder Spinner();

        /// <summary>
        /// Creates an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <returns>The spinner scope.</returns>
        SpinnerScope Spinner(string message, SpinnerOptions options = null);

        /// <summary>
        /// Runs an action inside an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The scoped action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        void Spinner(string message, Action<SpinnerScope> action, SpinnerOptions options = null);

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        Task SpinnerAsync(string message, Func<SpinnerScope, CancellationToken, Task> action, SpinnerOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a multi-task progress context.
        /// </summary>
        /// <param name="options">Optional progress rendering options.</param>
        /// <returns>The progress context.</returns>
        ProgressContext Progress(ProgressContextOptions options = null);

        /// <summary>
        /// Runs an action inside a multi-task progress context.
        /// </summary>
        /// <param name="action">The scoped progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        void Progress(Action<ProgressContext> action, ProgressContextOptions options = null);

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context.
        /// </summary>
        /// <param name="action">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        Task ProgressAsync(Func<ProgressContext, CancellationToken, Task> action, ProgressContextOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a live display context.
        /// </summary>
        /// <param name="options">Optional live display options.</param>
        /// <returns>The live display context.</returns>
        LiveDisplayContext Live(LiveDisplayOptions options = null);

        /// <summary>
        /// Creates a live display context with an initial render target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="options">Optional live display options.</param>
        /// <returns>The live display context.</returns>
        LiveDisplayContext Live(IConsoulRenderable target, LiveDisplayOptions options = null);

        /// <summary>
        /// Runs an action inside a live display context.
        /// </summary>
        /// <param name="action">The scoped live display action.</param>
        /// <param name="options">Optional live display options.</param>
        void Live(Action<LiveDisplayContext> action, LiveDisplayOptions options = null);

        /// <summary>
        /// Runs an action inside a live display context with an initial render target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The scoped live display action.</param>
        /// <param name="options">Optional live display options.</param>
        void Live(IConsoulRenderable target, Action<LiveDisplayContext> action, LiveDisplayOptions options = null);

        /// <summary>
        /// Runs an asynchronous action inside a live display context.
        /// </summary>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        Task LiveAsync(Func<LiveDisplayContext, CancellationToken, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        Task LiveAsync(IConsoulRenderable target, Func<LiveDisplayContext, CancellationToken, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Captures Consoul output from a render action into a string.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="width">The virtual console width.</param>
        /// <param name="height">The virtual console height.</param>
        /// <returns>The captured text.</returns>
        string Capture(Action render, int width = 80, int height = 200);

        /// <summary>
        /// Captures Consoul output from a renderable component into a string.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="width">The virtual console width.</param>
        /// <param name="height">The virtual console height.</param>
        /// <returns>The captured text.</returns>
        string Capture(IConsoulRenderable renderable, int width = 80, int height = 200);
    }
}
