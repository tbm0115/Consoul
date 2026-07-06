namespace ConsoulLibrary
{
    /// <summary>
    /// Provides optional before and after hooks for cross-cutting command app behavior.
    /// </summary>
    public abstract class CommandInterceptor : ICommandInterceptor
    {
        /// <summary>
        /// Runs before the command handler executes.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings, or <c>null</c> when the command does not use typed settings.</param>
        public virtual void Intercept(CommandContext context, CommandSettings settings)
        {
        }

        /// <summary>
        /// Runs after the command handler executes and can adjust the exit code.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings, or <c>null</c> when the command does not use typed settings.</param>
        /// <param name="result">The process-style exit code.</param>
        public virtual void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
        {
        }
    }

    /// <summary>
    /// Provides typed before and after hooks for commands that bind a specific settings type.
    /// </summary>
    /// <typeparam name="TSettings">The settings type handled by this interceptor.</typeparam>
    public abstract class CommandInterceptor<TSettings> : CommandInterceptor
        where TSettings : CommandSettings
    {
        /// <summary>
        /// Runs before the command handler executes when the bound settings match <typeparamref name="TSettings"/>.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings, or <c>null</c> when the command does not use typed settings.</param>
        public override void Intercept(CommandContext context, CommandSettings settings)
        {
            if (settings is TSettings typedSettings)
            {
                Intercept(context, typedSettings);
            }
        }

        /// <summary>
        /// Runs after the command handler executes when the bound settings match <typeparamref name="TSettings"/>.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings, or <c>null</c> when the command does not use typed settings.</param>
        /// <param name="result">The process-style exit code.</param>
        public override void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
        {
            if (settings is TSettings typedSettings)
            {
                InterceptResult(context, typedSettings, ref result);
            }
        }

        /// <summary>
        /// Runs before the command handler executes for matching settings.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The typed command settings.</param>
        public virtual void Intercept(CommandContext context, TSettings settings)
        {
        }

        /// <summary>
        /// Runs after the command handler executes for matching settings and can adjust the exit code.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The typed command settings.</param>
        /// <param name="result">The process-style exit code.</param>
        public virtual void InterceptResult(CommandContext context, TSettings settings, ref int result)
        {
        }
    }
}
