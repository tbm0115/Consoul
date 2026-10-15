using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent metrics construction over <see cref="MetricsEntry"/>.
    /// </summary>
    public class Metrics : MetricsEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Metrics"/> class.
        /// </summary>
        public Metrics()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metrics"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the metrics.</param>
        public Metrics(MetricsOptions options) : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metrics"/> class with items.
        /// </summary>
        /// <param name="items">The metric items to render.</param>
        /// <param name="options">Optional rendering options for the metrics.</param>
        public Metrics(IEnumerable<MetricItem> items, MetricsOptions options = null) : base(items, options)
        {
        }

        /// <summary>
        /// Adds a metric item and returns the current metrics builder for fluent snippets.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="unit">Optional unit text rendered after the value.</param>
        /// <param name="note">Optional note rendered below the value.</param>
        /// <param name="kind">The visual intent used for the value.</param>
        /// <param name="labelColor">Optional foreground color used for the label.</param>
        /// <param name="valueColor">Optional foreground color used for the value.</param>
        /// <param name="noteColor">Optional foreground color used for the note.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public new Metrics Add(
            string label,
            object value,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default,
            ConsoleColor? labelColor = null,
            ConsoleColor? valueColor = null,
            ConsoleColor? noteColor = null)
        {
            base.Add(label, value, unit, note, kind, labelColor, valueColor, noteColor);
            return this;
        }

        /// <summary>
        /// Adds a metric item using reusable style objects and returns the current metrics builder.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="labelStyle">Optional style used for the label.</param>
        /// <param name="valueStyle">Optional style used for the value.</param>
        /// <param name="noteStyle">Optional style used for the note.</param>
        /// <param name="unit">Optional unit text rendered after the value.</param>
        /// <param name="note">Optional note rendered below the value.</param>
        /// <param name="kind">The visual intent used for the value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public new Metrics Add(
            string label,
            object value,
            Style labelStyle,
            Style valueStyle = null,
            Style noteStyle = null,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default)
        {
            base.Add(label, value, labelStyle, valueStyle, noteStyle, unit, note, kind);
            return this;
        }

        /// <summary>
        /// Adds an existing metric item and returns the current metrics builder.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public new Metrics Add(MetricItem item)
        {
            base.Add(item);
            return this;
        }

        /// <summary>
        /// Adds existing metric items and returns the current metrics builder.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics Add(IEnumerable<MetricItem> items)
        {
            return AddMetrics(items);
        }

        /// <summary>
        /// Adds existing metric items and returns the current metrics builder.
        /// </summary>
        /// <param name="firstItem">The first item to add.</param>
        /// <param name="additionalItems">Additional items to add.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics Add(MetricItem firstItem, params MetricItem[] additionalItems)
        {
            return AddMetrics(firstItem, additionalItems);
        }

        /// <summary>
        /// Adds metric items projected from a model collection and returns the current metrics builder.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into metrics.</param>
        /// <param name="labelSelector">Selects the metric label.</param>
        /// <param name="valueSelector">Selects the metric value.</param>
        /// <param name="unitSelector">Optionally selects the unit text rendered after the value.</param>
        /// <param name="noteSelector">Optionally selects the note rendered below the value.</param>
        /// <param name="kindSelector">Optionally selects the visual intent for the metric value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics Add<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, string> unitSelector = null,
            Func<T, string> noteSelector = null,
            Func<T, MetricKind> kindSelector = null)
        {
            return AddMetrics(items, labelSelector, valueSelector, unitSelector, noteSelector, kindSelector);
        }

        /// <summary>
        /// Adds metric items projected from a model collection using Spectre-style colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into metrics.</param>
        /// <param name="labelSelector">Selects the metric label.</param>
        /// <param name="valueSelector">Selects the metric value.</param>
        /// <param name="labelColorSelector">Selects a Spectre-style color for the metric label.</param>
        /// <param name="valueColorSelector">Optionally selects a Spectre-style color for the metric value.</param>
        /// <param name="noteColorSelector">Optionally selects a Spectre-style color for the metric note.</param>
        /// <param name="unitSelector">Optionally selects the unit text rendered after the value.</param>
        /// <param name="noteSelector">Optionally selects the note rendered below the value.</param>
        /// <param name="kindSelector">Optionally selects the visual intent for the metric value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics Add<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, SpectreColor> labelColorSelector,
            Func<T, SpectreColor> valueColorSelector = null,
            Func<T, SpectreColor> noteColorSelector = null,
            Func<T, string> unitSelector = null,
            Func<T, string> noteSelector = null,
            Func<T, MetricKind> kindSelector = null)
        {
            return AddMetrics(
                items,
                labelSelector,
                valueSelector,
                labelColorSelector,
                valueColorSelector,
                noteColorSelector,
                unitSelector,
                noteSelector,
                kindSelector);
        }

        /// <summary>
        /// Adds metric items projected from a model collection using RGB colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into metrics.</param>
        /// <param name="labelSelector">Selects the metric label.</param>
        /// <param name="valueSelector">Selects the metric value.</param>
        /// <param name="labelColorSelector">Selects an RGB color for the metric label.</param>
        /// <param name="valueColorSelector">Optionally selects an RGB color for the metric value.</param>
        /// <param name="noteColorSelector">Optionally selects an RGB color for the metric note.</param>
        /// <param name="unitSelector">Optionally selects the unit text rendered after the value.</param>
        /// <param name="noteSelector">Optionally selects the note rendered below the value.</param>
        /// <param name="kindSelector">Optionally selects the visual intent for the metric value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics Add<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, RgbColor> labelColorSelector,
            Func<T, RgbColor> valueColorSelector = null,
            Func<T, RgbColor> noteColorSelector = null,
            Func<T, string> unitSelector = null,
            Func<T, string> noteSelector = null,
            Func<T, MetricKind> kindSelector = null)
        {
            return AddMetrics(
                items,
                labelSelector,
                valueSelector,
                labelColorSelector,
                valueColorSelector,
                noteColorSelector,
                unitSelector,
                noteSelector,
                kindSelector);
        }

        /// <summary>
        /// Adds metric items projected from a model collection and returns the current metrics builder.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into metrics.</param>
        /// <param name="itemSelector">Selects the metric item to add for each model item.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics Add<T>(IEnumerable<T> items, Func<T, MetricItem> itemSelector)
        {
            return AddMetrics(items, itemSelector);
        }

        /// <summary>
        /// Adds a metric item.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="unit">Optional unit text rendered after the value.</param>
        /// <param name="note">Optional note rendered below the value.</param>
        /// <param name="kind">The visual intent used for the value.</param>
        /// <param name="labelColor">Optional foreground color used for the label.</param>
        /// <param name="valueColor">Optional foreground color used for the value.</param>
        /// <param name="noteColor">Optional foreground color used for the note.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddMetric(
            string label,
            object value,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default,
            ConsoleColor? labelColor = null,
            ConsoleColor? valueColor = null,
            ConsoleColor? noteColor = null)
        {
            Add(label, value, unit, note, kind, labelColor, valueColor, noteColor);
            return this;
        }

        /// <summary>
        /// Adds a metric item using reusable style objects.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="labelStyle">Optional style used for the label.</param>
        /// <param name="valueStyle">Optional style used for the value.</param>
        /// <param name="noteStyle">Optional style used for the note.</param>
        /// <param name="unit">Optional unit text rendered after the value.</param>
        /// <param name="note">Optional note rendered below the value.</param>
        /// <param name="kind">The visual intent used for the value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddMetric(
            string label,
            object value,
            Style labelStyle,
            Style valueStyle = null,
            Style noteStyle = null,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default)
        {
            Add(label, value, labelStyle, valueStyle, noteStyle, unit, note, kind);
            return this;
        }

        /// <summary>
        /// Adds a metric item using Spectre-style style text.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="labelStyle">Optional style used for the label, such as <c>grey</c>.</param>
        /// <param name="valueStyle">Optional style used for the value, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="noteStyle">Optional style used for the note, such as <c>darkgray</c>.</param>
        /// <param name="unit">Optional unit text rendered after the value.</param>
        /// <param name="note">Optional note rendered below the value.</param>
        /// <param name="kind">The visual intent used for the value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddMetricStyled(
            string label,
            object value,
            string labelStyle = null,
            string valueStyle = null,
            string noteStyle = null,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default)
        {
            AddStyled(label, value, labelStyle, valueStyle, noteStyle, unit, note, kind);
            return this;
        }

        /// <summary>
        /// Adds an existing metric item.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddMetric(MetricItem item)
        {
            Add(item);
            return this;
        }

        /// <summary>
        /// Adds existing metric items.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddMetrics(IEnumerable<MetricItem> items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (MetricItem item in items)
            {
                AddMetric(item);
            }

            return this;
        }

        /// <summary>
        /// Adds existing metric items.
        /// </summary>
        /// <param name="firstItem">The first item to add.</param>
        /// <param name="additionalItems">Additional items to add.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddMetrics(MetricItem firstItem, params MetricItem[] additionalItems)
        {
            AddMetric(firstItem);

            if (additionalItems != null)
            {
                foreach (MetricItem item in additionalItems)
                {
                    AddMetric(item);
                }
            }

            return this;
        }

        /// <summary>
        /// Adds metric items projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into metrics.</param>
        /// <param name="labelSelector">Selects the metric label.</param>
        /// <param name="valueSelector">Selects the metric value.</param>
        /// <param name="unitSelector">Optionally selects the unit text rendered after the value.</param>
        /// <param name="noteSelector">Optionally selects the note rendered below the value.</param>
        /// <param name="kindSelector">Optionally selects the visual intent for the metric value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddMetrics<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, string> unitSelector = null,
            Func<T, string> noteSelector = null,
            Func<T, MetricKind> kindSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddMetric(
                    labelSelector(item),
                    valueSelector(item),
                    unitSelector != null ? unitSelector(item) : null,
                    noteSelector != null ? noteSelector(item) : null,
                    kindSelector != null ? kindSelector(item) : MetricKind.Default);
            }

            return this;
        }

        /// <summary>
        /// Adds metric items projected from a model collection using Spectre-style colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into metrics.</param>
        /// <param name="labelSelector">Selects the metric label.</param>
        /// <param name="valueSelector">Selects the metric value.</param>
        /// <param name="labelColorSelector">Selects a Spectre-style color for the metric label.</param>
        /// <param name="valueColorSelector">Optionally selects a Spectre-style color for the metric value.</param>
        /// <param name="noteColorSelector">Optionally selects a Spectre-style color for the metric note.</param>
        /// <param name="unitSelector">Optionally selects the unit text rendered after the value.</param>
        /// <param name="noteSelector">Optionally selects the note rendered below the value.</param>
        /// <param name="kindSelector">Optionally selects the visual intent for the metric value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddMetrics<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, SpectreColor> labelColorSelector,
            Func<T, SpectreColor> valueColorSelector = null,
            Func<T, SpectreColor> noteColorSelector = null,
            Func<T, string> unitSelector = null,
            Func<T, string> noteSelector = null,
            Func<T, MetricKind> kindSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
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
                AddMetric(MetricItem.Create(
                    labelSelector(item),
                    valueSelector(item),
                    labelColorSelector(item),
                    valueColorSelector != null ? valueColorSelector(item) : (SpectreColor?)null,
                    noteColorSelector != null ? noteColorSelector(item) : (SpectreColor?)null,
                    unitSelector != null ? unitSelector(item) : null,
                    noteSelector != null ? noteSelector(item) : null,
                    kindSelector != null ? kindSelector(item) : MetricKind.Default));
            }

            return this;
        }

        /// <summary>
        /// Adds metric items projected from a model collection using RGB colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into metrics.</param>
        /// <param name="labelSelector">Selects the metric label.</param>
        /// <param name="valueSelector">Selects the metric value.</param>
        /// <param name="labelColorSelector">Selects an RGB color for the metric label.</param>
        /// <param name="valueColorSelector">Optionally selects an RGB color for the metric value.</param>
        /// <param name="noteColorSelector">Optionally selects an RGB color for the metric note.</param>
        /// <param name="unitSelector">Optionally selects the unit text rendered after the value.</param>
        /// <param name="noteSelector">Optionally selects the note rendered below the value.</param>
        /// <param name="kindSelector">Optionally selects the visual intent for the metric value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddMetrics<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, RgbColor> labelColorSelector,
            Func<T, RgbColor> valueColorSelector = null,
            Func<T, RgbColor> noteColorSelector = null,
            Func<T, string> unitSelector = null,
            Func<T, string> noteSelector = null,
            Func<T, MetricKind> kindSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
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
                AddMetric(MetricItem.Create(
                    labelSelector(item),
                    valueSelector(item),
                    labelColorSelector(item),
                    valueColorSelector != null ? valueColorSelector(item) : (RgbColor?)null,
                    noteColorSelector != null ? noteColorSelector(item) : (RgbColor?)null,
                    unitSelector != null ? unitSelector(item) : null,
                    noteSelector != null ? noteSelector(item) : null,
                    kindSelector != null ? kindSelector(item) : MetricKind.Default));
            }

            return this;
        }

        /// <summary>
        /// Adds metric items projected from a model collection using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into metrics.</param>
        /// <param name="labelSelector">Selects the metric label.</param>
        /// <param name="valueSelector">Selects the metric value.</param>
        /// <param name="unitSelector">Optionally selects the unit text rendered after the value.</param>
        /// <param name="noteSelector">Optionally selects the note rendered below the value.</param>
        /// <param name="kindSelector">Optionally selects the visual intent for the metric value.</param>
        /// <param name="labelStyleSelector">Optionally selects style text for the metric label, such as <c>grey</c>.</param>
        /// <param name="valueStyleSelector">Optionally selects style text for the metric value, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="noteStyleSelector">Optionally selects style text for the metric note, such as <c>darkgray</c>.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddMetricsStyled<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, string> unitSelector = null,
            Func<T, string> noteSelector = null,
            Func<T, MetricKind> kindSelector = null,
            Func<T, string> labelStyleSelector = null,
            Func<T, string> valueStyleSelector = null,
            Func<T, string> noteStyleSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddStyled(
                    labelSelector(item),
                    valueSelector(item),
                    labelStyleSelector != null ? labelStyleSelector(item) : null,
                    valueStyleSelector != null ? valueStyleSelector(item) : null,
                    noteStyleSelector != null ? noteStyleSelector(item) : null,
                    unitSelector != null ? unitSelector(item) : null,
                    noteSelector != null ? noteSelector(item) : null,
                    kindSelector != null ? kindSelector(item) : MetricKind.Default);
            }

            return this;
        }

        /// <summary>
        /// Adds metric items projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into metrics.</param>
        /// <param name="itemSelector">Selects the metric item to add for each model item.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddMetrics<T>(IEnumerable<T> items, Func<T, MetricItem> itemSelector)
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
                AddMetric(itemSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds an informational metric item.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="unit">Optional unit text rendered after the value.</param>
        /// <param name="note">Optional note rendered below the value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddInfo(string label, object value, string unit = null, string note = null)
        {
            return AddMetric(label, value, unit, note, MetricKind.Info);
        }

        /// <summary>
        /// Adds a successful or healthy metric item.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="unit">Optional unit text rendered after the value.</param>
        /// <param name="note">Optional note rendered below the value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddSuccess(string label, object value, string unit = null, string note = null)
        {
            return AddMetric(label, value, unit, note, MetricKind.Success);
        }

        /// <summary>
        /// Adds a warning metric item.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="unit">Optional unit text rendered after the value.</param>
        /// <param name="note">Optional note rendered below the value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddWarning(string label, object value, string unit = null, string note = null)
        {
            return AddMetric(label, value, unit, note, MetricKind.Warning);
        }

        /// <summary>
        /// Adds an error or unhealthy metric item.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="unit">Optional unit text rendered after the value.</param>
        /// <param name="note">Optional note rendered below the value.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics AddError(string label, object value, string unit = null, string note = null)
        {
            return AddMetric(label, value, unit, note, MetricKind.Error);
        }

        /// <summary>
        /// Sets the requested metrics width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the metrics to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics Expand()
        {
            Options.Width = null;
            return this;
        }

        /// <summary>
        /// Sets the requested number of metric columns.
        /// </summary>
        /// <param name="columns">The requested column count.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ColumnCount(int? columns)
        {
            Options.Columns = columns;
            return this;
        }

        /// <summary>
        /// Sets the minimum preferred width for each metric column.
        /// </summary>
        /// <param name="width">The minimum preferred column width.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics MinimumColumnWidth(int width)
        {
            Options.MinimumColumnWidth = width;
            return this;
        }

        /// <summary>
        /// Sets the number of spaces written between metric columns.
        /// </summary>
        /// <param name="gap">The column gap.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ColumnGap(int gap)
        {
            Options.ColumnGap = gap;
            return this;
        }

        /// <summary>
        /// Shows or hides metric notes.
        /// </summary>
        /// <param name="show">Whether notes should render.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ShowNotes(bool show = true)
        {
            Options.ShowNotes = show;
            return this;
        }

        /// <summary>
        /// Hides metric notes.
        /// </summary>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics HideNotes()
        {
            Options.ShowNotes = false;
            return this;
        }

        /// <summary>
        /// Sets the default metric label foreground color.
        /// </summary>
        /// <param name="color">The label foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics LabelColor(ConsoleColor color)
        {
            Options.LabelScheme = Scheme(color);
            Options.LabelDecoration = Decoration.None;
            Options.LabelForegroundRgb = null;
            Options.LabelBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default metric label foreground color.
        /// </summary>
        /// <param name="color">The label foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics LabelColor(SpectreColor color)
        {
            Options.LabelScheme = Scheme(color);
            Options.LabelDecoration = Decoration.None;
            Options.LabelForegroundRgb = color.ToRgbColor();
            Options.LabelBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default metric label foreground color.
        /// </summary>
        /// <param name="color">The label foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics LabelColor(RgbColor color)
        {
            Options.LabelScheme = Scheme(color);
            Options.LabelDecoration = Decoration.None;
            Options.LabelForegroundRgb = color;
            Options.LabelBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default metric label color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics LabelColor(Style style)
        {
            ApplyLabelStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the default metric label color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics LabelColor(string style)
        {
            ApplyLabelStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the default metric value foreground color.
        /// </summary>
        /// <param name="color">The value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ValueColor(ConsoleColor color)
        {
            Options.ValueScheme = Scheme(color);
            Options.ValueDecoration = Decoration.None;
            Options.ValueForegroundRgb = null;
            Options.ValueBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default metric value foreground color.
        /// </summary>
        /// <param name="color">The value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ValueColor(SpectreColor color)
        {
            Options.ValueScheme = Scheme(color);
            Options.ValueDecoration = Decoration.None;
            Options.ValueForegroundRgb = color.ToRgbColor();
            Options.ValueBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default metric value foreground color.
        /// </summary>
        /// <param name="color">The value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ValueColor(RgbColor color)
        {
            Options.ValueScheme = Scheme(color);
            Options.ValueDecoration = Decoration.None;
            Options.ValueForegroundRgb = color;
            Options.ValueBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default metric value color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ValueColor(Style style)
        {
            ApplyValueStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the default metric value color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ValueColor(string style)
        {
            ApplyValueStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the metric note foreground color.
        /// </summary>
        /// <param name="color">The note foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics NoteColor(ConsoleColor color)
        {
            Options.NoteScheme = Scheme(color);
            Options.NoteDecoration = Decoration.None;
            Options.NoteForegroundRgb = null;
            Options.NoteBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the metric note foreground color.
        /// </summary>
        /// <param name="color">The note foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics NoteColor(SpectreColor color)
        {
            Options.NoteScheme = Scheme(color);
            Options.NoteDecoration = Decoration.None;
            Options.NoteForegroundRgb = color.ToRgbColor();
            Options.NoteBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the metric note foreground color.
        /// </summary>
        /// <param name="color">The note foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics NoteColor(RgbColor color)
        {
            Options.NoteScheme = Scheme(color);
            Options.NoteDecoration = Decoration.None;
            Options.NoteForegroundRgb = color;
            Options.NoteBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the metric note color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics NoteColor(Style style)
        {
            ApplyNoteStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the metric note color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics NoteColor(string style)
        {
            ApplyNoteStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the informational metric value foreground color.
        /// </summary>
        /// <param name="color">The informational value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics InfoColor(ConsoleColor color)
        {
            Options.InfoScheme = Scheme(color);
            Options.InfoDecoration = Decoration.None;
            Options.InfoForegroundRgb = null;
            Options.InfoBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the informational metric value foreground color.
        /// </summary>
        /// <param name="color">The informational value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics InfoColor(SpectreColor color)
        {
            Options.InfoScheme = Scheme(color);
            Options.InfoDecoration = Decoration.None;
            Options.InfoForegroundRgb = color.ToRgbColor();
            Options.InfoBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the informational metric value foreground color.
        /// </summary>
        /// <param name="color">The informational value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics InfoColor(RgbColor color)
        {
            Options.InfoScheme = Scheme(color);
            Options.InfoDecoration = Decoration.None;
            Options.InfoForegroundRgb = color;
            Options.InfoBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the informational metric value color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics InfoColor(Style style)
        {
            ApplyInfoStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the informational metric value color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics InfoColor(string style)
        {
            ApplyInfoStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the successful metric value foreground color.
        /// </summary>
        /// <param name="color">The successful value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics SuccessColor(ConsoleColor color)
        {
            Options.SuccessScheme = Scheme(color);
            Options.SuccessDecoration = Decoration.None;
            Options.SuccessForegroundRgb = null;
            Options.SuccessBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the successful metric value foreground color.
        /// </summary>
        /// <param name="color">The successful value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics SuccessColor(SpectreColor color)
        {
            Options.SuccessScheme = Scheme(color);
            Options.SuccessDecoration = Decoration.None;
            Options.SuccessForegroundRgb = color.ToRgbColor();
            Options.SuccessBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the successful metric value foreground color.
        /// </summary>
        /// <param name="color">The successful value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics SuccessColor(RgbColor color)
        {
            Options.SuccessScheme = Scheme(color);
            Options.SuccessDecoration = Decoration.None;
            Options.SuccessForegroundRgb = color;
            Options.SuccessBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the successful metric value color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics SuccessColor(Style style)
        {
            ApplySuccessStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the successful metric value color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics SuccessColor(string style)
        {
            ApplySuccessStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the warning metric value foreground color.
        /// </summary>
        /// <param name="color">The warning value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics WarningColor(ConsoleColor color)
        {
            Options.WarningScheme = Scheme(color);
            Options.WarningDecoration = Decoration.None;
            Options.WarningForegroundRgb = null;
            Options.WarningBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the warning metric value foreground color.
        /// </summary>
        /// <param name="color">The warning value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics WarningColor(SpectreColor color)
        {
            Options.WarningScheme = Scheme(color);
            Options.WarningDecoration = Decoration.None;
            Options.WarningForegroundRgb = color.ToRgbColor();
            Options.WarningBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the warning metric value foreground color.
        /// </summary>
        /// <param name="color">The warning value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics WarningColor(RgbColor color)
        {
            Options.WarningScheme = Scheme(color);
            Options.WarningDecoration = Decoration.None;
            Options.WarningForegroundRgb = color;
            Options.WarningBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the warning metric value color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics WarningColor(Style style)
        {
            ApplyWarningStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the warning metric value color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#f59e0b</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics WarningColor(string style)
        {
            ApplyWarningStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the error metric value foreground color.
        /// </summary>
        /// <param name="color">The error value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ErrorColor(ConsoleColor color)
        {
            Options.ErrorScheme = Scheme(color);
            Options.ErrorDecoration = Decoration.None;
            Options.ErrorForegroundRgb = null;
            Options.ErrorBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the error metric value foreground color.
        /// </summary>
        /// <param name="color">The error value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ErrorColor(SpectreColor color)
        {
            Options.ErrorScheme = Scheme(color);
            Options.ErrorDecoration = Decoration.None;
            Options.ErrorForegroundRgb = color.ToRgbColor();
            Options.ErrorBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the error metric value foreground color.
        /// </summary>
        /// <param name="color">The error value foreground color.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ErrorColor(RgbColor color)
        {
            Options.ErrorScheme = Scheme(color);
            Options.ErrorDecoration = Decoration.None;
            Options.ErrorForegroundRgb = color;
            Options.ErrorBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the error metric value color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ErrorColor(Style style)
        {
            ApplyErrorStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the error metric value color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#ef4444</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Metrics"/> instance.</returns>
        public Metrics ErrorColor(string style)
        {
            ApplyErrorStyle(Style.Parse(style));
            return this;
        }

        private void ApplyLabelStyle(Style style)
        {
            Options.LabelScheme = Scheme(style, Options.ResolveLabelScheme());
            Options.LabelDecoration = style?.Decoration ?? Decoration.None;
            Options.LabelForegroundRgb = style?.ForegroundRgb;
            Options.LabelBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyValueStyle(Style style)
        {
            Options.ValueScheme = Scheme(style, Options.ResolveValueScheme(MetricKind.Default));
            Options.ValueDecoration = style?.Decoration ?? Decoration.None;
            Options.ValueForegroundRgb = style?.ForegroundRgb;
            Options.ValueBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyNoteStyle(Style style)
        {
            Options.NoteScheme = Scheme(style, Options.ResolveNoteScheme());
            Options.NoteDecoration = style?.Decoration ?? Decoration.None;
            Options.NoteForegroundRgb = style?.ForegroundRgb;
            Options.NoteBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyInfoStyle(Style style)
        {
            Options.InfoScheme = Scheme(style, Options.ResolveValueScheme(MetricKind.Info));
            Options.InfoDecoration = style?.Decoration ?? Decoration.None;
            Options.InfoForegroundRgb = style?.ForegroundRgb;
            Options.InfoBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplySuccessStyle(Style style)
        {
            Options.SuccessScheme = Scheme(style, Options.ResolveValueScheme(MetricKind.Success));
            Options.SuccessDecoration = style?.Decoration ?? Decoration.None;
            Options.SuccessForegroundRgb = style?.ForegroundRgb;
            Options.SuccessBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyWarningStyle(Style style)
        {
            Options.WarningScheme = Scheme(style, Options.ResolveValueScheme(MetricKind.Warning));
            Options.WarningDecoration = style?.Decoration ?? Decoration.None;
            Options.WarningForegroundRgb = style?.ForegroundRgb;
            Options.WarningBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyErrorStyle(Style style)
        {
            Options.ErrorScheme = Scheme(style, Options.ResolveValueScheme(MetricKind.Error));
            Options.ErrorDecoration = style?.Decoration ?? Decoration.None;
            Options.ErrorForegroundRgb = style?.ForegroundRgb;
            Options.ErrorBackgroundRgb = style?.BackgroundRgb;
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
