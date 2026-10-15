using System;
using System.IO;
using System.Text;

namespace ConsoulLibrary
{
    internal interface IConsoleDriver
    {
        int BufferWidth { get; }

        int BufferHeight { get; }

        int CursorLeft { get; }

        int CursorTop { get; }

        bool CursorVisible { get; set; }

        ConsoleColor ForegroundColor { get; set; }

        ConsoleColor BackgroundColor { get; set; }

        Encoding InputEncoding { get; }

        string Title { get; set; }

        bool IsInputRedirected { get; }

        bool IsOutputRedirected { get; }

        bool IsErrorRedirected { get; }

        bool IsVirtual { get; }

        void Write(string value);

        void WriteLine(string value);

        void WriteControl(string value);

        void Clear();

        void ClearLine(int top);

        void SetCursorPosition(int left, int top);

        // A default ConsoleKeyInfo signals that no further input is available.
        ConsoleKeyInfo ReadKey(bool intercept);

        Stream OpenStandardInput();
    }
}
