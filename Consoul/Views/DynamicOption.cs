using Consoul.Entry;
using System;
using System.Linq.Expressions;

namespace Consoul.Views
{
    public class DynamicOption<T> : IOption
    {
        public DynamicEntry<T> Entry { get; set; }
        public Expression<Func<object>> Action { get; set; }
        public ConsoleColor Color { get; set; }

        public DynamicOption(Expression<Func<T, string>> messageExpression, Expression<Func<object>> action, ConsoleColor color = ConsoleColor.White)
        {
            Entry = new DynamicEntry<T>(messageExpression, color);
            Action = action;
            Color = Entry.Color;
        }

        public string BuildMessage(T source, Expression<Func<string, string>> template = null)
        {
            if (template != null)
            {
                return template.Compile().Invoke(Entry.MessageExpression.Compile().Invoke(source));
            }
            return Entry.MessageExpression.Compile().Invoke(source);
        }
    }
}
