using System;
using System.Linq.Expressions;

namespace Consoul.Entry
{
    public class DynamicEntry<T> : IDynamicEntry<T>
    {
        public Expression<Func<T, string>> MessageExpression { get; set; }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public DynamicEntry(Expression<Func<T,string>> messageExpression, ConsoleColor color = ConsoleColor.White)
        {
            MessageExpression = messageExpression;
            Color = color;
        }
    }
}
