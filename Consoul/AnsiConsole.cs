using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style entry points that delegate to Consoul's dependency-light MVC console APIs.
    /// </summary>
    /// <remarks>
    /// This facade is intended for migration ergonomics. Prompt methods preserve Consoul's result-first return types
    /// so cancellation and empty selections remain explicit.
    /// </remarks>
    public static class AnsiConsole
    {
        private static readonly IAnsiConsole DefaultConsole = new ConsoulAnsiConsole();
        private static readonly AsyncLocal<int> DispatchSuppressionDepth = new AsyncLocal<int>();
        private static IAnsiConsole _console = DefaultConsole;
        private static RecordingConsoleDriver _recordingDriver;
        private static IDisposable _recordingScope;

        /// <summary>
        /// Gets or sets the injectable facade instance used by static entry points when a custom facade is assigned.
        /// </summary>
        public static IAnsiConsole Console
        {
            get => _console;
            set => _console = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates an injectable console facade from Spectre-style console settings.
        /// </summary>
        /// <param name="settings">The settings used to create the console facade.</param>
        /// <returns>A configured console facade.</returns>
        public static IAnsiConsole Create(AnsiConsoleSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            IAnsiConsoleOutput output = settings.Out ?? new AnsiConsoleOutput(System.Console.Out);
            return new ConsoulAnsiConsole(new TextWriterConsoleDriver(output, settings));
        }

        /// <summary>
        /// Gets a snapshot of the active terminal profile.
        /// </summary>
        public static TerminalProfile Profile => Console.Profile;

        /// <summary>
        /// Gets cursor operations for the active console.
        /// </summary>
        public static IAnsiConsoleCursor Cursor => Console.Cursor;

        /// <summary>
        /// Starts recording output written through the active Consoul console driver.
        /// </summary>
        public static void Record()
        {
            if (TryDispatch(console => console.Record()))
            {
                return;
            }

            if (_recordingDriver != null)
            {
                return;
            }

            var driver = new RecordingConsoleDriver(Consoul.ConsoleDriver);
            _recordingScope = Consoul.UseConsoleDriver(driver);
            _recordingDriver = driver;
        }

        /// <summary>
        /// Exports recorded console output as plain text.
        /// </summary>
        /// <returns>The recorded console output.</returns>
        public static string ExportText()
        {
            string result;
            if (TryDispatch(console => console.ExportText(), out result))
            {
                return result;
            }

            return GetRecordingDriver().GetText();
        }

        /// <summary>
        /// Exports recorded console output as escaped HTML.
        /// </summary>
        /// <returns>The recorded console output wrapped in a preformatted HTML block.</returns>
        public static string ExportHtml()
        {
            string result;
            if (TryDispatch(console => console.ExportHtml(), out result))
            {
                return result;
            }

            return GetRecordingDriver().GetHtml();
        }

        internal static void ResetRecording()
        {
            if (_recordingScope != null)
            {
                _recordingScope.Dispose();
                _recordingScope = null;
            }

            _recordingDriver = null;
        }

        internal static IDisposable SuppressDispatch()
        {
            DispatchSuppressionDepth.Value++;
            return new DispatchSuppressionScope();
        }

        /// <summary>
        /// Clears the active console driver.
        /// </summary>
        public static void Clear()
        {
            Clear(home: true);
        }

        /// <summary>
        /// Clears the active console driver and accepts Spectre-style cursor-home intent.
        /// </summary>
        /// <param name="home">Whether the cursor should be moved to the home position after clearing.</param>
        public static void Clear(bool home)
        {
            if (TryDispatch(console => console.Clear(home)))
            {
                return;
            }

            Consoul.ConsoleDriver.Clear();
            if (home)
            {
                Consoul.ConsoleDriver.SetCursorPosition(0, 0);
            }
        }

        /// <summary>
        /// Enters the terminal alternate screen when the active console supports ANSI sequences.
        /// </summary>
        /// <returns>A disposable scope that restores the main screen on disposal.</returns>
        public static AlternateScreenScope AlternateScreen()
        {
            return AnsiConsoleExtensions.AlternateScreen(Console);
        }

        /// <summary>
        /// Runs an action inside the terminal alternate screen when ANSI sequences are supported.
        /// </summary>
        /// <param name="action">The action to run inside the alternate screen.</param>
        public static void AlternateScreen(Action action)
        {
            AnsiConsoleExtensions.AlternateScreen(Console, action);
        }

        /// <summary>
        /// Runs a function inside the terminal alternate screen and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="func">The function to run inside the alternate screen.</param>
        /// <returns>The function result.</returns>
        public static T AlternateScreen<T>(Func<T> func)
        {
            return AnsiConsoleExtensions.AlternateScreen(Console, func);
        }

        /// <summary>
        /// Runs an asynchronous action inside the terminal alternate screen without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="action">The asynchronous action to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the action completes.</returns>
        public static Task AlternateScreenAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            return AnsiConsoleExtensions.AlternateScreenAsync(Console, action, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside the terminal alternate screen.
        /// </summary>
        /// <param name="action">The asynchronous action to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the action completes.</returns>
        public static Task AlternateScreenAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            return AnsiConsoleExtensions.AlternateScreenAsync(Console, action, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous function inside the terminal alternate screen and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="func">The asynchronous function to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the function result.</returns>
        public static Task<T> AlternateScreenAsync<T>(Func<Task<T>> func, CancellationToken cancellationToken = default)
        {
            return AnsiConsoleExtensions.AlternateScreenAsync(Console, func, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous function inside the terminal alternate screen and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="func">The asynchronous function to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the function result.</returns>
        public static Task<T> AlternateScreenAsync<T>(Func<CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
        {
            return AnsiConsoleExtensions.AlternateScreenAsync(Console, func, cancellationToken);
        }

        /// <summary>
        /// Writes plain text without appending a line break.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public static void Write(string text)
        {
            if (TryDispatch(console => console.Write(text)))
            {
                return;
            }

            Consoul.Write(text ?? string.Empty, writeLine: false);
        }

        /// <summary>
        /// Writes the text representation of an object without appending a line break.
        /// </summary>
        /// <param name="value">The object to write.</param>
        public static void Write(object value)
        {
            Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of an object without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The object to write.</param>
        public static void Write(IFormatProvider provider, object value)
        {
            if (value is IConsoulRenderable renderable)
            {
                Write(renderable);
                return;
            }

            if (value is IRenderable spectreRenderable)
            {
                Write(spectreRenderable);
                return;
            }

            Write(Convert.ToString(value, provider) ?? string.Empty);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(int value)
        {
            Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(IFormatProvider provider, int value)
        {
            Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(uint value)
        {
            Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(IFormatProvider provider, uint value)
        {
            Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(long value)
        {
            Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(IFormatProvider provider, long value)
        {
            Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(ulong value)
        {
            Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(IFormatProvider provider, ulong value)
        {
            Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(float value)
        {
            Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(IFormatProvider provider, float value)
        {
            Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(double value)
        {
            Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(IFormatProvider provider, double value)
        {
            Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a decimal value without appending a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(decimal value)
        {
            Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a decimal value without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(IFormatProvider provider, decimal value)
        {
            Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a Boolean value without appending a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(bool value)
        {
            Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a Boolean value without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(IFormatProvider provider, bool value)
        {
            Write(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes a Unicode character without appending a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(char value)
        {
            Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes a Unicode character without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(IFormatProvider provider, char value)
        {
            Write(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes an array of Unicode characters without appending a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(char[] value)
        {
            Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes an array of Unicode characters without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(IFormatProvider provider, char[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Write(new string(value));
        }

        /// <summary>
        /// Writes a formatted string without appending a line break.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void Write(string format, params object[] args)
        {
            Write(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes a formatted string without appending a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void Write(IFormatProvider provider, string format, params object[] args)
        {
            Write(string.Format(provider, format ?? string.Empty, args ?? Array.Empty<object>()));
        }

        /// <summary>
        /// Writes ANSI or VT escape sequences through a caller-provided action.
        /// </summary>
        /// <param name="action">The action that writes the escape sequence output.</param>
        public static void WriteAnsi(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (TryDispatch(console => console.WriteAnsi(action)))
            {
                return;
            }

            action();
        }

        /// <summary>
        /// Renders a Consoul renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Write(IConsoulRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            if (TryDispatch(console => console.Write(renderable)))
            {
                return;
            }

            renderable.Render();
        }

        /// <summary>
        /// Renders a Spectre-style renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Write(IRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            IConsoulRenderable consoulRenderable = Consoul.Renderable(renderable);
            if (TryDispatch(console => console.Write(consoulRenderable)))
            {
                return;
            }

            renderable.Render();
        }

        /// <summary>
        /// Renders a Consoul renderable component using the legacy Spectre-style method name.
        /// </summary>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Render(IConsoulRenderable renderable)
        {
            Write(renderable);
        }

        /// <summary>
        /// Renders a Spectre-style renderable component using the legacy Spectre-style method name.
        /// </summary>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Render(IRenderable renderable)
        {
            Write(renderable);
        }

        /// <summary>
        /// Writes a blank line.
        /// </summary>
        public static void WriteLine()
        {
            if (TryDispatch(console => console.WriteLine()))
            {
                return;
            }

            Consoul.Write(string.Empty);
        }

        /// <summary>
        /// Writes plain text followed by a line break.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public static void WriteLine(string text)
        {
            if (TryDispatch(console => console.WriteLine(text)))
            {
                return;
            }

            Consoul.Write(text ?? string.Empty);
        }

        /// <summary>
        /// Renders a Consoul renderable component followed by a line break.
        /// </summary>
        /// <param name="renderable">The renderable component to render.</param>
        public static void WriteLine(IConsoulRenderable renderable)
        {
            Write(renderable);
            WriteLine();
        }

        /// <summary>
        /// Renders a Spectre-style renderable component followed by a line break.
        /// </summary>
        /// <param name="renderable">The renderable component to render.</param>
        public static void WriteLine(IRenderable renderable)
        {
            Write(renderable);
            WriteLine();
        }

        /// <summary>
        /// Writes the text representation of an object followed by a line break.
        /// </summary>
        /// <param name="value">The object to write.</param>
        public static void WriteLine(object value)
        {
            WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of an object followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The object to write.</param>
        public static void WriteLine(IFormatProvider provider, object value)
        {
            if (value is IConsoulRenderable renderable)
            {
                Write(renderable);
                WriteLine();
                return;
            }

            if (value is IRenderable spectreRenderable)
            {
                Write(spectreRenderable);
                WriteLine();
                return;
            }

            WriteLine(Convert.ToString(value, provider) ?? string.Empty);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(int value)
        {
            WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(IFormatProvider provider, int value)
        {
            WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(uint value)
        {
            WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(IFormatProvider provider, uint value)
        {
            WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(long value)
        {
            WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(IFormatProvider provider, long value)
        {
            WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(ulong value)
        {
            WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(IFormatProvider provider, ulong value)
        {
            WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(float value)
        {
            WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(IFormatProvider provider, float value)
        {
            WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(double value)
        {
            WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(IFormatProvider provider, double value)
        {
            WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a decimal value followed by a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(decimal value)
        {
            WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a decimal value followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(IFormatProvider provider, decimal value)
        {
            WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a Boolean value followed by a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(bool value)
        {
            WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a Boolean value followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(IFormatProvider provider, bool value)
        {
            WriteLine(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes a Unicode character followed by a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(char value)
        {
            WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes a Unicode character followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(IFormatProvider provider, char value)
        {
            WriteLine(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes an array of Unicode characters followed by a line break.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(char[] value)
        {
            WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes an array of Unicode characters followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(IFormatProvider provider, char[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            WriteLine(new string(value));
        }

        /// <summary>
        /// Writes a formatted string followed by a line break.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void WriteLine(string format, params object[] args)
        {
            WriteLine(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes a formatted string followed by a line break.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void WriteLine(IFormatProvider provider, string format, params object[] args)
        {
            WriteLine(string.Format(provider, format ?? string.Empty, args ?? Array.Empty<object>()));
        }

        /// <summary>
        /// Writes lightweight markup without appending a line break.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        public static void Markup(string markup)
        {
            if (TryDispatch(console => console.Markup(markup)))
            {
                return;
            }

            Consoul.Markup(markup ?? string.Empty);
        }

        /// <summary>
        /// Writes formatted lightweight markup without appending a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void Markup(string format, params object[] args)
        {
            Markup(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes formatted lightweight markup without appending a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void Markup(IFormatProvider provider, string format, params object[] args)
        {
            Markup(MarkupEntry.FormatMarkup(provider, format, args));
        }

        /// <summary>
        /// Writes interpolated markup without appending a line break, escaping interpolated values.
        /// </summary>
        /// <param name="markup">The interpolated markup text to render.</param>
        public static void MarkupInterpolated(FormattableString markup)
        {
            if (markup == null)
            {
                throw new ArgumentNullException(nameof(markup));
            }

            if (TryDispatch(console => console.MarkupInterpolated(markup)))
            {
                return;
            }

            Consoul.MarkupInterpolated(markup);
        }

        /// <summary>
        /// Writes a blank markup line.
        /// </summary>
        public static void MarkupLine()
        {
            if (TryDispatch(console => console.MarkupLine()))
            {
                return;
            }

            Consoul.MarkupLine(string.Empty);
        }

        /// <summary>
        /// Writes lightweight markup followed by a line break.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        public static void MarkupLine(string markup)
        {
            if (TryDispatch(console => console.MarkupLine(markup)))
            {
                return;
            }

            Consoul.MarkupLine(markup ?? string.Empty);
        }

        /// <summary>
        /// Writes formatted lightweight markup followed by a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void MarkupLine(string format, params object[] args)
        {
            MarkupLine(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes formatted lightweight markup followed by a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void MarkupLine(IFormatProvider provider, string format, params object[] args)
        {
            MarkupLine(MarkupEntry.FormatMarkup(provider, format, args));
        }

        /// <summary>
        /// Writes interpolated markup followed by a line break, escaping interpolated values.
        /// </summary>
        /// <param name="markup">The interpolated markup text to render.</param>
        public static void MarkupLineInterpolated(FormattableString markup)
        {
            if (markup == null)
            {
                throw new ArgumentNullException(nameof(markup));
            }

            if (TryDispatch(console => console.MarkupLineInterpolated(markup)))
            {
                return;
            }

            Consoul.MarkupLineInterpolated(markup);
        }

        /// <summary>
        /// Escapes square brackets in text that will be inserted into markup.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>Markup-safe text.</returns>
        public static string EscapeMarkup(string value)
        {
            string result;
            if (TryDispatch(console => console.EscapeMarkup(value), out result))
            {
                return result;
            }

            return Consoul.EscapeMarkup(value);
        }

        /// <summary>
        /// Removes recognized markup tags from text.
        /// </summary>
        /// <param name="value">Markup text to strip.</param>
        /// <returns>Plain text with recognized markup tags removed.</returns>
        public static string RemoveMarkup(string value)
        {
            string result;
            if (TryDispatch(console => console.RemoveMarkup(value), out result))
            {
                return result;
            }

            return Consoul.RemoveMarkup(value);
        }

        /// <summary>
        /// Shows a prompt through the active console facade.
        /// </summary>
        /// <typeparam name="TResult">The prompt result type.</typeparam>
        /// <param name="prompt">The prompt to show.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The prompt result.</returns>
        public static TResult Prompt<TResult>(IPrompt<TResult> prompt, CancellationToken cancellationToken = default)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            TResult result;
            if (TryDispatch(console => console.Prompt(prompt, cancellationToken), out result))
            {
                return result;
            }

            return prompt.ShowAsync(Console, cancellationToken).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Shows a prompt asynchronously through the active console facade.
        /// </summary>
        /// <typeparam name="TResult">The prompt result type.</typeparam>
        /// <param name="prompt">The prompt to show.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the prompt result.</returns>
        public static Task<TResult> PromptAsync<TResult>(IPrompt<TResult> prompt, CancellationToken cancellationToken = default)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            Task<TResult> result;
            if (TryDispatch(console => console.PromptAsync(prompt, cancellationToken), out result))
            {
                return result;
            }

            return prompt.ShowAsync(Console, cancellationToken);
        }

        /// <summary>
        /// Renders a selection prompt and returns its explicit selection result.
        /// </summary>
        /// <param name="prompt">The selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The selection result.</returns>
        public static PromptResult Prompt(SelectionPrompt prompt, CancellationToken cancellationToken = default)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            PromptResult result;
            if (TryDispatch(console => console.Prompt(prompt, cancellationToken), out result))
            {
                return result;
            }

            return prompt.Render(cancellationToken);
        }

        /// <summary>
        /// Renders a typed selection prompt and returns the selected item result.
        /// </summary>
        /// <typeparam name="T">The option item type.</typeparam>
        /// <param name="prompt">The typed selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed selection result.</returns>
        public static SelectionResult<T> Prompt<T>(SelectionPrompt<T> prompt, CancellationToken cancellationToken = default)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            SelectionResult<T> result;
            if (TryDispatch(console => console.Prompt(prompt, cancellationToken), out result))
            {
                return result;
            }

            return prompt.RenderSelection(cancellationToken);
        }

        /// <summary>
        /// Renders a typed text prompt and returns its parsed value result.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The typed text prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed text prompt result.</returns>
        public static TextPromptResult<T> Prompt<T>(TextPrompt<T> prompt, CancellationToken cancellationToken = default)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            TextPromptResult<T> result;
            if (TryDispatch(console => console.Prompt(prompt, cancellationToken), out result))
            {
                return result;
            }

            return prompt.Render(cancellationToken);
        }

        /// <summary>
        /// Renders a confirmation prompt and returns its explicit confirmation result.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The confirmation result.</returns>
        public static ConfirmResult Prompt(ConfirmPrompt prompt, CancellationToken cancellationToken = default)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            ConfirmResult result;
            if (TryDispatch(console => console.Prompt(prompt, cancellationToken), out result))
            {
                return result;
            }

            return prompt.Render(cancellationToken);
        }

        /// <summary>
        /// Renders a secret prompt and returns its explicit secret result.
        /// </summary>
        /// <param name="prompt">The secret prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The secret prompt result.</returns>
        public static SecretResult Prompt(SecretPrompt prompt, CancellationToken cancellationToken = default)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            SecretResult result;
            if (TryDispatch(console => console.Prompt(prompt, cancellationToken), out result))
            {
                return result;
            }

            return prompt.Render(cancellationToken);
        }

        /// <summary>
        /// Renders a multi-selection prompt and returns selected indexes.
        /// </summary>
        /// <param name="prompt">The multi-selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The multi-selection result.</returns>
        public static MultiSelectionResult Prompt(MultiSelectionPrompt prompt, CancellationToken cancellationToken = default)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            MultiSelectionResult result;
            if (TryDispatch(console => console.Prompt(prompt, cancellationToken), out result))
            {
                return result;
            }

            return prompt.Render(cancellationToken);
        }

        /// <summary>
        /// Renders a typed multi-selection prompt and returns selected items.
        /// </summary>
        /// <typeparam name="T">The option item type.</typeparam>
        /// <param name="prompt">The typed multi-selection prompt to render.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> Prompt<T>(MultiSelectionPrompt<T> prompt, CancellationToken cancellationToken = default)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            MultiSelectionResult<T> result;
            if (TryDispatch(console => console.Prompt(prompt, cancellationToken), out result))
            {
                return result;
            }

            return prompt.RenderSelection(cancellationToken);
        }

        /// <summary>
        /// Creates and renders a typed text prompt.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed text prompt result.</returns>
        public static TextPromptResult<T> Ask<T>(string message, CancellationToken cancellationToken = default)
        {
            TextPromptResult<T> result;
            if (TryDispatch(console => console.Ask<T>(message, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.Text<T>(message).Render(cancellationToken);
        }

        /// <summary>
        /// Creates and renders a typed text prompt with a default value.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="defaultValue">The default value used when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed text prompt result.</returns>
        public static TextPromptResult<T> Ask<T>(string message, T defaultValue, CancellationToken cancellationToken = default)
        {
            TextPromptResult<T> result;
            if (TryDispatch(console => console.Ask(message, defaultValue, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.Text<T>(message).WithDefault(defaultValue).Render(cancellationToken);
        }

        /// <summary>
        /// Creates and renders a typed text prompt asynchronously.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the typed text prompt result.</returns>
        public static Task<TextPromptResult<T>> AskAsync<T>(string message, CancellationToken cancellationToken = default)
        {
            Task<TextPromptResult<T>> result;
            if (TryDispatch(console => console.AskAsync<T>(message, cancellationToken), out result))
            {
                return result;
            }

            return Task.FromResult(Ask<T>(message, cancellationToken));
        }

        /// <summary>
        /// Creates and renders a typed text prompt with a default value asynchronously.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="message">The prompt message.</param>
        /// <param name="defaultValue">The default value used when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the typed text prompt result.</returns>
        public static Task<TextPromptResult<T>> AskAsync<T>(string message, T defaultValue, CancellationToken cancellationToken = default)
        {
            Task<TextPromptResult<T>> result;
            if (TryDispatch(console => console.AskAsync(message, defaultValue, cancellationToken), out result))
            {
                return result;
            }

            return Task.FromResult(Ask(message, defaultValue, cancellationToken));
        }

        /// <summary>
        /// Renders a yes/no confirmation prompt.
        /// </summary>
        /// <param name="message">The confirmation message.</param>
        /// <param name="defaultValue">Optional default confirmation value.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The confirmation result.</returns>
        public static ConfirmResult Confirm(string message, bool? defaultValue = null, CancellationToken cancellationToken = default)
        {
            ConfirmResult result;
            if (TryDispatch(console => console.Confirm(message, defaultValue, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.Confirm(message, defaultValue: defaultValue, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Renders a yes/no confirmation prompt asynchronously.
        /// </summary>
        /// <param name="message">The confirmation message.</param>
        /// <param name="defaultValue">Optional default confirmation value.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the confirmation result.</returns>
        public static Task<ConfirmResult> ConfirmAsync(string message, bool? defaultValue = null, CancellationToken cancellationToken = default)
        {
            Task<ConfirmResult> result;
            if (TryDispatch(console => console.ConfirmAsync(message, defaultValue, cancellationToken), out result))
            {
                return result;
            }

            return Task.FromResult(Confirm(message, defaultValue, cancellationToken));
        }

        /// <summary>
        /// Writes an exception using Consoul's exception renderer.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="message">Optional contextual message.</param>
        /// <param name="options">Optional exception rendering options.</param>
        public static void WriteException(Exception exception, string message = null, ExceptionRenderOptions options = null)
        {
            if (TryDispatch(console => console.WriteException(exception, message, options)))
            {
                return;
            }

            Consoul.WriteException(exception, message, options);
        }

        /// <summary>
        /// Writes an exception using migration-friendly formatting flags.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="format">Formatting flags for the exception panel.</param>
        public static void WriteException(Exception exception, ExceptionFormats format)
        {
            if (TryDispatch(console => console.WriteException(exception, format)))
            {
                return;
            }

            Consoul.WriteException(exception, format);
        }

        /// <summary>
        /// Writes an exception using Spectre-style settings.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="settings">Settings used to adapt the exception panel.</param>
        public static void WriteException(Exception exception, ExceptionSettings settings)
        {
            if (TryDispatch(console => console.WriteException(exception, settings)))
            {
                return;
            }

            Consoul.WriteException(exception, settings);
        }

        /// <summary>
        /// Renders a horizontal rule.
        /// </summary>
        /// <param name="title">Optional rule title.</param>
        /// <param name="options">Optional rule rendering options.</param>
        public static void Rule(string title = null, RuleOptions options = null)
        {
            if (TryDispatch(console => console.Rule(title, options)))
            {
                return;
            }

            Consoul.Rule(title, options ?? new RuleOptions());
        }

        /// <summary>
        /// Renders content in a panel.
        /// </summary>
        /// <param name="content">The panel content.</param>
        /// <param name="options">Optional panel rendering options.</param>
        public static void Panel(string content, PanelOptions options = null)
        {
            if (TryDispatch(console => console.Panel(content, options)))
            {
                return;
            }

            Consoul.Panel(content ?? string.Empty, options ?? new PanelOptions());
        }

        /// <summary>
        /// Captures and renders a component in a panel.
        /// </summary>
        /// <param name="renderable">The component to render inside the panel.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the component.</param>
        /// <param name="captureHeight">Optional virtual console height used while capturing the component.</param>
        public static void Panel(IConsoulRenderable renderable, PanelOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            if (TryDispatch(console => console.Panel(renderable, options, captureWidth, captureHeight)))
            {
                return;
            }

            Consoul.Panel(renderable, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures and renders a delegate in a panel.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="options">Optional panel rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the action.</param>
        /// <param name="captureHeight">Optional virtual console height used while capturing the action.</param>
        public static void Panel(Action render, PanelOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            if (TryDispatch(console => console.Panel(render, options, captureWidth, captureHeight)))
            {
                return;
            }

            Consoul.Panel(render, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Renders a model-backed table.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The table items.</param>
        /// <param name="selectors">Optional property selectors for visible columns.</param>
        /// <returns>The rendered dynamic table view.</returns>
        public static DynamicTableView<T> Table<T>(IEnumerable<T> items, params Expression<Func<T, object>>[] selectors) where T : class
        {
            DynamicTableView<T> result;
            if (TryDispatch(console => console.Table(items, selectors), out result))
            {
                return result;
            }

            return Consoul.Table(items, selectors);
        }

        /// <summary>
        /// Renders a model-backed table with explicit rendering options.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The table items.</param>
        /// <param name="options">The table rendering options.</param>
        /// <param name="selectors">Optional property selectors for visible columns.</param>
        /// <returns>The rendered dynamic table view.</returns>
        public static DynamicTableView<T> Table<T>(IEnumerable<T> items, TableRenderOptions options, params Expression<Func<T, object>>[] selectors) where T : class
        {
            DynamicTableView<T> result;
            if (TryDispatch(console => console.Table(items, options, selectors), out result))
            {
                return result;
            }

            return Consoul.Table(items, options, selectors);
        }

        /// <summary>
        /// Renders a tree.
        /// </summary>
        /// <param name="root">The root node to render.</param>
        /// <param name="options">Optional tree rendering options.</param>
        public static void Tree(TreeNode root, TreeOptions options = null)
        {
            if (TryDispatch(console => console.Tree(root, options)))
            {
                return;
            }

            Consoul.Tree(root, options);
        }

        /// <summary>
        /// Renders a tree configured through a root-node callback.
        /// </summary>
        /// <param name="rootText">The root node text.</param>
        /// <param name="configure">Optional callback used to add child nodes.</param>
        /// <param name="options">Optional tree rendering options.</param>
        public static void Tree(string rootText, Action<TreeNode> configure = null, TreeOptions options = null)
        {
            if (TryDispatch(console => console.Tree(rootText, configure, options)))
            {
                return;
            }

            Consoul.Tree(rootText, configure, options);
        }

        /// <summary>
        /// Renders text columns.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        public static void Columns(params string[] columns)
        {
            if (TryDispatch(console => console.Columns(columns)))
            {
                return;
            }

            Consoul.Columns(columns);
        }

        /// <summary>
        /// Renders columns from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        public static void Columns(params object[] columns)
        {
            if (TryDispatch(console => console.Columns(columns)))
            {
                return;
            }

            Consoul.Columns(columns);
        }

        /// <summary>
        /// Renders text columns with explicit options.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        /// <param name="options">Optional column rendering options.</param>
        public static void Columns(IEnumerable<string> columns, ColumnsOptions options = null)
        {
            if (TryDispatch(console => console.Columns(columns, options)))
            {
                return;
            }

            Consoul.Columns(columns, options);
        }

        /// <summary>
        /// Renders columns from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="columns">The column values to render.</param>
        /// <param name="options">Optional column rendering options.</param>
        public static void Columns(IEnumerable<object> columns, ColumnsOptions options = null)
        {
            if (TryDispatch(console => console.Columns(columns, options)))
            {
                return;
            }

            Consoul.Columns(columns, options);
        }

        /// <summary>
        /// Renders responsive side-by-side text columns with explicit options.
        /// </summary>
        /// <param name="options">Optional column rendering options.</param>
        /// <param name="columns">The column values to render.</param>
        public static void Columns(ColumnsOptions options, params string[] columns)
        {
            if (TryDispatch(console => console.Columns(options, columns)))
            {
                return;
            }

            Consoul.Columns(options, columns);
        }

        /// <summary>
        /// Renders responsive side-by-side columns from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Optional column rendering options.</param>
        /// <param name="columns">The column values to render.</param>
        public static void Columns(ColumnsOptions options, params object[] columns)
        {
            if (TryDispatch(console => console.Columns(options, columns)))
            {
                return;
            }

            Consoul.Columns(options, columns);
        }

        /// <summary>
        /// Builds and renders responsive side-by-side text columns.
        /// </summary>
        /// <param name="configure">Action used to add columns before rendering.</param>
        /// <param name="options">Optional column rendering options.</param>
        public static void Columns(Action<ColumnsEntry> configure, ColumnsOptions options = null)
        {
            if (TryDispatch(console => console.Columns(configure, options)))
            {
                return;
            }

            Consoul.Columns(configure, options);
        }

        private static bool TryDispatch(Action<IAnsiConsole> action)
        {
            IAnsiConsole console = _console;
            var consoulConsole = console as ConsoulAnsiConsole;
            if (DispatchSuppressionDepth.Value > 0 || ReferenceEquals(console, DefaultConsole) || (consoulConsole != null && !consoulConsole.UsesScopedDriver))
            {
                return false;
            }

            action(console);
            return true;
        }

        private static bool TryDispatch<TResult>(Func<IAnsiConsole, TResult> action, out TResult result)
        {
            IAnsiConsole console = _console;
            var consoulConsole = console as ConsoulAnsiConsole;
            if (DispatchSuppressionDepth.Value > 0 || ReferenceEquals(console, DefaultConsole) || (consoulConsole != null && !consoulConsole.UsesScopedDriver))
            {
                result = default(TResult);
                return false;
            }

            result = action(console);
            return true;
        }

        private static RecordingConsoleDriver GetRecordingDriver()
        {
            if (_recordingDriver == null)
            {
                throw new InvalidOperationException("Cannot export output since recording has not been started.");
            }

            return _recordingDriver;
        }

        private sealed class DispatchSuppressionScope : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                DispatchSuppressionDepth.Value = Math.Max(0, DispatchSuppressionDepth.Value - 1);
            }
        }

        /// <summary>
        /// Writes a compact single-line badge.
        /// </summary>
        /// <param name="label">The badge label to render.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional badge rendering options.</param>
        /// <param name="writeLine">Whether to append a line break after the badge.</param>
        public static void Badge(string label, BadgeKind kind = BadgeKind.Default, BadgeOptions options = null, bool writeLine = true)
        {
            if (TryDispatch(console => console.Badge(label, kind, options, writeLine)))
            {
                return;
            }

            Consoul.Badge(label, kind, options, writeLine);
        }

        /// <summary>
        /// Creates compact badge text without writing to the console.
        /// </summary>
        /// <param name="label">The badge label.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional badge rendering options.</param>
        /// <param name="maxWidth">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded badge string.</returns>
        public static string BadgeText(string label, BadgeKind kind = BadgeKind.Default, BadgeOptions options = null, int? maxWidth = null)
        {
            string result;
            if (TryDispatch(console => console.BadgeText(label, kind, options, maxWidth), out result))
            {
                return result;
            }

            return Consoul.BadgeText(label, kind, options, maxWidth);
        }

        /// <summary>
        /// Renders a responsive headline.
        /// </summary>
        /// <param name="text">The headline text to render.</param>
        /// <param name="options">Headline rendering options.</param>
        public static void Headline(string text, HeadlineOptions options)
        {
            if (TryDispatch(console => console.Headline(text, options)))
            {
                return;
            }

            Consoul.Headline(text, options);
        }

        /// <summary>
        /// Renders a responsive headline.
        /// </summary>
        /// <param name="text">The headline text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="style">The headline rendering style.</param>
        public static void Headline(string text, int? width = null, ConsoleColor? color = null, HeadlineStyle style = HeadlineStyle.Block)
        {
            if (TryDispatch(console => console.Headline(text, width, color, style)))
            {
                return;
            }

            Consoul.Headline(text, width, color, style);
        }

        /// <summary>
        /// Renders large ASCII text.
        /// </summary>
        /// <param name="text">The text to render as large glyphs.</param>
        /// <param name="options">Figlet rendering options.</param>
        public static void FigletText(string text, FigletTextOptions options)
        {
            if (TryDispatch(console => console.FigletText(text, options)))
            {
                return;
            }

            Consoul.FigletText(text, options);
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
        public static void FigletText(
            string text,
            int? width = null,
            TextBlockAlignment alignment = TextBlockAlignment.Center,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null,
            char character = '#')
        {
            if (TryDispatch(console => console.FigletText(text, width, alignment, color, backgroundColor, character)))
            {
                return;
            }

            Consoul.FigletText(text, width, alignment, color, backgroundColor, character);
        }

        /// <summary>
        /// Renders a responsive month calendar.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        /// <param name="options">Calendar rendering options.</param>
        public static void Calendar(DateTime month, CalendarOptions options = null)
        {
            if (TryDispatch(console => console.Calendar(month, options)))
            {
                return;
            }

            Consoul.Calendar(month, options);
        }

        /// <summary>
        /// Renders a responsive month calendar.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="options">Calendar rendering options.</param>
        public static void Calendar(int year, int month, CalendarOptions options = null)
        {
            if (TryDispatch(console => console.Calendar(year, month, options)))
            {
                return;
            }

            Consoul.Calendar(year, month, options);
        }

        /// <summary>
        /// Builds and renders a responsive month calendar.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        /// <param name="configure">Action used to mark days before rendering.</param>
        /// <param name="options">Calendar rendering options.</param>
        public static void Calendar(DateTime month, Action<CalendarEntry> configure, CalendarOptions options = null)
        {
            if (TryDispatch(console => console.Calendar(month, configure, options)))
            {
                return;
            }

            Consoul.Calendar(month, configure, options);
        }

        /// <summary>
        /// Builds and renders a responsive month calendar.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="configure">Action used to mark days before rendering.</param>
        /// <param name="options">Calendar rendering options.</param>
        public static void Calendar(int year, int month, Action<CalendarEntry> configure, CalendarOptions options = null)
        {
            if (TryDispatch(console => console.Calendar(year, month, configure, options)))
            {
                return;
            }

            Consoul.Calendar(year, month, configure, options);
        }

        /// <summary>
        /// Renders a responsive intent-colored callout.
        /// </summary>
        /// <param name="message">The callout message.</param>
        /// <param name="kind">The callout visual intent.</param>
        /// <param name="options">Optional callout rendering options.</param>
        public static void Callout(string message, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null)
        {
            if (TryDispatch(console => console.Callout(message, kind, options)))
            {
                return;
            }

            Consoul.Callout(message, kind, options);
        }

        /// <summary>
        /// Captures a render action and renders the captured output inside a callout.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="kind">The callout visual intent.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public static void Callout(Action render, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            if (TryDispatch(console => console.Callout(render, kind, options, captureWidth, captureHeight)))
            {
                return;
            }

            Consoul.Callout(render, kind, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a renderable and renders the captured output inside a callout.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="kind">The callout visual intent.</param>
        /// <param name="options">Optional callout rendering options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public static void Callout(IConsoulRenderable renderable, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            if (TryDispatch(console => console.Callout(renderable, kind, options, captureWidth, captureHeight)))
            {
                return;
            }

            Consoul.Callout(renderable, kind, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Validates a model without rendering output.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <returns>The validation result for the model.</returns>
        public static ValidationSummaryResult ValidateModel(object model, ValidationSummaryOptions options = null)
        {
            ValidationSummaryResult result;
            if (TryDispatch(console => console.ValidateModel(model, options), out result))
            {
                return result;
            }

            return Consoul.ValidateModel(model, options);
        }

        /// <summary>
        /// Validates a model and renders a validation summary when issues are present.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <returns>The validation result for the model.</returns>
        public static ValidationSummaryResult ValidationSummary(object model, ValidationSummaryOptions options = null)
        {
            ValidationSummaryResult result;
            if (TryDispatch(console => console.ValidationSummary(model, options), out result))
            {
                return result;
            }

            return Consoul.ValidationSummary(model, options);
        }

        /// <summary>
        /// Renders a responsive timeline.
        /// </summary>
        /// <param name="items">The timeline items to render.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        public static void Timeline(IEnumerable<TimelineItem> items, TimelineOptions options = null)
        {
            if (TryDispatch(console => console.Timeline(items, options)))
            {
                return;
            }

            Consoul.Timeline(items, options);
        }

        /// <summary>
        /// Renders a responsive timeline.
        /// </summary>
        /// <param name="options">Optional timeline rendering options.</param>
        /// <param name="items">The timeline items to render.</param>
        public static void Timeline(TimelineOptions options, params TimelineItem[] items)
        {
            if (TryDispatch(console => console.Timeline(options, items)))
            {
                return;
            }

            Consoul.Timeline(options, items);
        }

        /// <summary>
        /// Configures and renders a responsive timeline.
        /// </summary>
        /// <param name="configure">Action used to add timeline items.</param>
        /// <param name="options">Optional timeline rendering options.</param>
        public static void Timeline(Action<TimelineEntry> configure, TimelineOptions options = null)
        {
            if (TryDispatch(console => console.Timeline(configure, options)))
            {
                return;
            }

            Consoul.Timeline(configure, options);
        }

        /// <summary>
        /// Renders a responsive bullet or numbered list.
        /// </summary>
        /// <param name="items">The list items to render.</param>
        /// <param name="options">Optional list rendering options.</param>
        public static void BulletList(IEnumerable<BulletListItem> items, BulletListOptions options = null)
        {
            if (TryDispatch(console => console.BulletList(items, options)))
            {
                return;
            }

            Consoul.BulletList(items, options);
        }

        /// <summary>
        /// Renders a responsive bullet or numbered list.
        /// </summary>
        /// <param name="options">Optional list rendering options.</param>
        /// <param name="items">The list items to render.</param>
        public static void BulletList(BulletListOptions options, params BulletListItem[] items)
        {
            if (TryDispatch(console => console.BulletList(options, items)))
            {
                return;
            }

            Consoul.BulletList(options, items);
        }

        /// <summary>
        /// Configures and renders a responsive bullet or numbered list.
        /// </summary>
        /// <param name="configure">Action used to add list items.</param>
        /// <param name="options">Optional list rendering options.</param>
        public static void BulletList(Action<BulletListEntry> configure, BulletListOptions options = null)
        {
            if (TryDispatch(console => console.BulletList(configure, options)))
            {
                return;
            }

            Consoul.BulletList(configure, options);
        }

        /// <summary>
        /// Renders responsive grid cells.
        /// </summary>
        /// <param name="cells">The cell contents to render.</param>
        public static void Grid(params string[] cells)
        {
            if (TryDispatch(console => console.Grid(cells)))
            {
                return;
            }

            Consoul.Grid(cells);
        }

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="cells">The cell values to render.</param>
        public static void Grid(params object[] cells)
        {
            if (TryDispatch(console => console.Grid(cells)))
            {
                return;
            }

            Consoul.Grid(cells);
        }

        /// <summary>
        /// Renders responsive grid cells.
        /// </summary>
        /// <param name="cells">The cell contents to render.</param>
        /// <param name="options">Optional grid rendering options.</param>
        public static void Grid(IEnumerable<string> cells, GridOptions options = null)
        {
            if (TryDispatch(console => console.Grid(cells, options)))
            {
                return;
            }

            Consoul.Grid(cells, options);
        }

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="cells">The cell values to render.</param>
        /// <param name="options">Optional grid rendering options.</param>
        public static void Grid(IEnumerable<object> cells, GridOptions options = null)
        {
            if (TryDispatch(console => console.Grid(cells, options)))
            {
                return;
            }

            Consoul.Grid(cells, options);
        }

        /// <summary>
        /// Renders responsive grid cells with explicit options.
        /// </summary>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="cells">The cell contents to render.</param>
        public static void Grid(GridOptions options, params string[] cells)
        {
            if (TryDispatch(console => console.Grid(options, cells)))
            {
                return;
            }

            Consoul.Grid(options, cells);
        }

        /// <summary>
        /// Renders responsive grid cells from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Optional grid rendering options.</param>
        /// <param name="cells">The cell values to render.</param>
        public static void Grid(GridOptions options, params object[] cells)
        {
            if (TryDispatch(console => console.Grid(options, cells)))
            {
                return;
            }

            Consoul.Grid(options, cells);
        }

        /// <summary>
        /// Configures and renders responsive grid cells.
        /// </summary>
        /// <param name="configure">Action used to add cells.</param>
        /// <param name="options">Optional grid rendering options.</param>
        public static void Grid(Action<GridEntry> configure, GridOptions options = null)
        {
            if (TryDispatch(console => console.Grid(configure, options)))
            {
                return;
            }

            Consoul.Grid(configure, options);
        }

        /// <summary>
        /// Builds and renders a small ASCII canvas.
        /// </summary>
        /// <param name="width">The logical canvas width.</param>
        /// <param name="height">The logical canvas height.</param>
        /// <param name="configure">Action used to draw on the canvas.</param>
        /// <param name="options">Optional canvas rendering options.</param>
        public static void Canvas(int width, int height, Action<CanvasEntry> configure, CanvasOptions options = null)
        {
            if (TryDispatch(console => console.Canvas(width, height, configure, options)))
            {
                return;
            }

            Consoul.Canvas(width, height, configure, options);
        }

        /// <summary>
        /// Renders an existing ASCII canvas.
        /// </summary>
        /// <param name="canvas">The canvas to render.</param>
        public static void Canvas(CanvasEntry canvas)
        {
            if (TryDispatch(console => console.Canvas(canvas)))
            {
                return;
            }

            Consoul.Canvas(canvas);
        }

        /// <summary>
        /// Renders an existing dependency-free terminal canvas image.
        /// </summary>
        /// <param name="image">The image to render.</param>
        public static void CanvasImage(CanvasImageEntry image)
        {
            if (TryDispatch(console => console.CanvasImage(image)))
            {
                return;
            }

            Consoul.CanvasImage(image);
        }

        /// <summary>
        /// Renders a dependency-free terminal canvas image from a row-major pixel matrix.
        /// </summary>
        /// <param name="pixels">The source pixels, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional image rendering options.</param>
        public static void CanvasImage(CanvasImagePixel[,] pixels, CanvasImageOptions options = null)
        {
            if (TryDispatch(console => console.CanvasImage(pixels, options)))
            {
                return;
            }

            Consoul.CanvasImage(pixels, options);
        }

        /// <summary>
        /// Renders a dependency-free terminal canvas image from a row-major console color matrix.
        /// </summary>
        /// <param name="pixels">The source colors, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional image rendering options.</param>
        public static void CanvasImage(ConsoleColor[,] pixels, CanvasImageOptions options = null)
        {
            if (TryDispatch(console => console.CanvasImage(pixels, options)))
            {
                return;
            }

            Consoul.CanvasImage(pixels, options);
        }

        /// <summary>
        /// Renders vertically stacked rows.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        public static void Rows(params string[] rows)
        {
            if (TryDispatch(console => console.Rows(rows)))
            {
                return;
            }

            Consoul.Rows(rows);
        }

        /// <summary>
        /// Renders vertically stacked rows from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        public static void Rows(params object[] rows)
        {
            if (TryDispatch(console => console.Rows(rows)))
            {
                return;
            }

            Consoul.Rows(rows);
        }

        /// <summary>
        /// Renders vertically stacked rows.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        /// <param name="options">Optional row rendering options.</param>
        public static void Rows(IEnumerable<string> rows, RowsOptions options = null)
        {
            if (TryDispatch(console => console.Rows(rows, options)))
            {
                return;
            }

            Consoul.Rows(rows, options);
        }

        /// <summary>
        /// Renders vertically stacked rows from mixed plain values and reusable renderables.
        /// </summary>
        /// <param name="rows">The row values to render.</param>
        /// <param name="options">Optional row rendering options.</param>
        public static void Rows(IEnumerable<object> rows, RowsOptions options = null)
        {
            if (TryDispatch(console => console.Rows(rows, options)))
            {
                return;
            }

            Consoul.Rows(rows, options);
        }

        /// <summary>
        /// Renders vertically stacked rows with explicit options.
        /// </summary>
        /// <param name="options">Optional row rendering options.</param>
        /// <param name="rows">The row contents to render.</param>
        public static void Rows(RowsOptions options, params string[] rows)
        {
            if (TryDispatch(console => console.Rows(options, rows)))
            {
                return;
            }

            Consoul.Rows(options, rows);
        }

        /// <summary>
        /// Renders vertically stacked rows from mixed plain values and reusable renderables with explicit options.
        /// </summary>
        /// <param name="options">Optional row rendering options.</param>
        /// <param name="rows">The row values to render.</param>
        public static void Rows(RowsOptions options, params object[] rows)
        {
            if (TryDispatch(console => console.Rows(options, rows)))
            {
                return;
            }

            Consoul.Rows(options, rows);
        }

        /// <summary>
        /// Configures and renders vertically stacked rows.
        /// </summary>
        /// <param name="configure">Action used to add rows.</param>
        /// <param name="options">Optional row rendering options.</param>
        public static void Rows(Action<RowsEntry> configure, RowsOptions options = null)
        {
            if (TryDispatch(console => console.Rows(configure, options)))
            {
                return;
            }

            Consoul.Rows(configure, options);
        }

        /// <summary>
        /// Renders responsive layout panes.
        /// </summary>
        /// <param name="panes">The panes to render.</param>
        /// <param name="options">Optional layout rendering options.</param>
        public static void Layout(IEnumerable<LayoutPane> panes, LayoutOptions options = null)
        {
            if (TryDispatch(console => console.Layout(panes, options)))
            {
                return;
            }

            Consoul.Layout(panes, options);
        }

        /// <summary>
        /// Configures and renders responsive layout panes.
        /// </summary>
        /// <param name="configure">Action used to add panes.</param>
        /// <param name="options">Optional layout rendering options.</param>
        public static void Layout(Action<LayoutEntry> configure, LayoutOptions options = null)
        {
            if (TryDispatch(console => console.Layout(configure, options)))
            {
                return;
            }

            Consoul.Layout(configure, options);
        }

        /// <summary>
        /// Renders text with responsive padding.
        /// </summary>
        /// <param name="content">The text content to pad.</param>
        /// <param name="options">Padding options.</param>
        public static void Pad(string content, PadOptions options)
        {
            if (TryDispatch(console => console.Pad(content, options)))
            {
                return;
            }

            Consoul.Pad(content, options);
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
            if (TryDispatch(console => console.Pad(content, left, right, top, bottom, width, color, backgroundColor)))
            {
                return;
            }

            Consoul.Pad(content, left, right, top, bottom, width, color, backgroundColor);
        }

        /// <summary>
        /// Captures a render action and renders the captured text with responsive padding.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="options">Padding options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public static void Pad(Action render, PadOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            if (TryDispatch(console => console.Pad(render, options, captureWidth, captureHeight)))
            {
                return;
            }

            Consoul.Pad(render, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a renderable and renders the captured text with responsive padding.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="options">Padding options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public static void Pad(IConsoulRenderable renderable, PadOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            if (TryDispatch(console => console.Pad(renderable, options, captureWidth, captureHeight)))
            {
                return;
            }

            Consoul.Pad(renderable, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Renders text aligned inside a responsive width.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="options">Alignment options.</param>
        public static void Align(string content, AlignOptions options)
        {
            if (TryDispatch(console => console.Align(content, options)))
            {
                return;
            }

            Consoul.Align(content, options);
        }

        /// <summary>
        /// Renders text aligned inside a responsive width.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="alignment">Alignment used for content lines.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        public static void Align(
            string content,
            TextBlockAlignment alignment = TextBlockAlignment.Left,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null)
        {
            if (TryDispatch(console => console.Align(content, alignment, width, color, backgroundColor)))
            {
                return;
            }

            Consoul.Align(content, alignment, width, color, backgroundColor);
        }

        /// <summary>
        /// Captures a render action and renders the captured text aligned inside a responsive width.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="options">Alignment options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public static void Align(Action render, AlignOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            if (TryDispatch(console => console.Align(render, options, captureWidth, captureHeight)))
            {
                return;
            }

            Consoul.Align(render, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a renderable and renders the captured text aligned inside a responsive width.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="options">Alignment options.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing.</param>
        /// <param name="captureHeight">Virtual console height used while capturing.</param>
        public static void Align(IConsoulRenderable renderable, AlignOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            if (TryDispatch(console => console.Align(renderable, options, captureWidth, captureHeight)))
            {
                return;
            }

            Consoul.Align(renderable, options, captureWidth, captureHeight);
        }

        /// <summary>
        /// Renders wrapped, aligned, and padded text.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="options">Text block options.</param>
        public static void TextBlock(string content, TextBlockOptions options)
        {
            if (TryDispatch(console => console.TextBlock(content, options)))
            {
                return;
            }

            Consoul.TextBlock(content, options);
        }

        /// <summary>
        /// Renders wrapped, aligned, and padded text.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="alignment">Alignment used for wrapped lines.</param>
        /// <param name="color">Optional foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        public static void TextBlock(
            string content,
            int? width = null,
            TextBlockAlignment alignment = TextBlockAlignment.Left,
            ConsoleColor? color = null,
            ConsoleColor? backgroundColor = null)
        {
            if (TryDispatch(console => console.TextBlock(content, width, alignment, color, backgroundColor)))
            {
                return;
            }

            Consoul.TextBlock(content, width, alignment, color, backgroundColor);
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="options">Path rendering options.</param>
        public static void TextPath(string path, TextPathOptions options)
        {
            if (TryDispatch(console => console.TextPath(path, options)))
            {
                return;
            }

            Consoul.TextPath(path, options);
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="directoryColor">Optional directory segment color.</param>
        /// <param name="fileNameColor">Optional file-name color.</param>
        /// <param name="separatorColor">Optional separator color.</param>
        public static void TextPath(
            string path,
            int? width = null,
            ConsoleColor? directoryColor = null,
            ConsoleColor? fileNameColor = null,
            ConsoleColor? separatorColor = null)
        {
            if (TryDispatch(console => console.TextPath(path, width, directoryColor, fileNameColor, separatorColor)))
            {
                return;
            }

            Consoul.TextPath(path, width, directoryColor, fileNameColor, separatorColor);
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path using Spectre-style style text.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="directoryStyle">Optional directory segment style.</param>
        /// <param name="fileNameStyle">Optional file-name style.</param>
        /// <param name="separatorStyle">Optional separator style.</param>
        public static void TextPath(
            string path,
            int? width,
            string directoryStyle,
            string fileNameStyle = null,
            string separatorStyle = null)
        {
            if (TryDispatch(console => console.TextPath(path, width, directoryStyle, fileNameStyle, separatorStyle)))
            {
                return;
            }

            Consoul.TextPath(path, width, directoryStyle, fileNameStyle, separatorStyle);
        }

        /// <summary>
        /// Renders a responsive, colorized terminal path using Spectre-style style text.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="directoryStyle">Optional directory segment style.</param>
        /// <param name="fileNameStyle">Optional file-name style.</param>
        /// <param name="separatorStyle">Optional separator style.</param>
        public static void TextPathStyled(
            string path,
            int? width = null,
            string directoryStyle = null,
            string fileNameStyle = null,
            string separatorStyle = null)
        {
            if (TryDispatch(console => console.TextPathStyled(path, width, directoryStyle, fileNameStyle, separatorStyle)))
            {
                return;
            }

            Consoul.TextPathStyled(path, width, directoryStyle, fileNameStyle, separatorStyle);
        }

        /// <summary>
        /// Renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="items">The breadcrumb items to render.</param>
        public static void Breadcrumb(params string[] items)
        {
            if (TryDispatch(console => console.Breadcrumb(items)))
            {
                return;
            }

            Consoul.Breadcrumb(items);
        }

        /// <summary>
        /// Renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="items">The breadcrumb items to render.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        public static void Breadcrumb(IEnumerable<string> items, BreadcrumbOptions options = null)
        {
            if (TryDispatch(console => console.Breadcrumb(items, options)))
            {
                return;
            }

            Consoul.Breadcrumb(items, options);
        }

        /// <summary>
        /// Renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="items">The breadcrumb items to render.</param>
        public static void Breadcrumb(BreadcrumbOptions options, params string[] items)
        {
            if (TryDispatch(console => console.Breadcrumb(options, items)))
            {
                return;
            }

            Consoul.Breadcrumb(options, items);
        }

        /// <summary>
        /// Configures and renders a responsive breadcrumb trail.
        /// </summary>
        /// <param name="configure">Action used to add breadcrumb items.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        public static void Breadcrumb(Action<BreadcrumbEntry> configure, BreadcrumbOptions options = null)
        {
            if (TryDispatch(console => console.Breadcrumb(configure, options)))
            {
                return;
            }

            Consoul.Breadcrumb(configure, options);
        }

        /// <summary>
        /// Creates responsive breadcrumb text without writing to the console.
        /// </summary>
        /// <param name="items">The breadcrumb items.</param>
        /// <returns>A bounded breadcrumb string.</returns>
        public static string BreadcrumbText(params string[] items)
        {
            string result;
            if (TryDispatch(console => console.BreadcrumbText(items), out result))
            {
                return result;
            }

            return Consoul.BreadcrumbText(items);
        }

        /// <summary>
        /// Creates responsive breadcrumb text without writing to the console.
        /// </summary>
        /// <param name="items">The breadcrumb items.</param>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded breadcrumb string.</returns>
        public static string BreadcrumbText(IEnumerable<string> items, BreadcrumbOptions options = null, int? width = null)
        {
            string result;
            if (TryDispatch(console => console.BreadcrumbText(items, options, width), out result))
            {
                return result;
            }

            return Consoul.BreadcrumbText(items, options, width);
        }

        /// <summary>
        /// Creates responsive breadcrumb text without writing to the console.
        /// </summary>
        /// <param name="options">Optional breadcrumb rendering options.</param>
        /// <param name="items">The breadcrumb items.</param>
        /// <returns>A bounded breadcrumb string.</returns>
        public static string BreadcrumbText(BreadcrumbOptions options, params string[] items)
        {
            string result;
            if (TryDispatch(console => console.BreadcrumbText(options, items), out result))
            {
                return result;
            }

            return Consoul.BreadcrumbText(options, items);
        }

        /// <summary>
        /// Renders a responsive shortcut bar.
        /// </summary>
        /// <param name="items">The shortcut items to render.</param>
        public static void ShortcutBar(params ShortcutItem[] items)
        {
            if (TryDispatch(console => console.ShortcutBar(items)))
            {
                return;
            }

            Consoul.ShortcutBar(items);
        }

        /// <summary>
        /// Renders a responsive shortcut bar.
        /// </summary>
        /// <param name="items">The shortcut items to render.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        public static void ShortcutBar(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null)
        {
            if (TryDispatch(console => console.ShortcutBar(items, options)))
            {
                return;
            }

            Consoul.ShortcutBar(items, options);
        }

        /// <summary>
        /// Renders a responsive shortcut bar.
        /// </summary>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="items">The shortcut items to render.</param>
        public static void ShortcutBar(ShortcutBarOptions options, params ShortcutItem[] items)
        {
            if (TryDispatch(console => console.ShortcutBar(options, items)))
            {
                return;
            }

            Consoul.ShortcutBar(options, items);
        }

        /// <summary>
        /// Configures and renders a responsive shortcut bar.
        /// </summary>
        /// <param name="configure">Action used to add shortcut items.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        public static void ShortcutBar(Action<ShortcutBarEntry> configure, ShortcutBarOptions options = null)
        {
            if (TryDispatch(console => console.ShortcutBar(configure, options)))
            {
                return;
            }

            Consoul.ShortcutBar(configure, options);
        }

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="items">The shortcut items.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        public static string ShortcutBarText(params ShortcutItem[] items)
        {
            string result;
            if (TryDispatch(console => console.ShortcutBarText(items), out result))
            {
                return result;
            }

            return Consoul.ShortcutBarText(items);
        }

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="items">The shortcut items.</param>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        public static string ShortcutBarText(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null, int? width = null)
        {
            string result;
            if (TryDispatch(console => console.ShortcutBarText(items, options, width), out result))
            {
                return result;
            }

            return Consoul.ShortcutBarText(items, options, width);
        }

        /// <summary>
        /// Creates responsive shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="options">Optional shortcut bar rendering options.</param>
        /// <param name="items">The shortcut items.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        public static string ShortcutBarText(ShortcutBarOptions options, params ShortcutItem[] items)
        {
            string result;
            if (TryDispatch(console => console.ShortcutBarText(options, items), out result))
            {
                return result;
            }

            return Consoul.ShortcutBarText(options, items);
        }

        /// <summary>
        /// Renders a responsive checklist.
        /// </summary>
        /// <param name="items">The checklist items to render.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        public static void Checklist(IEnumerable<ChecklistItem> items, ChecklistOptions options = null)
        {
            if (TryDispatch(console => console.Checklist(items, options)))
            {
                return;
            }

            Consoul.Checklist(items, options);
        }

        /// <summary>
        /// Renders a responsive checklist.
        /// </summary>
        /// <param name="options">Optional checklist rendering options.</param>
        /// <param name="items">The checklist items to render.</param>
        public static void Checklist(ChecklistOptions options, params ChecklistItem[] items)
        {
            if (TryDispatch(console => console.Checklist(options, items)))
            {
                return;
            }

            Consoul.Checklist(options, items);
        }

        /// <summary>
        /// Configures and renders a responsive checklist.
        /// </summary>
        /// <param name="configure">Action used to add checklist items.</param>
        /// <param name="options">Optional checklist rendering options.</param>
        public static void Checklist(Action<ChecklistEntry> configure, ChecklistOptions options = null)
        {
            if (TryDispatch(console => console.Checklist(configure, options)))
            {
                return;
            }

            Consoul.Checklist(configure, options);
        }

        /// <summary>
        /// Renders a responsive list of labeled values.
        /// </summary>
        /// <param name="items">The property list items to render.</param>
        /// <param name="options">Optional property list rendering options.</param>
        public static void PropertyList(IEnumerable<PropertyListItem> items, PropertyListOptions options = null)
        {
            if (TryDispatch(console => console.PropertyList(items, options)))
            {
                return;
            }

            Consoul.PropertyList(items, options);
        }

        /// <summary>
        /// Renders readable model properties as a responsive property list.
        /// </summary>
        /// <param name="model">The model object whose properties should be summarized.</param>
        /// <param name="options">Optional property list rendering options.</param>
        public static void PropertyList(object model, PropertyListOptions options = null)
        {
            if (TryDispatch(console => console.PropertyList(model, options)))
            {
                return;
            }

            Consoul.PropertyList(model, options);
        }

        /// <summary>
        /// Configures and renders a responsive property list.
        /// </summary>
        /// <param name="configure">Action used to add property list items.</param>
        /// <param name="options">Optional property list rendering options.</param>
        public static void PropertyList(Action<PropertyListEntry> configure, PropertyListOptions options = null)
        {
            if (TryDispatch(console => console.PropertyList(configure, options)))
            {
                return;
            }

            Consoul.PropertyList(configure, options);
        }

        /// <summary>
        /// Renders compact responsive metrics.
        /// </summary>
        /// <param name="items">The metric items to render.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        public static void Metrics(IEnumerable<MetricItem> items, MetricsOptions options = null)
        {
            if (TryDispatch(console => console.Metrics(items, options)))
            {
                return;
            }

            Consoul.Metrics(items, options);
        }

        /// <summary>
        /// Renders compact responsive metrics.
        /// </summary>
        /// <param name="options">Optional metrics rendering options.</param>
        /// <param name="items">The metric items to render.</param>
        public static void Metrics(MetricsOptions options, params MetricItem[] items)
        {
            if (TryDispatch(console => console.Metrics(options, items)))
            {
                return;
            }

            Consoul.Metrics(options, items);
        }

        /// <summary>
        /// Configures and renders compact responsive metrics.
        /// </summary>
        /// <param name="configure">Action used to add metric items.</param>
        /// <param name="options">Optional metrics rendering options.</param>
        public static void Metrics(Action<MetricsEntry> configure, MetricsOptions options = null)
        {
            if (TryDispatch(console => console.Metrics(configure, options)))
            {
                return;
            }

            Consoul.Metrics(configure, options);
        }

        /// <summary>
        /// Renders a responsive horizontal bar chart.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Optional chart rendering options.</param>
        public static void BarChart(IEnumerable<BarChartItem> items, BarChartOptions options = null)
        {
            if (TryDispatch(console => console.BarChart(items, options)))
            {
                return;
            }

            Consoul.BarChart(items, options);
        }

        /// <summary>
        /// Configures and renders a responsive horizontal bar chart.
        /// </summary>
        /// <param name="configure">Action used to add chart items.</param>
        /// <param name="options">Optional chart rendering options.</param>
        public static void BarChart(Action<BarChartEntry> configure, BarChartOptions options = null)
        {
            if (TryDispatch(console => console.BarChart(configure, options)))
            {
                return;
            }

            Consoul.BarChart(configure, options);
        }

        /// <summary>
        /// Renders a responsive breakdown chart.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Optional chart rendering options.</param>
        public static void BreakdownChart(IEnumerable<BreakdownChartItem> items, BreakdownChartOptions options = null)
        {
            if (TryDispatch(console => console.BreakdownChart(items, options)))
            {
                return;
            }

            Consoul.BreakdownChart(items, options);
        }

        /// <summary>
        /// Configures and renders a responsive breakdown chart.
        /// </summary>
        /// <param name="configure">Action used to add chart items.</param>
        /// <param name="options">Optional chart rendering options.</param>
        public static void BreakdownChart(Action<BreakdownChartEntry> configure, BreakdownChartOptions options = null)
        {
            if (TryDispatch(console => console.BreakdownChart(configure, options)))
            {
                return;
            }

            Consoul.BreakdownChart(configure, options);
        }

        /// <summary>
        /// Renders a compact responsive sparkline chart.
        /// </summary>
        /// <param name="values">The values rendered by the sparkline.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        public static void Sparkline(IEnumerable<double> values, SparklineOptions options = null)
        {
            if (TryDispatch(console => console.Sparkline(values, options)))
            {
                return;
            }

            Consoul.Sparkline(values, options);
        }

        /// <summary>
        /// Renders a compact responsive sparkline chart.
        /// </summary>
        /// <param name="values">The values rendered by the sparkline.</param>
        /// <param name="label">Optional label rendered before the sparkline.</param>
        /// <param name="width">Optional requested width.</param>
        /// <param name="color">Optional sparkline foreground color.</param>
        /// <param name="showRange">Whether the rendered value range should appear after the sparkline.</param>
        public static void Sparkline(
            IEnumerable<double> values,
            string label = null,
            int? width = null,
            ConsoleColor? color = null,
            bool showRange = false)
        {
            if (TryDispatch(console => console.Sparkline(values, label, width, color, showRange)))
            {
                return;
            }

            Consoul.Sparkline(values, label, width, color, showRange);
        }

        /// <summary>
        /// Configures and renders a compact responsive sparkline chart.
        /// </summary>
        /// <param name="configure">Action used to add values.</param>
        /// <param name="options">Optional sparkline rendering options.</param>
        public static void Sparkline(Action<SparklineEntry> configure, SparklineOptions options = null)
        {
            if (TryDispatch(console => console.Sparkline(configure, options)))
            {
                return;
            }

            Consoul.Sparkline(configure, options);
        }

        /// <summary>
        /// Creates a status builder that can start status scopes with a Spectre-style workflow.
        /// </summary>
        /// <returns>A status builder.</returns>
        public static StatusBuilder Status()
        {
            StatusBuilder result;
            if (TryDispatch(console => console.Status(), out result))
            {
                return result;
            }

            return new StatusBuilder();
        }

        /// <summary>
        /// Creates an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <returns>The status scope.</returns>
        public static StatusScope Status(string message, StatusOptions options = null)
        {
            StatusScope result;
            if (TryDispatch(console => console.Status(message, options), out result))
            {
                return result;
            }

            return Consoul.Status(message, options);
        }

        /// <summary>
        /// Runs an action inside an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The scoped action.</param>
        /// <param name="options">Optional status rendering options.</param>
        public static void Status(string message, Action<StatusScope> action, StatusOptions options = null)
        {
            if (TryDispatch(console => console.Status(message, action, options)))
            {
                return;
            }

            Consoul.Status(message, action, options);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task StatusAsync(string message, Func<StatusScope, Task> action, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            Task result;
            if (TryDispatch(console => console.StatusAsync(message, action, options, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.StatusAsync(message, action, options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> StatusAsync<T>(string message, Func<StatusScope, Task<T>> func, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return StatusAsync<T>(message, (status, token) => func(status), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task StatusAsync(string message, Func<StatusScope, CancellationToken, Task> action, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            Task result;
            if (TryDispatch(console => console.StatusAsync(message, action, options, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.StatusAsync(message, action, options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> StatusAsync<T>(string message, Func<StatusScope, CancellationToken, Task<T>> func, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return AnsiConsoleExtensions.StatusAsync<T>(Console, message, func, options, cancellationToken);
        }

        /// <summary>
        /// Creates a spinner builder that can start spinner scopes with a Spectre-style workflow.
        /// </summary>
        /// <returns>A spinner builder.</returns>
        public static SpinnerBuilder Spinner()
        {
            SpinnerBuilder result;
            if (TryDispatch(console => console.Spinner(), out result))
            {
                return result;
            }

            return new SpinnerBuilder();
        }

        /// <summary>
        /// Creates an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <returns>The spinner scope.</returns>
        public static SpinnerScope Spinner(string message, SpinnerOptions options = null)
        {
            SpinnerScope result;
            if (TryDispatch(console => console.Spinner(message, options), out result))
            {
                return result;
            }

            return Consoul.Spinner(message, options);
        }

        /// <summary>
        /// Runs an action inside an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The scoped action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        public static void Spinner(string message, Action<SpinnerScope> action, SpinnerOptions options = null)
        {
            if (TryDispatch(console => console.Spinner(message, action, options)))
            {
                return;
            }

            Consoul.Spinner(message, action, options);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task SpinnerAsync(string message, Func<SpinnerScope, Task> action, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            Task result;
            if (TryDispatch(console => console.SpinnerAsync(message, action, options, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.SpinnerAsync(message, action, options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> SpinnerAsync<T>(string message, Func<SpinnerScope, Task<T>> func, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return SpinnerAsync<T>(message, (spinner, token) => func(spinner), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task SpinnerAsync(string message, Func<SpinnerScope, CancellationToken, Task> action, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            Task result;
            if (TryDispatch(console => console.SpinnerAsync(message, action, options, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.SpinnerAsync(message, action, options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> SpinnerAsync<T>(string message, Func<SpinnerScope, CancellationToken, Task<T>> func, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return AnsiConsoleExtensions.SpinnerAsync<T>(Console, message, func, options, cancellationToken);
        }

        /// <summary>
        /// Creates a multi-task progress context.
        /// </summary>
        /// <param name="options">Optional progress rendering options.</param>
        /// <returns>The progress context.</returns>
        public static ProgressContext Progress(ProgressContextOptions options = null)
        {
            ProgressContext result;
            if (TryDispatch(console => console.Progress(options), out result))
            {
                return result;
            }

            return Consoul.Progress(options);
        }

        /// <summary>
        /// Runs an action inside a multi-task progress context.
        /// </summary>
        /// <param name="action">The scoped progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        public static void Progress(Action<ProgressContext> action, ProgressContextOptions options = null)
        {
            if (TryDispatch(console => console.Progress(action, options)))
            {
                return;
            }

            Consoul.Progress(action, options);
        }

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="action">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task ProgressAsync(Func<ProgressContext, Task> action, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            Task result;
            if (TryDispatch(console => console.ProgressAsync(action, options, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.ProgressAsync(action, options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="func">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> ProgressAsync<T>(Func<ProgressContext, Task<T>> func, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return ProgressAsync<T>((progress, token) => func(progress), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context.
        /// </summary>
        /// <param name="action">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task ProgressAsync(Func<ProgressContext, CancellationToken, Task> action, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            Task result;
            if (TryDispatch(console => console.ProgressAsync(action, options, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.ProgressAsync(action, options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="func">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> ProgressAsync<T>(Func<ProgressContext, CancellationToken, Task<T>> func, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return AnsiConsoleExtensions.ProgressAsync<T>(Console, func, options, cancellationToken);
        }

        /// <summary>
        /// Creates a live display context.
        /// </summary>
        /// <param name="options">Optional live display options.</param>
        /// <returns>The live display context.</returns>
        public static LiveDisplayContext Live(LiveDisplayOptions options = null)
        {
            LiveDisplayContext result;
            if (TryDispatch(console => console.Live(options), out result))
            {
                return result;
            }

            return Consoul.Live(options);
        }

        /// <summary>
        /// Creates a live display context with an initial render target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="options">Optional live display options.</param>
        /// <returns>The live display context.</returns>
        public static LiveDisplayContext Live(IConsoulRenderable target, LiveDisplayOptions options = null)
        {
            LiveDisplayContext result;
            if (TryDispatch(console => console.Live(target, options), out result))
            {
                return result;
            }

            return Consoul.Live(target, options);
        }

        /// <summary>
        /// Runs an action inside a live display context.
        /// </summary>
        /// <param name="action">The scoped live display action.</param>
        /// <param name="options">Optional live display options.</param>
        public static void Live(Action<LiveDisplayContext> action, LiveDisplayOptions options = null)
        {
            if (TryDispatch(console => console.Live(action, options)))
            {
                return;
            }

            Consoul.Live(action, options);
        }

        /// <summary>
        /// Runs an action inside a live display context with an initial render target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The scoped live display action.</param>
        /// <param name="options">Optional live display options.</param>
        public static void Live(IConsoulRenderable target, Action<LiveDisplayContext> action, LiveDisplayOptions options = null)
        {
            if (TryDispatch(console => console.Live(target, action, options)))
            {
                return;
            }

            Consoul.Live(target, action, options);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task LiveAsync(Func<LiveDisplayContext, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            Task result;
            if (TryDispatch(console => console.LiveAsync(action, options, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.LiveAsync(action, options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> LiveAsync<T>(Func<LiveDisplayContext, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return LiveAsync<T>((live, token) => func(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context.
        /// </summary>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task LiveAsync(Func<LiveDisplayContext, CancellationToken, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            Task result;
            if (TryDispatch(console => console.LiveAsync(action, options, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.LiveAsync(action, options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> LiveAsync<T>(Func<LiveDisplayContext, CancellationToken, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return AnsiConsoleExtensions.LiveAsync<T>(Console, func, options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task LiveAsync(IConsoulRenderable target, Func<LiveDisplayContext, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            Task result;
            if (TryDispatch(console => console.LiveAsync(target, action, options, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.LiveAsync(target, action, options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="target">The initial render target.</param>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> LiveAsync<T>(IConsoulRenderable target, Func<LiveDisplayContext, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return LiveAsync<T>(target, (live, token) => func(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target.
        /// </summary>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task LiveAsync(IConsoulRenderable target, Func<LiveDisplayContext, CancellationToken, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            Task result;
            if (TryDispatch(console => console.LiveAsync(target, action, options, cancellationToken), out result))
            {
                return result;
            }

            return Consoul.LiveAsync(target, action, options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="target">The initial render target.</param>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> LiveAsync<T>(IConsoulRenderable target, Func<LiveDisplayContext, CancellationToken, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return AnsiConsoleExtensions.LiveAsync<T>(Console, target, func, options, cancellationToken);
        }

        /// <summary>
        /// Captures Consoul output from a render action into a string.
        /// </summary>
        /// <param name="render">The render action to capture.</param>
        /// <param name="width">The virtual console width.</param>
        /// <param name="height">The virtual console height.</param>
        /// <returns>The captured text.</returns>
        public static string Capture(Action render, int width = 80, int height = 200)
        {
            string result;
            if (TryDispatch(console => console.Capture(render, width, height), out result))
            {
                return result;
            }

            return Consoul.Capture(render, width, height);
        }

        /// <summary>
        /// Captures Consoul output from a renderable component into a string.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="width">The virtual console width.</param>
        /// <param name="height">The virtual console height.</param>
        /// <returns>The captured text.</returns>
        public static string Capture(IConsoulRenderable renderable, int width = 80, int height = 200)
        {
            string result;
            if (TryDispatch(console => console.Capture(renderable, width, height), out result))
            {
                return result;
            }

            return Consoul.Capture(renderable, width, height);
        }

        /// <summary>
        /// Captures Consoul output from a Spectre-style renderable component into a string.
        /// </summary>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="width">The virtual console width.</param>
        /// <param name="height">The virtual console height.</param>
        /// <returns>The captured text.</returns>
        public static string Capture(IRenderable renderable, int width = 80, int height = 200)
        {
            IConsoulRenderable consoulRenderable = Consoul.Renderable(renderable);
            string result;
            if (TryDispatch(console => console.Capture(consoulRenderable, width, height), out result))
            {
                return result;
            }

            return Consoul.Capture(renderable, width, height);
        }
    }
}
