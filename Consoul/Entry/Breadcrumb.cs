using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent breadcrumb construction over <see cref="BreadcrumbEntry"/>.
    /// </summary>
    public class Breadcrumb : BreadcrumbEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Breadcrumb"/> class.
        /// </summary>
        public Breadcrumb()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Breadcrumb"/> class with items.
        /// </summary>
        /// <param name="items">The breadcrumb items to render.</param>
        public Breadcrumb(params string[] items) : base(items)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Breadcrumb"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the breadcrumb.</param>
        public Breadcrumb(BreadcrumbOptions options) : base(options)
        {
        }

        /// <summary>
        /// Adds an item to the breadcrumb trail.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb AddItem(string item)
        {
            Add(item);
            return this;
        }

        /// <summary>
        /// Adds an item to the breadcrumb trail using a foreground color.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <param name="color">The foreground color used for the item.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb AddItem(string item, ConsoleColor color)
        {
            Add(item, color);
            return this;
        }

        /// <summary>
        /// Adds an item to the breadcrumb trail using a reusable style object.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <param name="style">Optional style used for the item.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb AddItem(string item, Style style)
        {
            Add(item, style);
            return this;
        }

        /// <summary>
        /// Adds an item to the breadcrumb trail using Spectre-style style text.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <param name="style">Optional style used for the item, such as <c>grey</c>, <c>aqua</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb AddItem(string item, string style)
        {
            Add(item, style);
            return this;
        }

        /// <summary>
        /// Adds an item to the breadcrumb trail.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public new Breadcrumb Add(string item)
        {
            base.Add(item);
            return this;
        }

        /// <summary>
        /// Adds an item to the breadcrumb trail using a foreground color.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <param name="color">The foreground color used for the item.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public new Breadcrumb Add(string item, ConsoleColor color)
        {
            base.Add(item, color);
            return this;
        }

        /// <summary>
        /// Adds an item to the breadcrumb trail using a reusable style object.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <param name="style">Optional style used for the item.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public new Breadcrumb Add(string item, Style style)
        {
            base.Add(item, style);
            return this;
        }

        /// <summary>
        /// Adds an item to the breadcrumb trail using Spectre-style style text.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <param name="style">Optional style used for the item, such as <c>grey</c>, <c>aqua</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public new Breadcrumb Add(string item, string style)
        {
            base.Add(item, style);
            return this;
        }

        /// <summary>
        /// Adds multiple item texts to the breadcrumb trail.
        /// </summary>
        /// <param name="firstItem">The first item text to add.</param>
        /// <param name="additionalItems">Additional item texts to add.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb AddItems(string firstItem, params string[] additionalItems)
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
        /// Adds items to the breadcrumb trail.
        /// </summary>
        /// <param name="items">The item texts.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb AddItems(IEnumerable<string> items)
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
        /// Adds items projected from a model collection to the breadcrumb trail.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into breadcrumb items.</param>
        /// <param name="itemSelector">Selects the item text rendered for each breadcrumb segment.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public new Breadcrumb AddItems<T>(IEnumerable<T> items, Func<T, string> itemSelector)
        {
            base.AddItems(items, itemSelector);
            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection to the breadcrumb trail using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into breadcrumb items.</param>
        /// <param name="itemSelector">Selects the item text rendered for each breadcrumb segment.</param>
        /// <param name="styleSelector">Selects style text for each breadcrumb segment.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public new Breadcrumb AddItems<T>(IEnumerable<T> items, Func<T, string> itemSelector, Func<T, string> styleSelector)
        {
            base.AddItems(items, itemSelector, styleSelector);
            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection to the breadcrumb trail using reusable style objects.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into breadcrumb items.</param>
        /// <param name="itemSelector">Selects the item text rendered for each breadcrumb segment.</param>
        /// <param name="styleSelector">Selects a reusable style for each breadcrumb segment.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public new Breadcrumb AddItems<T>(IEnumerable<T> items, Func<T, string> itemSelector, Func<T, Style> styleSelector)
        {
            base.AddItems(items, itemSelector, styleSelector);
            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection to the breadcrumb trail using Spectre-style colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into breadcrumb items.</param>
        /// <param name="itemSelector">Selects the item text rendered for each breadcrumb segment.</param>
        /// <param name="colorSelector">Selects a Spectre-style color for each breadcrumb segment.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public new Breadcrumb AddItems<T>(IEnumerable<T> items, Func<T, string> itemSelector, Func<T, SpectreColor> colorSelector)
        {
            base.AddItems(items, itemSelector, colorSelector);
            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection to the breadcrumb trail using RGB colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into breadcrumb items.</param>
        /// <param name="itemSelector">Selects the item text rendered for each breadcrumb segment.</param>
        /// <param name="colorSelector">Selects an RGB color for each breadcrumb segment.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public new Breadcrumb AddItems<T>(IEnumerable<T> items, Func<T, string> itemSelector, Func<T, RgbColor> colorSelector)
        {
            base.AddItems(items, itemSelector, colorSelector);
            return this;
        }

        /// <summary>
        /// Sets the requested render width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Sets the separator written between breadcrumb items.
        /// </summary>
        /// <param name="separator">The separator text.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb Separator(string separator)
        {
            Options.Separator = separator ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the text used when middle items are elided.
        /// </summary>
        /// <param name="ellipsis">The ellipsis text.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb Ellipsis(string ellipsis)
        {
            Options.Ellipsis = ellipsis ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Shows or hides current-item highlighting.
        /// </summary>
        /// <param name="highlight">Whether the current item should be highlighted.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb HighlightCurrent(bool highlight = true)
        {
            Options.HighlightCurrent = highlight;
            return this;
        }

        /// <summary>
        /// Sets the non-current item foreground color.
        /// </summary>
        /// <param name="color">The item foreground color.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb ItemColor(ConsoleColor color)
        {
            Options.ItemScheme = Scheme(color);
            Options.ItemDecoration = Decoration.None;
            Options.ItemForegroundRgb = null;
            Options.ItemBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the non-current item foreground color.
        /// </summary>
        /// <param name="color">The item foreground color.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb ItemColor(SpectreColor color)
        {
            return ItemColor(new Style(color));
        }

        /// <summary>
        /// Sets the non-current item foreground color.
        /// </summary>
        /// <param name="color">The item foreground color.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb ItemColor(RgbColor color)
        {
            return ItemColor(new Style(color));
        }

        /// <summary>
        /// Sets the non-current item color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb ItemColor(Style style)
        {
            ApplyItemStyle(style, Options.ResolveItemScheme());
            return this;
        }

        /// <summary>
        /// Sets the non-current item color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb ItemColor(string style)
        {
            return ItemColor(Style.Parse(style));
        }

        /// <summary>
        /// Sets the current item foreground color.
        /// </summary>
        /// <param name="color">The current item foreground color.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb CurrentColor(ConsoleColor color)
        {
            Options.CurrentScheme = Scheme(color);
            Options.CurrentDecoration = Decoration.None;
            Options.CurrentForegroundRgb = null;
            Options.CurrentBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the current item foreground color.
        /// </summary>
        /// <param name="color">The current item foreground color.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb CurrentColor(SpectreColor color)
        {
            return CurrentColor(new Style(color));
        }

        /// <summary>
        /// Sets the current item foreground color.
        /// </summary>
        /// <param name="color">The current item foreground color.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb CurrentColor(RgbColor color)
        {
            return CurrentColor(new Style(color));
        }

        /// <summary>
        /// Sets the current item color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb CurrentColor(Style style)
        {
            ApplyCurrentStyle(style, Options.ResolveCurrentScheme());
            return this;
        }

        /// <summary>
        /// Sets the current item color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb CurrentColor(string style)
        {
            return CurrentColor(Style.Parse(style));
        }

        /// <summary>
        /// Sets the separator foreground color.
        /// </summary>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb SeparatorColor(ConsoleColor color)
        {
            Options.SeparatorScheme = Scheme(color);
            Options.SeparatorDecoration = Decoration.None;
            Options.SeparatorForegroundRgb = null;
            Options.SeparatorBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the separator foreground color.
        /// </summary>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb SeparatorColor(SpectreColor color)
        {
            return SeparatorColor(new Style(color));
        }

        /// <summary>
        /// Sets the separator foreground color.
        /// </summary>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb SeparatorColor(RgbColor color)
        {
            return SeparatorColor(new Style(color));
        }

        /// <summary>
        /// Sets the separator color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb SeparatorColor(Style style)
        {
            ApplySeparatorStyle(style, Options.ResolveSeparatorScheme());
            return this;
        }

        /// <summary>
        /// Sets the separator color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Breadcrumb"/> instance.</returns>
        public Breadcrumb SeparatorColor(string style)
        {
            return SeparatorColor(Style.Parse(style));
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

        private void ApplyItemStyle(Style style, ColorScheme fallback)
        {
            BreadcrumbOptions.StyledText resolved = BreadcrumbOptions.StyledText.FromStyle(style, fallback);
            Options.ItemScheme = resolved.Scheme;
            Options.ItemDecoration = resolved.Decoration;
            Options.ItemForegroundRgb = resolved.ForegroundRgb;
            Options.ItemBackgroundRgb = resolved.BackgroundRgb;
        }

        private void ApplyCurrentStyle(Style style, ColorScheme fallback)
        {
            BreadcrumbOptions.StyledText resolved = BreadcrumbOptions.StyledText.FromStyle(style, fallback);
            Options.CurrentScheme = resolved.Scheme;
            Options.CurrentDecoration = resolved.Decoration;
            Options.CurrentForegroundRgb = resolved.ForegroundRgb;
            Options.CurrentBackgroundRgb = resolved.BackgroundRgb;
        }

        private void ApplySeparatorStyle(Style style, ColorScheme fallback)
        {
            BreadcrumbOptions.StyledText resolved = BreadcrumbOptions.StyledText.FromStyle(style, fallback);
            Options.SeparatorScheme = resolved.Scheme;
            Options.SeparatorDecoration = resolved.Decoration;
            Options.SeparatorForegroundRgb = resolved.ForegroundRgb;
            Options.SeparatorBackgroundRgb = resolved.BackgroundRgb;
        }
    }
}
