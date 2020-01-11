using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoulLibrary.Entry
{
    public delegate void OptionAction();
    public delegate string OptionMessage();
    public delegate ConsoleColor OptionColor();

    internal interface IOption
    {
        OptionAction Action { get; set; }
    }
}
