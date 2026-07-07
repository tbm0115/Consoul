using ConsoulLibrary.Color;
using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for responsive callouts.
    /// </summary>
    public class CalloutOptions
    {
        /// <summary>
        /// Gets or sets an explicit title rendered in the callout border.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the requested callout width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the horizontal content padding inside the callout.
        /// </summary>
        public int Padding { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether the callout title should be rendered.
        /// </summary>
        public bool ShowTitle { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the kind marker should be included in the callout title.
        /// </summary>
        public bool ShowMarker { get; set; } = true;

        /// <summary>
        /// Gets or sets the title used for informational callouts when <see cref="Title"/> is not set.
        /// </summary>
        public string InfoTitle { get; set; } = "Info";

        /// <summary>
        /// Gets or sets the title used for success callouts when <see cref="Title"/> is not set.
        /// </summary>
        public string SuccessTitle { get; set; } = "Success";

        /// <summary>
        /// Gets or sets the title used for warning callouts when <see cref="Title"/> is not set.
        /// </summary>
        public string WarningTitle { get; set; } = "Warning";

        /// <summary>
        /// Gets or sets the title used for error callouts when <see cref="Title"/> is not set.
        /// </summary>
        public string ErrorTitle { get; set; } = "Error";

        /// <summary>
        /// Gets or sets the marker used for informational callouts.
        /// </summary>
        public string InfoMarker { get; set; } = "[i]";

        /// <summary>
        /// Gets or sets the marker used for success callouts.
        /// </summary>
        public string SuccessMarker { get; set; } = "[OK]";

        /// <summary>
        /// Gets or sets the marker used for warning callouts.
        /// </summary>
        public string WarningMarker { get; set; } = "[!]";

        /// <summary>
        /// Gets or sets the marker used for error callouts.
        /// </summary>
        public string ErrorMarker { get; set; } = "[X]";

        /// <summary>
        /// Gets or sets the color scheme used for informational callouts.
        /// </summary>
        public ColorScheme? InfoScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for informational callout chrome.
        /// </summary>
        public Decoration InfoDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for informational callout chrome on ANSI-capable terminals.
        /// </summary>
        public RgbColor? InfoForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for informational callout chrome on ANSI-capable terminals.
        /// </summary>
        public RgbColor? InfoBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for success callouts.
        /// </summary>
        public ColorScheme? SuccessScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for success callout chrome.
        /// </summary>
        public Decoration SuccessDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for success callout chrome on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SuccessForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for success callout chrome on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SuccessBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for warning callouts.
        /// </summary>
        public ColorScheme? WarningScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for warning callout chrome.
        /// </summary>
        public Decoration WarningDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for warning callout chrome on ANSI-capable terminals.
        /// </summary>
        public RgbColor? WarningForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for warning callout chrome on ANSI-capable terminals.
        /// </summary>
        public RgbColor? WarningBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for error callouts.
        /// </summary>
        public ColorScheme? ErrorScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for error callout chrome.
        /// </summary>
        public Decoration ErrorDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for error callout chrome on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ErrorForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for error callout chrome on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ErrorBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets an override for the callout border color scheme.
        /// </summary>
        public ColorScheme? BorderScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for the callout border.
        /// </summary>
        public Decoration BorderDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for the callout border on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BorderForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for the callout border on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BorderBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets an override for the callout title color scheme.
        /// </summary>
        public ColorScheme? TitleScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for the callout title.
        /// </summary>
        public Decoration TitleDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for the callout title on ANSI-capable terminals.
        /// </summary>
        public RgbColor? TitleForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for the callout title on ANSI-capable terminals.
        /// </summary>
        public RgbColor? TitleBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets an override for the callout content color scheme.
        /// </summary>
        public ColorScheme? ContentScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for untagged callout content.
        /// </summary>
        public Decoration ContentDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for untagged callout content on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ContentForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for untagged callout content on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ContentBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current callout options.
        /// </summary>
        /// <returns>A new <see cref="CalloutOptions"/> instance with the same values.</returns>
        public CalloutOptions Clone()
        {
            return new CalloutOptions
            {
                Title = Title,
                Width = Width,
                Padding = Padding,
                ShowTitle = ShowTitle,
                ShowMarker = ShowMarker,
                InfoTitle = InfoTitle,
                SuccessTitle = SuccessTitle,
                WarningTitle = WarningTitle,
                ErrorTitle = ErrorTitle,
                InfoMarker = InfoMarker,
                SuccessMarker = SuccessMarker,
                WarningMarker = WarningMarker,
                ErrorMarker = ErrorMarker,
                InfoScheme = InfoScheme,
                InfoDecoration = InfoDecoration,
                InfoForegroundRgb = InfoForegroundRgb,
                InfoBackgroundRgb = InfoBackgroundRgb,
                SuccessScheme = SuccessScheme,
                SuccessDecoration = SuccessDecoration,
                SuccessForegroundRgb = SuccessForegroundRgb,
                SuccessBackgroundRgb = SuccessBackgroundRgb,
                WarningScheme = WarningScheme,
                WarningDecoration = WarningDecoration,
                WarningForegroundRgb = WarningForegroundRgb,
                WarningBackgroundRgb = WarningBackgroundRgb,
                ErrorScheme = ErrorScheme,
                ErrorDecoration = ErrorDecoration,
                ErrorForegroundRgb = ErrorForegroundRgb,
                ErrorBackgroundRgb = ErrorBackgroundRgb,
                BorderScheme = BorderScheme,
                BorderDecoration = BorderDecoration,
                BorderForegroundRgb = BorderForegroundRgb,
                BorderBackgroundRgb = BorderBackgroundRgb,
                TitleScheme = TitleScheme,
                TitleDecoration = TitleDecoration,
                TitleForegroundRgb = TitleForegroundRgb,
                TitleBackgroundRgb = TitleBackgroundRgb,
                ContentScheme = ContentScheme,
                ContentDecoration = ContentDecoration,
                ContentForegroundRgb = ContentForegroundRgb,
                ContentBackgroundRgb = ContentBackgroundRgb
            };
        }

        internal PanelOptions CreatePanelOptions(CalloutKind kind)
        {
            StyledText kindStyle = ResolveKindStyle(kind);
            bool hasBorderOverride = BorderScheme.HasValue;
            bool hasTitleOverride = TitleScheme.HasValue;
            return new PanelOptions
            {
                Title = ResolvePanelTitle(kind),
                Width = Width,
                Padding = Padding,
                BorderScheme = BorderScheme ?? kindStyle.Scheme,
                BorderDecoration = hasBorderOverride ? BorderDecoration : kindStyle.Decoration,
                BorderForegroundRgb = hasBorderOverride ? BorderForegroundRgb : kindStyle.ForegroundRgb,
                BorderBackgroundRgb = hasBorderOverride ? BorderBackgroundRgb : kindStyle.BackgroundRgb,
                TitleScheme = TitleScheme ?? kindStyle.Scheme,
                TitleDecoration = hasTitleOverride ? TitleDecoration : kindStyle.Decoration,
                TitleForegroundRgb = hasTitleOverride ? TitleForegroundRgb : kindStyle.ForegroundRgb,
                TitleBackgroundRgb = hasTitleOverride ? TitleBackgroundRgb : kindStyle.BackgroundRgb,
                ContentScheme = ContentScheme ?? RenderOptions.DefaultScheme,
                ContentDecoration = ContentDecoration,
                ContentForegroundRgb = ContentForegroundRgb,
                ContentBackgroundRgb = ContentBackgroundRgb
            };
        }

        internal ColorScheme ResolveKindScheme(CalloutKind kind)
        {
            switch (kind)
            {
                case CalloutKind.Success:
                    return SuccessScheme ?? Scheme(ConsoleColor.Green);
                case CalloutKind.Warning:
                    return WarningScheme ?? Scheme(ConsoleColor.Yellow);
                case CalloutKind.Error:
                    return ErrorScheme ?? RenderOptions.InvalidScheme;
                default:
                    return InfoScheme ?? RenderOptions.PromptScheme;
            }
        }

        internal StyledText ResolveKindStyle(CalloutKind kind)
        {
            switch (kind)
            {
                case CalloutKind.Success:
                    return new StyledText(SuccessScheme ?? Scheme(ConsoleColor.Green), SuccessDecoration, SuccessForegroundRgb, SuccessBackgroundRgb);
                case CalloutKind.Warning:
                    return new StyledText(WarningScheme ?? Scheme(ConsoleColor.Yellow), WarningDecoration, WarningForegroundRgb, WarningBackgroundRgb);
                case CalloutKind.Error:
                    return new StyledText(ErrorScheme ?? RenderOptions.InvalidScheme, ErrorDecoration, ErrorForegroundRgb, ErrorBackgroundRgb);
                default:
                    return new StyledText(InfoScheme ?? RenderOptions.PromptScheme, InfoDecoration, InfoForegroundRgb, InfoBackgroundRgb);
            }
        }

        internal string ResolvePanelTitle(CalloutKind kind)
        {
            if (!ShowTitle && !ShowMarker)
            {
                return null;
            }

            string marker = ShowMarker ? ResolveMarker(kind) : null;
            string title = ShowTitle ? ResolveTitle(kind) : null;

            if (string.IsNullOrWhiteSpace(marker))
            {
                return string.IsNullOrWhiteSpace(title) ? null : title.Trim();
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return MarkupEntry.EscapeMarkupText(marker.Trim());
            }

            return MarkupEntry.EscapeMarkupText(marker.Trim()) + " " + title.Trim();
        }

        private string ResolveTitle(CalloutKind kind)
        {
            if (!string.IsNullOrWhiteSpace(Title))
            {
                return Title;
            }

            switch (kind)
            {
                case CalloutKind.Success:
                    return SuccessTitle;
                case CalloutKind.Warning:
                    return WarningTitle;
                case CalloutKind.Error:
                    return ErrorTitle;
                default:
                    return InfoTitle;
            }
        }

        private string ResolveMarker(CalloutKind kind)
        {
            switch (kind)
            {
                case CalloutKind.Success:
                    return SuccessMarker;
                case CalloutKind.Warning:
                    return WarningMarker;
                case CalloutKind.Error:
                    return ErrorMarker;
                default:
                    return InfoMarker;
            }
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        internal readonly struct StyledText
        {
            public StyledText(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
            {
                Scheme = scheme;
                Decoration = decoration;
                ForegroundRgb = foregroundRgb;
                BackgroundRgb = backgroundRgb;
            }

            public ColorScheme Scheme { get; }

            public Decoration Decoration { get; }

            public RgbColor? ForegroundRgb { get; }

            public RgbColor? BackgroundRgb { get; }
        }
    }
}
