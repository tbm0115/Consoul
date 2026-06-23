using System;
using System.IO;
using System.Text;

namespace ConsoulLibrary
{
    internal sealed class SystemConsoleDriver : IConsoleDriver
    {
        private const int DefaultBufferWidth = 80;
        private const int DefaultBufferHeight = 25;

        public int BufferWidth => Math.Max(1, TryRead(() => Console.BufferWidth, DefaultBufferWidth));

        public int BufferHeight => Math.Max(1, TryRead(() => Console.BufferHeight, DefaultBufferHeight));

        public int CursorLeft => Math.Max(0, TryRead(() => Console.CursorLeft, 0));

        public int CursorTop => Math.Max(0, TryRead(() => Console.CursorTop, 0));

        public bool CursorVisible
        {
            get => TryRead(() => Console.CursorVisible, true);
            set => TryWrite(() => Console.CursorVisible = value);
        }

        public ConsoleColor ForegroundColor
        {
            get => TryRead(() => Console.ForegroundColor, RenderOptions.DefaultColor);
            set => TryWrite(() => Console.ForegroundColor = value);
        }

        public ConsoleColor BackgroundColor
        {
            get => TryRead(() => Console.BackgroundColor, RenderOptions.DefaultScheme.BackgroundColor);
            set => TryWrite(() => Console.BackgroundColor = value);
        }

        public Encoding InputEncoding => TryRead(() => Console.InputEncoding, Encoding.UTF8);

        public void Write(string value) => TryWrite(() => Console.Write(value));

        public void WriteLine(string value) => TryWrite(() => Console.WriteLine(value));

        public void Clear() => TryWrite(Console.Clear);

        public void SetCursorPosition(int left, int top)
        {
            int targetLeft = Math.Max(0, Math.Min(left, BufferWidth - 1));
            int targetTop = Math.Max(0, Math.Min(top, BufferHeight - 1));
            TryWrite(() => Console.SetCursorPosition(targetLeft, targetTop));
        }

        public ConsoleKeyInfo ReadKey(bool intercept) => TryRead(() => Console.ReadKey(intercept), new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));

        public Stream OpenStandardInput() => TryRead(() => Console.OpenStandardInput(), Stream.Null);

        private static T TryRead<T>(Func<T> read, T fallback)
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

        private static void TryWrite(Action write)
        {
            try
            {
                write();
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
            catch (ArgumentOutOfRangeException)
            {
            }
        }
    }
}
