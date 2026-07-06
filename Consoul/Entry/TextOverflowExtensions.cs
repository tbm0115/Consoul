using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides fluent overflow helpers for text renderables.
    /// </summary>
    public static class TextOverflowExtensions
    {
        /// <summary>
        /// Sets how text wider than the available line width should be handled.
        /// </summary>
        /// <param name="text">The text renderable to configure.</param>
        /// <param name="overflow">The overflow behavior.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public static Text Overflow(this Text text, Overflow overflow)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            text.Options.Overflow = overflow;
            return text;
        }

        /// <summary>
        /// Crops text that is wider than the available line width.
        /// </summary>
        /// <param name="text">The text renderable to configure.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public static Text Crop(this Text text)
        {
            return Overflow(text, ConsoulLibrary.Overflow.Crop);
        }

        /// <summary>
        /// Adds an ellipsis marker when text is wider than the available line width.
        /// </summary>
        /// <param name="text">The text renderable to configure.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public static Text Ellipsis(this Text text)
        {
            return Overflow(text, ConsoulLibrary.Overflow.Ellipsis);
        }

        /// <summary>
        /// Sets how paragraph text wider than the available line width should be handled.
        /// </summary>
        /// <param name="paragraph">The paragraph renderable to configure.</param>
        /// <param name="overflow">The overflow behavior.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public static Paragraph Overflow(this Paragraph paragraph, Overflow overflow)
        {
            if (paragraph == null)
            {
                throw new ArgumentNullException(nameof(paragraph));
            }

            paragraph.Options.Overflow = overflow;
            return paragraph;
        }

        /// <summary>
        /// Crops paragraph text that is wider than the available line width.
        /// </summary>
        /// <param name="paragraph">The paragraph renderable to configure.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public static Paragraph Crop(this Paragraph paragraph)
        {
            return Overflow(paragraph, ConsoulLibrary.Overflow.Crop);
        }

        /// <summary>
        /// Adds an ellipsis marker when paragraph text is wider than the available line width.
        /// </summary>
        /// <param name="paragraph">The paragraph renderable to configure.</param>
        /// <returns>The current <see cref="Paragraph"/> instance.</returns>
        public static Paragraph Ellipsis(this Paragraph paragraph)
        {
            return Overflow(paragraph, ConsoulLibrary.Overflow.Ellipsis);
        }
    }
}
