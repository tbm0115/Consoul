namespace ConsoulLibrary
{
    /// <summary>
    /// Configures a branch that has shared settings.
    /// </summary>
    /// <typeparam name="TSettings">The shared branch settings type.</typeparam>
    public interface IConfigurator<in TSettings> : IBranchConfigurator where TSettings : CommandSettings
    {
    }
}
