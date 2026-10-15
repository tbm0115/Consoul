using ConsoulLibrary.Color;
using ConsoulLibrary.Views;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary {
    /// <summary>
    /// Static extensions to <see cref="Console"/>
    /// </summary>
    public static class Consoul
    {
        /// <summary>
        /// The selection index used by legacy prompt callers to represent an escape or cancellation choice.
        /// </summary>
        [Obsolete("Use PromptResult.IsCanceled instead of sentinel values.")]
        public const int EscapeIndex = -100;

        internal static IConsoleDriver ConsoleDriver { get; private set; } = new SystemConsoleDriver();

        internal static int ConsoleBufferWidth => Math.Max(1, ConsoleDriver.BufferWidth);

        internal static int ConsoleBufferHeight => Math.Max(1, ConsoleDriver.BufferHeight);

        internal static readonly string DefaultReadExitCode = Environment.NewLine;

        /// <summary>
        /// Gets the current terminal buffer width, using Consoul's console driver fallback when the platform console is unavailable.
        /// </summary>
        public static int TerminalWidth => ConsoleBufferWidth;

        /// <summary>
        /// Gets the current terminal buffer height, using Consoul's console driver fallback when the platform console is unavailable.
        /// </summary>
        public static int TerminalHeight => ConsoleBufferHeight;

        /// <summary>
        /// Gets a snapshot of the current terminal profile, including size, redirection, and virtual-driver status.
        /// </summary>
        public static TerminalProfile Terminal => TerminalProfile.FromDriver(ConsoleDriver);

        internal static IDisposable UseConsoleDriver(IConsoleDriver consoleDriver)
        {
            if (consoleDriver == null)
                throw new ArgumentNullException(nameof(consoleDriver));

            var previous = ConsoleDriver;
            ConsoleDriver = consoleDriver;
            _lastKnownWidth = ConsoleBufferWidth;
            return new ConsoleDriverScope(previous);
        }

        /// <summary>
        /// Captures Consoul output from a render action as plain text without writing to the active console.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="width">Virtual console width used while capturing.</param>
        /// <param name="height">Virtual console height used while capturing.</param>
        /// <returns>The plain-text output captured from the render action.</returns>
        public static string Capture(Action render, int width = 80, int height = 200)
        {
            if (render == null)
            {
                throw new ArgumentNullException(nameof(render));
            }

            var driver = new CaptureConsoleDriver(width, height);
            using (UseConsoleDriver(driver))
            {
                render();
            }

            return driver.GetText();
        }

        /// <summary>
        /// Captures Consoul output from a renderable component as plain text without writing to the active console.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="width">Virtual console width used while capturing.</param>
        /// <param name="height">Virtual console height used while capturing.</param>
        /// <returns>The plain-text output captured from the renderable component.</returns>
        public static string Capture(IConsoulRenderable renderable, int width = 80, int height = 200)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return Capture(renderable.Render, width, height);
        }

        /// <summary>
        /// Captures output from a Spectre-style renderable component as plain text without writing to the active console.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="width">Virtual console width used while capturing.</param>
        /// <param name="height">Virtual console height used while capturing.</param>
        /// <returns>The plain-text output captured from the renderable component.</returns>
        public static string Capture(IRenderable renderable, int width = 80, int height = 200)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return Capture(renderable.Render, width, height);
        }

        /// <summary>
        /// Renders a reusable Consoul visual component to the active console driver.
        /// </summary>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Render(IConsoulRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            renderable.Render();
        }

        /// <summary>
        /// Renders a Spectre-style renderable component to the active console driver.
        /// </summary>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Render(IRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            renderable.Render();
        }

        /// <summary>
        /// Wraps a render action as a reusable Consoul visual component.
        /// </summary>
        /// <param name="render">The action that writes content to the active Consoul console driver.</param>
        /// <returns>A renderable component that invokes the supplied action.</returns>
        public static RenderActionEntry Renderable(Action render)
        {
            return new RenderActionEntry(render);
        }

        /// <summary>
        /// Adapts a Spectre-style renderable component to Consoul's reusable renderable contract.
        /// </summary>
        /// <param name="renderable">The renderable component to adapt.</param>
        /// <returns>A Consoul renderable component that invokes the supplied renderable.</returns>
        public static IConsoulRenderable Renderable(IRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return renderable as IConsoulRenderable ?? new RenderActionEntry(renderable.Render);
        }

        private sealed class ConsoleDriverScope : IDisposable
        {
            private readonly IConsoleDriver _previous;
            private bool _disposed;

            public ConsoleDriverScope(IConsoleDriver previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                ConsoleDriver = _previous;
                _lastKnownWidth = ConsoleBufferWidth;
            }
        }

        #region Window Resize Listener
        private static Timer _resizeCheckTimer;
        private static int _lastKnownWidth = ConsoleBufferWidth;
        /// <summary>
        /// Event that is triggered whenever the console window is resized.
        /// </summary>
        public static event EventHandler WindowResized
        {
            add
            {
                if (WindowResizedInternal == null)
                {
                    StartResizeListener();
                }
                WindowResizedInternal += value;
            }
            remove
            {
                WindowResizedInternal -= value;
                if (WindowResizedInternal == null)
                {
                    StopResizeListener();
                }
            }
        }

        private static event EventHandler WindowResizedInternal;

        /// <summary>
        /// Starts a timer to check for window resize events and raises the event if a change is detected.
        /// </summary>
        private static void StartResizeListener()
        {
            if (_resizeCheckTimer == null)
            {
                _resizeCheckTimer = new Timer(CheckForResize, null, 500, 500); // Check every 500 ms
            }
        }

        /// <summary>
        /// Stops the timer that checks for window resize events.
        /// </summary>
        private static void StopResizeListener()
        {
            _resizeCheckTimer?.Dispose();
            _resizeCheckTimer = null;
        }

        /// <summary>
        /// Checks if the console window size has changed.
        /// </summary>
        private static void CheckForResize(object state)
        {
            int newWidth = ConsoleBufferWidth;
            if (newWidth != _lastKnownWidth)
            {
                _lastKnownWidth = newWidth;
                WindowResizedInternal?.Invoke(null, EventArgs.Empty);
            }
        }
        #endregion

        /// <summary>
        /// Waits for the user to press "Enter". Performs Console.ReadLine()
        /// <paramref name="silent">Flags whether or not to show continue message.</paramref>
        /// <paramref name="cancellationToken"/>
        /// </summary>
        public static void Wait(bool silent = false, CancellationToken cancellationToken = default)
        {
            if (!silent)
                Consoul.WriteCore(RenderOptions.ContinueMessage, RenderOptions.SubnoteScheme);
            Read(cancellationToken);
        }

        /// <summary>
        /// Writes a new line as a break
        /// </summary>
        public static void LineBreak()
            => ConsoleDriver.WriteLine(string.Empty);

        /// <summary>
        /// Applies a color theme to Consoul rendering defaults.
        /// </summary>
        /// <param name="theme">The theme to apply.</param>
        public static void ApplyTheme(ConsoulTheme theme)
        {
            RenderOptions.ApplyTheme(theme);
        }

        /// <summary>
        /// Applies a color theme until the returned scope is disposed.
        /// </summary>
        /// <param name="theme">The theme to apply while the scope is active.</param>
        /// <returns>A disposable scope that restores the previous render theme.</returns>
        public static IDisposable UseTheme(ConsoulTheme theme)
        {
            return RenderOptions.UseTheme(theme);
        }

        /// <summary>
        /// Applies scoped render option changes until the returned scope is disposed.
        /// </summary>
        /// <param name="configure">Action that mutates <see cref="RenderOptions"/> for the active scope.</param>
        /// <returns>A disposable scope that restores the previous render options.</returns>
        public static IDisposable UseRenderOptions(Action configure)
        {
            return RenderOptions.Use(configure);
        }

        /// <summary>
        /// Prompts the user to provide a string input.
        /// </summary>
        /// <param name="message">Prompt Message</param>
        /// <param name="color">Override the Prompt Message color</param>
        /// <param name="backgroundColor">Override the Prompt background color</param>
        /// <param name="allowEmpty">Specifies whether the user can provide an empty response. Can result in <see cref="string.Empty"/> .</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User response (string)</returns>
        public static string Input(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            string output;
            bool valid = false;
            do
            {
                Consoul.WriteCore(message, RenderOptions.GetColorOrDefault(color), backgroundColor: RenderOptions.GetBackgroundColorOrDefault(backgroundColor));
                output = Read(cancellationToken);
                if (allowEmpty)
                {
                    valid = true;
                }
                else if (!string.IsNullOrEmpty(output))
                {
                    valid = true;
                }
            } while (!valid);
            return output;
        }

        /// <summary>
        /// Prompts the user to provide a string input and converts it to the expected type.
        /// </summary>
        /// <param name="message">Prompt message</param>
        /// <param name="expectedType">The expected type of the return value</param>
        /// <param name="color">Override the prompt message color</param>
        /// <param name="backgroundColor">Override the prompt background color</param>
        /// <param name="allowEmpty">Specifies whether the user can provide an empty response. Can result in string.Empty</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
        /// <returns>User response converted to the expected type</returns>
        public static object Input(string message, Type expectedType, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            string output = string.Empty;
            bool valid = false;
            object result = null;
            do
            {
                Consoul.WriteCore(message, RenderOptions.GetColorOrDefault(color), backgroundColor: RenderOptions.GetBackgroundColorOrDefault(backgroundColor));
                output = Read(cancellationToken);
                if (allowEmpty && string.IsNullOrEmpty(output))
                {
                    valid = true;
                    result = string.Empty;
                }
                else if (!string.IsNullOrEmpty(output))
                {
                    try
                    {
                        result = ConvertInputValue(output, expectedType);
                        valid = true;
                    }
                    catch (Exception ex)
                    {
                        Consoul.WriteCore($"Invalid input. Please enter a value of type {expectedType.Name}. Error: {ex.Message}", RenderOptions.InvalidScheme);
                    }
                }
            } while (!valid);

            return result;
        }

        private static object ConvertInputValue(string output, Type expectedType)
        {
            if (expectedType == null)
            {
                return output;
            }

            var nullableType = Nullable.GetUnderlyingType(expectedType);
            var targetType = nullableType ?? expectedType;
            if (nullableType != null && string.IsNullOrEmpty(output))
            {
                return null;
            }

            if (targetType == typeof(string))
            {
                return output;
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, output, ignoreCase: true);
            }

            if (targetType == typeof(Guid))
            {
                return Guid.Parse(output);
            }

            if (targetType == typeof(TimeSpan))
            {
                return TimeSpan.Parse(output, CultureInfo.CurrentCulture);
            }

            if (targetType == typeof(DateTimeOffset))
            {
                return DateTimeOffset.Parse(output, CultureInfo.CurrentCulture);
            }

            if (targetType == typeof(Uri))
            {
                return new Uri(output, UriKind.RelativeOrAbsolute);
            }

            if (targetType == typeof(Version))
            {
                return Version.Parse(output);
            }

            return Convert.ChangeType(output, targetType, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Prompts the user to provide a string input and converts it to the expected type "T".
        /// </summary>
        /// <typeparam name="T">The expected return type</typeparam>
        /// <param name="message">Prompt message</param>
        /// <param name="color">Override the prompt message color</param>
        /// <param name="backgroundColor">Override the prompt background color</param>
        /// <param name="allowEmpty">Specifies whether the user can provide an empty response. Can result in string.Empty</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
        /// <returns>User response converted to type T</returns>
        public static T Input<T>(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            return (T)Input(message, typeof(T), color: color, backgroundColor: backgroundColor, allowEmpty: allowEmpty, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Creates a typed text prompt with parsing, default value, validation, and cancellation support.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <returns>A configurable <see cref="TextPrompt{T}"/>.</returns>
        public static TextPrompt<T> Text<T>(string message, bool clear = false)
        {
            return new TextPrompt<T>(message, clear);
        }

        /// <summary>
        /// Creates a text prompt configured for file paths, including pasted-quote trimming and optional existence checks.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="mustExist">Indicates whether the entered path must reference an existing file.</param>
        /// <returns>A configurable <see cref="TextPrompt{T}"/> for file path input.</returns>
        public static TextPrompt<string> FilePath(string message, bool clear = false, bool mustExist = false)
        {
            return CreatePathPrompt(
                message,
                clear,
                mustExist,
                File.Exists,
                "File path is required.",
                "File does not exist.");
        }

        /// <summary>
        /// Creates a text prompt configured for directory paths, including pasted-quote trimming and optional existence checks.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="mustExist">Indicates whether the entered path must reference an existing directory.</param>
        /// <returns>A configurable <see cref="TextPrompt{T}"/> for directory path input.</returns>
        public static TextPrompt<string> DirectoryPath(string message, bool clear = false, bool mustExist = false)
        {
            return CreatePathPrompt(
                message,
                clear,
                mustExist,
                Directory.Exists,
                "Directory path is required.",
                "Directory does not exist.");
        }

        /// <summary>
        /// Creates a masked secret prompt for passwords, API keys, and other sensitive values.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <returns>A configurable <see cref="SecretPrompt"/>.</returns>
        public static SecretPrompt Secret(string message, bool clear = false)
        {
            return new SecretPrompt(message, clear);
        }

        /// <summary>
        /// Displays a masked secret prompt and returns a result describing the entered value or cancellation state.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="allowEmpty">Indicates whether empty input is a valid secret value.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="SecretResult"/> describing the secret value or cancellation state.</returns>
        public static SecretResult ReadSecret(string message, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            var prompt = new SecretPrompt(message, clear)
            {
                AllowEmpty = allowEmpty
            };

            return prompt.Render(cancellationToken);
        }

        /// <summary>
        /// Creates a confirmation prompt with yes/no parsing, defaults, and cancellation support.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <returns>A configurable <see cref="ConfirmPrompt"/>.</returns>
        public static ConfirmPrompt Confirmation(string message, bool clear = false)
        {
            return new ConfirmPrompt(message, clear);
        }

        /// <summary>
        /// Displays a confirmation prompt and returns a result describing the selected value or cancellation state.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="defaultValue">Optional default value used when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ConfirmResult"/> describing the confirmed value or cancellation state.</returns>
        public static ConfirmResult Confirm(string message, bool clear = false, bool? defaultValue = null, CancellationToken cancellationToken = default)
        {
            var prompt = new ConfirmPrompt(message, clear);
            if (defaultValue.HasValue)
            {
                prompt.WithDefault(defaultValue.Value);
            }

            return prompt.Render(cancellationToken);
        }

        /// <summary>
        /// Internal <see cref="Console.WriteLine()"/> wrapper
        /// </summary>
        /// <param name="message">Display message</param>
        /// <param name="color">Color for Message. Defaults to <see cref="RenderOptions.DefaultColor"/></param>
        /// <param name="backgroundColor">Color for the background. Defaults to the background color from <see cref="RenderOptions.DefaultScheme"/>.</param>
        /// <param name="writeLine">Specifies whether to use <see cref="Console.WriteLine()"/> or <see cref="Console.Write(string)"/></param>
        internal static void WriteCore(string message, ConsoleColor color, ConsoleColor? backgroundColor = null, bool writeLine = true)
        {
            using (var colors = new ColorScope(color, backgroundColor))
            {
                if (writeLine)
                {
                    ConsoleDriver.WriteLine(message);
                }
                else
                {
                    ConsoleDriver.Write(message);
                }
            }
        }

        /// <summary>
        /// Internal <see cref="Console.WriteLine()"/> wrapper
        /// </summary>
        /// <param name="message">Display message</param>
        /// <param name="colorScheme">Color scheme for Message. Defaults to <see cref="RenderOptions.DefaultColor"/></param>
        /// <param name="writeLine">Specifies whether to use <see cref="Console.WriteLine()"/> or <see cref="Console.Write(string)"/></param>
        internal static void WriteCore(string message, ColorScheme? colorScheme = null, bool writeLine = true)
        {
            if (colorScheme == null)
                colorScheme = RenderOptions.DefaultScheme;
            WriteCore(message, colorScheme.Value.Color, backgroundColor: colorScheme.Value.BackgroundColor, writeLine: writeLine);
        }

        internal static void WriteControl(string value)
        {
            ConsoleDriver.WriteControl(value ?? string.Empty);
        }

        /// <summary>
        /// Writes a message to the <see cref="Console"/>. Rendering depends on <see cref="RenderOptions.WriteMode"/>
        /// </summary>
        /// <param name="message"><inheritdoc cref="WriteCore(string, ConsoleColor, ConsoleColor?, bool)" path="/param[@name='message']"/></param>
        /// <param name="color"><inheritdoc cref="WriteCore(string, ConsoleColor, ConsoleColor?, bool)" path="/param[@name='color']"/></param>
        /// <param name="backgroundColor"><inheritdoc cref="WriteCore(string, ConsoleColor, ConsoleColor?, bool)" path="/param[@name='backgroundColor']"/></param>
        /// <param name="writeLine"><inheritdoc cref="WriteCore(string, ConsoleColor, ConsoleColor?, bool)" path="/param[@name='writeLine']"/></param>
        public static void Write(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool writeLine = true)
        {
            switch (RenderOptions.WriteMode)
            {
                case RenderOptions.WriteModes.SuppressAll:
                    // Do nothing
                    break;
                case RenderOptions.WriteModes.SuppressBlacklist:
                    if (!RenderOptions.BlacklistColors.Any(c => c == color))
                        WriteCore(message, color: RenderOptions.GetColorOrDefault(color), backgroundColor: RenderOptions.GetBackgroundColorOrDefault(backgroundColor), writeLine: writeLine);
                    break;
                default: // Include WriteAll
                    WriteCore(message, color: RenderOptions.GetColorOrDefault(color), backgroundColor: RenderOptions.GetBackgroundColorOrDefault(backgroundColor), writeLine: writeLine);
                    break;
            }
        }

        /// <summary>
        /// Writes a message to the <see cref="Console"/>. Rendering depends on <see cref="RenderOptions.WriteMode"/>
        /// </summary>
        /// <param name="message"><inheritdoc cref="WriteCore(string, ColorScheme?, bool)" path="/param[@name='message']"/></param>
        /// <param name="colorScheme"><inheritdoc cref="WriteCore(string, ColorScheme?, bool)" path="/param[@name='colorScheme']"/></param>
        /// <param name="writeLine"><inheritdoc cref="WriteCore(string, ColorScheme?, bool)" path="/param[@name='writeLine']"/></param>
        public static void Write(string message, ColorScheme colorScheme, bool writeLine = true)
            => WriteCore(message, colorScheme, writeLine);

        private static readonly Regex _argumentReplacer = new Regex(@"\{(?<PropertyName>\w+)(:(?<Color>\w+))?\}");
        /// <summary>
        /// Writes a formatted string to the console with flexible color formatting.
        /// </summary>
        /// <param name="template">The template string containing placeholders in the form "{PropertyName:Color}".</param>
        /// <param name="color">Optional foreground color for the rendered text.</param>
        /// <param name="backgroundColor">Optional background color for the rendered text.</param>
        /// <param name="writeLine">Indicates whether to append a newline after writing the message.</param>
        /// <param name="args">The values for the placeholders in the template.</param>
        public static void Write(string template, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool writeLine = true, params object[] args)
        {
            MatchCollection matches = _argumentReplacer.Matches(template);

            var values = new Dictionary<string, object>();

            int currentIndex = 0;
            foreach (Match match in matches)
            {
                if (match.Index > currentIndex)
                {
                    Consoul.Write(template.Substring(currentIndex, match.Index - currentIndex), color: color, backgroundColor: backgroundColor, writeLine: false);
                }

                string propertyName = match.Groups["PropertyName"].Value;
                string colorName = match.Groups["Color"].Success ? match.Groups["Color"].Value : null;

                object value;
                if (!values.TryGetValue(propertyName, out value))
                {
                    values[propertyName] = null;
                    value = null;
                }

                if (value == null)
                {
                    foreach (var arg in args)
                    {
                        var prop = arg.GetType().GetProperty(propertyName);
                        if (prop != null)
                        {
                            value = prop.GetValue(arg);
                            values[propertyName] = value;
                            break;
                        }
                    }

                    if (value == null && values.Count > 0 && values.Count <= args.Length)
                    {
                        values[propertyName] = args[values.Count - 1];
                        value = values[propertyName];
                    }
                }

                ConsoleColor subtextColor;
                if (string.IsNullOrEmpty(colorName) || !Enum.TryParse(colorName, true, out subtextColor))
                {
                    subtextColor = color ?? RenderOptions.GetColorOrDefault();
                }

                if (value != null)
                {
                    Consoul.Write(value.ToString(), color: subtextColor, backgroundColor: backgroundColor, writeLine: false);
                }
                else
                {
                    Consoul.Write(match.Value, color: subtextColor, backgroundColor: backgroundColor, writeLine: false);
                }

                currentIndex = match.Index + match.Length;
            }

            if (currentIndex < template.Length)
            {
                Consoul.Write(template.Substring(currentIndex), color: color, backgroundColor: backgroundColor, writeLine: writeLine);
            }
            else if (writeLine)
            {
                Consoul.Write(string.Empty);
            }
        }

        /// <summary>
        /// Writes a formatted message for exceptions.
        /// </summary>
        /// <param name="ex">Exception to be written</param>
        /// <param name="message">Optional context message to render before the exception details.</param>
        /// <param name="includeStackTrace">Flag to include stack trace in output</param>
        /// <param name="color">Optional foreground color for the rendered exception text.</param>
        /// <param name="backgroundColor">Optional background color for the rendered exception text.</param>
        public static void Write(Exception ex, string message = null, bool includeStackTrace = true, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            if (!string.IsNullOrEmpty(message))
                Write(message);
            Write(ex.Message, RenderOptions.InvalidScheme);
            if (includeStackTrace)
                Write($"\t{ex.StackTrace}", RenderOptions.InvalidScheme);
            if (ex.InnerException != null)
                Write(ex.InnerException, null, includeStackTrace, color: color, backgroundColor: backgroundColor);
        }

        /// <summary>
        /// Writes a structured, responsive diagnostic panel for an exception.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="options">Rendering options for the exception panel.</param>
        public static void WriteException(Exception exception, ExceptionRenderOptions options)
        {
            WriteException(exception, null, options);
        }

        /// <summary>
        /// Writes a structured, responsive diagnostic panel for an exception using migration-friendly formatting flags.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="format">Formatting flags for the exception panel.</param>
        public static void WriteException(Exception exception, ExceptionFormats format)
        {
            WriteException(exception, null, new ExceptionRenderOptions
            {
                Format = format,
                IncludeStackTrace = !format.HasFlag(ExceptionFormats.NoStackTrace)
            });
        }

        /// <summary>
        /// Writes a structured, responsive diagnostic panel for an exception using Spectre-style settings.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="settings">Settings used to adapt the exception panel.</param>
        public static void WriteException(Exception exception, ExceptionSettings settings)
        {
            WriteException(exception, null, settings != null ? settings.ToRenderOptions() : null);
        }

        /// <summary>
        /// Writes a structured, responsive diagnostic panel for an exception.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="message">Optional context message rendered before the exception details.</param>
        /// <param name="options">Rendering options for the exception panel.</param>
        public static void WriteException(Exception exception, string message = null, ExceptionRenderOptions options = null)
        {
            ExceptionEntry.Render(exception, message, options);
        }

        /// <summary>
        /// Writes lightweight Consoul markup without appending a newline.
        /// </summary>
        /// <param name="markup">Markup text containing color tags such as <c>[green]Ready[/]</c> or <c>[white on blue]Info[/]</c>.</param>
        public static void Markup(string markup)
        {
            MarkupEntry.Render(markup, false);
        }

        /// <summary>
        /// Writes formatted lightweight Consoul markup without appending a newline, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="format">A composite markup format string containing color tags such as <c>[green]{0}[/]</c>.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void Markup(string format, params object[] args)
        {
            MarkupEntry.Render(MarkupEntry.FormatMarkup(format, args), false);
        }

        /// <summary>
        /// Writes formatted lightweight Consoul markup without appending a newline, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite markup format string containing color tags such as <c>[green]{0}[/]</c>.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void Markup(IFormatProvider provider, string format, params object[] args)
        {
            MarkupEntry.Render(MarkupEntry.FormatMarkup(provider, format, args), false);
        }

        /// <summary>
        /// Writes lightweight Consoul markup from an interpolated string without appending a newline, escaping interpolated values.
        /// </summary>
        /// <param name="markup">Interpolated markup text. Markup tags in the format string are honored; interpolated values are escaped.</param>
        public static void MarkupInterpolated(FormattableString markup)
        {
            MarkupEntry.Render(MarkupEntry.FormatInterpolated(markup), false);
        }

        /// <summary>
        /// Writes lightweight Consoul markup and appends a newline.
        /// </summary>
        /// <param name="markup">Markup text containing color tags such as <c>[green]Ready[/]</c> or <c>[white on blue]Info[/]</c>.</param>
        public static void MarkupLine(string markup)
        {
            MarkupEntry.Render(markup, true);
        }

        /// <summary>
        /// Writes formatted lightweight Consoul markup and appends a newline, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="format">A composite markup format string containing color tags such as <c>[green]{0}[/]</c>.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void MarkupLine(string format, params object[] args)
        {
            MarkupEntry.Render(MarkupEntry.FormatMarkup(format, args), true);
        }

        /// <summary>
        /// Writes formatted lightweight Consoul markup and appends a newline, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite markup format string containing color tags such as <c>[green]{0}[/]</c>.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void MarkupLine(IFormatProvider provider, string format, params object[] args)
        {
            MarkupEntry.Render(MarkupEntry.FormatMarkup(provider, format, args), true);
        }

        /// <summary>
        /// Writes lightweight Consoul markup from an interpolated string and appends a newline, escaping interpolated values.
        /// </summary>
        /// <param name="markup">Interpolated markup text. Markup tags in the format string are honored; interpolated values are escaped.</param>
        public static void MarkupLineInterpolated(FormattableString markup)
        {
            MarkupEntry.Render(MarkupEntry.FormatInterpolated(markup), true);
        }

        /// <summary>
        /// Escapes text so square brackets are rendered literally inside Consoul markup.
        /// </summary>
        /// <param name="value">Text to escape for use inside Consoul markup.</param>
        /// <returns>Markup-safe text with literal square brackets escaped.</returns>
        public static string EscapeMarkup(string value)
        {
            return MarkupEntry.EscapeMarkupText(value);
        }

        /// <summary>
        /// Removes recognized Consoul markup tags from text.
        /// </summary>
        /// <param name="value">Markup text to strip.</param>
        /// <returns>Plain text with recognized markup tags removed.</returns>
        public static string RemoveMarkup(string value)
        {
            return MarkupEntry.RemoveMarkupText(value);
        }

        /// <summary>
        /// Writes a compact single-line badge for statuses, tags, and model state.
        /// </summary>
        /// <param name="label">The badge label to render.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional rendering options for the badge.</param>
        /// <param name="writeLine">Indicates whether to append a newline after writing the badge.</param>
        public static void Badge(string label, BadgeKind kind = BadgeKind.Default, BadgeOptions options = null, bool writeLine = true)
        {
            BadgeEntry.Render(label, kind, options, writeLine);
        }

        /// <summary>
        /// Creates a compact single-line badge without writing to the console.
        /// </summary>
        /// <param name="label">The badge label.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional rendering options for the badge.</param>
        /// <param name="maxWidth">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded single-line badge string.</returns>
        public static string BadgeText(string label, BadgeKind kind = BadgeKind.Default, BadgeOptions options = null, int? maxWidth = null)
        {
            return BadgeEntry.Text(label, kind, options, maxWidth);
        }

        /// <summary>
        /// Reads user input from the console.
        /// </summary>
        /// <returns>Response from the user.</returns>
        public static string Read() => Read(DefaultReadExitCode);

        /// <summary>
        /// Waits for user input and reads the user response.
        /// </summary>
        /// <param name="exitCode">Reference to the string that indicates the end of stream.</param>
        /// <returns>Value from the user</returns>
        public static string Read(string exitCode)
        {
            using (var cancelSource = new CancellationTokenSource())
            {
                return Read(cancelSource.Token, exitCode);
            }
        }

        /// <summary>
        /// Asynchronously reads any input from the user and allows the operation to be cancelled at any time.
        /// </summary>
        /// <param name="cancellationToken">Reference to the cancellation token to stop the read operation.</param>
        /// <returns>Response from the user.</returns>
        public static string Read(CancellationToken cancellationToken) => Read(cancellationToken, DefaultReadExitCode);

        /// <summary>
        /// Asynchronously reads any input from the user and allows the operation to be cancelled at any time.
        /// </summary>
        /// <param name="cancellationToken">Reference to the cancellation token to stop the read operation.</param>
        /// <param name="exitCode">Reference to the string that indicates the end of stream.</param>
        /// <returns>Response from the user.</returns>
        public static string Read(CancellationToken cancellationToken, string exitCode)
        {
            RoutineInput input = new RoutineInput();
            if (Routines.HasBuffer())
            {
                input = Routines.Next();

                long delayTicks = 0;
                if (Routines.UseDelays && input.Delay.HasValue)
                    delayTicks = input.Delay.Value.Ticks / 2;
                TimeSpan delay = new TimeSpan(delayTicks);
                if (!string.IsNullOrEmpty(input.Description))
                    Write(input.Description, RenderOptions.SubnoteScheme);

                System.Threading.Thread.Sleep(delay);
                Write(input.Value, ConsoleColor.Cyan);
                System.Threading.Thread.Sleep(delay);
            }
            else
            {
                using (var stream = ConsoleDriver.OpenStandardInput())
                {
                    input.Value = ReadFromStream(stream, ConsoleDriver.InputEncoding, cancellationToken, exitCode);
                }
            }

            string resolvedInput = Routines.ResolveInputValue(input);

            // Check if we should save the input to the Routine Stack
            if (Routines.MonitorInputs)
                Routines.UserInputs.Push(input);

            return resolvedInput;
        }

        internal static string ReadFromStream(Stream stream, Encoding encoding, CancellationToken cancellationToken, string exitCode)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (exitCode == null)
                throw new ArgumentNullException(nameof(exitCode));

            string userInput = string.Empty;
            byte[] data = new byte[1];
            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead;
                using (var readCanceller = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)))
                {
                    try
                    {
                        bytesRead = stream.ReadAsync(data, 0, data.Length, readCanceller.Token).GetAwaiter().GetResult();
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                if (bytesRead <= 0)
                    break;

                userInput += encoding.GetString(data, 0, bytesRead);

                if (TryTrimReadExitCode(userInput, exitCode, out userInput))
                    break;
            }

            return userInput;
        }

        private static bool TryTrimReadExitCode(string userInput, string exitCode, out string trimmedInput)
        {
            if (IsLineTerminatorExitCode(exitCode))
            {
                if (TryTrimSuffix(userInput, "\r\n", out trimmedInput))
                    return true;
                if (TryTrimSuffix(userInput, "\n", out trimmedInput))
                    return true;
            }

            return TryTrimSuffix(userInput, exitCode, out trimmedInput);
        }

        private static bool IsLineTerminatorExitCode(string exitCode) => exitCode == "\r\n" || exitCode == "\n";

        private static bool TryTrimSuffix(string value, string suffix, out string trimmedValue)
        {
            trimmedValue = value;
            if (!value.EndsWith(suffix, StringComparison.Ordinal))
                return false;

            trimmedValue = value.Substring(0, value.Length - suffix.Length);
            return true;
        }

        /// <summary>
        /// Method to read password without showing it in the console
        /// </summary>
        /// <returns>Entered password</returns>
        public static string ReadPassword(ConsoleColor? color = null, CancellationToken cancellationToken = default)
        {
            return ReadPassword(color, '*', cancellationToken);
        }

        /// <summary>
        /// Method to read password without showing typed characters in the console.
        /// </summary>
        /// <param name="color">Optional color for rendered mask characters.</param>
        /// <param name="maskCharacter">Character to render for each typed character. Use <c>'\0'</c> to render no mask characters.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>Entered password.</returns>
        public static string ReadPassword(ConsoleColor? color, char maskCharacter, CancellationToken cancellationToken = default)
        {
            ConsoleColor resolvedColor = RenderOptions.GetColorOrDefault(color);
            return ReadPassword(PromptRenderHelpers.FromColor(resolvedColor), maskCharacter, cancellationToken);
        }

        internal static string ReadPassword(PromptRenderHelpers.StyledText maskStyle, char maskCharacter, CancellationToken cancellationToken = default)
        {
            RoutineInput input = new RoutineInput();
            if (Routines.HasBuffer())
            {
                input = Routines.Next();

                long delayTicks = 0;
                if (Routines.UseDelays && input.Delay.HasValue)
                    delayTicks = input.Delay.Value.Ticks / 2;
                TimeSpan delay = new TimeSpan(delayTicks);
                if (!string.IsNullOrEmpty(input.Description))
                    Write(input.Description, RenderOptions.SubnoteScheme);

                System.Threading.Thread.Sleep(delay);
                if (maskCharacter != '\0' && !string.IsNullOrEmpty(input.Value))
                {
                    PromptRenderHelpers.WriteText(new string(maskCharacter, input.Value.Length), maskStyle, writeLine: false);
                    ConsoleDriver.WriteLine(string.Empty);
                }
                System.Threading.Thread.Sleep(delay);
            }
            else
            {
                string password = "";
                ConsoleKeyInfo key;

                while(!cancellationToken.IsCancellationRequested)
                {
                    key = ConsoleDriver.ReadKey(true);

                    // Ignore any key other than Enter
                    if (key.Key == ConsoleKey.Enter)
                    {
                        //password = password.Substring(0, password.Length - 1);
                        ConsoleDriver.WriteLine(string.Empty);

                        break;
                    }
                     else if (key.Key != ConsoleKey.Backspace)
                    {
                        password += key.KeyChar;
                        if (maskCharacter != '\0')
                        {
                            PromptRenderHelpers.WriteText(maskCharacter.ToString(), maskStyle, writeLine: false);
                        }
                    }
                    else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, password.Length - 1);
                        if (maskCharacter != '\0')
                        {
                            ConsoleDriver.Write("\b \b");
                        }
                    }
                }
                input.Value = password;
            }

            string resolvedInput = Routines.ResolveInputValue(input);

            // Check if we should save the input to the Routine Stack
            if (Routines.MonitorInputs)
                Routines.UserInputs.Push(input);

            return resolvedInput;
        }

        /// <summary>
        /// Renders a Consoul view using the <see cref="ViewRenderer"/> to chain renderings.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IView"/> to render.</typeparam>
        /// <param name="factory">Optional factory method to create the view.</param>
        /// <param name="configure">Optional configuration action to modify the view after creation.</param>
        /// <returns>Reference to the created <see cref="ViewRenderer"/> to chain renderings.</returns>
        public static ViewRenderer Render<T>(Func<IView> factory = null, Action<IView> configure = null) where T : IView
        {
            var renderer = new ViewRenderer();
            return renderer.Render<T>(factory, configure);
        }

        /// <summary>
        /// Renders a Consoul view using the <see cref="ViewRenderer"/> to chain renderings.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IView"/> to render.</typeparam>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <param name="factory">Optional factory method to create the view.</param>
        /// <param name="configure">Optional configuration action to modify the view after creation.</param>
        /// <returns>Reference to the created <see cref="ViewRenderer"/> to chain renderings.</returns>
        public static ViewRenderer Render<T>(CancellationToken cancellationToken, Func<IView> factory = null, Action<IView> configure = null) where T : IView
        {
            var renderer = new ViewRenderer();
            return renderer.Render<T>(cancellationToken, factory, configure);
        }

        /// <summary>
        /// Renders a Consoul view using the <see cref="ViewRenderer"/> to chain renderings.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IView"/> to render.</typeparam>
        /// <param name="factory">Optional factory method to create the view.</param>
        /// <param name="configure">Optional configuration action to modify the view after creation.</param>
        /// <returns>Reference to the created <see cref="ViewRenderer"/> to chain renderings.</returns>
        public static async Task<ViewRenderer> RenderAsync<T>(Func<IView> factory = null, Action<IView> configure = null) where T : IView
        {
            var renderer = new ViewRenderer();
            return await renderer.RenderAsync<T>(factory, configure);
        }

        /// <summary>
        /// Renders a Consoul view asynchronously using the <see cref="ViewRenderer"/> to chain renderings.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IView"/> to render.</typeparam>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <param name="factory">Optional factory method to create the view.</param>
        /// <param name="configure">Optional configuration action to modify the view after creation.</param>
        /// <returns>Reference to the created <see cref="ViewRenderer"/> to chain renderings.</returns>
        public static async Task<ViewRenderer> RenderAsync<T>(CancellationToken cancellationToken, Func<IView> factory = null, Action<IView> configure = null) where T : IView
        {
            var renderer = new ViewRenderer();
            return await renderer.RenderAsync<T>(cancellationToken, factory, configure);
        }

        /// <summary>
        /// Creates a dynamically configured Consoul view for quick menus and proof-of-concept views.
        /// </summary>
        /// <param name="title">The title rendered above the option prompt.</param>
        /// <param name="goBackMessage">Optional label for the generated option that exits the view.</param>
        /// <returns>A <see cref="DynamicView{T}"/> that can be configured with options and navigation entries.</returns>
        public static DynamicView<object> View(string title, string goBackMessage = null)
        {
            return new DynamicView<object>(title, new object(), goBackMessage);
        }

        /// <summary>
        /// Creates a dynamically configured Consoul view backed by a default-constructed strongly typed model.
        /// </summary>
        /// <typeparam name="TModel">The model type owned by the view.</typeparam>
        /// <param name="title">The title rendered above the model content and option prompt.</param>
        /// <param name="goBackMessage">Optional label for the generated option that exits the view.</param>
        /// <returns>A <see cref="DynamicView{T}"/> that owns a new model instance for quick proof-of-concept screens.</returns>
        public static DynamicView<TModel> View<TModel>(string title, string goBackMessage = null) where TModel : new()
        {
            return new DynamicView<TModel>(title, new TModel(), goBackMessage);
        }

        /// <summary>
        /// Creates a dynamically configured Consoul view backed by a strongly typed model.
        /// </summary>
        /// <typeparam name="TModel">The model type owned by the view.</typeparam>
        /// <param name="title">The title rendered above the model content and option prompt.</param>
        /// <param name="model">The model owned by the view.</param>
        /// <param name="goBackMessage">Optional label for the generated option that exits the view.</param>
        /// <returns>A <see cref="DynamicView{T}"/> that can render model content and run model actions.</returns>
        public static DynamicView<TModel> View<TModel>(string title, TModel model, string goBackMessage = null)
        {
            return new DynamicView<TModel>(title, model, goBackMessage);
        }

        /// <summary>
        /// Creates an object editor view for editing a model object.
        /// </summary>
        /// <param name="model">The model object edited by the object editor.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>An <see cref="EditObjectView"/> backed by the supplied model.</returns>
        public static EditObjectView EditObject(
            object model,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return new EditObjectView(model, bindingAttr, enableDocumentEditor);
        }

        /// <summary>
        /// Creates a form-oriented object editor view for editing a model object.
        /// </summary>
        /// <param name="model">The model object edited by the form editor.</param>
        /// <param name="bindingAttr">Binding flags describing the properties exposed for editing.</param>
        /// <param name="enableDocumentEditor">Indicates whether the form document editor option should be available.</param>
        /// <returns>An <see cref="EditObjectView"/> backed by the supplied model.</returns>
        /// <remarks>
        /// This MVC-style convenience method uses the same implementation as <see cref="EditObject(object, BindingFlags, bool)"/>
        /// so form editing, object editing, and legacy JSON-editor aliases stay on one editor stack.
        /// </remarks>
        public static EditObjectView Form(
            object model,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            bool enableDocumentEditor = true)
        {
            return EditObject(model, bindingAttr, enableDocumentEditor);
        }

        /// <summary>
        /// Renders a responsive headline for view titles, demos, and section openers.
        /// </summary>
        /// <param name="text">The headline text to render.</param>
        /// <param name="options">Rendering options for the headline.</param>
        public static void Headline(string text, HeadlineOptions options)
        {
            HeadlineEntry.Render(text, options);
        }

        /// <summary>
        /// Renders a responsive headline for view titles, demos, and section openers.
        /// </summary>
        /// <param name="text">The headline text to render.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for the headline.</param>
        /// <param name="style">The headline rendering style.</param>
        public static void Headline(string text, int? width = null, ConsoleColor? color = null, HeadlineStyle style = HeadlineStyle.Block)
        {
            var options = new HeadlineOptions
            {
                Width = width,
                Style = style
            };

            if (color.HasValue)
            {
                options.Scheme = new ColorScheme
                {
                    Color = color.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            Headline(text, options);
        }

        /// <summary>
        /// Renders large ASCII text for application titles, splash screens, and section headers.
        /// </summary>
        /// <param name="text">The text to render as large ASCII glyphs.</param>
        /// <param name="options">Rendering options for the text.</param>
        public static void FigletText(string text, FigletTextOptions options)
        {
            FigletTextEntry.Render(text, options);
        }

        /// <summary>
        /// Renders large ASCII text for application titles, splash screens, and section headers.
        /// </summary>
        /// <param name="text">The text to render as large ASCII glyphs.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="alignment">Alignment used for each rendered line.</param>
        /// <param name="color">Optional foreground color for the rendered text.</param>
        /// <param name="backgroundColor">Optional background color for the rendered text.</param>
        /// <param name="character">The character used to fill the large text glyphs.</param>
        public static void FigletText(
            string text,
            int? width = null,
            TextBlockAlignment alignment = TextBlockAlignment.Center,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null,
            char character = '#')
        {
            var options = new FigletTextOptions
            {
                Width = width,
                Alignment = alignment,
                Character = character
            };

            if (color.HasValue || backgroundColor.HasValue)
            {
                options.Scheme = new ColorScheme
                {
                    Color = RenderOptions.GetColorOrDefault(color),
                    BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
                };
            }

            FigletText(text, options);
        }

        /// <summary>
        /// Renders a responsive month calendar.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        /// <param name="options">Rendering options for the calendar.</param>
        public static void Calendar(DateTime month, CalendarOptions options = null)
        {
            CalendarEntry.Render(month, options);
        }

        /// <summary>
        /// Renders a responsive month calendar.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="options">Rendering options for the calendar.</param>
        public static void Calendar(int year, int month, CalendarOptions options = null)
        {
            CalendarEntry.Render(year, month, options);
        }

        /// <summary>
        /// Builds and renders a responsive month calendar.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        /// <param name="configure">Action used to mark days before rendering.</param>
        /// <param name="options">Rendering options for the calendar.</param>
        public static void Calendar(DateTime month, Action<CalendarEntry> configure, CalendarOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var calendar = new CalendarEntry(month, options);
            configure(calendar);
            calendar.Render();
        }

        /// <summary>
        /// Builds and renders a responsive month calendar.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="configure">Action used to mark days before rendering.</param>
        /// <param name="options">Rendering options for the calendar.</param>
        public static void Calendar(int year, int month, Action<CalendarEntry> configure, CalendarOptions options = null)
        {
            Calendar(new DateTime(year, month, 1), configure, options);
        }

        /// <summary>
        /// Renders a dynamic table from source items and selected properties.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <param name="items">Source items to render as rows.</param>
        /// <param name="selectors">Property selectors to render as columns. When omitted, public readable scalar properties are used.</param>
        /// <returns>The created <see cref="DynamicTableView{T}"/>.</returns>
        public static DynamicTableView<T> Table<T>(IEnumerable<T> items, params Expression<Func<T, object>>[] selectors) where T : class
        {
            return Table(items, CreateDisplayTableOptions(null), selectors);
        }

        /// <summary>
        /// Renders a dynamic table from source items, render options, and selected properties.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <param name="items">Source items to render as rows.</param>
        /// <param name="options">Rendering options for the table.</param>
        /// <param name="selectors">Property selectors to render as columns. When omitted, public readable scalar properties are used.</param>
        /// <returns>The created <see cref="DynamicTableView{T}"/>.</returns>
        public static DynamicTableView<T> Table<T>(IEnumerable<T> items, TableRenderOptions options, params Expression<Func<T, object>>[] selectors) where T : class
        {
            var table = DynamicTableView<T>.Create(items ?? Enumerable.Empty<T>(), CreateDisplayTableOptions(options), selectors);
            table.RenderInline();
            return table;
        }

        /// <summary>
        /// Renders a dynamic table from source items and custom column configuration.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <param name="items">Source items to render as rows.</param>
        /// <param name="configure">Action used to add columns before rows are rendered.</param>
        /// <param name="options">Rendering options for the table.</param>
        /// <returns>The created <see cref="DynamicTableView{T}"/>.</returns>
        public static DynamicTableView<T> Table<T>(IEnumerable<T> items, Action<DynamicTableView<T>> configure, TableRenderOptions options = null) where T : class
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var table = new DynamicTableView<T>(CreateDisplayTableOptions(options));
            configure(table);
            table.AddRows(items ?? Enumerable.Empty<T>());
            table.RenderInline();
            return table;
        }

        private static TableRenderOptions CreateDisplayTableOptions(TableRenderOptions options)
        {
            if (options != null)
            {
                return options;
            }

            return new TableRenderOptions
            {
                ShowFooterHelp = false
            };
        }

        /// <summary>
        /// Renders content inside a responsive bordered panel.
        /// </summary>
        /// <param name="content">The panel content to render.</param>
        /// <param name="title">Optional title rendered in the top border.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for content text.</param>
        /// <param name="borderColor">Optional foreground color for the panel border.</param>
        public static void Panel(string content, string title = null, int? width = null, ConsoleColor? color = null, ConsoleColor? borderColor = null)
        {
            PanelEntry.Render(content, title, width, color, borderColor);
        }

        /// <summary>
        /// Renders content inside a responsive bordered panel.
        /// </summary>
        /// <param name="content">The panel content to render.</param>
        /// <param name="options">Rendering options for the panel.</param>
        public static void Panel(string content, PanelOptions options)
        {
            new PanelEntry(content, options).Render();
        }

        /// <summary>
        /// Captures a render action and renders the captured output inside a responsive bordered panel.
        /// </summary>
        /// <param name="render">The render action used to produce panel content.</param>
        /// <param name="options">Rendering options for the panel.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Panel(Action render, PanelOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            PanelEntry.Render(render, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a renderable component and renders the captured output inside a responsive bordered panel.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce panel content.</param>
        /// <param name="options">Rendering options for the panel.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Panel(IConsoulRenderable renderable, PanelOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            PanelEntry.Render(renderable, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a render action and renders the captured output inside a responsive bordered panel.
        /// </summary>
        /// <param name="render">The render action used to produce panel content.</param>
        /// <param name="title">Optional title rendered in the top border.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for content text.</param>
        /// <param name="borderColor">Optional foreground color for the panel border.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Panel(
            Action render,
            string title = null,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? borderColor = null,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            var options = new PanelOptions
            {
                Title = title,
                Width = width
            };

            if (color.HasValue)
            {
                options.ContentScheme = new ColorScheme
                {
                    Color = color.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            if (borderColor.HasValue)
            {
                options.BorderScheme = new ColorScheme
                {
                    Color = borderColor.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            Panel(render, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a renderable component and renders the captured output inside a responsive bordered panel.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce panel content.</param>
        /// <param name="title">Optional title rendered in the top border.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for content text.</param>
        /// <param name="borderColor">Optional foreground color for the panel border.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Panel(
            IConsoulRenderable renderable,
            string title = null,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? borderColor = null,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            var options = new PanelOptions
            {
                Title = title,
                Width = width
            };

            if (color.HasValue)
            {
                options.ContentScheme = new ColorScheme
                {
                    Color = color.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            if (borderColor.HasValue)
            {
                options.BorderScheme = new ColorScheme
                {
                    Color = borderColor.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            Panel(renderable, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Renders a responsive intent-colored callout for view messages.
        /// </summary>
        /// <param name="message">The callout message to render.</param>
        /// <param name="kind">The visual intent of the callout.</param>
        /// <param name="options">Optional rendering options for the callout.</param>
        public static void Callout(string message, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null)
        {
            CalloutEntry.Render(message, kind, options);
        }

        /// <summary>
        /// Captures a render action and renders the captured output inside a responsive intent-colored callout.
        /// </summary>
        /// <param name="render">The render action used to produce callout content.</param>
        /// <param name="kind">The visual intent of the callout.</param>
        /// <param name="options">Optional rendering options for the callout.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Callout(Action render, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            CalloutEntry.Render(render, kind, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a renderable component and renders the captured output inside a responsive intent-colored callout.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce callout content.</param>
        /// <param name="kind">The visual intent of the callout.</param>
        /// <param name="options">Optional rendering options for the callout.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Callout(IConsoulRenderable renderable, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            CalloutEntry.Render(renderable, kind, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Validates a model with DataAnnotations-style metadata without rendering output.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <returns>The validation result for the model.</returns>
        public static ValidationSummaryResult ValidateModel(object model, ValidationSummaryOptions options = null)
        {
            return ValidationSummaryEntry.Validate(model, options);
        }

        /// <summary>
        /// Validates a model and renders a responsive validation summary when issues are present.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <returns>The validation result for the model.</returns>
        public static ValidationSummaryResult ValidationSummary(object model, ValidationSummaryOptions options = null)
        {
            return ValidationSummaryEntry.Render(model, options);
        }

        /// <summary>
        /// Renders a responsive horizontal rule for separating sections in a view.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for the rule.</param>
        /// <param name="alignment">Title alignment when a title is supplied.</param>
        public static void Rule(string title = null, int? width = null, ConsoleColor? color = null, RuleTitleAlignment alignment = RuleTitleAlignment.Center)
        {
            RuleEntry.Render(title, width, color, alignment);
        }

        /// <summary>
        /// Renders a responsive horizontal rule for separating sections in a view using Spectre-style alignment vocabulary.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        /// <param name="alignment">Title alignment when a title is supplied.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for the rule.</param>
        public static void Rule(string title, Justify alignment, int? width = null, ConsoleColor? color = null)
        {
            Rule(title, width, color, JustifyMap.ToRuleTitleAlignment(alignment));
        }

        /// <summary>
        /// Renders a responsive horizontal rule for separating sections in a view using a foreground color and Spectre-style alignment vocabulary.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        /// <param name="color">Foreground color for the rule.</param>
        /// <param name="alignment">Title alignment when a title is supplied.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        public static void Rule(string title, ConsoleColor color, Justify alignment, int? width = null)
        {
            Rule(title, width, color, JustifyMap.ToRuleTitleAlignment(alignment));
        }

        /// <summary>
        /// Renders a responsive horizontal rule for separating sections in a view.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        /// <param name="options">Rendering options for the rule.</param>
        public static void Rule(string title, RuleOptions options)
        {
            new RuleEntry(title, options).Render();
        }

        /// <summary>
        /// Renders a responsive tree for nested view content.
        /// </summary>
        /// <param name="root">The root node to render.</param>
        /// <param name="options">Rendering options for the tree.</param>
        public static void Tree(TreeNode root, TreeOptions options = null)
        {
            TreeEntry.Render(root, options);
        }

        /// <summary>
        /// Builds and renders a responsive tree for nested view content.
        /// </summary>
        /// <param name="rootText">The text rendered for the root node.</param>
        /// <param name="configure">Optional action used to add child nodes before rendering.</param>
        /// <param name="options">Rendering options for the tree.</param>
        public static void Tree(string rootText, Action<TreeNode> configure = null, TreeOptions options = null)
        {
            var root = new TreeNode(rootText);
            configure?.Invoke(root);
            Tree(root, options);
        }

        /// <summary>
        /// Renders a responsive timeline for workflow steps, event history, and model state changes.
        /// </summary>
        /// <param name="items">The timeline items to render.</param>
        /// <param name="options">Optional rendering options for the timeline.</param>
        public static void Timeline(IEnumerable<TimelineItem> items, TimelineOptions options = null)
        {
            TimelineEntry.Render(items, options);
        }

        /// <summary>
        /// Renders a responsive timeline for workflow steps, event history, and model state changes.
        /// </summary>
        /// <param name="options">Optional rendering options for the timeline.</param>
        /// <param name="items">The timeline items to render.</param>
        public static void Timeline(TimelineOptions options, params TimelineItem[] items)
        {
            TimelineEntry.Render(items, options);
        }

        /// <summary>
        /// Configures and renders a responsive timeline for workflow steps, event history, and model state changes.
        /// </summary>
        /// <param name="configure">The action used to add timeline items.</param>
        /// <param name="options">Optional rendering options for the timeline.</param>
        public static void Timeline(Action<TimelineEntry> configure, TimelineOptions options = null)
        {
            var timeline = new TimelineEntry(options);
            configure?.Invoke(timeline);
            timeline.Render();
        }

        /// <summary>
        /// Renders a responsive bullet or numbered list for grouped notes, requirements, and dashboard details.
        /// </summary>
        /// <param name="items">The list items to render.</param>
        /// <param name="options">Optional rendering options for the bullet list.</param>
        public static void BulletList(IEnumerable<BulletListItem> items, BulletListOptions options = null)
        {
            BulletListEntry.Render(items, options);
        }

        /// <summary>
        /// Renders a responsive bullet or numbered list for grouped notes, requirements, and dashboard details.
        /// </summary>
        /// <param name="options">Optional rendering options for the bullet list.</param>
        /// <param name="items">The list items to render.</param>
        public static void BulletList(BulletListOptions options, params BulletListItem[] items)
        {
            BulletList((IEnumerable<BulletListItem>)items, options);
        }

        /// <summary>
        /// Configures and renders a responsive bullet or numbered list for grouped notes, requirements, and dashboard details.
        /// </summary>
        /// <param name="configure">The action used to add list items.</param>
        /// <param name="options">Optional rendering options for the bullet list.</param>
        public static void BulletList(Action<BulletListEntry> configure, BulletListOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var bulletList = new BulletListEntry(options);
            configure(bulletList);
            bulletList.Render();
        }

        /// <summary>
        /// Renders responsive columns for view content.
        /// </summary>
        /// <param name="cells">The cell contents to render.</param>
        public static void Grid(params string[] cells)
        {
            Grid((IEnumerable<string>)cells, null);
        }

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="cells">The cell values to render.</param>
        public static void Grid(params object[] cells)
        {
            Grid((IEnumerable<object>)cells, null);
        }

        /// <summary>
        /// Renders responsive columns for view content.
        /// </summary>
        /// <param name="cells">The cell contents to render.</param>
        /// <param name="options">Rendering options for the grid.</param>
        public static void Grid(IEnumerable<string> cells, GridOptions options = null)
        {
            GridEntry.Render(cells, options);
        }

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="cells">The cell values to render.</param>
        /// <param name="options">Rendering options for the grid.</param>
        public static void Grid(IEnumerable<object> cells, GridOptions options = null)
        {
            new GridEntry(cells, options).Render();
        }

        /// <summary>
        /// Renders responsive columns for view content with explicit options.
        /// </summary>
        /// <param name="options">Rendering options for the grid.</param>
        /// <param name="cells">The cell contents to render.</param>
        public static void Grid(GridOptions options, params string[] cells)
        {
            Grid((IEnumerable<string>)cells, options);
        }

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Rendering options for the grid.</param>
        /// <param name="cells">The cell values to render.</param>
        public static void Grid(GridOptions options, params object[] cells)
        {
            Grid((IEnumerable<object>)cells, options);
        }

        /// <summary>
        /// Builds and renders responsive columns for view content.
        /// </summary>
        /// <param name="configure">Action used to add cells before rendering.</param>
        /// <param name="options">Rendering options for the grid.</param>
        public static void Grid(Action<GridEntry> configure, GridOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var grid = new GridEntry(options);
            configure(grid);
            grid.Render();
        }

        /// <summary>
        /// Builds and renders a small ASCII canvas for diagrams, maps, and proof-of-concept visual sketches.
        /// </summary>
        /// <param name="width">The logical canvas width.</param>
        /// <param name="height">The logical canvas height.</param>
        /// <param name="configure">Action used to draw on the canvas before rendering.</param>
        /// <param name="options">Rendering options for the canvas.</param>
        public static void Canvas(int width, int height, Action<CanvasEntry> configure, CanvasOptions options = null)
        {
            CanvasEntry.Render(width, height, configure, options);
        }

        /// <summary>
        /// Renders an existing ASCII canvas.
        /// </summary>
        /// <param name="canvas">The canvas to render.</param>
        public static void Canvas(CanvasEntry canvas)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            canvas.Render();
        }

        /// <summary>
        /// Renders an existing dependency-free terminal canvas image.
        /// </summary>
        /// <param name="image">The image to render.</param>
        public static void CanvasImage(CanvasImageEntry image)
        {
            CanvasImageEntry.Render(image);
        }

        /// <summary>
        /// Renders a dependency-free terminal canvas image from a row-major pixel matrix.
        /// </summary>
        /// <param name="pixels">The source pixels, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional image rendering options.</param>
        public static void CanvasImage(CanvasImagePixel[,] pixels, CanvasImageOptions options = null)
        {
            CanvasImage(new CanvasImageEntry(pixels, options));
        }

        /// <summary>
        /// Renders a dependency-free terminal canvas image from a row-major console color matrix.
        /// </summary>
        /// <param name="pixels">The source colors, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional image rendering options.</param>
        public static void CanvasImage(ConsoleColor[,] pixels, CanvasImageOptions options = null)
        {
            CanvasImage(new CanvasImageEntry(pixels, options));
        }

        /// <summary>
        /// Renders responsive side-by-side text columns for view content.
        /// </summary>
        /// <param name="columns">The column contents to render.</param>
        public static void Columns(params string[] columns)
        {
            Columns((IEnumerable<string>)columns, null);
        }

        /// <summary>
        /// Renders responsive side-by-side columns for mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        public static void Columns(params object[] columns)
        {
            Columns((IEnumerable<object>)columns, null);
        }

        /// <summary>
        /// Renders responsive side-by-side text columns for view content.
        /// </summary>
        /// <param name="columns">The column contents to render.</param>
        /// <param name="options">Rendering options for the columns.</param>
        public static void Columns(IEnumerable<string> columns, ColumnsOptions options = null)
        {
            ColumnsEntry.Render(columns, options);
        }

        /// <summary>
        /// Renders responsive side-by-side columns for mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        /// <param name="options">Rendering options for the columns.</param>
        public static void Columns(IEnumerable<object> columns, ColumnsOptions options = null)
        {
            var entry = new Columns(options);
            entry.AddColumns(columns);
            entry.Render();
        }

        /// <summary>
        /// Renders responsive side-by-side text columns for view content with explicit options.
        /// </summary>
        /// <param name="options">Rendering options for the columns.</param>
        /// <param name="columns">The column contents to render.</param>
        public static void Columns(ColumnsOptions options, params string[] columns)
        {
            Columns((IEnumerable<string>)columns, options);
        }

        /// <summary>
        /// Renders responsive side-by-side columns for mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Rendering options for the columns.</param>
        /// <param name="columns">The column values to render.</param>
        public static void Columns(ColumnsOptions options, params object[] columns)
        {
            Columns((IEnumerable<object>)columns, options);
        }

        /// <summary>
        /// Builds and renders responsive side-by-side text columns for view content.
        /// </summary>
        /// <param name="configure">Action used to add columns before rendering.</param>
        /// <param name="options">Rendering options for the columns.</param>
        public static void Columns(Action<ColumnsEntry> configure, ColumnsOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var columns = new ColumnsEntry(options);
            configure(columns);
            columns.Render();
        }

        /// <summary>
        /// Renders vertically stacked rows for view content.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        public static void Rows(params string[] rows)
        {
            Rows((IEnumerable<string>)rows, null);
        }

        /// <summary>
        /// Renders vertically stacked rows for mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        public static void Rows(params object[] rows)
        {
            Rows((IEnumerable<object>)rows, null);
        }

        /// <summary>
        /// Renders vertically stacked rows for view content.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        /// <param name="options">Rendering options for the rows.</param>
        public static void Rows(IEnumerable<string> rows, RowsOptions options = null)
        {
            RowsEntry.Render(rows, options);
        }

        /// <summary>
        /// Renders vertically stacked rows for mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        /// <param name="options">Rendering options for the rows.</param>
        public static void Rows(IEnumerable<object> rows, RowsOptions options = null)
        {
            var entry = new Rows(options);
            entry.AddRows(rows);
            entry.Render();
        }

        /// <summary>
        /// Renders vertically stacked rows for view content with explicit options.
        /// </summary>
        /// <param name="options">Rendering options for the rows.</param>
        /// <param name="rows">The row contents to render.</param>
        public static void Rows(RowsOptions options, params string[] rows)
        {
            Rows((IEnumerable<string>)rows, options);
        }

        /// <summary>
        /// Renders vertically stacked rows for mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Rendering options for the rows.</param>
        /// <param name="rows">The row values to render.</param>
        public static void Rows(RowsOptions options, params object[] rows)
        {
            Rows((IEnumerable<object>)rows, options);
        }

        /// <summary>
        /// Builds and renders vertically stacked rows for view content.
        /// </summary>
        /// <param name="configure">Action used to add rows before rendering.</param>
        /// <param name="options">Rendering options for the rows.</param>
        public static void Rows(Action<RowsEntry> configure, RowsOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var rows = new RowsEntry(options);
            configure(rows);
            rows.Render();
        }

        /// <summary>
        /// Renders responsive layout panes for dashboard-style view content.
        /// </summary>
        /// <param name="panes">The panes to render.</param>
        /// <param name="options">Rendering options for the layout.</param>
        public static void Layout(IEnumerable<LayoutPane> panes, LayoutOptions options = null)
        {
            LayoutEntry.Render(panes, options);
        }

        /// <summary>
        /// Builds and renders responsive layout panes for dashboard-style view content.
        /// </summary>
        /// <param name="configure">Action used to add panes before rendering.</param>
        /// <param name="options">Rendering options for the layout.</param>
        public static void Layout(Action<LayoutEntry> configure, LayoutOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var layout = new LayoutEntry(options);
            configure(layout);
            layout.Render();
        }

        /// <summary>
        /// Renders text with responsive padding.
        /// </summary>
        /// <param name="content">The text content to pad.</param>
        /// <param name="options">Rendering options for the padded content.</param>
        public static void Pad(string content, PadOptions options)
        {
            PadEntry.Render(content, options);
        }

        /// <summary>
        /// Renders text with responsive padding.
        /// </summary>
        /// <param name="content">The text content to pad.</param>
        /// <param name="left">Spaces added to the left of each content line.</param>
        /// <param name="right">Spaces added to the right of each content line.</param>
        /// <param name="top">Blank lines added before the content.</param>
        /// <param name="bottom">Blank lines added after the content.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color used for padded content.</param>
        /// <param name="backgroundColor">Optional background color used for padded content.</param>
        public static void Pad(
            string content,
            int left = 0,
            int right = 0,
            int top = 0,
            int bottom = 0,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null)
        {
            var options = new PadOptions
            {
                Width = width,
                Left = left,
                Right = right,
                Top = top,
                Bottom = bottom
            };

            if (color.HasValue || backgroundColor.HasValue)
            {
                options.Scheme = new ColorScheme
                {
                    Color = RenderOptions.GetColorOrDefault(color),
                    BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
                };
            }

            Pad(content, options);
        }

        /// <summary>
        /// Captures a render action and renders the captured text with responsive padding.
        /// </summary>
        /// <param name="render">The render action used to produce padded content.</param>
        /// <param name="options">Rendering options for the padded content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Pad(Action render, PadOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            PadEntry.Render(render, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a renderable component and renders the captured text with responsive padding.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce padded content.</param>
        /// <param name="options">Rendering options for the padded content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Pad(IConsoulRenderable renderable, PadOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            PadEntry.Render(renderable, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a render action and renders the captured text with responsive padding.
        /// </summary>
        /// <param name="render">The render action used to produce padded content.</param>
        /// <param name="left">Spaces added to the left of each content line.</param>
        /// <param name="right">Spaces added to the right of each content line.</param>
        /// <param name="top">Blank lines added before the content.</param>
        /// <param name="bottom">Blank lines added after the content.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color used for padded content.</param>
        /// <param name="backgroundColor">Optional background color used for padded content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Pad(
            Action render,
            int left = 0,
            int right = 0,
            int top = 0,
            int bottom = 0,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            var options = new PadOptions
            {
                Width = width,
                Left = left,
                Right = right,
                Top = top,
                Bottom = bottom
            };

            if (color.HasValue || backgroundColor.HasValue)
            {
                options.Scheme = new ColorScheme
                {
                    Color = RenderOptions.GetColorOrDefault(color),
                    BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
                };
            }

            Pad(render, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a renderable component and renders the captured text with responsive padding.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce padded content.</param>
        /// <param name="left">Spaces added to the left of each content line.</param>
        /// <param name="right">Spaces added to the right of each content line.</param>
        /// <param name="top">Blank lines added before the content.</param>
        /// <param name="bottom">Blank lines added after the content.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color used for padded content.</param>
        /// <param name="backgroundColor">Optional background color used for padded content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Pad(
            IConsoulRenderable renderable,
            int left = 0,
            int right = 0,
            int top = 0,
            int bottom = 0,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            var options = new PadOptions
            {
                Width = width,
                Left = left,
                Right = right,
                Top = top,
                Bottom = bottom
            };

            if (color.HasValue || backgroundColor.HasValue)
            {
                options.Scheme = new ColorScheme
                {
                    Color = RenderOptions.GetColorOrDefault(color),
                    BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
                };
            }

            Pad(renderable, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Renders text aligned inside a responsive width.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="options">Rendering options for the aligned content.</param>
        public static void Align(string content, AlignOptions options)
        {
            AlignEntry.Render(content, options);
        }

        /// <summary>
        /// Renders text aligned inside a responsive width.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="alignment">Alignment used for each content line.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color used for aligned content.</param>
        /// <param name="backgroundColor">Optional background color used for aligned content.</param>
        public static void Align(
            string content,
            TextBlockAlignment alignment = TextBlockAlignment.Left,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null)
        {
            var options = new AlignOptions
            {
                Width = width,
                Alignment = alignment
            };

            if (color.HasValue || backgroundColor.HasValue)
            {
                options.Scheme = new ColorScheme
                {
                    Color = RenderOptions.GetColorOrDefault(color),
                    BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
                };
            }

            Align(content, options);
        }

        /// <summary>
        /// Captures a render action and renders the captured text aligned inside a responsive width.
        /// </summary>
        /// <param name="render">The render action used to produce aligned content.</param>
        /// <param name="options">Rendering options for the aligned content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Align(Action render, AlignOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            AlignEntry.Render(render, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a renderable component and renders the captured text aligned inside a responsive width.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce aligned content.</param>
        /// <param name="options">Rendering options for the aligned content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Align(IConsoulRenderable renderable, AlignOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            AlignEntry.Render(renderable, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a render action and renders the captured text aligned inside a responsive width.
        /// </summary>
        /// <param name="render">The render action used to produce aligned content.</param>
        /// <param name="alignment">Alignment used for each captured content line.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color used for aligned content.</param>
        /// <param name="backgroundColor">Optional background color used for aligned content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Align(
            Action render,
            TextBlockAlignment alignment = TextBlockAlignment.Left,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            var options = new AlignOptions
            {
                Width = width,
                Alignment = alignment
            };

            if (color.HasValue || backgroundColor.HasValue)
            {
                options.Scheme = new ColorScheme
                {
                    Color = RenderOptions.GetColorOrDefault(color),
                    BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
                };
            }

            Align(render, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a renderable component and renders the captured text aligned inside a responsive width.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce aligned content.</param>
        /// <param name="alignment">Alignment used for each captured content line.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color used for aligned content.</param>
        /// <param name="backgroundColor">Optional background color used for aligned content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Align(
            IConsoulRenderable renderable,
            TextBlockAlignment alignment = TextBlockAlignment.Left,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            var options = new AlignOptions
            {
                Width = width,
                Alignment = alignment
            };

            if (color.HasValue || backgroundColor.HasValue)
            {
                options.Scheme = new ColorScheme
                {
                    Color = RenderOptions.GetColorOrDefault(color),
                    BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
                };
            }

            Align(renderable, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Renders wrapped, aligned, and padded text for view content.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="options">Rendering options for the text block.</param>
        public static void TextBlock(string content, TextBlockOptions options)
        {
            TextBlockEntry.Render(content, options);
        }

        /// <summary>
        /// Renders wrapped, aligned, and padded text for view content.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="alignment">Alignment used for each wrapped line.</param>
        /// <param name="color">Optional foreground color used for the text.</param>
        /// <param name="backgroundColor">Optional background color used for the text.</param>
        public static void TextBlock(
            string content,
            int? width = null,
            TextBlockAlignment alignment = TextBlockAlignment.Left,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null)
        {
            var options = new TextBlockOptions
            {
                Width = width,
                Alignment = alignment
            };

            if (color.HasValue || backgroundColor.HasValue)
            {
                options.Scheme = new ColorScheme
                {
                    Color = RenderOptions.GetColorOrDefault(color),
                    BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
                };
            }

            TextBlock(content, options);
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="options">Rendering options for the path.</param>
        public static void TextPath(string path, TextPathOptions options)
        {
            TextPathEntry.Render(path, options);
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="directoryColor">Optional foreground color used for directory segments.</param>
        /// <param name="fileNameColor">Optional foreground color used for the file name or final segment.</param>
        /// <param name="separatorColor">Optional foreground color used for path separators and elision markers.</param>
        public static void TextPath(
            string path,
            int? width = null,
            ConsoleColor? directoryColor = null,
            ConsoleColor? fileNameColor = null,
            ConsoleColor? separatorColor = null)
        {
            var options = new TextPathOptions
            {
                Width = width
            };

            if (directoryColor.HasValue)
            {
                options.DirectoryScheme = new ColorScheme
                {
                    Color = directoryColor.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            if (fileNameColor.HasValue)
            {
                options.FileNameScheme = new ColorScheme
                {
                    Color = fileNameColor.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            if (separatorColor.HasValue)
            {
                options.SeparatorScheme = new ColorScheme
                {
                    Color = separatorColor.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            TextPath(path, options);
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path using Spectre-style style text.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="directoryStyle">Optional style used for directory segments, such as <c>grey</c>.</param>
        /// <param name="fileNameStyle">Optional style used for the file name or final segment, such as <c>aqua</c>.</param>
        /// <param name="separatorStyle">Optional style used for path separators and elision markers, such as <c>darkgray</c>.</param>
        public static void TextPath(
            string path,
            int? width,
            string directoryStyle,
            string fileNameStyle = null,
            string separatorStyle = null)
        {
            TextPath(path, CreateTextPathOptions(width, directoryStyle, fileNameStyle, separatorStyle));
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path using Spectre-style style text.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="directoryStyle">Optional style used for directory segments, such as <c>grey</c>.</param>
        /// <param name="fileNameStyle">Optional style used for the file name or final segment, such as <c>aqua</c>.</param>
        /// <param name="separatorStyle">Optional style used for path separators and elision markers, such as <c>darkgray</c>.</param>
        public static void TextPathStyled(
            string path,
            int? width = null,
            string directoryStyle = null,
            string fileNameStyle = null,
            string separatorStyle = null)
        {
            TextPath(path, CreateTextPathOptions(width, directoryStyle, fileNameStyle, separatorStyle));
        }

        private static TextPathOptions CreateTextPathOptions(
            int? width,
            string directoryStyle,
            string fileNameStyle,
            string separatorStyle)
        {
            var options = new TextPathOptions
            {
                Width = width
            };

            if (!string.IsNullOrWhiteSpace(directoryStyle))
            {
                options.DirectoryScheme = Style.Parse(directoryStyle).ToColorScheme(RenderOptions.DefaultScheme);
            }

            if (!string.IsNullOrWhiteSpace(fileNameStyle))
            {
                options.FileNameScheme = Style.Parse(fileNameStyle).ToColorScheme(RenderOptions.DefaultScheme);
            }

            if (!string.IsNullOrWhiteSpace(separatorStyle))
            {
                options.SeparatorScheme = Style.Parse(separatorStyle).ToColorScheme(RenderOptions.DefaultScheme);
            }

            return options;
        }

        /// <summary>
        /// Renders a responsive breadcrumb trail for MVC-style console navigation.
        /// </summary>
        /// <param name="items">The breadcrumb items to render.</param>
        public static void Breadcrumb(params string[] items)
        {
            Breadcrumb((IEnumerable<string>)items, null);
        }

        /// <summary>
        /// Renders a responsive breadcrumb trail for MVC-style console navigation.
        /// </summary>
        /// <param name="items">The breadcrumb items to render.</param>
        /// <param name="options">Optional rendering options for the breadcrumb.</param>
        public static void Breadcrumb(IEnumerable<string> items, BreadcrumbOptions options = null)
        {
            BreadcrumbEntry.Render(items, options);
        }

        /// <summary>
        /// Renders a responsive breadcrumb trail for MVC-style console navigation.
        /// </summary>
        /// <param name="options">Optional rendering options for the breadcrumb.</param>
        /// <param name="items">The breadcrumb items to render.</param>
        public static void Breadcrumb(BreadcrumbOptions options, params string[] items)
        {
            Breadcrumb((IEnumerable<string>)items, options);
        }

        /// <summary>
        /// Configures and renders a responsive breadcrumb trail for MVC-style console navigation.
        /// </summary>
        /// <param name="configure">The action used to add breadcrumb items.</param>
        /// <param name="options">Optional rendering options for the breadcrumb.</param>
        public static void Breadcrumb(Action<BreadcrumbEntry> configure, BreadcrumbOptions options = null)
        {
            var breadcrumb = new BreadcrumbEntry(options);
            configure?.Invoke(breadcrumb);
            breadcrumb.Render();
        }

        /// <summary>
        /// Creates a responsive breadcrumb trail without writing to the console.
        /// </summary>
        /// <param name="items">The breadcrumb items.</param>
        /// <returns>A bounded single-line breadcrumb string.</returns>
        public static string BreadcrumbText(params string[] items)
        {
            return BreadcrumbText((IEnumerable<string>)items, null);
        }

        /// <summary>
        /// Creates a responsive breadcrumb trail without writing to the console.
        /// </summary>
        /// <param name="items">The breadcrumb items.</param>
        /// <param name="options">Optional rendering options for the breadcrumb.</param>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded single-line breadcrumb string.</returns>
        public static string BreadcrumbText(IEnumerable<string> items, BreadcrumbOptions options = null, int? width = null)
        {
            return BreadcrumbEntry.Text(items, options, width);
        }

        /// <summary>
        /// Creates a responsive breadcrumb trail without writing to the console.
        /// </summary>
        /// <param name="options">Optional rendering options for the breadcrumb.</param>
        /// <param name="items">The breadcrumb items.</param>
        /// <returns>A bounded single-line breadcrumb string.</returns>
        public static string BreadcrumbText(BreadcrumbOptions options, params string[] items)
        {
            return BreadcrumbText((IEnumerable<string>)items, options);
        }

        /// <summary>
        /// Renders a responsive shortcut bar for available view actions and keyboard hints.
        /// </summary>
        /// <param name="items">The shortcut items to render.</param>
        public static void ShortcutBar(params ShortcutItem[] items)
        {
            ShortcutBar((IEnumerable<ShortcutItem>)items, null);
        }

        /// <summary>
        /// Renders a responsive shortcut bar for available view actions and keyboard hints.
        /// </summary>
        /// <param name="items">The shortcut items to render.</param>
        /// <param name="options">Optional rendering options for the shortcut bar.</param>
        public static void ShortcutBar(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null)
        {
            ShortcutBarEntry.Render(items, options);
        }

        /// <summary>
        /// Renders a responsive shortcut bar for available view actions and keyboard hints.
        /// </summary>
        /// <param name="options">Optional rendering options for the shortcut bar.</param>
        /// <param name="items">The shortcut items to render.</param>
        public static void ShortcutBar(ShortcutBarOptions options, params ShortcutItem[] items)
        {
            ShortcutBar((IEnumerable<ShortcutItem>)items, options);
        }

        /// <summary>
        /// Configures and renders a responsive shortcut bar for available view actions and keyboard hints.
        /// </summary>
        /// <param name="configure">The action used to add shortcut items.</param>
        /// <param name="options">Optional rendering options for the shortcut bar.</param>
        public static void ShortcutBar(Action<ShortcutBarEntry> configure, ShortcutBarOptions options = null)
        {
            var shortcutBar = new ShortcutBarEntry(options);
            configure?.Invoke(shortcutBar);
            shortcutBar.Render();
        }

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="items">The shortcut items.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        public static string ShortcutBarText(params ShortcutItem[] items)
        {
            return ShortcutBarText((IEnumerable<ShortcutItem>)items, null);
        }

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="items">The shortcut items.</param>
        /// <param name="options">Optional rendering options for the shortcut bar.</param>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        public static string ShortcutBarText(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null, int? width = null)
        {
            return ShortcutBarEntry.Text(items, options, width);
        }

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="options">Optional rendering options for the shortcut bar.</param>
        /// <param name="items">The shortcut items.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        public static string ShortcutBarText(ShortcutBarOptions options, params ShortcutItem[] items)
        {
            return ShortcutBarText((IEnumerable<ShortcutItem>)items, options);
        }

        /// <summary>
        /// Renders a responsive help surface for MVC-style view options.
        /// </summary>
        /// <param name="items">The help items to render.</param>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        public static void ViewHelp(IEnumerable<ViewHelpItem> items, string title = null, ViewHelpOptions options = null)
        {
            ViewHelpEntry.Render(items, title, options);
        }

        /// <summary>
        /// Builds and renders a responsive help surface for MVC-style view options.
        /// </summary>
        /// <param name="configure">Action used to add help items before rendering.</param>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        public static void ViewHelp(Action<ViewHelpEntry> configure, string title = null, ViewHelpOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var help = new ViewHelpEntry(title, options);
            configure(help);
            help.Render();
        }

        /// <summary>
        /// Renders a responsive checklist for workflow state, validation results, and dashboard summaries.
        /// </summary>
        /// <param name="items">The checklist items to render.</param>
        /// <param name="options">Rendering options for the checklist.</param>
        public static void Checklist(IEnumerable<ChecklistItem> items, ChecklistOptions options = null)
        {
            ChecklistEntry.Render(items, options);
        }

        /// <summary>
        /// Renders a responsive checklist for workflow state, validation results, and dashboard summaries.
        /// </summary>
        /// <param name="options">Rendering options for the checklist.</param>
        /// <param name="items">The checklist items to render.</param>
        public static void Checklist(ChecklistOptions options, params ChecklistItem[] items)
        {
            Checklist((IEnumerable<ChecklistItem>)items, options);
        }

        /// <summary>
        /// Builds and renders a responsive checklist for workflow state, validation results, and dashboard summaries.
        /// </summary>
        /// <param name="configure">Action used to add checklist items before rendering.</param>
        /// <param name="options">Rendering options for the checklist.</param>
        public static void Checklist(Action<ChecklistEntry> configure, ChecklistOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var checklist = new ChecklistEntry(options);
            configure(checklist);
            checklist.Render();
        }

        /// <summary>
        /// Renders a responsive list of labeled values for model state and settings views.
        /// </summary>
        /// <param name="items">The property list items to render.</param>
        /// <param name="options">Rendering options for the property list.</param>
        public static void PropertyList(IEnumerable<PropertyListItem> items, PropertyListOptions options = null)
        {
            PropertyListEntry.Render(items, options);
        }

        /// <summary>
        /// Renders a responsive list of readable model properties for quick model summaries.
        /// </summary>
        /// <param name="model">The model object whose properties should be summarized.</param>
        /// <param name="options">Rendering options for the property list.</param>
        public static void PropertyList(object model, PropertyListOptions options = null)
        {
            var propertyList = new PropertyListEntry(options);
            propertyList.AddModel(model);
            propertyList.Render();
        }

        /// <summary>
        /// Builds and renders a responsive list of labeled values for model state and settings views.
        /// </summary>
        /// <param name="configure">Action used to add items before rendering.</param>
        /// <param name="options">Rendering options for the property list.</param>
        public static void PropertyList(Action<PropertyListEntry> configure, PropertyListOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var propertyList = new PropertyListEntry(options);
            configure(propertyList);
            propertyList.Render();
        }

        /// <summary>
        /// Renders compact responsive metrics for dashboards and model-backed views.
        /// </summary>
        /// <param name="items">The metric items to render.</param>
        /// <param name="options">Optional rendering options for the metrics.</param>
        public static void Metrics(IEnumerable<MetricItem> items, MetricsOptions options = null)
        {
            MetricsEntry.Render(items, options);
        }

        /// <summary>
        /// Renders compact responsive metrics for dashboards and model-backed views.
        /// </summary>
        /// <param name="options">Optional rendering options for the metrics.</param>
        /// <param name="items">The metric items to render.</param>
        public static void Metrics(MetricsOptions options, params MetricItem[] items)
        {
            Metrics((IEnumerable<MetricItem>)items, options);
        }

        /// <summary>
        /// Builds and renders compact responsive metrics for dashboards and model-backed views.
        /// </summary>
        /// <param name="configure">Action used to add metric items before rendering.</param>
        /// <param name="options">Optional rendering options for the metrics.</param>
        public static void Metrics(Action<MetricsEntry> configure, MetricsOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var metrics = new MetricsEntry(options);
            configure(metrics);
            metrics.Render();
        }

        /// <summary>
        /// Renders a responsive horizontal bar chart for view content.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Rendering options for the chart.</param>
        public static void BarChart(IEnumerable<BarChartItem> items, BarChartOptions options = null)
        {
            BarChartEntry.Render(items, options);
        }

        /// <summary>
        /// Builds and renders a responsive horizontal bar chart for view content.
        /// </summary>
        /// <param name="configure">Action used to add items before rendering.</param>
        /// <param name="options">Rendering options for the chart.</param>
        public static void BarChart(Action<BarChartEntry> configure, BarChartOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var chart = new BarChartEntry(options);
            configure(chart);
            chart.Render();
        }

        /// <summary>
        /// Renders a responsive breakdown chart for composition-style view content.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Rendering options for the chart.</param>
        public static void BreakdownChart(IEnumerable<BreakdownChartItem> items, BreakdownChartOptions options = null)
        {
            BreakdownChartEntry.Render(items, options);
        }

        /// <summary>
        /// Builds and renders a responsive breakdown chart for composition-style view content.
        /// </summary>
        /// <param name="configure">Action used to add items before rendering.</param>
        /// <param name="options">Rendering options for the chart.</param>
        public static void BreakdownChart(Action<BreakdownChartEntry> configure, BreakdownChartOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var chart = new BreakdownChartEntry(options);
            configure(chart);
            chart.Render();
        }

        /// <summary>
        /// Renders a compact responsive trend chart for dashboard and model summary content.
        /// </summary>
        /// <param name="values">The values rendered by the sparkline.</param>
        /// <param name="options">Rendering options for the sparkline.</param>
        public static void Sparkline(IEnumerable<double> values, SparklineOptions options = null)
        {
            SparklineEntry.Render(values, options);
        }

        /// <summary>
        /// Renders a compact responsive trend chart for dashboard and model summary content.
        /// </summary>
        /// <param name="values">The values rendered by the sparkline.</param>
        /// <param name="label">Optional label rendered before the sparkline.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color used for the sparkline.</param>
        /// <param name="showRange">Indicates whether the rendered value range should appear after the sparkline.</param>
        public static void Sparkline(
            IEnumerable<double> values,
            string label = null,
            int? width = null,
            ConsoleColor? color = null,
            bool showRange = false)
        {
            var options = new SparklineOptions
            {
                Label = label,
                Width = width,
                ShowRange = showRange
            };

            if (color.HasValue)
            {
                options.LineScheme = new ColorScheme
                {
                    Color = color.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            Sparkline(values, options);
        }

        /// <summary>
        /// Builds and renders a compact responsive trend chart for dashboard and model summary content.
        /// </summary>
        /// <param name="configure">Action used to add values before rendering.</param>
        /// <param name="options">Rendering options for the sparkline.</param>
        public static void Sparkline(Action<SparklineEntry> configure, SparklineOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var sparkline = new SparklineEntry(options);
            configure(sparkline);
            sparkline.Render();
        }

        /// <summary>
        /// Creates an in-place status message scope for a short-running operation.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="options">Rendering options for the status message.</param>
        /// <returns>A <see cref="StatusScope"/> that can update or complete the status message.</returns>
        public static StatusScope Status(string message, StatusOptions options = null)
        {
            return new StatusScope(message, options);
        }

        /// <summary>
        /// Runs an operation inside an in-place status message scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The operation to run with the active status scope.</param>
        /// <param name="options">Rendering options for the status message.</param>
        public static void Status(string message, Action<StatusScope> action, StatusOptions options = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (StatusScope status = Status(message, options))
            {
                try
                {
                    action(status);
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
        /// Runs an asynchronous operation inside an in-place status message scope without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The operation to run with the active status scope.</param>
        /// <param name="options">Rendering options for the status message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task StatusAsync(string message, Func<StatusScope, Task> action, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return StatusAsync(message, (status, token) => action(status), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place status message scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The operation to run with the active status scope.</param>
        /// <param name="options">Rendering options for the status message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public static Task<T> StatusAsync<T>(string message, Func<StatusScope, Task<T>> func, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return StatusAsync<T>(message, (status, token) => func(status), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place status message scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The operation to run with the active status scope.</param>
        /// <param name="options">Rendering options for the status message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task StatusAsync(string message, Func<StatusScope, CancellationToken, Task> action, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (StatusScope status = Status(message, options))
            {
                try
                {
                    await action(status, cancellationToken).ConfigureAwait(false);
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
        /// Runs an asynchronous operation inside an in-place status message scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The operation to run with the active status scope.</param>
        /// <param name="options">Rendering options for the status message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public static async Task<T> StatusAsync<T>(string message, Func<StatusScope, CancellationToken, Task<T>> func, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (StatusScope status = Status(message, options))
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

        /// <summary>
        /// Creates an in-place spinner scope for a short-running operation.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="options">Rendering options for the spinner.</param>
        /// <returns>A <see cref="SpinnerScope"/> that can tick, update, or complete the spinner message.</returns>
        public static SpinnerScope Spinner(string message, SpinnerOptions options = null)
        {
            return new SpinnerScope(message, options);
        }

        /// <summary>
        /// Runs an operation inside an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The operation to run with the active spinner scope.</param>
        /// <param name="options">Rendering options for the spinner.</param>
        public static void Spinner(string message, Action<SpinnerScope> action, SpinnerOptions options = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (SpinnerScope spinner = Spinner(message, options))
            {
                try
                {
                    action(spinner);
                    if (!spinner.IsCompleted)
                    {
                        spinner.Succeed();
                    }
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
        /// Runs an asynchronous operation inside an in-place spinner scope without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The operation to run with the active spinner scope.</param>
        /// <param name="options">Rendering options for the spinner.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task SpinnerAsync(string message, Func<SpinnerScope, Task> action, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return SpinnerAsync(message, (spinner, token) => action(spinner), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place spinner scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="func">The operation to run with the active spinner scope.</param>
        /// <param name="options">Rendering options for the spinner.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public static Task<T> SpinnerAsync<T>(string message, Func<SpinnerScope, Task<T>> func, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return SpinnerAsync<T>(message, (spinner, token) => func(spinner), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The operation to run with the active spinner scope.</param>
        /// <param name="options">Rendering options for the spinner.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task SpinnerAsync(string message, Func<SpinnerScope, CancellationToken, Task> action, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (SpinnerScope spinner = Spinner(message, options))
            {
                try
                {
                    await action(spinner, cancellationToken).ConfigureAwait(false);
                    if (!spinner.IsCompleted)
                    {
                        spinner.Succeed();
                    }
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

        /// <summary>
        /// Runs an asynchronous operation inside an in-place spinner scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="func">The operation to run with the active spinner scope.</param>
        /// <param name="options">Rendering options for the spinner.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public static async Task<T> SpinnerAsync<T>(string message, Func<SpinnerScope, CancellationToken, Task<T>> func, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (SpinnerScope spinner = Spinner(message, options))
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

        /// <summary>
        /// Creates a multi-task progress context for a workflow.
        /// </summary>
        /// <param name="options">Rendering options for progress rows.</param>
        /// <returns>A <see cref="ProgressContext"/> that can create and update task rows.</returns>
        public static ProgressContext Progress(ProgressContextOptions options = null)
        {
            return new ProgressContext(options);
        }

        /// <summary>
        /// Runs an operation inside a multi-task progress context.
        /// </summary>
        /// <param name="action">The operation to run with the active progress context.</param>
        /// <param name="options">Rendering options for progress rows.</param>
        public static void Progress(Action<ProgressContext> action, ProgressContextOptions options = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (ProgressContext progress = Progress(options))
            {
                action(progress);
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside a multi-task progress context without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="action">The operation to run with the active progress context.</param>
        /// <param name="options">Rendering options for progress rows.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task ProgressAsync(Func<ProgressContext, Task> action, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return ProgressAsync((progress, token) => action(progress), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside a multi-task progress context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="func">The operation to run with the active progress context.</param>
        /// <param name="options">Rendering options for progress rows.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public static Task<T> ProgressAsync<T>(Func<ProgressContext, Task<T>> func, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return ProgressAsync<T>((progress, token) => func(progress), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside a multi-task progress context.
        /// </summary>
        /// <param name="action">The operation to run with the active progress context.</param>
        /// <param name="options">Rendering options for progress rows.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ProgressAsync(Func<ProgressContext, CancellationToken, Task> action, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (ProgressContext progress = Progress(options))
            {
                await action(progress, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside a multi-task progress context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="func">The operation to run with the active progress context.</param>
        /// <param name="options">Rendering options for progress rows.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public static async Task<T> ProgressAsync<T>(Func<ProgressContext, CancellationToken, Task<T>> func, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (ProgressContext progress = Progress(options))
            {
                return await func(progress, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a live display context for repeatedly refreshing a dashboard or model-backed view.
        /// </summary>
        /// <param name="options">Rendering options for the live display.</param>
        /// <returns>A <see cref="LiveDisplayContext"/> that can refresh dashboard content.</returns>
        public static LiveDisplayContext Live(LiveDisplayOptions options = null)
        {
            return new LiveDisplayContext(options);
        }

        /// <summary>
        /// Creates a live display context for repeatedly refreshing a renderable dashboard target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="options">Rendering options for the live display.</param>
        /// <returns>A <see cref="LiveDisplayContext"/> that can refresh or replace dashboard content.</returns>
        public static LiveDisplayContext Live(IConsoulRenderable target, LiveDisplayOptions options = null)
        {
            return new LiveDisplayContext(target, options);
        }

        /// <summary>
        /// Runs an operation inside a live display context.
        /// </summary>
        /// <param name="action">The operation to run with the active live display context.</param>
        /// <param name="options">Rendering options for the live display.</param>
        public static void Live(Action<LiveDisplayContext> action, LiveDisplayOptions options = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (LiveDisplayContext live = Live(options))
            {
                action(live);
            }
        }

        /// <summary>
        /// Runs an operation inside a live display context with an initial render target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The operation to run with the active live display context.</param>
        /// <param name="options">Rendering options for the live display.</param>
        public static void Live(IConsoulRenderable target, Action<LiveDisplayContext> action, LiveDisplayOptions options = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (LiveDisplayContext live = Live(target, options))
            {
                live.Start(action);
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside a live display context without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="action">The asynchronous operation to run with the active live display context.</param>
        /// <param name="options">Rendering options for the live display.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task LiveAsync(Func<LiveDisplayContext, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return LiveAsync((live, token) => action(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside a live display context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="func">The asynchronous operation to run with the active live display context.</param>
        /// <param name="options">Rendering options for the live display.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public static Task<T> LiveAsync<T>(Func<LiveDisplayContext, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return LiveAsync<T>((live, token) => func(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside a live display context.
        /// </summary>
        /// <param name="action">The asynchronous operation to run with the active live display context.</param>
        /// <param name="options">Rendering options for the live display.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task LiveAsync(Func<LiveDisplayContext, CancellationToken, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (LiveDisplayContext live = Live(options))
            {
                await action(live, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside a live display context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="func">The asynchronous operation to run with the active live display context.</param>
        /// <param name="options">Rendering options for the live display.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public static async Task<T> LiveAsync<T>(Func<LiveDisplayContext, CancellationToken, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (LiveDisplayContext live = Live(options))
            {
                return await func(live, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside a live display context with an initial render target without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The asynchronous operation to run with the active live display context.</param>
        /// <param name="options">Rendering options for the live display.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task LiveAsync(IConsoulRenderable target, Func<LiveDisplayContext, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return LiveAsync(target, (live, token) => action(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside a live display context with an initial render target and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="target">The initial render target.</param>
        /// <param name="func">The asynchronous operation to run with the active live display context.</param>
        /// <param name="options">Rendering options for the live display.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public static Task<T> LiveAsync<T>(IConsoulRenderable target, Func<LiveDisplayContext, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return LiveAsync<T>(target, (live, token) => func(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous operation inside a live display context with an initial render target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The asynchronous operation to run with the active live display context.</param>
        /// <param name="options">Rendering options for the live display.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task LiveAsync(IConsoulRenderable target, Func<LiveDisplayContext, CancellationToken, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (LiveDisplayContext live = Live(target, options))
            {
                await live.StartAsync(action, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside a live display context with an initial render target and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="target">The initial render target.</param>
        /// <param name="func">The asynchronous operation to run with the active live display context.</param>
        /// <param name="options">Rendering options for the live display.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public static async Task<T> LiveAsync<T>(IConsoulRenderable target, Func<LiveDisplayContext, CancellationToken, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (LiveDisplayContext live = Live(target, options))
            {
                return await live.StartAsync(func, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes a message centered in the window in its current size.
        /// </summary>
        /// <param name="message">Message to center.</param>
        /// <param name="maxWidth">Maximum width that the text can render (in character length)</param>
        /// <param name="color">Optional foreground color for the centered text.</param>
        /// <param name="backgroundColor">Optional background color for the centered text.</param>
        /// <param name="writeLine">Indicates whether to append a newline after writing the centered text.</param>
        /// <param name="whitespace">Character used to fill space around the <paramref name="message"/></param>
        public static void Center(string message, int maxWidth, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool writeLine = true, char whitespace = ' ')
        {
            if (maxWidth == 0)
                maxWidth = ConsoleBufferWidth;
            string text = message.Length > maxWidth ? message.Substring(0, maxWidth - 1) + "…" : message;

            int remainder = maxWidth - text.Length - 1;
            int left, right;
            right = remainder / 2;
            left = right;
            if (remainder % 2 != 0)
                left = (remainder + 1) / 2;


            text = $"{(new string(whitespace, left))}{text}{(new string(whitespace, right))}";

            Write(text, color: color, backgroundColor: backgroundColor, writeLine: writeLine);
        }

        /// <summary>
        /// <inheritdoc cref="Center(string, int, ConsoleColor?, ConsoleColor?, bool, char)"/>
        /// </summary>
        /// <param name="message"><inheritdoc cref="Center(string, int, ConsoleColor?, ConsoleColor?, bool, char)" path="/param[@name='message']"/></param>
        /// <param name="maxWidth"><inheritdoc cref="Center(string, int, ConsoleColor?, ConsoleColor?, bool, char)" path="/param[@name='maxWidth']"/></param>
        /// <param name="colorScheme">Color scheme used to write message and background</param>
        /// <param name="writeLine"><inheritdoc cref="Center(string, int, ConsoleColor?, ConsoleColor?, bool, char)" path="/param[@name='writeLine']"/></param>
        /// <param name="whitespace"><inheritdoc cref="Center(string, int, ConsoleColor?, ConsoleColor?, bool, char)" path="/param[@name='whitespace']"/></param>
        public static void Center(string message, int maxWidth, ColorScheme colorScheme, bool writeLine = true, char whitespace = ' ')
            => Center(message, maxWidth, colorScheme.Color, colorScheme.BackgroundColor, writeLine, whitespace);

        /// <summary>
        /// Prompts the user to acknowledge a Prompt.
        /// </summary>
        /// <param name="message">Display message</param>
        /// <param name="clear">Option to clear the Console buffer. If true, can make the prompt more prominant.</param>
        /// <param name="allowEmpty">Specifies whether the user can provide an empty response. Default is typically the 'No', but can be overriden</param>
        /// <param name="defaultIsNo">Specifies whether the default entry should be 'No'. This only applies if 'allowEmpty' is true.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Boolean of users response relative to 'Yes' or 'No'</returns>
        public static bool Ask(string message, bool clear = false, bool allowEmpty = false, bool defaultIsNo = true, CancellationToken cancellationToken = default)
        {
            string input = "";
            string orEmpty = $" or Press Enter";
            string[] options = new string[] {
                $"Y{(allowEmpty && !defaultIsNo ? orEmpty : string.Empty)}=Yes",
                $"N{(allowEmpty && defaultIsNo ? orEmpty : string.Empty)}=No"
            };
            string optionMessage = $"({string.Join(", ", options)})";

            do
            {
                if (clear)
                {
                    ConsoleDriver.Clear();
                }
                Consoul.WriteCore(message, RenderOptions.PromptScheme);
                Consoul.WriteCore(optionMessage, RenderOptions.SubnoteScheme);
                input = NormalizeAskInput(Read(cancellationToken));
                if (!IsAskInputAccepted(input, allowEmpty) && !string.IsNullOrEmpty(input))
                {
                    Consoul.WriteCore("Invalid input!", RenderOptions.InvalidScheme);
                }
            } while (!IsAskInputAccepted(input, allowEmpty));

            if (allowEmpty && string.IsNullOrEmpty(input))
                input = defaultIsNo ? "n" : "y";
            return input == "y";
        }

        private static string NormalizeAskInput(string input)
        {
            return (input ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static bool IsAskInputAccepted(string input, bool allowEmpty)
        {
            return input == "y" || input == "n" || (allowEmpty && string.IsNullOrEmpty(input));
        }

        /// <summary>
        /// Prompts the user with a simple list of choices.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>Index of the option that was chosen. Returns -1 if selection was invalid.</returns>
        public static int Prompt(string message, bool clear = false, params string[] options)
        {
            return Prompt(message, clear, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts the user with a simple list of choices.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="options">Simple list of options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Index of the option that was chosen. Returns -1 if selection was invalid.</returns>
        public static int Prompt(string message, bool clear = false, CancellationToken cancellationToken = default, params string[] options)
        {
            var result = (new SelectionPrompt(message, clear, options)).Render(cancellationToken);
            return result.Index;
        }

        /// <summary>
        /// Prompts the user with a complex list of choices.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Array of complex options.</param>
        /// <param name="clear"><inheritdoc cref="Prompt(string, bool, string[])" path="/param[@name='clear']"/></param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>Index of the option that was chosen. Returns -1 if selection was invalid.</returns>
        public static int Prompt(string message, SelectOption[] options, bool clear = false, CancellationToken cancellationToken = default)
        {
            var result = (new SelectionPrompt(message, clear, options)).Render(cancellationToken);
            return result.Index;
        }

        /// <summary>
        /// Prompts the user with a simple list of choices and returns the full prompt result.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>The prompt result describing the selected option or cancellation state.</returns>
        public static PromptResult Select(string message, bool clear = false, params string[] options)
        {
            return Select(message, clear, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts the user with a simple list of choices and returns the full prompt result.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>The prompt result describing the selected option or cancellation state.</returns>
        public static PromptResult Select(string message, bool clear = false, CancellationToken cancellationToken = default, params string[] options)
        {
            return (new SelectionPrompt(message, clear, options)).Render(cancellationToken);
        }

        /// <summary>
        /// Prompts the user with a complex list of choices and returns the full prompt result.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Array of complex options.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The prompt result describing the selected option or cancellation state.</returns>
        public static PromptResult Select(string message, SelectOption[] options, bool clear = false, CancellationToken cancellationToken = default)
        {
            return (new SelectionPrompt(message, clear, options)).Render(cancellationToken);
        }

        /// <summary>
        /// Prompts the user with a list of choices, marks a default option, and returns the selected index.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Simple list of options.</param>
        /// <param name="defaultOption">Optional option label selected when the user presses Enter.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The prompt result describing the selected option or cancellation state.</returns>
        public static PromptResult Select(string message, IEnumerable<string> options, string defaultOption = null, bool clear = false, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var prompt = new SelectionPrompt(message, clear);
            foreach (string option in options)
            {
                bool isDefault = defaultOption != null && string.Equals(option, defaultOption, StringComparison.Ordinal);
                prompt.Add(option, isDefault: isDefault);
            }

            return prompt.Render(cancellationToken);
        }

        /// <summary>
        /// Prompts the user with typed choices and returns both the prompt result and selected item.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="options">Typed options to present.</param>
        /// <returns>The typed selection result.</returns>
        public static SelectionResult<T> Select<T>(string message, Func<T, string> labelSelector, bool clear = false, params T[] options)
        {
            return Select(message, labelSelector, clear, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts the user with typed choices and returns both the prompt result and selected item.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="options">Typed options to present.</param>
        /// <returns>The typed selection result.</returns>
        public static SelectionResult<T> Select<T>(string message, Func<T, string> labelSelector, bool clear = false, CancellationToken cancellationToken = default, params T[] options)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            return Select(message, options ?? Array.Empty<T>(), labelSelector, (Func<T, bool>)null, clear, cancellationToken);
        }

        /// <summary>
        /// Prompts the user with typed choices and returns both the prompt result and selected item.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed selection result.</returns>
        public static SelectionResult<T> Select<T>(string message, IEnumerable<T> options, Func<T, string> labelSelector, bool clear = false, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return Select(message, labelSelector, clear, cancellationToken, options.ToArray());
        }

        /// <summary>
        /// Prompts the user with typed choices, marks a default item, and returns both the prompt result and selected item.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="defaultItem">Optional item selected when the user presses Enter.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed selection result.</returns>
        public static SelectionResult<T> Select<T>(string message, IEnumerable<T> options, Func<T, string> labelSelector, T defaultItem, bool clear = false, CancellationToken cancellationToken = default)
        {
            return Select(message, options, labelSelector, item => EqualityComparer<T>.Default.Equals(item, defaultItem), clear, cancellationToken);
        }

        /// <summary>
        /// Prompts the user with typed choices, marks items matching a default predicate, and returns both the prompt result and selected item.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="isDefault">Optional predicate used to mark a default option.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed selection result.</returns>
        public static SelectionResult<T> Select<T>(string message, IEnumerable<T> options, Func<T, string> labelSelector, Func<T, bool> isDefault, bool clear = false, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            var values = options.ToArray();
            var prompt = new SelectionPrompt<T>(message, clear, labelSelector);
            foreach (T option in values)
            {
                bool defaultOption = isDefault != null && isDefault(option);
                prompt.Add(option, isDefault: defaultOption);
            }

            return prompt.RenderSelection(cancellationToken);
        }

        /// <summary>
        /// Prompts the user with enum values and returns both the prompt result and selected enum value.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="defaultValue">Optional default value selected when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum selection result.</returns>
        public static SelectionResult<TEnum> SelectEnum<TEnum>(string message, bool clear = false, TEnum? defaultValue = null, CancellationToken cancellationToken = default) where TEnum : struct
        {
            return SelectEnum(message, Enum.GetValues(typeof(TEnum)).Cast<TEnum>(), null, clear, defaultValue, cancellationToken);
        }

        /// <summary>
        /// Prompts the user with enum values and custom labels.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each enum value.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="defaultValue">Optional default value selected when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum selection result.</returns>
        public static SelectionResult<TEnum> SelectEnum<TEnum>(string message, Func<TEnum, string> labelSelector, bool clear = false, TEnum? defaultValue = null, CancellationToken cancellationToken = default) where TEnum : struct
        {
            return SelectEnum(message, Enum.GetValues(typeof(TEnum)).Cast<TEnum>(), labelSelector, clear, defaultValue, cancellationToken);
        }

        /// <summary>
        /// Prompts the user with a filtered set of enum values.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="values">Enum values to present.</param>
        /// <param name="labelSelector">Optional function that creates a display label for each enum value.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="defaultValue">Optional default value selected when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum selection result.</returns>
        public static SelectionResult<TEnum> SelectEnum<TEnum>(string message, IEnumerable<TEnum> values, Func<TEnum, string> labelSelector = null, bool clear = false, TEnum? defaultValue = null, CancellationToken cancellationToken = default) where TEnum : struct
        {
            EnsureEnumType<TEnum>();
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var options = values.ToArray();
            var prompt = new SelectionPrompt<TEnum>(message, clear, labelSelector ?? FormatEnumValue);
            foreach (TEnum option in options)
            {
                bool isDefault = defaultValue.HasValue && EqualityComparer<TEnum>.Default.Equals(option, defaultValue.Value);
                prompt.Add(option, isDefault: isDefault);
            }

            PromptResult result = prompt.Render(cancellationToken);
            TEnum selectedItem = default(TEnum);
            if (result.HasSelection && result.Index >= 0 && result.Index < options.Length)
            {
                selectedItem = options[result.Index];
            }

            return new SelectionResult<TEnum>(result, selectedItem);
        }

        private static void EnsureEnumType<TEnum>() where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be an enum type.");
            }
        }

        private static string FormatEnumValue<TEnum>(TEnum value) where TEnum : struct
        {
            string name = Enum.GetName(typeof(TEnum), value);
            if (string.IsNullOrEmpty(name))
            {
                return value.ToString();
            }

            FieldInfo field = typeof(TEnum).GetField(name);
            string label = ResolveEnumDisplayName(field);
            return !string.IsNullOrWhiteSpace(label) ? label : SplitPascalCase(name);
        }

        private static string ResolveEnumDisplayName(FieldInfo field)
        {
            if (field == null)
            {
                return null;
            }

            object displayAttribute = field
                .GetCustomAttributes(true)
                .FirstOrDefault(attribute => string.Equals(
                    attribute.GetType().FullName,
                    "System.ComponentModel.DataAnnotations.DisplayAttribute",
                    StringComparison.Ordinal));

            if (displayAttribute != null)
            {
                string displayName = InvokeStringMethod(displayAttribute, "GetName");
                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    return displayName;
                }
            }

            var displayNameAttribute = field.GetCustomAttribute<DisplayNameAttribute>(true);
            if (displayNameAttribute != null && !string.IsNullOrWhiteSpace(displayNameAttribute.DisplayName))
            {
                return displayNameAttribute.DisplayName;
            }

            var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>(true);
            if (descriptionAttribute != null && !string.IsNullOrWhiteSpace(descriptionAttribute.Description))
            {
                return descriptionAttribute.Description;
            }

            return null;
        }

        private static string InvokeStringMethod(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            return method?.Invoke(target, null) as string;
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return Regex.Replace(value, "([a-z0-9])([A-Z])", "$1 $2");
        }

        /// <summary>
        /// Prompts the user with a simple list of choices and returns all selected indexes.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>The multi-selection result describing selected indexes or cancellation state.</returns>
        public static MultiSelectionResult MultiSelect(string message, bool clear = false, params string[] options)
        {
            return MultiSelect(message, clear, false, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts the user with a simple list of choices and returns all selected indexes.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="allowEmpty">Indicates whether no selected options is a valid result.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>The multi-selection result describing selected indexes or cancellation state.</returns>
        public static MultiSelectionResult MultiSelect(string message, bool clear = false, bool allowEmpty = false, params string[] options)
        {
            return MultiSelect(message, clear, allowEmpty, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts the user with a simple list of choices and returns all selected indexes.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="allowEmpty">Indicates whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>The multi-selection result describing selected indexes or cancellation state.</returns>
        public static MultiSelectionResult MultiSelect(string message, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default, params string[] options)
        {
            return (new MultiSelectionPrompt(message, clear, allowEmpty, options)).Render(cancellationToken);
        }

        /// <summary>
        /// Prompts the user with a list of choices, marks default options, and returns all selected indexes.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Simple list of options.</param>
        /// <param name="defaultOptions">Optional option labels selected when the user presses Enter.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="allowEmpty">Indicates whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The multi-selection result describing selected indexes or cancellation state.</returns>
        public static MultiSelectionResult MultiSelect(string message, IEnumerable<string> options, IEnumerable<string> defaultOptions = null, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var defaults = new HashSet<string>(defaultOptions ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            var prompt = new MultiSelectionPrompt(message, clear, allowEmpty);
            foreach (string option in options)
            {
                prompt.Add(option, isDefault: defaults.Contains(option));
            }

            return prompt.Render(cancellationToken);
        }

        /// <summary>
        /// Prompts the user with typed choices and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="options">Typed options to present.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(string message, Func<T, string> labelSelector, bool clear = false, params T[] options)
        {
            return MultiSelect(message, labelSelector, clear, false, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts the user with typed choices and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="allowEmpty">Indicates whether no selected options is a valid result.</param>
        /// <param name="options">Typed options to present.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(string message, Func<T, string> labelSelector, bool clear = false, bool allowEmpty = false, params T[] options)
        {
            return MultiSelect(message, labelSelector, clear, allowEmpty, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts the user with typed choices and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="allowEmpty">Indicates whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="options">Typed options to present.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(string message, Func<T, string> labelSelector, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default, params T[] options)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            return MultiSelect(message, options ?? Array.Empty<T>(), labelSelector, (Func<T, bool>)null, clear, allowEmpty, cancellationToken);
        }

        /// <summary>
        /// Prompts the user with typed choices and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="allowEmpty">Indicates whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(string message, IEnumerable<T> options, Func<T, string> labelSelector, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return MultiSelect(message, labelSelector, clear, allowEmpty, cancellationToken, options.ToArray());
        }

        /// <summary>
        /// Prompts the user with typed choices, marks default items, and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="defaultItems">Optional items selected when the user presses Enter.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="allowEmpty">Indicates whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(string message, IEnumerable<T> options, Func<T, string> labelSelector, IEnumerable<T> defaultItems, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            var defaults = new HashSet<T>(defaultItems ?? Enumerable.Empty<T>());
            return MultiSelect(message, options, labelSelector, item => defaults.Contains(item), clear, allowEmpty, cancellationToken);
        }

        /// <summary>
        /// Prompts the user with typed choices, marks items matching a default predicate, and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="isDefault">Optional predicate used to mark default options.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="allowEmpty">Indicates whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(string message, IEnumerable<T> options, Func<T, string> labelSelector, Func<T, bool> isDefault, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            var values = options.ToArray();
            var prompt = new MultiSelectionPrompt<T>(message, clear, allowEmpty, labelSelector);
            foreach (T option in values)
            {
                bool defaultOption = isDefault != null && isDefault(option);
                prompt.Add(option, isDefault: defaultOption);
            }

            return prompt.RenderSelection(cancellationToken);
        }

        /// <summary>
        /// Prompts the user with enum values and returns selected enum values.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="allowEmpty">Indicates whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum multi-selection result.</returns>
        public static MultiSelectionResult<TEnum> MultiSelectEnum<TEnum>(string message, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default) where TEnum : struct
        {
            return MultiSelectEnum(message, Enum.GetValues(typeof(TEnum)).Cast<TEnum>(), null, null, clear, allowEmpty, cancellationToken);
        }

        /// <summary>
        /// Prompts the user with enum values and custom labels.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each enum value.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="allowEmpty">Indicates whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum multi-selection result.</returns>
        public static MultiSelectionResult<TEnum> MultiSelectEnum<TEnum>(string message, Func<TEnum, string> labelSelector, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default) where TEnum : struct
        {
            return MultiSelectEnum(message, Enum.GetValues(typeof(TEnum)).Cast<TEnum>(), labelSelector, null, clear, allowEmpty, cancellationToken);
        }

        /// <summary>
        /// Prompts the user with a filtered set of enum values and returns selected enum values.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="values">Enum values to present.</param>
        /// <param name="labelSelector">Optional function that creates a display label for each enum value.</param>
        /// <param name="defaultValues">Optional enum values selected when the user presses Enter.</param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="allowEmpty">Indicates whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum multi-selection result.</returns>
        public static MultiSelectionResult<TEnum> MultiSelectEnum<TEnum>(string message, IEnumerable<TEnum> values, Func<TEnum, string> labelSelector = null, IEnumerable<TEnum> defaultValues = null, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default) where TEnum : struct
        {
            EnsureEnumType<TEnum>();
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var defaults = new HashSet<TEnum>(defaultValues ?? Enumerable.Empty<TEnum>());
            return MultiSelect(
                message,
                values,
                labelSelector ?? FormatEnumValue,
                value => defaults.Contains(value),
                clear,
                allowEmpty,
                cancellationToken);
        }

        /// <summary>
        /// Prompts the user to input a file path.
        /// </summary>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="checkExists">Indicates whether to check the file exists before allowing the user exit the loop.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Filepath string</returns>
        public static string PromptForFilepath(string message, bool checkExists, CancellationToken cancellationToken = default) {
            string path;
            do
            {
                Write(message, RenderOptions.PromptScheme);
                path = NormalizePromptedFilePath(Read(cancellationToken));
            } while (!IsPromptedFilePathAccepted(path, checkExists));

            return path;
        }

        internal static string NormalizePromptedFilePath(string path)
        {
            path = path ?? string.Empty;
            path = path.Trim();
            if (path.Length >= 2 && path.StartsWith("\"") && path.EndsWith("\""))
            {
                path = path.Substring(1, path.Length - 2);
            }

            return path;
        }

        private static TextPrompt<string> CreatePathPrompt(
            string message,
            bool clear,
            bool mustExist,
            Func<string, bool> exists,
            string requiredMessage,
            string missingMessage)
        {
            var prompt = new TextPrompt<string>(message, clear);
            prompt.ParseWith((string text, out string value) =>
            {
                value = NormalizePromptedFilePath(text);
                return true;
            });
            prompt.Validate(path =>
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return prompt.AllowEmpty ? null : requiredMessage;
                }

                if (mustExist && !exists(path))
                {
                    return missingMessage;
                }

                return null;
            });

            return prompt;
        }

        internal static bool IsPromptedFilePathAccepted(string path, bool checkExists)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            return !checkExists || File.Exists(path);
        }

        /// <summary>
        /// Prompts the user to input a file path with a suggested default path.
        /// </summary>
        /// <param name="defaultPath">The default file path the user must accept.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="checkExists"><inheritdoc cref="PromptForFilepath(string, bool, CancellationToken)" path="/param[@name='checkExists']"/></param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Filepath string</returns>
        public static string PromptForFilepath(string defaultPath, string message, bool checkExists, CancellationToken cancellationToken = default) {
            string path = NormalizePromptedFilePath(defaultPath);
            if (!IsPromptedFilePathAccepted(path, checkExists) || !Ask($"Would you like to use the default path:\r\n{path}", defaultIsNo: false, cancellationToken: cancellationToken)) {
                path = PromptForFilepath(message, checkExists, cancellationToken);
            }
            return path;
        }

        /// <summary>
        /// Plays the BEL character in the console. See <see href="https://en.wikipedia.org/wiki/Bell_character">Wikipedia</see> for more details.
        /// </summary>
        public static void Ding()
        {
            const char BEL = (char)7;
            ConsoleDriver.Write(BEL.ToString());
        }

        /// <summary>
        /// Displays a message in the console and plays <see cref="Ding"/>.
        /// </summary>
        /// <param name="message">Message to display before playing the alert sound.</param>
        /// <param name="color">Optional foreground color for the alert text.</param>
        /// <param name="backgroundColor">Optional background color for the alert text.</param>
        /// <param name="writeLine">Indicates whether to append a newline after writing the alert text.</param>
        public static void Alert(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool writeLine = true)
        {
            Write(message, color: color, backgroundColor: backgroundColor, writeLine: writeLine);
            Ding();
        }

        /// <summary>
        /// Saves the current cursor position. NOTE: You <b>MUST</b> dispose this variable.
        /// </summary>
        /// <returns>Reference to a <see cref="CursorScope"/> position</returns>
        public static CursorScope SaveCursor()
            => new CursorScope();
    }
}
