using System;

namespace ConsoulLibrary
{
    internal sealed class ConsoulAnsiConsoleCursor : IAnsiConsoleCursor
    {
        public void Show(bool show)
        {
            Consoul.ConsoleDriver.CursorVisible = show;
        }

        public void SetPosition(int column, int line)
        {
            Consoul.ConsoleDriver.SetCursorPosition(
                Clamp(column, Consoul.ConsoleBufferWidth),
                Clamp(line, Consoul.ConsoleBufferHeight));
        }

        public void Move(CursorDirection direction, int steps)
        {
            int distance = Math.Max(0, steps);
            int column = Consoul.ConsoleDriver.CursorLeft;
            int line = Consoul.ConsoleDriver.CursorTop;

            switch (direction)
            {
                case CursorDirection.Up:
                    line -= distance;
                    break;
                case CursorDirection.Down:
                    line += distance;
                    break;
                case CursorDirection.Left:
                    column -= distance;
                    break;
                case CursorDirection.Right:
                    column += distance;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }

            SetPosition(column, line);
        }

        private static int Clamp(int value, int length)
        {
            int maximum = Math.Max(0, length - 1);
            return Math.Max(0, Math.Min(value, maximum));
        }
    }
}
