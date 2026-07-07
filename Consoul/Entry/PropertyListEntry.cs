using ConsoulLibrary.Color;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using System.Text;
using ConsoulLibrary.Views.Editing;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive list of labeled values for model state and settings views.
    /// </summary>
    public class PropertyListEntry : IRenderable
    {
        private readonly List<PropertyListItem> _items = new List<PropertyListItem>();
        private readonly PropertyListOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyListEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the property list.</param>
        public PropertyListEntry(PropertyListOptions options = null)
        {
            _options = options != null ? options.Clone() : new PropertyListOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyListEntry"/> class with items.
        /// </summary>
        /// <param name="items">The property list items to render.</param>
        /// <param name="options">Optional rendering options for the property list.</param>
        public PropertyListEntry(IEnumerable<PropertyListItem> items, PropertyListOptions options = null) : this(options)
        {
            if (items == null)
            {
                return;
            }

            foreach (PropertyListItem item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the items rendered by the property list.
        /// </summary>
        public IList<PropertyListItem> Items => _items;

        /// <summary>
        /// Gets the options used to render the property list.
        /// </summary>
        public PropertyListOptions Options => _options;

        /// <summary>
        /// Adds a labeled value to the property list.
        /// </summary>
        /// <param name="label">The label rendered before the value.</param>
        /// <param name="value">The value rendered for the label.</param>
        /// <param name="valueColor">Optional foreground color used for the value.</param>
        /// <param name="labelColor">Optional foreground color used for the label.</param>
        /// <returns>The added property list item.</returns>
        public PropertyListItem Add(string label, object value, ConsoleColor? valueColor = null, ConsoleColor? labelColor = null)
        {
            var item = PropertyListItem.Create(label, value, valueColor, labelColor);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a labeled value to the property list using Spectre-style style text.
        /// </summary>
        /// <param name="label">The label rendered before the value.</param>
        /// <param name="value">The value rendered for the label.</param>
        /// <param name="valueStyle">Optional style used for the value, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="labelStyle">Optional style used for the label, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The added property list item.</returns>
        public PropertyListItem Add(string label, object value, string valueStyle, string labelStyle = null)
        {
            var item = PropertyListItem.Create(label, value, valueStyle, labelStyle);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a labeled value to the property list using reusable styles.
        /// </summary>
        /// <param name="label">The label rendered before the value.</param>
        /// <param name="value">The value rendered for the label.</param>
        /// <param name="valueStyle">Optional style used for the value.</param>
        /// <param name="labelStyle">Optional style used for the label.</param>
        /// <returns>The added property list item.</returns>
        public PropertyListItem Add(string label, object value, Style valueStyle, Style labelStyle = null)
        {
            var item = PropertyListItem.Create(label, value, valueStyle, labelStyle);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the property list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The added property list item.</returns>
        public PropertyListItem Add(PropertyListItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds key/value pairs as labeled values for quick settings and diagnostic summaries.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="values">The key/value pairs to append.</param>
        /// <param name="valueColor">Optional foreground color used for generated values.</param>
        /// <param name="labelColor">Optional foreground color used for generated labels.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddKeyValues<TKey, TValue>(
            IEnumerable<KeyValuePair<TKey, TValue>> values,
            ConsoleColor? valueColor = null,
            ConsoleColor? labelColor = null)
        {
            foreach (KeyValuePair<TKey, TValue> pair in values ?? new KeyValuePair<TKey, TValue>[0])
            {
                Add(FormatModelValue(pair.Key), FormatModelValue(pair.Value), valueColor, labelColor);
            }

            return this;
        }

        /// <summary>
        /// Adds key/value pairs as labeled values using Spectre-style style text.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="values">The key/value pairs to append.</param>
        /// <param name="valueStyle">Optional style used for generated values, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="labelStyle">Optional style used for generated labels, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddKeyValues<TKey, TValue>(
            IEnumerable<KeyValuePair<TKey, TValue>> values,
            string valueStyle,
            string labelStyle = null)
        {
            foreach (KeyValuePair<TKey, TValue> pair in values ?? new KeyValuePair<TKey, TValue>[0])
            {
                Add(FormatModelValue(pair.Key), FormatModelValue(pair.Value), valueStyle, labelStyle);
            }

            return this;
        }

        /// <summary>
        /// Adds key/value pairs as labeled values using reusable styles.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="values">The key/value pairs to append.</param>
        /// <param name="valueStyle">Optional style used for generated values.</param>
        /// <param name="labelStyle">Optional style used for generated labels.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddKeyValues<TKey, TValue>(
            IEnumerable<KeyValuePair<TKey, TValue>> values,
            Style valueStyle,
            Style labelStyle = null)
        {
            foreach (KeyValuePair<TKey, TValue> pair in values ?? new KeyValuePair<TKey, TValue>[0])
            {
                Add(FormatModelValue(pair.Key), FormatModelValue(pair.Value), valueStyle, labelStyle);
            }

            return this;
        }

        /// <summary>
        /// Adds labeled values projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into labeled values.</param>
        /// <param name="labelSelector">Selects the label rendered before the value.</param>
        /// <param name="valueSelector">Selects the value rendered for the label.</param>
        /// <param name="valueColorSelector">Optionally selects the foreground color used for each value.</param>
        /// <param name="labelColorSelector">Optionally selects the foreground color used for each label.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, ConsoleColor?> valueColorSelector = null,
            Func<T, ConsoleColor?> labelColorSelector = null)
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
                Add(
                    labelSelector(item),
                    valueSelector(item),
                    valueColorSelector != null ? valueColorSelector(item) : null,
                    labelColorSelector != null ? labelColorSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds labeled values projected from a model collection with Spectre-style value and label colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into labeled values.</param>
        /// <param name="labelSelector">Selects the label rendered before the value.</param>
        /// <param name="valueSelector">Selects the value rendered for the label.</param>
        /// <param name="valueColorSelector">Selects the foreground color used for each value.</param>
        /// <param name="labelColorSelector">Optionally selects the foreground color used for each label.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, SpectreColor> valueColorSelector,
            Func<T, SpectreColor> labelColorSelector = null)
        {
            if (valueColorSelector == null)
            {
                throw new ArgumentNullException(nameof(valueColorSelector));
            }

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
                Add(PropertyListItem.Create(
                    labelSelector(item),
                    valueSelector(item),
                    valueColorSelector(item),
                    labelColorSelector != null ? labelColorSelector(item) : (SpectreColor?)null));
            }

            return this;
        }

        /// <summary>
        /// Adds labeled values projected from a model collection with RGB value and label colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into labeled values.</param>
        /// <param name="labelSelector">Selects the label rendered before the value.</param>
        /// <param name="valueSelector">Selects the value rendered for the label.</param>
        /// <param name="valueColorSelector">Selects the foreground color used for each value.</param>
        /// <param name="labelColorSelector">Optionally selects the foreground color used for each label.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, RgbColor> valueColorSelector,
            Func<T, RgbColor> labelColorSelector = null)
        {
            if (valueColorSelector == null)
            {
                throw new ArgumentNullException(nameof(valueColorSelector));
            }

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
                Add(PropertyListItem.Create(
                    labelSelector(item),
                    valueSelector(item),
                    valueColorSelector(item),
                    labelColorSelector != null ? labelColorSelector(item) : (RgbColor?)null));
            }

            return this;
        }

        /// <summary>
        /// Adds labeled values projected from a model collection using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into labeled values.</param>
        /// <param name="labelSelector">Selects the label rendered before the value.</param>
        /// <param name="valueSelector">Selects the value rendered for the label.</param>
        /// <param name="valueStyleSelector">Optionally selects style text used for each value, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="labelStyleSelector">Optionally selects style text used for each label, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddItemsStyled<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, string> valueStyleSelector = null,
            Func<T, string> labelStyleSelector = null)
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
                Add(
                    labelSelector(item),
                    valueSelector(item),
                    valueStyleSelector != null ? valueStyleSelector(item) : null,
                    labelStyleSelector != null ? labelStyleSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds labeled values projected from a model collection using reusable style objects.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into labeled values.</param>
        /// <param name="labelSelector">Selects the label rendered before the value.</param>
        /// <param name="valueSelector">Selects the value rendered for the label.</param>
        /// <param name="valueStyleSelector">Optionally selects the style used for each value.</param>
        /// <param name="labelStyleSelector">Optionally selects the style used for each label.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddItemsStyled<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, object> valueSelector,
            Func<T, Style> valueStyleSelector,
            Func<T, Style> labelStyleSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            if (valueStyleSelector == null)
            {
                throw new ArgumentNullException(nameof(valueStyleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(
                    labelSelector(item),
                    valueSelector(item),
                    valueStyleSelector(item),
                    labelStyleSelector != null ? labelStyleSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds entries from a non-generic dictionary as labeled values for quick settings and diagnostic summaries.
        /// </summary>
        /// <param name="values">The dictionary entries to append.</param>
        /// <param name="valueColor">Optional foreground color used for generated values.</param>
        /// <param name="labelColor">Optional foreground color used for generated labels.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddDictionary(
            IDictionary values,
            ConsoleColor? valueColor = null,
            ConsoleColor? labelColor = null)
        {
            if (values == null)
            {
                return this;
            }

            foreach (DictionaryEntry entry in values)
            {
                Add(FormatModelValue(entry.Key), FormatModelValue(entry.Value), valueColor, labelColor);
            }

            return this;
        }

        /// <summary>
        /// Adds entries from a non-generic dictionary as labeled values using Spectre-style style text.
        /// </summary>
        /// <param name="values">The dictionary entries to append.</param>
        /// <param name="valueStyle">Optional style used for generated values, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="labelStyle">Optional style used for generated labels, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddDictionary(
            IDictionary values,
            string valueStyle,
            string labelStyle = null)
        {
            if (values == null)
            {
                return this;
            }

            foreach (DictionaryEntry entry in values)
            {
                Add(FormatModelValue(entry.Key), FormatModelValue(entry.Value), valueStyle, labelStyle);
            }

            return this;
        }

        /// <summary>
        /// Adds readable scalar properties from a model as labeled values.
        /// </summary>
        /// <param name="model">The model object whose properties should be summarized.</param>
        /// <param name="bindingAttr">Binding flags used to discover properties.</param>
        /// <param name="valueColor">Optional foreground color used for generated values.</param>
        /// <param name="labelColor">Optional foreground color used for generated labels.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddModel(
            object model,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance,
            ConsoleColor? valueColor = null,
            ConsoleColor? labelColor = null)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            foreach (PropertyInfo property in GetModelProperties(model.GetType(), bindingAttr))
            {
                Add(ResolvePropertyLabel(property), FormatModelValue(property.GetValue(model)), valueColor, labelColor);
            }

            return this;
        }

        /// <summary>
        /// Adds readable scalar properties from a model as labeled values using Spectre-style style text.
        /// </summary>
        /// <param name="model">The model object whose properties should be summarized.</param>
        /// <param name="valueStyle">Optional style used for generated values, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="labelStyle">Optional style used for generated labels, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddModel(object model, string valueStyle, string labelStyle = null)
        {
            return AddModel(model, BindingFlags.Public | BindingFlags.Instance, valueStyle, labelStyle);
        }

        /// <summary>
        /// Adds readable scalar properties from a model as labeled values using reusable styles.
        /// </summary>
        /// <param name="model">The model object whose properties should be summarized.</param>
        /// <param name="valueStyle">Optional style used for generated values.</param>
        /// <param name="labelStyle">Optional style used for generated labels.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddModel(object model, Style valueStyle, Style labelStyle = null)
        {
            return AddModel(model, BindingFlags.Public | BindingFlags.Instance, valueStyle, labelStyle);
        }

        /// <summary>
        /// Adds readable scalar properties from a model as labeled values using Spectre-style style text.
        /// </summary>
        /// <param name="model">The model object whose properties should be summarized.</param>
        /// <param name="bindingAttr">Binding flags used to discover properties.</param>
        /// <param name="valueStyle">Optional style used for generated values, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="labelStyle">Optional style used for generated labels, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddModel(
            object model,
            BindingFlags bindingAttr,
            string valueStyle,
            string labelStyle = null)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            foreach (PropertyInfo property in GetModelProperties(model.GetType(), bindingAttr))
            {
                Add(ResolvePropertyLabel(property), FormatModelValue(property.GetValue(model)), valueStyle, labelStyle);
            }

            return this;
        }

        /// <summary>
        /// Adds readable scalar properties from a model as labeled values using reusable styles.
        /// </summary>
        /// <param name="model">The model object whose properties should be summarized.</param>
        /// <param name="bindingAttr">Binding flags used to discover properties.</param>
        /// <param name="valueStyle">Optional style used for generated values.</param>
        /// <param name="labelStyle">Optional style used for generated labels.</param>
        /// <returns>The current <see cref="PropertyListEntry"/> instance.</returns>
        public PropertyListEntry AddModel(
            object model,
            BindingFlags bindingAttr,
            Style valueStyle,
            Style labelStyle = null)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            foreach (PropertyInfo property in GetModelProperties(model.GetType(), bindingAttr))
            {
                Add(ResolvePropertyLabel(property), FormatModelValue(property.GetValue(model)), valueStyle, labelStyle);
            }

            return this;
        }

        /// <summary>
        /// Renders the property list to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_items.Count == 0)
            {
                return;
            }

            int width = PanelEntry.ResolveWidth(_options.Width);
            Layout layout = ResolveLayout(width);
            foreach (PropertyListItem item in _items)
            {
                if (layout.TwoColumn)
                {
                    RenderTwoColumn(item, layout);
                }
                else
                {
                    RenderStacked(item, width);
                }
            }
        }

        /// <summary>
        /// Renders a temporary property list to the active console driver.
        /// </summary>
        /// <param name="items">The property list items to render.</param>
        /// <param name="options">Optional rendering options for the property list.</param>
        public static void Render(IEnumerable<PropertyListItem> items, PropertyListOptions options = null)
        {
            new PropertyListEntry(items, options).Render();
        }

        private Layout ResolveLayout(int width)
        {
            string separator = _options.Separator ?? string.Empty;
            int separatorWidth = Math.Min(separator.Length, Math.Max(0, width - 2));
            int longestLabel = _items.Select(item => MarkupRenderText.GetPlainText(item.Label).Length).DefaultIfEmpty(0).Max();
            int desiredLabelWidth = _options.LabelWidth ?? Math.Min(longestLabel, Math.Max(4, width / 3));
            desiredLabelWidth = Math.Max(0, desiredLabelWidth);
            int labelWidth = Math.Min(desiredLabelWidth, Math.Max(0, width - separatorWidth - 1));
            int valueWidth = width - labelWidth - separatorWidth;
            bool twoColumn = labelWidth > 0 && valueWidth > 0;
            return new Layout(twoColumn, labelWidth, separator.Substring(0, separatorWidth), valueWidth);
        }

        private void RenderTwoColumn(PropertyListItem item, Layout layout)
        {
            StyledText labelStyle = ResolveLabelStyle(item);
            StyledText valueStyle = ResolveValueStyle(item);
            ColorScheme separatorScheme = _options.ResolveSeparatorScheme();
            IList<List<FixedMessageSegment>> valueLines = MarkupRenderText.WrapSegments(
                item.Value,
                layout.ValueWidth,
                valueStyle.Scheme,
                valueStyle.Decoration,
                valueStyle.ForegroundRgb,
                valueStyle.BackgroundRgb);

            List<FixedMessageSegment> label = PadOrTrimSegments(item.Label, layout.LabelWidth, labelStyle);
            string separator = PadOrTrim(layout.Separator, layout.Separator.Length);
            for (int index = 0; index < valueLines.Count; index++)
            {
                if (index == 0)
                {
                    MarkupRenderText.WriteSegments(label, labelStyle.Scheme, false);
                    Consoul.Write(separator, separatorScheme, false);
                }
                else
                {
                    string prefix = _options.IndentWrappedValues
                        ? new string(' ', layout.LabelWidth + layout.Separator.Length)
                        : string.Empty;
                    Consoul.Write(prefix, RenderOptions.DefaultScheme, false);
                }

                MarkupRenderText.WriteSegments(PadOrTrimSegments(valueLines[index], layout.ValueWidth, valueStyle), valueStyle.Scheme, true);
            }
        }

        private void RenderStacked(PropertyListItem item, int width)
        {
            StyledText labelStyle = ResolveLabelStyle(item);
            StyledText valueStyle = ResolveValueStyle(item);

            foreach (List<FixedMessageSegment> labelLine in MarkupRenderText.WrapSegments(
                item.Label,
                width,
                labelStyle.Scheme,
                labelStyle.Decoration,
                labelStyle.ForegroundRgb,
                labelStyle.BackgroundRgb))
            {
                MarkupRenderText.WriteSegments(PadOrTrimSegments(labelLine, width, labelStyle), labelStyle.Scheme, true);
            }

            foreach (List<FixedMessageSegment> valueLine in MarkupRenderText.WrapSegments(
                item.Value,
                width,
                valueStyle.Scheme,
                valueStyle.Decoration,
                valueStyle.ForegroundRgb,
                valueStyle.BackgroundRgb))
            {
                MarkupRenderText.WriteSegments(PadOrTrimSegments(valueLine, width, valueStyle), valueStyle.Scheme, true);
            }
        }

        private StyledText ResolveLabelStyle(PropertyListItem item)
        {
            ColorScheme scheme = item.LabelScheme ?? _options.ResolveLabelScheme();
            bool hasItemScheme = item.LabelScheme.HasValue;
            return new StyledText(
                scheme,
                item.LabelDecoration ?? (hasItemScheme ? Decoration.None : _options.LabelDecoration),
                item.LabelForegroundRgb ?? (hasItemScheme ? null : _options.LabelForegroundRgb),
                item.LabelBackgroundRgb ?? (hasItemScheme ? null : _options.LabelBackgroundRgb));
        }

        private StyledText ResolveValueStyle(PropertyListItem item)
        {
            ColorScheme scheme = item.ValueScheme ?? _options.ResolveValueScheme();
            bool hasItemScheme = item.ValueScheme.HasValue;
            return new StyledText(
                scheme,
                item.ValueDecoration ?? (hasItemScheme ? Decoration.None : _options.ValueDecoration),
                item.ValueForegroundRgb ?? (hasItemScheme ? null : _options.ValueForegroundRgb),
                item.ValueBackgroundRgb ?? (hasItemScheme ? null : _options.ValueBackgroundRgb));
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(string text, int width, StyledText style)
        {
            return PadOrTrimSegments(
                MarkupRenderText.ParseSegments(
                    text,
                    style.Scheme,
                    style.Decoration,
                    style.ForegroundRgb,
                    style.BackgroundRgb),
                width,
                style);
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(IList<FixedMessageSegment> segments, int width, StyledText style)
        {
            width = Math.Max(0, width);
            List<FixedMessageSegment> result = MarkupRenderText.TruncateSegments(segments, width);
            AddPadding(result, Math.Max(0, width - MarkupRenderText.GetLength(result)), style);
            return result;
        }

        private static void AddPadding(IList<FixedMessageSegment> segments, int count, StyledText style)
        {
            if (count <= 0)
            {
                return;
            }

            segments.Add(new FixedMessageSegment(
                new string(' ', count),
                style.Scheme,
                style.Decoration,
                null,
                style.ForegroundRgb,
                style.BackgroundRgb));
        }

        private static string PadOrTrim(string text, int width)
        {
            text = text ?? string.Empty;
            width = Math.Max(0, width);
            if (text.Length > width)
            {
                return text.Substring(0, width);
            }

            return text.PadRight(width);
        }

        private static IEnumerable<PropertyInfo> GetModelProperties(Type type, BindingFlags bindingAttr)
        {
            return type
                .GetProperties(bindingAttr)
                .Where(property =>
                    property.CanRead &&
                    property.GetIndexParameters().Length == 0 &&
                    !PropertyUiMetadata.IsIgnored(property))
                .OrderBy(PropertyUiMetadata.GetOrder);
        }

        private static string ResolvePropertyLabel(PropertyInfo property)
        {
            var documentation = new PropertyDocumentation(property);
            if (!string.IsNullOrWhiteSpace(documentation.DisplayName))
            {
                return documentation.DisplayName;
            }

            if (!string.IsNullOrWhiteSpace(documentation.DisplayDescription))
            {
                return documentation.DisplayDescription;
            }

            return SplitPascalCase(property.Name);
        }

        private static string FormatModelValue(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is bool boolean)
            {
                return boolean ? "Yes" : "No";
            }

            if (value is string text)
            {
                return text;
            }

            if (value is DateTime dateTime)
            {
                return dateTime.ToString("g", CultureInfo.CurrentCulture);
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.ToString("g", CultureInfo.CurrentCulture);
            }

            if (value is TimeSpan timeSpan)
            {
                return timeSpan.ToString();
            }

            if (value is IEnumerable && TryGetCollectionCount(value, out int count))
            {
                return count == 1 ? "1 item" : count.ToString(CultureInfo.CurrentCulture) + " items";
            }

            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.CurrentCulture);
            }

            return value.ToString() ?? string.Empty;
        }

        private static bool TryGetCollectionCount(object value, out int count)
        {
            if (value is ICollection collection)
            {
                count = collection.Count;
                return true;
            }

            PropertyInfo countProperty = value
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(property =>
                    string.Equals(property.Name, "Count", StringComparison.Ordinal) &&
                    property.PropertyType == typeof(int) &&
                    property.GetIndexParameters().Length == 0);

            if (countProperty != null)
            {
                count = (int)countProperty.GetValue(value);
                return true;
            }

            count = 0;
            return false;
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var builder = new StringBuilder(value.Length + 8);
            for (int index = 0; index < value.Length; index++)
            {
                if (index > 0 && ShouldInsertWordBoundary(value, index))
                {
                    builder.Append(' ');
                }

                builder.Append(value[index]);
            }

            return builder.ToString();
        }

        private static bool ShouldInsertWordBoundary(string value, int index)
        {
            char current = value[index];
            char previous = value[index - 1];
            if (char.IsWhiteSpace(current) || char.IsWhiteSpace(previous))
            {
                return false;
            }

            if (char.IsUpper(current) && (char.IsLower(previous) || char.IsDigit(previous)))
            {
                return true;
            }

            if (char.IsUpper(current) &&
                char.IsUpper(previous) &&
                index + 1 < value.Length &&
                char.IsLower(value[index + 1]))
            {
                return true;
            }

            if (char.IsDigit(current) && char.IsLetter(previous))
            {
                return true;
            }

            return char.IsLetter(current) && char.IsDigit(previous);
        }

        private readonly struct Layout
        {
            public Layout(bool twoColumn, int labelWidth, string separator, int valueWidth)
            {
                TwoColumn = twoColumn;
                LabelWidth = labelWidth;
                Separator = separator ?? string.Empty;
                ValueWidth = valueWidth;
            }

            public bool TwoColumn { get; }

            public int LabelWidth { get; }

            public string Separator { get; }

            public int ValueWidth { get; }
        }

        private readonly struct StyledText
        {
            public StyledText(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
            {
                Scheme = scheme;
                Decoration = decoration;
                ForegroundRgb = foregroundRgb;
                BackgroundRgb = backgroundRgb;
            }

            public ColorScheme Scheme { get; }

            public Decoration Decoration { get; }

            public RgbColor? ForegroundRgb { get; }

            public RgbColor? BackgroundRgb { get; }
        }
    }
}
