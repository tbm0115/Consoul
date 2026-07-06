using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an active alternate-screen terminal scope.
    /// </summary>
    /// <remarks>
    /// When the active console supports ANSI escape sequences, disposing the scope returns the terminal to the main screen buffer.
    /// On non-ANSI outputs, the scope is a no-op wrapper so the caller's render action still runs safely.
    /// </remarks>
    public sealed class AlternateScreenScope : IDisposable
    {
        internal const string EnterSequence = "\u001b[?1049h";
        internal const string ExitSequence = "\u001b[?1049l";

        private readonly IAnsiConsole _console;
        private bool _disposed;

        internal AlternateScreenScope(IAnsiConsole console, bool enabled)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            IsEnabled = enabled;

            if (IsEnabled)
            {
                WriteAnsiSequence(EnterSequence);
                _console.Clear(true);
            }
        }

        /// <summary>
        /// Gets a value indicating whether ANSI alternate-screen sequences were emitted for this scope.
        /// </summary>
        public bool IsEnabled { get; }

        /// <summary>
        /// Leaves the alternate screen when it was enabled.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (IsEnabled)
            {
                WriteAnsiSequence(ExitSequence);
            }
        }

        private void WriteAnsiSequence(string sequence)
        {
            _console.WriteAnsi(() => _console.Write(sequence));
        }
    }
}
