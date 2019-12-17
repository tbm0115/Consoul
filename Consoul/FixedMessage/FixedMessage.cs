using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoulLibrary
{
    public class FixedMessage
    {
        protected int _x, _y, _fw;
        public int? MaxWidth { get; set; }

        public FixedMessage()
        {
            Initialize();
        }
        public FixedMessage(int maxWidth) : this()
        {
            MaxWidth = maxWidth;
        }

        public void Initialize()
        {
            _x = Console.CursorLeft;
            _y = Console.CursorTop;
            _fw = Console.BufferWidth;
        }

        public void Update(string message, ConsoleColor? color = null)
        {
            int prevX, prevY;
            prevX = Console.CursorLeft;
            prevY = Console.CursorTop;
            if (message?.Length > (MaxWidth ?? _fw))
                message = message.Substring(0, (MaxWidth ?? _fw) - 3) + "...";
            // Clear Message Space
            Console.SetCursorPosition(_x, _y);
            Consoul.Write(new string(' ', MaxWidth ?? _fw), writeLine: false);
            // Write Message
            Console.SetCursorPosition(_x, _y);
            Consoul.Write(message, color, false);
            Console.SetCursorPosition(prevX, prevY);
        }
    }
}
