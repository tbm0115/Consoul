namespace ConsoulLibrary
{
    /// <summary>
    /// Defines settings used when creating a progress task with Spectre-style task configuration.
    /// </summary>
    public sealed class ProgressTaskSettings
    {
        /// <summary>
        /// Gets or sets the task description used by settings-first batch helpers.
        /// </summary>
        public string Description { get; set; }

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

        /// <summary>
        /// Gets or sets optional user data associated with the task at creation time.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets or sets whether the task should be hidden after it completes, overriding the context default.
        /// </summary>
        public bool? HideWhenCompleted { get; set; }
    }
}
