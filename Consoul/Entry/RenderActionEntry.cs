using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Wraps a render action as a reusable Consoul visual component.
    /// </summary>
    public sealed class RenderActionEntry : IRenderable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderActionEntry"/> class.
        /// </summary>
        /// <param name="render">The action that writes content to the active Consoul console driver.</param>
        public RenderActionEntry(Action render)
        {
            RenderAction = render ?? throw new ArgumentNullException(nameof(render));
        }

        /// <summary>
        /// Gets the action invoked when the entry renders.
        /// </summary>
        public Action RenderAction { get; }

        /// <summary>
        /// Renders the wrapped action to the active console driver.
        /// </summary>
        public void Render()
        {
            RenderAction();
        }
    }
}
