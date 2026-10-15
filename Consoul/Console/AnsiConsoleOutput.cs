using System;
using System.IO;
using System.Text;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a <see cref="TextWriter"/> output target for created console facades.
    /// </summary>
    public sealed class AnsiConsoleOutput : IAnsiConsoleOutput
    {
        private const int DefaultWidth = 80;
        private const int DefaultHeight = 25;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnsiConsoleOutput"/> class.
        /// </summary>
        /// <param name="writer">The writer used for output.</param>
        /// <param name="width">Optional output width. Defaults to the active terminal width for standard output and 80 for custom writers.</param>
        /// <param name="height">Optional output height. Defaults to the active terminal height for standard output and 25 for custom writers.</param>
        /// <param name="isTerminal">Optional terminal flag. Defaults to standard-output redirection detection for <see cref="Console.Out"/>.</param>
        public AnsiConsoleOutput(TextWriter writer, int? width = null, int? height = null, bool? isTerminal = null)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            IsTerminal = isTerminal ?? ResolveIsTerminal(writer);
            Width = Math.Max(1, width ?? ResolveWidth(writer));
            Height = Math.Max(1, height ?? ResolveHeight(writer));
        }

        /// <summary>
        /// Gets the writer used for output.
        /// </summary>
        public TextWriter Writer { get; }

        /// <summary>
        /// Gets a value indicating whether the writer targets an interactive terminal.
        /// </summary>
        public bool IsTerminal { get; }

        /// <summary>
        /// Gets the output width.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the output height.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Sets the output encoding when supported by the underlying writer.
        /// </summary>
        /// <param name="encoding">The encoding to apply.</param>
        public void SetEncoding(Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            if (ReferenceEquals(Writer, Console.Out) || ReferenceEquals(Writer, Console.Error))
            {
                try
                {
                    Console.OutputEncoding = encoding;
                }
                catch (IOException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (PlatformNotSupportedException)
                {
                }
            }
        }

        private static bool ResolveIsTerminal(TextWriter writer)
        {
            if (ReferenceEquals(writer, Console.Out))
            {
                return !SafeRead(() => Console.IsOutputRedirected, true);
            }

            if (ReferenceEquals(writer, Console.Error))
            {
                return !SafeRead(() => Console.IsErrorRedirected, true);
            }

            return false;
        }

        private static int ResolveWidth(TextWriter writer)
        {
            return ReferenceEquals(writer, Console.Out) || ReferenceEquals(writer, Console.Error)
                ? SafeRead(() => Console.BufferWidth, DefaultWidth)
                : DefaultWidth;
        }

        private static int ResolveHeight(TextWriter writer)
        {
            return ReferenceEquals(writer, Console.Out) || ReferenceEquals(writer, Console.Error)
                ? SafeRead(() => Console.BufferHeight, DefaultHeight)
                : DefaultHeight;
        }

        private static T SafeRead<T>(Func<T> read, T fallback)
        {
            try
            {
                return read();
            }
            catch (IOException)
            {
                return fallback;
            }
            catch (InvalidOperationException)
            {
                return fallback;
            }
            catch (PlatformNotSupportedException)
            {
                return fallback;
            }
        }
    }
}
