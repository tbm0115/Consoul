using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Maintains a fixed positioned progress bar with an optional message and block representing progress.
    /// </summary>
    public class ProgressBar
    {
        private FixedMessage _messageArea;
        private FixedMessage _barArea;
        private double _progress = 0.0;
        public int BarWidth { get; set; } = Console.BufferWidth - 10; // Default to a width slightly smaller than buffer width to prevent overflow


        /// <summary>
        /// The character that is used to render each 'tick' in the progress bar.
        /// </summary>
        public char BlockCharacter { get; set; } = (char)0x2588;

        /// <summary>
        /// Current progress value between 0.0 and 1.0.
        /// </summary>
        public double Progress => _progress;

        /// <summary>
        /// Constructs a new progress bar with an optional initial message.
        /// </summary>
        /// <param name="initialMessage">Optional initial message to display above the progress bar</param>
        public ProgressBar(string initialMessage = "")
        {
            Initialize(initialMessage);
        }

        /// <summary>
        /// Initializes the fixed position of the progress bar.
        /// </summary>
        private void Initialize(string initialMessage)
        {
            _messageArea = new FixedMessage();
            _messageArea.Render(initialMessage);
            Consoul.LineBreak();
            _barArea = new FixedMessage();
            _barArea.Render(string.Empty);
            Consoul.LineBreak();
        }

        /// <summary>
        /// Resets the progress of the rendered bar.
        /// </summary>
        /// <param name="reAffix">If <c>true</c>, resets the fixed position</param>
        public void Reset(bool reAffix = false)
        {
            _progress = 0.0;
            if (reAffix)
                Initialize(string.Empty);
            else
                Update(0.0, "", ConsoleColor.Gray, ConsoleColor.Gray);
        }

        /// <summary>
        /// Updates the progress bar with a new progress value and optional message.
        /// </summary>
        /// <param name="progress">Progress value as a value between 0.0 and 1.0</param>
        /// <param name="message">Optional message to display above the progress bar</param>
        /// <param name="messageColor">Optional color for the message text</param>
        /// <param name="barColor">Optional color for the progress bar</param>
        public void Update(double progress, string message = null, ConsoleColor? messageColor = null, ConsoleColor? barColor = null)
        {
            // Clamp progress value between 0.0 and 1.0
            _progress = (progress < 0.0) ? 0.0 : (progress > 1.0) ? 1.0 : progress;

            // Set default colors if not provided
            messageColor = messageColor ?? RenderOptions.DefaultColor;
            barColor = barColor ?? RenderOptions.SubnoteColor;

            int width = (int)(_progress * BarWidth);

            // Render message and bar
            _messageArea.Render(message ?? string.Empty, messageColor);
            _barArea.Render(new string(BlockCharacter, width), barColor);
        }
    }
}
