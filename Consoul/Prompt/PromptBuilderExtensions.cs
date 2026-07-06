using System;
using System.Collections.Generic;
using System.Globalization;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent prompt builder methods for settings that are exposed as Consoul prompt properties.
    /// </summary>
    public static class PromptBuilderExtensions
    {
        /// <summary>
        /// Sets the maximum number of choices shown per page.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="pageSize">The maximum number of choices per page.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt PageSize(this SelectionPrompt prompt, int pageSize)
        {
            return Require(prompt).WithPageSize(pageSize);
        }

        /// <summary>
        /// Sets the maximum number of typed choices shown per page.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="pageSize">The maximum number of choices per page.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> PageSize<T>(this SelectionPrompt<T> prompt, int pageSize)
        {
            return Require(prompt).WithPageSize(pageSize);
        }

        /// <summary>
        /// Sets the maximum number of multi-selection choices shown per page.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="pageSize">The maximum number of choices per page.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt PageSize(this MultiSelectionPrompt prompt, int pageSize)
        {
            return Require(prompt).WithPageSize(pageSize);
        }

        /// <summary>
        /// Sets the maximum number of typed multi-selection choices shown per page.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="pageSize">The maximum number of choices per page.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> PageSize<T>(this MultiSelectionPrompt<T> prompt, int pageSize)
        {
            return Require(prompt).WithPageSize(pageSize);
        }

        /// <summary>
        /// Sets the paging helper text.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The paging helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt MoreChoicesText(this SelectionPrompt prompt, string text)
        {
            return Require(prompt).WithMoreChoicesText(text);
        }

        /// <summary>
        /// Sets the paging helper text for a typed selection prompt.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The paging helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> MoreChoicesText<T>(this SelectionPrompt<T> prompt, string text)
        {
            return Require(prompt).WithMoreChoicesText(text);
        }

        /// <summary>
        /// Sets the paging helper text for a multi-selection prompt.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The paging helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt MoreChoicesText(this MultiSelectionPrompt prompt, string text)
        {
            return Require(prompt).WithMoreChoicesText(text);
        }

        /// <summary>
        /// Sets the paging helper text for a typed multi-selection prompt.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The paging helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> MoreChoicesText<T>(this MultiSelectionPrompt<T> prompt, string text)
        {
            return Require(prompt).WithMoreChoicesText(text);
        }

        /// <summary>
        /// Sets whether keyboard navigation wraps at the first and last available options.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="enabled">Whether arrow-key navigation should wrap around.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt WrapAround(this SelectionPrompt prompt, bool enabled = true)
        {
            return Require(prompt).UseWrapAround(enabled);
        }

        /// <summary>
        /// Sets whether typed selection keyboard navigation wraps at the first and last available options.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="enabled">Whether arrow-key navigation should wrap around.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> WrapAround<T>(this SelectionPrompt<T> prompt, bool enabled = true)
        {
            return Require(prompt).UseWrapAround(enabled);
        }

        /// <summary>
        /// Sets whether multi-selection keyboard navigation wraps at the first and last available options.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="enabled">Whether arrow-key navigation should wrap around.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt WrapAround(this MultiSelectionPrompt prompt, bool enabled = true)
        {
            return Require(prompt).UseWrapAround(enabled);
        }

        /// <summary>
        /// Sets whether typed multi-selection keyboard navigation wraps at the first and last available options.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="enabled">Whether arrow-key navigation should wrap around.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> WrapAround<T>(this MultiSelectionPrompt<T> prompt, bool enabled = true)
        {
            return Require(prompt).UseWrapAround(enabled);
        }

        /// <summary>
        /// Sets the keyboard search placeholder text shown before any filter text has been entered.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The placeholder text to render in keyboard search mode.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt SearchPlaceholderText(this SelectionPrompt prompt, string text)
        {
            return Require(prompt).WithSearchPlaceholderText(text);
        }

        /// <summary>
        /// Sets the typed keyboard search placeholder text shown before any filter text has been entered.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The placeholder text to render in keyboard search mode.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> SearchPlaceholderText<T>(this SelectionPrompt<T> prompt, string text)
        {
            return Require(prompt).WithSearchPlaceholderText(text);
        }

        /// <summary>
        /// Sets the multi-selection keyboard search placeholder text shown before any filter text has been entered.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The placeholder text to render in keyboard search mode.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt SearchPlaceholderText(this MultiSelectionPrompt prompt, string text)
        {
            return Require(prompt).WithSearchPlaceholderText(text);
        }

        /// <summary>
        /// Sets the typed multi-selection keyboard search placeholder text shown before any filter text has been entered.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The placeholder text to render in keyboard search mode.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> SearchPlaceholderText<T>(this MultiSelectionPrompt<T> prompt, string text)
        {
            return Require(prompt).WithSearchPlaceholderText(text);
        }

        /// <summary>
        /// Sets the command prefix used for filtering selection prompt options.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="prefix">The command prefix.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt FilterCommandPrefix(this SelectionPrompt prompt, string prefix)
        {
            SelectionPrompt configured = Require(prompt);
            configured.FilterCommandPrefix = prefix ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the command prefix used for filtering typed selection prompt options.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="prefix">The command prefix.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> FilterCommandPrefix<T>(this SelectionPrompt<T> prompt, string prefix)
        {
            SelectionPrompt<T> configured = Require(prompt);
            configured.FilterCommandPrefix = prefix ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the command prefix used for filtering multi-selection prompt options.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="prefix">The command prefix.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt FilterCommandPrefix(this MultiSelectionPrompt prompt, string prefix)
        {
            MultiSelectionPrompt configured = Require(prompt);
            configured.FilterCommandPrefix = prefix ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the command prefix used for filtering typed multi-selection prompt options.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="prefix">The command prefix.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> FilterCommandPrefix<T>(this MultiSelectionPrompt<T> prompt, string prefix)
        {
            MultiSelectionPrompt<T> configured = Require(prompt);
            configured.FilterCommandPrefix = prefix ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown when a selection prompt can filter options.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The filter helper text. Use <c>{0}</c> for the command prefix.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt FilterHelpText(this SelectionPrompt prompt, string text)
        {
            SelectionPrompt configured = Require(prompt);
            configured.FilterHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown when a typed selection prompt can filter options.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The filter helper text. Use <c>{0}</c> for the command prefix.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> FilterHelpText<T>(this SelectionPrompt<T> prompt, string text)
        {
            SelectionPrompt<T> configured = Require(prompt);
            configured.FilterHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown when a multi-selection prompt can filter options.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The filter helper text. Use <c>{0}</c> for the command prefix.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt FilterHelpText(this MultiSelectionPrompt prompt, string text)
        {
            MultiSelectionPrompt configured = Require(prompt);
            configured.FilterHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown when a typed multi-selection prompt can filter options.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The filter helper text. Use <c>{0}</c> for the command prefix.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> FilterHelpText<T>(this MultiSelectionPrompt<T> prompt, string text)
        {
            MultiSelectionPrompt<T> configured = Require(prompt);
            configured.FilterHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the text shown when a selection prompt filter is active.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The active filter text. Placeholders are filter text and match count.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt ActiveFilterText(this SelectionPrompt prompt, string text)
        {
            SelectionPrompt configured = Require(prompt);
            configured.ActiveFilterText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the text shown when a typed selection prompt filter is active.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The active filter text. Placeholders are filter text and match count.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> ActiveFilterText<T>(this SelectionPrompt<T> prompt, string text)
        {
            SelectionPrompt<T> configured = Require(prompt);
            configured.ActiveFilterText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the text shown when a multi-selection prompt filter is active.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The active filter text. Placeholders are filter text and match count.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt ActiveFilterText(this MultiSelectionPrompt prompt, string text)
        {
            MultiSelectionPrompt configured = Require(prompt);
            configured.ActiveFilterText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the text shown when a typed multi-selection prompt filter is active.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The active filter text. Placeholders are filter text and match count.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> ActiveFilterText<T>(this MultiSelectionPrompt<T> prompt, string text)
        {
            MultiSelectionPrompt<T> configured = Require(prompt);
            configured.ActiveFilterText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the text shown when a selection prompt filter has no matches.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The no-match text. Placeholders are filter text and match count.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt NoFilterMatchesText(this SelectionPrompt prompt, string text)
        {
            SelectionPrompt configured = Require(prompt);
            configured.NoFilterMatchesText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the text shown when a typed selection prompt filter has no matches.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The no-match text. Placeholders are filter text and match count.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> NoFilterMatchesText<T>(this SelectionPrompt<T> prompt, string text)
        {
            SelectionPrompt<T> configured = Require(prompt);
            configured.NoFilterMatchesText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the text shown when a multi-selection prompt filter has no matches.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The no-match text. Placeholders are filter text and match count.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt NoFilterMatchesText(this MultiSelectionPrompt prompt, string text)
        {
            MultiSelectionPrompt configured = Require(prompt);
            configured.NoFilterMatchesText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the text shown when a typed multi-selection prompt filter has no matches.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The no-match text. Placeholders are filter text and match count.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> NoFilterMatchesText<T>(this MultiSelectionPrompt<T> prompt, string text)
        {
            MultiSelectionPrompt<T> configured = Require(prompt);
            configured.NoFilterMatchesText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown when a selection prompt uses keyboard navigation.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The keyboard navigation helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt KeyboardNavigationHelpText(this SelectionPrompt prompt, string text)
        {
            SelectionPrompt configured = Require(prompt);
            configured.KeyboardNavigationHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown when a typed selection prompt uses keyboard navigation.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The keyboard navigation helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> KeyboardNavigationHelpText<T>(this SelectionPrompt<T> prompt, string text)
        {
            SelectionPrompt<T> configured = Require(prompt);
            configured.KeyboardNavigationHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown when a multi-selection prompt uses keyboard navigation.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The keyboard navigation helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt KeyboardNavigationHelpText(this MultiSelectionPrompt prompt, string text)
        {
            MultiSelectionPrompt configured = Require(prompt);
            configured.KeyboardNavigationHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown when a typed multi-selection prompt uses keyboard navigation.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The keyboard navigation helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> KeyboardNavigationHelpText<T>(this MultiSelectionPrompt<T> prompt, string text)
        {
            MultiSelectionPrompt<T> configured = Require(prompt);
            configured.KeyboardNavigationHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown before a keyboard selection prompt has filter text.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The keyboard filter helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt KeyboardFilterHelpText(this SelectionPrompt prompt, string text)
        {
            SelectionPrompt configured = Require(prompt);
            configured.KeyboardFilterHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown before a typed keyboard selection prompt has filter text.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The keyboard filter helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> KeyboardFilterHelpText<T>(this SelectionPrompt<T> prompt, string text)
        {
            SelectionPrompt<T> configured = Require(prompt);
            configured.KeyboardFilterHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown before a keyboard multi-selection prompt has filter text.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The keyboard filter helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt KeyboardFilterHelpText(this MultiSelectionPrompt prompt, string text)
        {
            MultiSelectionPrompt configured = Require(prompt);
            configured.KeyboardFilterHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the helper text shown before a typed keyboard multi-selection prompt has filter text.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="text">The keyboard filter helper text.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> KeyboardFilterHelpText<T>(this MultiSelectionPrompt<T> prompt, string text)
        {
            MultiSelectionPrompt<T> configured = Require(prompt);
            configured.KeyboardFilterHelpText = text ?? string.Empty;
            return configured;
        }

        /// <summary>
        /// Sets the message rendered when a selection prompt receives input that does not match an available choice.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="message">The invalid selection message.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt InvalidSelectionMessage(this SelectionPrompt prompt, string message)
        {
            return Require(prompt).WithInvalidSelectionMessage(message);
        }

        /// <summary>
        /// Sets the message rendered when a typed selection prompt receives input that does not match an available choice.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="message">The invalid selection message.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> InvalidSelectionMessage<T>(this SelectionPrompt<T> prompt, string message)
        {
            return Require(prompt).WithInvalidSelectionMessage(message);
        }

        /// <summary>
        /// Sets the message rendered when a multi-selection prompt receives input that does not match available choices.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="message">The invalid selection message.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt InvalidSelectionMessage(this MultiSelectionPrompt prompt, string message)
        {
            return Require(prompt).WithInvalidSelectionMessage(message);
        }

        /// <summary>
        /// Sets the message rendered when a typed multi-selection prompt receives input that does not match available choices.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="message">The invalid selection message.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> InvalidSelectionMessage<T>(this MultiSelectionPrompt<T> prompt, string message)
        {
            return Require(prompt).WithInvalidSelectionMessage(message);
        }

        /// <summary>
        /// Sets the message rendered when a selection prompt receives input that does not match an available choice.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="message">The invalid choice message.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt InvalidChoiceMessage(this SelectionPrompt prompt, string message)
        {
            return InvalidSelectionMessage(prompt, message);
        }

        /// <summary>
        /// Sets the message rendered when a typed selection prompt receives input that does not match an available choice.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="message">The invalid choice message.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> InvalidChoiceMessage<T>(this SelectionPrompt<T> prompt, string message)
        {
            return InvalidSelectionMessage(prompt, message);
        }

        /// <summary>
        /// Sets the message rendered when a multi-selection prompt receives input that does not match available choices.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="message">The invalid choice message.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt InvalidChoiceMessage(this MultiSelectionPrompt prompt, string message)
        {
            return InvalidSelectionMessage(prompt, message);
        }

        /// <summary>
        /// Sets the message rendered when a typed multi-selection prompt receives input that does not match available choices.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="message">The invalid choice message.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> InvalidChoiceMessage<T>(this MultiSelectionPrompt<T> prompt, string message)
        {
            return InvalidSelectionMessage(prompt, message);
        }

        /// <summary>
        /// Sets the style used for disabled selection prompt choices.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt DisabledStyle(this SelectionPrompt prompt, Style style)
        {
            SelectionPrompt configured = Require(prompt);
            configured.DisabledStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the style used for disabled selection prompt choices.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The foreground color to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt DisabledStyle(this SelectionPrompt prompt, ConsoleColor color)
        {
            return DisabledStyle(prompt, new Style((ConsoleColor?)color));
        }

        /// <summary>
        /// Sets the style used for disabled selection prompt choices from Spectre-style style text.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">Style text such as <c>grey</c>, <c>red</c>, <c>#64748b</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt DisabledStyle(this SelectionPrompt prompt, string style)
        {
            return DisabledStyle(prompt, Style.Parse(style));
        }

        /// <summary>
        /// Sets the style used for disabled typed selection prompt choices.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> DisabledStyle<T>(this SelectionPrompt<T> prompt, Style style)
        {
            SelectionPrompt<T> configured = Require(prompt);
            configured.DisabledStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the style used for disabled typed selection prompt choices.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The foreground color to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> DisabledStyle<T>(this SelectionPrompt<T> prompt, ConsoleColor color)
        {
            return DisabledStyle(prompt, new Style((ConsoleColor?)color));
        }

        /// <summary>
        /// Sets the style used for disabled typed selection prompt choices from Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">Style text such as <c>grey</c>, <c>red</c>, <c>#64748b</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> DisabledStyle<T>(this SelectionPrompt<T> prompt, string style)
        {
            return DisabledStyle(prompt, Style.Parse(style));
        }

        /// <summary>
        /// Sets the style used for visible selection prompt text that matches the active search filter.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt SearchHighlightStyle(this SelectionPrompt prompt, Style style)
        {
            SelectionPrompt configured = Require(prompt);
            configured.SearchHighlightStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the style used for visible selection prompt text that matches the active search filter.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The foreground color to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt SearchHighlightStyle(this SelectionPrompt prompt, ConsoleColor color)
        {
            return SearchHighlightStyle(prompt, new Style((ConsoleColor?)color));
        }

        /// <summary>
        /// Sets the style used for visible selection prompt text that matches the active search filter from Spectre-style style text.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">Style text such as <c>yellow</c>, <c>aqua</c>, <c>#facc15</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt SearchHighlightStyle(this SelectionPrompt prompt, string style)
        {
            return SearchHighlightStyle(prompt, Style.Parse(style));
        }

        /// <summary>
        /// Sets the style used for visible typed selection prompt text that matches the active search filter.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> SearchHighlightStyle<T>(this SelectionPrompt<T> prompt, Style style)
        {
            SelectionPrompt<T> configured = Require(prompt);
            configured.SearchHighlightStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the style used for visible typed selection prompt text that matches the active search filter.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The foreground color to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> SearchHighlightStyle<T>(this SelectionPrompt<T> prompt, ConsoleColor color)
        {
            return SearchHighlightStyle(prompt, new Style((ConsoleColor?)color));
        }

        /// <summary>
        /// Sets the style used for visible typed selection prompt text that matches the active search filter from Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">Style text such as <c>yellow</c>, <c>aqua</c>, <c>#facc15</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> SearchHighlightStyle<T>(this SelectionPrompt<T> prompt, string style)
        {
            return SearchHighlightStyle(prompt, Style.Parse(style));
        }

        /// <summary>
        /// Sets the style used for visible multi-selection prompt text that matches the active search filter.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt SearchHighlightStyle(this MultiSelectionPrompt prompt, Style style)
        {
            MultiSelectionPrompt configured = Require(prompt);
            configured.SearchHighlightStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the style used for visible multi-selection prompt text that matches the active search filter.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The foreground color to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt SearchHighlightStyle(this MultiSelectionPrompt prompt, ConsoleColor color)
        {
            return SearchHighlightStyle(prompt, new Style((ConsoleColor?)color));
        }

        /// <summary>
        /// Sets the style used for visible multi-selection prompt text that matches the active search filter from Spectre-style style text.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">Style text such as <c>yellow</c>, <c>aqua</c>, <c>#facc15</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt SearchHighlightStyle(this MultiSelectionPrompt prompt, string style)
        {
            return SearchHighlightStyle(prompt, Style.Parse(style));
        }

        /// <summary>
        /// Sets the style used for visible typed multi-selection prompt text that matches the active search filter.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> SearchHighlightStyle<T>(this MultiSelectionPrompt<T> prompt, Style style)
        {
            MultiSelectionPrompt<T> configured = Require(prompt);
            configured.SearchHighlightStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the style used for visible typed multi-selection prompt text that matches the active search filter.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The foreground color to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> SearchHighlightStyle<T>(this MultiSelectionPrompt<T> prompt, ConsoleColor color)
        {
            return SearchHighlightStyle(prompt, new Style((ConsoleColor?)color));
        }

        /// <summary>
        /// Sets the style used for visible typed multi-selection prompt text that matches the active search filter from Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The prompt choice type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">Style text such as <c>yellow</c>, <c>aqua</c>, <c>#facc15</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> SearchHighlightStyle<T>(this MultiSelectionPrompt<T> prompt, string style)
        {
            return SearchHighlightStyle(prompt, Style.Parse(style));
        }

        /// <summary>
        /// Allows empty text input using Spectre-style builder vocabulary.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> AllowEmpty<T>(this TextPrompt<T> prompt)
        {
            return Require(prompt).AllowEmptyInput();
        }

        /// <summary>
        /// Allows empty text input using Spectre-style optional prompt vocabulary.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> NotRequired<T>(this TextPrompt<T> prompt)
        {
            return AllowEmpty(prompt);
        }

        /// <summary>
        /// Requires non-empty text input using Spectre-style builder vocabulary.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="message">Optional message rendered when the value is missing.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> NotEmpty<T>(this TextPrompt<T> prompt, string message = null)
        {
            return Require(prompt).Required(message);
        }

        /// <summary>
        /// Sets the culture used by a text prompt's built-in parser and default value formatter.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> Culture<T>(this TextPrompt<T> prompt, CultureInfo culture)
        {
            return Require(prompt).UseCulture(culture);
        }

        /// <summary>
        /// Sets the message rendered when the prompt has known choices and the entered value cannot be matched to one.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="message">The invalid choice message.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> InvalidChoiceMessage<T>(this TextPrompt<T> prompt, string message)
        {
            return Require(prompt).WithInvalidChoiceMessage(message);
        }

        /// <summary>
        /// Restricts a text prompt to known choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="choices">The allowed prompt values.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> Choices<T>(this TextPrompt<T> prompt, params T[] choices)
        {
            return Require(prompt)
                .AcceptOnly(choices ?? new T[0])
                .UseInvalidChoiceMessageForAcceptedValueMismatch();
        }

        /// <summary>
        /// Restricts a text prompt to known choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="choices">The allowed prompt values.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> Choices<T>(this TextPrompt<T> prompt, IEnumerable<T> choices)
        {
            return Require(prompt)
                .AcceptOnly(choices)
                .UseInvalidChoiceMessageForAcceptedValueMismatch();
        }

        /// <summary>
        /// Shows accepted text prompt choices in the prompt message.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="show"><c>true</c> to show choices; otherwise, <c>false</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> ShowChoices<T>(this TextPrompt<T> prompt, bool show = true)
        {
            TextPrompt<T> configured = Require(prompt);
            configured.ShowChoices = show;
            return configured;
        }

        /// <summary>
        /// Hides accepted text prompt choices from the prompt message.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> HideChoices<T>(this TextPrompt<T> prompt)
        {
            return ShowChoices(prompt, false);
        }

        /// <summary>
        /// Shows a text prompt default value in the prompt message.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="show"><c>true</c> to show the default value; otherwise, <c>false</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> ShowDefaultValue<T>(this TextPrompt<T> prompt, bool show = true)
        {
            TextPrompt<T> configured = Require(prompt);
            configured.ShowDefaultValue = show;
            return configured;
        }

        /// <summary>
        /// Hides a text prompt default value from the prompt message while preserving Enter-to-accept behavior.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> HideDefaultValue<T>(this TextPrompt<T> prompt)
        {
            return ShowDefaultValue(prompt, false);
        }

        /// <summary>
        /// Clears the rendered text prompt after a successful response.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="clear"><c>true</c> to clear the prompt after completion; otherwise, <c>false</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> ClearOnFinish<T>(this TextPrompt<T> prompt, bool clear = true)
        {
            TextPrompt<T> configured = Require(prompt);
            configured.ClearOnFinish = clear;
            return configured;
        }

        /// <summary>
        /// Seeds a text prompt's default value into the editable input buffer.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="enabled"><c>true</c> to seed the editable buffer; otherwise, <c>false</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> EditableDefaultValue<T>(this TextPrompt<T> prompt, bool enabled = true)
        {
            TextPrompt<T> configured = Require(prompt);
            configured.EditableDefaultValue = enabled;
            return configured;
        }

        /// <summary>
        /// Reads text prompt input as a secret without echoing typed characters.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="mask">The mask character, or <see langword="null"/> to hide input without rendering mask characters.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> Secret<T>(this TextPrompt<T> prompt, char? mask = '*')
        {
            TextPrompt<T> configured = Require(prompt);
            configured.IsSecret = true;
            configured.Mask = mask;
            return configured;
        }

        /// <summary>
        /// Sets the character used to mask secret text prompt input.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="mask">The mask character, or <see langword="null"/> to hide input without rendering mask characters.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> Mask<T>(this TextPrompt<T> prompt, char? mask = '*')
        {
            TextPrompt<T> configured = Require(prompt);
            configured.Mask = mask;
            return configured;
        }

        /// <summary>
        /// Sets the style used when secret text prompt mask characters are rendered.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The foreground color to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> MaskStyle<T>(this TextPrompt<T> prompt, ConsoleColor color)
        {
            TextPrompt<T> configured = Require(prompt);
            configured.MaskColor = color;
            return configured;
        }

        /// <summary>
        /// Sets the style used when secret text prompt mask characters are rendered from Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> MaskStyle<T>(this TextPrompt<T> prompt, string style)
        {
            TextPrompt<T> configured = Require(prompt);
            configured.MaskColor = Style.ParseForeground(style, configured.MaskColor);
            return configured;
        }

        /// <summary>
        /// Sets the separator used when accepted text prompt choices are rendered.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="separator">The separator to render between accepted values.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> ChoicesSeparator<T>(this TextPrompt<T> prompt, string separator)
        {
            TextPrompt<T> configured = Require(prompt);
            configured.ChoicesSeparator = separator;
            return configured;
        }

        /// <summary>
        /// Sets the style used when accepted text prompt choices are rendered.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> ChoicesStyle<T>(this TextPrompt<T> prompt, Style style)
        {
            TextPrompt<T> configured = Require(prompt);
            configured.ChoicesStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the style used when accepted text prompt choices are rendered.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The foreground color to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> ChoicesStyle<T>(this TextPrompt<T> prompt, ConsoleColor color)
        {
            return ChoicesStyle(prompt, new Style((ConsoleColor?)color));
        }

        /// <summary>
        /// Sets the style used when accepted text prompt choices are rendered from Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> ChoicesStyle<T>(this TextPrompt<T> prompt, string style)
        {
            return ChoicesStyle(prompt, Style.Parse(style));
        }

        /// <summary>
        /// Sets the style used when a text prompt default value is rendered.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> DefaultValueStyle<T>(this TextPrompt<T> prompt, Style style)
        {
            TextPrompt<T> configured = Require(prompt);
            configured.DefaultValueStyle = style;
            return configured;
        }

        /// <summary>
        /// Sets the style used when a text prompt default value is rendered.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The foreground color to apply.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> DefaultValueStyle<T>(this TextPrompt<T> prompt, ConsoleColor color)
        {
            return DefaultValueStyle(prompt, new Style((ConsoleColor?)color));
        }

        /// <summary>
        /// Sets the style used when a text prompt default value is rendered from Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The prompt value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> DefaultValueStyle<T>(this TextPrompt<T> prompt, string style)
        {
            return DefaultValueStyle(prompt, Style.Parse(style));
        }

        /// <summary>
        /// Allows empty secret input using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <returns>The configured prompt.</returns>
        public static SecretPrompt AllowEmpty(this SecretPrompt prompt)
        {
            return Require(prompt).AllowEmptyInput();
        }

        /// <summary>
        /// Allows empty secret input using Spectre-style optional prompt vocabulary.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <returns>The configured prompt.</returns>
        public static SecretPrompt NotRequired(this SecretPrompt prompt)
        {
            return AllowEmpty(prompt);
        }

        private static TPrompt Require<TPrompt>(TPrompt prompt) where TPrompt : class
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            return prompt;
        }
    }
}
