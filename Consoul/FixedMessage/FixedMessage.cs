using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoulLibrary.FixedMessage
{
    public class FixedMessage
    {
        protected int _x, _y, _fw;
        public int? MaxWidth { get; set; }

        public FixedMessage()
        {
            _x = Console.CursorLeft;
            _y = Console.CursorTop;
            _fw = Console.BufferWidth;
        }
        public FixedMessage(int maxWidth) : base()
        {
            MaxWidth = maxWidth;
        }

        public void Update(string message, ConsoleColor? color = null)
        {
            int prevX, prevY;
            prevX = Console.CursorLeft;
            prevY = Console.CursorTop;
            Console.SetCursorPosition(_x, _y);
            if (MaxWidth != null && message.Length > MaxWidth)
                message = message.Substring(0, (int)MaxWidth - 3) + "...";
            Consoul.Write(message, color, false);
            Console.SetCursorPosition(prevX, prevY);
        }
    }
}
