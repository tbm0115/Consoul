using System;

namespace ConsoulLibrary.Entry
{
    public interface ILineEntry : IEntry
    {
        string Message { get; set; }
    }
}
