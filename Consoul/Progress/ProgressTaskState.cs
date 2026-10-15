namespace ConsoulLibrary
{
    /// <summary>
    /// Defines the lifecycle state of a <see cref="ProgressTask"/>.
    /// </summary>
    public enum ProgressTaskState
    {
        /// <summary>
        /// The task has been created but has not started yet.
        /// </summary>
        Pending = -1,

        /// <summary>
        /// The task is active and may continue receiving progress updates.
        /// </summary>
        Active = 0,

        /// <summary>
        /// The task completed successfully.
        /// </summary>
        Completed = 1,

        /// <summary>
        /// The task failed and should be rendered with failure styling.
        /// </summary>
        Failed = 2
    }
}
