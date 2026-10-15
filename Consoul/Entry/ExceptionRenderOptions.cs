using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for structured exception output.
    /// </summary>
    public class ExceptionRenderOptions
    {
        /// <summary>
        /// Gets or sets migration-friendly exception formatting flags.
        /// </summary>
        public ExceptionFormats Format { get; set; } = ExceptionFormats.Default;

        /// <summary>
        /// Gets or sets the requested exception panel width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the title rendered in the exception panel border.
        /// </summary>
        public string Title { get; set; } = "Exception";

        /// <summary>
        /// Gets or sets a value indicating whether stack trace lines should be rendered.
        /// </summary>
        public bool IncludeStackTrace { get; set; } = true;

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

        /// <summary>
        /// Gets or sets the maximum number of stack frames rendered per exception. A null value renders every frame.
        /// </summary>
        public int? MaxStackFrames { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the panel border.
        /// </summary>
        public ColorScheme? BorderScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the panel title.
        /// </summary>
        public ColorScheme? TitleScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for optional context text.
        /// </summary>
        public ColorScheme? ContextScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for exception type lines.
        /// </summary>
        public ColorScheme? TypeScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for exception message lines.
        /// </summary>
        public ColorScheme? MessageScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for stack trace lines.
        /// </summary>
        public ColorScheme? StackTraceScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current exception rendering options.
        /// </summary>
        /// <returns>A new <see cref="ExceptionRenderOptions"/> instance with the same values.</returns>
        public ExceptionRenderOptions Clone()
        {
            return new ExceptionRenderOptions
            {
                Format = Format,
                Width = Width,
                Title = Title,
                IncludeStackTrace = IncludeStackTrace,
                IncludeInnerExceptions = IncludeInnerExceptions,
                FlattenAggregateExceptions = FlattenAggregateExceptions,
                MaxAggregateExceptions = MaxAggregateExceptions,
                MaxStackFrames = MaxStackFrames,
                BorderScheme = BorderScheme,
                TitleScheme = TitleScheme,
                ContextScheme = ContextScheme,
                TypeScheme = TypeScheme,
                MessageScheme = MessageScheme,
                StackTraceScheme = StackTraceScheme
            };
        }

        internal ColorScheme ResolveBorderScheme()
        {
            return BorderScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveTitleScheme()
        {
            return TitleScheme ?? RenderOptions.InvalidScheme;
        }

        internal ColorScheme ResolveContextScheme()
        {
            return ContextScheme ?? RenderOptions.DefaultScheme;
        }

        internal ColorScheme ResolveTypeScheme()
        {
            return TypeScheme ?? RenderOptions.InvalidScheme;
        }

        internal ColorScheme ResolveMessageScheme()
        {
            return MessageScheme ?? RenderOptions.InvalidScheme;
        }

        internal ColorScheme ResolveStackTraceScheme()
        {
            return StackTraceScheme ?? RenderOptions.SubnoteScheme;
        }
    }
}
