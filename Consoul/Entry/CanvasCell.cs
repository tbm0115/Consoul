using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one drawn cell in an ASCII canvas.
    /// </summary>
    public readonly struct CanvasCell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasCell"/> struct.
        /// </summary>
        /// <param name="character">The character rendered for the cell.</param>
        /// <param name="scheme">The color scheme rendered for the cell.</param>
        public CanvasCell(char character, ColorScheme scheme)
        {
            Character = character;
            Scheme = scheme;
        }

        /// <summary>
        /// Gets the character rendered for the cell.
        /// </summary>
        public char Character { get; }

        /// <summary>
        /// Gets the color scheme rendered for the cell.
        /// </summary>
        public ColorScheme Scheme { get; }
    }
}
