using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent bullet list construction over <see cref="BulletListEntry"/>.
    /// </summary>
    public class BulletList : BulletListEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BulletList"/> class.
        /// </summary>
        public BulletList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulletList"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the bullet list.</param>
        public BulletList(BulletListOptions options) : base(options)
        {
        }

        /// <summary>
        /// Adds an item to the list.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="textColor">Optional foreground color used for item text.</param>
        /// <param name="markerColor">Optional foreground color used for the item marker.</param>
        /// <returns>The added list item.</returns>
        public BulletListItem AddItem(string text, ConsoleColor? textColor = null, ConsoleColor? markerColor = null)
        {
            return Add(text, textColor, markerColor);
        }

        /// <summary>
        /// Adds an item to the list using reusable style objects.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="textStyle">Optional style used for item text.</param>
        /// <param name="markerStyle">Optional style used for the item marker.</param>
        /// <returns>The added list item.</returns>
        public BulletListItem AddItem(string text, Style textStyle, Style markerStyle = null)
        {
            return Add(text, textStyle, markerStyle);
        }

        /// <summary>
        /// Adds an item to the list using Spectre-style style text.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="textStyle">Optional style used for item text, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the item marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The added list item.</returns>
        public BulletListItem AddItem(string text, string textStyle, string markerStyle = null)
        {
            return Add(text, textStyle, markerStyle);
        }

        /// <summary>
        /// Adds an item to the list using Spectre-style style text.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="textStyle">Optional style used for item text, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the item marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The added list item.</returns>
        public BulletListItem AddItemStyled(string text, string textStyle = null, string markerStyle = null)
        {
            return AddStyled(text, textStyle, markerStyle);
        }

        /// <summary>
        /// Adds an item to the list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The added list item.</returns>
        public BulletListItem AddItem(BulletListItem item)
        {
            return Add(item);
        }

        /// <summary>
        /// Adds multiple item texts to the list.
        /// </summary>
        /// <param name="firstItem">The first item text to add.</param>
        /// <param name="additionalItems">Additional item texts to add.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList AddItems(string firstItem, params string[] additionalItems)
        {
            AddItem(firstItem);

            if (additionalItems != null)
            {
                foreach (string item in additionalItems)
                {
                    AddItem(item);
                }
            }

            return this;
        }

        /// <summary>
        /// Adds multiple item texts to the list.
        /// </summary>
        /// <param name="items">The item texts.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList AddItems(IEnumerable<string> items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (string item in items)
            {
                AddItem(item);
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into bullet list items.</param>
        /// <param name="textSelector">Selects the item text.</param>
        /// <param name="textColorSelector">Optionally selects the item text foreground color.</param>
        /// <param name="markerColorSelector">Optionally selects the item marker foreground color.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> textSelector,
            Func<T, ConsoleColor?> textColorSelector = null,
            Func<T, ConsoleColor?> markerColorSelector = null)
        {
            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(
                    textSelector(item),
                    textColorSelector != null ? textColorSelector(item) : null,
                    markerColorSelector != null ? markerColorSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection with Spectre-style per-item colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into bullet list items.</param>
        /// <param name="textSelector">Selects the item text.</param>
        /// <param name="textColorSelector">Selects the item text foreground color.</param>
        /// <param name="markerColorSelector">Optionally selects the item marker foreground color.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> textSelector,
            Func<T, SpectreColor> textColorSelector,
            Func<T, SpectreColor> markerColorSelector = null)
        {
            if (textColorSelector == null)
            {
                throw new ArgumentNullException(nameof(textColorSelector));
            }

            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(BulletListItem.Create(
                    textSelector(item),
                    textColorSelector(item),
                    markerColorSelector != null ? markerColorSelector(item) : (SpectreColor?)null));
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection with RGB per-item colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into bullet list items.</param>
        /// <param name="textSelector">Selects the item text.</param>
        /// <param name="textColorSelector">Selects the item text foreground color.</param>
        /// <param name="markerColorSelector">Optionally selects the item marker foreground color.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> textSelector,
            Func<T, RgbColor> textColorSelector,
            Func<T, RgbColor> markerColorSelector = null)
        {
            if (textColorSelector == null)
            {
                throw new ArgumentNullException(nameof(textColorSelector));
            }

            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(BulletListItem.Create(
                    textSelector(item),
                    textColorSelector(item),
                    markerColorSelector != null ? markerColorSelector(item) : (RgbColor?)null));
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection with reusable per-item styles.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into bullet list items.</param>
        /// <param name="textSelector">Selects the item text.</param>
        /// <param name="textStyleSelector">Selects the item text style.</param>
        /// <param name="markerStyleSelector">Optionally selects the item marker style.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> textSelector,
            Func<T, Style> textStyleSelector,
            Func<T, Style> markerStyleSelector = null)
        {
            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            if (textStyleSelector == null)
            {
                throw new ArgumentNullException(nameof(textStyleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(
                    textSelector(item),
                    textStyleSelector(item),
                    markerStyleSelector != null ? markerStyleSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection with Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into bullet list items.</param>
        /// <param name="textSelector">Selects the item text.</param>
        /// <param name="textStyleSelector">Selects style text for the item text.</param>
        /// <param name="markerStyleSelector">Optionally selects style text for the item marker.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> textSelector,
            Func<T, string> textStyleSelector,
            Func<T, string> markerStyleSelector = null)
        {
            if (textSelector == null)
            {
                throw new ArgumentNullException(nameof(textSelector));
            }

            if (textStyleSelector == null)
            {
                throw new ArgumentNullException(nameof(textStyleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(
                    textSelector(item),
                    textStyleSelector(item),
                    markerStyleSelector != null ? markerStyleSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds bullet list items projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into bullet list items.</param>
        /// <param name="itemSelector">Selects the bullet list item to add for each model item.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList AddItems<T>(IEnumerable<T> items, Func<T, BulletListItem> itemSelector)
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
                AddItem(itemSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds multiple existing bullet list items.
        /// </summary>
        /// <param name="items">The list items to add.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList AddItems(IEnumerable<BulletListItem> items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (BulletListItem item in items)
            {
                AddItem(item);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple existing bullet list items.
        /// </summary>
        /// <param name="firstItem">The first item to add.</param>
        /// <param name="additionalItems">Additional items to add.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList AddItems(BulletListItem firstItem, params BulletListItem[] additionalItems)
        {
            AddItem(firstItem);

            if (additionalItems != null)
            {
                foreach (BulletListItem item in additionalItems)
                {
                    AddItem(item);
                }
            }

            return this;
        }

        /// <summary>
        /// Sets the requested render width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Uses bullet markers for list items.
        /// </summary>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList Bullet()
        {
            Options.Style = BulletListStyle.Bullet;
            return this;
        }

        /// <summary>
        /// Uses numbered markers for list items.
        /// </summary>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList Numbered()
        {
            Options.Style = BulletListStyle.Numbered;
            return this;
        }

        /// <summary>
        /// Sets the nesting indentation size.
        /// </summary>
        /// <param name="size">The indentation size.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList IndentSize(int size)
        {
            Options.IndentSize = size;
            return this;
        }

        /// <summary>
        /// Sets the number of spaces between marker and item text.
        /// </summary>
        /// <param name="gap">The marker gap.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList MarkerGap(int gap)
        {
            Options.MarkerGap = gap;
            return this;
        }

        /// <summary>
        /// Sets reusable bullet markers by nesting level.
        /// </summary>
        /// <param name="markers">The bullet markers.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList Markers(params string[] markers)
        {
            Options.BulletMarkers = markers;
            return this;
        }

        /// <summary>
        /// Sets the numbered marker format.
        /// </summary>
        /// <param name="format">The numbered marker format. The item number is argument zero.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList NumberFormat(string format)
        {
            Options.NumberFormat = format;
            return this;
        }

        /// <summary>
        /// Sets the marker foreground color.
        /// </summary>
        /// <param name="color">The marker foreground color.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList MarkerColor(ConsoleColor color)
        {
            Options.MarkerScheme = Scheme(color);
            Options.MarkerDecoration = Decoration.None;
            Options.MarkerForegroundRgb = null;
            Options.MarkerBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the marker foreground color.
        /// </summary>
        /// <param name="color">The marker foreground color.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList MarkerColor(SpectreColor color)
        {
            Options.MarkerScheme = Scheme(color);
            Options.MarkerDecoration = Decoration.None;
            Options.MarkerForegroundRgb = color.ToRgbColor();
            Options.MarkerBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the marker foreground color.
        /// </summary>
        /// <param name="color">The marker foreground color.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList MarkerColor(RgbColor color)
        {
            Options.MarkerScheme = Scheme(color);
            Options.MarkerDecoration = Decoration.None;
            Options.MarkerForegroundRgb = color;
            Options.MarkerBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList MarkerColor(Style style)
        {
            ApplyMarkerStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList MarkerColor(string style)
        {
            ApplyMarkerStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the item text foreground color.
        /// </summary>
        /// <param name="color">The item text foreground color.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList TextColor(ConsoleColor color)
        {
            Options.TextScheme = Scheme(color);
            Options.TextDecoration = Decoration.None;
            Options.TextForegroundRgb = null;
            Options.TextBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the item text foreground color.
        /// </summary>
        /// <param name="color">The item text foreground color.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList TextColor(SpectreColor color)
        {
            Options.TextScheme = Scheme(color);
            Options.TextDecoration = Decoration.None;
            Options.TextForegroundRgb = color.ToRgbColor();
            Options.TextBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the item text foreground color.
        /// </summary>
        /// <param name="color">The item text foreground color.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList TextColor(RgbColor color)
        {
            Options.TextScheme = Scheme(color);
            Options.TextDecoration = Decoration.None;
            Options.TextForegroundRgb = color;
            Options.TextBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the item text color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList TextColor(Style style)
        {
            ApplyTextStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the item text color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BulletList"/> instance.</returns>
        public BulletList TextColor(string style)
        {
            ApplyTextStyle(Style.Parse(style));
            return this;
        }

        private void ApplyMarkerStyle(Style style)
        {
            Options.MarkerScheme = Scheme(style, Options.ResolveMarkerScheme());
            Options.MarkerDecoration = style?.Decoration ?? Decoration.None;
            Options.MarkerForegroundRgb = style?.ForegroundRgb;
            Options.MarkerBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyTextStyle(Style style)
        {
            Options.TextScheme = Scheme(style, Options.ResolveTextScheme());
            Options.TextDecoration = style?.Decoration ?? Decoration.None;
            Options.TextForegroundRgb = style?.ForegroundRgb;
            Options.TextBackgroundRgb = style?.BackgroundRgb;
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
