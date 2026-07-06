using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Describes padding around a renderable component.
    /// </summary>
    public sealed class Padding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Padding"/> class with the same amount on every side.
        /// </summary>
        /// <param name="padding">Padding applied to the left, top, right, and bottom sides.</param>
        public Padding(int padding) : this(padding, padding, padding, padding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Padding"/> class with horizontal and vertical amounts.
        /// </summary>
        /// <param name="horizontal">Padding applied to the left and right sides.</param>
        /// <param name="vertical">Padding applied above and below the content.</param>
        public Padding(int horizontal, int vertical) : this(horizontal, vertical, horizontal, vertical)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Padding"/> class.
        /// </summary>
        /// <param name="left">Padding added to the left side.</param>
        /// <param name="top">Padding added above the content.</param>
        /// <param name="right">Padding added to the right side.</param>
        /// <param name="bottom">Padding added below the content.</param>
        public Padding(int left, int top, int right, int bottom)
        {
            Left = Math.Max(0, left);
            Top = Math.Max(0, top);
            Right = Math.Max(0, right);
            Bottom = Math.Max(0, bottom);
        }

        /// <summary>
        /// Gets or sets the padding added to the left side.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Gets or sets the padding added above the content.
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// Gets or sets the padding added to the right side.
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// Gets or sets the padding added below the content.
        /// </summary>
        public int Bottom { get; set; }

        internal int Horizontal => Math.Max(0, Left) + Math.Max(0, Right);

        internal PadOptions ToPadOptions()
        {
            return new PadOptions
            {
                Left = Math.Max(0, Left),
                Top = Math.Max(0, Top),
                Right = Math.Max(0, Right),
                Bottom = Math.Max(0, Bottom)
            };
        }

        internal Padding Clone()
        {
            return new Padding(Left, Top, Right, Bottom);
        }
    }
}
