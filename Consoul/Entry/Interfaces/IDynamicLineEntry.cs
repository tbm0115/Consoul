using System;
using System.Linq.Expressions;

namespace ConsoulLibrary.Entry
{
    internal interface IDynamicEntry<T>
    {
        Expression<Func<T,string>> MessageExpression { get; set; }
        Expression<Func<T, ConsoleColor>> ColorExpression { get; set; }
    }
}
