using System;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent callout construction over <see cref="CalloutEntry"/>.
    /// </summary>
    public class Callout : CalloutEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Callout"/> class.
        /// </summary>
        /// <param name="message">The callout message to render.</param>
        /// <param name="kind">The visual intent of the callout.</param>
        public Callout(string message, CalloutKind kind = CalloutKind.Info) : base(message, kind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Callout"/> class with explicit options.
        /// </summary>
        /// <param name="message">The callout message to render.</param>
        /// <param name="kind">The visual intent of the callout.</param>
        /// <param name="options">Optional rendering options for the callout.</param>
        public Callout(string message, CalloutKind kind, CalloutOptions options) : base(message, kind, options)
        {
        }

        /// <summary>
        /// Sets the callout message.
        /// </summary>
        /// <param name="message">The callout message.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout Text(string message)
        {
            Message = message ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the callout title.
        /// </summary>
        /// <param name="title">The callout title.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout Title(string title)
        {
            Options.Title = title;
            return this;
        }

        /// <summary>
        /// Sets the callout visual intent.
        /// </summary>
        /// <param name="kind">The callout visual intent.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout KindOf(CalloutKind kind)
        {
            Kind = kind;
            return this;
        }

        /// <summary>
        /// Marks the callout as success.
        /// </summary>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout Success()
        {
            Kind = CalloutKind.Success;
            return this;
        }

        /// <summary>
        /// Marks the callout as warning.
        /// </summary>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout Warning()
        {
            Kind = CalloutKind.Warning;
            return this;
        }

        /// <summary>
        /// Marks the callout as error.
        /// </summary>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout Error()
        {
            Kind = CalloutKind.Error;
            return this;
        }

        /// <summary>
        /// Sets the requested callout width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Sets horizontal content padding inside the callout.
        /// </summary>
        /// <param name="padding">The horizontal padding amount.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout Padding(int padding)
        {
            Options.Padding = padding;
            return this;
        }

        /// <summary>
        /// Shows or hides the callout title.
        /// </summary>
        /// <param name="show">Whether the title should render.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout ShowTitle(bool show = true)
        {
            Options.ShowTitle = show;
            return this;
        }

        /// <summary>
        /// Shows or hides the callout marker.
        /// </summary>
        /// <param name="show">Whether the marker should render.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout ShowMarker(bool show = true)
        {
            Options.ShowMarker = show;
            return this;
        }

        /// <summary>
        /// Sets the callout border foreground color.
        /// </summary>
        /// <param name="color">The border foreground color.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout BorderColor(ConsoleColor color)
        {
            Options.BorderScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the callout border foreground color.
        /// </summary>
        /// <param name="color">The border foreground color.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout BorderColor(SpectreColor color)
        {
            Options.BorderScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the callout border foreground color.
        /// </summary>
        /// <param name="color">The border foreground color.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout BorderColor(RgbColor color)
        {
            Options.BorderScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the callout border color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout BorderColor(Style style)
        {
            Options.BorderScheme = Scheme(style, RenderOptions.DefaultScheme);
            return this;
        }

        /// <summary>
        /// Sets the callout border color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout BorderColor(string style)
        {
            Options.BorderScheme = Style.ParseScheme(style, RenderOptions.DefaultScheme);
            return this;
        }

        /// <summary>
        /// Sets the callout title foreground color.
        /// </summary>
        /// <param name="color">The title foreground color.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout TitleColor(ConsoleColor color)
        {
            Options.TitleScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the callout title foreground color.
        /// </summary>
        /// <param name="color">The title foreground color.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout TitleColor(SpectreColor color)
        {
            Options.TitleScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the callout title foreground color.
        /// </summary>
        /// <param name="color">The title foreground color.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout TitleColor(RgbColor color)
        {
            Options.TitleScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the callout title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout TitleColor(Style style)
        {
            Options.TitleScheme = Scheme(style, RenderOptions.DefaultScheme);
            return this;
        }

        /// <summary>
        /// Sets the callout title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout TitleColor(string style)
        {
            Options.TitleScheme = Style.ParseScheme(style, RenderOptions.DefaultScheme);
            return this;
        }

        /// <summary>
        /// Sets the callout content foreground color.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout ContentColor(ConsoleColor color)
        {
            Options.ContentScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the callout content foreground color.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout ContentColor(SpectreColor color)
        {
            Options.ContentScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the callout content foreground color.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout ContentColor(RgbColor color)
        {
            Options.ContentScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the callout content color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout ContentColor(Style style)
        {
            Options.ContentScheme = Scheme(style, RenderOptions.DefaultScheme);
            return this;
        }

        /// <summary>
        /// Sets the callout content color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Callout"/> instance.</returns>
        public Callout ContentColor(string style)
        {
            Options.ContentScheme = Style.ParseScheme(style, RenderOptions.DefaultScheme);
            return this;
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme Scheme(SpectreColor color)
        {
            return Scheme(color.ToConsoleColor());
        }

        private static ColorScheme Scheme(RgbColor color)
        {
            return Scheme(color.ToConsoleColor());
        }

        private static ColorScheme Scheme(Style style, ColorScheme fallback)
        {
            return (style ?? Style.Plain).ToColorScheme(fallback);
        }
    }
}
