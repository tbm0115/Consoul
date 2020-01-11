using ConsoulLibrary.Entry;
using System;
using System.Linq.Expressions;

namespace ConsoulLibrary.Views
{
    public class DynamicOption<T> : IOption
    {
        public DynamicEntry<T> Entry { get; set; }

        public OptionAction Action { get; set; }

        public DynamicOption(OptionMessage messageExpression, OptionAction action, OptionColor colorExpression = null)
        {
            Entry = new DynamicEntry<T>(messageExpression, colorExpression);
            Action = action;
        }
    }
}
