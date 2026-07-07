using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent timeline construction over <see cref="TimelineEntry"/>.
    /// </summary>
    public class Timeline : TimelineEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Timeline"/> class.
        /// </summary>
        public Timeline()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Timeline"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the timeline.</param>
        public Timeline(TimelineOptions options) : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Timeline"/> class with items.
        /// </summary>
        /// <param name="items">The items to render.</param>
        /// <param name="options">Optional rendering options for the timeline.</param>
        public Timeline(IEnumerable<TimelineItem> items, TimelineOptions options = null) : base(items, options)
        {
        }

        /// <summary>
        /// Adds a timeline step.
        /// </summary>
        /// <param name="title">The primary step text.</param>
        /// <param name="state">The rendered step state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        /// <param name="titleColor">Optional foreground color used for the title.</param>
        /// <param name="markerColor">Optional foreground color used for the marker.</param>
        /// <param name="descriptionColor">Optional foreground color used for the description.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddStep(
            string title,
            TimelineItemState state = TimelineItemState.Pending,
            string description = null,
            DateTimeOffset? timestamp = null,
            ConsoleColor? titleColor = null,
            ConsoleColor? markerColor = null,
            ConsoleColor? descriptionColor = null)
        {
            Add(title, state, description, timestamp, titleColor, markerColor, descriptionColor);
            return this;
        }

        /// <summary>
        /// Adds a timeline step using reusable style objects.
        /// </summary>
        /// <param name="title">The primary step text.</param>
        /// <param name="titleStyle">Optional style used for the title.</param>
        /// <param name="markerStyle">Optional style used for the marker.</param>
        /// <param name="descriptionStyle">Optional style used for the description.</param>
        /// <param name="state">The rendered step state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddStep(
            string title,
            Style titleStyle,
            Style markerStyle = null,
            Style descriptionStyle = null,
            TimelineItemState state = TimelineItemState.Pending,
            string description = null,
            DateTimeOffset? timestamp = null)
        {
            Add(title, titleStyle, markerStyle, descriptionStyle, state, description, timestamp);
            return this;
        }

        /// <summary>
        /// Adds a timeline step using Spectre-style style text.
        /// </summary>
        /// <param name="title">The primary step text.</param>
        /// <param name="state">The rendered step state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        /// <param name="titleStyle">Optional style used for the title, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <param name="descriptionStyle">Optional style used for the description, such as <c>darkgray</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddStepStyled(
            string title,
            TimelineItemState state = TimelineItemState.Pending,
            string description = null,
            DateTimeOffset? timestamp = null,
            string titleStyle = null,
            string markerStyle = null,
            string descriptionStyle = null)
        {
            AddStyled(title, state, description, timestamp, titleStyle, markerStyle, descriptionStyle);
            return this;
        }

        /// <summary>
        /// Adds an existing timeline item.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddStep(TimelineItem item)
        {
            Add(item);
            return this;
        }

        /// <summary>
        /// Adds existing timeline items.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddSteps(IEnumerable<TimelineItem> items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (TimelineItem item in items)
            {
                AddStep(item);
            }

            return this;
        }

        /// <summary>
        /// Adds timeline steps projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into timeline steps.</param>
        /// <param name="titleSelector">Selects the step title.</param>
        /// <param name="stateSelector">Optionally selects the step state. Steps default to <see cref="TimelineItemState.Pending"/>.</param>
        /// <param name="descriptionSelector">Optionally selects the step description.</param>
        /// <param name="timestampSelector">Optionally selects the step timestamp.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddSteps<T>(
            IEnumerable<T> items,
            Func<T, string> titleSelector,
            Func<T, TimelineItemState> stateSelector = null,
            Func<T, string> descriptionSelector = null,
            Func<T, DateTimeOffset?> timestampSelector = null)
        {
            if (titleSelector == null)
            {
                throw new ArgumentNullException(nameof(titleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddStep(
                    titleSelector(item),
                    stateSelector != null ? stateSelector(item) : TimelineItemState.Pending,
                    descriptionSelector != null ? descriptionSelector(item) : null,
                    timestampSelector != null ? timestampSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds timeline steps projected from a model collection using Spectre-style colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into timeline steps.</param>
        /// <param name="titleSelector">Selects the step title.</param>
        /// <param name="titleColorSelector">Selects a Spectre-style color for the step title.</param>
        /// <param name="markerColorSelector">Optionally selects a Spectre-style color for the step marker.</param>
        /// <param name="descriptionColorSelector">Optionally selects a Spectre-style color for the step description.</param>
        /// <param name="stateSelector">Optionally selects the step state. Steps default to <see cref="TimelineItemState.Pending"/>.</param>
        /// <param name="descriptionSelector">Optionally selects the step description.</param>
        /// <param name="timestampSelector">Optionally selects the step timestamp.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddSteps<T>(
            IEnumerable<T> items,
            Func<T, string> titleSelector,
            Func<T, SpectreColor> titleColorSelector,
            Func<T, SpectreColor> markerColorSelector = null,
            Func<T, SpectreColor> descriptionColorSelector = null,
            Func<T, TimelineItemState> stateSelector = null,
            Func<T, string> descriptionSelector = null,
            Func<T, DateTimeOffset?> timestampSelector = null)
        {
            if (titleSelector == null)
            {
                throw new ArgumentNullException(nameof(titleSelector));
            }

            if (titleColorSelector == null)
            {
                throw new ArgumentNullException(nameof(titleColorSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddStep(TimelineItem.Create(
                    titleSelector(item),
                    titleColorSelector(item),
                    markerColorSelector != null ? markerColorSelector(item) : (SpectreColor?)null,
                    descriptionColorSelector != null ? descriptionColorSelector(item) : (SpectreColor?)null,
                    stateSelector != null ? stateSelector(item) : TimelineItemState.Pending,
                    descriptionSelector != null ? descriptionSelector(item) : null,
                    timestampSelector != null ? timestampSelector(item) : null));
            }

            return this;
        }

        /// <summary>
        /// Adds timeline steps projected from a model collection using RGB colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into timeline steps.</param>
        /// <param name="titleSelector">Selects the step title.</param>
        /// <param name="titleColorSelector">Selects an RGB color for the step title.</param>
        /// <param name="markerColorSelector">Optionally selects an RGB color for the step marker.</param>
        /// <param name="descriptionColorSelector">Optionally selects an RGB color for the step description.</param>
        /// <param name="stateSelector">Optionally selects the step state. Steps default to <see cref="TimelineItemState.Pending"/>.</param>
        /// <param name="descriptionSelector">Optionally selects the step description.</param>
        /// <param name="timestampSelector">Optionally selects the step timestamp.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddSteps<T>(
            IEnumerable<T> items,
            Func<T, string> titleSelector,
            Func<T, RgbColor> titleColorSelector,
            Func<T, RgbColor> markerColorSelector = null,
            Func<T, RgbColor> descriptionColorSelector = null,
            Func<T, TimelineItemState> stateSelector = null,
            Func<T, string> descriptionSelector = null,
            Func<T, DateTimeOffset?> timestampSelector = null)
        {
            if (titleSelector == null)
            {
                throw new ArgumentNullException(nameof(titleSelector));
            }

            if (titleColorSelector == null)
            {
                throw new ArgumentNullException(nameof(titleColorSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddStep(TimelineItem.Create(
                    titleSelector(item),
                    titleColorSelector(item),
                    markerColorSelector != null ? markerColorSelector(item) : (RgbColor?)null,
                    descriptionColorSelector != null ? descriptionColorSelector(item) : (RgbColor?)null,
                    stateSelector != null ? stateSelector(item) : TimelineItemState.Pending,
                    descriptionSelector != null ? descriptionSelector(item) : null,
                    timestampSelector != null ? timestampSelector(item) : null));
            }

            return this;
        }

        /// <summary>
        /// Adds timeline steps projected from a model collection using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into timeline steps.</param>
        /// <param name="titleSelector">Selects the step title.</param>
        /// <param name="stateSelector">Optionally selects the step state. Steps default to <see cref="TimelineItemState.Pending"/>.</param>
        /// <param name="descriptionSelector">Optionally selects the step description.</param>
        /// <param name="timestampSelector">Optionally selects the step timestamp.</param>
        /// <param name="titleStyleSelector">Optionally selects style text for the step title, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyleSelector">Optionally selects style text for the step marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <param name="descriptionStyleSelector">Optionally selects style text for the step description, such as <c>darkgray</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddStepsStyled<T>(
            IEnumerable<T> items,
            Func<T, string> titleSelector,
            Func<T, TimelineItemState> stateSelector = null,
            Func<T, string> descriptionSelector = null,
            Func<T, DateTimeOffset?> timestampSelector = null,
            Func<T, string> titleStyleSelector = null,
            Func<T, string> markerStyleSelector = null,
            Func<T, string> descriptionStyleSelector = null)
        {
            if (titleSelector == null)
            {
                throw new ArgumentNullException(nameof(titleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddStyled(
                    titleSelector(item),
                    stateSelector != null ? stateSelector(item) : TimelineItemState.Pending,
                    descriptionSelector != null ? descriptionSelector(item) : null,
                    timestampSelector != null ? timestampSelector(item) : null,
                    titleStyleSelector != null ? titleStyleSelector(item) : null,
                    markerStyleSelector != null ? markerStyleSelector(item) : null,
                    descriptionStyleSelector != null ? descriptionStyleSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds timeline steps projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into timeline steps.</param>
        /// <param name="itemSelector">Selects the timeline item to add for each model item.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddSteps<T>(IEnumerable<T> items, Func<T, TimelineItem> itemSelector)
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
                AddStep(itemSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds a pending timeline step.
        /// </summary>
        /// <param name="title">The primary step text.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddPending(string title, string description = null)
        {
            return AddStep(title, TimelineItemState.Pending, description);
        }

        /// <summary>
        /// Adds an active timeline step.
        /// </summary>
        /// <param name="title">The primary step text.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddActive(string title, string description = null)
        {
            return AddStep(title, TimelineItemState.Active, description);
        }

        /// <summary>
        /// Adds a completed timeline step.
        /// </summary>
        /// <param name="title">The primary step text.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddComplete(string title, string description = null)
        {
            return AddStep(title, TimelineItemState.Complete, description);
        }

        /// <summary>
        /// Adds a warning timeline step.
        /// </summary>
        /// <param name="title">The primary step text.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddWarning(string title, string description = null)
        {
            return AddStep(title, TimelineItemState.Warning, description);
        }

        /// <summary>
        /// Adds an error timeline step.
        /// </summary>
        /// <param name="title">The primary step text.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddError(string title, string description = null)
        {
            return AddStep(title, TimelineItemState.Error, description);
        }

        /// <summary>
        /// Adds a skipped timeline step.
        /// </summary>
        /// <param name="title">The primary step text.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline AddSkipped(string title, string description = null)
        {
            return AddStep(title, TimelineItemState.Skipped, description);
        }

        /// <summary>
        /// Sets the requested timeline width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the timeline to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline Expand()
        {
            Options.Width = null;
            return this;
        }

        /// <summary>
        /// Sets the number of spaces written between markers and item text.
        /// </summary>
        /// <param name="gap">The marker gap.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline MarkerGap(int gap)
        {
            Options.MarkerGap = gap;
            return this;
        }

        /// <summary>
        /// Shows or hides item descriptions.
        /// </summary>
        /// <param name="show">Whether descriptions should render.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ShowDescriptions(bool show = true)
        {
            Options.ShowDescriptions = show;
            return this;
        }

        /// <summary>
        /// Hides item descriptions.
        /// </summary>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline HideDescriptions()
        {
            Options.ShowDescriptions = false;
            return this;
        }

        /// <summary>
        /// Shows or hides item timestamps.
        /// </summary>
        /// <param name="show">Whether timestamps should render.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ShowTimestamps(bool show = true)
        {
            Options.ShowTimestamps = show;
            return this;
        }

        /// <summary>
        /// Hides item timestamps.
        /// </summary>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline HideTimestamps()
        {
            Options.ShowTimestamps = false;
            return this;
        }

        /// <summary>
        /// Shows or hides connectors between timeline items.
        /// </summary>
        /// <param name="show">Whether connectors should render.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ShowConnectors(bool show = true)
        {
            Options.ShowConnectors = show;
            return this;
        }

        /// <summary>
        /// Hides connectors between timeline items.
        /// </summary>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline HideConnectors()
        {
            Options.ShowConnectors = false;
            return this;
        }

        /// <summary>
        /// Sets the timestamp format string.
        /// </summary>
        /// <param name="format">The timestamp format string.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline TimestampFormat(string format)
        {
            Options.TimestampFormat = format;
            return this;
        }

        /// <summary>
        /// Sets the connector rendered between timeline items.
        /// </summary>
        /// <param name="connector">The connector text.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline Connector(string connector)
        {
            Options.Connector = connector;
            return this;
        }

        /// <summary>
        /// Sets the marker rendered for each timeline state.
        /// </summary>
        /// <param name="pending">The marker for pending items.</param>
        /// <param name="active">The marker for active items.</param>
        /// <param name="complete">The marker for completed items.</param>
        /// <param name="warning">The marker for warning items.</param>
        /// <param name="error">The marker for error items.</param>
        /// <param name="skipped">The marker for skipped items.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline Markers(
            string pending = null,
            string active = null,
            string complete = null,
            string warning = null,
            string error = null,
            string skipped = null)
        {
            if (pending != null)
            {
                Options.PendingMarker = pending;
            }

            if (active != null)
            {
                Options.ActiveMarker = active;
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
        /// Sets the default title foreground color.
        /// </summary>
        /// <param name="color">The title foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline TitleColor(ConsoleColor color)
        {
            Options.TitleScheme = Scheme(color);
            Options.TitleDecoration = Decoration.None;
            Options.TitleForegroundRgb = null;
            Options.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default title foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The title foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline TitleColor(SpectreColor color)
        {
            Options.TitleScheme = Scheme(color);
            Options.TitleDecoration = Decoration.None;
            Options.TitleForegroundRgb = color.ToRgbColor();
            Options.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default title foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The title foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline TitleColor(RgbColor color)
        {
            Options.TitleScheme = Scheme(color);
            Options.TitleDecoration = Decoration.None;
            Options.TitleForegroundRgb = color;
            Options.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline TitleColor(Style style)
        {
            ApplyTitleStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the default title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline TitleColor(string style)
        {
            ApplyTitleStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the description foreground color.
        /// </summary>
        /// <param name="color">The description foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline DescriptionColor(ConsoleColor color)
        {
            Options.DescriptionScheme = Scheme(color);
            Options.DescriptionDecoration = Decoration.None;
            Options.DescriptionForegroundRgb = null;
            Options.DescriptionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the description foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The description foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline DescriptionColor(SpectreColor color)
        {
            Options.DescriptionScheme = Scheme(color);
            Options.DescriptionDecoration = Decoration.None;
            Options.DescriptionForegroundRgb = color.ToRgbColor();
            Options.DescriptionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the description foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The description foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline DescriptionColor(RgbColor color)
        {
            Options.DescriptionScheme = Scheme(color);
            Options.DescriptionDecoration = Decoration.None;
            Options.DescriptionForegroundRgb = color;
            Options.DescriptionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the description color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline DescriptionColor(Style style)
        {
            ApplyDescriptionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the description color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline DescriptionColor(string style)
        {
            ApplyDescriptionStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the pending marker foreground color.
        /// </summary>
        /// <param name="color">The pending marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline PendingColor(ConsoleColor color)
        {
            ApplyPendingStyle(Scheme(color), Decoration.None, null, null);
            return this;
        }

        /// <summary>
        /// Sets the pending marker foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The pending marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline PendingColor(SpectreColor color)
        {
            ApplyPendingStyle(Scheme(color), Decoration.None, color.ToRgbColor(), null);
            return this;
        }

        /// <summary>
        /// Sets the pending marker foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The pending marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline PendingColor(RgbColor color)
        {
            ApplyPendingStyle(Scheme(color), Decoration.None, color, null);
            return this;
        }

        /// <summary>
        /// Sets the pending marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline PendingColor(Style style)
        {
            ApplyPendingStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the pending marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline PendingColor(string style)
        {
            ApplyPendingStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the active marker foreground color.
        /// </summary>
        /// <param name="color">The active marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ActiveColor(ConsoleColor color)
        {
            ApplyActiveStyle(Scheme(color), Decoration.None, null, null);
            return this;
        }

        /// <summary>
        /// Sets the active marker foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The active marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ActiveColor(SpectreColor color)
        {
            ApplyActiveStyle(Scheme(color), Decoration.None, color.ToRgbColor(), null);
            return this;
        }

        /// <summary>
        /// Sets the active marker foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The active marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ActiveColor(RgbColor color)
        {
            ApplyActiveStyle(Scheme(color), Decoration.None, color, null);
            return this;
        }

        /// <summary>
        /// Sets the active marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ActiveColor(Style style)
        {
            ApplyActiveStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the active marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ActiveColor(string style)
        {
            ApplyActiveStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the completed marker foreground color.
        /// </summary>
        /// <param name="color">The completed marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline CompleteColor(ConsoleColor color)
        {
            ApplyCompleteStyle(Scheme(color), Decoration.None, null, null);
            return this;
        }

        /// <summary>
        /// Sets the completed marker foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The completed marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline CompleteColor(SpectreColor color)
        {
            ApplyCompleteStyle(Scheme(color), Decoration.None, color.ToRgbColor(), null);
            return this;
        }

        /// <summary>
        /// Sets the completed marker foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The completed marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline CompleteColor(RgbColor color)
        {
            ApplyCompleteStyle(Scheme(color), Decoration.None, color, null);
            return this;
        }

        /// <summary>
        /// Sets the completed marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline CompleteColor(Style style)
        {
            ApplyCompleteStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the completed marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline CompleteColor(string style)
        {
            ApplyCompleteStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the warning marker foreground color.
        /// </summary>
        /// <param name="color">The warning marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline WarningColor(ConsoleColor color)
        {
            ApplyWarningStyle(Scheme(color), Decoration.None, null, null);
            return this;
        }

        /// <summary>
        /// Sets the warning marker foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The warning marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline WarningColor(SpectreColor color)
        {
            ApplyWarningStyle(Scheme(color), Decoration.None, color.ToRgbColor(), null);
            return this;
        }

        /// <summary>
        /// Sets the warning marker foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The warning marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline WarningColor(RgbColor color)
        {
            ApplyWarningStyle(Scheme(color), Decoration.None, color, null);
            return this;
        }

        /// <summary>
        /// Sets the warning marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline WarningColor(Style style)
        {
            ApplyWarningStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the warning marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#f59e0b</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline WarningColor(string style)
        {
            ApplyWarningStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the error marker foreground color.
        /// </summary>
        /// <param name="color">The error marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ErrorColor(ConsoleColor color)
        {
            ApplyErrorStyle(Scheme(color), Decoration.None, null, null);
            return this;
        }

        /// <summary>
        /// Sets the error marker foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The error marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ErrorColor(SpectreColor color)
        {
            ApplyErrorStyle(Scheme(color), Decoration.None, color.ToRgbColor(), null);
            return this;
        }

        /// <summary>
        /// Sets the error marker foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The error marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ErrorColor(RgbColor color)
        {
            ApplyErrorStyle(Scheme(color), Decoration.None, color, null);
            return this;
        }

        /// <summary>
        /// Sets the error marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ErrorColor(Style style)
        {
            ApplyErrorStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the error marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#ef4444</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ErrorColor(string style)
        {
            ApplyErrorStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the skipped marker foreground color.
        /// </summary>
        /// <param name="color">The skipped marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline SkippedColor(ConsoleColor color)
        {
            ApplySkippedStyle(Scheme(color), Decoration.None, null, null);
            return this;
        }

        /// <summary>
        /// Sets the skipped marker foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The skipped marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline SkippedColor(SpectreColor color)
        {
            ApplySkippedStyle(Scheme(color), Decoration.None, color.ToRgbColor(), null);
            return this;
        }

        /// <summary>
        /// Sets the skipped marker foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The skipped marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline SkippedColor(RgbColor color)
        {
            ApplySkippedStyle(Scheme(color), Decoration.None, color, null);
            return this;
        }

        /// <summary>
        /// Sets the skipped marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline SkippedColor(Style style)
        {
            ApplySkippedStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the skipped marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline SkippedColor(string style)
        {
            ApplySkippedStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the connector foreground color.
        /// </summary>
        /// <param name="color">The connector foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ConnectorColor(ConsoleColor color)
        {
            Options.ConnectorScheme = Scheme(color);
            Options.ConnectorDecoration = Decoration.None;
            Options.ConnectorForegroundRgb = null;
            Options.ConnectorBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the connector foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The connector foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ConnectorColor(SpectreColor color)
        {
            Options.ConnectorScheme = Scheme(color);
            Options.ConnectorDecoration = Decoration.None;
            Options.ConnectorForegroundRgb = color.ToRgbColor();
            Options.ConnectorBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the connector foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The connector foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ConnectorColor(RgbColor color)
        {
            Options.ConnectorScheme = Scheme(color);
            Options.ConnectorDecoration = Decoration.None;
            Options.ConnectorForegroundRgb = color;
            Options.ConnectorBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the connector color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ConnectorColor(Style style)
        {
            ApplyConnectorStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the connector color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ConnectorColor(string style)
        {
            ApplyConnectorStyle(Style.Parse(style));
            return this;
        }

        private void ApplyTitleStyle(Style style)
        {
            Options.TitleScheme = Scheme(style, Options.ResolveTitleScheme());
            Options.TitleDecoration = style?.Decoration ?? Decoration.None;
            Options.TitleForegroundRgb = style?.ForegroundRgb;
            Options.TitleBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyDescriptionStyle(Style style)
        {
            Options.DescriptionScheme = Scheme(style, Options.ResolveDescriptionScheme());
            Options.DescriptionDecoration = style?.Decoration ?? Decoration.None;
            Options.DescriptionForegroundRgb = style?.ForegroundRgb;
            Options.DescriptionBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyPendingStyle(Style style)
        {
            ApplyPendingStyle(Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Pending)), style?.Decoration ?? Decoration.None, style?.ForegroundRgb, style?.BackgroundRgb);
        }

        private void ApplyPendingStyle(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            Options.PendingScheme = scheme;
            Options.PendingDecoration = decoration;
            Options.PendingForegroundRgb = foregroundRgb;
            Options.PendingBackgroundRgb = backgroundRgb;
        }

        private void ApplyActiveStyle(Style style)
        {
            ApplyActiveStyle(Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Active)), style?.Decoration ?? Decoration.None, style?.ForegroundRgb, style?.BackgroundRgb);
        }

        private void ApplyActiveStyle(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            Options.ActiveScheme = scheme;
            Options.ActiveDecoration = decoration;
            Options.ActiveForegroundRgb = foregroundRgb;
            Options.ActiveBackgroundRgb = backgroundRgb;
        }

        private void ApplyCompleteStyle(Style style)
        {
            ApplyCompleteStyle(Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Complete)), style?.Decoration ?? Decoration.None, style?.ForegroundRgb, style?.BackgroundRgb);
        }

        private void ApplyCompleteStyle(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            Options.CompleteScheme = scheme;
            Options.CompleteDecoration = decoration;
            Options.CompleteForegroundRgb = foregroundRgb;
            Options.CompleteBackgroundRgb = backgroundRgb;
        }

        private void ApplyWarningStyle(Style style)
        {
            ApplyWarningStyle(Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Warning)), style?.Decoration ?? Decoration.None, style?.ForegroundRgb, style?.BackgroundRgb);
        }

        private void ApplyWarningStyle(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            Options.WarningScheme = scheme;
            Options.WarningDecoration = decoration;
            Options.WarningForegroundRgb = foregroundRgb;
            Options.WarningBackgroundRgb = backgroundRgb;
        }

        private void ApplyErrorStyle(Style style)
        {
            ApplyErrorStyle(Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Error)), style?.Decoration ?? Decoration.None, style?.ForegroundRgb, style?.BackgroundRgb);
        }

        private void ApplyErrorStyle(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            Options.ErrorScheme = scheme;
            Options.ErrorDecoration = decoration;
            Options.ErrorForegroundRgb = foregroundRgb;
            Options.ErrorBackgroundRgb = backgroundRgb;
        }

        private void ApplySkippedStyle(Style style)
        {
            ApplySkippedStyle(Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Skipped)), style?.Decoration ?? Decoration.None, style?.ForegroundRgb, style?.BackgroundRgb);
        }

        private void ApplySkippedStyle(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            Options.SkippedScheme = scheme;
            Options.SkippedDecoration = decoration;
            Options.SkippedForegroundRgb = foregroundRgb;
            Options.SkippedBackgroundRgb = backgroundRgb;
        }

        private void ApplyConnectorStyle(Style style)
        {
            Options.ConnectorScheme = Scheme(style, Options.ResolveConnectorScheme());
            Options.ConnectorDecoration = style?.Decoration ?? Decoration.None;
            Options.ConnectorForegroundRgb = style?.ForegroundRgb;
            Options.ConnectorBackgroundRgb = style?.BackgroundRgb;
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
