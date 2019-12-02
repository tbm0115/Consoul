using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ConsoulLibrary.Entry
{
    public interface IOption
    {
        Expression<Func<object>> Action { get; set; }
    }
}
