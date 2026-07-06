using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a compact responsive trend chart for console dashboards and model summaries.
    /// </summary>
    public class SparklineEntry : IRenderable
    {
        private readonly List<double> _values = new List<double>();
        private readonly SparklineOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparklineEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the sparkline.</param>
        public SparklineEntry(SparklineOptions options = null)
        {
            _options = options != null ? options.Clone() : new SparklineOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparklineEntry"/> class with values.
        /// </summary>
        /// <param name="values">The values rendered by the sparkline.</param>
        /// <param name="options">Optional rendering options for the sparkline.</param>
        public SparklineEntry(IEnumerable<double> values, SparklineOptions options = null) : this(options)
        {
            AddRange(values);
        }

        /// <summary>
        /// Gets the values rendered by the sparkline.
        /// </summary>
        public IList<double> Values => _values;

        /// <summary>
        /// Gets the options used to render the sparkline.
        /// </summary>
        public SparklineOptions Options => _options;

        /// <summary>
        /// Adds a value to the sparkline.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The current <see cref="SparklineEntry"/> instance.</returns>
        public SparklineEntry Add(double value)
        {
            if (!double.IsNaN(value) && !double.IsInfinity(value))
            {
                _values.Add(value);
            }

            return this;
        }

        /// <summary>
        /// Adds values to the sparkline.
        /// </summary>
        /// <param name="values">The values to add.</param>
        /// <returns>The current <see cref="SparklineEntry"/> instance.</returns>
        public SparklineEntry AddRange(IEnumerable<double> values)
        {
            if (values == null)
            {
                return this;
            }

            foreach (double value in values)
            {
                Add(value);
            }

            return this;
        }

        /// <summary>
        /// Renders the sparkline to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_values.Count == 0)
            {
                return;
            }

            int width = PanelEntry.ResolveWidth(_options.Width);
            Layout layout = ResolveLayout(width);
            if (layout.LabelWidth > 0)
            {
                ColorScheme labelScheme = _options.ResolveLabelScheme();
                MarkupRenderText.WriteSegments(PadOrTrimSegments(_options.Label, layout.LabelWidth, labelScheme), labelScheme, false);
                Consoul.Write(" ", RenderOptions.DefaultScheme, false);
            }

            Consoul.Write(BuildLine(layout.LineWidth), _options.ResolveLineScheme(), !layout.ShowRange);

            if (layout.ShowRange)
            {
                Consoul.Write(" ", RenderOptions.DefaultScheme, false);
                Consoul.Write(PadOrTrim(BuildRangeText(), layout.RangeWidth), _options.ResolveRangeScheme(), true);
            }
        }

        /// <summary>
        /// Renders a temporary sparkline to the active console driver.
        /// </summary>
        /// <param name="values">The values rendered by the sparkline.</param>
        /// <param name="options">Optional rendering options for the sparkline.</param>
        public static void Render(IEnumerable<double> values, SparklineOptions options = null)
        {
            new SparklineEntry(values, options).Render();
        }

        private Layout ResolveLayout(int width)
        {
            string label = _options.Label ?? string.Empty;
            int visibleLabelLength = MarkupRenderText.GetPlainText(NormalizeInline(label)).Length;
            int labelWidth = string.IsNullOrEmpty(label)
                ? 0
                : _options.LabelWidth ?? Math.Min(visibleLabelLength, Math.Max(4, width / 4));
            labelWidth = Math.Max(0, Math.Min(labelWidth, Math.Max(0, width - 1)));

            string rangeText = BuildRangeText();
            int rangeWidth = _options.ShowRange ? rangeText.Length : 0;
            bool showRange = _options.ShowRange && rangeWidth > 0;

            while (showRange && labelWidth + 1 + 1 + 1 + rangeWidth > width)
            {
                rangeWidth--;
                if (rangeWidth <= 0)
                {
                    showRange = false;
                    rangeWidth = 0;
                }
            }

            while (labelWidth > 0 && labelWidth + 1 + 1 + (showRange ? 1 + rangeWidth : 0) > width)
            {
                labelWidth--;
            }

            int lineWidth = width - (labelWidth > 0 ? labelWidth + 1 : 0) - (showRange ? rangeWidth + 1 : 0);
            if (lineWidth < 1)
            {
                lineWidth = 1;
                labelWidth = Math.Max(0, width - 1);
                showRange = false;
                rangeWidth = 0;
            }

            return new Layout(labelWidth, lineWidth, showRange, rangeWidth);
        }

        private string BuildLine(int width)
        {
            string characters = _options.ResolveCharacters();
            double minimum = ResolveMinimum();
            double maximum = ResolveMaximum(minimum);
            var output = new char[Math.Max(1, width)];

            for (int index = 0; index < output.Length; index++)
            {
                double value = Sample(index, output.Length);
                output[index] = Scale(value, minimum, maximum, characters);
            }

            return new string(output);
        }

        private double Sample(int index, int width)
        {
            if (_values.Count == 1 || width <= 1)
            {
                return _values[_values.Count - 1];
            }

            double position = index * (_values.Count - 1.0) / (width - 1.0);
            int valueIndex = (int)Math.Round(position, MidpointRounding.AwayFromZero);
            valueIndex = Math.Max(0, Math.Min(_values.Count - 1, valueIndex));
            return _values[valueIndex];
        }

        private static char Scale(double value, double minimum, double maximum, string characters)
        {
            if (characters.Length == 1 || maximum <= minimum)
            {
                return characters[characters.Length - 1];
            }

            double ratio = (value - minimum) / (maximum - minimum);
            ratio = Math.Max(0, Math.Min(1, ratio));
            int charIndex = (int)Math.Round(ratio * (characters.Length - 1), MidpointRounding.AwayFromZero);
            charIndex = Math.Max(0, Math.Min(characters.Length - 1, charIndex));
            return characters[charIndex];
        }

        private double ResolveMinimum()
        {
            return _options.MinimumValue ?? _values.Min();
        }

        private double ResolveMaximum(double minimum)
        {
            double maximum = _options.MaximumValue ?? _values.Max();
            return maximum < minimum ? minimum : maximum;
        }

        private string BuildRangeText()
        {
            double minimum = ResolveMinimum();
            double maximum = ResolveMaximum(minimum);
            return _options.FormatValue(minimum) + ".." + _options.FormatValue(maximum);
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(string text, int width, ColorScheme scheme)
        {
            width = Math.Max(0, width);
            List<FixedMessageSegment> result = MarkupRenderText.TruncateSegments(
                MarkupRenderText.ParseSegments(NormalizeInline(text), scheme),
                width);
            MarkupRenderText.AddPadding(result, Math.Max(0, width - MarkupRenderText.GetLength(result)), scheme);
            return result;
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

        private static string NormalizeInline(string text)
        {
            return (text ?? string.Empty)
                .Replace('\r', ' ')
                .Replace('\n', ' ');
        }

        private readonly struct Layout
        {
            public Layout(int labelWidth, int lineWidth, bool showRange, int rangeWidth)
            {
                LabelWidth = labelWidth;
                LineWidth = lineWidth;
                ShowRange = showRange;
                RangeWidth = rangeWidth;
            }

            public int LabelWidth { get; }

            public int LineWidth { get; }

            public bool ShowRange { get; }

            public int RangeWidth { get; }
        }
    }
}
