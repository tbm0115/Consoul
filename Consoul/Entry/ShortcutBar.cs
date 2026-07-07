using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent shortcut bar construction over <see cref="ShortcutBarEntry"/>.
    /// </summary>
    public class ShortcutBar : ShortcutBarEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortcutBar"/> class.
        /// </summary>
        public ShortcutBar()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortcutBar"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the shortcut bar.</param>
        public ShortcutBar(ShortcutBarOptions options) : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortcutBar"/> class with items.
        /// </summary>
        /// <param name="items">The shortcut items to render.</param>
        /// <param name="options">Optional rendering options for the shortcut bar.</param>
        public ShortcutBar(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null) : base(items, options)
        {
        }

        /// <summary>
        /// Adds a shortcut item.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <param name="keyColor">Optional foreground color for the key token.</param>
        /// <param name="descriptionColor">Optional foreground color for the action description.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcut(
            string key,
            string description,
            bool disabled = false,
            ConsoleColor? keyColor = null,
            ConsoleColor? descriptionColor = null)
        {
            Add(key, description, disabled, keyColor, descriptionColor);
            return this;
        }

        /// <summary>
        /// Adds a shortcut item using Spectre-style color facades.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="keyColor">Foreground color used for the key token.</param>
        /// <param name="descriptionColor">Optional foreground color used for the action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcut(
            string key,
            string description,
            SpectreColor keyColor,
            SpectreColor? descriptionColor = null,
            bool disabled = false)
        {
            Add(ShortcutItem.Create(key, description, keyColor, descriptionColor, disabled));
            return this;
        }

        /// <summary>
        /// Adds a shortcut item using RGB foreground colors.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="keyColor">Foreground color used for the key token.</param>
        /// <param name="descriptionColor">Optional foreground color used for the action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcut(
            string key,
            string description,
            RgbColor keyColor,
            RgbColor? descriptionColor = null,
            bool disabled = false)
        {
            Add(ShortcutItem.Create(key, description, keyColor, descriptionColor, disabled));
            return this;
        }

        /// <summary>
        /// Adds a shortcut item using reusable style objects.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="keyStyle">Optional style for the key token.</param>
        /// <param name="descriptionStyle">Optional style for the action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcut(
            string key,
            string description,
            Style keyStyle,
            Style descriptionStyle = null,
            bool disabled = false)
        {
            Add(key, description, keyStyle, descriptionStyle, disabled);
            return this;
        }

        /// <summary>
        /// Adds a shortcut item using Spectre-style style text.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="keyStyle">Optional style for the key token, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="descriptionStyle">Optional style for the action description, such as <c>grey</c>.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcut(
            string key,
            string description,
            string keyStyle,
            string descriptionStyle = null,
            bool disabled = false)
        {
            Add(key, description, keyStyle, descriptionStyle, disabled);
            return this;
        }

        /// <summary>
        /// Adds a shortcut item using Spectre-style style text.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <param name="keyStyle">Optional style for the key token, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="descriptionStyle">Optional style for the action description, such as <c>grey</c>.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcutStyled(
            string key,
            string description,
            bool disabled = false,
            string keyStyle = null,
            string descriptionStyle = null)
        {
            AddStyled(key, description, disabled, keyStyle, descriptionStyle);
            return this;
        }

        /// <summary>
        /// Adds an existing shortcut item.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcut(ShortcutItem item)
        {
            Add(item);
            return this;
        }

        /// <summary>
        /// Adds existing shortcut items.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcuts(IEnumerable<ShortcutItem> items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (ShortcutItem item in items)
            {
                AddShortcut(item);
            }

            return this;
        }

        /// <summary>
        /// Adds shortcuts projected from a model collection using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into shortcuts.</param>
        /// <param name="keySelector">Selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Selects the shortcut action description.</param>
        /// <param name="disabledSelector">Optionally selects whether the shortcut should render as disabled.</param>
        /// <param name="keyStyleSelector">Optionally selects style text for the key token.</param>
        /// <param name="descriptionStyleSelector">Optionally selects style text for the action description.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcuts<T>(
            IEnumerable<T> items,
            Func<T, string> keySelector,
            Func<T, string> descriptionSelector,
            Func<T, bool> disabledSelector,
            Func<T, string> keyStyleSelector,
            Func<T, string> descriptionStyleSelector = null)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (descriptionSelector == null)
            {
                throw new ArgumentNullException(nameof(descriptionSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddShortcut(
                    keySelector(item),
                    descriptionSelector(item),
                    keyStyleSelector != null ? keyStyleSelector(item) : null,
                    descriptionStyleSelector != null ? descriptionStyleSelector(item) : null,
                    disabledSelector != null && disabledSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds shortcuts projected from a model collection using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into shortcuts.</param>
        /// <param name="keySelector">Selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Selects the shortcut action description.</param>
        /// <param name="keyStyleSelector">Optionally selects style text for the key token.</param>
        /// <param name="descriptionStyleSelector">Optionally selects style text for the action description.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcuts<T>(
            IEnumerable<T> items,
            Func<T, string> keySelector,
            Func<T, string> descriptionSelector,
            Func<T, string> keyStyleSelector,
            Func<T, string> descriptionStyleSelector = null)
        {
            return AddShortcuts(items, keySelector, descriptionSelector, null, keyStyleSelector, descriptionStyleSelector);
        }

        /// <summary>
        /// Adds shortcuts projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into shortcuts.</param>
        /// <param name="keySelector">Selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Selects the shortcut action description.</param>
        /// <param name="disabledSelector">Optionally selects whether the shortcut should render as disabled.</param>
        /// <param name="keyColorSelector">Optionally selects the key token foreground color.</param>
        /// <param name="descriptionColorSelector">Optionally selects the action description foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcuts<T>(
            IEnumerable<T> items,
            Func<T, string> keySelector,
            Func<T, string> descriptionSelector,
            Func<T, bool> disabledSelector = null,
            Func<T, ConsoleColor?> keyColorSelector = null,
            Func<T, ConsoleColor?> descriptionColorSelector = null)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (descriptionSelector == null)
            {
                throw new ArgumentNullException(nameof(descriptionSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddShortcut(
                    keySelector(item),
                    descriptionSelector(item),
                    disabledSelector != null && disabledSelector(item),
                    keyColorSelector != null ? keyColorSelector(item) : null,
                    descriptionColorSelector != null ? descriptionColorSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds shortcuts projected from a model collection with Spectre-style key and description colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into shortcuts.</param>
        /// <param name="keySelector">Selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Selects the shortcut action description.</param>
        /// <param name="disabledSelector">Optionally selects whether the shortcut should render as disabled.</param>
        /// <param name="keyColorSelector">Selects the key token foreground color.</param>
        /// <param name="descriptionColorSelector">Optionally selects the action description foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcuts<T>(
            IEnumerable<T> items,
            Func<T, string> keySelector,
            Func<T, string> descriptionSelector,
            Func<T, bool> disabledSelector,
            Func<T, SpectreColor> keyColorSelector,
            Func<T, SpectreColor> descriptionColorSelector = null)
        {
            if (keyColorSelector == null)
            {
                throw new ArgumentNullException(nameof(keyColorSelector));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (descriptionSelector == null)
            {
                throw new ArgumentNullException(nameof(descriptionSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddShortcut(ShortcutItem.Create(
                    keySelector(item),
                    descriptionSelector(item),
                    keyColorSelector(item),
                    descriptionColorSelector != null ? descriptionColorSelector(item) : (SpectreColor?)null,
                    disabledSelector != null && disabledSelector(item)));
            }

            return this;
        }

        /// <summary>
        /// Adds shortcuts projected from a model collection with Spectre-style key and description colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into shortcuts.</param>
        /// <param name="keySelector">Selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Selects the shortcut action description.</param>
        /// <param name="keyColorSelector">Selects the key token foreground color.</param>
        /// <param name="descriptionColorSelector">Optionally selects the action description foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcuts<T>(
            IEnumerable<T> items,
            Func<T, string> keySelector,
            Func<T, string> descriptionSelector,
            Func<T, SpectreColor> keyColorSelector,
            Func<T, SpectreColor> descriptionColorSelector = null)
        {
            return AddShortcuts(
                items,
                keySelector,
                descriptionSelector,
                null,
                keyColorSelector,
                descriptionColorSelector);
        }

        /// <summary>
        /// Adds shortcuts projected from a model collection with RGB key and description colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into shortcuts.</param>
        /// <param name="keySelector">Selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Selects the shortcut action description.</param>
        /// <param name="disabledSelector">Optionally selects whether the shortcut should render as disabled.</param>
        /// <param name="keyColorSelector">Selects the key token foreground color.</param>
        /// <param name="descriptionColorSelector">Optionally selects the action description foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcuts<T>(
            IEnumerable<T> items,
            Func<T, string> keySelector,
            Func<T, string> descriptionSelector,
            Func<T, bool> disabledSelector,
            Func<T, RgbColor> keyColorSelector,
            Func<T, RgbColor> descriptionColorSelector = null)
        {
            if (keyColorSelector == null)
            {
                throw new ArgumentNullException(nameof(keyColorSelector));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (descriptionSelector == null)
            {
                throw new ArgumentNullException(nameof(descriptionSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddShortcut(ShortcutItem.Create(
                    keySelector(item),
                    descriptionSelector(item),
                    keyColorSelector(item),
                    descriptionColorSelector != null ? descriptionColorSelector(item) : (RgbColor?)null,
                    disabledSelector != null && disabledSelector(item)));
            }

            return this;
        }

        /// <summary>
        /// Adds shortcuts projected from a model collection with RGB key and description colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into shortcuts.</param>
        /// <param name="keySelector">Selects the shortcut key or command token.</param>
        /// <param name="descriptionSelector">Selects the shortcut action description.</param>
        /// <param name="keyColorSelector">Selects the key token foreground color.</param>
        /// <param name="descriptionColorSelector">Optionally selects the action description foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcuts<T>(
            IEnumerable<T> items,
            Func<T, string> keySelector,
            Func<T, string> descriptionSelector,
            Func<T, RgbColor> keyColorSelector,
            Func<T, RgbColor> descriptionColorSelector = null)
        {
            return AddShortcuts(
                items,
                keySelector,
                descriptionSelector,
                null,
                keyColorSelector,
                descriptionColorSelector);
        }

        /// <summary>
        /// Adds shortcuts projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into shortcuts.</param>
        /// <param name="itemSelector">Selects the shortcut item to add for each model item.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddShortcuts<T>(IEnumerable<T> items, Func<T, ShortcutItem> itemSelector)
        {
            if (itemSelector == null)
            {
                throw new ArgumentNullException(nameof(itemSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddShortcut(itemSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds a disabled shortcut item.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar AddDisabled(string key, string description)
        {
            return AddShortcut(key, description, disabled: true);
        }

        /// <summary>
        /// Sets the requested shortcut bar width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the shortcut bar to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar Expand()
        {
            Options.Width = null;
            return this;
        }

        /// <summary>
        /// Sets the left and right edge text rendered around key tokens.
        /// </summary>
        /// <param name="left">The left edge text.</param>
        /// <param name="right">The right edge text.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar KeyEdges(string left, string right)
        {
            Options.KeyLeftEdge = left ?? string.Empty;
            Options.KeyRightEdge = right ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the text written between key tokens and descriptions.
        /// </summary>
        /// <param name="separator">The separator text.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar KeyDescriptionSeparator(string separator)
        {
            Options.KeyDescriptionSeparator = separator ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the separator written between shortcut items on the same line.
        /// </summary>
        /// <param name="separator">The separator text.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar ItemSeparator(string separator)
        {
            Options.ItemSeparator = separator ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the text used when a shortcut item is truncated.
        /// </summary>
        /// <param name="ellipsis">The truncation marker.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar Ellipsis(string ellipsis)
        {
            Options.Ellipsis = ellipsis ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Shows or hides disabled shortcut items.
        /// </summary>
        /// <param name="show">Whether disabled shortcut items should render.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar ShowDisabled(bool show = true)
        {
            Options.ShowDisabled = show;
            return this;
        }

        /// <summary>
        /// Hides disabled shortcut items.
        /// </summary>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar HideDisabled()
        {
            Options.ShowDisabled = false;
            return this;
        }

        /// <summary>
        /// Sets the default key token foreground color.
        /// </summary>
        /// <param name="color">The key token foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar KeyColor(ConsoleColor color)
        {
            Options.KeyScheme = Scheme(color);
            Options.KeyDecoration = Decoration.None;
            Options.KeyForegroundRgb = null;
            Options.KeyBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default key token foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The key token foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar KeyColor(SpectreColor color)
        {
            Options.KeyScheme = Scheme(color);
            Options.KeyDecoration = Decoration.None;
            Options.KeyForegroundRgb = color.ToRgbColor();
            Options.KeyBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default key token foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The key token foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar KeyColor(RgbColor color)
        {
            Options.KeyScheme = Scheme(color);
            Options.KeyDecoration = Decoration.None;
            Options.KeyForegroundRgb = color;
            Options.KeyBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default key token color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar KeyColor(Style style)
        {
            ApplyKeyStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the default key token color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar KeyColor(string style)
        {
            ApplyKeyStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the default description foreground color.
        /// </summary>
        /// <param name="color">The description foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar DescriptionColor(ConsoleColor color)
        {
            Options.DescriptionScheme = Scheme(color);
            Options.DescriptionDecoration = Decoration.None;
            Options.DescriptionForegroundRgb = null;
            Options.DescriptionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default description foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The description foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar DescriptionColor(SpectreColor color)
        {
            Options.DescriptionScheme = Scheme(color);
            Options.DescriptionDecoration = Decoration.None;
            Options.DescriptionForegroundRgb = color.ToRgbColor();
            Options.DescriptionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default description foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The description foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar DescriptionColor(RgbColor color)
        {
            Options.DescriptionScheme = Scheme(color);
            Options.DescriptionDecoration = Decoration.None;
            Options.DescriptionForegroundRgb = color;
            Options.DescriptionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default description color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar DescriptionColor(Style style)
        {
            ApplyDescriptionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the default description color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar DescriptionColor(string style)
        {
            ApplyDescriptionStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the item separator foreground color.
        /// </summary>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar SeparatorColor(ConsoleColor color)
        {
            Options.SeparatorScheme = Scheme(color);
            Options.SeparatorDecoration = Decoration.None;
            Options.SeparatorForegroundRgb = null;
            Options.SeparatorBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the item separator foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar SeparatorColor(SpectreColor color)
        {
            Options.SeparatorScheme = Scheme(color);
            Options.SeparatorDecoration = Decoration.None;
            Options.SeparatorForegroundRgb = color.ToRgbColor();
            Options.SeparatorBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the item separator foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar SeparatorColor(RgbColor color)
        {
            Options.SeparatorScheme = Scheme(color);
            Options.SeparatorDecoration = Decoration.None;
            Options.SeparatorForegroundRgb = color;
            Options.SeparatorBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the item separator color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar SeparatorColor(Style style)
        {
            ApplySeparatorStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the item separator color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar SeparatorColor(string style)
        {
            ApplySeparatorStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the disabled shortcut foreground color.
        /// </summary>
        /// <param name="color">The disabled foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar DisabledColor(ConsoleColor color)
        {
            Options.DisabledScheme = Scheme(color);
            Options.DisabledDecoration = Decoration.None;
            Options.DisabledForegroundRgb = null;
            Options.DisabledBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the disabled shortcut foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The disabled foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar DisabledColor(SpectreColor color)
        {
            Options.DisabledScheme = Scheme(color);
            Options.DisabledDecoration = Decoration.None;
            Options.DisabledForegroundRgb = color.ToRgbColor();
            Options.DisabledBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the disabled shortcut foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The disabled foreground color.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar DisabledColor(RgbColor color)
        {
            Options.DisabledScheme = Scheme(color);
            Options.DisabledDecoration = Decoration.None;
            Options.DisabledForegroundRgb = color;
            Options.DisabledBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the disabled shortcut color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar DisabledColor(Style style)
        {
            ApplyDisabledStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the disabled shortcut color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#f59e0b</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ShortcutBar"/> instance.</returns>
        public ShortcutBar DisabledColor(string style)
        {
            ApplyDisabledStyle(Style.Parse(style));
            return this;
        }

        private void ApplyKeyStyle(Style style)
        {
            Options.KeyScheme = Scheme(style, Options.ResolveKeyScheme());
            Options.KeyDecoration = style?.Decoration ?? Decoration.None;
            Options.KeyForegroundRgb = style?.ForegroundRgb;
            Options.KeyBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyDescriptionStyle(Style style)
        {
            Options.DescriptionScheme = Scheme(style, Options.ResolveDescriptionScheme());
            Options.DescriptionDecoration = style?.Decoration ?? Decoration.None;
            Options.DescriptionForegroundRgb = style?.ForegroundRgb;
            Options.DescriptionBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplySeparatorStyle(Style style)
        {
            Options.SeparatorScheme = Scheme(style, Options.ResolveSeparatorScheme());
            Options.SeparatorDecoration = style?.Decoration ?? Decoration.None;
            Options.SeparatorForegroundRgb = style?.ForegroundRgb;
            Options.SeparatorBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyDisabledStyle(Style style)
        {
            Options.DisabledScheme = Scheme(style, Options.ResolveDisabledScheme());
            Options.DisabledDecoration = style?.Decoration ?? Decoration.None;
            Options.DisabledForegroundRgb = style?.ForegroundRgb;
            Options.DisabledBackgroundRgb = style?.BackgroundRgb;
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
