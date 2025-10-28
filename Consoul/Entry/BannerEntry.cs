using System;
using System.Linq;
using ConsoulLibrary.Entry;

namespace ConsoulLibrary
{
    /// <summary>
    /// A stylized rendering of text surrounded by repeating characters
    /// </summary>
    public class BannerEntry : ILineEntry
    {
        /// <summary>
        /// Width of the banner (default is the <see cref="Console.BufferWidth"/>)
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The character to be repeated and surrounding the <see cref="Message" />.
        /// </summary>
        public char RepeatCharacter { get; set; } = '*';

        private string _message { get; set; }
        /// <summary>
        /// The message centered in the banner
        /// </summary>
        public string Message
        {
            get
            {
                return $"{String.Join("", Enumerable.Repeat(RepeatCharacter, Width))}\r\n" + 
                    $"{String.Join("", Enumerable.Repeat(" ", (Width - _message.Length) / 2))}" + 
                    $"{_message}" + 
                    $"{String.Join("", Enumerable.Repeat(" ", (Width - _message.Length) / 2))}\r\n" + 
                    $"{String.Join("", Enumerable.Repeat(RepeatCharacter, Width))}";
            }
            set
            {
                _message = value;
            }
        }

        /// <summary>
        /// Color of the 
        /// </summary>
        public ConsoleColor Color { get; set; } = RenderOptions.DefaultColor;

        public BannerEntry(string message, ConsoleColor? color = null)
        {
            _message = message;
            Width = Math.Max(Console.BufferWidth, _message.Length);
            Color = color ?? RenderOptions.DefaultColor;
        }

        /// <summary>
        /// Renders the given message in a default Banner
        /// </summary>
        /// <param name="message">Banner text</param>
        /// <returns>Formatted banner string</returns>
        public static string Render(string message)
            => new BannerEntry(message).Message;
    }
}
