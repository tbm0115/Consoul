using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides common string validation helpers for <see cref="TextPrompt{T}"/>.
    /// </summary>
    public static class TextPromptStringExtensions
    {
        /// <summary>
        /// Requires a string prompt value to contain at least the specified number of characters.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="minimumLength">The inclusive minimum character count.</param>
        /// <param name="message">Optional message rendered when the value is too short.</param>
        /// <returns>The configured <see cref="TextPrompt{T}"/>.</returns>
        public static TextPrompt<string> MinimumLength(this TextPrompt<string> prompt, int minimumLength, string message = null)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            if (minimumLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLength), "Minimum length cannot be negative.");
            }

            return prompt.Validate(
                value => (value ?? string.Empty).Length >= minimumLength,
                message ?? "Value must be at least " + minimumLength + " characters.");
        }

        /// <summary>
        /// Requires a string prompt value to contain no more than the specified number of characters.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="maximumLength">The inclusive maximum character count.</param>
        /// <param name="message">Optional message rendered when the value is too long.</param>
        /// <returns>The configured <see cref="TextPrompt{T}"/>.</returns>
        public static TextPrompt<string> MaximumLength(this TextPrompt<string> prompt, int maximumLength, string message = null)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            if (maximumLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLength), "Maximum length cannot be negative.");
            }

            return prompt.Validate(
                value => (value ?? string.Empty).Length <= maximumLength,
                message ?? "Value must be no more than " + maximumLength + " characters.");
        }

        /// <summary>
        /// Requires a string prompt value to contain a character count inside the inclusive range.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="minimumLength">The inclusive minimum character count.</param>
        /// <param name="maximumLength">The inclusive maximum character count.</param>
        /// <param name="message">Optional message rendered when the value length is outside the range.</param>
        /// <returns>The configured <see cref="TextPrompt{T}"/>.</returns>
        public static TextPrompt<string> LengthBetween(this TextPrompt<string> prompt, int minimumLength, int maximumLength, string message = null)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            if (minimumLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLength), "Minimum length cannot be negative.");
            }

            if (maximumLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLength), "Maximum length cannot be negative.");
            }

            if (minimumLength > maximumLength)
            {
                throw new ArgumentException("Minimum length cannot be greater than maximum length.", nameof(minimumLength));
            }

            return prompt.Validate(
                value =>
                {
                    int length = (value ?? string.Empty).Length;
                    return length >= minimumLength && length <= maximumLength;
                },
                message ?? "Value must be between " + minimumLength + " and " + maximumLength + " characters.");
        }
    }
}
