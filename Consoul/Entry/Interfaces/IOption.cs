using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Consoul.Entry
{
    public interface IOption
    {
        Expression<Func<object>> Action { get; set; }
        ConsoleColor Color { get; set; }
    }
}
