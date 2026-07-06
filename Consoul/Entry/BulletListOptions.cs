using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for responsive bullet lists.
    /// </summary>
    public class BulletListOptions
    {
        /// <summary>
        /// Gets or sets the requested render width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the marker style used for list items.
        /// </summary>
        public BulletListStyle Style { get; set; } = BulletListStyle.Bullet;

        /// <summary>
        /// Gets or sets the number of spaces used for each nested level.
        /// </summary>
        public int IndentSize { get; set; } = 2;

        /// <summary>
        /// Gets or sets the number of spaces written between the marker and item text.
        /// </summary>
        public int MarkerGap { get; set; } = 1;

        /// <summary>
        /// Gets or sets reusable bullet markers by nesting level.
        /// </summary>
        public string[] BulletMarkers { get; set; } = new[] { "-", "*", "+" };

        /// <summary>
        /// Gets or sets the numeric marker format. The item number is passed as the first format argument.
        /// </summary>
        public string NumberFormat { get; set; } = "{0}.";

        /// <summary>
        /// Gets or sets the color scheme used for item markers.
        /// </summary>
        public ColorScheme? MarkerScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for item text.
        /// </summary>
        public ColorScheme? TextScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current bullet list options.
        /// </summary>
        /// <returns>A new <see cref="BulletListOptions"/> instance with the same values.</returns>
        public BulletListOptions Clone()
        {
            return new BulletListOptions
            {
                Width = Width,
                Style = Style,
                IndentSize = IndentSize,
                MarkerGap = MarkerGap,
                BulletMarkers = BulletMarkers != null ? (string[])BulletMarkers.Clone() : null,
                NumberFormat = NumberFormat,
                MarkerScheme = MarkerScheme,
                TextScheme = TextScheme
            };
        }

        internal string ResolveMarker(int level, int index)
        {
            if (Style == BulletListStyle.Numbered)
            {
                string format = string.IsNullOrWhiteSpace(NumberFormat) ? "{0}." : NumberFormat;
                return string.Format(format, index + 1);
            }

            string[] markers = BulletMarkers == null || BulletMarkers.Length == 0
                ? new[] { "-" }
                : BulletMarkers;
            return markers[Math.Abs(level) % markers.Length] ?? string.Empty;
        }

        internal ColorScheme ResolveMarkerScheme()
        {
            return MarkerScheme ?? RenderOptions.PromptScheme;
        }

        internal ColorScheme ResolveTextScheme()
        {
            return TextScheme ?? RenderOptions.DefaultScheme;
        }
    }
}
