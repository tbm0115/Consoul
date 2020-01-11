using System;
using System.Linq.Expressions;

namespace ConsoulLibrary.Entry
{
    public class DynamicEntry<T> : IDynamicEntry<T>
    {
        public OptionMessage MessageExpression { get; set; }

        public OptionColor ColorExpression { get; set; } = () => RenderOptions.DefaultColor;

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
