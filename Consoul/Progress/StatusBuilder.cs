using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Builds and runs status scopes using a Spectre-style <c>Start</c> workflow.
    /// </summary>
    public sealed class StatusBuilder
    {
        private readonly StatusOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusBuilder"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for status scopes started by this builder.</param>
        public StatusBuilder(StatusOptions options = null)
        {
            _options = options != null ? options.Clone() : new StatusOptions();
        }

        /// <summary>
        /// Sets whether status changes render immediately or wait for an explicit refresh.
        /// </summary>
        /// <param name="enabled"><c>true</c> to render changes immediately; otherwise, <c>false</c>.</param>
        /// <returns>The current builder.</returns>
        public StatusBuilder AutoRefresh(bool enabled)
        {
            _options.AutoRefresh = enabled;
            return this;
        }

        /// <summary>
        /// Sets the minimum time between rendered status updates.
        /// </summary>
        /// <param name="milliseconds">The refresh rate in milliseconds. Negative values are treated as zero.</param>
        /// <returns>The current builder.</returns>
        public StatusBuilder RefreshRate(int milliseconds)
        {
            _options.RefreshRate = RefreshRateHelpers.Normalize(milliseconds);
            return this;
        }

        /// <summary>
        /// Sets the minimum time between rendered status updates.
        /// </summary>
        /// <param name="interval">The refresh interval. Negative values are treated as zero.</param>
        /// <returns>The current builder.</returns>
        public StatusBuilder RefreshRate(TimeSpan interval)
        {
            return RefreshRate(RefreshRateHelpers.Normalize(interval));
        }

        /// <summary>
        /// Sets the spinner frames used for active status messages.
        /// </summary>
        /// <param name="spinner">The spinner frame set to use.</param>
        /// <returns>The current builder.</returns>
        public StatusBuilder Spinner(Spinner spinner)
        {
            _options.Spinner = spinner;
            return this;
        }

        /// <summary>
        /// Sets the spinner frames used for active status messages.
        /// </summary>
        /// <param name="frames">The frames rendered by successive refreshes.</param>
        /// <returns>The current builder.</returns>
        public StatusBuilder Frames(params string[] frames)
        {
            _options.SpinnerFrames = frames;
            return this;
        }

        /// <summary>
        /// Sets the spinner frames used for active status messages.
        /// </summary>
        /// <param name="frames">The frames rendered by successive refreshes.</param>
        /// <returns>The current builder.</returns>
        public StatusBuilder Frames(IEnumerable<string> frames)
        {
            _options.SpinnerFrames = frames == null ? null : frames.ToArray();
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for active status and spinner text.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current builder.</returns>
        public StatusBuilder SpinnerStyle(Style style)
        {
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            _options.ActiveStyle = style;
            _options.ActiveColor = style.ToForegroundColor(_options.ActiveColor);
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for active status and spinner text.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current builder.</returns>
        public StatusBuilder SpinnerStyle(ConsoleColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active status and spinner text from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current builder.</returns>
        public StatusBuilder SpinnerStyle(SpectreColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active status and spinner text from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current builder.</returns>
        public StatusBuilder SpinnerStyle(RgbColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active status and spinner text.
        /// </summary>
        /// <param name="style">The style text to parse.</param>
        /// <returns>The current builder.</returns>
        public StatusBuilder SpinnerStyle(string style)
        {
            return SpinnerStyle(Style.Parse(style));
        }

        /// <summary>
        /// Creates an in-place status scope that the caller can update and dispose.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <returns>The active status scope.</returns>
        public StatusScope Start(string message)
        {
            return Consoul.Status(message, _options);
        }

        /// <summary>
        /// Creates an in-place status context that the caller can update with Spectre-style properties.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <returns>The active status context.</returns>
        public StatusContext StartContext(string message)
        {
            return Start(message).Context;
        }

        /// <summary>
        /// Runs an operation inside an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The operation to run with the active status scope.</param>
        public void Start(string message, Action<StatusScope> action)
        {
            Consoul.Status(message, action, _options);
        }

        /// <summary>
        /// Runs an operation inside an in-place status context with Spectre-style properties.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The operation to run with the active status context.</param>
        public void StartContext(string message, Action<StatusContext> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (StatusScope status = Start(message))
            {
                try
                {
                    action(status.Context);
                    if (!status.IsCompleted)
                    {
                        status.Succeed();
                    }
                }
                catch
                {
                    if (!status.IsCompleted)
                    {
                        status.Fail();
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Runs an operation inside an in-place status context with Spectre-style properties and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The operation to run with the active status context.</param>
        /// <returns>The operation result.</returns>
        public T StartContext<T>(string message, Func<StatusContext, T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (StatusScope status = Start(message))
            {
                try
                {
                    T result = func(status.Context);
                    if (!status.IsCompleted)
                    {
                        status.Succeed();
                    }

                    return result;
                }
                catch
                {
                    if (!status.IsCompleted)
                    {
                        status.Fail();
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Runs an operation inside an in-place status scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The operation to run with the active status scope.</param>
        /// <returns>The operation result.</returns>
        public T Start<T>(string message, Func<StatusScope, T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (StatusScope status = Start(message))
            {
                try
                {
                    T result = func(status);
                    if (!status.IsCompleted)
                    {
                        status.Succeed();
                    }

                    return result;
                }
                catch
                {
                    if (!status.IsCompleted)
                    {
                        status.Fail();
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The operation to run with the active status scope.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task StartAsync(string message, Func<StatusScope, Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Consoul.StatusAsync(message, (status, cancellationToken) => action(status), _options);
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The operation to run with the active status scope.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task StartAsync(string message, Func<StatusScope, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            return Consoul.StatusAsync(message, action, _options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place status context with Spectre-style properties.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The operation to run with the active status context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task StartContextAsync(string message, Func<StatusContext, Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return StartContextAsync(message, (context, cancellationToken) => action(context));
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place status context with Spectre-style properties.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The operation to run with the active status context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StartContextAsync(string message, Func<StatusContext, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (StatusScope status = Start(message))
            {
                try
                {
                    await action(status.Context, cancellationToken).ConfigureAwait(false);
                    if (!status.IsCompleted)
                    {
                        status.Succeed();
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    if (!status.IsCompleted)
                    {
                        status.Fail("Canceled");
                    }

                    throw;
                }
                catch
                {
                    if (!status.IsCompleted)
                    {
                        status.Fail();
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place status context with Spectre-style properties and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The operation to run with the active status context.</param>
        /// <returns>A task that yields the operation result.</returns>
        public Task<T> StartContextAsync<T>(string message, Func<StatusContext, Task<T>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return StartContextAsync(message, (context, cancellationToken) => func(context));
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place status context with Spectre-style properties and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The operation to run with the active status context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public async Task<T> StartContextAsync<T>(string message, Func<StatusContext, CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (StatusScope status = Start(message))
            {
                try
                {
                    T result = await func(status.Context, cancellationToken).ConfigureAwait(false);
                    if (!status.IsCompleted)
                    {
                        status.Succeed();
                    }

                    return result;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    if (!status.IsCompleted)
                    {
                        status.Fail("Canceled");
                    }

                    throw;
                }
                catch
                {
                    if (!status.IsCompleted)
                    {
                        status.Fail();
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place status scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The operation to run with the active status scope.</param>
        /// <returns>A task that yields the operation result.</returns>
        public Task<T> StartAsync<T>(string message, Func<StatusScope, Task<T>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return StartAsync(message, (status, cancellationToken) => func(status));
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place status scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The operation to run with the active status scope.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public async Task<T> StartAsync<T>(string message, Func<StatusScope, CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (StatusScope status = Start(message))
            {
                try
                {
                    T result = await func(status, cancellationToken).ConfigureAwait(false);
                    if (!status.IsCompleted)
                    {
                        status.Succeed();
                    }

                    return result;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    if (!status.IsCompleted)
                    {
                        status.Fail("Canceled");
                    }

                    throw;
                }
                catch
                {
                    if (!status.IsCompleted)
                    {
                        status.Fail();
                    }

                    throw;
                }
            }
        }
    }
}
