using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Registers command app services and command implementation types for an external container.
    /// </summary>
    public interface ITypeRegistrar
    {
        /// <summary>
        /// Builds the resolver used to create commands and MVC-style views.
        /// </summary>
        /// <returns>The resolver for registered command app services.</returns>
        ITypeResolver Build();

        /// <summary>
        /// Registers an implementation type for a service type.
        /// </summary>
        /// <param name="service">The service type requested by Consoul.</param>
        /// <param name="implementation">The concrete implementation type to create.</param>
        void Register(Type service, Type implementation);

        /// <summary>
        /// Registers an existing service instance.
        /// </summary>
        /// <param name="service">The service type requested by Consoul.</param>
        /// <param name="implementation">The service instance.</param>
        void RegisterInstance(Type service, object implementation);

        /// <summary>
        /// Registers a lazily-created service instance.
        /// </summary>
        /// <param name="service">The service type requested by Consoul.</param>
        /// <param name="factory">The factory that creates the service instance.</param>
        void RegisterLazy(Type service, Func<object> factory);
    }
}
