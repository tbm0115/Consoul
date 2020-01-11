using System;
using System.Linq.Expressions;

namespace ConsoulLibrary.Entry
{
    internal interface IDynamicEntry<T>
    {
        OptionMessage MessageExpression { get; set; }
        OptionColor ColorExpression { get; set; }
    }
}
