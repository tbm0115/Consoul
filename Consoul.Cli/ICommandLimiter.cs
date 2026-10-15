namespace ConsoulLibrary
{
    /// <summary>
    /// Marks a command as compatible with a command settings type.
    /// </summary>
    /// <typeparam name="TSettings">The command settings type.</typeparam>
    public interface ICommandLimiter<out TSettings> where TSettings : CommandSettings
    {
    }
}
