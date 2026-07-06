using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides additive Spectre-style helpers for <see cref="IAnsiConsole"/> without expanding the interface contract.
    /// </summary>
    public static class AnsiConsoleExtensions
    {
        /// <summary>
        /// Clears the console and accepts Spectre-style cursor-home intent.
        /// </summary>
        /// <param name="console">The console facade to clear.</param>
        /// <param name="home">Whether the cursor should be moved to the home position after clearing.</param>
        public static void Clear(this IAnsiConsole console, bool home)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (console is ConsoulAnsiConsole consoulConsole)
            {
                consoulConsole.Clear(home);
                return;
            }

            console.Clear();
        }

        /// <summary>
        /// Enters the terminal alternate screen when the active console supports ANSI sequences.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <returns>A disposable scope that restores the main screen on disposal.</returns>
        public static AlternateScreenScope AlternateScreen(this IAnsiConsole console)
        {
            IAnsiConsole target = EnsureConsole(console);
            return new AlternateScreenScope(target, target.Profile.Capabilities.Ansi);
        }

        /// <summary>
        /// Runs an action inside the terminal alternate screen when ANSI sequences are supported.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="action">The action to run inside the alternate screen.</param>
        public static void AlternateScreen(this IAnsiConsole console, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (AlternateScreen(console))
            {
                action();
            }
        }

        /// <summary>
        /// Runs a function inside the terminal alternate screen and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The function to run inside the alternate screen.</param>
        /// <returns>The function result.</returns>
        public static T AlternateScreen<T>(this IAnsiConsole console, Func<T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (AlternateScreen(console))
            {
                return func();
            }
        }

        /// <summary>
        /// Runs an asynchronous action inside the terminal alternate screen without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="action">The asynchronous action to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the action completes.</returns>
        public static Task AlternateScreenAsync(this IAnsiConsole console, Func<Task> action, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AlternateScreenAsync(console, token => action(), cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside the terminal alternate screen.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="action">The asynchronous action to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the action completes.</returns>
        public static async Task AlternateScreenAsync(this IAnsiConsole console, Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (AlternateScreen(console))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await action(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Runs an asynchronous function inside the terminal alternate screen and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The asynchronous function to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the function result.</returns>
        public static Task<T> AlternateScreenAsync<T>(this IAnsiConsole console, Func<Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return AlternateScreenAsync<T>(console, token => func(), cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous function inside the terminal alternate screen and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The asynchronous function to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the function result.</returns>
        public static async Task<T> AlternateScreenAsync<T>(this IAnsiConsole console, Func<CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (AlternateScreen(console))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await func(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, int value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, int value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, uint value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, uint value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, long value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, long value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, ulong value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, ulong value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, float value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, float value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, double value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, double value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a decimal value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, decimal value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a decimal value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, decimal value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a Boolean value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, bool value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a Boolean value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, bool value)
        {
            EnsureConsole(console).Write(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes a Unicode character without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, char value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes a Unicode character without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, char value)
        {
            EnsureConsole(console).Write(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes an array of Unicode characters without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The characters to write.</param>
        public static void Write(this IAnsiConsole console, char[] value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes an array of Unicode characters without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The characters to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, char[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            EnsureConsole(console).Write(new string(value));
        }

        /// <summary>
        /// Writes the text representation of an object without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The object to write.</param>
        public static void Write(this IAnsiConsole console, object value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of an object without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The object to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, object value)
        {
            IAnsiConsole target = EnsureConsole(console);
            if (value is IConsoulRenderable renderable)
            {
                target.Write(renderable);
                return;
            }

            if (value is IRenderable spectreRenderable)
            {
                target.Write(Consoul.Renderable(spectreRenderable));
                return;
            }

            target.Write(Convert.ToString(value, provider) ?? string.Empty);
        }

        /// <summary>
        /// Renders a Spectre-style renderable component without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Write(this IAnsiConsole console, IRenderable renderable)
        {
            EnsureConsole(console).Write(Consoul.Renderable(renderable));
        }

        /// <summary>
        /// Writes a formatted string without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void Write(this IAnsiConsole console, string format, params object[] args)
        {
            console.Write(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes a formatted string without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, string format, params object[] args)
        {
            EnsureConsole(console).Write(string.Format(provider, format ?? string.Empty, args ?? Array.Empty<object>()));
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, int value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, int value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, uint value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, uint value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, long value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, long value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, ulong value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, ulong value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, float value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, float value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, double value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, double value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a decimal value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, decimal value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a decimal value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, decimal value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a Boolean value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, bool value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a Boolean value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, bool value)
        {
            EnsureConsole(console).WriteLine(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes a Unicode character followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, char value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes a Unicode character followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, char value)
        {
            EnsureConsole(console).WriteLine(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes an array of Unicode characters followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The characters to write.</param>
        public static void WriteLine(this IAnsiConsole console, char[] value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes an array of Unicode characters followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The characters to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, char[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            EnsureConsole(console).WriteLine(new string(value));
        }

        /// <summary>
        /// Writes the text representation of an object followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The object to write.</param>
        public static void WriteLine(this IAnsiConsole console, object value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of an object followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The object to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, object value)
        {
            IAnsiConsole target = EnsureConsole(console);
            if (value is IConsoulRenderable renderable)
            {
                target.Write(renderable);
                target.WriteLine();
                return;
            }

            if (value is IRenderable spectreRenderable)
            {
                target.Write(Consoul.Renderable(spectreRenderable));
                target.WriteLine();
                return;
            }

            target.WriteLine(Convert.ToString(value, provider) ?? string.Empty);
        }

        /// <summary>
        /// Renders a Consoul renderable component followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="renderable">The renderable component to render.</param>
        public static void WriteLine(this IAnsiConsole console, IConsoulRenderable renderable)
        {
            IAnsiConsole target = EnsureConsole(console);
            target.Write(renderable);
            target.WriteLine();
        }

        /// <summary>
        /// Renders a Spectre-style renderable component followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="renderable">The renderable component to render.</param>
        public static void WriteLine(this IAnsiConsole console, IRenderable renderable)
        {
            IAnsiConsole target = EnsureConsole(console);
            target.Write(Consoul.Renderable(renderable));
            target.WriteLine();
        }

        /// <summary>
        /// Writes a formatted string followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void WriteLine(this IAnsiConsole console, string format, params object[] args)
        {
            console.WriteLine(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes a formatted string followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, string format, params object[] args)
        {
            EnsureConsole(console).WriteLine(string.Format(provider, format ?? string.Empty, args ?? Array.Empty<object>()));
        }

        /// <summary>
        /// Writes formatted lightweight markup without appending a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void Markup(this IAnsiConsole console, string format, params object[] args)
        {
            console.Markup(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes formatted lightweight markup without appending a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void Markup(this IAnsiConsole console, IFormatProvider provider, string format, params object[] args)
        {
            EnsureConsole(console).Markup(MarkupEntry.FormatMarkup(provider, format, args));
        }

        /// <summary>
        /// Writes formatted lightweight markup followed by a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void MarkupLine(this IAnsiConsole console, string format, params object[] args)
        {
            console.MarkupLine(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes formatted lightweight markup followed by a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void MarkupLine(this IAnsiConsole console, IFormatProvider provider, string format, params object[] args)
        {
            EnsureConsole(console).MarkupLine(MarkupEntry.FormatMarkup(provider, format, args));
        }

        /// <summary>
        /// Renders a Consoul renderable component using the legacy Spectre-style method name.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Render(this IAnsiConsole console, IConsoulRenderable renderable)
        {
            EnsureConsole(console).Write(renderable);
        }

        /// <summary>
        /// Renders a Spectre-style renderable component using the legacy Spectre-style method name.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Render(this IAnsiConsole console, IRenderable renderable)
        {
            EnsureConsole(console).Write(Consoul.Renderable(renderable));
        }

        /// <summary>
        /// Captures a Spectre-style renderable component through an injectable console facade.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="width">The virtual capture width.</param>
        /// <param name="height">The virtual capture height.</param>
        /// <returns>The captured plain text output.</returns>
        public static string Capture(this IAnsiConsole console, IRenderable renderable, int width = 80, int height = 200)
        {
            return EnsureConsole(console).Capture(Consoul.Renderable(renderable), width, height);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task StatusAsync(this IAnsiConsole console, string message, Func<StatusScope, Task> action, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return EnsureConsole(console).StatusAsync(message, (status, token) => action(status), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope and returns its result without requiring a cancellation-token parameter.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> StatusAsync<T>(this IAnsiConsole console, string message, Func<StatusScope, Task<T>> func, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return StatusAsync<T>(console, message, (status, token) => func(status), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static async Task<T> StatusAsync<T>(this IAnsiConsole console, string message, Func<StatusScope, CancellationToken, Task<T>> func, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            await EnsureConsole(console).StatusAsync(message, async (status, token) =>
            {
                result = await func(status, token).ConfigureAwait(false);
            }, options, cancellationToken).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task SpinnerAsync(this IAnsiConsole console, string message, Func<SpinnerScope, Task> action, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return EnsureConsole(console).SpinnerAsync(message, (spinner, token) => action(spinner), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope and returns its result without requiring a cancellation-token parameter.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> SpinnerAsync<T>(this IAnsiConsole console, string message, Func<SpinnerScope, Task<T>> func, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return SpinnerAsync<T>(console, message, (spinner, token) => func(spinner), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static async Task<T> SpinnerAsync<T>(this IAnsiConsole console, string message, Func<SpinnerScope, CancellationToken, Task<T>> func, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            await EnsureConsole(console).SpinnerAsync(message, async (spinner, token) =>
            {
                result = await func(spinner, token).ConfigureAwait(false);
            }, options, cancellationToken).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="action">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task ProgressAsync(this IAnsiConsole console, Func<ProgressContext, Task> action, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return EnsureConsole(console).ProgressAsync((progress, token) => action(progress), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context and returns its result without requiring a cancellation-token parameter.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> ProgressAsync<T>(this IAnsiConsole console, Func<ProgressContext, Task<T>> func, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return ProgressAsync<T>(console, (progress, token) => func(progress), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static async Task<T> ProgressAsync<T>(this IAnsiConsole console, Func<ProgressContext, CancellationToken, Task<T>> func, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            await EnsureConsole(console).ProgressAsync(async (progress, token) =>
            {
                result = await func(progress, token).ConfigureAwait(false);
            }, options, cancellationToken).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task LiveAsync(this IAnsiConsole console, Func<LiveDisplayContext, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return EnsureConsole(console).LiveAsync((live, token) => action(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context and returns its result without requiring a cancellation-token parameter.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> LiveAsync<T>(this IAnsiConsole console, Func<LiveDisplayContext, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return LiveAsync<T>(console, (live, token) => func(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static async Task<T> LiveAsync<T>(this IAnsiConsole console, Func<LiveDisplayContext, CancellationToken, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            await EnsureConsole(console).LiveAsync(async (live, token) =>
            {
                result = await func(live, token).ConfigureAwait(false);
            }, options, cancellationToken).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task LiveAsync(this IAnsiConsole console, IConsoulRenderable target, Func<LiveDisplayContext, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return EnsureConsole(console).LiveAsync(target, (live, token) => action(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target and returns its result without requiring a cancellation-token parameter.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="target">The initial render target.</param>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> LiveAsync<T>(this IAnsiConsole console, IConsoulRenderable target, Func<LiveDisplayContext, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return LiveAsync<T>(console, target, (live, token) => func(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="target">The initial render target.</param>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static async Task<T> LiveAsync<T>(this IAnsiConsole console, IConsoulRenderable target, Func<LiveDisplayContext, CancellationToken, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            await EnsureConsole(console).LiveAsync(target, async (live, token) =>
            {
                result = await func(live, token).ConfigureAwait(false);
            }, options, cancellationToken).ConfigureAwait(false);

            return result;
        }

        private static IAnsiConsole EnsureConsole(IAnsiConsole console)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            return console;
        }
    }
}
