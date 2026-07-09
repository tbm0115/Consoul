using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style exception rendering settings that adapt to Consoul's responsive exception panel.
    /// </summary>
    public class ExceptionSettings
    {
        /// <summary>
        /// Gets or sets the exception formatting flags.
        /// </summary>
        public ExceptionFormats Format { get; set; } = ExceptionFormats.Default;

        /// <summary>
        /// Gets or sets the style used by exception sections.
        /// </summary>
        public ExceptionStyle Style { get; set; }

        /// <summary>
        /// Gets or sets the requested exception panel width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the title rendered in the exception panel border.
        /// </summary>
        public string Title { get; set; } = "Exception";

        /// <summary>
        /// Gets or sets the maximum number of stack frames rendered per exception. A null value renders every frame.
        /// </summary>
        public int? MaxStackFrames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inner exceptions should be rendered.
        /// </summary>
        public bool IncludeInnerExceptions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether aggregate exceptions should render flattened child exceptions.
        /// </summary>
        public bool FlattenAggregateExceptions { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of flattened aggregate child exceptions rendered. A null value renders every child exception.
        /// </summary>
        public int? MaxAggregateExceptions { get; set; }

        internal ExceptionRenderOptions ToRenderOptions()
        {
            var options = new ExceptionRenderOptions
            {
                Format = Format,
                Width = Width,
                Title = Title,
                MaxStackFrames = MaxStackFrames,
                IncludeInnerExceptions = IncludeInnerExceptions,
                FlattenAggregateExceptions = FlattenAggregateExceptions,
                MaxAggregateExceptions = MaxAggregateExceptions,
                IncludeStackTrace = !Format.HasFlag(ExceptionFormats.NoStackTrace)
            };

            if (Style == null)
            {
                return options;
            }

            options.TypeScheme = ToScheme(Style.Exception, RenderOptions.InvalidScheme);
            options.MessageScheme = ToScheme(Style.Message, RenderOptions.InvalidScheme);
            options.StackTraceScheme = ToScheme(
                Style.Method ?? Style.Path ?? Style.LineNumber,
                RenderOptions.SubnoteScheme);
            return options;
        }

        private static ColorScheme? ToScheme(Style style, ColorScheme fallback)
        {
            return style == null ? (ColorScheme?)null : style.ToColorScheme(fallback);
        }
    }
}
