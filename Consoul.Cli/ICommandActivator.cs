using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Creates command and view instances for a <see cref="CommandApp"/>.
    /// </summary>
    public interface ICommandActivator
    {
        /// <summary>
        /// Creates an instance of the requested command or view type.
        /// </summary>
        /// <param name="commandType">The command or view type to create.</param>
        /// <returns>The created command or view instance.</returns>
        object Create(Type commandType);
    }
}
