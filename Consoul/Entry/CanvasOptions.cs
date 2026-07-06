using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for an ASCII canvas.
    /// </summary>
    public class CanvasOptions
    {
        /// <summary>
        /// Gets or sets the maximum rendered width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the maximum rendered height.
        /// </summary>
        public int? MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pixel canvases are scaled to fit the configured render width.
        /// </summary>
        public bool Scale { get; set; } = true;

        /// <summary>
        /// Gets or sets the character used for empty cells.
        /// </summary>
        public char EmptyCharacter { get; set; } = ' ';

        /// <summary>
        /// Gets or sets the color scheme used for empty cells and uncolored drawing operations.
        /// </summary>
        public ColorScheme? DefaultScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current canvas options.
        /// </summary>
        /// <returns>A new <see cref="CanvasOptions"/> instance with the same values.</returns>
        public CanvasOptions Clone()
        {
            return new CanvasOptions
            {
                MaxWidth = MaxWidth,
                MaxHeight = MaxHeight,
                Scale = Scale,
                EmptyCharacter = EmptyCharacter,
                DefaultScheme = DefaultScheme
            };
        }

        internal ColorScheme ResolveDefaultScheme()
        {
            return DefaultScheme ?? RenderOptions.DefaultScheme;
        }
    }
}
