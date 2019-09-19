using System;
using System.Linq.Expressions;

namespace Consoul.Entry
{
    public interface IDynamicEntry<T> : IEntry
    {
        Expression<Func<T,string>> MessageExpression { get; set; }
    }
}
