using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a progress display column requested by migration-friendly progress builders.
    /// </summary>
    public abstract class ProgressColumn
    {
    }

    /// <summary>
    /// Requests task description text in a progress display.
    /// </summary>
    public sealed class TaskDescriptionColumn : ProgressColumn
    {
    }

    /// <summary>
    /// Requests a progress bar in a progress display.
    /// </summary>
    public sealed class ProgressBarColumn : ProgressColumn
    {
        /// <summary>
        /// Gets or sets the preferred width of the bar body, excluding the surrounding brackets.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the style used for completed bar cells.
        /// </summary>
        public Style CompleteStyle
        {
            get => CompletedStyle;
            set => CompletedStyle = value;
        }

        /// <summary>
        /// Gets or sets the style used for completed bar cells.
        /// </summary>
        public Style CompletedStyle { get; set; }

        /// <summary>
        /// Gets or sets the style used for finished bar cells.
        /// </summary>
        public Style FinishedStyle { get; set; }

        /// <summary>
        /// Gets or sets the style used for remaining bar cells.
        /// </summary>
        public Style RemainingStyle { get; set; }

        /// <summary>
        /// Gets or sets the style used for indeterminate bar cells.
        /// </summary>
        public Style IndeterminateStyle { get; set; }

        /// <summary>
        /// Sets the style used for completed bar cells.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn Completed(Style style)
        {
            CompletedStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for completed bar cells.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn CompletedColor(ConsoleColor color)
        {
            return Completed(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for completed bar cells from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn CompletedColor(SpectreColor color)
        {
            return Completed(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for completed bar cells from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn CompletedColor(RgbColor color)
        {
            return Completed(new Style(color));
        }

        /// <summary>
        /// Sets the style used for completed bar cells.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn Complete(Style style)
        {
            return Completed(style);
        }

        /// <summary>
        /// Sets the foreground color used for completed bar cells.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn CompleteColor(ConsoleColor color)
        {
            return CompletedColor(color);
        }

        /// <summary>
        /// Sets the foreground color used for completed bar cells from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn CompleteColor(SpectreColor color)
        {
            return CompletedColor(color);
        }

        /// <summary>
        /// Sets the foreground color used for completed bar cells from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn CompleteColor(RgbColor color)
        {
            return CompletedColor(color);
        }

        /// <summary>
        /// Sets the style used for finished bar cells.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn Finished(Style style)
        {
            FinishedStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for finished bar cells.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn FinishedColor(ConsoleColor color)
        {
            return Finished(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for finished bar cells from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn FinishedColor(SpectreColor color)
        {
            return Finished(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for finished bar cells from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn FinishedColor(RgbColor color)
        {
            return Finished(new Style(color));
        }

        /// <summary>
        /// Sets the style used for remaining bar cells.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn Remaining(Style style)
        {
            RemainingStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for remaining bar cells.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn RemainingColor(ConsoleColor color)
        {
            return Remaining(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for remaining bar cells from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn RemainingColor(SpectreColor color)
        {
            return Remaining(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for remaining bar cells from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn RemainingColor(RgbColor color)
        {
            return Remaining(new Style(color));
        }

        /// <summary>
        /// Sets the style used for indeterminate bar cells.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn Indeterminate(Style style)
        {
            IndeterminateStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for indeterminate bar cells.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn IndeterminateColor(ConsoleColor color)
        {
            return Indeterminate(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for indeterminate bar cells from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn IndeterminateColor(SpectreColor color)
        {
            return Indeterminate(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for indeterminate bar cells from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current progress bar column.</returns>
        public ProgressBarColumn IndeterminateColor(RgbColor color)
        {
            return Indeterminate(new Style(color));
        }
    }

    /// <summary>
    /// Requests percentage text in a progress display.
    /// </summary>
    public sealed class PercentageColumn : ProgressColumn
    {
    }

    /// <summary>
    /// Requests remaining-time text in a progress display.
    /// </summary>
    public sealed class RemainingTimeColumn : ProgressColumn
    {
    }

    /// <summary>
    /// Requests elapsed-time text in a progress display.
    /// </summary>
    public sealed class ElapsedTimeColumn : ProgressColumn
    {
    }

    /// <summary>
    /// Requests spinner text in a progress display.
    /// </summary>
    public sealed class SpinnerColumn : ProgressColumn
    {
        /// <summary>
        /// Gets or sets the spinner frame set.
        /// </summary>
        public Spinner Spinner { get; set; }

        /// <summary>
        /// Gets or sets the style used for the spinner.
        /// </summary>
        public Style Style { get; set; }

        /// <summary>
        /// Sets the style used for the spinner.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current spinner column.</returns>
        public SpinnerColumn SpinnerStyle(Style style)
        {
            Style = style;
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for the spinner.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current spinner column.</returns>
        public SpinnerColumn SpinnerStyle(ConsoleColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for the spinner from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current spinner column.</returns>
        public SpinnerColumn SpinnerStyle(SpectreColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for the spinner from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current spinner column.</returns>
        public SpinnerColumn SpinnerStyle(RgbColor color)
        {
            return SpinnerStyle(new Style(color));
        }
    }

    /// <summary>
    /// Requests literal or template text in a progress display.
    /// </summary>
    public sealed class TextColumn : ProgressColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextColumn"/> class.
        /// </summary>
        public TextColumn() : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextColumn"/> class.
        /// </summary>
        /// <param name="text">The literal text or task template to render.</param>
        public TextColumn(string text)
        {
            Text = text ?? string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextColumn"/> class.
        /// </summary>
        /// <param name="text">The literal text or task template to render.</param>
        /// <param name="style">The default style used when the text does not contain explicit markup.</param>
        public TextColumn(string text, Style style) : this(text)
        {
            Style = style;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextColumn"/> class.
        /// </summary>
        /// <param name="text">The literal text or task template to render.</param>
        /// <param name="color">The foreground color used when the text does not contain explicit markup.</param>
        public TextColumn(string text, ConsoleColor color) : this(text, new Style(color))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextColumn"/> class.
        /// </summary>
        /// <param name="text">The literal text or task template to render.</param>
        /// <param name="color">Color facade mapped to the nearest console color for text without explicit markup.</param>
        public TextColumn(string text, SpectreColor color) : this(text, new Style(color))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextColumn"/> class.
        /// </summary>
        /// <param name="text">The literal text or task template to render.</param>
        /// <param name="color">RGB color mapped to the nearest console color for text without explicit markup.</param>
        public TextColumn(string text, RgbColor color) : this(text, new Style(color))
        {
        }

        /// <summary>
        /// Gets or sets the literal text or task template to render.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the preferred width of the text column.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the default style used when the text does not contain explicit markup.
        /// </summary>
        public Style Style { get; set; }

        /// <summary>
        /// Sets the default style used when the text does not contain explicit markup.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current text column.</returns>
        public TextColumn TextStyle(Style style)
        {
            Style = style;
            return this;
        }

        /// <summary>
        /// Sets the foreground color used when the text does not contain explicit markup.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current text column.</returns>
        public TextColumn TextColor(ConsoleColor color)
        {
            return TextStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used when the text does not contain explicit markup from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current text column.</returns>
        public TextColumn TextColor(SpectreColor color)
        {
            return TextStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used when the text does not contain explicit markup from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current text column.</returns>
        public TextColumn TextColor(RgbColor color)
        {
            return TextStyle(new Style(color));
        }
    }

    /// <summary>
    /// Requests downloaded-byte text in a progress display.
    /// </summary>
    public sealed class DownloadedColumn : ProgressColumn
    {
    }

    /// <summary>
    /// Requests transfer-speed text in a progress display.
    /// </summary>
    public sealed class TransferSpeedColumn : ProgressColumn
    {
    }

    /// <summary>
    /// Represents a callback that can replace a progress renderable before rendering.
    /// </summary>
    /// <param name="renderable">The generated renderable.</param>
    /// <param name="tasks">The current progress tasks.</param>
    /// <returns>The renderable to write.</returns>
    public delegate IConsoulRenderable ProgressRenderHook(IConsoulRenderable renderable, System.Collections.Generic.IReadOnlyList<ProgressTask> tasks);
}
