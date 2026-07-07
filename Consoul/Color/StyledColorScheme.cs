namespace ConsoulLibrary.Color
{
    internal readonly struct StyledColorScheme
    {
        public StyledColorScheme(ColorScheme scheme, RgbColor? foregroundRgb = null, RgbColor? backgroundRgb = null)
        {
            Scheme = scheme;
            ForegroundRgb = foregroundRgb;
            BackgroundRgb = backgroundRgb;
        }

        public ColorScheme Scheme { get; }

        public RgbColor? ForegroundRgb { get; }

        public RgbColor? BackgroundRgb { get; }

        public static StyledColorScheme FromScheme(ColorScheme scheme)
        {
            return new StyledColorScheme(scheme);
        }

        public static StyledColorScheme FromStyle(Style style, ColorScheme fallback)
        {
            Style resolvedStyle = style ?? Style.Plain;
            return new StyledColorScheme(
                resolvedStyle.ToColorScheme(fallback),
                resolvedStyle.ForegroundRgb,
                resolvedStyle.BackgroundRgb);
        }
    }
}
