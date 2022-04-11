using System;

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
