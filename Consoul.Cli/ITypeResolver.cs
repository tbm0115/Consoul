using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Resolves command app services and command implementation types from an external container.
    /// </summary>
    public interface ITypeResolver
    {
        /// <summary>
        /// Resolves an instance of the requested type.
        /// </summary>
        /// <param name="type">The type requested by Consoul.</param>
        /// <returns>The resolved instance, or <see langword="null"/> when the type is not registered.</returns>
        object Resolve(Type type);
    }
}
