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

        void Write(string value);

        void WriteLine(string value);

        void Clear();

        void SetCursorPosition(int left, int top);

        ConsoleKeyInfo ReadKey(bool intercept);

        Stream OpenStandardInput();
    }
}
