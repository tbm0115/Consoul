using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive callout for informational, success, warning, and error messages.
    /// </summary>
    public class CalloutEntry : IRenderable
    {
        private readonly CalloutOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalloutEntry"/> class.
        /// </summary>
        /// <param name="message">The callout message to render.</param>
        /// <param name="kind">The visual intent of the callout.</param>
        /// <param name="options">Optional rendering options for the callout.</param>
        public CalloutEntry(string message, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null)
        {
            Message = message ?? string.Empty;
            Kind = kind;
            _options = options != null ? options.Clone() : new CalloutOptions();
        }

        /// <summary>
        /// Gets or sets the callout message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the visual intent of the callout.
        /// </summary>
        public CalloutKind Kind { get; set; }

        /// <summary>
        /// Gets the options used to render the callout.
        /// </summary>
        public CalloutOptions Options => _options;

        /// <summary>
        /// Renders the callout to the active console driver.
        /// </summary>
        public void Render()
        {
            new PanelEntry(Message, _options.CreatePanelOptions(Kind)).Render();
        }

        /// <summary>
        /// Renders a temporary callout to the active console driver.
        /// </summary>
        /// <param name="message">The callout message to render.</param>
        /// <param name="kind">The visual intent of the callout.</param>
        /// <param name="options">Optional rendering options for the callout.</param>
        public static void Render(string message, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null)
        {
            new CalloutEntry(message, kind, options).Render();
        }

        /// <summary>
        /// Captures a render action and renders the captured output inside a responsive callout.
        /// </summary>
        /// <param name="render">The render action used to produce callout content.</param>
        /// <param name="kind">The visual intent of the callout.</param>
        /// <param name="options">Optional rendering options for the callout.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Render(Action render, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            CalloutOptions resolvedOptions = options != null ? options.Clone() : new CalloutOptions();
            PanelEntry.Render(render, resolvedOptions.CreatePanelOptions(kind), captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures a renderable component and renders the captured output inside a responsive callout.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce callout content.</param>
        /// <param name="kind">The visual intent of the callout.</param>
        /// <param name="options">Optional rendering options for the callout.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Render(IConsoulRenderable renderable, CalloutKind kind = CalloutKind.Info, CalloutOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            CalloutOptions resolvedOptions = options != null ? options.Clone() : new CalloutOptions();
            PanelEntry.Render(renderable, resolvedOptions.CreatePanelOptions(kind), captureWidth, captureHeight);
        }
    }
}
