namespace ConsoulLibrary
{
    /// <summary>
    /// Configures a type registrar through a small, reusable frontend.
    /// </summary>
    public interface ITypeRegistrarFrontend
    {
        /// <summary>
        /// Adds service registrations to the supplied registrar.
        /// </summary>
        /// <param name="registrar">The registrar to configure.</param>
        void Configure(ITypeRegistrar registrar);
    }
}
