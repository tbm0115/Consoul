using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoulLibrary
{
    public class ProgressBar
    {
        private FixedMessage _message { get; set; }
        private FixedMessage _bar { get; set; }
        private int _total;
        private double _percent = 0.0;
        public int Total => _total;
        public Char BlockCharacter { get; set; }
        public double Percent => _percent;

        public ProgressBar(int total)
        {
            _total = total;
            BlockCharacter = (char)0x2588;
            Initialize();
        }

        public void Initialize()
        {
            _message = new FixedMessage();
            _message.Update(string.Empty);
            Console.WriteLine();// Create Line Break
            _bar = new FixedMessage();
            _bar.Update(string.Empty);
            Console.WriteLine();// Create Line Break;
        }

        public void Reset(int total, bool reAffix = false)
        {
            _total = total;
            _percent = 0.0;
            if (reAffix)
                Initialize();
        }

        public void Update(int index, string message = null, ConsoleColor? messageColor = null, ConsoleColor? barColor = null)
        {
            Update((double)index / (double)_total, message, messageColor, barColor);
        }
        public void Update(double percent, string message = null, ConsoleColor? messageColor = null, ConsoleColor? barColor = null)
        {
            _percent = percent;
            if (messageColor == null)
                messageColor = RenderOptions.DefaultColor;
            int width = (int)(_percent * (double)Console.BufferWidth);

            _message.Update(message, messageColor);

            if (barColor == null)
                barColor = RenderOptions.SubnoteColor;
            _bar.Update(new string(BlockCharacter, width), barColor);
        }
    }
}
