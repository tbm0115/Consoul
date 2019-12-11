using ConsoulLibrary.Entry;
using System;
using System.Linq.Expressions;

namespace ConsoulLibrary.Views
{
    public class DynamicOption<T> : IOption
    {
        public DynamicEntry<T> Entry { get; set; }

        public Expression<Func<object>> Action { get; set; }

        public DynamicOption(Expression<Func<T, string>> messageExpression, Expression<Func<object>> action, Expression<Func<T, ConsoleColor>> colorExpression = null)
        {
            Entry = new DynamicEntry<T>(messageExpression, colorExpression);
            Action = action;
        }

        public string BuildMessage(T source, Expression<Func<string, string>> template = null)
        {
            Func<T, string> buildFunc = Entry.MessageExpression.Compile();
            if (template != null)
            {
                return template.Compile().Invoke(buildFunc.Invoke(source));
            }
            return buildFunc(source);
        }

        public ConsoleColor BuildColor(T source) => Entry.ColorExpression.Compile()(source);
    }
}
