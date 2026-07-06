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
                AddStep(
                    titleSelector(item),
                    stateSelector != null ? stateSelector(item) : TimelineItemState.Pending,
                    descriptionSelector != null ? descriptionSelector(item) : null,
                    timestampSelector != null ? timestampSelector(item) : null,
                    ResolveColor(item, titleColorSelector),
                    ResolveColor(item, markerColorSelector),
                    ResolveColor(item, descriptionColorSelector));
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
                AddStep(
                    titleSelector(item),
                    stateSelector != null ? stateSelector(item) : TimelineItemState.Pending,
                    descriptionSelector != null ? descriptionSelector(item) : null,
                    timestampSelector != null ? timestampSelector(item) : null,
                    ResolveColor(item, titleColorSelector),
                    ResolveColor(item, markerColorSelector),
                    ResolveColor(item, descriptionColorSelector));
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
            return this;
        }

        /// <summary>
        /// Sets the default title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline TitleColor(Style style)
        {
            Options.TitleScheme = Scheme(style, Options.ResolveTitleScheme());
            return this;
        }

        /// <summary>
        /// Sets the default title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline TitleColor(string style)
        {
            Options.TitleScheme = Style.ParseScheme(style, Options.ResolveTitleScheme());
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
            return this;
        }

        /// <summary>
        /// Sets the description color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline DescriptionColor(Style style)
        {
            Options.DescriptionScheme = Scheme(style, Options.ResolveDescriptionScheme());
            return this;
        }

        /// <summary>
        /// Sets the description color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline DescriptionColor(string style)
        {
            Options.DescriptionScheme = Style.ParseScheme(style, Options.ResolveDescriptionScheme());
            return this;
        }

        /// <summary>
        /// Sets the pending marker foreground color.
        /// </summary>
        /// <param name="color">The pending marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline PendingColor(ConsoleColor color)
        {
            Options.PendingScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the pending marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline PendingColor(Style style)
        {
            Options.PendingScheme = Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Pending));
            return this;
        }

        /// <summary>
        /// Sets the pending marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline PendingColor(string style)
        {
            Options.PendingScheme = Style.ParseScheme(style, Options.ResolveMarkerScheme(TimelineItemState.Pending));
            return this;
        }

        /// <summary>
        /// Sets the active marker foreground color.
        /// </summary>
        /// <param name="color">The active marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ActiveColor(ConsoleColor color)
        {
            Options.ActiveScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the active marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ActiveColor(Style style)
        {
            Options.ActiveScheme = Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Active));
            return this;
        }

        /// <summary>
        /// Sets the active marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ActiveColor(string style)
        {
            Options.ActiveScheme = Style.ParseScheme(style, Options.ResolveMarkerScheme(TimelineItemState.Active));
            return this;
        }

        /// <summary>
        /// Sets the completed marker foreground color.
        /// </summary>
        /// <param name="color">The completed marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline CompleteColor(ConsoleColor color)
        {
            Options.CompleteScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the completed marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline CompleteColor(Style style)
        {
            Options.CompleteScheme = Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Complete));
            return this;
        }

        /// <summary>
        /// Sets the completed marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline CompleteColor(string style)
        {
            Options.CompleteScheme = Style.ParseScheme(style, Options.ResolveMarkerScheme(TimelineItemState.Complete));
            return this;
        }

        /// <summary>
        /// Sets the warning marker foreground color.
        /// </summary>
        /// <param name="color">The warning marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline WarningColor(ConsoleColor color)
        {
            Options.WarningScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the warning marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline WarningColor(Style style)
        {
            Options.WarningScheme = Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Warning));
            return this;
        }

        /// <summary>
        /// Sets the warning marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#f59e0b</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline WarningColor(string style)
        {
            Options.WarningScheme = Style.ParseScheme(style, Options.ResolveMarkerScheme(TimelineItemState.Warning));
            return this;
        }

        /// <summary>
        /// Sets the error marker foreground color.
        /// </summary>
        /// <param name="color">The error marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ErrorColor(ConsoleColor color)
        {
            Options.ErrorScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the error marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ErrorColor(Style style)
        {
            Options.ErrorScheme = Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Error));
            return this;
        }

        /// <summary>
        /// Sets the error marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#ef4444</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ErrorColor(string style)
        {
            Options.ErrorScheme = Style.ParseScheme(style, Options.ResolveMarkerScheme(TimelineItemState.Error));
            return this;
        }

        /// <summary>
        /// Sets the skipped marker foreground color.
        /// </summary>
        /// <param name="color">The skipped marker foreground color.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline SkippedColor(ConsoleColor color)
        {
            Options.SkippedScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the skipped marker color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline SkippedColor(Style style)
        {
            Options.SkippedScheme = Scheme(style, Options.ResolveMarkerScheme(TimelineItemState.Skipped));
            return this;
        }

        /// <summary>
        /// Sets the skipped marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline SkippedColor(string style)
        {
            Options.SkippedScheme = Style.ParseScheme(style, Options.ResolveMarkerScheme(TimelineItemState.Skipped));
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
            return this;
        }

        /// <summary>
        /// Sets the connector color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ConnectorColor(Style style)
        {
            Options.ConnectorScheme = Scheme(style, Options.ResolveConnectorScheme());
            return this;
        }

        /// <summary>
        /// Sets the connector color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Timeline"/> instance.</returns>
        public Timeline ConnectorColor(string style)
        {
            Options.ConnectorScheme = Style.ParseScheme(style, Options.ResolveConnectorScheme());
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

        private static ColorScheme Scheme(Style style, ColorScheme fallback)
        {
            return (style ?? Style.Plain).ToColorScheme(fallback);
        }
    }
}
