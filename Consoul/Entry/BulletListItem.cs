using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one item rendered by a bullet list.
    /// </summary>
    public class BulletListItem
    {
        private readonly List<BulletListItem> _children = new List<BulletListItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BulletListItem"/> class.
        /// </summary>
        public BulletListItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulletListItem"/> class.
        /// </summary>
        /// <param name="text">The item text.</param>
        public BulletListItem(string text)
        {
            Text = text ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the item text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets the child items rendered below this item.
        /// </summary>
        public IList<BulletListItem> Children => _children;

        /// <summary>
        /// Gets or sets an optional color scheme for the item marker.
        /// </summary>
        public ColorScheme? MarkerScheme { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme for the item text.
        /// </summary>
        public ColorScheme? TextScheme { get; set; }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="text">The child item text.</param>
        /// <param name="textColor">Optional foreground color used for child text.</param>
        /// <param name="markerColor">Optional foreground color used for the child marker.</param>
        /// <returns>The added child item.</returns>
        public BulletListItem Add(string text, ConsoleColor? textColor = null, ConsoleColor? markerColor = null)
        {
            BulletListItem item = Create(text, textColor, markerColor);
            _children.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a child item using reusable style objects.
        /// </summary>
        /// <param name="text">The child item text.</param>
        /// <param name="textStyle">Optional style used for child text.</param>
        /// <param name="markerStyle">Optional style used for the child marker.</param>
        /// <returns>The added child item.</returns>
        public BulletListItem Add(string text, Style textStyle, Style markerStyle = null)
        {
            BulletListItem item = Create(text, textStyle, markerStyle);
            _children.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a child item using Spectre-style style text.
        /// </summary>
        /// <param name="text">The child item text.</param>
        /// <param name="textStyle">Optional style used for child text, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the child marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The added child item.</returns>
        public BulletListItem Add(string text, string textStyle, string markerStyle = null)
        {
            return AddStyled(text, textStyle, markerStyle);
        }

        /// <summary>
        /// Adds a child item using Spectre-style style text.
        /// </summary>
        /// <param name="text">The child item text.</param>
        /// <param name="textStyle">Optional style used for child text, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the child marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The added child item.</returns>
        public BulletListItem AddStyled(string text, string textStyle = null, string markerStyle = null)
        {
            BulletListItem item = Create(text, textStyle, markerStyle);
            _children.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="text">The child item text.</param>
        /// <param name="textColor">Optional foreground color used for child text.</param>
        /// <param name="markerColor">Optional foreground color used for the child marker.</param>
        /// <returns>The added child item.</returns>
        public BulletListItem AddItem(string text, ConsoleColor? textColor = null, ConsoleColor? markerColor = null)
        {
            return Add(text, textColor, markerColor);
        }

        /// <summary>
        /// Adds a child item using reusable style objects.
        /// </summary>
        /// <param name="text">The child item text.</param>
        /// <param name="textStyle">Optional style used for child text.</param>
        /// <param name="markerStyle">Optional style used for the child marker.</param>
        /// <returns>The added child item.</returns>
        public BulletListItem AddItem(string text, Style textStyle, Style markerStyle = null)
        {
            return Add(text, textStyle, markerStyle);
        }

        /// <summary>
        /// Adds a child item using Spectre-style style text.
        /// </summary>
        /// <param name="text">The child item text.</param>
        /// <param name="textStyle">Optional style used for child text, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the child marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The added child item.</returns>
        public BulletListItem AddItem(string text, string textStyle, string markerStyle = null)
        {
            return Add(text, textStyle, markerStyle);
        }

        /// <summary>
        /// Adds a child item using Spectre-style style text.
        /// </summary>
        /// <param name="text">The child item text.</param>
        /// <param name="textStyle">Optional style used for child text, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the child marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The added child item.</returns>
        public BulletListItem AddItemStyled(string text, string textStyle = null, string markerStyle = null)
        {
            return AddStyled(text, textStyle, markerStyle);
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="item">The child item to add.</param>
        /// <returns>The added child item.</returns>
        public BulletListItem Add(BulletListItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _children.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="item">The child item to add.</param>
        /// <returns>The added child item.</returns>
        public BulletListItem AddItem(BulletListItem item)
        {
            return Add(item);
        }

        /// <summary>
        /// Adds multiple child item texts.
        /// </summary>
        /// <param name="firstItem">The first child item text to add.</param>
        /// <param name="additionalItems">Additional child item texts to add.</param>
        /// <returns>The current <see cref="BulletListItem"/> instance.</returns>
        public BulletListItem AddItems(string firstItem, params string[] additionalItems)
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
        /// Adds multiple child item texts.
        /// </summary>
        /// <param name="items">The child item texts to add.</param>
        /// <returns>The current <see cref="BulletListItem"/> instance.</returns>
        public BulletListItem AddItems(IEnumerable<string> items)
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
        /// Adds child items projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child bullet list items.</param>
        /// <param name="textSelector">Selects the child item text.</param>
        /// <param name="textColorSelector">Optionally selects the child item text foreground color.</param>
        /// <param name="markerColorSelector">Optionally selects the child item marker foreground color.</param>
        /// <returns>The current <see cref="BulletListItem"/> instance.</returns>
        public BulletListItem AddItems<T>(
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
        /// Adds child items projected from a model collection with Spectre-style per-item colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child bullet list items.</param>
        /// <param name="textSelector">Selects the child item text.</param>
        /// <param name="textColorSelector">Selects the child item text foreground color.</param>
        /// <param name="markerColorSelector">Optionally selects the child item marker foreground color.</param>
        /// <returns>The current <see cref="BulletListItem"/> instance.</returns>
        public BulletListItem AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> textSelector,
            Func<T, SpectreColor> textColorSelector,
            Func<T, SpectreColor> markerColorSelector = null)
        {
            if (textColorSelector == null)
            {
                throw new ArgumentNullException(nameof(textColorSelector));
            }

            return AddItems(
                items,
                textSelector,
                ToConsoleColorSelector(textColorSelector),
                ToConsoleColorSelector(markerColorSelector));
        }

        /// <summary>
        /// Adds child items projected from a model collection with RGB per-item colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child bullet list items.</param>
        /// <param name="textSelector">Selects the child item text.</param>
        /// <param name="textColorSelector">Selects the child item text foreground color.</param>
        /// <param name="markerColorSelector">Optionally selects the child item marker foreground color.</param>
        /// <returns>The current <see cref="BulletListItem"/> instance.</returns>
        public BulletListItem AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> textSelector,
            Func<T, RgbColor> textColorSelector,
            Func<T, RgbColor> markerColorSelector = null)
        {
            if (textColorSelector == null)
            {
                throw new ArgumentNullException(nameof(textColorSelector));
            }

            return AddItems(
                items,
                textSelector,
                ToConsoleColorSelector(textColorSelector),
                ToConsoleColorSelector(markerColorSelector));
        }

        /// <summary>
        /// Adds child items projected from a model collection with reusable per-item styles.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child bullet list items.</param>
        /// <param name="textSelector">Selects the child item text.</param>
        /// <param name="textStyleSelector">Selects the child item text style.</param>
        /// <param name="markerStyleSelector">Optionally selects the child item marker style.</param>
        /// <returns>The current <see cref="BulletListItem"/> instance.</returns>
        public BulletListItem AddItems<T>(
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
        /// Adds child items projected from a model collection with Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child bullet list items.</param>
        /// <param name="textSelector">Selects the child item text.</param>
        /// <param name="textStyleSelector">Selects style text for the child item text.</param>
        /// <param name="markerStyleSelector">Optionally selects style text for the child item marker.</param>
        /// <returns>The current <see cref="BulletListItem"/> instance.</returns>
        public BulletListItem AddItems<T>(
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
        /// Adds child bullet list items projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child bullet list items.</param>
        /// <param name="itemSelector">Selects the child bullet list item to add for each model item.</param>
        /// <returns>The current <see cref="BulletListItem"/> instance.</returns>
        public BulletListItem AddItems<T>(IEnumerable<T> items, Func<T, BulletListItem> itemSelector)
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
        /// Adds multiple existing child items.
        /// </summary>
        /// <param name="items">The child items to add.</param>
        /// <returns>The current <see cref="BulletListItem"/> instance.</returns>
        public BulletListItem AddItems(IEnumerable<BulletListItem> items)
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
        /// Adds multiple existing child items.
        /// </summary>
        /// <param name="firstItem">The first child item to add.</param>
        /// <param name="additionalItems">Additional child items to add.</param>
        /// <returns>The current <see cref="BulletListItem"/> instance.</returns>
        public BulletListItem AddItems(BulletListItem firstItem, params BulletListItem[] additionalItems)
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

        private static Func<T, ConsoleColor?> ToConsoleColorSelector<T>(Func<T, SpectreColor> selector)
        {
            return selector == null
                ? (Func<T, ConsoleColor?>)null
                : item => selector(item).ToConsoleColor();
        }

        private static Func<T, ConsoleColor?> ToConsoleColorSelector<T>(Func<T, RgbColor> selector)
        {
            return selector == null
                ? (Func<T, ConsoleColor?>)null
                : item => selector(item).ToConsoleColor();
        }

        /// <summary>
        /// Creates a bullet list item.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="textColor">Optional foreground color used for item text.</param>
        /// <param name="markerColor">Optional foreground color used for the item marker.</param>
        /// <returns>A configured <see cref="BulletListItem"/> instance.</returns>
        public static BulletListItem Create(string text, ConsoleColor? textColor = null, ConsoleColor? markerColor = null)
        {
            return new BulletListItem(text)
            {
                TextScheme = textColor.HasValue ? Scheme(textColor.Value) : (ColorScheme?)null,
                MarkerScheme = markerColor.HasValue ? Scheme(markerColor.Value) : (ColorScheme?)null
            };
        }

        /// <summary>
        /// Creates a bullet list item using reusable style objects.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="textStyle">Optional style used for item text.</param>
        /// <param name="markerStyle">Optional style used for the item marker.</param>
        /// <returns>A configured <see cref="BulletListItem"/> instance.</returns>
        public static BulletListItem Create(string text, Style textStyle, Style markerStyle = null)
        {
            return new BulletListItem(text)
            {
                TextScheme = Scheme(textStyle),
                MarkerScheme = Scheme(markerStyle)
            };
        }

        /// <summary>
        /// Creates a bullet list item using Spectre-style style text.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="textStyle">Optional style used for item text, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the item marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>A configured <see cref="BulletListItem"/> instance.</returns>
        public static BulletListItem Create(string text, string textStyle, string markerStyle = null)
        {
            return new BulletListItem(text)
            {
                TextScheme = string.IsNullOrWhiteSpace(textStyle) ? (ColorScheme?)null : Style.Parse(textStyle).ToColorScheme(RenderOptions.DefaultScheme),
                MarkerScheme = string.IsNullOrWhiteSpace(markerStyle) ? (ColorScheme?)null : Style.Parse(markerStyle).ToColorScheme(RenderOptions.DefaultScheme)
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
