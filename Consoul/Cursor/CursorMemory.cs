using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Disposable container for the current position of the cursor. Repositions the cursor to original position when disposed.
    /// </summary>
    public class CursorMemory : IDisposable
    {
        /// <summary>
        /// Reference to the original cursor position at the time of construction.
        /// </summary>
        public CursorPosition OriginalPosition { get; private set; }

        /// <summary>
        /// Reference to the current cursor position.
        /// </summary>
        public CursorPosition CurrentPosition => new CursorPosition()
        {
            Left = Console.CursorLeft,
            Top = Console.CursorTop,
        };

        /// <summary>
        /// Constructs a new cursor position memory.
        /// </summary>
        public CursorMemory()
        {
            OriginalPosition = new CursorPosition()
            {
                Left = Console.CursorLeft,
                Top = Console.CursorTop
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (CurrentPosition.Left >= OriginalPosition.Left && CurrentPosition.Top >= OriginalPosition.Top)
                return;
            Console.SetCursorPosition(OriginalPosition.Left, OriginalPosition.Top);
        }
    }
}
