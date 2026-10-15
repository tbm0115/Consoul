using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoulLibrary
{
    internal sealed class CaptureConsoleDriver : IConsoleDriver
    {
        private readonly List<StringBuilder> _lines = new List<StringBuilder>();
        private int _cursorLeft;
        private int _cursorTop;

        public CaptureConsoleDriver(int width, int height)
        {
            BufferWidth = Math.Max(1, width);
            BufferHeight = Math.Max(1, height);
            _lines.Add(new StringBuilder());
        }

        public int BufferWidth { get; }

        public int BufferHeight { get; }

        public int CursorLeft => _cursorLeft;

        public int CursorTop => _cursorTop;

        public bool CursorVisible { get; set; } = true;

        public ConsoleColor ForegroundColor { get; set; } = RenderOptions.DefaultColor;

        public ConsoleColor BackgroundColor { get; set; } = RenderOptions.DefaultScheme.BackgroundColor;

        public Encoding InputEncoding => Encoding.UTF8;

        public string Title { get; set; } = string.Empty;

        public bool IsInputRedirected => true;

        public bool IsOutputRedirected => true;

        public bool IsErrorRedirected => true;

        public bool IsVirtual => true;

        public void Write(string value)
        {
            value = value ?? string.Empty;
            foreach (char character in value)
            {
                if (character == '\r')
                {
                    continue;
                }

                if (character == '\n')
                {
                    NewLine();
                    continue;
                }

                EnsureLine(_cursorTop);
                StringBuilder line = _lines[_cursorTop];
                while (line.Length < _cursorLeft)
                {
                    line.Append(' ');
                }

                if (_cursorLeft < line.Length)
                {
                    line[_cursorLeft] = character;
                }
                else
                {
                    line.Append(character);
                }

                _cursorLeft++;
                if (_cursorLeft >= BufferWidth)
                {
                    NewLine();
                }
            }
        }

        public void WriteLine(string value)
        {
            Write(value ?? string.Empty);
            NewLine();
        }

        public void WriteControl(string value)
        {
        }

        public void Clear()
        {
            _lines.Clear();
            _lines.Add(new StringBuilder());
            _cursorLeft = 0;
            _cursorTop = 0;
        }

        public void ClearLine(int top)
        {
            int targetTop = Math.Max(0, Math.Min(top, BufferHeight - 1));
            EnsureLine(targetTop);
            _lines[targetTop].Clear();
            _cursorLeft = 0;
            _cursorTop = targetTop;
        }

        public void SetCursorPosition(int left, int top)
        {
            _cursorLeft = Math.Max(0, Math.Min(left, BufferWidth - 1));
            _cursorTop = Math.Max(0, Math.Min(top, BufferHeight - 1));
            EnsureLine(_cursorTop);
        }

        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            return default;
        }

        public Stream OpenStandardInput()
        {
            return new MemoryStream();
        }

        public string GetText()
        {
            int lastLine = _lines.Count - 1;
            while (lastLine > 0 && _lines[lastLine].Length == 0)
            {
                lastLine--;
            }

            return string.Join(Environment.NewLine, _lines.Take(lastLine + 1).Select(line => line.ToString()));
        }

        private void NewLine()
        {
            _cursorLeft = 0;
            _cursorTop++;
            if (_cursorTop >= BufferHeight)
            {
                _lines.RemoveAt(0);
                _lines.Add(new StringBuilder());
                _cursorTop = BufferHeight - 1;
            }
            else
            {
                EnsureLine(_cursorTop);
            }
        }

        private void EnsureLine(int index)
        {
            while (_lines.Count <= index)
            {
                _lines.Add(new StringBuilder());
            }
        }
    }
}
