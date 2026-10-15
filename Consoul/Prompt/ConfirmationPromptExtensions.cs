using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides builder-style helpers for <see cref="ConfirmationPrompt"/>.
    /// </summary>
    public static class ConfirmationPromptExtensions
    {
        /// <summary>
        /// Shows or hides the yes/no choices in the prompt message.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="show"><c>true</c> to show choices; otherwise, <c>false</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt ShowChoices(this ConfirmationPrompt prompt, bool show = true)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.ShowChoices = show;
            return configured;
        }

        /// <summary>
        /// Hides the yes/no choices in the prompt message.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt HideChoices(this ConfirmationPrompt prompt)
        {
            return ShowChoices(prompt, false);
        }

        /// <summary>
        /// Sets the style used for non-default yes/no choices.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="style">The style to use for non-default choices.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt ChoicesStyle(this ConfirmationPrompt prompt, Style style)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.ChoicesStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the style used for non-default yes/no choices from Spectre-style style text.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="style">Style text such as <c>blue</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt ChoicesStyle(this ConfirmationPrompt prompt, string style)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.ChoicesStyle = ConsoulLibrary.Style.Parse(style);
            return configured;
        }

        /// <summary>
        /// Shows or hides the default answer emphasis in the prompt message.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="show"><c>true</c> to show default emphasis; otherwise, <c>false</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt ShowDefaultValue(this ConfirmationPrompt prompt, bool show = true)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.ShowDefaultValue = show;
            return configured;
        }

        /// <summary>
        /// Hides the default answer emphasis in the prompt message.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt HideDefaultValue(this ConfirmationPrompt prompt)
        {
            return ShowDefaultValue(prompt, false);
        }

        /// <summary>
        /// Sets the style used for the default yes/no choice.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="style">The style to use for the default choice.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt DefaultValueStyle(this ConfirmationPrompt prompt, Style style)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.DefaultValueStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the style used for the default yes/no choice from Spectre-style style text.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt DefaultValueStyle(this ConfirmationPrompt prompt, string style)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.DefaultValueStyle = ConsoulLibrary.Style.Parse(style);
            return configured;
        }

        /// <summary>
        /// Sets the message shown when input cannot be matched to a confirmation choice.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="message">The invalid choice message.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt InvalidChoiceMessage(this ConfirmationPrompt prompt, string message)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.InvalidChoiceMessage = message ?? throw new ArgumentNullException(nameof(message));
            return configured;
        }

        /// <summary>
        /// Sets the character used for an affirmative answer.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="character">The affirmative answer character.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt Yes(this ConfirmationPrompt prompt, char character)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.Yes = character;
            return configured;
        }

        /// <summary>
        /// Sets the character used for a negative answer.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="character">The negative answer character.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt No(this ConfirmationPrompt prompt, char character)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.No = character;
            return configured;
        }

        /// <summary>
        /// Sets whether the user must press Enter after typing a confirmation choice.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="require"><c>true</c> to require Enter; otherwise, <c>false</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt RequireEnter(this ConfirmationPrompt prompt, bool require = true)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.RequireEnter = require;
            return configured;
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt PromptStyle(this ConfirmationPrompt prompt, ConsoleColor color)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.PromptColor = color;
            return configured;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt PromptStyle(this ConfirmationPrompt prompt, Style style)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.PromptColor = PromptRenderHelpers.ResolveStyleColor(style, configured.PromptColor);
            configured.PromptTextStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt PromptStyle(this ConfirmationPrompt prompt, string style)
        {
            return PromptStyle(prompt, ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt Style(this ConfirmationPrompt prompt, ConsoleColor color)
        {
            return PromptStyle(prompt, color);
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt Style(this ConfirmationPrompt prompt, Style style)
        {
            return PromptStyle(prompt, style);
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt Style(this ConfirmationPrompt prompt, string style)
        {
            return PromptStyle(prompt, style);
        }

        /// <summary>
        /// Sets the color used for invalid input messages.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt InvalidStyle(this ConfirmationPrompt prompt, ConsoleColor color)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.InvalidColor = color;
            return configured;
        }

        /// <summary>
        /// Sets the color used for invalid input messages from a reusable style object.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt InvalidStyle(this ConfirmationPrompt prompt, Style style)
        {
            ConfirmationPrompt configured = Require(prompt);
            configured.InvalidColor = PromptRenderHelpers.ResolveStyleColor(style, configured.InvalidColor);
            configured.InvalidTextStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the color used for invalid input messages from Spectre-style style text.
        /// </summary>
        /// <param name="prompt">The confirmation prompt to configure.</param>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#ef4444</c>, or <c>white on red</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt InvalidStyle(this ConfirmationPrompt prompt, string style)
        {
            return InvalidStyle(prompt, ConsoulLibrary.Style.Parse(style));
        }

        private static ConfirmationPrompt Require(ConfirmationPrompt prompt)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            return prompt;
        }
    }
}
