
namespace ConsoulLibrary.Entry
{
    /// <summary>
    /// A dynamic option message.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicEntry<T> : IDynamicEntry<T>
    {
        /// <summary>
        /// Expression that formats the text.
        /// </summary>
        public OptionMessage MessageExpression { get; set; }

        /// <summary>
        /// Expression that sets the text color.
        /// </summary>
        public OptionColor ColorExpression { get; set; } = () => RenderOptions.DefaultColor;

        /// <summary>
        /// Constructs a dynamic option message.
        /// </summary>
        /// <param name="messageExpression"><see cref="MessageExpression"/></param>
        /// <param name="colorExpression"><see cref="ColorExpression"/></param>
        public DynamicEntry(OptionMessage messageExpression, OptionColor colorExpression = null)
        {
            MessageExpression = messageExpression;
            if (colorExpression != null)
            {
                ColorExpression = colorExpression;
            }
        }
    }
}
