namespace ConsoulLibrary
{
    /// <summary>
    /// Defines the visual intent for a metric value.
    /// </summary>
    public enum MetricKind
    {
        /// <summary>
        /// Renders a neutral metric value.
        /// </summary>
        Default,

        /// <summary>
        /// Renders an informational metric value.
        /// </summary>
        Info,

        /// <summary>
        /// Renders a successful or healthy metric value.
        /// </summary>
        Success,

        /// <summary>
        /// Renders a warning metric value.
        /// </summary>
        Warning,

        /// <summary>
        /// Renders an error or unhealthy metric value.
        /// </summary>
        Error
    }
}
