using System;

namespace Consoul.Entry
{
    public class LineEntry : ILineEntry
    {
        public string Message { get; set; }
        public ConsoleColor Color { get; set; } = RenderOptions.DefaultColor;

        public LineEntry(string message, ConsoleColor? color = null)
        {
            Message = message;
            Color = color ?? RenderOptions.DefaultColor;
        }
    }
}
