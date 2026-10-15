using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents Spectre-style cursor operations over the active Consoul console driver.
    /// </summary>
    public interface IAnsiConsoleCursor
    {
        /// <summary>
        /// Shows or hides the cursor.
        /// </summary>
        /// <param name="show">True to show the cursor; false to hide it.</param>
        void Show(bool show);

        /// <summary>
        /// Sets the cursor position.
        /// </summary>
        /// <param name="column">The zero-based column.</param>
        /// <param name="line">The zero-based line.</param>
        void SetPosition(int column, int line);

        /// <summary>
        /// Moves the cursor relative to its current position.
        /// </summary>
        /// <param name="direction">The movement direction.</param>
        /// <param name="steps">The number of cells to move.</param>
        void Move(CursorDirection direction, int steps);
    }

    /// <summary>
    /// Provides convenience methods for <see cref="IAnsiConsoleCursor"/>.
    /// </summary>
    public static class CursorExtensions
    {
        /// <summary>
        /// Shows the cursor.
        /// </summary>
        /// <param name="cursor">The cursor facade.</param>
        public static void Show(this IAnsiConsoleCursor cursor)
        {
            if (cursor == null)
            {
                throw new ArgumentNullException(nameof(cursor));
            }

            cursor.Show(true);
        }

        /// <summary>
        /// Hides the cursor.
        /// </summary>
        /// <param name="cursor">The cursor facade.</param>
        public static void Hide(this IAnsiConsoleCursor cursor)
        {
            if (cursor == null)
            {
                throw new ArgumentNullException(nameof(cursor));
            }

            cursor.Show(false);
        }

        /// <summary>
        /// Moves the cursor up one line.
        /// </summary>
        /// <param name="cursor">The cursor facade.</param>
        public static void MoveUp(this IAnsiConsoleCursor cursor)
        {
            MoveUp(cursor, 1);
        }

        /// <summary>
        /// Moves the cursor up.
        /// </summary>
        /// <param name="cursor">The cursor facade.</param>
        /// <param name="steps">The number of lines to move.</param>
        public static void MoveUp(this IAnsiConsoleCursor cursor, int steps)
        {
            Move(cursor, CursorDirection.Up, steps);
        }

        /// <summary>
        /// Moves the cursor down one line.
        /// </summary>
        /// <param name="cursor">The cursor facade.</param>
        public static void MoveDown(this IAnsiConsoleCursor cursor)
        {
            MoveDown(cursor, 1);
        }

        /// <summary>
        /// Moves the cursor down.
        /// </summary>
        /// <param name="cursor">The cursor facade.</param>
        /// <param name="steps">The number of lines to move.</param>
        public static void MoveDown(this IAnsiConsoleCursor cursor, int steps)
        {
            Move(cursor, CursorDirection.Down, steps);
        }

        /// <summary>
        /// Moves the cursor left one column.
        /// </summary>
        /// <param name="cursor">The cursor facade.</param>
        public static void MoveLeft(this IAnsiConsoleCursor cursor)
        {
            MoveLeft(cursor, 1);
        }

        /// <summary>
        /// Moves the cursor left.
        /// </summary>
        /// <param name="cursor">The cursor facade.</param>
        /// <param name="steps">The number of columns to move.</param>
        public static void MoveLeft(this IAnsiConsoleCursor cursor, int steps)
        {
            Move(cursor, CursorDirection.Left, steps);
        }

        /// <summary>
        /// Moves the cursor right one column.
        /// </summary>
        /// <param name="cursor">The cursor facade.</param>
        public static void MoveRight(this IAnsiConsoleCursor cursor)
        {
            MoveRight(cursor, 1);
        }

        /// <summary>
        /// Moves the cursor right.
        /// </summary>
        /// <param name="cursor">The cursor facade.</param>
        /// <param name="steps">The number of columns to move.</param>
        public static void MoveRight(this IAnsiConsoleCursor cursor, int steps)
        {
            Move(cursor, CursorDirection.Right, steps);
        }

        private static void Move(IAnsiConsoleCursor cursor, CursorDirection direction, int steps)
        {
            if (cursor == null)
            {
                throw new ArgumentNullException(nameof(cursor));
            }

            cursor.Move(direction, steps);
        }
    }
}
