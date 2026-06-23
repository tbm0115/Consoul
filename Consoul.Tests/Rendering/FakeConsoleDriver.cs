using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ConsoulLibrary;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    internal sealed class FakeConsoleDriver : IConsoleDriver
    {
        private readonly List<StringBuilder> _lines = new List<StringBuilder>();
        private readonly Queue<ConsoleKeyInfo> _keys = new Queue<ConsoleKeyInfo>();
        private int _cursorLeft;
        private int _cursorTop;

        public FakeConsoleDriver(int width, int height = 200)
        {
            BufferWidth = Math.Max(1, width);
            BufferHeight = Math.Max(1, height);
            EnsureLine(0);
        }

        public int BufferWidth { get; private set; }

        public int BufferHeight { get; private set; }

        public int CursorLeft => _cursorLeft;

        public int CursorTop => _cursorTop;

        public bool CursorVisible { get; set; } = true;

        public ConsoleColor ForegroundColor { get; set; } = RenderOptions.DefaultColor;

        public ConsoleColor BackgroundColor { get; set; } = RenderOptions.DefaultScheme.BackgroundColor;

        public Encoding InputEncoding => Encoding.UTF8;

        public int ClearCount { get; private set; }

        public List<string> InvalidCursorOperations { get; } = new List<string>();

        public List<FakeConsoleWrite> Writes { get; } = new List<FakeConsoleWrite>();

        public IReadOnlyList<string> Lines => _lines.Select(line => line.ToString()).ToArray();

        public string Text => string.Join("\n", Lines);

        public void Resize(int width, int height = 200)
        {
            BufferWidth = Math.Max(1, width);
            BufferHeight = Math.Max(1, height);
            _cursorLeft = Math.Max(0, Math.Min(_cursorLeft, BufferWidth - 1));
            _cursorTop = Math.Max(0, Math.Min(_cursorTop, BufferHeight - 1));
        }

        public void EnqueueKey(ConsoleKeyInfo keyInfo) => _keys.Enqueue(keyInfo);

        public void Write(string value)
        {
            value = value ?? string.Empty;
            Writes.Add(new FakeConsoleWrite(value, false, _cursorLeft, _cursorTop, ForegroundColor, BackgroundColor));

            foreach (char ch in value)
            {
                WriteCharacter(ch);
            }
        }

        public void WriteLine(string value)
        {
            value = value ?? string.Empty;
            Writes.Add(new FakeConsoleWrite(value, true, _cursorLeft, _cursorTop, ForegroundColor, BackgroundColor));
            Write(value);
            MoveToNextLine();
        }

        public void Clear()
        {
            ClearCount++;
            _lines.Clear();
            _cursorLeft = 0;
            _cursorTop = 0;
            EnsureLine(0);
        }

        public void SetCursorPosition(int left, int top)
        {
            if (left < 0 || left >= BufferWidth || top < 0 || top >= BufferHeight)
            {
                InvalidCursorOperations.Add($"({left},{top}) outside {BufferWidth}x{BufferHeight}");
            }

            _cursorLeft = Math.Max(0, Math.Min(left, BufferWidth - 1));
            _cursorTop = Math.Max(0, Math.Min(top, BufferHeight - 1));
            EnsureLine(_cursorTop);
        }

        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            if (_keys.Count > 0)
            {
                return _keys.Dequeue();
            }

            return new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false);
        }

        public Stream OpenStandardInput() => new MemoryStream();

        private void WriteCharacter(char ch)
        {
            if (ch == '\r')
            {
                return;
            }

            if (ch == '\n')
            {
                MoveToNextLine();
                return;
            }

            if (ch == '\b')
            {
                _cursorLeft = Math.Max(0, _cursorLeft - 1);
                return;
            }

            if (_cursorLeft >= BufferWidth)
            {
                MoveToNextLine();
            }

            _cursorLeft = Math.Max(0, _cursorLeft);
            EnsureLine(_cursorTop);
            var line = _lines[_cursorTop];
            while (line.Length <= _cursorLeft)
            {
                line.Append(' ');
            }

            line[_cursorLeft] = ch;
            _cursorLeft++;
            if (_cursorLeft >= BufferWidth)
            {
                MoveToNextLine();
            }
        }

        private void MoveToNextLine()
        {
            _cursorLeft = 0;
            _cursorTop++;
            if (_cursorTop >= BufferHeight)
            {
                InvalidCursorOperations.Add($"line {_cursorTop} outside height {BufferHeight}");
                _cursorTop = BufferHeight - 1;
            }

            EnsureLine(_cursorTop);
        }

        private void EnsureLine(int top)
        {
            while (_lines.Count <= top && _lines.Count < BufferHeight)
            {
                _lines.Add(new StringBuilder());
            }
        }
    }

    internal readonly struct FakeConsoleWrite
    {
        public FakeConsoleWrite(string text, bool writeLine, int left, int top, ConsoleColor color, ConsoleColor backgroundColor)
        {
            Text = text;
            WriteLine = writeLine;
            Left = left;
            Top = top;
            Color = color;
            BackgroundColor = backgroundColor;
        }

        public string Text { get; }

        public bool WriteLine { get; }

        public int Left { get; }

        public int Top { get; }

        public ConsoleColor Color { get; }

        public ConsoleColor BackgroundColor { get; }
    }

    internal sealed class ConsoleDriverScope : IDisposable
    {
        private readonly IDisposable _scope;

        public ConsoleDriverScope(int width, int height = 200)
        {
            Driver = new FakeConsoleDriver(width, height);
            _scope = Consoul.UseConsoleDriver(Driver);
        }

        public FakeConsoleDriver Driver { get; }

        public void Dispose() => _scope.Dispose();
    }

    internal static class VisualAssert
    {
        public static void NoOverflow(FakeConsoleDriver driver)
        {
            Assert.Empty(driver.InvalidCursorOperations);
            Assert.All(driver.Lines, line => Assert.True(line.Length <= driver.BufferWidth, $"Line length {line.Length} exceeded width {driver.BufferWidth}: '{line}'"));
        }

        public static void ContainsText(FakeConsoleDriver driver, string expected)
        {
            Assert.Contains(expected, driver.Text);
        }

        public static void WroteText(FakeConsoleDriver driver, string expected)
        {
            Assert.Contains(driver.Writes, write => write.Text.Contains(expected));
        }
    }
}
