using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Describes a panel header using Spectre-style text and justification vocabulary.
    /// </summary>
    /// <remarks>
    /// Consoul stores the header on <see cref="PanelOptions.Title"/>, but this type makes common
    /// Spectre-style snippets portable without changing the underlying panel renderer.
    /// </remarks>
    public sealed class PanelHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PanelHeader"/> class.
        /// </summary>
        /// <param name="text">The header text.</param>
        /// <param name="alignment">Optional header justification.</param>
        public PanelHeader(string text, Justify? alignment = null)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Justification = alignment;
        }

        /// <summary>
        /// Gets the panel header text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets or sets the optional header justification.
        /// </summary>
        public Justify? Justification { get; set; }

        /// <summary>
        /// Sets the panel header justification.
        /// </summary>
        /// <param name="alignment">The header justification.</param>
        /// <returns>The current <see cref="PanelHeader"/> instance.</returns>
        public PanelHeader SetAlignment(Justify alignment)
        {
            Justification = alignment;
            return this;
        }
    }
}
