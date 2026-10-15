using System;
using System.IO;
using System.Text;

namespace ConsoulLibrary
{
    internal sealed class RecordingConsoleDriver : IConsoleDriver, IConsoleDriverProfileProvider
    {
        private readonly IConsoleDriver _inner;
        private readonly StringBuilder _text = new StringBuilder();

        public RecordingConsoleDriver(IConsoleDriver inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public int BufferWidth => _inner.BufferWidth;

        public int BufferHeight => _inner.BufferHeight;

        public int CursorLeft => _inner.CursorLeft;

        public int CursorTop => _inner.CursorTop;

        public bool CursorVisible
        {
            get => _inner.CursorVisible;
            set => _inner.CursorVisible = value;
        }

        public ConsoleColor ForegroundColor
        {
            get => _inner.ForegroundColor;
            set => _inner.ForegroundColor = value;
        }

        public ConsoleColor BackgroundColor
        {
            get => _inner.BackgroundColor;
            set => _inner.BackgroundColor = value;
        }

        public Encoding InputEncoding => _inner.InputEncoding;

        public string Title
        {
            get => _inner.Title;
            set => _inner.Title = value;
        }

        public bool IsInputRedirected => _inner.IsInputRedirected;

        public bool IsOutputRedirected => _inner.IsOutputRedirected;

        public bool IsErrorRedirected => _inner.IsErrorRedirected;

        public bool IsVirtual => _inner.IsVirtual;

        public TerminalProfile Profile => TerminalProfile.FromDriver(_inner);

        public void Write(string value)
        {
            _text.Append(NormalizeNewLines(value));
            _inner.Write(value);
        }

        public void WriteLine(string value)
        {
            _text.Append(NormalizeNewLines(value));
            _text.Append(Environment.NewLine);
            _inner.WriteLine(value);
        }

        public void WriteControl(string value)
        {
            _inner.WriteControl(value);
        }

        public void Clear()
        {
            _inner.Clear();
        }

        public void ClearLine(int top)
        {
            _inner.ClearLine(top);
        }

        public void SetCursorPosition(int left, int top)
        {
            _inner.SetCursorPosition(left, top);
        }

        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            return _inner.ReadKey(intercept);
        }

        public Stream OpenStandardInput()
        {
            return _inner.OpenStandardInput();
        }

        public string GetText()
        {
            return _text.ToString();
        }

        public string GetHtml()
        {
            return "<pre class=\"consoul-recording\">" + EncodeHtml(GetText()) + "</pre>";
        }

        private static string NormalizeNewLines(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string normalized = value.Replace("\r\n", "\n").Replace('\r', '\n');
            return normalized.Replace("\n", Environment.NewLine);
        }

        private static string EncodeHtml(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }
    }
}
