using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one keyboard shortcut or action hint rendered by a shortcut bar.
    /// </summary>
    public class ShortcutItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortcutItem"/> class.
        /// </summary>
        public ShortcutItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortcutItem"/> class.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        public ShortcutItem(string key, string description, bool disabled = false)
        {
            Key = key ?? string.Empty;
            Description = description ?? string.Empty;
            Disabled = disabled;
        }

        /// <summary>
        /// Gets or sets the key or command token.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the shortcut should render as disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme for the key token.
        /// </summary>
        public ColorScheme? KeyScheme { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme for the action description.
        /// </summary>
        public ColorScheme? DescriptionScheme { get; set; }

        /// <summary>
        /// Creates a shortcut item.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <param name="keyColor">Optional foreground color for the key token.</param>
        /// <param name="descriptionColor">Optional foreground color for the action description.</param>
        /// <returns>A configured <see cref="ShortcutItem"/> instance.</returns>
        public static ShortcutItem Create(
            string key,
            string description,
            bool disabled = false,
            ConsoleColor? keyColor = null,
            ConsoleColor? descriptionColor = null)
        {
            return new ShortcutItem(key, description, disabled)
            {
                KeyScheme = keyColor.HasValue ? Scheme(keyColor.Value) : (ColorScheme?)null,
                DescriptionScheme = descriptionColor.HasValue ? Scheme(descriptionColor.Value) : (ColorScheme?)null
            };
        }

        /// <summary>
        /// Creates a shortcut item using reusable style objects.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="keyStyle">Optional style for the key token.</param>
        /// <param name="descriptionStyle">Optional style for the action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <returns>A configured <see cref="ShortcutItem"/> instance.</returns>
        public static ShortcutItem Create(
            string key,
            string description,
            Style keyStyle,
            Style descriptionStyle = null,
            bool disabled = false)
        {
            return new ShortcutItem(key, description, disabled)
            {
                KeyScheme = Scheme(keyStyle),
                DescriptionScheme = Scheme(descriptionStyle)
            };
        }

        /// <summary>
        /// Creates a shortcut item using Spectre-style style text.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <param name="keyStyle">Optional style for the key token, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="descriptionStyle">Optional style for the action description, such as <c>grey</c>.</param>
        /// <returns>A configured <see cref="ShortcutItem"/> instance.</returns>
        public static ShortcutItem CreateStyled(
            string key,
            string description,
            bool disabled = false,
            string keyStyle = null,
            string descriptionStyle = null)
        {
            return new ShortcutItem(key, description, disabled)
            {
                KeyScheme = string.IsNullOrWhiteSpace(keyStyle) ? (ColorScheme?)null : Style.Parse(keyStyle).ToColorScheme(RenderOptions.DefaultScheme),
                DescriptionScheme = string.IsNullOrWhiteSpace(descriptionStyle) ? (ColorScheme?)null : Style.Parse(descriptionStyle).ToColorScheme(RenderOptions.DefaultScheme)
            };
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme? Scheme(Style style)
        {
            return style?.ToColorScheme(RenderOptions.DefaultScheme);
        }
    }
}
