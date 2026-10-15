namespace ConsoulLibrary
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Provides Spectre-style reusable markup construction over <see cref="MarkupEntry"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="MarkupEntry"/>, this compatibility alias defaults to rendering without an appended line
    /// break so it behaves naturally with <see cref="AnsiConsole.Write(IConsoulRenderable)"/>.
    /// </remarks>
    public class Markup : MarkupEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Markup"/> class.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        public Markup(string markup) : this(markup, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Markup"/> class with a default style for untagged text.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        /// <param name="style">Optional default style applied to untagged text.</param>
        public Markup(string markup, Style style) : base(markup, false)
        {
            DefaultStyle = style;
        }

        /// <summary>
        /// Returns a new markup widget from an interpolated string, escaping interpolated values.
        /// </summary>
        /// <param name="value">The interpolated markup value.</param>
        /// <param name="style">Optional default style applied to untagged text.</param>
        /// <returns>A markup widget with escaped interpolated values.</returns>
        public static Markup FromInterpolated(FormattableString value, Style style = null)
        {
            return FromInterpolated(CultureInfo.CurrentCulture, value, style);
        }

        /// <summary>
        /// Returns a new markup widget from an interpolated string, escaping interpolated values.
        /// </summary>
        /// <param name="provider">The format provider used for interpolated values.</param>
        /// <param name="value">The interpolated markup value.</param>
        /// <param name="style">Optional default style applied to untagged text.</param>
        /// <returns>A markup widget with escaped interpolated values.</returns>
        public static Markup FromInterpolated(IFormatProvider provider, FormattableString value, Style style = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new Markup(MarkupEntry.FormatInterpolated(provider, value), style);
        }

        /// <summary>
        /// Returns a new markup widget from a composite format string, escaping formatted values.
        /// </summary>
        /// <param name="format">A composite markup format string containing color tags such as <c>[green]{0}[/]</c>.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        /// <returns>A markup widget with escaped formatted values.</returns>
        public static Markup FromFormat(string format, params object[] args)
        {
            return FromFormat(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Returns a new markup widget from a composite format string, escaping formatted values.
        /// </summary>
        /// <param name="provider">The format provider used for formatted values.</param>
        /// <param name="format">A composite markup format string containing color tags such as <c>[green]{0}[/]</c>.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        /// <returns>A markup widget with escaped formatted values.</returns>
        public static Markup FromFormat(IFormatProvider provider, string format, params object[] args)
        {
            return new Markup(MarkupEntry.FormatMarkup(provider, format, args));
        }

        /// <summary>
        /// Escapes text so it is rendered literally inside markup.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>Markup-safe text.</returns>
        public static string Escape(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return MarkupEntry.EscapeMarkupText(text);
        }

        /// <summary>
        /// Removes recognized markup tags from text.
        /// </summary>
        /// <param name="text">The markup text to strip.</param>
        /// <returns>Plain text with recognized markup tags removed.</returns>
        public static string Remove(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return MarkupEntry.RemoveMarkupText(text);
        }

        /// <summary>
        /// Replaces the markup text rendered by this instance.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        /// <returns>The current <see cref="Markup"/> instance.</returns>
        public Markup Text(string markup)
        {
            Markup = markup ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Appends markup text to the current content.
        /// </summary>
        /// <param name="markup">The markup text to append.</param>
        /// <returns>The current <see cref="Markup"/> instance.</returns>
        public Markup Append(string markup)
        {
            Markup += markup ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Enables or disables appending a line break after rendering.
        /// </summary>
        /// <param name="enabled">Whether a line break should be appended.</param>
        /// <returns>The current <see cref="Markup"/> instance.</returns>
        public Markup NewLine(bool enabled = true)
        {
            WriteLine = enabled;
            return this;
        }

        /// <summary>
        /// Disables appending a line break after rendering.
        /// </summary>
        /// <returns>The current <see cref="Markup"/> instance.</returns>
        public Markup NoNewLine()
        {
            WriteLine = false;
            return this;
        }
    }
}
