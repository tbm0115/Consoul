namespace ConsoulLibrary
{
    /// <summary>
    /// Represents optional hyperlink metadata accepted by migration-friendly text APIs.
    /// </summary>
    public sealed class Link
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Link"/> class.
        /// </summary>
        /// <param name="url">The link target URL.</param>
        public Link(string url)
        {
            Url = url ?? string.Empty;
        }

        /// <summary>
        /// Gets the link target URL.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Creates a link from a URL string.
        /// </summary>
        /// <param name="url">The link target URL.</param>
        public static implicit operator Link(string url)
        {
            return new Link(url);
        }

        /// <summary>
        /// Returns the link target URL.
        /// </summary>
        /// <returns>The link target URL.</returns>
        public override string ToString()
        {
            return Url;
        }
    }
}
