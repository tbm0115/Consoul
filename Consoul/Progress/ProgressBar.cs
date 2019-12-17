using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoulLibrary.Progress
{
    public class ProgressBar
    {
        private FixedMessage.FixedMessage _message { get; set; }
        private FixedMessage.FixedMessage _bar { get; set; }
        private int _total;
        public int Total => _total;
        public Char BlockCharacter { get; set; }

        public ProgressBar(int total)
        {
            _total = total;
            BlockCharacter = (char)0x2588;
            Initialize();
        }

        public void Initialize()
        {
            _message = new FixedMessage.FixedMessage();
            _message.Update(string.Empty);
            Console.WriteLine();// Create Line Break
            _bar = new FixedMessage.FixedMessage();
            _bar.Update(string.Empty);
            Console.WriteLine();// Create Line Break;
        }

        public void Update(int index, string message = null, ConsoleColor? color = null)
        {
            Update((double)index / (double)_total, message, color);
        }
        public void Update(double percent, string message = null, ConsoleColor? color = null)
        {
            if (color == null)
                color = ConsoleColor.Green;
            int width = (int)(percent * (double)Console.BufferWidth);

            _message.Update(message, color);
            _bar.Update(new string(BlockCharacter, width), RenderOptions.SubnoteColor);
        }
    }
}
