using System;
using System.Linq.Expressions;

namespace ConsoulLibrary.Entry
{
    public interface IDynamicEntry<T>
    {
        Expression<Func<T,string>> MessageExpression { get; set; }
        Expression<Func<T, ConsoleColor>> ColorExpression { get; set; }
    }
}
