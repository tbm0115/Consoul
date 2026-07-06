using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one item rendered by <see cref="ChecklistEntry"/>.
    /// </summary>
    public class ChecklistItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChecklistItem"/> class.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <param name="labelScheme">Optional color scheme used for the item label.</param>
        /// <param name="markerScheme">Optional color scheme used for the item marker.</param>
        /// <param name="descriptionScheme">Optional color scheme used for the item description.</param>
        public ChecklistItem(
            string label,
            ChecklistItemState state = ChecklistItemState.Pending,
            string description = null,
            ColorScheme? labelScheme = null,
            ColorScheme? markerScheme = null,
            ColorScheme? descriptionScheme = null)
        {
            Label = label ?? string.Empty;
            State = state;
            Description = description ?? string.Empty;
            LabelScheme = labelScheme;
            MarkerScheme = markerScheme;
            DescriptionScheme = descriptionScheme;
        }

        /// <summary>
        /// Gets or sets the item label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the state rendered for the item.
        /// </summary>
        public ChecklistItemState State { get; set; }

        /// <summary>
        /// Gets or sets optional descriptive text rendered under the item label.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme used for the item label.
        /// </summary>
        public ColorScheme? LabelScheme { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme used for the item marker.
        /// </summary>
        public ColorScheme? MarkerScheme { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme used for the item description.
        /// </summary>
        public ColorScheme? DescriptionScheme { get; set; }

        /// <summary>
        /// Creates a checklist item with optional foreground colors.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <param name="labelColor">Optional foreground color used for the item label.</param>
        /// <param name="markerColor">Optional foreground color used for the item marker.</param>
        /// <param name="descriptionColor">Optional foreground color used for the item description.</param>
        /// <returns>A configured <see cref="ChecklistItem"/>.</returns>
        public static ChecklistItem Create(
            string label,
            ChecklistItemState state = ChecklistItemState.Pending,
            string description = null,
            ConsoleColor? labelColor = null,
            ConsoleColor? markerColor = null,
            ConsoleColor? descriptionColor = null)
        {
            return new ChecklistItem(
                label,
                state,
                description,
                CreateScheme(labelColor),
                CreateScheme(markerColor),
                CreateScheme(descriptionColor));
        }

        /// <summary>
        /// Creates a checklist item using reusable style objects.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="labelStyle">Optional style used for the item label.</param>
        /// <param name="markerStyle">Optional style used for the item marker.</param>
        /// <param name="descriptionStyle">Optional style used for the item description.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <returns>A configured <see cref="ChecklistItem"/>.</returns>
        public static ChecklistItem Create(
            string label,
            Style labelStyle,
            Style markerStyle = null,
            Style descriptionStyle = null,
            ChecklistItemState state = ChecklistItemState.Pending,
            string description = null)
        {
            return new ChecklistItem(
                label,
                state,
                description,
                CreateScheme(labelStyle),
                CreateScheme(markerStyle),
                CreateScheme(descriptionStyle));
        }

        /// <summary>
        /// Creates a checklist item using Spectre-style style text.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <param name="labelStyle">Optional style used for the item label, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the item marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <param name="descriptionStyle">Optional style used for the item description, such as <c>darkgray</c>.</param>
        /// <returns>A configured <see cref="ChecklistItem"/>.</returns>
        public static ChecklistItem CreateStyled(
            string label,
            ChecklistItemState state = ChecklistItemState.Pending,
            string description = null,
            string labelStyle = null,
            string markerStyle = null,
            string descriptionStyle = null)
        {
            return new ChecklistItem(
                label,
                state,
                description,
                CreateScheme(labelStyle),
                CreateScheme(markerStyle),
                CreateScheme(descriptionStyle));
        }

        private static ColorScheme? CreateScheme(ConsoleColor? color)
        {
            if (!color.HasValue)
            {
                return null;
            }

            return new ColorScheme
            {
                Color = color.Value,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme? CreateScheme(string style)
        {
            return string.IsNullOrWhiteSpace(style)
                ? (ColorScheme?)null
                : Style.Parse(style).ToColorScheme(RenderOptions.DefaultScheme);
        }

        private static ColorScheme? CreateScheme(Style style)
        {
            return style?.ToColorScheme(RenderOptions.DefaultScheme);
        }
    }
}
