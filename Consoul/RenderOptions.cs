using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary {

    /// <summary>
    /// Provides global rendering defaults for Consoul output, prompts, routines, and view behavior.
    /// </summary>
    public static class RenderOptions
    {
        /// <summary>
        /// Default color scheme for general the Write method(s)
        /// </summary>
        public static ColorScheme DefaultScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.White,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="DefaultScheme"/>.
        /// </summary>
        public static ConsoleColor DefaultColor => DefaultScheme.Color;

        /// <summary>
        /// Default color scheme of main Prompt messages
        /// </summary>
        public static ColorScheme PromptScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="PromptScheme"/>.
        /// </summary>
        public static ConsoleColor PromptColor => PromptScheme.Color;

        /// <summary>
        /// Default color scheme for supplementary or help text
        /// </summary>
        public static ColorScheme SubnoteScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.Gray,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="SubnoteScheme"/>.
        /// </summary>
        public static ConsoleColor SubnoteColor => SubnoteScheme.Color;

        /// <summary>
        /// Default color for Invalid Operation messages
        /// </summary>
        public static ColorScheme InvalidScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.Red,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="InvalidScheme"/>.
        /// </summary>
        public static ConsoleColor InvalidColor => InvalidScheme.Color;

        /// <summary>
        /// Default color for PromptOptions
        /// </summary>
        public static ColorScheme OptionScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.DarkYellow,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="OptionScheme"/>.
        /// </summary>
        public static ConsoleColor OptionColor => OptionScheme.Color;

        /// <summary>
        /// Default color for rendering Routine input values
        /// </summary>
        public static ColorScheme RoutineInputScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.Cyan,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="RoutineInputScheme"/>.
        /// </summary>
        public static ConsoleColor RoutineInputColor => RoutineInputScheme.Color;

        /// <summary>
        /// Messages colored with this will not be rendered
        /// </summary>
        public static List<ConsoleColor> BlacklistColors { get; set; } = new List<ConsoleColor>();

        /// <summary>
        /// Default Write mode for the library
        /// </summary>
        public static WriteModes WriteMode { get; set; } = WriteModes.WriteAll;

        /// <summary>
        /// Default message for the 'Go Back' selection
        /// </summary>
        public static string DefaultGoBackMessage { get; set; } = "←\tGo Back";

        /// <summary>
        /// Gets or sets the message shown when Consoul waits for the user to continue.
        /// </summary>
        public static string ContinueMessage { get; set; } = "Press enter to continue…";

        /// <summary>
        /// Upon error within a view, Consoul will Wait() after capturing the error and displaying the message.
        /// </summary>
        public static bool WaitOnError { get; set; } = false;

        /// <summary>
        /// Gets or sets how Consoul handles exceptions that occur while rendering a view.
        /// </summary>
        public static ViewErrorModes ViewErrorMode { get; set; } = ViewErrorModes.RenderAndContinue;

        /// <summary>
        /// Gets or sets whether newly created view prompts use arrow-key navigation by default.
        /// </summary>
        public static bool ViewPromptKeyboardNavigationEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether newly created view prompts support filtering by default.
        /// </summary>
        public static bool ViewPromptFilterEnabled { get; set; }

        /// <summary>
        /// Gets or sets the default number of view options rendered per prompt page. Set to <see langword="null"/> to render all options.
        /// </summary>
        public static int? ViewPromptPageSize { get; set; }

        /// <summary>
        /// Gets the active render colors as a reusable theme.
        /// </summary>
        public static ConsoulTheme CurrentTheme => ConsoulTheme.FromRenderOptions();

        /// <summary>
        /// Applies a color theme to the global render options.
        /// </summary>
        /// <param name="theme">The theme to apply.</param>
        public static void ApplyTheme(ConsoulTheme theme)
        {
            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            theme.ApplyToRenderOptions();
        }

        /// <summary>
        /// Applies a color theme until the returned scope is disposed.
        /// </summary>
        /// <param name="theme">The theme to apply while the scope is active.</param>
        /// <returns>A disposable scope that restores the previous theme.</returns>
        public static IDisposable UseTheme(ConsoulTheme theme)
        {
            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            ConsoulTheme originalTheme = CurrentTheme;
            ApplyTheme(theme);
            return new RenderThemeScope(originalTheme);
        }

        /// <summary>
        /// Applies scoped render option changes until the returned scope is disposed.
        /// </summary>
        /// <param name="configure">Action that mutates <see cref="RenderOptions"/> for the active scope.</param>
        /// <returns>A disposable scope that restores the previous render options.</returns>
        public static IDisposable Use(Action configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            RenderOptionsSnapshot snapshot = RenderOptionsSnapshot.Capture();
            try
            {
                configure();
            }
            catch
            {
                snapshot.Restore();
                throw;
            }

            return new RenderOptionsScope(snapshot);
        }

        /// <summary>
        /// Applies scoped render option changes until the returned scope is disposed.
        /// </summary>
        /// <param name="configure">Action that mutates <see cref="RenderOptions"/> for the active scope.</param>
        /// <returns>A disposable scope that restores the previous render options.</returns>
        public static IDisposable UseScoped(Action configure)
        {
            return Use(configure);
        }

        /// <summary>
        /// Defines how Consoul applies write suppression rules.
        /// </summary>
        public enum WriteModes
        {
            /// <summary>
            /// Writes all messages
            /// </summary>
            WriteAll,
            /// <summary>
            /// Suppresses all messages
            /// </summary>
            SuppressAll,
            /// <summary>
            /// Suppresses messages of certain <see cref="ConsoleColor"/>
            /// </summary>
            SuppressBlacklist
        }

        /// <summary>
        /// Defines how view rendering errors are handled after Consoul writes the error details.
        /// </summary>
        public enum ViewErrorModes
        {
            /// <summary>
            /// Writes the error details and continues view navigation where possible.
            /// </summary>
            RenderAndContinue,

            /// <summary>
            /// Writes the error details and rethrows the original exception.
            /// </summary>
            Throw
        }

        /// <summary>
        /// Returns the provided non-null <paramref name="color"/> or <see cref="DefaultScheme"/> color
        /// </summary>
        /// <param name="color">Attempted color to use in the console.</param>
        /// <returns>Non-null color</returns>
        public static ConsoleColor GetColorOrDefault(ConsoleColor? color = null)
        {
            if (color == null)
                color = DefaultScheme.Color;
            return (ConsoleColor)color;
        }

        /// <summary>
        /// Returns the provided non-null <paramref name="color"/> or <see cref="DefaultScheme"/> color
        /// </summary>
        /// <param name="color">Attempted color to use in the console.</param>
        /// <returns>Non-null color</returns>
        public static ConsoleColor GetBackgroundColorOrDefault(ConsoleColor? color = null)
        {
            if (color == null)
                color = DefaultScheme.BackgroundColor;
            return (ConsoleColor)color;
        }

        private sealed class RenderThemeScope : IDisposable
        {
            private readonly ConsoulTheme _originalTheme;
            private bool _disposed;

            public RenderThemeScope(ConsoulTheme originalTheme)
            {
                _originalTheme = originalTheme ?? throw new ArgumentNullException(nameof(originalTheme));
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                ApplyTheme(_originalTheme);
                _disposed = true;
            }
        }

        private sealed class RenderOptionsScope : IDisposable
        {
            private readonly RenderOptionsSnapshot _snapshot;
            private bool _disposed;

            public RenderOptionsScope(RenderOptionsSnapshot snapshot)
            {
                _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _snapshot.Restore();
                _disposed = true;
            }
        }

        private sealed class RenderOptionsSnapshot
        {
            private readonly ColorScheme _defaultScheme;
            private readonly ColorScheme _promptScheme;
            private readonly ColorScheme _subnoteScheme;
            private readonly ColorScheme _invalidScheme;
            private readonly ColorScheme _optionScheme;
            private readonly ColorScheme _routineInputScheme;
            private readonly List<ConsoleColor> _blacklistColors;
            private readonly WriteModes _writeMode;
            private readonly string _defaultGoBackMessage;
            private readonly string _continueMessage;
            private readonly bool _waitOnError;
            private readonly ViewErrorModes _viewErrorMode;
            private readonly bool _viewPromptKeyboardNavigationEnabled;
            private readonly bool _viewPromptFilterEnabled;
            private readonly int? _viewPromptPageSize;

            private RenderOptionsSnapshot()
            {
                _defaultScheme = DefaultScheme;
                _promptScheme = PromptScheme;
                _subnoteScheme = SubnoteScheme;
                _invalidScheme = InvalidScheme;
                _optionScheme = OptionScheme;
                _routineInputScheme = RoutineInputScheme;
                _blacklistColors = BlacklistColors == null
                    ? null
                    : new List<ConsoleColor>(BlacklistColors);
                _writeMode = WriteMode;
                _defaultGoBackMessage = DefaultGoBackMessage;
                _continueMessage = ContinueMessage;
                _waitOnError = WaitOnError;
                _viewErrorMode = ViewErrorMode;
                _viewPromptKeyboardNavigationEnabled = ViewPromptKeyboardNavigationEnabled;
                _viewPromptFilterEnabled = ViewPromptFilterEnabled;
                _viewPromptPageSize = ViewPromptPageSize;
            }

            public static RenderOptionsSnapshot Capture()
            {
                return new RenderOptionsSnapshot();
            }

            public void Restore()
            {
                DefaultScheme = _defaultScheme;
                PromptScheme = _promptScheme;
                SubnoteScheme = _subnoteScheme;
                InvalidScheme = _invalidScheme;
                OptionScheme = _optionScheme;
                RoutineInputScheme = _routineInputScheme;
                BlacklistColors = _blacklistColors == null
                    ? null
                    : new List<ConsoleColor>(_blacklistColors);
                WriteMode = _writeMode;
                DefaultGoBackMessage = _defaultGoBackMessage;
                ContinueMessage = _continueMessage;
                WaitOnError = _waitOnError;
                ViewErrorMode = _viewErrorMode;
                ViewPromptKeyboardNavigationEnabled = _viewPromptKeyboardNavigationEnabled;
                ViewPromptFilterEnabled = _viewPromptFilterEnabled;
                ViewPromptPageSize = _viewPromptPageSize;
            }
        }
    }
}
