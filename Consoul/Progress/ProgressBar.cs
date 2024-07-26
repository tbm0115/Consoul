using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Maintains a fixed positioned progress bar with an optional message and block representing progress.
    /// </summary>
    public class ProgressBar
    {
        private FixedMessage _message { get; set; }
        private FixedMessage _bar { get; set; }
        private int _total;
        private double _percent = 0.0;

        /// <summary>
        /// Total integer value.
        /// </summary>
        public int Total => _total;

        /// <summary>
        /// The character that is used to render each 'tick' in the progress bar.
        /// </summary>
        public Char BlockCharacter { get; set; }

        /// <summary>
        /// Current percentage of completion.
        /// </summary>
        public double Percent => _percent;

        /// <summary>
        /// Constructs a new progress bar.
        /// </summary>
        /// <param name="total">Total number of 'ticks' expected</param>
        public ProgressBar(int total)
        {
            _total = total;
            BlockCharacter = (char)0x2588;
            Initialize();
        }

        /// <summary>
        /// Initializes the fixed position of the progress bar.
        /// </summary>
        public void Initialize()
        {
            _message = new FixedMessage();
            _message.Update(string.Empty);
            Console.WriteLine();// Create Line Break
            _bar = new FixedMessage();
            _bar.Update(string.Empty);
            Console.WriteLine();// Create Line Break;
        }

        /// <summary>
        /// Resets the 'tick' progress of the rendered bar.
        /// </summary>
        /// <param name="total">Reset the <see cref="Total"/></param>
        /// <param name="reAffix">If <c>true</c>, resets the fixed position</param>
        public void Reset(int total, bool reAffix = false)
        {
            _total = total;
            _percent = 0.0;
            if (reAffix)
                Initialize();
        }

        /// <summary>
        /// Update the 'tick' progress and the message.
        /// </summary>
        /// <param name="index">Update 'tick' progress by the index number of a 'tick' against the <see cref="Total"/> number of 'ticks'.</param>
        /// <param name="message">New text above the progress bar.</param>
        /// <param name="messageColor">Change the text color.</param>
        /// <param name="barColor">Change the current color of the bar.</param>
        public void Update(int index, string message = null, ConsoleColor? messageColor = null, ConsoleColor? barColor = null)
        {
            Update((double)index / (double)_total, message, messageColor, barColor);
        }

        /// <summary>
        /// Update the 'tick' progress and the message.
        /// </summary>
        /// <param name="percent">Update 'tick' progress by a percent of <see cref="Total"/>.</param>
        /// <param name="message">New text above the progress bar.</param>
        /// <param name="messageColor">Change the text color.</param>
        /// <param name="barColor">Change the current color of the bar.</param>
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
