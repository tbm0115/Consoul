using System;

namespace ConsoulLibrary.Entry
{
    internal interface ILineEntry : IEntry
    {
        string Message { get; set; }
    }
}
