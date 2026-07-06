using System;
using System.Threading;
using System.Threading.Tasks;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Builds and runs spinner scopes using a Spectre-style <c>Start</c> workflow.
    /// </summary>
    public sealed class SpinnerBuilder
    {
        private readonly SpinnerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpinnerBuilder"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for spinner scopes started by this builder.</param>
        public SpinnerBuilder(SpinnerOptions options = null)
        {
            _options = options != null ? options.Clone() : new SpinnerOptions();
        }

        /// <summary>
        /// Sets whether spinner changes render immediately or wait for an explicit refresh.
        /// </summary>
        /// <param name="enabled"><c>true</c> to render changes immediately; otherwise, <c>false</c>.</param>
        /// <returns>The current builder.</returns>
        public SpinnerBuilder AutoRefresh(bool enabled)
        {
            _options.AutoRefresh = enabled;
            return this;
        }

        /// <summary>
        /// Sets the minimum time between rendered spinner updates.
        /// </summary>
        /// <param name="milliseconds">The refresh rate in milliseconds. Negative values are treated as zero.</param>
        /// <returns>The current builder.</returns>
        public SpinnerBuilder RefreshRate(int milliseconds)
        {
            _options.RefreshRate = RefreshRateHelpers.Normalize(milliseconds);
            return this;
        }

        /// <summary>
        /// Sets the minimum time between rendered spinner updates.
        /// </summary>
        /// <param name="interval">The refresh interval. Negative values are treated as zero.</param>
        /// <returns>The current builder.</returns>
        public SpinnerBuilder RefreshRate(TimeSpan interval)
        {
            return RefreshRate(RefreshRateHelpers.Normalize(interval));
        }

        /// <summary>
        /// Sets the spinner frames used for active messages.
        /// </summary>
        /// <param name="spinner">The spinner frame set to use.</param>
        /// <returns>The current builder.</returns>
        public SpinnerBuilder Spinner(Spinner spinner)
        {
            _options.Frames = spinner?.ToFrameArray();
            return this;
        }

        /// <summary>
        /// Sets the spinner frames used for active messages.
        /// </summary>
        /// <param name="frames">The frames rendered by successive ticks.</param>
        /// <returns>The current builder.</returns>
        public SpinnerBuilder Frames(params string[] frames)
        {
            _options.Frames = frames;
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for active spinner text.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current builder.</returns>
        public SpinnerBuilder SpinnerStyle(Style style)
        {
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            _options.ActiveColor = style.ToForegroundColor(_options.ActiveColor);
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for active spinner text.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current builder.</returns>
        public SpinnerBuilder SpinnerStyle(ConsoleColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active spinner text from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current builder.</returns>
        public SpinnerBuilder SpinnerStyle(SpectreColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active spinner text from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current builder.</returns>
        public SpinnerBuilder SpinnerStyle(RgbColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active spinner text.
        /// </summary>
        /// <param name="style">The style text to parse.</param>
        /// <returns>The current builder.</returns>
        public SpinnerBuilder SpinnerStyle(string style)
        {
            return SpinnerStyle(Style.Parse(style));
        }

        /// <summary>
        /// Creates an in-place spinner scope that the caller can update and dispose.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <returns>The active spinner scope.</returns>
        public SpinnerScope Start(string message)
        {
            return Consoul.Spinner(message, _options);
        }

        /// <summary>
        /// Runs an operation inside an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The operation to run with the active spinner scope.</param>
        public void Start(string message, Action<SpinnerScope> action)
        {
            Consoul.Spinner(message, action, _options);
        }

        /// <summary>
        /// Runs an operation inside an in-place spinner scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="func">The operation to run with the active spinner scope.</param>
        /// <returns>The operation result.</returns>
        public T Start<T>(string message, Func<SpinnerScope, T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (SpinnerScope spinner = Start(message))
            {
                try
                {
                    T result = func(spinner);
                    if (!spinner.IsCompleted)
                    {
                        spinner.Succeed();
                    }

                    return result;
                }
                catch
                {
                    if (!spinner.IsCompleted)
                    {
                        spinner.Fail();
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The operation to run with the active spinner scope.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task StartAsync(string message, Func<SpinnerScope, Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Consoul.SpinnerAsync(message, (spinner, cancellationToken) => action(spinner), _options);
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The operation to run with the active spinner scope.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task StartAsync(string message, Func<SpinnerScope, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            return Consoul.SpinnerAsync(message, action, _options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place spinner scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="func">The operation to run with the active spinner scope.</param>
        /// <returns>A task that yields the operation result.</returns>
        public Task<T> StartAsync<T>(string message, Func<SpinnerScope, Task<T>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return StartAsync(message, (spinner, cancellationToken) => func(spinner));
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place spinner scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="func">The operation to run with the active spinner scope.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public async Task<T> StartAsync<T>(string message, Func<SpinnerScope, CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (SpinnerScope spinner = Start(message))
            {
                try
                {
                    T result = await func(spinner, cancellationToken).ConfigureAwait(false);
                    if (!spinner.IsCompleted)
                    {
                        spinner.Succeed();
                    }

                    return result;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    if (!spinner.IsCompleted)
                    {
                        spinner.Fail("Canceled");
                    }

                    throw;
                }
                catch
                {
                    if (!spinner.IsCompleted)
                    {
                        spinner.Fail();
                    }

                    throw;
                }
            }
        }
    }
}
