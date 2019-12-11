using System;
using System.Linq;

namespace ConsoulLibrary.Entry
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

        public ConsoleColor Color { get; set; } = RenderOptions.DefaultColor;

        public BannerEntry(string message, ConsoleColor? color = null)
        {
            _message = message;
            Width = Math.Max(Console.BufferWidth, _message.Length);
            Color = color ?? RenderOptions.DefaultColor;
        }
    }
}
