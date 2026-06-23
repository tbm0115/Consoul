
namespace ConsoulLibrary
{
    /// <summary>
    /// A dynamic option message.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicEntry<T> : IDynamicViewOption<T>
    {
        /// <summary>
        /// Expression that formats the text.
        /// </summary>
        public SetViewOptionMessage SetMessage { get; set; }

        /// <summary>
        /// Expression that sets the text color.
        /// </summary>
        public SetViewOptionColor SetColor { get; set; } = () => RenderOptions.DefaultColor;

        /// <summary>
        /// Constructs a dynamic option message.
        /// </summary>
        /// <param name="messageExpression"><see cref="SetMessage"/></param>
        /// <param name="colorExpression"><see cref="SetColor"/></param>
        public DynamicEntry(SetViewOptionMessage messageExpression, SetViewOptionColor colorExpression = null)
        {
            SetMessage = messageExpression;
            if (colorExpression != null)
            {
                SetColor = colorExpression;
            }
        }
    }
}
