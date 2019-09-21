using System;
using System.Linq;

namespace Consoul.Entry
{
    public class BannerEntry : ILineEntry
    {
        public int Width { get; set; }
        private string _message { get; set; }
        public string Message
        {
            get
            {
                return $"{String.Join("", Enumerable.Repeat("*", Width))}\r\n" + 
                    $"{String.Join("", Enumerable.Repeat(" ", (Width - _message.Length) / 2))}" + 
                    $"{_message}" + 
                    $"{String.Join("", Enumerable.Repeat(" ", (Width - _message.Length) / 2))}\r\n" + 
                    $"{String.Join("", Enumerable.Repeat("*", Width))}";
            }
            set
            {
                _message = value;
            }
        }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public BannerEntry(string message, ConsoleColor color = ConsoleColor.White)
        {
            _message = message;
            Width = Math.Max(Console.BufferWidth, _message.Length);
            Color = color;
        }
    }
}
