using System.IO;
using System.Text;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an output target used by a created <see cref="IAnsiConsole"/>.
    /// </summary>
    public interface IAnsiConsoleOutput
    {
        /// <summary>
        /// Gets the writer used for output.
        /// </summary>
        TextWriter Writer { get; }

        /// <summary>
        /// Gets a value indicating whether the writer targets an interactive terminal.
        /// </summary>
        bool IsTerminal { get; }

        /// <summary>
        /// Gets the output width.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the output height.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Sets the output encoding when supported by the underlying writer.
        /// </summary>
        /// <param name="encoding">The encoding to apply.</param>
        void SetEncoding(Encoding encoding);
    }
}
