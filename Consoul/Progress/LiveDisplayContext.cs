using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Maintains a refreshable live display area for dashboards and model-backed progress views.
    /// </summary>
    public class LiveDisplayContext : IDisposable
    {
        private readonly LiveDisplayOptions _options;
        private IConsoulRenderable _target;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveDisplayContext"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the live display.</param>
        public LiveDisplayContext(LiveDisplayOptions options = null)
        {
            _options = options != null ? options.Clone() : new LiveDisplayOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveDisplayContext"/> class with an initial render target.
        /// </summary>
        /// <param name="target">The renderable target to refresh.</param>
        /// <param name="options">Optional rendering options for the live display.</param>
        public LiveDisplayContext(IConsoulRenderable target, LiveDisplayOptions options = null)
            : this(options)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        /// <summary>
        /// Gets the rendering options used by this live display.
        /// </summary>
        public LiveDisplayOptions Options => _options;

        /// <summary>
        /// Gets the current render target, if one was configured.
        /// </summary>
        public IConsoulRenderable Target => _target;

        /// <summary>
        /// Gets the number of refreshes rendered by this live display.
        /// </summary>
        public int RefreshCount { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the live display has been completed.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Refreshes the live display by clearing the display and running the supplied render action.
        /// </summary>
        /// <param name="render">Render action that writes the current dashboard or model state.</param>
        /// <returns>The current live display context.</returns>
        public LiveDisplayContext Refresh(Action render)
        {
            ThrowIfDisposed();
            if (render == null)
            {
                throw new ArgumentNullException(nameof(render));
            }

            if (_options.ClearBeforeRender)
            {
                Consoul.ConsoleDriver.Clear();
            }

            RenderTitle();
            RenderContent(render);
            RefreshCount++;
            return this;
        }

        /// <summary>
        /// Refreshes the current render target.
        /// </summary>
        /// <returns>The current live display context.</returns>
        public LiveDisplayContext Refresh()
        {
            ThrowIfDisposed();
            if (_target == null)
            {
                throw new InvalidOperationException("A live display target has not been configured.");
            }

            return Refresh(_target.Render);
        }

        /// <summary>
        /// Refreshes the live display by clearing the display and running the supplied render action.
        /// </summary>
        /// <param name="render">Render action that writes the current dashboard or model state.</param>
        /// <returns>The current live display context.</returns>
        public LiveDisplayContext Update(Action render)
        {
            return Refresh(render);
        }

        /// <summary>
        /// Replaces the current render target and refreshes the live display.
        /// </summary>
        /// <param name="target">The new render target.</param>
        /// <returns>The current live display context.</returns>
        public LiveDisplayContext UpdateTarget(IConsoulRenderable target)
        {
            ThrowIfDisposed();
            _target = target ?? throw new ArgumentNullException(nameof(target));
            return Refresh();
        }

        /// <summary>
        /// Replaces the current render target with a text block and refreshes the live display.
        /// </summary>
        /// <param name="text">The text to render.</param>
        /// <returns>The current live display context.</returns>
        public LiveDisplayContext UpdateTarget(string text)
        {
            return UpdateTarget(new Text(text ?? string.Empty));
        }

        /// <summary>
        /// Replaces the current render target with a render action and refreshes the live display.
        /// </summary>
        /// <param name="render">The render action to use as the new target.</param>
        /// <returns>The current live display context.</returns>
        public LiveDisplayContext UpdateTarget(Action render)
        {
            return UpdateTarget(new RenderActionEntry(render));
        }

        /// <summary>
        /// Sets whether the live display is cleared when the context is disposed.
        /// </summary>
        /// <param name="enabled"><c>true</c> to clear on dispose; otherwise, <c>false</c>.</param>
        /// <returns>The current live display context.</returns>
        public LiveDisplayContext AutoClear(bool enabled)
        {
            ThrowIfDisposed();
            _options.ClearOnDispose = enabled;
            return this;
        }

        /// <summary>
        /// Sets the vertical overflow behavior for captured live display targets.
        /// </summary>
        /// <param name="overflow">The overflow behavior to use.</param>
        /// <returns>The current live display context.</returns>
        public LiveDisplayContext Overflow(VerticalOverflow overflow)
        {
            ThrowIfDisposed();
            _options.Overflow = overflow;
            return this;
        }

        /// <summary>
        /// Sets which side of overflowing content should be cropped.
        /// </summary>
        /// <param name="cropping">The cropping strategy to use.</param>
        /// <returns>The current live display context.</returns>
        public LiveDisplayContext Cropping(VerticalOverflowCropping cropping)
        {
            ThrowIfDisposed();
            _options.Cropping = cropping;
            return this;
        }

        /// <summary>
        /// Runs an operation inside this live display context and disposes the context when the operation completes.
        /// </summary>
        /// <param name="action">The operation to run with the active live display context.</param>
        public void Start(Action<LiveDisplayContext> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (this)
            {
                RefreshInitialTarget();
                action(this);
            }
        }

        /// <summary>
        /// Runs an operation inside this live display context, disposes the context, and returns the operation result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="func">The operation to run with the active live display context.</param>
        /// <returns>The operation result.</returns>
        public T Start<T>(Func<LiveDisplayContext, T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (this)
            {
                RefreshInitialTarget();
                return func(this);
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside this live display context and disposes the context when the operation completes.
        /// </summary>
        /// <param name="action">The operation to run with the active live display context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task StartAsync(Func<LiveDisplayContext, Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return StartAsync((live, cancellationToken) => action(live));
        }

        /// <summary>
        /// Runs an asynchronous operation inside this live display context and disposes the context when the operation completes.
        /// </summary>
        /// <param name="action">The operation to run with the active live display context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StartAsync(Func<LiveDisplayContext, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (this)
            {
                RefreshInitialTarget();
                await action(this, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside this live display context, disposes the context, and returns the operation result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="func">The operation to run with the active live display context.</param>
        /// <returns>A task that yields the operation result.</returns>
        public Task<T> StartAsync<T>(Func<LiveDisplayContext, Task<T>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return StartAsync((live, cancellationToken) => func(live));
        }

        /// <summary>
        /// Runs an asynchronous operation inside this live display context, disposes the context, and returns the operation result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="func">The operation to run with the active live display context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public async Task<T> StartAsync<T>(Func<LiveDisplayContext, CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (this)
            {
                RefreshInitialTarget();
                return await func(this, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Marks the live display as successful and optionally writes a final message.
        /// </summary>
        /// <param name="message">Optional final message to render.</param>
        /// <returns>The current live display context.</returns>
        public LiveDisplayContext Succeed(string message = null)
        {
            Complete(message, _options.SuccessColor);
            return this;
        }

        /// <summary>
        /// Marks the live display as failed and optionally writes a final message.
        /// </summary>
        /// <param name="message">Optional final message to render.</param>
        /// <returns>The current live display context.</returns>
        public LiveDisplayContext Fail(string message = null)
        {
            Complete(message, _options.FailureColor);
            return this;
        }

        /// <summary>
        /// Releases resources used by the live display.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_options.ClearOnDispose)
            {
                Consoul.ConsoleDriver.Clear();
            }
        }

        private void Complete(string message, ConsoleColor color)
        {
            ThrowIfDisposed();
            IsCompleted = true;
            if (!string.IsNullOrEmpty(message))
            {
                Consoul.Write(message, color);
            }
        }

        private void RefreshInitialTarget()
        {
            if (_target != null && RefreshCount == 0)
            {
                Refresh();
            }
        }

        private void RenderContent(Action render)
        {
            if (_options.Overflow == VerticalOverflow.Visible)
            {
                render();
                return;
            }

            int width = Consoul.ConsoleBufferWidth;
            int availableLines = Math.Max(1, Consoul.ConsoleBufferHeight - Consoul.ConsoleDriver.CursorTop);
            string content = Consoul.Capture(render, width, Math.Max(availableLines, 1000));
            string[] lines = ApplyOverflow(content, availableLines);
            WriteCapturedLines(lines);
        }

        private string[] ApplyOverflow(string content, int availableLines)
        {
            string[] lines = SplitLines(content);
            if (lines.Length <= availableLines)
            {
                return lines;
            }

            if (_options.Overflow == VerticalOverflow.Ellipsis)
            {
                return ApplyEllipsis(lines, availableLines);
            }

            return CropLines(lines, availableLines);
        }

        private string[] ApplyEllipsis(string[] lines, int availableLines)
        {
            if (availableLines <= 1)
            {
                return new[] { "..." };
            }

            if (_options.Cropping == VerticalOverflowCropping.Top)
            {
                return new[] { "..." }.Concat(lines.Skip(lines.Length - availableLines + 1)).ToArray();
            }

            return lines.Take(availableLines - 1).Concat(new[] { "..." }).ToArray();
        }

        private string[] CropLines(string[] lines, int availableLines)
        {
            if (_options.Cropping == VerticalOverflowCropping.Top)
            {
                return lines.Skip(lines.Length - availableLines).ToArray();
            }

            return lines.Take(availableLines).ToArray();
        }

        private static string[] SplitLines(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return Array.Empty<string>();
            }

            return content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        private static void WriteCapturedLines(string[] lines)
        {
            for (int index = 0; index < lines.Length; index++)
            {
                Consoul.Write(lines[index], writeLine: index < lines.Length - 1);
            }
        }

        private void RenderTitle()
        {
            if (string.IsNullOrWhiteSpace(_options.Title))
            {
                return;
            }

            if (_options.RenderTitleRule)
            {
                Consoul.Rule(_options.Title, color: _options.TitleColor, alignment: RuleTitleAlignment.Left);
                return;
            }

            Consoul.Write(_options.Title, _options.TitleColor);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LiveDisplayContext));
            }
        }
    }
}
