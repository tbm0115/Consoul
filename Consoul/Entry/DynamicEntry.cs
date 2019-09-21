using System;
using System.Linq.Expressions;

namespace Consoul.Entry
{
    public class DynamicEntry<T> : IDynamicEntry<T>
    {
        public Expression<Func<T, string>> MessageExpression { get; set; }
        public Expression<Func<T, ConsoleColor>> ColorExpression { get; set; } = o => ConsoleColor.White;

        public DynamicEntry(Expression<Func<T,string>> messageExpression, Expression<Func<T,ConsoleColor>> colorExpression = null)
        {
            MessageExpression = messageExpression;
            if (colorExpression != null)
            {
                ColorExpression = colorExpression;
            }
        }
    }
}
