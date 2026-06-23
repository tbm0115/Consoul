using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Disposable container for the current position of the cursor. Repositions the cursor to original position when disposed.
    /// </summary>
    public class CursorScope : IDisposable
    {
        private bool _disposed;
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
        public CursorScope()
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
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            int bufferWidth = Console.BufferWidth;
            int bufferHeight = Console.BufferHeight;

            int targetLeft = OriginalPosition.Left;
            int targetTop = OriginalPosition.Top;

            if (bufferWidth > 0)
            {
                targetLeft = Math.Max(0, Math.Min(targetLeft, bufferWidth - 1));
            }
            else
            {
                targetLeft = 0;
            }

            if (bufferHeight > 0)
            {
                targetTop = Math.Max(0, Math.Min(targetTop, bufferHeight - 1));
            }
            else
            {
                targetTop = 0;
            }

            Console.SetCursorPosition(targetLeft, targetTop);
        }
    }
}
