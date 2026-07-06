namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a Consoul visual component that can render itself to the active console driver.
    /// </summary>
    public interface IConsoulRenderable
    {
        /// <summary>
        /// Renders the component to the active console driver.
        /// </summary>
        void Render();
    }

    /// <summary>
    /// Represents a reusable visual component using Spectre-style renderable vocabulary.
    /// </summary>
    public interface IRenderable : IConsoulRenderable
    {
    }
}
