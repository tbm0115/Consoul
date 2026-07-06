using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent checklist construction over <see cref="ChecklistEntry"/>.
    /// </summary>
    public class Checklist : ChecklistEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Checklist"/> class.
        /// </summary>
        public Checklist()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Checklist"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the checklist.</param>
        public Checklist(ChecklistOptions options) : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Checklist"/> class with checklist items.
        /// </summary>
        /// <param name="items">The checklist items to render.</param>
        public Checklist(params ChecklistItem[] items) : this((IEnumerable<ChecklistItem>)items)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Checklist"/> class with checklist items.
        /// </summary>
        /// <param name="items">The checklist items to render.</param>
        public Checklist(IEnumerable<ChecklistItem> items) : base(items)
        {
        }

        /// <summary>
        /// Adds an item to the checklist.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItem(string label, ChecklistItemState state = ChecklistItemState.Pending, string description = null)
        {
            Add(label, state, description);
            return this;
        }

        /// <summary>
        /// Adds an item to the checklist using reusable style objects.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="labelStyle">Optional style used for the item label.</param>
        /// <param name="markerStyle">Optional style used for the item marker.</param>
        /// <param name="descriptionStyle">Optional style used for the item description.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItem(
            string label,
            Style labelStyle,
            Style markerStyle = null,
            Style descriptionStyle = null,
            ChecklistItemState state = ChecklistItemState.Pending,
            string description = null)
        {
            Add(label, labelStyle, markerStyle, descriptionStyle, state, description);
            return this;
        }

        /// <summary>
        /// Adds an item to the checklist using Spectre-style style text.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="labelStyle">Optional style used for the item label, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the item marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <param name="descriptionStyle">Optional style used for the item description, such as <c>darkgray</c>.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItem(
            string label,
            string labelStyle,
            string markerStyle = null,
            string descriptionStyle = null,
            ChecklistItemState state = ChecklistItemState.Pending,
            string description = null)
        {
            Add(label, labelStyle, markerStyle, descriptionStyle, state, description);
            return this;
        }

        /// <summary>
        /// Adds an item to the checklist using Spectre-style style text.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <param name="labelStyle">Optional style used for the item label, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the item marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <param name="descriptionStyle">Optional style used for the item description, such as <c>darkgray</c>.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItemStyled(
            string label,
            ChecklistItemState state = ChecklistItemState.Pending,
            string description = null,
            string labelStyle = null,
            string markerStyle = null,
            string descriptionStyle = null)
        {
            AddStyled(label, state, description, labelStyle, markerStyle, descriptionStyle);
            return this;
        }

        /// <summary>
        /// Adds an existing checklist item.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItem(ChecklistItem item)
        {
            Add(item);
            return this;
        }

        /// <summary>
        /// Adds multiple existing checklist items.
        /// </summary>
        /// <param name="items">The checklist items to add.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItems(IEnumerable<ChecklistItem> items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (ChecklistItem item in items)
            {
                Add(item);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple pending checklist item labels.
        /// </summary>
        /// <param name="labels">The item labels to add.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItems(params string[] labels)
        {
            return AddItems((IEnumerable<string>)labels);
        }

        /// <summary>
        /// Adds multiple checklist item labels with the same state.
        /// </summary>
        /// <param name="labels">The item labels to add.</param>
        /// <param name="state">The state rendered for the items.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItems(IEnumerable<string> labels, ChecklistItemState state = ChecklistItemState.Pending)
        {
            if (labels == null)
            {
                return this;
            }

            foreach (string label in labels)
            {
                AddItem(label, state);
            }

            return this;
        }

        /// <summary>
        /// Adds checklist items projected from a model collection with Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into checklist items.</param>
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="labelStyleSelector">Selects style text for the item label.</param>
        /// <param name="markerStyleSelector">Optionally selects style text for the item marker.</param>
        /// <param name="descriptionStyleSelector">Optionally selects style text for the item description.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, string> labelStyleSelector,
            Func<T, string> markerStyleSelector = null,
            Func<T, string> descriptionStyleSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (labelStyleSelector == null)
            {
                throw new ArgumentNullException(nameof(labelStyleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(
                    labelSelector(item),
                    labelStyleSelector(item),
                    markerStyleSelector != null ? markerStyleSelector(item) : null,
                    descriptionStyleSelector != null ? descriptionStyleSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds checklist items projected from a model collection with Spectre-style colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into checklist items.</param>
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="labelColorSelector">Selects a Spectre-style color for the item label.</param>
        /// <param name="markerColorSelector">Optionally selects a Spectre-style color for the item marker.</param>
        /// <param name="descriptionColorSelector">Optionally selects a Spectre-style color for the item description.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, SpectreColor> labelColorSelector,
            Func<T, SpectreColor> markerColorSelector = null,
            Func<T, SpectreColor> descriptionColorSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (labelColorSelector == null)
            {
                throw new ArgumentNullException(nameof(labelColorSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(
                    labelSelector(item),
                    ChecklistItemState.Pending,
                    null,
                    ResolveColor(item, labelColorSelector),
                    ResolveColor(item, markerColorSelector),
                    ResolveColor(item, descriptionColorSelector));
            }

            return this;
        }

        /// <summary>
        /// Adds checklist items projected from a model collection with RGB colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into checklist items.</param>
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="labelColorSelector">Selects an RGB color for the item label.</param>
        /// <param name="markerColorSelector">Optionally selects an RGB color for the item marker.</param>
        /// <param name="descriptionColorSelector">Optionally selects an RGB color for the item description.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, RgbColor> labelColorSelector,
            Func<T, RgbColor> markerColorSelector = null,
            Func<T, RgbColor> descriptionColorSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (labelColorSelector == null)
            {
                throw new ArgumentNullException(nameof(labelColorSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(
                    labelSelector(item),
                    ChecklistItemState.Pending,
                    null,
                    ResolveColor(item, labelColorSelector),
                    ResolveColor(item, markerColorSelector),
                    ResolveColor(item, descriptionColorSelector));
            }

            return this;
        }

        /// <summary>
        /// Adds checklist items projected from a model collection with state, description, and Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into checklist items.</param>
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="stateSelector">Selects the item state.</param>
        /// <param name="descriptionSelector">Selects the item description.</param>
        /// <param name="labelStyleSelector">Selects style text for the item label.</param>
        /// <param name="markerStyleSelector">Optionally selects style text for the item marker.</param>
        /// <param name="descriptionStyleSelector">Optionally selects style text for the item description.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, ChecklistItemState> stateSelector,
            Func<T, string> descriptionSelector,
            Func<T, string> labelStyleSelector,
            Func<T, string> markerStyleSelector = null,
            Func<T, string> descriptionStyleSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (stateSelector == null)
            {
                throw new ArgumentNullException(nameof(stateSelector));
            }

            if (descriptionSelector == null)
            {
                throw new ArgumentNullException(nameof(descriptionSelector));
            }

            if (labelStyleSelector == null)
            {
                throw new ArgumentNullException(nameof(labelStyleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(
                    labelSelector(item),
                    labelStyleSelector(item),
                    markerStyleSelector != null ? markerStyleSelector(item) : null,
                    descriptionStyleSelector != null ? descriptionStyleSelector(item) : null,
                    stateSelector(item),
                    descriptionSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds checklist items projected from a model collection with state, description, and Spectre-style colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into checklist items.</param>
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="stateSelector">Selects the item state.</param>
        /// <param name="descriptionSelector">Selects the item description.</param>
        /// <param name="labelColorSelector">Selects a Spectre-style color for the item label.</param>
        /// <param name="markerColorSelector">Optionally selects a Spectre-style color for the item marker.</param>
        /// <param name="descriptionColorSelector">Optionally selects a Spectre-style color for the item description.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, ChecklistItemState> stateSelector,
            Func<T, string> descriptionSelector,
            Func<T, SpectreColor> labelColorSelector,
            Func<T, SpectreColor> markerColorSelector = null,
            Func<T, SpectreColor> descriptionColorSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (stateSelector == null)
            {
                throw new ArgumentNullException(nameof(stateSelector));
            }

            if (descriptionSelector == null)
            {
                throw new ArgumentNullException(nameof(descriptionSelector));
            }

            if (labelColorSelector == null)
            {
                throw new ArgumentNullException(nameof(labelColorSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(
                    labelSelector(item),
                    stateSelector(item),
                    descriptionSelector(item),
                    ResolveColor(item, labelColorSelector),
                    ResolveColor(item, markerColorSelector),
                    ResolveColor(item, descriptionColorSelector));
            }

            return this;
        }

        /// <summary>
        /// Adds checklist items projected from a model collection with state, description, and RGB colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into checklist items.</param>
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="stateSelector">Selects the item state.</param>
        /// <param name="descriptionSelector">Selects the item description.</param>
        /// <param name="labelColorSelector">Selects an RGB color for the item label.</param>
        /// <param name="markerColorSelector">Optionally selects an RGB color for the item marker.</param>
        /// <param name="descriptionColorSelector">Optionally selects an RGB color for the item description.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, ChecklistItemState> stateSelector,
            Func<T, string> descriptionSelector,
            Func<T, RgbColor> labelColorSelector,
            Func<T, RgbColor> markerColorSelector = null,
            Func<T, RgbColor> descriptionColorSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (stateSelector == null)
            {
                throw new ArgumentNullException(nameof(stateSelector));
            }

            if (descriptionSelector == null)
            {
                throw new ArgumentNullException(nameof(descriptionSelector));
            }

            if (labelColorSelector == null)
            {
                throw new ArgumentNullException(nameof(labelColorSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(
                    labelSelector(item),
                    stateSelector(item),
                    descriptionSelector(item),
                    ResolveColor(item, labelColorSelector),
                    ResolveColor(item, markerColorSelector),
                    ResolveColor(item, descriptionColorSelector));
            }

            return this;
        }

        /// <summary>
        /// Adds checklist items projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into checklist items.</param>
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="stateSelector">Optionally selects the item state. Items default to <see cref="ChecklistItemState.Pending"/>.</param>
        /// <param name="descriptionSelector">Optionally selects the item description.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, ChecklistItemState> stateSelector = null,
            Func<T, string> descriptionSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(
                    labelSelector(item),
                    stateSelector != null ? stateSelector(item) : ChecklistItemState.Pending,
                    descriptionSelector != null ? descriptionSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds checklist items projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into checklist items.</param>
        /// <param name="itemSelector">Selects the checklist item to add for each model item.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddItems<T>(IEnumerable<T> items, Func<T, ChecklistItem> itemSelector)
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
        /// Adds a completed item to the checklist.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddSuccess(string label, string description = null)
        {
            return AddItem(label, ChecklistItemState.Complete, description);
        }

        /// <summary>
        /// Adds a warning item to the checklist.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddWarning(string label, string description = null)
        {
            return AddItem(label, ChecklistItemState.Warning, description);
        }

        /// <summary>
        /// Adds an error item to the checklist.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddError(string label, string description = null)
        {
            return AddItem(label, ChecklistItemState.Error, description);
        }

        /// <summary>
        /// Adds a skipped item to the checklist.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist AddSkipped(string label, string description = null)
        {
            return AddItem(label, ChecklistItemState.Skipped, description);
        }

        /// <summary>
        /// Sets the requested checklist width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Sets the number of spaces between the marker and item text.
        /// </summary>
        /// <param name="gap">The marker gap.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist MarkerGap(int gap)
        {
            Options.MarkerGap = gap;
            return this;
        }

        /// <summary>
        /// Shows or hides item descriptions.
        /// </summary>
        /// <param name="show">Whether descriptions should render.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist ShowDescriptions(bool show = true)
        {
            Options.ShowDescriptions = show;
            return this;
        }

        /// <summary>
        /// Sets the marker text for checklist states.
        /// </summary>
        /// <param name="pending">Marker for pending items.</param>
        /// <param name="complete">Marker for completed items.</param>
        /// <param name="warning">Marker for warning items.</param>
        /// <param name="error">Marker for error items.</param>
        /// <param name="skipped">Marker for skipped items.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist Markers(string pending = null, string complete = null, string warning = null, string error = null, string skipped = null)
        {
            if (pending != null)
            {
                Options.PendingMarker = pending;
            }

            if (complete != null)
            {
                Options.CompleteMarker = complete;
            }

            if (warning != null)
            {
                Options.WarningMarker = warning;
            }

            if (error != null)
            {
                Options.ErrorMarker = error;
            }

            if (skipped != null)
            {
                Options.SkippedMarker = skipped;
            }

            return this;
        }

        /// <summary>
        /// Sets the item label foreground color.
        /// </summary>
        /// <param name="color">The item label foreground color.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist LabelColor(ConsoleColor color)
        {
            Options.LabelScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the item label foreground color.
        /// </summary>
        /// <param name="color">The item label foreground color.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist LabelColor(SpectreColor color)
        {
            Options.LabelScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the item label foreground color.
        /// </summary>
        /// <param name="color">The item label foreground color.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist LabelColor(RgbColor color)
        {
            Options.LabelScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the item label color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist LabelColor(Style style)
        {
            Options.LabelScheme = Scheme(style, Options.ResolveLabelScheme());
            return this;
        }

        /// <summary>
        /// Sets the item label color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist LabelColor(string style)
        {
            Options.LabelScheme = Style.ParseScheme(style, Options.ResolveLabelScheme());
            return this;
        }

        /// <summary>
        /// Sets the item description foreground color.
        /// </summary>
        /// <param name="color">The item description foreground color.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist DescriptionColor(ConsoleColor color)
        {
            Options.DescriptionScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the item description foreground color.
        /// </summary>
        /// <param name="color">The item description foreground color.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist DescriptionColor(SpectreColor color)
        {
            Options.DescriptionScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the item description foreground color.
        /// </summary>
        /// <param name="color">The item description foreground color.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist DescriptionColor(RgbColor color)
        {
            Options.DescriptionScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the item description color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist DescriptionColor(Style style)
        {
            Options.DescriptionScheme = Scheme(style, Options.ResolveDescriptionScheme());
            return this;
        }

        /// <summary>
        /// Sets the item description color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Checklist"/> instance.</returns>
        public Checklist DescriptionColor(string style)
        {
            Options.DescriptionScheme = Style.ParseScheme(style, Options.ResolveDescriptionScheme());
            return this;
        }

        private static ConsoleColor? ResolveColor<T>(T item, Func<T, SpectreColor> selector)
        {
            return selector == null
                ? (ConsoleColor?)null
                : selector(item).ToConsoleColor();
        }

        private static ConsoleColor? ResolveColor<T>(T item, Func<T, RgbColor> selector)
        {
            return selector == null
                ? (ConsoleColor?)null
                : selector(item).ToConsoleColor();
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
