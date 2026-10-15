using System;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one action, shortcut, separator, or disabled command displayed in a view help surface.
    /// </summary>
    public class ViewHelpItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewHelpItem"/> class.
        /// </summary>
        /// <param name="label">Action label or separator text.</param>
        /// <param name="description">Optional helper text describing the action.</param>
        /// <param name="shortcut">Optional shortcut that can select the action.</param>
        /// <param name="disabledReason">Optional reason the action is currently unavailable.</param>
        /// <param name="isSeparator">Indicates whether the item is a display-only separator.</param>
        /// <param name="scheme">Optional color scheme used for the item label.</param>
        public ViewHelpItem(
            string label,
            string description = null,
            string shortcut = null,
            string disabledReason = null,
            bool isSeparator = false,
            ColorScheme? scheme = null)
        {
            Label = label ?? string.Empty;
            Description = description;
            Shortcut = shortcut;
            DisabledReason = disabledReason;
            IsSeparator = isSeparator;
            Scheme = scheme;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewHelpItem"/> class.
        /// </summary>
        /// <param name="label">Action label or separator text.</param>
        /// <param name="description">Optional helper text describing the action.</param>
        /// <param name="shortcut">Optional shortcut that can select the action.</param>
        /// <param name="disabledReason">Optional reason the action is currently unavailable.</param>
        /// <param name="isSeparator">Indicates whether the item is a display-only separator.</param>
        /// <param name="scheme">Optional color scheme used for the item label.</param>
        /// <param name="decoration">Optional ANSI decoration used for the item label.</param>
        /// <param name="foregroundRgb">Optional RGB foreground color used on ANSI-capable terminals.</param>
        /// <param name="backgroundRgb">Optional RGB background color used on ANSI-capable terminals.</param>
        public ViewHelpItem(
            string label,
            string description,
            string shortcut,
            string disabledReason,
            bool isSeparator,
            ColorScheme? scheme,
            Decoration? decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
            : this(label, description, shortcut, disabledReason, isSeparator, scheme)
        {
            Decoration = decoration;
            ForegroundRgb = foregroundRgb;
            BackgroundRgb = backgroundRgb;
        }

        /// <summary>
        /// Gets or sets the action label or separator text.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets optional helper text describing the action.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets optional shortcut text that can select the action.
        /// </summary>
        public string Shortcut { get; set; }

        /// <summary>
        /// Gets or sets optional text explaining why the action is currently unavailable.
        /// </summary>
        public string DisabledReason { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is a display-only separator.
        /// </summary>
        public bool IsSeparator { get; set; }

        /// <summary>
        /// Gets or sets the optional color scheme used for the item label.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Gets or sets the optional ANSI decoration used for the item label.
        /// </summary>
        public Decoration? Decoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for the item label on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for the item label on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BackgroundRgb { get; set; }

        /// <summary>
        /// Creates a display-only separator for a view help surface.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Optional separator foreground color.</param>
        /// <returns>A configured separator item.</returns>
        public static ViewHelpItem Separator(string label = null, ConsoleColor? color = null)
        {
            ColorScheme? scheme = null;
            if (color.HasValue)
            {
                scheme = CreateScheme(color.Value);
            }

            return new ViewHelpItem(label ?? string.Empty, isSeparator: true, scheme: scheme);
        }

        /// <summary>
        /// Creates a display-only separator for a view help surface using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Separator foreground color.</param>
        /// <returns>A configured separator item.</returns>
        public static ViewHelpItem Separator(string label, SpectreColor color)
        {
            return Separator(label, color.ToRgbColor());
        }

        /// <summary>
        /// Creates a display-only separator for a view help surface using an RGB foreground color.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Separator foreground color.</param>
        /// <returns>A configured separator item.</returns>
        public static ViewHelpItem Separator(string label, RgbColor color)
        {
            return new ViewHelpItem(
                label ?? string.Empty,
                null,
                null,
                null,
                true,
                CreateScheme(color.ToConsoleColor()),
                global::ConsoulLibrary.Decoration.None,
                color,
                null);
        }

        /// <summary>
        /// Creates an action item using Spectre-style style text for the label.
        /// </summary>
        /// <param name="label">Action label.</param>
        /// <param name="style">Optional style used for the item label, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="description">Optional helper text describing the action.</param>
        /// <param name="shortcut">Optional shortcut that can select the action.</param>
        /// <param name="disabledReason">Optional reason the action is currently unavailable.</param>
        /// <returns>A configured help item.</returns>
        public static ViewHelpItem Styled(
            string label,
            string style,
            string description = null,
            string shortcut = null,
            string disabledReason = null)
        {
            return Styled(
                label,
                string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style),
                description,
                shortcut,
                disabledReason);
        }

        /// <summary>
        /// Creates an action item using a reusable style object for the label.
        /// </summary>
        /// <param name="label">Action label.</param>
        /// <param name="style">Optional style used for the item label.</param>
        /// <param name="description">Optional helper text describing the action.</param>
        /// <param name="shortcut">Optional shortcut that can select the action.</param>
        /// <param name="disabledReason">Optional reason the action is currently unavailable.</param>
        /// <returns>A configured help item.</returns>
        public static ViewHelpItem Styled(
            string label,
            Style style,
            string description = null,
            string shortcut = null,
            string disabledReason = null)
        {
            return new ViewHelpItem(
                label,
                description,
                shortcut,
                disabledReason,
                false,
                style?.ToColorScheme(RenderOptions.DefaultScheme),
                style?.Decoration,
                style?.ForegroundRgb,
                style?.BackgroundRgb);
        }

        /// <summary>
        /// Creates an action item using a Spectre-style color facade for the label.
        /// </summary>
        /// <param name="label">Action label.</param>
        /// <param name="color">Foreground color used for the item label.</param>
        /// <param name="description">Optional helper text describing the action.</param>
        /// <param name="shortcut">Optional shortcut that can select the action.</param>
        /// <param name="disabledReason">Optional reason the action is currently unavailable.</param>
        /// <returns>A configured help item.</returns>
        public static ViewHelpItem Styled(
            string label,
            SpectreColor color,
            string description = null,
            string shortcut = null,
            string disabledReason = null)
        {
            return Styled(label, color.ToRgbColor(), description, shortcut, disabledReason);
        }

        /// <summary>
        /// Creates an action item using an RGB foreground color for the label.
        /// </summary>
        /// <param name="label">Action label.</param>
        /// <param name="color">Foreground color used for the item label.</param>
        /// <param name="description">Optional helper text describing the action.</param>
        /// <param name="shortcut">Optional shortcut that can select the action.</param>
        /// <param name="disabledReason">Optional reason the action is currently unavailable.</param>
        /// <returns>A configured help item.</returns>
        public static ViewHelpItem Styled(
            string label,
            RgbColor color,
            string description = null,
            string shortcut = null,
            string disabledReason = null)
        {
            return new ViewHelpItem(
                label,
                description,
                shortcut,
                disabledReason,
                false,
                CreateScheme(color.ToConsoleColor()),
                global::ConsoulLibrary.Decoration.None,
                color,
                null);
        }

        /// <summary>
        /// Creates a display-only separator for a view help surface using Spectre-style style text.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional style used for the separator, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>A configured separator item.</returns>
        public static ViewHelpItem Separator(string label, string style)
        {
            return Separator(label, string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style));
        }

        /// <summary>
        /// Creates a display-only separator for a view help surface using a reusable style object.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional style used for the separator.</param>
        /// <returns>A configured separator item.</returns>
        public static ViewHelpItem Separator(string label, Style style)
        {
            return new ViewHelpItem(
                label ?? string.Empty,
                null,
                null,
                null,
                true,
                style?.ToColorScheme(RenderOptions.DefaultScheme),
                style?.Decoration,
                style?.ForegroundRgb,
                style?.BackgroundRgb);
        }

        private static ColorScheme CreateScheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }
    }
}
