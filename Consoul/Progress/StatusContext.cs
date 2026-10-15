using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style property access over an active <see cref="StatusScope"/>.
    /// </summary>
    public sealed class StatusContext : IDisposable
    {
        private readonly StatusScope _scope;

        internal StatusContext(StatusScope scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        /// <summary>
        /// Gets or sets the current status message.
        /// </summary>
        public string Status
        {
            get => _scope.Message;
            set => _scope.SetContextStatus(value);
        }

        /// <summary>
        /// Gets or sets the spinner used for active status messages.
        /// </summary>
        public Spinner Spinner
        {
            get => _scope.CurrentSpinner;
            set => _scope.SetSpinner(value, throwOnNull: true);
        }

        /// <summary>
        /// Gets or sets the spinner style used for active status messages.
        /// </summary>
        public Style SpinnerStyle
        {
            get => _scope.CurrentSpinnerStyle;
            set => _scope.SetSpinnerStyle(value);
        }

        /// <summary>
        /// Gets the underlying status scope for Consoul-specific completion methods.
        /// </summary>
        public StatusScope Scope => _scope;

        /// <summary>
        /// Refreshes the current status immediately.
        /// </summary>
        public void Refresh()
        {
            _scope.Refresh();
        }

        /// <summary>
        /// Marks the status as successful.
        /// </summary>
        /// <param name="message">Optional final message. When omitted, the current status is reused.</param>
        public void Succeed(string message = null)
        {
            _scope.Succeed(message);
        }

        /// <summary>
        /// Marks the status as failed.
        /// </summary>
        /// <param name="message">Optional final message. When omitted, the current status is reused.</param>
        public void Fail(string message = null)
        {
            _scope.Fail(message);
        }

        /// <summary>
        /// Releases the underlying status scope.
        /// </summary>
        public void Dispose()
        {
            _scope.Dispose();
        }
    }

    /// <summary>
    /// Provides chainable helpers for <see cref="StatusContext"/>.
    /// </summary>
    public static class StatusContextExtensions
    {
        /// <summary>
        /// Sets the status message.
        /// </summary>
        /// <param name="context">The status context.</param>
        /// <param name="status">The status message.</param>
        /// <returns>The current status context.</returns>
        public static StatusContext Status(this StatusContext context, string status)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Status = status;
            return context;
        }

        /// <summary>
        /// Sets the status message using a Spectre-style color facade.
        /// </summary>
        /// <param name="context">The status context.</param>
        /// <param name="status">The status message.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the message.</param>
        /// <returns>The current status context.</returns>
        public static StatusContext Status(this StatusContext context, string status, SpectreColor color)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Scope.Status(status, color);
            return context;
        }

        /// <summary>
        /// Sets the status message using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="context">The status context.</param>
        /// <param name="status">The status message.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the message.</param>
        /// <returns>The current status context.</returns>
        public static StatusContext Status(this StatusContext context, string status, RgbColor color)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Scope.Status(status, color);
            return context;
        }

        /// <summary>
        /// Sets the spinner used for active status messages.
        /// </summary>
        /// <param name="context">The status context.</param>
        /// <param name="spinner">The spinner to use.</param>
        /// <returns>The current status context.</returns>
        public static StatusContext Spinner(this StatusContext context, Spinner spinner)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Spinner = spinner;
            return context;
        }

        /// <summary>
        /// Sets the spinner style used for active status messages.
        /// </summary>
        /// <param name="context">The status context.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current status context.</returns>
        public static StatusContext SpinnerStyle(this StatusContext context, Style style)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.SpinnerStyle = style;
            return context;
        }

        /// <summary>
        /// Sets the spinner style used for active status messages.
        /// </summary>
        /// <param name="context">The status context.</param>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current status context.</returns>
        public static StatusContext SpinnerStyle(this StatusContext context, ConsoleColor color)
        {
            return SpinnerStyle(context, new Style(color));
        }

        /// <summary>
        /// Sets the spinner style used for active status messages from a Spectre-style color facade.
        /// </summary>
        /// <param name="context">The status context.</param>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current status context.</returns>
        public static StatusContext SpinnerStyle(this StatusContext context, SpectreColor color)
        {
            return SpinnerStyle(context, new Style(color));
        }

        /// <summary>
        /// Sets the spinner style used for active status messages from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="context">The status context.</param>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current status context.</returns>
        public static StatusContext SpinnerStyle(this StatusContext context, RgbColor color)
        {
            return SpinnerStyle(context, new Style(color));
        }

        /// <summary>
        /// Sets the spinner style used for active status messages.
        /// </summary>
        /// <param name="context">The status context.</param>
        /// <param name="style">The style text to parse.</param>
        /// <returns>The current status context.</returns>
        public static StatusContext SpinnerStyle(this StatusContext context, string style)
        {
            return SpinnerStyle(context, Style.Parse(style));
        }
    }
}
