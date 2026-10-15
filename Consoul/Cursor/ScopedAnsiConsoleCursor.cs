using System;

namespace ConsoulLibrary
{
    internal sealed class ScopedAnsiConsoleCursor : IAnsiConsoleCursor
    {
        private readonly Func<IConsoleDriver> _driverAccessor;
        private readonly ConsoulAnsiConsoleCursor _inner = new ConsoulAnsiConsoleCursor();

        public ScopedAnsiConsoleCursor(Func<IConsoleDriver> driverAccessor)
        {
            _driverAccessor = driverAccessor ?? throw new ArgumentNullException(nameof(driverAccessor));
        }

        public void Show(bool show)
        {
            using (Consoul.UseConsoleDriver(_driverAccessor()))
            {
                _inner.Show(show);
            }
        }

        public void SetPosition(int column, int line)
        {
            using (Consoul.UseConsoleDriver(_driverAccessor()))
            {
                _inner.SetPosition(column, line);
            }
        }

        public void Move(CursorDirection direction, int steps)
        {
            using (Consoul.UseConsoleDriver(_driverAccessor()))
            {
                _inner.Move(direction, steps);
            }
        }
    }
}
