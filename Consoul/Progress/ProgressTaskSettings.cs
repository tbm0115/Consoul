namespace ConsoulLibrary
{
    /// <summary>
    /// Defines settings used when creating a progress task with Spectre-style task configuration.
    /// </summary>
    public sealed class ProgressTaskSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the task should start active immediately.
        /// </summary>
        public bool AutoStart { get; set; } = true;

        /// <summary>
        /// Gets or sets the value that represents task completion.
        /// </summary>
        public double MaxValue { get; set; } = 100.0;

        /// <summary>
        /// Gets or sets a value indicating whether the task should start in indeterminate mode.
        /// </summary>
        public bool IsIndeterminate { get; set; }
    }
}
