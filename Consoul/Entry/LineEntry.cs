using System;

namespace Consoul.Entry
{
    public class LineEntry : ILineEntry
    {
        public string Message { get; set; }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public LineEntry(string message, ConsoleColor color = ConsoleColor.White)
        {
            Message = message;
            Color = color;
        }
    }
}
