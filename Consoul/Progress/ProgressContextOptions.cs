using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a multi-task progress context.
    /// </summary>
    public class ProgressContextOptions
    {
        private ProgressColumn[] _columns;

        /// <summary>
        /// Gets or sets the maximum row width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the preferred label width for each progress row.
        /// </summary>
        public int LabelWidth { get; set; } = 18;

        /// <summary>
        /// Gets or sets the minimum number of characters reserved for the bar.
        /// </summary>
        public int MinimumBarWidth { get; set; } = 4;

        /// <summary>
        /// Gets or sets a value indicating whether a percentage should render after each bar.
        /// </summary>
        public bool ShowPercentage { get; set; } = true;

        /// <summary>
        /// Gets or sets the character used for completed bar units.
        /// </summary>
        public char CompletedCharacter { get; set; } = '#';

        /// <summary>
        /// Gets or sets the character used for remaining bar units.
        /// </summary>
        public char RemainingCharacter { get; set; } = '-';

        /// <summary>
        /// Gets or sets the minimum time between rendered updates in milliseconds.
        /// </summary>
        public int RefreshRate { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether progress task updates render immediately.
        /// </summary>
        public bool AutoRefresh { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether completed task rows should be cleared when the context is disposed.
        /// </summary>
        public bool AutoClear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether completed task rows should be hidden after completion.
        /// </summary>
        public bool HideCompleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether vertical padding around the progress display should be omitted.
        /// </summary>
        public bool ExcludeVerticalPadding { get; set; }

        /// <summary>
        /// Gets or sets the requested progress columns for migration-friendly progress builders.
        /// </summary>
        public ProgressColumn[] Columns
        {
            get => _columns?.ToArray();
            set => _columns = value == null ? null : value.Where(column => column != null).ToArray();
        }

        /// <summary>
        /// Gets or sets an optional render hook that can replace each generated progress row.
        /// </summary>
        public ProgressRenderHook RenderHook { get; set; }

        /// <summary>
        /// Gets or sets the color used for active progress rows.
        /// </summary>
        public ConsoleColor ActiveColor { get; set; } = RenderOptions.SubnoteColor;

        /// <summary>
        /// Gets or sets optional Spectre-style active row foreground/background and decoration metadata.
        /// </summary>
        public Style ActiveStyle { get; set; }

        /// <summary>
        /// Gets or sets the color used for completed progress rows.
        /// </summary>
        public ConsoleColor CompleteColor { get; set; } = ConsoleColor.Green;

        /// <summary>
        /// Gets or sets optional Spectre-style completed row foreground/background and decoration metadata.
        /// </summary>
        public Style CompleteStyle { get; set; }

        /// <summary>
        /// Gets or sets the color used for failed progress rows.
        /// </summary>
        public ConsoleColor FailedColor { get; set; } = ConsoleColor.Red;

        /// <summary>
        /// Gets or sets optional Spectre-style failed row foreground/background and decoration metadata.
        /// </summary>
        public Style FailedStyle { get; set; }

        /// <summary>
        /// Creates a copy of the current progress context options.
        /// </summary>
        /// <returns>A new <see cref="ProgressContextOptions"/> instance with the same values.</returns>
        public ProgressContextOptions Clone()
        {
            return new ProgressContextOptions
            {
                Width = Width,
                LabelWidth = LabelWidth,
                MinimumBarWidth = MinimumBarWidth,
                ShowPercentage = ShowPercentage,
                CompletedCharacter = CompletedCharacter,
                RemainingCharacter = RemainingCharacter,
                RefreshRate = RefreshRate,
                AutoRefresh = AutoRefresh,
                AutoClear = AutoClear,
                HideCompleted = HideCompleted,
                ExcludeVerticalPadding = ExcludeVerticalPadding,
                Columns = Columns,
                RenderHook = RenderHook,
                ActiveColor = ActiveColor,
                ActiveStyle = ActiveStyle,
                CompleteColor = CompleteColor,
                CompleteStyle = CompleteStyle,
                FailedColor = FailedColor,
                FailedStyle = FailedStyle
            };
        }

        /// <summary>
        /// Gets the configured progress columns.
        /// </summary>
        /// <returns>The configured columns, or an empty list when no columns were configured.</returns>
        public IReadOnlyList<ProgressColumn> GetColumns()
        {
            return _columns ?? Array.Empty<ProgressColumn>();
        }

        internal int ResolveLabelWidth()
        {
            return Math.Max(0, LabelWidth);
        }

        internal int ResolveMinimumBarWidth()
        {
            return Math.Max(1, MinimumBarWidth);
        }
    }
}
